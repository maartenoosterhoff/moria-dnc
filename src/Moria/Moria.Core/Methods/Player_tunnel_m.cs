using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Player_m;

namespace Moria.Core.Methods
{
    public interface IPlayerTunnel
    {
        void playerTunnel(int direction);
    }

    public class Player_tunnel_m : IPlayerTunnel
    {
        private readonly IDice dice;
        private readonly IDungeon dungeon;
        private readonly IDungeonPlacer dungeonPlacer;
        private readonly IGame game;
        private readonly IHelpers helpers;
        private readonly IIdentification identification;
        private readonly IRnd rnd;
        private readonly ITerminal terminal;

        public Player_tunnel_m(
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
            this.dice = dice;
            this.dungeon = dungeon;
            this.dungeonPlacer = dungeonPlacer;
            this.game = game;
            this.helpers = helpers;
            this.identification = identification;
            this.rnd = rnd;
            this.terminal = terminal;
        }

        // Don't let the player tunnel somewhere illegal, this is necessary to
        // prevent the player from getting a free attack by trying to tunnel
        // somewhere where it has no effect.
        private bool playerCanTunnel(int treasure_id, int tile_id)
        {
            var game = State.Instance.game;
            if (tile_id < MIN_CAVE_WALL &&
                (treasure_id == 0 || game.treasure.list[treasure_id].category_id != TV_RUBBLE && game.treasure.list[treasure_id].category_id != TV_SECRET_DOOR))
            {
                game.player_free_turn = true;

                if (treasure_id == 0)
                {
                    this.terminal.printMessage("Tunnel through what?  Empty air?!?");
                }
                else
                {
                    this.terminal.printMessage("You can't tunnel through that.");
                }

                return false;
            }

            return true;
        }

        // Compute the digging ability of player; based on strength, and type of tool used
        private int playerDiggingAbility(Inventory_t weapon)
        {
            var py = State.Instance.py;
            var digging_ability = (int)py.stats.used[(int)PlayerAttr.STR];

            if ((weapon.flags & Config.treasure_flags.TR_TUNNEL) != 0u)
            {
                digging_ability += 25 + weapon.misc_use * 50;
            }
            else
            {
                digging_ability += this.dice.maxDiceRoll(weapon.damage) + weapon.to_hit + weapon.to_damage;

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

        private void dungeonDigGraniteWall(Coord_t coord, int digging_ability)
        {
            var i = this.rnd.randomNumber(1200) + 80;

            if (playerTunnelWall(coord, digging_ability, i))
            {
                this.terminal.printMessage("You have finished the tunnel.");
            }
            else
            {
                this.terminal.printMessageNoCommandInterrupt("You tunnel into the granite wall.");
            }
        }

        private void dungeonDigMagmaWall(Coord_t coord, int digging_ability)
        {
            var i = this.rnd.randomNumber(600) + 10;

            if (playerTunnelWall(coord, digging_ability, i))
            {
                this.terminal.printMessage("You have finished the tunnel.");
            }
            else
            {
                this.terminal.printMessageNoCommandInterrupt("You tunnel into the magma intrusion.");
            }
        }

        private void dungeonDigQuartzWall(Coord_t coord, int digging_ability)
        {
            var i = this.rnd.randomNumber(400) + 10;

            if (playerTunnelWall(coord, digging_ability, i))
            {
                this.terminal.printMessage("You have finished the tunnel.");
            }
            else
            {
                this.terminal.printMessageNoCommandInterrupt("You tunnel into the quartz vein.");
            }
        }

        private void dungeonDigRubble(Coord_t coord, int digging_ability)
        {
            if (digging_ability > this.rnd.randomNumber(180))
            {
                this.dungeon.dungeonDeleteObject(coord);
                this.terminal.printMessage("You have removed the rubble.");

                if (this.rnd.randomNumber(10) == 1)
                {
                    this.dungeonPlacer.dungeonPlaceRandomObjectAt(coord, false);

                    if (this.dungeon.caveTileVisible(coord))
                    {
                        this.terminal.printMessage("You have found something!");
                    }
                }

                this.dungeon.dungeonLiteSpot(coord);
            }
            else
            {
                this.terminal.printMessageNoCommandInterrupt("You dig in the rubble.");
            }
        }

        // Dig regular walls; Granite, magma intrusion, quartz vein
        // Don't forget the boundary walls, made of titanium (255)
        // Return `true` if a wall was dug at
        private bool dungeonDigAtLocation(Coord_t coord, uint wall_type, int digging_ability)
        {
            switch (wall_type)
            {
                case TILE_GRANITE_WALL:
                    this.dungeonDigGraniteWall(coord, digging_ability);
                    break;
                case TILE_MAGMA_WALL:
                    this.dungeonDigMagmaWall(coord, digging_ability);
                    break;
                case TILE_QUARTZ_WALL:
                    this.dungeonDigQuartzWall(coord, digging_ability);
                    break;
                case TILE_BOUNDARY_WALL:
                    this.terminal.printMessage("This seems to be permanent rock.");
                    break;
                default:
                    return false;
            }
            return true;
        }

        // Tunnels through rubble and walls -RAK-
        // Must take into account: secret doors, special tools
        public void playerTunnel(int direction)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            // Confused?                    75% random movement
            if (py.flags.confused > 0 && this.rnd.randomNumber(4) > 1)
            {
                direction = this.rnd.randomNumber(9);
            }

            var coord = py.pos.Clone();
            this.helpers.movePosition(direction, ref coord);

            var tile = dg.floor[coord.y][coord.x];
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (!this.playerCanTunnel((int)tile.treasure_id, (int)tile.feature_id))
            {
                return;
            }

            if (tile.creature_id > 1)
            {
                this.identification.objectBlockedByMonster((int)tile.creature_id);
                playerAttackPosition(coord);
                return;
            }

            if (item.category_id != TV_NOTHING)
            {
                var digging_ability = this.playerDiggingAbility(item);

                if (!this.dungeonDigAtLocation(coord, tile.feature_id, digging_ability))
                {
                    // Is there an object in the way?  (Rubble and secret doors)
                    if (tile.treasure_id != 0)
                    {
                        if (game.treasure.list[tile.treasure_id].category_id == TV_RUBBLE)
                        {
                            this.dungeonDigRubble(coord, digging_ability);
                        }
                        else if (game.treasure.list[tile.treasure_id].category_id == TV_SECRET_DOOR)
                        {
                            // Found secret door!
                            this.terminal.printMessageNoCommandInterrupt("You tunnel into the granite wall.");
                            playerSearch(py.pos, py.misc.chance_in_search);
                        }
                        else
                        {
                            this.game.exitProgram();
                            //abort();
                        }
                    }
                    else
                    {
                        this.game.exitProgram();
                        //abort();
                    }
                }

                return;
            }

            this.terminal.printMessage("You dig with your hands, making no progress.");
        }
    }
}
