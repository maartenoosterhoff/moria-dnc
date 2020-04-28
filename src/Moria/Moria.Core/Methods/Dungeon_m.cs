using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Utils;
using System;
using Moria.Core.Data;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dungeon_los_m;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Dungeon_m
    {
        public static void SetDependencies(
            IRnd rnd,
            ITreasure treasure
        )
        {
            Dungeon_m.rnd = rnd;
            Dungeon_m.treasure = treasure;
        }

        private static IRnd rnd;
        private static ITreasure treasure;

        public static void dungeonDisplayMap()
        {
            // Save the game screen
            terminalSaveScreen();
            clearScreen();

            var priority = new int[256];
            priority[60] = 5;   // char '<'
            priority[62] = 5;   // char '>'
            priority[64] = 10;  // char '@'
            priority[35] = -5;  // char '#'
            priority[46] = -10; // char '.'
            priority[92] = -3;  // char '\'
            priority[32] = -15; // char ' '

            // Display highest priority object in the RATIO, by RATIO area
            var panel_width = (int)(MAX_WIDTH / RATIO);
            var panel_height = (int)(MAX_HEIGHT / RATIO);

            var map = ArrayInitializer.InitializeWithDefault(MAX_WIDTH / RATIO + 1, '\0');
            string line_buffer;
            //char line_buffer[80];

            // Add screen border
            addChar('+', new Coord_t(0, 0));
            addChar('+', new Coord_t(0, panel_width + 1));
            for (var i = 0; i < panel_width; i++)
            {
                addChar('-', new Coord_t(0, i + 1));
                addChar('-', new Coord_t(panel_height + 1, i + 1));
            }
            for (var i = 0; i < panel_height; i++)
            {
                addChar('|', new Coord_t(i + 1, 0));
                addChar('|', new Coord_t(i + 1, panel_width + 1));
            }
            addChar('+', new Coord_t(panel_height + 1, 0));
            addChar('+', new Coord_t(panel_height + 1, panel_width + 1));
            putString("Hit any key to continue", new Coord_t(23, 23));

            var player_y = 0;
            var player_x = 0;
            var line = -1;

            // Shrink the dungeon!
            for (var y = 0; y < MAX_HEIGHT; y++)
            {
                var row = y / (int)RATIO;
                if (row != line)
                {
                    if (line >= 0)
                    {
                        line_buffer = $"|{map}|";
                        //sprintf(line_buffer, "|%s|", map);
                        putString(line_buffer, new Coord_t(line + 1, 0));
                    }
                    for (var j = 0; j < panel_width; j++)
                    {
                        map[j] = ' ';
                    }
                    line = row;
                }

                for (var x = 0; x < MAX_WIDTH; x++)
                {
                    var col = x / (int)RATIO;
                    var cave_char = caveGetTileSymbol(new Coord_t(y, x));
                    if (priority[map[col]] < priority[cave_char])
                    {
                        map[col] = cave_char;
                    }
                    if (map[col] == '@')
                    {
                        // +1 to account for border
                        player_x = col + 1;
                        player_y = row + 1;
                    }
                }
            }

            if (line >= 0)
            {
                line_buffer = $"|{map}|";
                //sprintf(line_buffer, "|%s|", map);
                putString(line_buffer, new Coord_t(line + 1, 0));
            }

            // Move cursor onto player character
            moveCursor(new Coord_t(player_y, player_x));

            // wait for any keypress
            getKeyInput();

            // restore the game screen
            terminalRestoreScreen();
        }

        // Checks a co-ordinate for in bounds status -RAK-
        public static bool coordInBounds(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var y = coord.y > 0 && coord.y < dg.height - 1;
            var x = coord.x > 0 && coord.x < dg.width - 1;

            return y && x;
        }

        // Distance between two points -RAK-
        public static int coordDistanceBetween(Coord_t from, Coord_t to)
        {
            var dg = State.Instance.dg;

            var dy = from.y - to.y;
            if (dy < 0)
            {
                dy = -dy;
            }

            var dx = from.x - to.x;
            if (dx < 0)
            {
                dx = -dx;
            }

            var a = (dy + dx) << 1;
            var b = dy > dx ? dx : dy;

            return ((a - b) >> 1);
        }

        // Checks points north, south, east, and west for a wall -RAK-
        // note that y,x is always coordInBounds(), i.e. 0 < y < dg.height-1,
        // and 0 < x < dg.width-1
        public static int coordWallsNextTo(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var walls = 0;

            if (dg.floor[coord.y - 1][coord.x].feature_id >= MIN_CAVE_WALL)
            {
                walls++;
            }

            if (dg.floor[coord.y + 1][coord.x].feature_id >= MIN_CAVE_WALL)
            {
                walls++;
            }

            if (dg.floor[coord.y][coord.x - 1].feature_id >= MIN_CAVE_WALL)
            {
                walls++;
            }

            if (dg.floor[coord.y][coord.x + 1].feature_id >= MIN_CAVE_WALL)
            {
                walls++;
            }

            return walls;
        }

        // Checks all adjacent spots for corridors -RAK-
        // note that y, x is always coordInBounds(), hence no need to check that
        // j, k are coordInBounds(), even if they are 0 or cur_x-1 is still works
        public static int coordCorridorWallsNextTo(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var walls = 0;

            for (var y = coord.y - 1; y <= coord.y + 1; y++)
            {
                for (var x = coord.x - 1; x <= coord.x + 1; x++)
                {
                    var tile_id = (int)dg.floor[y][x].feature_id;
                    var treasure_id = (int)dg.floor[y][x].treasure_id;

                    // should fail if there is already a door present
                    if (tile_id == TILE_CORR_FLOOR && (treasure_id == 0 || game.treasure.list[treasure_id].category_id < TV_MIN_DOORS))
                    {
                        walls++;
                    }
                }
            }

            return walls;
        }

        // Returns symbol for given row, column -RAK-
        public static char caveGetTileSymbol(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            var tile = dg.floor[coord.y][coord.x];

            if (tile.creature_id == 1 && ((py.running_tracker == 0) || Config.options.run_print_self))
            {
                return '@';
            }

            if ((py.flags.status & Config.player_status.PY_BLIND) != 0u)
            {
                return ' ';
            }

            if (py.flags.image > 0 && rnd.randomNumber(12) == 1)
            {
                return (char)(rnd.randomNumber(95) + 31);
            }

            if (tile.creature_id > 1 && State.Instance.monsters[tile.creature_id].lit)
            {
                return (char)Library.Instance.Creatures.creatures_list[(int)State.Instance.monsters[tile.creature_id].creature_id].sprite;
            }

            if (!tile.permanent_light && !tile.temporary_light && !tile.field_mark)
            {
                return ' ';
            }

            if (tile.treasure_id != 0 && game.treasure.list[tile.treasure_id].category_id != TV_INVIS_TRAP)
            {
                return (char)game.treasure.list[tile.treasure_id].sprite;
            }

            if (tile.feature_id <= MAX_CAVE_FLOOR)
            {
                return '.';
            }

            if (tile.feature_id == TILE_GRANITE_WALL || tile.feature_id == TILE_BOUNDARY_WALL || !Config.options.highlight_seams)
            {
                return '#';
            }

            // Originally set highlight bit, but that is not portable,
            // now use the percent sign instead.
            return '%';
        }

        // Tests a spot for light or field mark status -RAK-
        public static bool caveTileVisible(Coord_t coord)
        {
            var dg = State.Instance.dg;
            return dg.floor[coord.y][coord.x].permanent_light ||
                   dg.floor[coord.y][coord.x].temporary_light ||
                   dg.floor[coord.y][coord.x].field_mark;
        }

        // Places a particular trap at location y, x -RAK-
        public static void dungeonSetTrap(Coord_t coord, int sub_type_id)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var free_treasure_id = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)free_treasure_id;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_TRAP_LIST + sub_type_id, game.treasure.list[free_treasure_id]);
        }

        // Change a trap from invisible to visible -RAK-
        // Note: Secret doors are handled here
        public static void trapChangeVisibility(Coord_t coord)
        {
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            var treasure_id = dg.floor[coord.y][coord.x].treasure_id;

            var item = game.treasure.list[treasure_id];

            if (item.category_id == TV_INVIS_TRAP)
            {
                item.category_id = TV_VIS_TRAP;
                dungeonLiteSpot(coord);
                return;
            }

            // change secret door to closed door
            if (item.category_id == TV_SECRET_DOOR)
            {
                item.id = Config.dungeon_objects.OBJ_CLOSED_DOOR;
                item.category_id = Library.Instance.Treasure.game_objects[(int)Config.dungeon_objects.OBJ_CLOSED_DOOR].category_id;
                item.sprite = Library.Instance.Treasure.game_objects[(int)Config.dungeon_objects.OBJ_CLOSED_DOOR].sprite;
                dungeonLiteSpot(coord);
            }
        }

        // Places rubble at location y, x -RAK-
        public static void dungeonPlaceRubble(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var free_treasure_id = popt();
            dg.floor[coord.y][coord.x].treasure_id = (uint)free_treasure_id;
            dg.floor[coord.y][coord.x].feature_id = TILE_BLOCKED_FLOOR;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_RUBBLE, game.treasure.list[free_treasure_id]);
        }

        // Places a treasure (Gold or Gems) at given row, column -RAK-
        public static void dungeonPlaceGold(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var free_treasure_id = popt();

            var gold_type_id = ((rnd.randomNumber(dg.current_level + 2) + 2) / 2) - 1;

            if (rnd.randomNumber(Config.treasure.TREASURE_CHANCE_OF_GREAT_ITEM) == 1)
            {
                gold_type_id += rnd.randomNumber(dg.current_level + 1);
            }

            if (gold_type_id >= Config.dungeon_objects.MAX_GOLD_TYPES)
            {
                gold_type_id = (int)Config.dungeon_objects.MAX_GOLD_TYPES - 1;
            }

            dg.floor[coord.y][coord.x].treasure_id = (uint)free_treasure_id;
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_GOLD_LIST + gold_type_id, game.treasure.list[free_treasure_id]);
            game.treasure.list[free_treasure_id].cost += (8 * rnd.randomNumber(game.treasure.list[free_treasure_id].cost)) + rnd.randomNumber(8);

            if (dg.floor[coord.y][coord.x].creature_id == 1)
            {
                printMessage("You feel something roll beneath your feet.");
            }
        }

        // Places an object at given row, column co-ordinate -RAK-
        public static void dungeonPlaceRandomObjectAt(Coord_t coord, bool must_be_small)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var free_treasure_id = popt();

            dg.floor[coord.y][coord.x].treasure_id = (uint)free_treasure_id;

            var object_id = itemGetRandomObjectId(dg.current_level, must_be_small);
            inventoryItemCopyTo(State.Instance.sorted_objects[object_id], State.Instance.game.treasure.list[free_treasure_id]);

            treasure.magicTreasureMagicalAbility(free_treasure_id, dg.current_level);

            if (dg.floor[coord.y][coord.x].creature_id == 1)
            {
                printMessage("You feel something roll beneath your feet."); // -CJS-
            }
        }

        // Allocates an object for tunnels and rooms -RAK-
        public static void dungeonAllocateAndPlaceObject(Func<int, bool> set_function, int object_type, int number)
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
                    coord.y = rnd.randomNumber(dg.height) - 1;
                    coord.x = rnd.randomNumber(dg.width) - 1;
                } while (!set_function((int)dg.floor[coord.y][coord.x].feature_id) ||
                         dg.floor[coord.y][coord.x].treasure_id != 0 ||
                         (coord.y == py.pos.y && coord.x == py.pos.x));

                switch (object_type)
                {
                    case 1:
                        dungeonSetTrap(coord, rnd.randomNumber(Config.dungeon_objects.MAX_TRAPS) - 1);
                        break;
                    case 2:
                    // NOTE: object_type == 2 is no longer used - it used to be visible traps.
                    // FIXME: there was no `break` here so `case 3` catches it? -MRC-
                    case 3:
                        dungeonPlaceRubble(coord);
                        break;
                    case 4:
                        dungeonPlaceGold(coord);
                        break;
                    case 5:
                        dungeonPlaceRandomObjectAt(coord, false);
                        break;
                    default:
                        break;
                }
            }
        }

        // Creates objects nearby the coordinates given -RAK-
        public static void dungeonPlaceRandomObjectNear(Coord_t coord, int tries)
        {
            var dg = State.Instance.dg;
            do
            {
                for (var i = 0; i <= 10; i++)
                {
                    var at = new Coord_t(coord.y - 3 + rnd.randomNumber(5), coord.x - 4 + rnd.randomNumber(7));

                    if (coordInBounds(at) &&
                        dg.floor[at.y][at.x].feature_id <= MAX_CAVE_FLOOR &&
                        dg.floor[at.y][at.x].treasure_id == 0)
                    {
                        if (rnd.randomNumber(100) < 75)
                        {
                            dungeonPlaceRandomObjectAt(at, false);
                        }
                        else
                        {
                            dungeonPlaceGold(at);
                        }
                        i = 9;
                    }
                }

                tries--;
            } while (tries != 0);
        }

        // Moves creature record from one space to another -RAK-
        // this always works correctly, even if y1==y2 and x1==x2
        public static void dungeonMoveCreatureRecord(Coord_t from, Coord_t to)
        {
            var dg = State.Instance.dg;

            var id = dg.floor[from.y][from.x].creature_id;
            dg.floor[from.y][from.x].creature_id = 0;
            dg.floor[to.y][to.x].creature_id = id;
        }

        // Room is lit, make it appear -RAK-
        public static void dungeonLightRoom(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var height_middle = ((int)SCREEN_HEIGHT / 2);
            var width_middle = ((int)SCREEN_WIDTH / 2);

            var top = (coord.y / height_middle) * height_middle;
            var left = (coord.x / width_middle) * width_middle;
            var bottom = top + height_middle - 1;
            var right = left + width_middle - 1;

            var location = new Coord_t(0, 0);

            for (location.y = top; location.y <= bottom; location.y++)
            {
                for (location.x = left; location.x <= right; location.x++)
                {
                    var tile = dg.floor[location.y][location.x];

                    if (tile.perma_lit_room && !tile.permanent_light)
                    {
                        tile.permanent_light = true;

                        if (tile.feature_id == TILE_DARK_FLOOR)
                        {
                            tile.feature_id = TILE_LIGHT_FLOOR;
                        }
                        if (!tile.field_mark && tile.treasure_id != 0)
                        {
                            var treasure_id = (int)game.treasure.list[tile.treasure_id].category_id;
                            if (treasure_id >= TV_MIN_VISIBLE && treasure_id <= TV_MAX_VISIBLE)
                            {
                                tile.field_mark = true;
                            }
                        }
                        panelPutTile(caveGetTileSymbol(location), location);
                    }
                }
            }
        }

        // Lights up given location -RAK-
        public static void dungeonLiteSpot(Coord_t coord)
        {
            if (!coordInsidePanel(coord))
            {
                return;
            }

            var symbol = caveGetTileSymbol(coord);
            panelPutTile(symbol, coord);
        }

        // Normal movement
        // When FIND_FLAG,  light only permanent features
        public static void sub1MoveLight(Coord_t from, Coord_t to)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            if (py.temporary_light_only)
            {
                // Turn off lamp light
                for (var y = from.y - 1; y <= from.y + 1; y++)
                {
                    for (var x = from.x - 1; x <= from.x + 1; x++)
                    {
                        dg.floor[y][x].temporary_light = false;
                    }
                }
                if ((py.running_tracker != 0) && !Config.options.run_print_self)
                {
                    py.temporary_light_only = false;
                }
            }
            else if ((py.running_tracker == 0) || Config.options.run_print_self)
            {
                py.temporary_light_only = true;
            }

            for (var y = to.y - 1; y <= to.y + 1; y++)
            {
                for (var x = to.x - 1; x <= to.x + 1; x++)
                {
                    var tile = dg.floor[y][x];

                    // only light up if normal movement
                    if (py.temporary_light_only)
                    {
                        tile.temporary_light = true;
                    }

                    if (tile.feature_id >= MIN_CAVE_WALL)
                    {
                        tile.permanent_light = true;
                    }
                    else if (!tile.field_mark && tile.treasure_id != 0)
                    {
                        var tval = (int)game.treasure.list[tile.treasure_id].category_id;

                        if (tval >= TV_MIN_VISIBLE && tval <= TV_MAX_VISIBLE)
                        {
                            tile.field_mark = true;
                        }
                    }
                }
            }

            // From uppermost to bottom most lines player was on.
            int top, left, bottom, right;

            if (from.y < to.y)
            {
                top = from.y - 1;
                bottom = to.y + 1;
            }
            else
            {
                top = to.y - 1;
                bottom = from.y + 1;
            }
            if (from.x < to.x)
            {
                left = from.x - 1;
                right = to.x + 1;
            }
            else
            {
                left = to.x - 1;
                right = from.x + 1;
            }

            var coord = new Coord_t(0, 0);
            for (coord.y = top; coord.y <= bottom; coord.y++)
            {
                // Leftmost to rightmost do
                for (coord.x = left; coord.x <= right; coord.x++)
                {
                    panelPutTile(caveGetTileSymbol(coord), coord);
                }
            }
        }

        // When blinded,  move only the player symbol.
        // With no light,  movement becomes involved.
        public static void sub3MoveLight(Coord_t from, Coord_t to)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            if (py.temporary_light_only)
            {
                var coord = new Coord_t(0, 0);

                for (coord.y = from.y - 1; coord.y <= from.y + 1; coord.y++)
                {
                    for (coord.x = from.x - 1; coord.x <= from.x + 1; coord.x++)
                    {
                        dg.floor[coord.y][coord.x].temporary_light = false;
                        panelPutTile(caveGetTileSymbol(coord), coord);
                    }
                }

                py.temporary_light_only = false;
            }
            else if ((py.running_tracker == 0) || Config.options.run_print_self)
            {
                panelPutTile(caveGetTileSymbol(from), from);
            }

            if ((py.running_tracker == 0) || Config.options.run_print_self)
            {
                panelPutTile('@', to);
            }
        }

        // Package for moving the character's light about the screen
        // Four cases : Normal, Finding, Blind, and No light -RAK-
        public static void dungeonMoveCharacterLight(Coord_t from, Coord_t to)
        {
            var py = State.Instance.py;

            if (py.flags.blind > 0 || !py.carrying_light)
            {
                sub3MoveLight(from, to);
            }
            else
            {
                sub1MoveLight(from, to);
            }
        }

        // Deletes a monster entry from the level -RAK-
        public static void dungeonDeleteMonster(int id)
        {
            var dg = State.Instance.dg;
            var monsters = State.Instance.monsters;

            var monster = monsters[id];

            dg.floor[monster.pos.y][monster.pos.x].creature_id = 0;

            if (monster.lit)
            {
                dungeonLiteSpot(new Coord_t(monster.pos.y, monster.pos.x));
            }

            var last_id = State.Instance.next_free_monster_id - 1;

            if (id != last_id)
            {
                monster = monsters[last_id];
                dg.floor[monster.pos.y][monster.pos.x].creature_id = (uint)id;
                monsters[id] = monsters[last_id];
            }

            State.Instance.next_free_monster_id--;
            monsters[State.Instance.next_free_monster_id] = State.Instance.blank_monster;

            if (State.Instance.monster_multiply_total > 0)
            {
                State.Instance.monster_multiply_total--;
            }
        }

        // The following two procedures implement the same function as delete monster.
        // However, they are used within updateMonsters(), because deleting a monster
        // while scanning the monsters causes two problems, monsters might get two
        // turns, and m_ptr/monptr might be invalid after the dungeonDeleteMonster.
        // Hence the delete is done in two steps.
        //
        // dungeonDeleteMonsterFix1 does everything dungeonDeleteMonster does except delete
        // the monster record and reduce next_free_monster_id, this is called in breathe, and
        // a couple of places in creatures.c
        public static void dungeonDeleteMonsterFix1(int id)
        {
            var dg = State.Instance.dg;
            var monsters = State.Instance.monsters;

            var monster = monsters[id];

            // force the hp negative to ensure that the monster is dead, for example,
            // if the monster was just eaten by another, it will still have positive
            // hit points
            monster.hp = -1;

            dg.floor[monster.pos.y][monster.pos.x].creature_id = 0;

            if (monster.lit)
            {
                dungeonLiteSpot(new Coord_t(monster.pos.y, monster.pos.x));
            }

            if (State.Instance.monster_multiply_total > 0)
            {
                State.Instance.monster_multiply_total--;
            }
        }

        // dungeonDeleteMonsterFix2 does everything in dungeonDeleteMonster that wasn't done
        // by fix1_monster_delete above, this is only called in updateMonsters()
        public static void dungeonDeleteMonsterFix2(int id)
        {
            var dg = State.Instance.dg;
            var monsters = State.Instance.monsters;

            var last_id = State.Instance.next_free_monster_id - 1;

            if (id != last_id)
            {
                var y = monsters[last_id].pos.y;
                var x = monsters[last_id].pos.x;
                dg.floor[y][x].creature_id = (uint)id;

                monsters[id] = monsters[last_id];
            }

            monsters[last_id] = State.Instance.blank_monster;
            State.Instance.next_free_monster_id--;
        }

        // Creates objects nearby the coordinates given -RAK-
        public static int dungeonSummonObject(Coord_t coord, int amount, int object_type)
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
                        coord.y - 3 + rnd.randomNumber(5),
                        coord.x - 3 + rnd.randomNumber(5));

                    if (coordInBounds(at) && los(coord, at))
                    {
                        if (dg.floor[at.y][at.x].feature_id <= MAX_OPEN_SPACE && dg.floor[at.y][at.x].treasure_id == 0)
                        {
                            // object_type == 3 -> 50% objects, 50% gold
                            if (object_type == 3 || object_type == 7)
                            {
                                if (rnd.randomNumber(100) < 50)
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
                                dungeonPlaceRandomObjectAt(at, (object_type >= 4));
                            }
                            else
                            {
                                dungeonPlaceGold(at);
                            }

                            dungeonLiteSpot(at);

                            if (caveTileVisible(at))
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

        // Deletes object from given location -RAK-
        public static bool dungeonDeleteObject(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var tile = dg.floor[coord.y][coord.x];

            if (tile.feature_id == TILE_BLOCKED_FLOOR)
            {
                tile.feature_id = TILE_CORR_FLOOR;
            }

            pusht(tile.treasure_id);

            tile.treasure_id = 0;
            tile.field_mark = false;

            dungeonLiteSpot(coord);

            return caveTileVisible(coord);
        }
    }
}
