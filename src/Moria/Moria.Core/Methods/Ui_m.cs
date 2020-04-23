using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Mage_spells_m;
using static Moria.Core.Methods.Game_files_m;
using static Moria.Core.Methods.Player_run_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Player_stats_m;

namespace Moria.Core.Methods
{
    public static class Ui_m
    {
        public static string[] stat_names = new string[] {
            "STR : ", "INT : ", "WIS : ", "DEX : ", "CON : ", "CHR : "
        };

        public static readonly int BLANK_LENGTH = 24;
        //#define BLANK_LENGTH 24

        public static string blank_string = "                        ";
        public const char ESCAPE = '\u001B';

        public static char CTRL_KEY(char x) => (char) (x & 0x1F);

        // Calculates current boundaries -RAK-
        public static void panelBounds()
        {
            var dg = State.Instance.dg;
            dg.panel.top = dg.panel.row * ((int)SCREEN_HEIGHT / 2);
            dg.panel.bottom = dg.panel.top + (int)SCREEN_HEIGHT - 1;
            dg.panel.row_prt = dg.panel.top - 1;
            dg.panel.left = dg.panel.col * ((int)SCREEN_WIDTH / 2);
            dg.panel.right = dg.panel.left + (int)SCREEN_WIDTH - 1;
            dg.panel.col_prt = dg.panel.left - 13;
        }

        // Given an row (y) and col (x), this routine detects -RAK-
        // when a move off the screen has occurred and figures new borders.
        // `force` forces the panel bounds to be recalculated, useful for 'W'here.
        public static bool coordOutsidePanel(Coord_t coord, bool force)
        {
            var dg = State.Instance.dg;
            Coord_t panel = new Coord_t(dg.panel.row, dg.panel.col);

            if (force || coord.y < dg.panel.top + 2 || coord.y > dg.panel.bottom - 2)
            {
                panel.y = (coord.y - (int)SCREEN_HEIGHT / 4) / ((int)SCREEN_HEIGHT / 2);

                if (panel.y > dg.panel.max_rows)
                {
                    panel.y = dg.panel.max_rows;
                }
                else if (panel.y < 0)
                {
                    panel.y = 0;
                }
            }

            if (force || coord.x < dg.panel.left + 3 || coord.x > dg.panel.right - 3)
            {
                panel.x = ((coord.x - (int)SCREEN_WIDTH / 4) / ((int)SCREEN_WIDTH / 2));
                if (panel.x > dg.panel.max_cols)
                {
                    panel.x = dg.panel.max_cols;
                }
                else if (panel.x < 0)
                {
                    panel.x = 0;
                }
            }

            if (panel.y != dg.panel.row || panel.x != dg.panel.col)
            {
                dg.panel.row = panel.y;
                dg.panel.col = panel.x;
                panelBounds();

                // stop movement if any
                if (Config.options.find_bound)
                {
                    playerEndRunning();
                }

                // Yes, the coordinates are beyond the current panel boundary
                return true;
            }

            return false;
        }

        // Is the given coordinate within the screen panel boundaries -RAK-
        public static bool coordInsidePanel(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var valid_y = coord.y >= dg.panel.top && coord.y <= dg.panel.bottom;
            var valid_x = coord.x >= dg.panel.left && coord.x <= dg.panel.right;

            return valid_y && valid_x;
        }

        // Prints the map of the dungeon -RAK-
        public static void drawDungeonPanel()
        {
            var dg = State.Instance.dg;

            int line = 1;

            Coord_t coord = new Coord_t(0, 0);

            // Top to bottom
            for (coord.y = dg.panel.top; coord.y <= dg.panel.bottom; coord.y++)
            {
                eraseLine(new Coord_t(line, 13));
                line++;

                // Left to right
                for (coord.x = dg.panel.left; coord.x <= dg.panel.right; coord.x++)
                {
                    char ch = caveGetTileSymbol(coord);
                    if (ch != ' ')
                    {
                        panelPutTile(ch, coord);
                    }
                }
            }
        }

        // Draws entire screen -RAK-
        public static void drawCavePanel()
        {
            clearScreen();
            printCharacterStatsBlock();
            drawDungeonPanel();
            printCharacterCurrentDepth();
        }

