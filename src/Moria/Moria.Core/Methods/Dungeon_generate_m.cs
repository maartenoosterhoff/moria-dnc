using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using Moria.Core.Utils;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Player_m;

namespace Moria.Core.Methods
{
    public interface IDungeonGenerate
    {
        void generateCave();
    }

    public class Dungeon_generate_m : IDungeonGenerate
    {
        private readonly IDungeon dungeon;
        private readonly IMonsterManager monsterManager;
        private readonly IRnd rnd;
        private readonly IStoreInventory storeInventory;

        public Dungeon_generate_m(
            IDungeon dungeon,
            IMonsterManager monsterManager,
            IRnd rnd,
            IStoreInventory storeInventory
        )
        {
            this.dungeon = dungeon;
            this.monsterManager = monsterManager;
            this.rnd = rnd;
            this.storeInventory = storeInventory;
        }

        // Returns a Dark/Light floor tile based on dg.current_level, and random number
        private uint dungeonFloorTileForLevel()
        {
            var dg = State.Instance.dg;
            if (dg.current_level <= this.rnd.randomNumber(25))
            {
                return TILE_LIGHT_FLOOR;
            }
            return TILE_DARK_FLOOR;
        }

        // Always picks a correct direction
        private void pickCorrectDirection(ref int vertical, ref int horizontal, Coord_t start, Coord_t end)
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
        private void chanceOfRandomDirection(ref int vertical, ref int horizontal)
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
        private void dungeonBlankEntireCave()
        {
            State.Instance.dg.floor = ArrayInitializer.Initialize<Tile_t>(Dungeon_c.MAX_HEIGHT, Dungeon_c.MAX_WIDTH);
            //memset((char*)&dg.floor[0][0], 0, sizeof(dg.floor));
        }

        // Fills in empty spots with desired rock -RAK-
        // Note: 9 is a temporary value.
        private void dungeonFillEmptyTilesWith(uint rock_type)
        {
            var dg = State.Instance.dg;

            // no need to check the border of the cave
            for (var y = dg.height - 2; y > 0; y--)
            {
                var x = 1;

                for (var j = dg.width - 2; j > 0; j--)
                {
                    if (dg.floor[y][x].feature_id == TILE_NULL_WALL ||
                        dg.floor[y][x].feature_id == TMP1_WALL ||
                        dg.floor[y][x].feature_id == TMP2_WALL)
                    {
                        dg.floor[y][x].feature_id = rock_type;
                    }
                    x++;
                }
            }
        }

        // Places indestructible rock around edges of dungeon -RAK-
        private void dungeonPlaceBoundaryWalls()
        {
            var dg = State.Instance.dg;

            for (var x = 0; x < MAX_WIDTH; x++)
            {
                dg.floor[0][x].feature_id = TILE_BOUNDARY_WALL;
                dg.floor[dg.height - 1][x].feature_id = TILE_BOUNDARY_WALL;
            }

            for (var y = 0; y < MAX_HEIGHT; y++)
            {
                dg.floor[y][0].feature_id = TILE_BOUNDARY_WALL;
                dg.floor[y][dg.width - 1].feature_id = TILE_BOUNDARY_WALL;
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
        private void dungeonPlaceStreamerRock(uint rock_type, int chance_of_treasure)
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

                    if (dungeon.coordInBounds(spot))
                    {
                        if (dg.floor[spot.y][spot.x].feature_id == TILE_GRANITE_WALL)
                        {
                            dg.floor[spot.y][spot.x].feature_id = rock_type;

                            if (this.rnd.randomNumber(chance_of_treasure) == 1)
                            {
                                dungeon.dungeonPlaceGold(spot);
                            }
                        }
                    }
                }
            } while (playerMovePosition(dir, ref coord));
        }

