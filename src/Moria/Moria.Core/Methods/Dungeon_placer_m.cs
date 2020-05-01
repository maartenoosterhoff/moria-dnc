using System;
using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods
{
    public interface IDungeonPlacer
    {
        int dungeonSummonObject(Coord_t coord, int amount, int object_type);
        void dungeonPlaceRubble(Coord_t coord);
        void dungeonAllocateAndPlaceObject(Func<int, bool> set_function, int object_type, int number);
        void dungeonSetTrap(Coord_t coord, int sub_type_id);
        void dungeonPlaceGold(Coord_t coord);
        void dungeonPlaceRandomObjectAt(Coord_t coord, bool must_be_small);
        void dungeonPlaceRandomObjectNear(Coord_t coord, int tries);
    }

    public class Dungeon_placer_m : IDungeonPlacer
    {
        public Dungeon_placer_m(
            IDungeon dungeon,
            IDungeonLos dungeonLos,
            IGameObjects gameObjects,
            IInventoryManager inventoryManager,
            IRnd rnd,
            ITreasure treasure
        )
        {
            this.dungeon = dungeon;
            this.dungeonLos = dungeonLos;
            this.gameObjects = gameObjects;
            this.inventoryManager = inventoryManager;
            this.rnd = rnd;
            this.treasure = treasure;
        }

        private readonly IDungeon dungeon;
        private readonly IDungeonLos dungeonLos;
        private readonly IGameObjects gameObjects;
        private readonly IInventoryManager inventoryManager;
        private readonly IRnd rnd;
        private readonly ITreasure treasure;

        // Creates objects nearby the coordinates given -RAK-
        public void dungeonPlaceRandomObjectNear(Coord_t coord, int tries)
        {
            var dg = State.Instance.dg;
            do
            {
                for (var i = 0; i <= 10; i++)
                {
                    var at = new Coord_t(coord.y - 3 + this.rnd.randomNumber(5), coord.x - 4 + this.rnd.randomNumber(7));

                    if (this.dungeon.coordInBounds(at) &&
                        dg.floor[at.y][at.x].feature_id <= Dungeon_tile_c.MAX_CAVE_FLOOR &&
                        dg.floor[at.y][at.x].treasure_id == 0)
                    {
                        if (this.rnd.randomNumber(100) < 75)
                        {
                            this.dungeonPlaceRandomObjectAt(at, false);
                        }
                        else
                        {
                            this.dungeonPlaceGold(at);
                        }
                        i = 9;
                    }
                }

                tries--;
            } while (tries != 0);
        }
        
        // Places an object at given row, column co-ordinate -RAK-
        public void dungeonPlaceRandomObjectAt(Coord_t coord, bool must_be_small)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var free_treasure_id = this.gameObjects.popt();

            dg.floor[coord.y][coord.x].treasure_id = (uint)free_treasure_id;

            var object_id = this.gameObjects.itemGetRandomObjectId(dg.current_level, must_be_small);
            this.inventoryManager.inventoryItemCopyTo(State.Instance.sorted_objects[object_id], State.Instance.game.treasure.list[free_treasure_id]);

            this.treasure.magicTreasureMagicalAbility(free_treasure_id, dg.current_level);

            if (dg.floor[coord.y][coord.x].creature_id == 1)
            {
                Ui_io_m.printMessage("You feel something roll beneath your feet."); // -CJS-
            }
        }

        // Places rubble at location y, x -RAK-
        public void dungeonPlaceRubble(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var free_treasure_id = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)free_treasure_id;
            dg.floor[coord.y][coord.x].feature_id = Dungeon_tile_c.TILE_BLOCKED_FLOOR;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_RUBBLE, game.treasure.list[free_treasure_id]);
        }

        // Places a particular trap at location y, x -RAK-
        public void dungeonSetTrap(Coord_t coord, int sub_type_id)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var free_treasure_id = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)free_treasure_id;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_TRAP_LIST + sub_type_id, game.treasure.list[free_treasure_id]);
        }

        // Places a treasure (Gold or Gems) at given row, column -RAK-
        public void dungeonPlaceGold(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var free_treasure_id = this.gameObjects.popt();

            var gold_type_id = ((this.rnd.randomNumber(dg.current_level + 2) + 2) / 2) - 1;

            if (this.rnd.randomNumber(Config.treasure.TREASURE_CHANCE_OF_GREAT_ITEM) == 1)
            {
                gold_type_id += this.rnd.randomNumber(dg.current_level + 1);
            }

            if (gold_type_id >= Config.dungeon_objects.MAX_GOLD_TYPES)
            {
                gold_type_id = (int)Config.dungeon_objects.MAX_GOLD_TYPES - 1;
            }

            dg.floor[coord.y][coord.x].treasure_id = (uint)free_treasure_id;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_GOLD_LIST + gold_type_id, game.treasure.list[free_treasure_id]);
            game.treasure.list[free_treasure_id].cost += (8 * this.rnd.randomNumber(game.treasure.list[free_treasure_id].cost)) + this.rnd.randomNumber(8);

            if (dg.floor[coord.y][coord.x].creature_id == 1)
            {
                Ui_io_m.printMessage("You feel something roll beneath your feet.");
            }
        }

        // Allocates an object for tunnels and rooms -RAK-
        public void dungeonAllocateAndPlaceObject(Func<int, bool> set_function, int object_type, int number)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            var coord = new Coord_t(0, 0);

            for (var i = 0; i < number; i++)
            {
                // don't put an object beneath the player, this could cause
                // problems if player is standing under rubble, or on a trap.
                do
                {
                    coord.y = this.rnd.randomNumber(dg.height) - 1;
                    coord.x = this.rnd.randomNumber(dg.width) - 1;
                } while (!set_function((int)dg.floor[coord.y][coord.x].feature_id) ||
                         dg.floor[coord.y][coord.x].treasure_id != 0 ||
                         (coord.y == py.pos.y && coord.x == py.pos.x));

                switch (object_type)
                {
                    case 1:
                        this.dungeonSetTrap(coord, this.rnd.randomNumber(Config.dungeon_objects.MAX_TRAPS) - 1);
                        break;
                    case 2:
                    // NOTE: object_type == 2 is no longer used - it used to be visible traps.
                    // FIXME: there was no `break` here so `case 3` catches it? -MRC-
                    case 3:
                        this.dungeonPlaceRubble(coord);
                        break;
                    case 4:
                        this.dungeonPlaceGold(coord);
                        break;
                    case 5:
                        this.dungeonPlaceRandomObjectAt(coord, false);
                        break;
                    default:
                        break;
                }
            }
        }

        // Creates objects nearby the coordinates given -RAK-
        public int dungeonSummonObject(Coord_t coord, int amount, int object_type)
        {
            var dg = State.Instance.dg;

            int real_type;

            if (object_type == 1 || object_type == 5)
            {
                real_type = 1; // object_type == 1 -> objects
            }
            else
            {
                real_type = 256; // object_type == 2 -> gold
            }

            var result = 0;

            do
            {
                for (var tries = 0; tries <= 20; tries++)
                {
                    var at = new Coord_t(
                        coord.y - 3 + this.rnd.randomNumber(5),
                        coord.x - 3 + this.rnd.randomNumber(5));

                    if (this.dungeon.coordInBounds(at) && this.dungeonLos.los(coord, at))
                    {
                        if (dg.floor[at.y][at.x].feature_id <= Dungeon_tile_c.MAX_OPEN_SPACE && dg.floor[at.y][at.x].treasure_id == 0)
                        {
                            // object_type == 3 -> 50% objects, 50% gold
                            if (object_type == 3 || object_type == 7)
                            {
                                if (this.rnd.randomNumber(100) < 50)
                                {
                                    real_type = 1;
                                }
                                else
                                {
                                    real_type = 256;
                                }
                            }

                            if (real_type == 1)
                            {
                                this.dungeonPlaceRandomObjectAt(at, (object_type >= 4));
                            }
                            else
                            {
                                this.dungeonPlaceGold(at);
                            }

                            this.dungeon.dungeonLiteSpot(at);

                            if (this.dungeon.caveTileVisible(at))
                            {
                                result += real_type;
                            }

                            tries = 20;
                        }
                    }
                }

                amount--;
            } while (amount != 0);

            return result;
        }

    }
}