        // We need to reset the view of things. -CJS-
        public static void dungeonResetView()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            var tile = dg.floor[py.pos.y][py.pos.x];

            // Check for new panel
            if (coordOutsidePanel(py.pos, false))
            {
                drawDungeonPanel();
            }

            // Move the light source
            dungeonMoveCharacterLight(py.pos, py.pos);

            // A room of light should be lit.
            if (tile.feature_id == TILE_LIGHT_FLOOR)
            {
                if (py.flags.blind < 1 && !tile.permanent_light)
                {
                    dungeonLightRoom(py.pos);
                }
                return;
            }

            // In doorway of light-room?
            if (tile.perma_lit_room && py.flags.blind < 1)
            {
                for (int i = py.pos.y - 1; i <= py.pos.y + 1; i++)
                {
                    for (int j = py.pos.x - 1; j <= py.pos.x + 1; j++)
                    {
                        if (dg.floor[i][j].feature_id == TILE_LIGHT_FLOOR && !dg.floor[i][j].permanent_light)
                        {
                            dungeonLightRoom(new Coord_t(i, j));
                        }
                    }
                }
            }
        }

        // Converts stat num into string
        public static void statsAsString(uint stat, ref string stat_string)
        {
            int percentile = (int)stat - 18;

            if (stat <= 18)
            {
                stat_string = $"{stat:d6}";
                //(void)sprintf(stat_string, "%6d", stat);
            }
            else if (percentile == 100)
            {
                stat_string = "18/100";
                //(void)strcpy(stat_string, "18/100");
            }
            else
            {
                stat_string = $" 18/{percentile:d2}";
                //(void)sprintf(stat_string, " 18/%02d", percentile);
            }
        }

        // Print character stat in given row, column -RAK-
        public static void displayCharacterStats(int stat)
        {
            var py = State.Instance.py;
            var text = string.Empty;
            statsAsString(py.stats.used[stat], ref text);
            putString(stat_names[stat], new Coord_t(6 + stat, (int)STAT_COLUMN));
            putString(text, new Coord_t(6 + stat, (int)STAT_COLUMN + 6));
        }

        // Print character info in given row, column -RAK-
        // The longest title is 13 characters, so only pad to 13
        public static void printCharacterInfoInField(string info, Coord_t coord)
        {
            // blank out the current field space
            putString(blank_string.Substring(0, BLANK_LENGTH - 13), coord);
            //putString(&blank_string[BLANK_LENGTH - 13], coord);

            putString(info, coord);
        }