        private void dungeonPlaceOpenDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var cur_pos = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)cur_pos;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_OPEN_DOOR, game.treasure.list[cur_pos]);
            dg.floor[coord.y][coord.x].feature_id = TILE_CORR_FLOOR;
        }

        private void dungeonPlaceBrokenDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var cur_pos = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)cur_pos;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_OPEN_DOOR, game.treasure.list[cur_pos]);
            dg.floor[coord.y][coord.x].feature_id = TILE_CORR_FLOOR;
            game.treasure.list[cur_pos].misc_use = 1;
        }

        private void dungeonPlaceClosedDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var cur_pos = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)cur_pos;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, game.treasure.list[cur_pos]);
            dg.floor[coord.y][coord.x].feature_id = TILE_BLOCKED_FLOOR;
        }

        private void dungeonPlaceLockedDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var cur_pos = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)cur_pos;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, game.treasure.list[cur_pos]);
            dg.floor[coord.y][coord.x].feature_id = TILE_BLOCKED_FLOOR;
            game.treasure.list[cur_pos].misc_use = this.rnd.randomNumber(10) + 10;
        }

        private void dungeonPlaceStuckDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var cur_pos = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)cur_pos;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, game.treasure.list[cur_pos]);
            dg.floor[coord.y][coord.x].feature_id = TILE_BLOCKED_FLOOR;
            game.treasure.list[cur_pos].misc_use = (-this.rnd.randomNumber(10) - 10);
        }

        private void dungeonPlaceSecretDoor(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var cur_pos = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)cur_pos;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_SECRET_DOOR, game.treasure.list[cur_pos]);
            dg.floor[coord.y][coord.x].feature_id = TILE_BLOCKED_FLOOR;
        }

        private void dungeonPlaceDoor(Coord_t coord)
        {
            var door_type = this.rnd.randomNumber(3);

            if (door_type == 1)
            {
                if (this.rnd.randomNumber(4) == 1)
                {
                    dungeonPlaceBrokenDoor(coord);
                }
                else
                {
                    dungeonPlaceOpenDoor(coord);
                }
            }
            else if (door_type == 2)
            {
                door_type = this.rnd.randomNumber(12);

                if (door_type > 3)
                {
                    dungeonPlaceClosedDoor(coord);
                }
                else if (door_type == 3)
                {
                    dungeonPlaceStuckDoor(coord);
                }
                else
                {
                    dungeonPlaceLockedDoor(coord);
                }
            }
            else
            {
                dungeonPlaceSecretDoor(coord);
            }
        }

        // Place an up staircase at given y, x -RAK-
        private void dungeonPlaceUpStairs(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            if (dg.floor[coord.y][coord.x].treasure_id != 0)
            {
                dungeon.dungeonDeleteObject(coord);
            }

            var cur_pos = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)cur_pos;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_UP_STAIR, game.treasure.list[cur_pos]);
        }

        // Place a down staircase at given y, x -RAK-
        private void dungeonPlaceDownStairs(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            if (dg.floor[coord.y][coord.x].treasure_id != 0)
            {
                dungeon.dungeonDeleteObject(coord);
            }

            var cur_pos = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)cur_pos;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_DOWN_STAIR, game.treasure.list[cur_pos]);
        }

        // Places a staircase 1=up, 2=down -RAK-
        private void dungeonPlaceStairs(int stair_type, int number, int walls)
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
                                if (dg.floor[coord1.y][coord1.x].feature_id <= MAX_OPEN_SPACE &&
                                    dg.floor[coord1.y][coord1.x].treasure_id == 0 && dungeon.coordWallsNextTo(coord1) >= walls)
                                {
                                    placed = true;
                                    if (stair_type == 1)
                                    {
                                        dungeonPlaceUpStairs(coord1);
                                    }
                                    else
                                    {
                                        dungeonPlaceDownStairs(coord1);
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
        private void dungeonPlaceVaultTrap(Coord_t coord, Coord_t displacement, int number)
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

                    if (dg.floor[spot.y][spot.x].feature_id != TILE_NULL_WALL &&
                        dg.floor[spot.y][spot.x].feature_id <= MAX_CAVE_FLOOR &&
                        dg.floor[spot.y][spot.x].treasure_id == 0)
                    {
                        dungeon.dungeonSetTrap(spot, this.rnd.randomNumber((int)Config.dungeon_objects.MAX_TRAPS) - 1);
                        placed = true;
                    }
                }
            }
        }

        // Place a trap with a given displacement of point -RAK-
        private void dungeonPlaceVaultMonster(Coord_t coord, int number)
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
        private void dungeonBuildRoom(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var floor = dungeonFloorTileForLevel();

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
                dg.floor[y][left - 1].feature_id = TILE_GRANITE_WALL;
                dg.floor[y][left - 1].perma_lit_room = true;

                dg.floor[y][right + 1].feature_id = TILE_GRANITE_WALL;
                dg.floor[y][right + 1].perma_lit_room = true;
            }

            for (x = left; x <= right; x++)
            {
                dg.floor[height - 1][x].feature_id = TILE_GRANITE_WALL;
                dg.floor[height - 1][x].perma_lit_room = true;

                dg.floor[depth + 1][x].feature_id = TILE_GRANITE_WALL;
                dg.floor[depth + 1][x].perma_lit_room = true;
            }
        }

        // Builds a room at a row, column coordinate -RAK-
        // Type 1 unusual rooms are several overlapping rectangular ones
        private void dungeonBuildRoomOverlappingRectangles(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var floor = dungeonFloorTileForLevel();

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
                        dg.floor[y][left - 1].feature_id = TILE_GRANITE_WALL;
                        dg.floor[y][left - 1].perma_lit_room = true;
                    }

                    if (dg.floor[y][right + 1].feature_id != floor)
                    {
                        dg.floor[y][right + 1].feature_id = TILE_GRANITE_WALL;
                        dg.floor[y][right + 1].perma_lit_room = true;
                    }
                }

                for (x = left; x <= right; x++)
                {
                    if (dg.floor[height - 1][x].feature_id != floor)
                    {
                        dg.floor[height - 1][x].feature_id = TILE_GRANITE_WALL;
                        dg.floor[height - 1][x].perma_lit_room = true;
                    }

                    if (dg.floor[depth + 1][x].feature_id != floor)
                    {
                        dg.floor[depth + 1][x].feature_id = TILE_GRANITE_WALL;
                        dg.floor[depth + 1][x].perma_lit_room = true;
                    }
                }
            }
        }

        private void dungeonPlaceRandomSecretDoor(Coord_t coord, int depth, int height, int left, int right)
        {
            switch (this.rnd.randomNumber(4))
            {
                case 1:
                    dungeonPlaceSecretDoor(new Coord_t(height - 1, coord.x));
                    break;
                case 2:
                    dungeonPlaceSecretDoor(new Coord_t(depth + 1, coord.x));
                    break;
                case 3:
                    dungeonPlaceSecretDoor(new Coord_t(coord.y, left - 1));
                    break;
                default:
                    dungeonPlaceSecretDoor(new Coord_t(coord.y, right + 1));
                    break;
            }
        }

        private void dungeonPlaceVault(Coord_t coord)
        {
            var dg = State.Instance.dg;

            for (var y = coord.y - 1; y <= coord.y + 1; y++)
            {
                dg.floor[y][coord.x - 1].feature_id = TMP1_WALL;
                dg.floor[y][coord.x + 1].feature_id = TMP1_WALL;
            }

            dg.floor[coord.y - 1][coord.x].feature_id = TMP1_WALL;
            dg.floor[coord.y + 1][coord.x].feature_id = TMP1_WALL;
        }

        private void dungeonPlaceTreasureVault(Coord_t coord, int depth, int height, int left, int right)
        {
            dungeonPlaceRandomSecretDoor(coord, depth, height, left, right);
            dungeonPlaceVault(coord);

            // Place a locked door
            var offset = this.rnd.randomNumber(4);
            if (offset < 3)
            {
                // 1 -> y-1; 2 -> y+1
                dungeonPlaceLockedDoor(new Coord_t(coord.y - 3 + (offset << 1), coord.x));
            }
            else
            {
                dungeonPlaceLockedDoor(new Coord_t(coord.y, coord.x - 7 + (offset << 1)));
            }
        }

        private void dungeonPlaceInnerPillars(Coord_t coord)
        {
            var dg = State.Instance.dg;

            int y, x;

            for (y = coord.y - 1; y <= coord.y + 1; y++)
            {
                for (x = coord.x - 1; x <= coord.x + 1; x++)
                {
                    dg.floor[y][x].feature_id = TMP1_WALL;
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
                    dg.floor[y][x].feature_id = TMP1_WALL;
                }
            }

            for (y = coord.y - 1; y <= coord.y + 1; y++)
            {
                for (x = coord.x + 3 + offset; x <= coord.x + 5 + offset; x++)
                {
                    dg.floor[y][x].feature_id = TMP1_WALL;
                }
            }
        }

        private void dungeonPlaceMazeInsideRoom(int depth, int height, int left, int right)
        {
            var dg = State.Instance.dg;

            for (var y = height; y <= depth; y++)
            {
                for (var x = left; x <= right; x++)
                {
                    if ((0x1 & (x + y)) != 0)
                    {
                        dg.floor[y][x].feature_id = TMP1_WALL;
                    }
                }
            }
        }

        private void dungeonPlaceFourSmallRooms(Coord_t coord, int depth, int height, int left, int right)
        {
            var dg = State.Instance.dg;

            for (var y = height; y <= depth; y++)
            {
                dg.floor[y][coord.x].feature_id = TMP1_WALL;
            }

            for (var x = left; x <= right; x++)
            {
                dg.floor[coord.y][x].feature_id = TMP1_WALL;
            }

            // place random secret door
            if (this.rnd.randomNumber(2) == 1)
            {
                var offset = this.rnd.randomNumber(10);
                dungeonPlaceSecretDoor(new Coord_t(height - 1, coord.x - offset));
                dungeonPlaceSecretDoor(new Coord_t(height - 1, coord.x + offset));
                dungeonPlaceSecretDoor(new Coord_t(depth + 1, coord.x - offset));
                dungeonPlaceSecretDoor(new Coord_t(depth + 1, coord.x + offset));
            }
            else
            {
                var offset = this.rnd.randomNumber(3);
                dungeonPlaceSecretDoor(new Coord_t(coord.y + offset, left - 1));
                dungeonPlaceSecretDoor(new Coord_t(coord.y - offset, left - 1));
                dungeonPlaceSecretDoor(new Coord_t(coord.y + offset, right + 1));
                dungeonPlaceSecretDoor(new Coord_t(coord.y - offset, right + 1));
            }
        }

        // Builds a type 2 unusual room at a row, column coordinate -RAK-
        private void dungeonBuildRoomWithInnerRooms(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var floor = dungeonFloorTileForLevel();

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
                dg.floor[i][left - 1].feature_id = TILE_GRANITE_WALL;
                dg.floor[i][left - 1].perma_lit_room = true;

                dg.floor[i][right + 1].feature_id = TILE_GRANITE_WALL;
                dg.floor[i][right + 1].perma_lit_room = true;
            }

            for (var i = left; i <= right; i++)
            {
                dg.floor[height - 1][i].feature_id = TILE_GRANITE_WALL;
                dg.floor[height - 1][i].perma_lit_room = true;

                dg.floor[depth + 1][i].feature_id = TILE_GRANITE_WALL;
                dg.floor[depth + 1][i].perma_lit_room = true;
            }

            // The inner room
            height = height + 2;
            depth = depth - 2;
            left = left + 2;
            right = right - 2;

            for (var i = (height - 1); i <= (depth + 1); i++)
            {
                dg.floor[i][left - 1].feature_id = TMP1_WALL;
                dg.floor[i][right + 1].feature_id = TMP1_WALL;
            }

            for (var i = left; i <= right; i++)
            {
                dg.floor[height - 1][i].feature_id = TMP1_WALL;
                dg.floor[depth + 1][i].feature_id = TMP1_WALL;
            }

            // Inner room variations
            switch ((InnerRoomTypes)this.rnd.randomNumber(5))
            {
                case InnerRoomTypes.Plain:
                    dungeonPlaceRandomSecretDoor(coord, depth, height, left, right);
                    dungeonPlaceVaultMonster(coord, 1);
                    break;
                case InnerRoomTypes.TreasureVault:
                    dungeonPlaceTreasureVault(coord, depth, height, left, right);

                    // Guard the treasure well
                    dungeonPlaceVaultMonster(coord, 2 + this.rnd.randomNumber(3));

                    // If the monsters don't get 'em.
                    dungeonPlaceVaultTrap(coord, new Coord_t(4, 10), 2 + this.rnd.randomNumber(3));
                    break;
                case InnerRoomTypes.Pillars:
                    dungeonPlaceRandomSecretDoor(coord, depth, height, left, right);

                    dungeonPlaceInnerPillars(coord);

                    if (this.rnd.randomNumber(3) != 1)
                    {
                        break;
                    }

                    // Inner rooms
                    for (var i = coord.x - 5; i <= coord.x + 5; i++)
                    {
                        dg.floor[coord.y - 1][i].feature_id = TMP1_WALL;
                        dg.floor[coord.y + 1][i].feature_id = TMP1_WALL;
                    }
                    dg.floor[coord.y][coord.x - 5].feature_id = TMP1_WALL;
                    dg.floor[coord.y][coord.x + 5].feature_id = TMP1_WALL;

                    dungeonPlaceSecretDoor(new Coord_t(coord.y - 3 + (this.rnd.randomNumber(2) << 1), coord.x - 3));
                    dungeonPlaceSecretDoor(new Coord_t(coord.y - 3 + (this.rnd.randomNumber(2) << 1), coord.x + 3));

                    if (this.rnd.randomNumber(3) == 1)
                    {
                        dungeon.dungeonPlaceRandomObjectAt(new Coord_t(coord.y, coord.x - 2), false);
                    }

                    if (this.rnd.randomNumber(3) == 1)
                    {
                        dungeon.dungeonPlaceRandomObjectAt(new Coord_t(coord.y, coord.x + 2), false);
                    }

                    dungeonPlaceVaultMonster(new Coord_t(coord.y, coord.x - 2), this.rnd.randomNumber(2));
                    dungeonPlaceVaultMonster(new Coord_t(coord.y, coord.x + 2), this.rnd.randomNumber(2));
                    break;
                case InnerRoomTypes.Maze:
                    dungeonPlaceRandomSecretDoor(coord, depth, height, left, right);

                    dungeonPlaceMazeInsideRoom(depth, height, left, right);

                    // Monsters just love mazes.
                    dungeonPlaceVaultMonster(new Coord_t(coord.y, coord.x - 5), this.rnd.randomNumber(3));
                    dungeonPlaceVaultMonster(new Coord_t(coord.y, coord.x + 5), this.rnd.randomNumber(3));

                    // Traps make them entertaining.
                    dungeonPlaceVaultTrap(new Coord_t(coord.y, coord.x - 3), new Coord_t(2, 8), this.rnd.randomNumber(3));
                    dungeonPlaceVaultTrap(new Coord_t(coord.y, coord.x + 3), new Coord_t(2, 8), this.rnd.randomNumber(3));

                    // Mazes should have some treasure too..
                    for (var i = 0; i < 3; i++)
                    {
                        dungeon.dungeonPlaceRandomObjectNear(coord, 1);
                    }
                    break;
                case InnerRoomTypes.FourSmallRooms:
                    dungeonPlaceFourSmallRooms(coord, depth, height, left, right);

                    // Treasure in each one.
                    dungeon.dungeonPlaceRandomObjectNear(coord, 2 + this.rnd.randomNumber(2));

                    // Gotta have some monsters.
                    dungeonPlaceVaultMonster(new Coord_t(coord.y + 2, coord.x - 4), this.rnd.randomNumber(2));
                    dungeonPlaceVaultMonster(new Coord_t(coord.y + 2, coord.x + 4), this.rnd.randomNumber(2));
                    dungeonPlaceVaultMonster(new Coord_t(coord.y - 2, coord.x - 4), this.rnd.randomNumber(2));
                    dungeonPlaceVaultMonster(new Coord_t(coord.y - 2, coord.x + 4), this.rnd.randomNumber(2));
                    break;
                default:
                    // All cases are handled, so this should never be reached!
                    break;
            }
        }

        private void dungeonPlaceLargeMiddlePillar(Coord_t coord)
        {
            var dg = State.Instance.dg;

            for (var y = coord.y - 1; y <= coord.y + 1; y++)
            {
                for (var x = coord.x - 1; x <= coord.x + 1; x++)
                {
                    dg.floor[y][x].feature_id = TMP1_WALL;
                }
            }
        }

        // Builds a room at a row, column coordinate -RAK-
        // Type 3 unusual rooms are cross shaped
        private void dungeonBuildRoomCrossShaped(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var floor = dungeonFloorTileForLevel();

            var random_offset = 2 + this.rnd.randomNumber(2);

            var height = coord.y - random_offset;
            var depth = coord.y + random_offset;
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
                dg.floor[i][left - 1].feature_id = TILE_GRANITE_WALL;
                dg.floor[i][left - 1].perma_lit_room = true;

                dg.floor[i][right + 1].feature_id = TILE_GRANITE_WALL;
                dg.floor[i][right + 1].perma_lit_room = true;
            }

            for (var i = left; i <= right; i++)
            {
                dg.floor[height - 1][i].feature_id = TILE_GRANITE_WALL;
                dg.floor[height - 1][i].perma_lit_room = true;

                dg.floor[depth + 1][i].feature_id = TILE_GRANITE_WALL;
                dg.floor[depth + 1][i].perma_lit_room = true;
            }

            random_offset = 2 + this.rnd.randomNumber(9);

            height = coord.y - 1;
            depth = coord.y + 1;
            left = coord.x - random_offset;
            right = coord.x + random_offset;

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
                    dg.floor[i][left - 1].feature_id = TILE_GRANITE_WALL;
                    dg.floor[i][left - 1].perma_lit_room = true;
                }

                if (dg.floor[i][right + 1].feature_id != floor)
                {
                    dg.floor[i][right + 1].feature_id = TILE_GRANITE_WALL;
                    dg.floor[i][right + 1].perma_lit_room = true;
                }
            }

            for (var i = left; i <= right; i++)
            {
                if (dg.floor[height - 1][i].feature_id != floor)
                {
                    dg.floor[height - 1][i].feature_id = TILE_GRANITE_WALL;
                    dg.floor[height - 1][i].perma_lit_room = true;
                }

                if (dg.floor[depth + 1][i].feature_id != floor)
                {
                    dg.floor[depth + 1][i].feature_id = TILE_GRANITE_WALL;
                    dg.floor[depth + 1][i].perma_lit_room = true;
                }
            }

            // Special features.
            switch (this.rnd.randomNumber(4))
            {
                case 1: // Large middle pillar
                    dungeonPlaceLargeMiddlePillar(coord);
                    break;
                case 2: // Inner treasure vault
                    dungeonPlaceVault(coord);

                    // Place a secret door
                    random_offset = this.rnd.randomNumber(4);
                    if (random_offset < 3)
                    {
                        dungeonPlaceSecretDoor(new Coord_t(coord.y - 3 + (random_offset << 1), coord.x));
                    }
                    else
                    {
                        dungeonPlaceSecretDoor(new Coord_t(coord.y, coord.x - 7 + (random_offset << 1)));
                    }

                    // Place a treasure in the vault
                    dungeon.dungeonPlaceRandomObjectAt(coord, false);

                    // Let's guard the treasure well.
                    dungeonPlaceVaultMonster(coord, 2 + this.rnd.randomNumber(2));

                    // Traps naturally
                    dungeonPlaceVaultTrap(coord, new Coord_t(4, 4), 1 + this.rnd.randomNumber(3));
                    break;
                case 3:
                    if (this.rnd.randomNumber(3) == 1)
                    {
                        dg.floor[coord.y - 1][coord.x - 2].feature_id = TMP1_WALL;
                        dg.floor[coord.y + 1][coord.x - 2].feature_id = TMP1_WALL;
                        dg.floor[coord.y - 1][coord.x + 2].feature_id = TMP1_WALL;
                        dg.floor[coord.y + 1][coord.x + 2].feature_id = TMP1_WALL;
                        dg.floor[coord.y - 2][coord.x - 1].feature_id = TMP1_WALL;
                        dg.floor[coord.y - 2][coord.x + 1].feature_id = TMP1_WALL;
                        dg.floor[coord.y + 2][coord.x - 1].feature_id = TMP1_WALL;
                        dg.floor[coord.y + 2][coord.x + 1].feature_id = TMP1_WALL;
                        if (this.rnd.randomNumber(3) == 1)
                        {
                            dungeonPlaceSecretDoor(new Coord_t(coord.y, coord.x - 2));
                            dungeonPlaceSecretDoor(new Coord_t(coord.y, coord.x + 2));
                            dungeonPlaceSecretDoor(new Coord_t(coord.y - 2, coord.x));
                            dungeonPlaceSecretDoor(new Coord_t(coord.y + 2, coord.x));
                        }
                    }
                    else if (this.rnd.randomNumber(3) == 1)
                    {
                        dg.floor[coord.y][coord.x].feature_id = TMP1_WALL;
                        dg.floor[coord.y - 1][coord.x].feature_id = TMP1_WALL;
                        dg.floor[coord.y + 1][coord.x].feature_id = TMP1_WALL;
                        dg.floor[coord.y][coord.x - 1].feature_id = TMP1_WALL;
                        dg.floor[coord.y][coord.x + 1].feature_id = TMP1_WALL;
                    }
                    else if (this.rnd.randomNumber(3) == 1)
                    {
                        dg.floor[coord.y][coord.x].feature_id = TMP1_WALL;
                    }
                    break;
                // handled by the default case
                // case 4:
                //     // no special feature!
                //     break;
                default:
                    break;
            }
        }

        // Constructs a tunnel between two points
        private void dungeonBuildTunnel(Coord_t start, Coord_t end)
        {
            var dg = State.Instance.dg;

            var tunnels_tk = ArrayInitializer.Initialize<Coord_t>(1000);
            var walls_tk = ArrayInitializer.Initialize<Coord_t>(1000);
            //Coord_t tunnels_tk[1000], walls_tk[1000];

            // Main procedure for Tunnel
            // Note: 9 is a temporary value
            var door_flag = false;
            var stop_flag = false;
            var main_loop_count = 0;
            var start_row = start.y;
            var start_col = start.x;
            var tunnel_index = 0;
            var wall_index = 0;

            int y_direction = 0, x_direction = 0;
            pickCorrectDirection(ref y_direction, ref x_direction, start, end);

            do
            {
                // prevent infinite loops, just in case
                main_loop_count++;
                if (main_loop_count > 2000)
                {
                    stop_flag = true;
                }

                if (this.rnd.randomNumber(100) > Config.dungeon.DUN_DIR_CHANGE)
                {
                    if (this.rnd.randomNumber((int)Config.dungeon.DUN_RANDOM_DIR) == 1)
                    {
                        chanceOfRandomDirection(ref y_direction, ref x_direction);
                    }
                    else
                    {
                        pickCorrectDirection(ref y_direction, ref x_direction, start, end);
                    }
                }

                var tmp_row = start.y + y_direction;
                var tmp_col = start.x + x_direction;

                while (!dungeon.coordInBounds(new Coord_t(tmp_row, tmp_col)))
                {
                    if (this.rnd.randomNumber((int)Config.dungeon.DUN_RANDOM_DIR) == 1)
                    {
                        chanceOfRandomDirection(ref y_direction, ref x_direction);
                    }
                    else
                    {
                        pickCorrectDirection(ref y_direction, ref x_direction, start, end);
                    }
                    tmp_row = start.y + y_direction;
                    tmp_col = start.x + x_direction;
                }

                switch (dg.floor[tmp_row][tmp_col].feature_id)
                {
                    case TILE_NULL_WALL:
                        start.y = tmp_row;
                        start.x = tmp_col;
                        if (tunnel_index < 1000)
                        {
                            tunnels_tk[tunnel_index].y = start.y;
                            tunnels_tk[tunnel_index].x = start.x;
                            tunnel_index++;
                        }
                        door_flag = false;
                        break;
                    case TMP2_WALL:
                        // do nothing
                        break;
                    case TILE_GRANITE_WALL:
                        start.y = tmp_row;
                        start.x = tmp_col;

                        if (wall_index < 1000)
                        {
                            walls_tk[wall_index].y = start.y;
                            walls_tk[wall_index].x = start.x;
                            wall_index++;
                        }

                        for (var y = start.y - 1; y <= start.y + 1; y++)
                        {
                            for (var x = start.x - 1; x <= start.x + 1; x++)
                            {
                                if (dungeon.coordInBounds(new Coord_t(y, x)))
                                {
                                    // values 11 and 12 are impossible here, dungeonPlaceStreamerRock
                                    // is never run before dungeonBuildTunnel
                                    if (dg.floor[y][x].feature_id == TILE_GRANITE_WALL)
                                    {
                                        dg.floor[y][x].feature_id = TMP2_WALL;
                                    }
                                }
                            }
                        }
                        break;
                    case TILE_CORR_FLOOR:
                    case TILE_BLOCKED_FLOOR:
                        start.y = tmp_row;
                        start.x = tmp_col;

                        if (!door_flag)
                        {
                            if (State.Instance.door_index < 100)
                            {
                                State.Instance.doors_tk[State.Instance.door_index].y = start.y;
                                State.Instance.doors_tk[State.Instance.door_index].x = start.x;
                                State.Instance.door_index++;
                            }
                            door_flag = true;
                        }

                        if (this.rnd.randomNumber(100) > Config.dungeon.DUN_TUNNELING)
                        {
                            // make sure that tunnel has gone a reasonable distance
                            // before stopping it, this helps prevent isolated rooms
                            tmp_row = start.y - start_row;
                            if (tmp_row < 0)
                            {
                                tmp_row = -tmp_row;
                            }

                            tmp_col = start.x - start_col;
                            if (tmp_col < 0)
                            {
                                tmp_col = -tmp_col;
                            }

                            if (tmp_row > 10 || tmp_col > 10)
                            {
                                stop_flag = true;
                            }
                        }
                        break;
                    default:
                        // none of: NULL, TMP2, GRANITE, CORR
                        start.y = tmp_row;
                        start.x = tmp_col;
                        break;
                }
            } while ((start.y != end.y || start.x != end.x) && !stop_flag);

            for (var i = 0; i < tunnel_index; i++)
            {
                dg.floor[tunnels_tk[i].y][tunnels_tk[i].x].feature_id = TILE_CORR_FLOOR;
            }

            for (var i = 0; i < wall_index; i++)
            {
                var tile = dg.floor[walls_tk[i].y][walls_tk[i].x];

                if (tile.feature_id == TMP2_WALL)
                {
                    if (this.rnd.randomNumber(100) < Config.dungeon.DUN_ROOM_DOORS)
                    {
                        dungeonPlaceDoor(new Coord_t(walls_tk[i].y, walls_tk[i].x));
                    }
                    else
                    {
                        // these have to be doorways to rooms
                        tile.feature_id = TILE_CORR_FLOOR;
                    }
                }
            }
        }

        private bool dungeonIsNextTo(Coord_t coord)
        {
            var dg = State.Instance.dg;

            if (dungeon.coordCorridorWallsNextTo(coord) > 2)
            {
                var vertical = dg.floor[coord.y - 1][coord.x].feature_id >= MIN_CAVE_WALL &&
                               dg.floor[coord.y + 1][coord.x].feature_id >= MIN_CAVE_WALL;
                var horizontal = dg.floor[coord.y][coord.x - 1].feature_id >= MIN_CAVE_WALL &&
                                 dg.floor[coord.y][coord.x + 1].feature_id >= MIN_CAVE_WALL;

                return vertical || horizontal;
            }

            return false;
        }

        // Places door at y, x position if at least 2 walls found
        private void dungeonPlaceDoorIfNextToTwoWalls(Coord_t coord)
        {
            var dg = State.Instance.dg;

            if (dg.floor[coord.y][coord.x].feature_id == TILE_CORR_FLOOR &&
                this.rnd.randomNumber(100) > Config.dungeon.DUN_TUNNEL_DOORS &&
                dungeonIsNextTo(coord))
            {
                dungeonPlaceDoor(coord);
            }
        }

        // Returns random co-ordinates -RAK-
        private void dungeonNewSpot(Coord_t coord)
        {
            var dg = State.Instance.dg;

            Tile_t tile;
            var position = new Coord_t(0, 0);

            do
            {
                position.y = this.rnd.randomNumber(dg.height - 2);
                position.x = this.rnd.randomNumber(dg.width - 2);
                tile = dg.floor[position.y][position.x];
            } while (tile.feature_id >= MIN_CLOSED_SPACE || tile.creature_id != 0 || tile.treasure_id != 0);

            coord.y = position.y;
            coord.x = position.x;
        }

        // Functions to emulate the original Pascal sets
        private bool setRooms(int tile_id)
        {
            return (tile_id == TILE_DARK_FLOOR || tile_id == TILE_LIGHT_FLOOR);
        }

        private bool setCorridors(int tile_id)
        {
            return (tile_id == TILE_CORR_FLOOR || tile_id == TILE_BLOCKED_FLOOR);
        }

        private bool setFloors(int tile_id)
        {
            return (tile_id <= MAX_CAVE_FLOOR);
        }

        // Cave logic flow for generation of new dungeon
        private void dungeonGenerate()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            // Room initialization
            var row_rooms = 2 * (dg.height / (int)SCREEN_HEIGHT);
            var col_rooms = 2 * (dg.width / (int)SCREEN_WIDTH);

            var room_map = ArrayInitializer.InitializeWithDefault(20, 20, false);

            var random_room_count = this.rnd.randomNumberNormalDistribution((int)Config.dungeon.DUN_ROOMS_MEAN, 2);
            for (var i = 0; i < random_room_count; i++)
            {
                room_map[this.rnd.randomNumber(row_rooms) - 1][this.rnd.randomNumber(col_rooms) - 1] = true;
            }

            // Build rooms
            var location_id = 0;
            var locations = ArrayInitializer.Initialize<Coord_t>(400);

            for (var row = 0; row < row_rooms; row++)
            {
                for (var col = 0; col < col_rooms; col++)
                {
                    if (room_map[row][col])
                    {
                        locations[location_id].y = (row * (int)(SCREEN_HEIGHT >> 1) + (int)QUART_HEIGHT);
                        locations[location_id].x = (col * (int)(SCREEN_WIDTH >> 1) + (int)QUART_WIDTH);
                        if (dg.current_level > this.rnd.randomNumber(Config.dungeon.DUN_UNUSUAL_ROOMS))
                        {
                            var room_type = this.rnd.randomNumber(3);

                            if (room_type == 1)
                            {
                                dungeonBuildRoomOverlappingRectangles(locations[location_id]);
                            }
                            else if (room_type == 2)
                            {
                                dungeonBuildRoomWithInnerRooms(locations[location_id]);
                            }
                            else
                            {
                                dungeonBuildRoomCrossShaped(locations[location_id]);
                            }
                        }
                        else
                        {
                            dungeonBuildRoom(locations[location_id]);
                        }
                        location_id++;
                    }
                }
            }

            for (var i = 0; i < location_id; i++)
            {
                var pick1 = this.rnd.randomNumber(location_id) - 1;
                var pick2 = this.rnd.randomNumber(location_id) - 1;

                var y = locations[pick1].y;
                var x = locations[pick1].x;
                locations[pick1].y = locations[pick2].y;
                locations[pick1].x = locations[pick2].x;
                locations[pick2].y = y;
                locations[pick2].x = x;
            }

            State.Instance.door_index = 0;

            // move zero entry to location_id, so that can call dungeonBuildTunnel all location_id times
            locations[location_id].y = locations[0].y;
            locations[location_id].x = locations[0].x;

            for (var i = 0; i < location_id; i++)
            {
                dungeonBuildTunnel(locations[i + 1], locations[i]);
            }

            // Generate walls and streamers
            dungeonFillEmptyTilesWith(TILE_GRANITE_WALL);
            for (var i = 0; i < Config.dungeon.DUN_MAGMA_STREAMER; i++)
            {
                dungeonPlaceStreamerRock(TILE_MAGMA_WALL, (int)Config.dungeon.DUN_MAGMA_TREASURE);
            }
            for (var i = 0; i < Config.dungeon.DUN_QUARTZ_STREAMER; i++)
            {
                dungeonPlaceStreamerRock(TILE_QUARTZ_WALL, (int)Config.dungeon.DUN_QUARTZ_TREASURE);
            }
            dungeonPlaceBoundaryWalls();

            // Place intersection doors
            for (var i = 0; i < State.Instance.door_index; i++)
            {
                dungeonPlaceDoorIfNextToTwoWalls(new Coord_t(State.Instance.doors_tk[i].y, State.Instance.doors_tk[i].x - 1));
                dungeonPlaceDoorIfNextToTwoWalls(new Coord_t(State.Instance.doors_tk[i].y, State.Instance.doors_tk[i].x + 1));
                dungeonPlaceDoorIfNextToTwoWalls(new Coord_t(State.Instance.doors_tk[i].y - 1, State.Instance.doors_tk[i].x));
                dungeonPlaceDoorIfNextToTwoWalls(new Coord_t(State.Instance.doors_tk[i].y + 1, State.Instance.doors_tk[i].x));
            }

            var alloc_level = (dg.current_level / 3);
            if (alloc_level < 2)
            {
                alloc_level = 2;
            }
            else if (alloc_level > 10)
            {
                alloc_level = 10;
            }

            dungeonPlaceStairs(2, this.rnd.randomNumber(2) + 2, 3);
            dungeonPlaceStairs(1, this.rnd.randomNumber(2), 3);

            // Set up the character coords, used by monsterPlaceNewWithinDistance, monsterPlaceWinning
            var coord = new Coord_t(0, 0);
            dungeonNewSpot(coord);
            //py.pos.y = coord.y;
            //py.pos.x = coord.x;
            py.pos = coord;

            this.monsterManager.monsterPlaceNewWithinDistance((this.rnd.randomNumber(8) + (int)Config.monsters.MON_MIN_PER_LEVEL + alloc_level), 0, true);
            dungeon.dungeonAllocateAndPlaceObject(setCorridors, 3, this.rnd.randomNumber(alloc_level));
            dungeon.dungeonAllocateAndPlaceObject(setRooms, 5, this.rnd.randomNumberNormalDistribution((int)Config.dungeon_objects.LEVEL_OBJECTS_PER_ROOM, 3));
            dungeon.dungeonAllocateAndPlaceObject(setFloors, 5, this.rnd.randomNumberNormalDistribution((int)Config.dungeon_objects.LEVEL_OBJECTS_PER_CORRIDOR, 3));
            dungeon.dungeonAllocateAndPlaceObject(setFloors, 4, this.rnd.randomNumberNormalDistribution((int)Config.dungeon_objects.LEVEL_TOTAL_GOLD_AND_GEMS, 3));
            dungeon.dungeonAllocateAndPlaceObject(setFloors, 1, this.rnd.randomNumber(alloc_level));

            if (dg.current_level >= Config.monsters.MON_ENDGAME_LEVEL)
            {
                this.monsterManager.monsterPlaceWinning();
            }
        }

        // Builds a store at a row, column coordinate
        private void dungeonBuildStore(int store_id, Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var yval = coord.y * 10 + 5;
            var xval = coord.x * 16 + 16;
            var height = yval - this.rnd.randomNumber(3);
            var depth = yval + this.rnd.randomNumber(4);
            var left = xval - this.rnd.randomNumber(6);
            var right = xval + this.rnd.randomNumber(6);

            int y, x;

            for (y = height; y <= depth; y++)
            {
                for (x = left; x <= right; x++)
                {
                    dg.floor[y][x].feature_id = TILE_BOUNDARY_WALL;
                }
            }

            var tmp = this.rnd.randomNumber(4);
            if (tmp < 3)
            {
                y = this.rnd.randomNumber(depth - height) + height - 1;

                if (tmp == 1)
                {
                    x = left;
                }
                else
                {
                    x = right;
                }
            }
            else
            {
                x = this.rnd.randomNumber(right - left) + left - 1;

                if (tmp == 3)
                {
                    y = depth;
                }
                else
                {
                    y = height;
                }
            }

            dg.floor[y][x].feature_id = TILE_CORR_FLOOR;

            var cur_pos = popt();
            dg.floor[y][x].treasure_id = (uint)cur_pos;

            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_STORE_DOOR + store_id, game.treasure.list[cur_pos]);
        }

        // Link all free space in treasure list together
        private void treasureLinker()
        {
            foreach (var item in State.Instance.game.treasure.list)
            {
                inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, item);
            }
            State.Instance.game.treasure.current_id = (int)Config.treasure.MIN_TREASURE_LIST_ID;
        }

        // Link all free space in monster list together
        private void monsterLinker()
        {
            var monsters = State.Instance.monsters;
            for (var i = 0; i < monsters.Length; i++)
            {
                monsters[i] = State.Instance.blank_monster;
            }
            State.Instance.next_free_monster_id = (int)Config.monsters.MON_MIN_INDEX_ID;
        }

        private void dungeonPlaceTownStores()
        {
            var rooms = new int[6];
            for (var i = 0; i < 6; i++)
            {
                rooms[i] = i;
            }

            var rooms_count = 6;

            for (var y = 0; y < 2; y++)
            {
                for (var x = 0; x < 3; x++)
                {
                    var room_id = this.rnd.randomNumber(rooms_count) - 1;
                    dungeonBuildStore(rooms[room_id], new Coord_t(y, x));

                    for (var i = room_id; i < rooms_count - 1; i++)
                    {
                        rooms[i] = rooms[i + 1];
                    }

                    rooms_count--;
                }
            }
        }

        private bool isNighTime()
        {
            var dg = State.Instance.dg;
            return (0x1 & (dg.game_turn / 5000)) != 0;
        }

        // Light town based on whether it is Night time, or day time.
        private void lightTown()
        {
            var dg = State.Instance.dg;
            if (isNighTime())
            {
                for (var y = 0; y < dg.height; y++)
                {
                    for (var x = 0; x < dg.width; x++)
                    {
                        if (dg.floor[y][x].feature_id != TILE_DARK_FLOOR)
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
        private void townGeneration()
        {
            this.rnd.seedSet(State.Instance.game.town_seed);

            dungeonPlaceTownStores();

            dungeonFillEmptyTilesWith(TILE_DARK_FLOOR);

            // make stairs before seedResetToOldSeed, so that they don't move around
            dungeonPlaceBoundaryWalls();
            dungeonPlaceStairs(2, 1, 0);

            this.rnd.seedResetToOldSeed();

            // Set up the character coords, used by monsterPlaceNewWithinDistance below
            var coord = new Coord_t(0, 0);
            dungeonNewSpot(coord);
            State.Instance.py.pos.y = coord.y;
            State.Instance.py.pos.x = coord.x;

            lightTown();

            storeInventory.storeMaintenance();
        }

        // Generates a random dungeon level -RAK-
        public void generateCave()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            dg.panel.top = 0;
            dg.panel.bottom = 0;
            dg.panel.left = 0;
            dg.panel.right = 0;

            py.pos.y = -1;
            py.pos.x = -1;

            treasureLinker();
            monsterLinker();
            dungeonBlankEntireCave();

            // We're in the dungeon more than the town, so let's default to that -MRC-
            dg.height = (int)MAX_HEIGHT;
            dg.width = (int)MAX_WIDTH;

            if (dg.current_level == 0)
            {
                dg.height = (int)SCREEN_HEIGHT;
                dg.width = (int)SCREEN_WIDTH;
            }

            dg.panel.max_rows = ((dg.height / (int)SCREEN_HEIGHT) * 2 - 2);
            dg.panel.max_cols = ((dg.width / (int)SCREEN_WIDTH) * 2 - 2);

            dg.panel.row = dg.panel.max_rows;
            dg.panel.col = dg.panel.max_cols;

            if (dg.current_level == 0)
            {
                townGeneration();
            }
            else
            {
                dungeonGenerate();
            }
        }
    }
}
