using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using Moria.Core.Utils;

namespace Moria.Core.Methods
{
    public interface IDungeonGenerator
    {
        void GenerateCave();
    }

    public class DungeonGenerator : IDungeonGenerator
    {
        private readonly IDungeon dungeon;
        private readonly IDungeonPlacer dungeonPlacer;
        private readonly IGameObjects gameObjects;
        private readonly IHelpers helpers;
        private readonly IInventoryManager inventoryManager;
        private readonly IMonsterManager monsterManager;
        private readonly IRnd rnd;
        private readonly IStoreInventory storeInventory;

        public DungeonGenerator(
            IDungeon dungeon,
            IDungeonPlacer dungeonPlacer,
            IGameObjects gameObjects,
            IHelpers helpers,
            IInventoryManager inventoryManager,
            IMonsterManager monsterManager,
            IRnd rnd,
            IStoreInventory storeInventory
        )
        {
            this.dungeon = dungeon;
            this.dungeonPlacer = dungeonPlacer;
            this.gameObjects = gameObjects;
            this.helpers = helpers;
            this.inventoryManager = inventoryManager;
            this.monsterManager = monsterManager;
            this.rnd = rnd;
            this.storeInventory = storeInventory;
        }

        // Returns a Dark/Light floor tile based on dg.current_level, and random number
        private uint DungeonFloorTileForLevel()
        {
            var dg = State.Instance.dg;
            if (dg.current_level <= this.rnd.randomNumber(25))
            {
                return Dungeon_tile_c.TILE_LIGHT_FLOOR;
            }
            return Dungeon_tile_c.TILE_DARK_FLOOR;
        }

        // Always picks a correct direction
        private void PickCorrectDirection(out int vertical, out int horizontal, Coord_t start, Coord_t end)
        {
            if (start.y < end.y)
            {
                vertical = 1;
            }
            else if (start.y == end.y)
            {
                vertical = 0;
            }
            else
            {
                vertical = -1;
            }

            if (start.x < end.x)
            {
                horizontal = 1;
            }
            else if (start.x == end.x)
            {
                horizontal = 0;
            }
            else
            {
                horizontal = -1;
            }

            if (vertical != 0 && horizontal != 0)
            {
                if (this.rnd.randomNumber(2) == 1)
                {
                    vertical = 0;
                }
                else
                {
                    horizontal = 0;
                }
            }
        }

        // Chance of wandering direction
        private void ChanceOfRandomDirection(out int vertical, out int horizontal)
        {
            var direction = this.rnd.randomNumber(4);

            if (direction < 3)
            {
                horizontal = 0;
                vertical = -3 + (direction << 1); // direction=1 -> y=-1; direction=2 -> y=1
            }
            else
            {
                vertical = 0;
                horizontal = -7 + (direction << 1); // direction=3 -> x=-1; direction=4 -> x=1
            }
        }

        // Blanks out entire cave -RAK-
        private static void DungeonBlankEntireCave()
        {
            State.Instance.dg.floor = ArrayInitializer.InitializeWithDefault(
                Dungeon_c.MAX_HEIGHT,
                Dungeon_c.MAX_WIDTH,
                () => new Tile_t
                {
                    perma_lit_room = false,
                    field_mark = false,
                    permanent_light = false,
                    temporary_light = false
                }
            );
            //memset((char*)&dg.floor[0][0], 0, sizeof(dg.floor));
        }

        // Fills in empty spots with desired rock -RAK-
        // Note: 9 is a temporary value.
        private static void DungeonFillEmptyTilesWith(uint rockType)
        {
            var dg = State.Instance.dg;

            // no need to check the border of the cave
            for (var y = dg.height - 2; y > 0; y--)
            {
                var x = 1;

                for (var j = dg.width - 2; j > 0; j--)
                {
                    if (dg.floor[y][x].feature_id == Dungeon_tile_c.TILE_NULL_WALL ||
                        dg.floor[y][x].feature_id == Dungeon_tile_c.TMP1_WALL ||
                        dg.floor[y][x].feature_id == Dungeon_tile_c.TMP2_WALL)
                    {
                        dg.floor[y][x].feature_id = rockType;
                    }
                    x++;
                }
            }
        }

        // Places indestructible rock around edges of dungeon -RAK-
        private static void DungeonPlaceBoundaryWalls()
        {
            var dg = State.Instance.dg;

            for (var x = 0; x < Dungeon_c.MAX_WIDTH; x++)
            {
                dg.floor[0][x].feature_id = Dungeon_tile_c.TILE_BOUNDARY_WALL;
                dg.floor[dg.height - 1][x].feature_id = Dungeon_tile_c.TILE_BOUNDARY_WALL;
            }

            for (var y = 0; y < Dungeon_c.MAX_HEIGHT; y++)
            {
                dg.floor[y][0].feature_id = Dungeon_tile_c.TILE_BOUNDARY_WALL;
                dg.floor[y][dg.width - 1].feature_id = Dungeon_tile_c.TILE_BOUNDARY_WALL;
            }

            /*
            Tile_t(*left_ptr)[MAX_WIDTH];
            Tile_t(*right_ptr)[MAX_WIDTH];

            // put permanent wall on leftmost row and rightmost row
            left_ptr = (Tile_t(*)[MAX_WIDTH]) & dg.floor[0][0];
            right_ptr = (Tile_t(*)[MAX_WIDTH]) & dg.floor[0][dg.width - 1];

            for (var i = 0; i < dg.height; i++)
            {
                ((Tile_t*)left_ptr)->feature_id = TILE_BOUNDARY_WALL;
                left_ptr++;

                ((Tile_t*)right_ptr)->feature_id = TILE_BOUNDARY_WALL;
                right_ptr++;
            }

            // put permanent wall on top row and bottom row
            Tile_t* top_ptr = &dg.floor[0][0];
            Tile_t* bottom_ptr = &dg.floor[dg.height - 1][0];

            for (var i = 0; i < dg.width; i++)
            {
                top_ptr->feature_id = TILE_BOUNDARY_WALL;
                top_ptr++;

                bottom_ptr->feature_id = TILE_BOUNDARY_WALL;
                bottom_ptr++;
            }
            */
        }

