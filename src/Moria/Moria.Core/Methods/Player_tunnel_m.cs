﻿using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Player_m;

namespace Moria.Core.Methods
{
    public static class Player_tunnel_m
    {
        public static void SetDependencies(
            IDice dice,
            IDungeon dungeon,
            IDungeonPlacer dungeonPlacer,
            IGame game,
            IHelpers helpers,
            IIdentification identification,
            IRnd rnd,
            ITerminal terminal
        )
        {
            Player_tunnel_m.dice = dice;
            Player_tunnel_m.dungeon = dungeon;
            Player_tunnel_m.dungeonPlacer = dungeonPlacer;
            Player_tunnel_m.game = game;
            Player_tunnel_m.helpers = helpers;
            Player_tunnel_m.identification = identification;
            Player_tunnel_m.rnd = rnd;
            Player_tunnel_m.terminal = terminal;
        }

        private static IDice dice;
        private static IDungeon dungeon;
        private static IDungeonPlacer dungeonPlacer;
        private static IGame game;
        private static IHelpers helpers;
        private static IIdentification identification;
        private static IRnd rnd;
        private static ITerminal terminal;

        // Don't let the player tunnel somewhere illegal, this is necessary to
        // prevent the player from getting a free attack by trying to tunnel
        // somewhere where it has no effect.
        private static bool playerCanTunnel(int treasure_id, int tile_id)
        {
            var game = State.Instance.game;
            if (tile_id < MIN_CAVE_WALL &&
                (treasure_id == 0 || game.treasure.list[treasure_id].category_id != TV_RUBBLE && game.treasure.list[treasure_id].category_id != TV_SECRET_DOOR))
            {
                game.player_free_turn = true;

                if (treasure_id == 0)
                {
                    terminal.printMessage("Tunnel through what?  Empty air?!?");
                }
                else
                {
                    terminal.printMessage("You can't tunnel through that.");
                }

                return false;
            }

            return true;
        }

        // Compute the digging ability of player; based on strength, and type of tool used
        private static int playerDiggingAbility(Inventory_t weapon)
        {
            var py = State.Instance.py;
            var digging_ability = (int)py.stats.used[(int)PlayerAttr.STR];

            if ((weapon.flags & Config.treasure_flags.TR_TUNNEL) != 0u)
            {
                digging_ability += 25 + weapon.misc_use * 50;
            }
            else
            {
                digging_ability += dice.maxDiceRoll(weapon.damage) + weapon.to_hit + weapon.to_damage;

                // divide by two so that digging without shovel isn't too easy
                digging_ability >>= 1;
            }

            // If this weapon is too heavy for the player to wield properly,
            // then also make it harder to dig with it.
            if (py.weapon_is_heavy)
            {
                digging_ability += (int)(py.stats.used[(int)PlayerAttr.STR] * 15 - weapon.weight);

                if (digging_ability < 0)
                {
                    digging_ability = 0;
                }
            }

            return digging_ability;
        }

        private static void dungeonDigGraniteWall(Coord_t coord, int digging_ability)
        {
            var i = rnd.randomNumber(1200) + 80;

            if (playerTunnelWall(coord, digging_ability, i))
            {
                terminal.printMessage("You have finished the tunnel.");
            }
            else
            {
                terminal.printMessageNoCommandInterrupt("You tunnel into the granite wall.");
            }
        }

        private static void dungeonDigMagmaWall(Coord_t coord, int digging_ability)
        {
            var i = rnd.randomNumber(600) + 10;

            if (playerTunnelWall(coord, digging_ability, i))
            {
                terminal.printMessage("You have finished the tunnel.");
            }
            else
            {
                terminal.printMessageNoCommandInterrupt("You tunnel into the magma intrusion.");
            }
        }

        private static void dungeonDigQuartzWall(Coord_t coord, int digging_ability)
        {
            var i = rnd.randomNumber(400) + 10;

            if (playerTunnelWall(coord, digging_ability, i))
            {
                terminal.printMessage("You have finished the tunnel.");
            }
            else
            {
                terminal.printMessageNoCommandInterrupt("You tunnel into the quartz vein.");
            }
        }

        private static void dungeonDigRubble(Coord_t coord, int digging_ability)
        {
            if (digging_ability > rnd.randomNumber(180))
            {
                dungeon.dungeonDeleteObject(coord);
                terminal.printMessage("You have removed the rubble.");

                if (rnd.randomNumber(10) == 1)
                {
                    dungeonPlacer.dungeonPlaceRandomObjectAt(coord, false);

                    if (dungeon.caveTileVisible(coord))
                    {
                        terminal.printMessage("You have found something!");
                    }
                }

                dungeon.dungeonLiteSpot(coord);
            }
            else
            {
                terminal.printMessageNoCommandInterrupt("You dig in the rubble.");
            }
        }

        // Dig regular walls; Granite, magma intrusion, quartz vein
        // Don't forget the boundary walls, made of titanium (255)
        // Return `true` if a wall was dug at
        private static bool dungeonDigAtLocation(Coord_t coord, uint wall_type, int digging_ability)
        {
            switch (wall_type)
            {
                case TILE_GRANITE_WALL:
                    dungeonDigGraniteWall(coord, digging_ability);
                    break;
                case TILE_MAGMA_WALL:
                    dungeonDigMagmaWall(coord, digging_ability);
                    break;
                case TILE_QUARTZ_WALL:
                    dungeonDigQuartzWall(coord, digging_ability);
                    break;
                case TILE_BOUNDARY_WALL:
                    terminal.printMessage("This seems to be permanent rock.");
                    break;
                default:
                    return false;
            }
            return true;
        }

        // Tunnels through rubble and walls -RAK-
        // Must take into account: secret doors, special tools
        public static void playerTunnel(int direction)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            // Confused?                    75% random movement
            if (py.flags.confused > 0 && rnd.randomNumber(4) > 1)
            {
                direction = rnd.randomNumber(9);
            }

            var coord = py.pos.Clone();
            helpers.movePosition(direction, ref coord);

            var tile = dg.floor[coord.y][coord.x];
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (!playerCanTunnel((int)tile.treasure_id, (int)tile.feature_id))
            {
                return;
            }

            if (tile.creature_id > 1)
            {
                identification.objectBlockedByMonster((int)tile.creature_id);
                playerAttackPosition(coord);
                return;
            }

            if (item.category_id != TV_NOTHING)
            {
                var digging_ability = playerDiggingAbility(item);

                if (!dungeonDigAtLocation(coord, tile.feature_id, digging_ability))
                {
                    // Is there an object in the way?  (Rubble and secret doors)
                    if (tile.treasure_id != 0)
                    {
                        if (game.treasure.list[tile.treasure_id].category_id == TV_RUBBLE)
                        {
                            dungeonDigRubble(coord, digging_ability);
                        }
                        else if (game.treasure.list[tile.treasure_id].category_id == TV_SECRET_DOOR)
                        {
                            // Found secret door!
                            terminal.printMessageNoCommandInterrupt("You tunnel into the granite wall.");
                            playerSearch(py.pos, py.misc.chance_in_search);
                        }
                        else
                        {
                            Player_tunnel_m.game.exitProgram();
                            //abort();
                        }
                    }
                    else
                    {
                        Player_tunnel_m.game.exitProgram();
                        //abort();
                    }
                }

                return;
            }

            terminal.printMessage("You dig with your hands, making no progress.");
        }
    }
}
