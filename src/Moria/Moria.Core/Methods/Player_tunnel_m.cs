﻿using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dice_m;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Ui_io_m;

namespace Moria.Core.Methods
{
    public static class Player_tunnel_m
    {
        // Don't let the player tunnel somewhere illegal, this is necessary to
        // prevent the player from getting a free attack by trying to tunnel
        // somewhere where it has no effect.
        public static bool playerCanTunnel(int treasure_id, int tile_id)
        {
            var game = State.Instance.game;
            if (tile_id < MIN_CAVE_WALL &&
                (treasure_id == 0 || (game.treasure.list[treasure_id].category_id != TV_RUBBLE && game.treasure.list[treasure_id].category_id != TV_SECRET_DOOR)))
            {
                game.player_free_turn = true;

                if (treasure_id == 0)
                {
                    printMessage("Tunnel through what?  Empty air?!?");
                }
                else
                {
                    printMessage("You can't tunnel through that.");
                }

                return false;
            }

            return true;
        }

        // Compute the digging ability of player; based on strength, and type of tool used
        static int playerDiggingAbility(Inventory_t weapon)
        {
            var py = State.Instance.py;
            int digging_ability = (int)py.stats.used[(int)PlayerAttr.STR];

            if ((weapon.flags & Config.treasure_flags.TR_TUNNEL) != 0u)
            {
                digging_ability += 25 + weapon.misc_use * 50;
            }
            else
            {
                digging_ability += maxDiceRoll(weapon.damage) + weapon.to_hit + weapon.to_damage;

                // divide by two so that digging without shovel isn't too easy
                digging_ability >>= 1;
            }

            // If this weapon is too heavy for the player to wield properly,
            // then also make it harder to dig with it.
            if (py.weapon_is_heavy)
            {
                digging_ability += (int)((py.stats.used[(int)PlayerAttr.STR] * 15) - weapon.weight);

                if (digging_ability < 0)
                {
                    digging_ability = 0;
                }
            }

            return digging_ability;
        }

        static void dungeonDigGraniteWall(Coord_t coord, int digging_ability)
        {
            int i = randomNumber(1200) + 80;

            if (playerTunnelWall(coord, digging_ability, i))
            {
                printMessage("You have finished the tunnel.");
            }
            else
            {
                printMessageNoCommandInterrupt("You tunnel into the granite wall.");
            }
        }

        static void dungeonDigMagmaWall(Coord_t coord, int digging_ability)
        {
            int i = randomNumber(600) + 10;

            if (playerTunnelWall(coord, digging_ability, i))
            {
                printMessage("You have finished the tunnel.");
            }
            else
            {
                printMessageNoCommandInterrupt("You tunnel into the magma intrusion.");
            }
        }

        static void dungeonDigQuartzWall(Coord_t coord, int digging_ability)
        {
            int i = randomNumber(400) + 10;

            if (playerTunnelWall(coord, digging_ability, i))
            {
                printMessage("You have finished the tunnel.");
            }
            else
            {
                printMessageNoCommandInterrupt("You tunnel into the quartz vein.");
            }
        }

        static void dungeonDigRubble(Coord_t coord, int digging_ability)
        {
            if (digging_ability > randomNumber(180))
            {
                dungeonDeleteObject(coord);
                printMessage("You have removed the rubble.");

                if (randomNumber(10) == 1)
                {
                    dungeonPlaceRandomObjectAt(coord, false);

                    if (caveTileVisible(coord))
                    {
                        printMessage("You have found something!");
                    }
                }

                dungeonLiteSpot(coord);
            }
            else
            {
                printMessageNoCommandInterrupt("You dig in the rubble.");
            }
        }

        // Dig regular walls; Granite, magma intrusion, quartz vein
        // Don't forget the boundary walls, made of titanium (255)
        // Return `true` if a wall was dug at
        static bool dungeonDigAtLocation(Coord_t coord, uint wall_type, int digging_ability)
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
                    printMessage("This seems to be permanent rock.");
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
            if (py.flags.confused > 0 && randomNumber(4) > 1)
            {
                direction = randomNumber(9);
            }

            Coord_t coord = py.pos;
            playerMovePosition(direction, coord);

            var tile = dg.floor[coord.y][coord.x];
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (!playerCanTunnel((int)tile.treasure_id, (int)tile.feature_id))
            {
                return;
            }

            if (tile.creature_id > 1)
            {
                objectBlockedByMonster((int)tile.creature_id);
                playerAttackPosition(coord);
                return;
            }

            if (item.category_id != TV_NOTHING)
            {
                int digging_ability = playerDiggingAbility(item);

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
                            printMessageNoCommandInterrupt("You tunnel into the granite wall.");
                            playerSearch(py.pos, py.misc.chance_in_search);
                        }
                        else
                        {
                            exitProgram();
                            //abort();    // TOFIX:
                        }
                    }
                    else
                    {
                        exitProgram();
                        //abort();    // TOFIX
                    }
                }

                return;
            }

            printMessage("You dig with your hands, making no progress.");
        }
    }
}