        // Places "streamers" of rock through dungeon -RAK-
        private void DungeonPlaceStreamerRock(uint rockType, int chanceOfTreasure)
        {
            var dg = State.Instance.dg;

            // Choose starting point and direction
            var coord = new Coord_t(
                (dg.height / 2) + 11 - this.rnd.randomNumber(23),
                (dg.width / 2) + 16 - this.rnd.randomNumber(33)
            );

            // Get random direction. Numbers 1-4, 6-9
            var dir = this.rnd.randomNumber(8);
            if (dir > 4)
            {
                dir += 1;
            }

            // Place streamer into dungeon
            var t1 = 2 * (int)Config.dungeon.DUN_STREAMER_WIDTH + 1; // Constants
            var t2 = (int)Config.dungeon.DUN_STREAMER_WIDTH + 1;

            do
            {
                for (var i = 0; i < Config.dungeon.DUN_STREAMER_DENSITY; i++)
                {
                    var spot = new Coord_t(
                        coord.y + this.rnd.randomNumber(t1) - t2,
                        coord.x + this.rnd.randomNumber(t1) - t2
                    );

                    if (this.dungeon.coordInBounds(spot))
                    {
                        if (dg.floor[spot.y][spot.x].feature_id == Dungeon_tile_c.TILE_GRANITE_WALL)
                        {
                            dg.floor[spot.y][spot.x].feature_id = rockType;

                            if (this.rnd.randomNumber(chanceOfTreasure) == 1)
                            {
                                this.dungeonPlacer.dungeonPlaceGold(spot);
                            }
                        }
                    }
                }
            } while (this.helpers.movePosition(dir, ref coord));
        }