        // Print long number with header at given row, column
        public static void printHeaderLongNumber(string header, int num, Coord_t coord)
        {
            var str = $"{header}: {num:d6}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%s: %6d", header, num);
            putString(str, coord);
        }

        // Print long number (7 digits of space) with header at given row, column
        public static void printHeaderLongNumber7Spaces(string header, int num, Coord_t coord)
        {
            var str = $"{header}: {num:d7}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%s: %7d", header, num);
            putString(str, coord);
        }

        // Print number with header at given row, column -RAK-
        public static void printHeaderNumber(string header, int num, Coord_t coord)
        {
            var str = $"{header}: {num:d6}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%s: %6d", header, num);
            putString(str, coord);
        }

        // Print long number at given row, column
        public static void printLongNumber(int num, Coord_t coord)
        {
            var str = $"{num:d6}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%6d", num);
            putString(str, coord);
        }

        // Print number at given row, column -RAK-
        public static void printNumber(int num, Coord_t coord)
        {
            var str = $"{num:d6}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%6d", num);
            putString(str, coord);
        }

        // Prints title of character -RAK-
        public static void printCharacterTitle()
        {
            printCharacterInfoInField(playerRankTitle(), new Coord_t(4, (int)STAT_COLUMN));
        }

        // Prints level -RAK-
        public static void printCharacterLevel()
        {
            var py = State.Instance.py;
            printNumber((int)py.misc.level, new Coord_t(13, (int)STAT_COLUMN + 6));
        }

        // Prints players current mana points. -RAK-
        public static void printCharacterCurrentMana()
        {
            var py = State.Instance.py;
            printNumber(py.misc.current_mana, new Coord_t(15, (int)STAT_COLUMN + 6));
        }

        // Prints Max hit points -RAK-
        public static void printCharacterMaxHitPoints()
        {
            var py = State.Instance.py;
            printNumber(py.misc.max_hp, new Coord_t(16, (int)STAT_COLUMN + 6));
        }

        // Prints players current hit points -RAK-
        public static void printCharacterCurrentHitPoints()
        {
            var py = State.Instance.py;
            printNumber(py.misc.current_hp, new Coord_t(17, (int)STAT_COLUMN + 6));
        }

        // prints current AC -RAK-
        public static void printCharacterCurrentArmorClass()
        {
            var py = State.Instance.py;
            printNumber(py.misc.display_ac, new Coord_t(19, (int)STAT_COLUMN + 6));
        }

        // Prints current gold -RAK-
        public static void printCharacterGoldValue()
        {
            var py = State.Instance.py;
            printLongNumber(py.misc.au, new Coord_t(20, (int)STAT_COLUMN + 6));
        }

        // Prints depth in stat area -RAK-
        public static void printCharacterCurrentDepth()
        {
            var dg = State.Instance.dg;
            string depths;
            //vtype_t depths = { '\0' };

            int depth = dg.current_level * 50;

            if (depth == 0)
            {
                depths = "Town level";
                //(void)strcpy(depths, "Town level");
            }
            else
            {
                depths = $"{depth} feet";
                //(void)sprintf(depths, "%d feet", depth);
            }

            putStringClearToEOL(depths, new Coord_t(23, 65));
        }

        // Prints status of hunger -RAK-
        public static void printCharacterHungerStatus()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_WEAK) != 0u)
            {
                putString("Weak  ", new Coord_t(23, 0));
            }
            else if ((py.flags.status & Config.player_status.PY_HUNGRY) != 0u)
            {
                putString("Hungry", new Coord_t(23, 0));
            }
            else
            {
                putString(blank_string.Substring(0, BLANK_LENGTH - 6), new Coord_t(23, 0));
                //putString(&blank_string[BLANK_LENGTH - 6], new Coord_t(23, 0));
            }
        }

        // Prints Blind status -RAK-
        public static void printCharacterBlindStatus()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_BLIND) != 0u)
            {
                putString("Blind", new Coord_t(23, 7));
            }
            else
            {
                putString(blank_string.Substring(0, BLANK_LENGTH - 5), new Coord_t(23, 7));
                //putString(&blank_string[BLANK_LENGTH - 5], new Coord_t(23, 7));
            }
        }

        // Prints Confusion status -RAK-
        public static void printCharacterConfusedState()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_CONFUSED) != 0u)
            {
                putString("Confused", new Coord_t(23, 13));
            }
            else
            {
                putString(blank_string.Substring(BLANK_LENGTH - 8), new Coord_t(23, 13));
                //putString(&blank_string[BLANK_LENGTH - 8], new Coord_t(23, 13));
            }
        }

        // Prints Fear status -RAK-
        public static void printCharacterFearState()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_FEAR) != 0u)
            {
                putString("Afraid", new Coord_t(23, 22));
            }
            else
            {
                //putString(&blank_string[BLANK_LENGTH - 6], new Coord_t(23, 22));
                putString(blank_string.Substring(BLANK_LENGTH - 6), new Coord_t(23, 22));
            }
        }

        // Prints Poisoned status -RAK-
        public static void printCharacterPoisonedState()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_POISONED) != 0u)
            {
                putString("Poisoned", new Coord_t(23, 29));
            }
            else
            {
                putString(blank_string.Substring(0, BLANK_LENGTH - 8), new Coord_t(23, 29));
                //putString(&blank_string[BLANK_LENGTH - 8], new Coord_t(23, 29));
            }
        }

        // Prints Searching, Resting, Paralysis, or 'count' status -RAK-
        public static void printCharacterMovementState()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            py.flags.status &= ~Config.player_status.PY_REPEAT;

            if (py.flags.paralysis > 1)
            {
                putString("Paralysed", new Coord_t(23, 38));
                return;
            }

            if ((py.flags.status & Config.player_status.PY_REST) != 0u)
            {
                string rest_string;
                //char rest_string[16];

                if (py.flags.rest < 0)
                {
                    rest_string = "Rest *";
                    //(void)strcpy(rest_string, "Rest *");
                }
                else if (Config.options.display_counts)
                {
                    rest_string = $"Rest {py.flags.rest:d-5}";
                    //(void)sprintf(rest_string, "Rest %-5d", py.flags.rest);
                }
                else
                {
                    rest_string = "Rest";
                    //(void)strcpy(rest_string, "Rest");
                }

                putString(rest_string, new Coord_t(23, 38));

                return;
            }

            if (game.command_count > 0)
            {
                string repeat_string;
                //char repeat_string[16];

                if (Config.options.display_counts)
                {
                    repeat_string = $"Repeat {game.command_count:d-3}";
                    //(void)sprintf(repeat_string, "Repeat %-3d", game.command_count);
                }
                else
                {
                    repeat_string = "Repeat";
                    //(void)strcpy(repeat_string, "Repeat");
                }

                py.flags.status |= Config.player_status.PY_REPEAT;

                putString(repeat_string, new Coord_t(23, 38));

                if ((py.flags.status & Config.player_status.PY_SEARCH) != 0u)
                {
                    putString("Search", new Coord_t(23, 38));
                }

                return;
            }

            if ((py.flags.status & Config.player_status.PY_SEARCH) != 0u)
            {
                putString("Searching", new Coord_t(23, 38));
                return;
            }

            // "repeat 999" is 10 characters
            putString(blank_string.Substring(0, BLANK_LENGTH - 10), new Coord_t(23, 38));
            //putString(&blank_string[BLANK_LENGTH - 10], new Coord_t(23, 38});
        }

        // Prints the speed of a character. -CJS-
        public static void printCharacterSpeed()
        {
            var py = State.Instance.py;
            int speed = py.flags.speed;

            // Search mode.
            if ((py.flags.status & Config.player_status.PY_SEARCH) != 0u)
            {
                speed--;
            }

            if (speed > 1)
            {
                putString("Very Slow", new Coord_t(23, 49));
            }
            else if (speed == 1)
            {
                putString("Slow     ", new Coord_t(23, 49));
            }
            else if (speed == 0)
            {
                putString(blank_string.Substring(0, BLANK_LENGTH - 9), new Coord_t(23, 49));
                //putString(&blank_string[BLANK_LENGTH - 9], new Coord_t(23, 49));
            }
            else if (speed == -1)
            {
                putString("Fast     ", new Coord_t(23, 49));
            }
            else
            {
                putString("Very Fast", new Coord_t(23, 49));
            }
        }

        public static void printCharacterStudyInstruction()
        {
            var py = State.Instance.py;

            py.flags.status &= ~Config.player_status.PY_STUDY;

            if (py.flags.new_spells_to_learn == 0)
            {
                putString(blank_string.Substring(BLANK_LENGTH - 5), new Coord_t(23, 59));
                //putString(&blank_string[BLANK_LENGTH - 5], new Coord_t(23, 59));
            }
            else
            {
                putString("Study", new Coord_t(23, 59));
            }
        }

        // Prints winner status on display -RAK-
        public static void printCharacterWinner()
        {
            var game = State.Instance.game;
            if ((game.noscore & 0x2) != 0)
            {
                if (game.wizard_mode)
                {
                    putString("Is wizard  ", new Coord_t(22, 0));
                }
                else
                {
                    putString("Was wizard ", new Coord_t(22, 0));
                }
            }
            else if ((game.noscore & 0x1) != 0)
            {
                putString("Resurrected", new Coord_t(22, 0));
            }
            else if ((game.noscore & 0x4) != 0)
            {
                putString("Duplicate", new Coord_t(22, 0));
            }
            else if (game.total_winner)
            {
                putString("*Winner*   ", new Coord_t(22, 0));
            }
        }

        // Prints character-screen info -RAK-
        public static void printCharacterStatsBlock()
        {
            var py = State.Instance.py;

            printCharacterInfoInField(State.Instance.character_races[py.misc.race_id].name, new Coord_t(2, (int)STAT_COLUMN));
            printCharacterInfoInField(State.Instance.classes[py.misc.class_id].title, new Coord_t(3, (int)STAT_COLUMN));
            printCharacterInfoInField(playerRankTitle(), new Coord_t(4, (int)STAT_COLUMN));

            for (int i = 0; i < 6; i++)
            {
                displayCharacterStats(i);
            }

            printHeaderNumber("LEV ", (int)py.misc.level, new Coord_t(13, (int)STAT_COLUMN));
            printHeaderLongNumber("EXP ", py.misc.exp, new Coord_t(14, (int)STAT_COLUMN));
            printHeaderNumber("MANA", py.misc.current_mana, new Coord_t(15, (int)STAT_COLUMN));
            printHeaderNumber("MHP ", py.misc.max_hp, new Coord_t(16, (int)STAT_COLUMN));
            printHeaderNumber("CHP ", py.misc.current_hp, new Coord_t(17, (int)STAT_COLUMN));
            printHeaderNumber("AC  ", py.misc.display_ac, new Coord_t(19, (int)STAT_COLUMN));
            printHeaderLongNumber("GOLD", py.misc.au, new Coord_t(20, (int)STAT_COLUMN));
            printCharacterWinner();

            uint status = py.flags.status;

            if (((Config.player_status.PY_HUNGRY | Config.player_status.PY_WEAK) & status) != 0u)
            {
                printCharacterHungerStatus();
            }

            if ((status & Config.player_status.PY_BLIND) != 0u)
            {
                printCharacterBlindStatus();
            }

            if ((status & Config.player_status.PY_CONFUSED) != 0u)
            {
                printCharacterConfusedState();
            }

            if ((status & Config.player_status.PY_FEAR) != 0u)
            {
                printCharacterFearState();
            }

            if ((status & Config.player_status.PY_POISONED) != 0u)
            {
                printCharacterPoisonedState();
            }

            if (((Config.player_status.PY_SEARCH | Config.player_status.PY_REST) & status) != 0u)
            {
                printCharacterMovementState();
            }

            // if speed non zero, print it, modify speed if Searching
            int speed = py.flags.speed - (int)((status & Config.player_status.PY_SEARCH) >> 8);
            if (speed != 0)
            {
                printCharacterSpeed();
            }

            // display the study field
            printCharacterStudyInstruction();
        }

        // Prints the following information on the screen. -JWT-
        public static void printCharacterInformation()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            clearScreen();

            putString("Name        :", new Coord_t(2, 1));
            putString("Race        :", new Coord_t(3, 1));
            putString("Sex         :", new Coord_t(4, 1));
            putString("Class       :", new Coord_t(5, 1));

            if (!game.character_generated)
            {
                return;
            }

            putString(py.misc.name, new Coord_t(2, 15));
            putString(State.Instance.character_races[py.misc.race_id].name, new Coord_t(3, 15));
            putString((playerGetGenderLabel()), new Coord_t(4, 15));
            putString(State.Instance.classes[py.misc.class_id].title, new Coord_t(5, 15));
        }

        // Prints the following information on the screen. -JWT-
        public static void printCharacterStats()
        {
            var py = State.Instance.py;
            for (int i = 0; i < 6; i++)
            {
                var buf = string.Empty;

                statsAsString(py.stats.used[i], ref buf);
                putString(stat_names[i], new Coord_t(2 + i, 61));
                putString(buf, new Coord_t(2 + i, 66));

                if (py.stats.max[i] > py.stats.current[i])
                {
                    statsAsString(py.stats.max[i], ref buf);
                    putString(buf, new Coord_t(2 + i, 73));
                }
            }

            printHeaderNumber("+ To Hit    ", py.misc.display_to_hit, new Coord_t(9, 1));
            printHeaderNumber("+ To Damage ", py.misc.display_to_damage, new Coord_t(10, 1));
            printHeaderNumber("+ To AC     ", py.misc.display_to_ac, new Coord_t(11, 1));
            printHeaderNumber("  Total AC  ", py.misc.display_ac, new Coord_t(12, 1));
        }

        // Returns a rating of x depending on y -JWT-
        public static string statRating(Coord_t coord)
        {
            switch (coord.x / coord.y)
            {
                case -3:
                case -2:
                case -1:
                    return "Very Bad";
                case 0:
                case 1:
                    return "Bad";
                case 2:
                    return "Poor";
                case 3:
                case 4:
                    return "Fair";
                case 5:
                    return "Good";
                case 6:
                    return "Very Good";
                case 7:
                case 8:
                    return "Excellent";
                default:
                    return "Superb";
            }
        }

        // Prints age, height, weight, and SC -JWT-
        public static void printCharacterVitalStatistics()
        {
            var py = State.Instance.py;
            printHeaderNumber("Age          ", (int)py.misc.age, new Coord_t(2, 38));
            printHeaderNumber("Height       ", (int)py.misc.height, new Coord_t(3, 38));
            printHeaderNumber("Weight       ", (int)py.misc.weight, new Coord_t(4, 38));
            printHeaderNumber("Social Class ", (int)py.misc.social_class, new Coord_t(5, 38));
        }

        // Prints the following information on the screen. -JWT-
        public static void printCharacterLevelExperience()
        {
            var py = State.Instance.py;
            printHeaderLongNumber7Spaces("Level      ", (int)py.misc.level, new Coord_t(9, 28));
            printHeaderLongNumber7Spaces("Experience ", py.misc.exp, new Coord_t(10, 28));
            printHeaderLongNumber7Spaces("Max Exp    ", py.misc.max_exp, new Coord_t(11, 28));

            if (py.misc.level >= PLAYER_MAX_LEVEL)
            {
                putStringClearToEOL("Exp to Adv.: *******", new Coord_t(12, 28));
            }
            else
            {
                printHeaderLongNumber7Spaces("Exp to Adv.", (int)(py.base_exp_levels[py.misc.level - 1] * py.misc.experience_factor / 100), new Coord_t(12, 28));
            }

            printHeaderLongNumber7Spaces("Gold       ", py.misc.au, new Coord_t(13, 28));
            printHeaderNumber("Max Hit Points ", py.misc.max_hp, new Coord_t(9, 52));
            printHeaderNumber("Cur Hit Points ", py.misc.current_hp, new Coord_t(10, 52));
            printHeaderNumber("Max Mana       ", py.misc.mana, new Coord_t(11, 52));
            printHeaderNumber("Cur Mana       ", py.misc.current_mana, new Coord_t(12, 52));
        }

        // Prints ratings on certain abilities -RAK-
        public static void printCharacterAbilities()
        {
            var py = State.Instance.py;
            var class_level_adj = State.Instance.class_level_adj;
            clearToBottom(14);

            int xbth = py.misc.bth + py.misc.plusses_to_hit * (int)BTH_PER_PLUS_TO_HIT_ADJUST + (class_level_adj[py.misc.class_id][(int)PlayerClassLevelAdj.BTH] * (int)py.misc.level);
            int xbthb = py.misc.bth_with_bows + py.misc.plusses_to_hit * (int)BTH_PER_PLUS_TO_HIT_ADJUST + (class_level_adj[py.misc.class_id][(int)PlayerClassLevelAdj.BTHB] * (int)py.misc.level);

            // this results in a range from 0 to 29
            int xfos = 40 - py.misc.fos;
            if (xfos < 0)
            {
                xfos = 0;
            }

            int xsrh = py.misc.chance_in_search;

            // this results in a range from 0 to 9
            int xstl = py.misc.stealth_factor + 1;
            int xdis = py.misc.disarm + 2 * playerDisarmAdjustment() + playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT) +
                       (class_level_adj[py.misc.class_id][(int)PlayerClassLevelAdj.DISARM] * (int)py.misc.level / 3);
            int xsave =
                py.misc.saving_throw + playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.WIS) + (class_level_adj[py.misc.class_id][(int)PlayerClassLevelAdj.SAVE] * (int)py.misc.level / 3);
            int xdev =
                py.misc.saving_throw + playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT) + (class_level_adj[py.misc.class_id][(int)PlayerClassLevelAdj.DEVICE] * (int)py.misc.level / 3);

            var xinfra = $"{py.flags.see_infra * 10} feed";
            //vtype_t xinfra = { '\0' };
            //(void)sprintf(xinfra, "%d feet", py.flags.see_infra * 10);

            putString("(Miscellaneous Abilities)", new Coord_t(15, 25));
            putString("Fighting    :", new Coord_t(16, 1));
            putString(statRating(new Coord_t(12, xbth)), new Coord_t(16, 15));
            putString("Bows/Throw  :", new Coord_t(17, 1));
            putString(statRating(new Coord_t(12, xbthb)), new Coord_t(17, 15));
            putString("Saving Throw:", new Coord_t(18, 1));
            putString(statRating(new Coord_t(6, xsave)), new Coord_t(18, 15));

            putString("Stealth     :", new Coord_t(16, 28));
            putString(statRating(new Coord_t(1, xstl)), new Coord_t(16, 42));
            putString("Disarming   :", new Coord_t(17, 28));
            putString(statRating(new Coord_t(8, xdis)), new Coord_t(17, 42));
            putString("Magic Device:", new Coord_t(18, 28));
            putString(statRating(new Coord_t(6, xdev)), new Coord_t(18, 42));

            putString("Perception  :", new Coord_t(16, 55));
            putString(statRating(new Coord_t(3, xfos)), new Coord_t(16, 69));
            putString("Searching   :", new Coord_t(17, 55));
            putString(statRating(new Coord_t(6, xsrh)), new Coord_t(17, 69));
            putString("Infra-Vision:", new Coord_t(18, 55));
            putString(xinfra, new Coord_t(18, 69));
        }

        // Used to display the character on the screen. -RAK-
        public static void printCharacter()
        {
            printCharacterInformation();
            printCharacterVitalStatistics();
            printCharacterStats();
            printCharacterLevelExperience();
            printCharacterAbilities();
        }

        // Gets a name for the character -JWT-
        public static void getCharacterName()
        {
            var py = State.Instance.py;
            putStringClearToEOL("Enter your player's name  [press <RETURN> when finished]", new Coord_t(21, 2));

            putString(blank_string.Substring(0, BLANK_LENGTH - 23), new Coord_t(2, 15));
            //putString(&blank_string[BLANK_LENGTH - 23], new Coord_t(2, 15));

            string name = string.Empty;
            var getStringInputResult = getStringInput(out name, new Coord_t(2, 15), 23);
            py.misc.name = name;
            if (!getStringInputResult || py.misc.name[0] == 0)
            {
                var name2 = py.misc.name;
                getDefaultPlayerName(ref name2);
                py.misc.name = name2;
                putString(py.misc.name, new Coord_t(2, 15));
            }

            clearToBottom(20);
        }

        // Changes the name of the character -JWT-
        public static void changeCharacterName()
        {
            var temp = string.Empty;
            //vtype_t temp = { '\0' };
            bool flag = false;

            printCharacter();

            while (!flag)
            {
                putStringClearToEOL("<f>ile character description. <c>hange character name.", new Coord_t(21, 2));

                switch (getKeyInput())
                {
                    case 'c':
                        getCharacterName();
                        flag = true;
                        break;
                    case 'f':
                        putStringClearToEOL("File name:", new Coord_t(0, 0));

                        if (getStringInput(out temp, new Coord_t(0, 10), 60) && (temp[0] != 0))
                        {
                            if (outputPlayerCharacterToFile(temp))
                            {
                                flag = true;
                            }
                        }
                        break;
                    case ESCAPE:
                    case ' ':
                    case '\n':
                    case '\r':
                        flag = true;
                        break;
                    default:
                        terminalBellSound();
                        break;
                }
            }
        }

        // Print list of spells -RAK-
        // if non_consecutive is  -1: spells numbered consecutively from 'a' to 'a'+num
        //                       >=0: spells numbered by offset from non_consecutive
        public static void displaySpellsList(int[] spell_ids, int number_of_choices, bool comment, int non_consecutive)
        {
            var py = State.Instance.py;
            int col;
            if (comment)
            {
                col = 22;
            }
            else
            {
                col = 31;
            }

            int consecutive_offset;
            if (State.Instance.classes[py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
            {
                consecutive_offset = (int)Config.spells.NAME_OFFSET_SPELLS;
            }
            else
            {
                consecutive_offset = (int)Config.spells.NAME_OFFSET_PRAYERS;
            }

            eraseLine(new Coord_t(1, col));
            putString("Name", new Coord_t(1, col + 5));
            putString("Lv Mana Fail", new Coord_t(1, col + 35));

            // only show the first 22 choices
            if (number_of_choices > 22)
            {
                number_of_choices = 22;
            }

            for (int i = 0; i < number_of_choices; i++)
            {
                int spell_id = spell_ids[i];
                var spell = State.Instance.magic_spells[py.misc.class_id - 1][spell_id];

                string p = string.Empty;
                if (!comment)
                {
                    p = "";
                }
                else if ((py.flags.spells_forgotten & (1L << spell_id)) != 0)
                {
                    p = " forgotten";
                }
                else if ((py.flags.spells_learnt & (1L << spell_id)) == 0)
                {
                    p = " unknown";
                }
                else if ((py.flags.spells_worked & (1L << spell_id)) == 0)
                {
                    p = " untried";
                }
                else
                {
                    p = "";
                }

                // determine whether or not to leave holes in character choices, non_consecutive -1
                // when learning spells, consecutive_offset>=0 when asking which spell to cast.
                char spell_char;
                if (non_consecutive == -1)
                {
                    spell_char = (char)('a' + i);
                }
                else
                {
                    spell_char = (char)('a' + spell_id - non_consecutive);
                }

                var out_val = $"  {spell_char}) {State.Instance.spell_names[spell_id + consecutive_offset]}{spell.level_required:d2} {spell.mana_required:d4} {spellChanceOfSuccess(spell_id):d3}%{p}";
                //vtype_t out_val = { '\0' };
                //(void)sprintf(out_val,
                //    "  %c) %-30s%2d %4d %3d%%%s",
                //    spell_char,
                //    State.Instance.spell_names[spell_id + consecutive_offset],
                //    spell.level_required,
                //    spell.mana_required,
                //    spellChanceOfSuccess(spell_id),
                //    p);
                putStringClearToEOL(out_val, new Coord_t(2 + i, col));
            }
        }

        // Increases hit points and level -RAK-
        public static void playerGainLevel()
        {
            var py = State.Instance.py;
            py.misc.level++;

            var msg = $"Welcome to level {(int)py.misc.level}";
            //vtype_t msg = { '\0' };
            //(void)sprintf(msg, "Welcome to level %d.", (int)py.misc.level);
            printMessage(msg);

            playerCalculateHitPoints();

            int new_exp = (int)(py.base_exp_levels[py.misc.level - 1] * py.misc.experience_factor / 100);

            if (py.misc.exp > new_exp)
            {
                // lose some of the 'extra' exp when gaining several levels at once
                int dif_exp = py.misc.exp - new_exp;
                py.misc.exp = new_exp + (dif_exp / 2);
            }

            printCharacterLevel();
            printCharacterTitle();

            var player_class = State.Instance.classes[py.misc.class_id];

            if (player_class.class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
            {
                playerCalculateAllowedSpellsCount((int)PlayerAttr.INT);
                playerGainMana((int)PlayerAttr.INT);
            }
            else if (player_class.class_to_use_mage_spells == Config.spells.SPELL_TYPE_PRIEST)
            {
                playerCalculateAllowedSpellsCount((int)PlayerAttr.WIS);
                playerGainMana((int)PlayerAttr.WIS);
            }
        }

        // Prints experience -RAK-
        public static void displayCharacterExperience()
        {
            var py = State.Instance.py;
            if (py.misc.exp > Config.player.PLAYER_MAX_EXP)
            {
                py.misc.exp = Config.player.PLAYER_MAX_EXP;
            }

            while ((py.misc.level < PLAYER_MAX_LEVEL) && (int)(py.base_exp_levels[py.misc.level - 1] * py.misc.experience_factor / 100) <= py.misc.exp)
            {
                playerGainLevel();
            }

            if (py.misc.exp > py.misc.max_exp)
            {
                py.misc.max_exp = py.misc.exp;
            }

            printLongNumber(py.misc.exp, new Coord_t(14, (int)STAT_COLUMN + 6));
        }
    }
}