        private void DungeonPlaceOpenDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var curPos = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)curPos;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_OPEN_DOOR, game.treasure.list[curPos]);
            dg.floor[coord.y][coord.x].feature_id = Dungeon_tile_c.TILE_CORR_FLOOR;
        }

        private void DungeonPlaceBrokenDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var curPos = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)curPos;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_OPEN_DOOR, game.treasure.list[curPos]);
            dg.floor[coord.y][coord.x].feature_id = Dungeon_tile_c.TILE_CORR_FLOOR;
            game.treasure.list[curPos].misc_use = 1;
        }

        private void DungeonPlaceClosedDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var curPos = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)curPos;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, game.treasure.list[curPos]);
            dg.floor[coord.y][coord.x].feature_id = Dungeon_tile_c.TILE_BLOCKED_FLOOR;
        }

        private void DungeonPlaceLockedDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var curPos = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)curPos;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, game.treasure.list[curPos]);
            dg.floor[coord.y][coord.x].feature_id = Dungeon_tile_c.TILE_BLOCKED_FLOOR;
            game.treasure.list[curPos].misc_use = this.rnd.randomNumber(10) + 10;
        }

        private void DungeonPlaceStuckDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var curPos = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)curPos;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, game.treasure.list[curPos]);
            dg.floor[coord.y][coord.x].feature_id = Dungeon_tile_c.TILE_BLOCKED_FLOOR;
            game.treasure.list[curPos].misc_use = (-this.rnd.randomNumber(10) - 10);
        }

        private void DungeonPlaceSecretDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var curPos = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)curPos;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_SECRET_DOOR, game.treasure.list[curPos]);
            dg.floor[coord.y][coord.x].feature_id = Dungeon_tile_c.TILE_BLOCKED_FLOOR;
        }

        private void DungeonPlaceDoor(Coord_t coord)
        {
            var doorType = this.rnd.randomNumber(3);

            if (doorType == 1)
            {
                if (this.rnd.randomNumber(4) == 1)
                {
                    this.DungeonPlaceBrokenDoor(coord);
                }
                else
                {
                    this.DungeonPlaceOpenDoor(coord);
                }
            }
            else if (doorType == 2)
            {
                doorType = this.rnd.randomNumber(12);

                if (doorType > 3)
                {
                    this.DungeonPlaceClosedDoor(coord);
                }
                else if (doorType == 3)
                {
                    this.DungeonPlaceStuckDoor(coord);
                }
                else
                {
                    this.DungeonPlaceLockedDoor(coord);
                }
            }
            else
            {
                this.DungeonPlaceSecretDoor(coord);
            }
        }

        // Place an up staircase at given y, x -RAK-
        private void DungeonPlaceUpStairs(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            if (dg.floor[coord.y][coord.x].treasure_id != 0)
            {
                this.dungeon.dungeonDeleteObject(coord);
            }

            var curPos = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)curPos;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_UP_STAIR, game.treasure.list[curPos]);
        }

        // Place a down staircase at given y, x -RAK-
        private void DungeonPlaceDownStairs(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            if (dg.floor[coord.y][coord.x].treasure_id != 0)
            {
                this.dungeon.dungeonDeleteObject(coord);
            }

            var curPos = this.gameObjects.popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)curPos;
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_DOWN_STAIR, game.treasure.list[curPos]);
        }

        // Places a staircase 1=up, 2=down -RAK-
        private void DungeonPlaceStairs(int stairType, int number, int walls)
        {
            var dg = State.Instance.dg;

            var coord1 = new Coord_t(0, 0);
            var coord2 = new Coord_t(0, 0);

            for (var i = 0; i < number; i++)
            {
                var placed = false;

                while (!placed)
                {
                    var j = 0;

                    do
                    {
                        // Note:
                        // don't let y1/x1 be zero,
                        // don't let y2/x2 be equal to dg.height-1/dg.width-1,
                        // these values are always BOUNDARY_ROCK.
                        coord1.y = this.rnd.randomNumber(dg.height - 14);
                        coord1.x = this.rnd.randomNumber(dg.width - 14);
                        coord2.y = coord1.y + 12;
                        coord2.x = coord1.x + 12;

                        do
                        {
                            do
                            {
                                if (dg.floor[coord1.y][coord1.x].feature_id <= Dungeon_tile_c.MAX_OPEN_SPACE &&
                                    dg.floor[coord1.y][coord1.x].treasure_id == 0 && this.dungeon.coordWallsNextTo(coord1) >= walls)
                                {
                                    placed = true;
                                    if (stairType == 1)
                                    {
                                        this.DungeonPlaceUpStairs(coord1);
                                    }
                                    else
                                    {
                                        this.DungeonPlaceDownStairs(coord1);
                                    }
                                }
                                coord1.x++;
                            } while ((coord1.x != coord2.x) && (!placed));

                            coord1.x = coord2.x - 12;
                            coord1.y++;
                        } while ((coord1.y != coord2.y) && (!placed));

                        j++;
                    } while ((!placed) && (j <= 30));

                    walls--;
                }
            }
        }

        // Place a trap with a given displacement of point -RAK-
        private void DungeonPlaceVaultTrap(Coord_t coord, Coord_t displacement, int number)
        {
            var dg = State.Instance.dg;

            var spot = new Coord_t(0, 0);

            for (var i = 0; i < number; i++)
            {
                var placed = false;

                for (var count = 0; !placed && count <= 5; count++)
                {
                    spot.y = coord.y - displacement.y - 1 + this.rnd.randomNumber(2 * displacement.y + 1);
                    spot.x = coord.x - displacement.x - 1 + this.rnd.randomNumber(2 * displacement.x + 1);

                    if (dg.floor[spot.y][spot.x].feature_id != Dungeon_tile_c.TILE_NULL_WALL &&
                        dg.floor[spot.y][spot.x].feature_id <= Dungeon_tile_c.MAX_CAVE_FLOOR &&
                        dg.floor[spot.y][spot.x].treasure_id == 0)
                    {
                        this.dungeonPlacer.dungeonSetTrap(spot, this.rnd.randomNumber((int)Config.dungeon_objects.MAX_TRAPS) - 1);
                        placed = true;
                    }
                }
            }
        }

        // Place a trap with a given displacement of point -RAK-
        private void DungeonPlaceVaultMonster(Coord_t coord, int number)
        {
            var spot = new Coord_t(0, 0);

            for (var i = 0; i < number; i++)
            {
                spot.y = coord.y;
                spot.x = coord.x;
                this.monsterManager.monsterSummon(spot, true);
            }
        }

        // Builds a room at a row, column coordinate -RAK-
        private void DungeonBuildRoom(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var floor = this.DungeonFloorTileForLevel();

            var height = coord.y - this.rnd.randomNumber(4);
            var depth = coord.y + this.rnd.randomNumber(3);
            var left = coord.x - this.rnd.randomNumber(11);
            var right = coord.x + this.rnd.randomNumber(11);

            // the x dim of rooms tends to be much larger than the y dim,
            // so don't bother rewriting the y loop.

            int y, x;

            for (y = height; y <= depth; y++)
            {
                for (x = left; x <= right; x++)
                {
                    dg.floor[y][x].feature_id = floor;
                    dg.floor[y][x].perma_lit_room = true;
                }
            }

            for (y = height - 1; y <= depth + 1; y++)
            {
                dg.floor[y][left - 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[y][left - 1].perma_lit_room = true;

                dg.floor[y][right + 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[y][right + 1].perma_lit_room = true;
            }

            for (x = left; x <= right; x++)
            {
                dg.floor[height - 1][x].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[height - 1][x].perma_lit_room = true;

                dg.floor[depth + 1][x].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[depth + 1][x].perma_lit_room = true;
            }
        }

        // Builds a room at a row, column coordinate -RAK-
        // Type 1 unusual rooms are several overlapping rectangular ones
        private void DungeonBuildRoomOverlappingRectangles(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var floor = this.DungeonFloorTileForLevel();

            var limit = 1 + this.rnd.randomNumber(2);

            for (var count = 0; count < limit; count++)
            {
                var height = coord.y - this.rnd.randomNumber(4);
                var depth = coord.y + this.rnd.randomNumber(3);
                var left = coord.x - this.rnd.randomNumber(11);
                var right = coord.x + this.rnd.randomNumber(11);

                // the x dim of rooms tends to be much larger than the y dim,
                // so don't bother rewriting the y loop.

                int y, x;

                for (y = height; y <= depth; y++)
                {
                    for (x = left; x <= right; x++)
                    {
                        dg.floor[y][x].feature_id = floor;
                        dg.floor[y][x].perma_lit_room = true;
                    }
                }
                for (y = (height - 1); y <= (depth + 1); y++)
                {
                    if (dg.floor[y][left - 1].feature_id != floor)
                    {
                        dg.floor[y][left - 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                        dg.floor[y][left - 1].perma_lit_room = true;
                    }

                    if (dg.floor[y][right + 1].feature_id != floor)
                    {
                        dg.floor[y][right + 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                        dg.floor[y][right + 1].perma_lit_room = true;
                    }
                }

                for (x = left; x <= right; x++)
                {
                    if (dg.floor[height - 1][x].feature_id != floor)
                    {
                        dg.floor[height - 1][x].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                        dg.floor[height - 1][x].perma_lit_room = true;
                    }

                    if (dg.floor[depth + 1][x].feature_id != floor)
                    {
                        dg.floor[depth + 1][x].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                        dg.floor[depth + 1][x].perma_lit_room = true;
                    }
                }
            }
        }

        private void DungeonPlaceRandomSecretDoor(Coord_t coord, int depth, int height, int left, int right)
        {
            switch (this.rnd.randomNumber(4))
            {
                case 1:
                    this.DungeonPlaceSecretDoor(new Coord_t(height - 1, coord.x));
                    break;
                case 2:
                    this.DungeonPlaceSecretDoor(new Coord_t(depth + 1, coord.x));
                    break;
                case 3:
                    this.DungeonPlaceSecretDoor(new Coord_t(coord.y, left - 1));
                    break;
                default:
                    this.DungeonPlaceSecretDoor(new Coord_t(coord.y, right + 1));
                    break;
            }
        }

        private static void DungeonPlaceVault(Coord_t coord)
        {
            var dg = State.Instance.dg;

            for (var y = coord.y - 1; y <= coord.y + 1; y++)
            {
                dg.floor[y][coord.x - 1].feature_id = Dungeon_tile_c.TMP1_WALL;
                dg.floor[y][coord.x + 1].feature_id = Dungeon_tile_c.TMP1_WALL;
            }

            dg.floor[coord.y - 1][coord.x].feature_id = Dungeon_tile_c.TMP1_WALL;
            dg.floor[coord.y + 1][coord.x].feature_id = Dungeon_tile_c.TMP1_WALL;
        }

        private void DungeonPlaceTreasureVault(Coord_t coord, int depth, int height, int left, int right)
        {
            this.DungeonPlaceRandomSecretDoor(coord, depth, height, left, right);
            DungeonPlaceVault(coord);

            // Place a locked door
            var offset = this.rnd.randomNumber(4);
            if (offset < 3)
            {
                // 1 -> y-1; 2 -> y+1
                this.DungeonPlaceLockedDoor(new Coord_t(coord.y - 3 + (offset << 1), coord.x));
            }
            else
            {
                this.DungeonPlaceLockedDoor(new Coord_t(coord.y, coord.x - 7 + (offset << 1)));
            }
        }

        private void DungeonPlaceInnerPillars(Coord_t coord)
        {
            var dg = State.Instance.dg;

            int y, x;

            for (y = coord.y - 1; y <= coord.y + 1; y++)
            {
                for (x = coord.x - 1; x <= coord.x + 1; x++)
                {
                    dg.floor[y][x].feature_id = Dungeon_tile_c.TMP1_WALL;
                }
            }

            if (this.rnd.randomNumber(2) != 1)
            {
                return;
            }

            var offset = this.rnd.randomNumber(2);

            for (y = coord.y - 1; y <= coord.y + 1; y++)
            {
                for (x = coord.x - 5 - offset; x <= coord.x - 3 - offset; x++)
                {
                    dg.floor[y][x].feature_id = Dungeon_tile_c.TMP1_WALL;
                }
            }

            for (y = coord.y - 1; y <= coord.y + 1; y++)
            {
                for (x = coord.x + 3 + offset; x <= coord.x + 5 + offset; x++)
                {
                    dg.floor[y][x].feature_id = Dungeon_tile_c.TMP1_WALL;
                }
            }
        }

        private static void DungeonPlaceMazeInsideRoom(int depth, int height, int left, int right)
        {
            var dg = State.Instance.dg;

            for (var y = height; y <= depth; y++)
            {
                for (var x = left; x <= right; x++)
                {
                    if ((0x1 & (x + y)) != 0)
                    {
                        dg.floor[y][x].feature_id = Dungeon_tile_c.TMP1_WALL;
                    }
                }
            }
        }

        private void DungeonPlaceFourSmallRooms(Coord_t coord, int depth, int height, int left, int right)
        {
            var dg = State.Instance.dg;

            for (var y = height; y <= depth; y++)
            {
                dg.floor[y][coord.x].feature_id = Dungeon_tile_c.TMP1_WALL;
            }

            for (var x = left; x <= right; x++)
            {
                dg.floor[coord.y][x].feature_id = Dungeon_tile_c.TMP1_WALL;
            }

            // place random secret door
            if (this.rnd.randomNumber(2) == 1)
            {
                var offset = this.rnd.randomNumber(10);
                this.DungeonPlaceSecretDoor(new Coord_t(height - 1, coord.x - offset));
                this.DungeonPlaceSecretDoor(new Coord_t(height - 1, coord.x + offset));
                this.DungeonPlaceSecretDoor(new Coord_t(depth + 1, coord.x - offset));
                this.DungeonPlaceSecretDoor(new Coord_t(depth + 1, coord.x + offset));
            }
            else
            {
                var offset = this.rnd.randomNumber(3);
                this.DungeonPlaceSecretDoor(new Coord_t(coord.y + offset, left - 1));
                this.DungeonPlaceSecretDoor(new Coord_t(coord.y - offset, left - 1));
                this.DungeonPlaceSecretDoor(new Coord_t(coord.y + offset, right + 1));
                this.DungeonPlaceSecretDoor(new Coord_t(coord.y - offset, right + 1));
            }
        }

        // Builds a type 2 unusual room at a row, column coordinate -RAK-
        private void DungeonBuildRoomWithInnerRooms(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var floor = this.DungeonFloorTileForLevel();

            var height = coord.y - 4;
            var depth = coord.y + 4;
            var left = coord.x - 11;
            var right = coord.x + 11;

            // the x dim of rooms tends to be much larger than the y dim,
            // so don't bother rewriting the y loop.

            for (var i = height; i <= depth; i++)
            {
                for (var j = left; j <= right; j++)
                {
                    dg.floor[i][j].feature_id = floor;
                    dg.floor[i][j].perma_lit_room = true;
                }
            }

            for (var i = (height - 1); i <= (depth + 1); i++)
            {
                dg.floor[i][left - 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[i][left - 1].perma_lit_room = true;

                dg.floor[i][right + 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[i][right + 1].perma_lit_room = true;
            }

            for (var i = left; i <= right; i++)
            {
                dg.floor[height - 1][i].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[height - 1][i].perma_lit_room = true;

                dg.floor[depth + 1][i].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[depth + 1][i].perma_lit_room = true;
            }

            // The inner room
            height += 2;
            depth -= 2;
            left += 2;
            right -= 2;

            for (var i = (height - 1); i <= (depth + 1); i++)
            {
                dg.floor[i][left - 1].feature_id = Dungeon_tile_c.TMP1_WALL;
                dg.floor[i][right + 1].feature_id = Dungeon_tile_c.TMP1_WALL;
            }

            for (var i = left; i <= right; i++)
            {
                dg.floor[height - 1][i].feature_id = Dungeon_tile_c.TMP1_WALL;
                dg.floor[depth + 1][i].feature_id = Dungeon_tile_c.TMP1_WALL;
            }

            // Inner room variations
            switch ((InnerRoomTypes) this.rnd.randomNumber(5))
            {
                case InnerRoomTypes.Plain:
                    this.DungeonPlaceRandomSecretDoor(coord, depth, height, left, right);
                    this.DungeonPlaceVaultMonster(coord, 1);
                    break;
                case InnerRoomTypes.TreasureVault:
                    this.DungeonPlaceTreasureVault(coord, depth, height, left, right);

                    // Guard the treasure well
                    this.DungeonPlaceVaultMonster(coord, 2 + this.rnd.randomNumber(3));

                    // If the monsters don't get 'em.
                    this.DungeonPlaceVaultTrap(coord, new Coord_t(4, 10), 2 + this.rnd.randomNumber(3));
                    break;
                case InnerRoomTypes.Pillars:
                    this.DungeonPlaceRandomSecretDoor(coord, depth, height, left, right);

                    this.DungeonPlaceInnerPillars(coord);

                    if (this.rnd.randomNumber(3) != 1)
                    {
                        break;
                    }

                    // Inner rooms
                    for (var i = coord.x - 5; i <= coord.x + 5; i++)
                    {
                        dg.floor[coord.y - 1][i].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y + 1][i].feature_id = Dungeon_tile_c.TMP1_WALL;
                    }
                    dg.floor[coord.y][coord.x - 5].feature_id = Dungeon_tile_c.TMP1_WALL;
                    dg.floor[coord.y][coord.x + 5].feature_id = Dungeon_tile_c.TMP1_WALL;

                    this.DungeonPlaceSecretDoor(new Coord_t(coord.y - 3 + (this.rnd.randomNumber(2) << 1), coord.x - 3));
                    this.DungeonPlaceSecretDoor(new Coord_t(coord.y - 3 + (this.rnd.randomNumber(2) << 1), coord.x + 3));

                    if (this.rnd.randomNumber(3) == 1)
                    {
                        this.dungeonPlacer.dungeonPlaceRandomObjectAt(new Coord_t(coord.y, coord.x - 2), false);
                    }

                    if (this.rnd.randomNumber(3) == 1)
                    {
                        this.dungeonPlacer.dungeonPlaceRandomObjectAt(new Coord_t(coord.y, coord.x + 2), false);
                    }

                    this.DungeonPlaceVaultMonster(new Coord_t(coord.y, coord.x - 2), this.rnd.randomNumber(2));
                    this.DungeonPlaceVaultMonster(new Coord_t(coord.y, coord.x + 2), this.rnd.randomNumber(2));
                    break;
                case InnerRoomTypes.Maze:
                    this.DungeonPlaceRandomSecretDoor(coord, depth, height, left, right);

                    DungeonPlaceMazeInsideRoom(depth, height, left, right);

                    // Monsters just love mazes.
                    this.DungeonPlaceVaultMonster(new Coord_t(coord.y, coord.x - 5), this.rnd.randomNumber(3));
                    this.DungeonPlaceVaultMonster(new Coord_t(coord.y, coord.x + 5), this.rnd.randomNumber(3));

                    // Traps make them entertaining.
                    this.DungeonPlaceVaultTrap(new Coord_t(coord.y, coord.x - 3), new Coord_t(2, 8), this.rnd.randomNumber(3));
                    this.DungeonPlaceVaultTrap(new Coord_t(coord.y, coord.x + 3), new Coord_t(2, 8), this.rnd.randomNumber(3));

                    // Mazes should have some treasure too..
                    for (var i = 0; i < 3; i++)
                    {
                        this.dungeonPlacer.dungeonPlaceRandomObjectNear(coord, 1);
                    }
                    break;
                case InnerRoomTypes.FourSmallRooms:
                    this.DungeonPlaceFourSmallRooms(coord, depth, height, left, right);

                    // Treasure in each one.
                    this.dungeonPlacer.dungeonPlaceRandomObjectNear(coord, 2 + this.rnd.randomNumber(2));

                    // Gotta have some monsters.
                    this.DungeonPlaceVaultMonster(new Coord_t(coord.y + 2, coord.x - 4), this.rnd.randomNumber(2));
                    this.DungeonPlaceVaultMonster(new Coord_t(coord.y + 2, coord.x + 4), this.rnd.randomNumber(2));
                    this.DungeonPlaceVaultMonster(new Coord_t(coord.y - 2, coord.x - 4), this.rnd.randomNumber(2));
                    this.DungeonPlaceVaultMonster(new Coord_t(coord.y - 2, coord.x + 4), this.rnd.randomNumber(2));
                    break;
                default:
                    // All cases are handled, so this should never be reached!
                    break;
            }
        }

        private static void DungeonPlaceLargeMiddlePillar(Coord_t coord)
        {
            var dg = State.Instance.dg;

            for (var y = coord.y - 1; y <= coord.y + 1; y++)
            {
                for (var x = coord.x - 1; x <= coord.x + 1; x++)
                {
                    dg.floor[y][x].feature_id = Dungeon_tile_c.TMP1_WALL;
                }
            }
        }

        // Builds a room at a row, column coordinate -RAK-
        // Type 3 unusual rooms are cross shaped
        private void DungeonBuildRoomCrossShaped(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var floor = this.DungeonFloorTileForLevel();

            var randomOffset = 2 + this.rnd.randomNumber(2);

            var height = coord.y - randomOffset;
            var depth = coord.y + randomOffset;
            var left = coord.x - 1;
            var right = coord.x + 1;

            for (var i = height; i <= depth; i++)
            {
                for (var j = left; j <= right; j++)
                {
                    dg.floor[i][j].feature_id = floor;
                    dg.floor[i][j].perma_lit_room = true;
                }
            }

            for (var i = height - 1; i <= depth + 1; i++)
            {
                dg.floor[i][left - 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[i][left - 1].perma_lit_room = true;

                dg.floor[i][right + 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[i][right + 1].perma_lit_room = true;
            }

            for (var i = left; i <= right; i++)
            {
                dg.floor[height - 1][i].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[height - 1][i].perma_lit_room = true;

                dg.floor[depth + 1][i].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                dg.floor[depth + 1][i].perma_lit_room = true;
            }

            randomOffset = 2 + this.rnd.randomNumber(9);

            height = coord.y - 1;
            depth = coord.y + 1;
            left = coord.x - randomOffset;
            right = coord.x + randomOffset;

            for (var i = height; i <= depth; i++)
            {
                for (var j = left; j <= right; j++)
                {
                    dg.floor[i][j].feature_id = floor;
                    dg.floor[i][j].perma_lit_room = true;
                }
            }

            for (var i = height - 1; i <= depth + 1; i++)
            {
                if (dg.floor[i][left - 1].feature_id != floor)
                {
                    dg.floor[i][left - 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                    dg.floor[i][left - 1].perma_lit_room = true;
                }

                if (dg.floor[i][right + 1].feature_id != floor)
                {
                    dg.floor[i][right + 1].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                    dg.floor[i][right + 1].perma_lit_room = true;
                }
            }

            for (var i = left; i <= right; i++)
            {
                if (dg.floor[height - 1][i].feature_id != floor)
                {
                    dg.floor[height - 1][i].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                    dg.floor[height - 1][i].perma_lit_room = true;
                }

                if (dg.floor[depth + 1][i].feature_id != floor)
                {
                    dg.floor[depth + 1][i].feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                    dg.floor[depth + 1][i].perma_lit_room = true;
                }
            }

            // Special features.
            switch (this.rnd.randomNumber(4))
            {
                case 1: // Large middle pillar
                    DungeonPlaceLargeMiddlePillar(coord);
                    break;
                case 2: // Inner treasure vault
                    DungeonPlaceVault(coord);

                    // Place a secret door
                    randomOffset = this.rnd.randomNumber(4);
                    if (randomOffset < 3)
                    {
                        this.DungeonPlaceSecretDoor(new Coord_t(coord.y - 3 + (randomOffset << 1), coord.x));
                    }
                    else
                    {
                        this.DungeonPlaceSecretDoor(new Coord_t(coord.y, coord.x - 7 + (randomOffset << 1)));
                    }

                    // Place a treasure in the vault
                    this.dungeonPlacer.dungeonPlaceRandomObjectAt(coord, false);

                    // Let's guard the treasure well.
                    this.DungeonPlaceVaultMonster(coord, 2 + this.rnd.randomNumber(2));

                    // Traps naturally
                    this.DungeonPlaceVaultTrap(coord, new Coord_t(4, 4), 1 + this.rnd.randomNumber(3));
                    break;
                case 3:
                    if (this.rnd.randomNumber(3) == 1)
                    {
                        dg.floor[coord.y - 1][coord.x - 2].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y + 1][coord.x - 2].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y - 1][coord.x + 2].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y + 1][coord.x + 2].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y - 2][coord.x - 1].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y - 2][coord.x + 1].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y + 2][coord.x - 1].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y + 2][coord.x + 1].feature_id = Dungeon_tile_c.TMP1_WALL;
                        if (this.rnd.randomNumber(3) == 1)
                        {
                            this.DungeonPlaceSecretDoor(new Coord_t(coord.y, coord.x - 2));
                            this.DungeonPlaceSecretDoor(new Coord_t(coord.y, coord.x + 2));
                            this.DungeonPlaceSecretDoor(new Coord_t(coord.y - 2, coord.x));
                            this.DungeonPlaceSecretDoor(new Coord_t(coord.y + 2, coord.x));
                        }
                    }
                    else if (this.rnd.randomNumber(3) == 1)
                    {
                        dg.floor[coord.y][coord.x].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y - 1][coord.x].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y + 1][coord.x].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y][coord.x - 1].feature_id = Dungeon_tile_c.TMP1_WALL;
                        dg.floor[coord.y][coord.x + 1].feature_id = Dungeon_tile_c.TMP1_WALL;
                    }
                    else if (this.rnd.randomNumber(3) == 1)
                    {
                        dg.floor[coord.y][coord.x].feature_id = Dungeon_tile_c.TMP1_WALL;
                    }
                    break;
                // handled by the default case
                // case 4:
                //     // no special feature!
                //     break;
            }
        }

        // Constructs a tunnel between two points
        private void DungeonBuildTunnel(Coord_t start, Coord_t end)
        {
            var dg = State.Instance.dg;

            var tunnelsTk = ArrayInitializer.Initialize<Coord_t>(1000);
            var wallsTk = ArrayInitializer.Initialize<Coord_t>(1000);
            //Coord_t tunnels_tk[1000], walls_tk[1000];

            // Main procedure for Tunnel
            // Note: 9 is a temporary value
            var doorFlag = false;
            var stopFlag = false;
            var mainLoopCount = 0;
            var startRow = start.y;
            var startCol = start.x;
            var tunnelIndex = 0;
            var wallIndex = 0;

            this.PickCorrectDirection(out var yDirection, out var xDirection, start, end);

            do
            {
                // prevent infinite loops, just in case
                mainLoopCount++;
                if (mainLoopCount > 2000)
                {
                    stopFlag = true;
                }

                if (this.rnd.randomNumber(100) > Config.dungeon.DUN_DIR_CHANGE)
                {
                    if (this.rnd.randomNumber((int)Config.dungeon.DUN_RANDOM_DIR) == 1)
                    {
                        this.ChanceOfRandomDirection(out yDirection, out xDirection);
                    }
                    else
                    {
                        this.PickCorrectDirection(out yDirection, out xDirection, start, end);
                    }
                }

                var tmpRow = start.y + yDirection;
                var tmpCol = start.x + xDirection;

                while (!this.dungeon.coordInBounds(new Coord_t(tmpRow, tmpCol)))
                {
                    if (this.rnd.randomNumber((int)Config.dungeon.DUN_RANDOM_DIR) == 1)
                    {
                        this.ChanceOfRandomDirection(out yDirection, out xDirection);
                    }
                    else
                    {
                        this.PickCorrectDirection(out yDirection, out xDirection, start, end);
                    }
                    tmpRow = start.y + yDirection;
                    tmpCol = start.x + xDirection;
                }

                switch (dg.floor[tmpRow][tmpCol].feature_id)
                {
                    case Dungeon_tile_c.TILE_NULL_WALL:
                        start.y = tmpRow;
                        start.x = tmpCol;
                        if (tunnelIndex < 1000)
                        {
                            tunnelsTk[tunnelIndex].y = start.y;
                            tunnelsTk[tunnelIndex].x = start.x;
                            tunnelIndex++;
                        }
                        doorFlag = false;
                        break;
                    case Dungeon_tile_c.TMP2_WALL:
                        // do nothing
                        break;
                    case Dungeon_tile_c.TILE_GRANITE_WALL:
                        start.y = tmpRow;
                        start.x = tmpCol;

                        if (wallIndex < 1000)
                        {
                            wallsTk[wallIndex].y = start.y;
                            wallsTk[wallIndex].x = start.x;
                            wallIndex++;
                        }

                        for (var y = start.y - 1; y <= start.y + 1; y++)
                        {
                            for (var x = start.x - 1; x <= start.x + 1; x++)
                            {
                                if (this.dungeon.coordInBounds(new Coord_t(y, x)))
                                {
                                    // values 11 and 12 are impossible here, dungeonPlaceStreamerRock
                                    // is never run before dungeonBuildTunnel
                                    if (dg.floor[y][x].feature_id == Dungeon_tile_c.TILE_GRANITE_WALL)
                                    {
                                        dg.floor[y][x].feature_id = Dungeon_tile_c.TMP2_WALL;
                                    }
                                }
                            }
                        }
                        break;
                    case Dungeon_tile_c.TILE_CORR_FLOOR:
                    case Dungeon_tile_c.TILE_BLOCKED_FLOOR:
                        start.y = tmpRow;
                        start.x = tmpCol;

                        if (!doorFlag)
                        {
                            if (State.Instance.door_index < 100)
                            {
                                State.Instance.doors_tk[State.Instance.door_index].y = start.y;
                                State.Instance.doors_tk[State.Instance.door_index].x = start.x;
                                State.Instance.door_index++;
                            }
                            doorFlag = true;
                        }

                        if (this.rnd.randomNumber(100) > Config.dungeon.DUN_TUNNELING)
                        {
                            // make sure that tunnel has gone a reasonable distance
                            // before stopping it, this helps prevent isolated rooms
                            tmpRow = start.y - startRow;
                            if (tmpRow < 0)
                            {
                                tmpRow = -tmpRow;
                            }

                            tmpCol = start.x - startCol;
                            if (tmpCol < 0)
                            {
                                tmpCol = -tmpCol;
                            }

                            if (tmpRow > 10 || tmpCol > 10)
                            {
                                stopFlag = true;
                            }
                        }
                        break;
                    default:
                        // none of: NULL, TMP2, GRANITE, CORR
                        start.y = tmpRow;
                        start.x = tmpCol;
                        break;
                }
            } while ((start.y != end.y || start.x != end.x) && !stopFlag);

            for (var i = 0; i < tunnelIndex; i++)
            {
                dg.floor[tunnelsTk[i].y][tunnelsTk[i].x].feature_id = Dungeon_tile_c.TILE_CORR_FLOOR;
            }

            for (var i = 0; i < wallIndex; i++)
            {
                var tile = dg.floor[wallsTk[i].y][wallsTk[i].x];

                if (tile.feature_id == Dungeon_tile_c.TMP2_WALL)
                {
                    if (this.rnd.randomNumber(100) < Config.dungeon.DUN_ROOM_DOORS)
                    {
                        this.DungeonPlaceDoor(new Coord_t(wallsTk[i].y, wallsTk[i].x));
                    }
                    else
                    {
                        // these have to be doorways to rooms
                        tile.feature_id = Dungeon_tile_c.TILE_CORR_FLOOR;
                    }
                }
            }
        }

        private bool DungeonIsNextTo(Coord_t coord)
        {
            var dg = State.Instance.dg;

            if (this.dungeon.coordCorridorWallsNextTo(coord) > 2)
            {
                var vertical =
                    dg.floor[coord.y - 1][coord.x].feature_id >= Dungeon_tile_c.MIN_CAVE_WALL &&
                    dg.floor[coord.y + 1][coord.x].feature_id >= Dungeon_tile_c.MIN_CAVE_WALL;
                var horizontal =
                    dg.floor[coord.y][coord.x - 1].feature_id >= Dungeon_tile_c.MIN_CAVE_WALL &&
                    dg.floor[coord.y][coord.x + 1].feature_id >= Dungeon_tile_c.MIN_CAVE_WALL;

                return vertical || horizontal;
            }

            return false;
        }

        // Places door at y, x position if at least 2 walls found
        private void DungeonPlaceDoorIfNextToTwoWalls(Coord_t coord)
        {
            var dg = State.Instance.dg;

            if (dg.floor[coord.y][coord.x].feature_id == Dungeon_tile_c.TILE_CORR_FLOOR && this.rnd.randomNumber(100) > Config.dungeon.DUN_TUNNEL_DOORS && this.DungeonIsNextTo(coord))
            {
                this.DungeonPlaceDoor(coord);
            }
        }

        // Returns random co-ordinates -RAK-
        private void DungeonNewSpot(Coord_t coord)
        {
            var dg = State.Instance.dg;

            Tile_t tile;
            var position = new Coord_t(0, 0);

            do
            {
                position.y = this.rnd.randomNumber(dg.height - 2);
                position.x = this.rnd.randomNumber(dg.width - 2);
                tile = dg.floor[position.y][position.x];
            } while (tile.feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE || tile.creature_id != 0 || tile.treasure_id != 0);

            coord.y = position.y;
            coord.x = position.x;
        }

        // Functions to emulate the original Pascal sets
        private static bool SetRooms(int tileId)
        {
            return (tileId == Dungeon_tile_c.TILE_DARK_FLOOR || tileId == Dungeon_tile_c.TILE_LIGHT_FLOOR);
        }

        private static bool SetCorridors(int tileId)
        {
            return (tileId == Dungeon_tile_c.TILE_CORR_FLOOR || tileId == Dungeon_tile_c.TILE_BLOCKED_FLOOR);
        }

        private static bool SetFloors(int tileId)
        {
            return (tileId <= Dungeon_tile_c.MAX_CAVE_FLOOR);
        }

        // Cave logic flow for generation of new dungeon
        private void DungeonGenerate()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            // Room initialization
            var rowRooms = 2 * (dg.height / (int)Dungeon_c.SCREEN_HEIGHT);
            var colRooms = 2 * (dg.width / (int)Dungeon_c.SCREEN_WIDTH);

            var roomMap = ArrayInitializer.InitializeWithDefault(20, 20, false);

            var randomRoomCount = this.rnd.randomNumberNormalDistribution((int)Config.dungeon.DUN_ROOMS_MEAN, 2);
            for (var i = 0; i < randomRoomCount; i++)
            {
                roomMap[this.rnd.randomNumber(rowRooms) - 1][this.rnd.randomNumber(colRooms) - 1] = true;
            }

            // Build rooms
            var locationId = 0;
            var locations = ArrayInitializer.Initialize<Coord_t>(400);

            for (var row = 0; row < rowRooms; row++)
            {
                for (var col = 0; col < colRooms; col++)
                {
                    if (roomMap[row][col])
                    {
                        locations[locationId].y = (row * (int)(Dungeon_c.SCREEN_HEIGHT >> 1) + (int)Dungeon_c.QUART_HEIGHT);
                        locations[locationId].x = (col * (int)(Dungeon_c.SCREEN_WIDTH >> 1) + (int)Dungeon_c.QUART_WIDTH);
                        if (dg.current_level > this.rnd.randomNumber(Config.dungeon.DUN_UNUSUAL_ROOMS))
                        {
                            var roomType = this.rnd.randomNumber(3);

                            if (roomType == 1)
                            {
                                this.DungeonBuildRoomOverlappingRectangles(locations[locationId]);
                            }
                            else if (roomType == 2)
                            {
                                this.DungeonBuildRoomWithInnerRooms(locations[locationId]);
                            }
                            else
                            {
                                this.DungeonBuildRoomCrossShaped(locations[locationId]);
                            }
                        }
                        else
                        {
                            this.DungeonBuildRoom(locations[locationId]);
                        }
                        locationId++;
                    }
                }
            }

            for (var i = 0; i < locationId; i++)
            {
                var pick1 = this.rnd.randomNumber(locationId) - 1;
                var pick2 = this.rnd.randomNumber(locationId) - 1;

                var y = locations[pick1].y;
                var x = locations[pick1].x;
                locations[pick1].y = locations[pick2].y;
                locations[pick1].x = locations[pick2].x;
                locations[pick2].y = y;
                locations[pick2].x = x;
            }

            State.Instance.door_index = 0;

            // move zero entry to location_id, so that can call dungeonBuildTunnel all location_id times
            locations[locationId].y = locations[0].y;
            locations[locationId].x = locations[0].x;

            for (var i = 0; i < locationId; i++)
            {
                this.DungeonBuildTunnel(locations[i + 1], locations[i]);
            }

            // Generate walls and streamers
            DungeonFillEmptyTilesWith(Dungeon_tile_c.TILE_GRANITE_WALL);
            for (var i = 0; i < Config.dungeon.DUN_MAGMA_STREAMER; i++)
            {
                this.DungeonPlaceStreamerRock(Dungeon_tile_c.TILE_MAGMA_WALL, (int)Config.dungeon.DUN_MAGMA_TREASURE);
            }
            for (var i = 0; i < Config.dungeon.DUN_QUARTZ_STREAMER; i++)
            {
                this.DungeonPlaceStreamerRock(Dungeon_tile_c.TILE_QUARTZ_WALL, (int)Config.dungeon.DUN_QUARTZ_TREASURE);
            }

            DungeonPlaceBoundaryWalls();

            // Place intersection doors
            for (var i = 0; i < State.Instance.door_index; i++)
            {
                this.DungeonPlaceDoorIfNextToTwoWalls(new Coord_t(State.Instance.doors_tk[i].y, State.Instance.doors_tk[i].x - 1));
                this.DungeonPlaceDoorIfNextToTwoWalls(new Coord_t(State.Instance.doors_tk[i].y, State.Instance.doors_tk[i].x + 1));
                this.DungeonPlaceDoorIfNextToTwoWalls(new Coord_t(State.Instance.doors_tk[i].y - 1, State.Instance.doors_tk[i].x));
                this.DungeonPlaceDoorIfNextToTwoWalls(new Coord_t(State.Instance.doors_tk[i].y + 1, State.Instance.doors_tk[i].x));
            }

            var allocLevel = (dg.current_level / 3);
            if (allocLevel < 2)
            {
                allocLevel = 2;
            }
            else if (allocLevel > 10)
            {
                allocLevel = 10;
            }

            this.DungeonPlaceStairs(2, this.rnd.randomNumber(2) + 2, 3);
            this.DungeonPlaceStairs(1, this.rnd.randomNumber(2), 3);

            // Set up the character coords, used by monsterPlaceNewWithinDistance, monsterPlaceWinning
            var coord = new Coord_t(0, 0);
            this.DungeonNewSpot(coord);
            //py.pos.y = coord.y;
            //py.pos.x = coord.x;
            py.pos = coord;

            this.monsterManager.monsterPlaceNewWithinDistance((this.rnd.randomNumber(8) + (int)Config.monsters.MON_MIN_PER_LEVEL + allocLevel), 0, true);
            this.dungeonPlacer.dungeonAllocateAndPlaceObject(SetCorridors, 3, this.rnd.randomNumber(allocLevel));
            this.dungeonPlacer.dungeonAllocateAndPlaceObject(SetRooms, 5, this.rnd.randomNumberNormalDistribution((int)Config.dungeon_objects.LEVEL_OBJECTS_PER_ROOM, 3));
            this.dungeonPlacer.dungeonAllocateAndPlaceObject(SetFloors, 5, this.rnd.randomNumberNormalDistribution((int)Config.dungeon_objects.LEVEL_OBJECTS_PER_CORRIDOR, 3));
            this.dungeonPlacer.dungeonAllocateAndPlaceObject(SetFloors, 4, this.rnd.randomNumberNormalDistribution((int)Config.dungeon_objects.LEVEL_TOTAL_GOLD_AND_GEMS, 3));
            this.dungeonPlacer.dungeonAllocateAndPlaceObject(SetFloors, 1, this.rnd.randomNumber(allocLevel));

            if (dg.current_level >= Config.monsters.MON_ENDGAME_LEVEL)
            {
                this.monsterManager.monsterPlaceWinning();
            }
        }

        // Builds a store at a row, column coordinate
        private void DungeonBuildStore(int storeId, Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var yValue = coord.y * 10 + 5;
            var xValue = coord.x * 16 + 16;
            var height = yValue - this.rnd.randomNumber(3);
            var depth = yValue + this.rnd.randomNumber(4);
            var left = xValue - this.rnd.randomNumber(6);
            var right = xValue + this.rnd.randomNumber(6);

            int y, x;

            for (y = height; y <= depth; y++)
            {
                for (x = left; x <= right; x++)
                {
                    dg.floor[y][x].feature_id = Dungeon_tile_c.TILE_BOUNDARY_WALL;
                }
            }

            var tmp = this.rnd.randomNumber(4);
            if (tmp < 3)
            {
                y = this.rnd.randomNumber(depth - height) + height - 1;

                x = tmp == 1 ? left : right;
            }
            else
            {
                x = this.rnd.randomNumber(right - left) + left - 1;

                y = tmp == 3 ? depth : height;
            }

            dg.floor[y][x].feature_id = Dungeon_tile_c.TILE_CORR_FLOOR;

            var curPos = this.gameObjects.popt();
            dg.floor[y][x].treasure_id = (uint)curPos;

            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_STORE_DOOR + storeId, game.treasure.list[curPos]);
        }

        // Link all free space in treasure list together
        private void TreasureLinker()
        {
            foreach (var item in State.Instance.game.treasure.list)
            {
                this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, item);
            }
            State.Instance.game.treasure.current_id = (int)Config.treasure.MIN_TREASURE_LIST_ID;
        }

        // Link all free space in monster list together
        private static void MonsterLinker()
        {
            var monsters = State.Instance.monsters;
            for (var i = 0; i < monsters.Length; i++)
            {
                monsters[i] = State.Instance.blank_monster;
            }
            State.Instance.next_free_monster_id = (int)Config.monsters.MON_MIN_INDEX_ID;
        }

        private void DungeonPlaceTownStores()
        {
            var rooms = new int[6];
            for (var i = 0; i < 6; i++)
            {
                rooms[i] = i;
            }

            var roomsCount = 6;

            for (var y = 0; y < 2; y++)
            {
                for (var x = 0; x < 3; x++)
                {
                    var roomId = this.rnd.randomNumber(roomsCount) - 1;
                    this.DungeonBuildStore(rooms[roomId], new Coord_t(y, x));

                    for (var i = roomId; i < roomsCount - 1; i++)
                    {
                        rooms[i] = rooms[i + 1];
                    }

                    roomsCount--;
                }
            }
        }

        private static bool IsNighTime()
        {
            var dg = State.Instance.dg;
            return (0x1 & (dg.game_turn / 5000)) != 0;
        }

        // Light town based on whether it is Night time, or day time.
        private void LightTown()
        {
            var dg = State.Instance.dg;
            if (IsNighTime())
            {
                for (var y = 0; y < dg.height; y++)
                {
                    for (var x = 0; x < dg.width; x++)
                    {
                        if (dg.floor[y][x].feature_id != Dungeon_tile_c.TILE_DARK_FLOOR)
                        {
                            dg.floor[y][x].permanent_light = true;
                        }
                    }
                }

                this.monsterManager.monsterPlaceNewWithinDistance((int)Config.monsters.MON_MIN_TOWNSFOLK_NIGHT, 3, true);
            }
            else
            {
                // ...it is day time
                for (var y = 0; y < dg.height; y++)
                {
                    for (var x = 0; x < dg.width; x++)
                    {
                        dg.floor[y][x].permanent_light = true;
                    }
                }

                this.monsterManager.monsterPlaceNewWithinDistance((int)Config.monsters.MON_MIN_TOWNSFOLK_DAY, 3, true);
            }
        }

        // I may have written the town level code, but I'm not exactly
        // proud of it.   Adding the stores required some real slucky
        // hooks which I have not had time to re-think. -RAK-

        // Town logic flow for generation of new town
        private void TownGeneration()
        {
            this.rnd.seedSet(State.Instance.game.town_seed);

            this.DungeonPlaceTownStores();

            DungeonFillEmptyTilesWith(Dungeon_tile_c.TILE_DARK_FLOOR);

            // make stairs before seedResetToOldSeed, so that they don't move around
            DungeonPlaceBoundaryWalls();
            this.DungeonPlaceStairs(2, 1, 0);

            this.rnd.seedResetToOldSeed();

            // Set up the character coords, used by monsterPlaceNewWithinDistance below
            var coord = new Coord_t(0, 0);
            this.DungeonNewSpot(coord);
            //State.Instance.py.pos.y = coord.y;
            //State.Instance.py.pos.x = coord.x;
            State.Instance.py.pos = coord;

            this.LightTown();

            this.storeInventory.storeMaintenance();
        }

        // Generates a random dungeon level -RAK-
        public void GenerateCave()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            dg.panel.top = 0;
            dg.panel.bottom = 0;
            dg.panel.left = 0;
            dg.panel.right = 0;

            py.pos.y = -1;
            py.pos.x = -1;

            this.TreasureLinker();
            MonsterLinker();
            DungeonBlankEntireCave();

            // We're in the dungeon more than the town, so let's default to that -MRC-
            dg.height = (int)Dungeon_c.MAX_HEIGHT;
            dg.width = (int)Dungeon_c.MAX_WIDTH;

            if (dg.current_level == 0)
            {
                dg.height = (int)Dungeon_c.SCREEN_HEIGHT;
                dg.width = (int)Dungeon_c.SCREEN_WIDTH;
            }

            dg.panel.max_rows = ((dg.height / (int)Dungeon_c.SCREEN_HEIGHT) * 2 - 2);
            dg.panel.max_cols = ((dg.width / (int)Dungeon_c.SCREEN_WIDTH) * 2 - 2);

            dg.panel.row = dg.panel.max_rows;
            dg.panel.col = dg.panel.max_cols;

            if (dg.current_level == 0)
            {
                this.TownGeneration();
            }
            else
            {
                this.DungeonGenerate();
            }
        }
    }
}