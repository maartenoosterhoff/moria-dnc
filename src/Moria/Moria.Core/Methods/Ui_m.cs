using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_stats_m;

namespace Moria.Core.Methods
{
    public interface ITerminalEx
    {
        void drawDungeonPanel();
        void drawCavePanel();
        void dungeonResetView();
        void displayCharacterStats(int stat);
        void printCharacterTitle();
        void printCharacterLevel();
        void printCharacterCurrentMana();
        void printCharacterMaxHitPoints();
        void printCharacterCurrentHitPoints();
        void printCharacterCurrentArmorClass();
        void printCharacterGoldValue();
        void printCharacterCurrentDepth();
        void printCharacterHungerStatus();
        void printCharacterBlindStatus();
        void printCharacterConfusedState();
        void printCharacterFearState();
        void printCharacterPoisonedState();
        void printCharacterMovementState();
        void printCharacterSpeed();
        void printCharacterStudyInstruction();
        void printCharacterWinner();
        void printCharacterStatsBlock();
        void printCharacterInformation();
        void printCharacterStats();
        void printCharacterVitalStatistics();
        void printCharacterLevelExperience();
        void printCharacterAbilities();
        void printCharacter();
        void getCharacterName();
        void changeCharacterName();
        void displayCharacterExperience();
    }

    public class Ui_m : ITerminalEx
    {
        private readonly IDungeon dungeon;
        private readonly IEventPublisher eventPublisher;
        private readonly IGameFiles gameFiles;
        private readonly IHelpers helpers;
        private readonly ITerminal terminal;

        public Ui_m(
            IDungeon dungeon,
            IEventPublisher eventPublisher,
            IGameFiles gameFiles,
            IHelpers helpers,
            ITerminal terminal
        )
        {
            this.dungeon = dungeon;
            this.eventPublisher = eventPublisher;
            this.gameFiles = gameFiles;
            this.helpers = helpers;
            this.terminal = terminal;
        }

        private string[] stat_names = {
            "STR : ", "INT : ", "WIS : ", "DEX : ", "CON : ", "CHR : "
        };

        private const int BLANK_LENGTH = 24;

        //#define BLANK_LENGTH 24

        private readonly string blank_string = "                        ";

        

        // Prints the map of the dungeon -RAK-
        public void drawDungeonPanel()
        {
            var dg = State.Instance.dg;

            var line = 1;

            var coord = new Coord_t(0, 0);

            // Top to bottom
            for (coord.y = dg.panel.top; coord.y <= dg.panel.bottom; coord.y++)
            {
                this.terminal.eraseLine(new Coord_t(line, 13));
                line++;

                // Left to right
                for (coord.x = dg.panel.left; coord.x <= dg.panel.right; coord.x++)
                {
                    var ch = this.dungeon.caveGetTileSymbol(coord);
                    if (ch != ' ')
                    {
                        this.terminal.panelPutTile(ch, coord);
                    }
                }
            }
        }

        // Draws entire screen -RAK-
        public void drawCavePanel()
        {
            this.terminal.clearScreen();
            this.printCharacterStatsBlock();
            this.drawDungeonPanel();
            this.printCharacterCurrentDepth();
        }

        // We need to reset the view of things. -CJS-
        public void dungeonResetView()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            var tile = dg.floor[py.pos.y][py.pos.x];

            // Check for new panel
            if (this.helpers.coordOutsidePanel(py.pos, false))
            {
                this.drawDungeonPanel();
            }

            // Move the light source
            this.dungeon.dungeonMoveCharacterLight(py.pos, py.pos);

            // A room of light should be lit.
            if (tile.feature_id == TILE_LIGHT_FLOOR)
            {
                if (py.flags.blind < 1 && !tile.permanent_light)
                {
                    this.dungeon.dungeonLightRoom(py.pos);
                }
                return;
            }

            // In doorway of light-room?
            if (tile.perma_lit_room && py.flags.blind < 1)
            {
                for (var i = py.pos.y - 1; i <= py.pos.y + 1; i++)
                {
                    for (var j = py.pos.x - 1; j <= py.pos.x + 1; j++)
                    {
                        if (dg.floor[i][j].feature_id == TILE_LIGHT_FLOOR && !dg.floor[i][j].permanent_light)
                        {
                            this.dungeon.dungeonLightRoom(new Coord_t(i, j));
                        }
                    }
                }
            }
        }

        // Converts stat num into string
        private void statsAsString(uint stat, out string stat_string)
        {
            var percentile = (int)stat - 18;

            if (stat <= 18)
            {
                stat_string = $"{stat,6:d}";
                //(void)sprintf(stat_string, "%6d", stat);
            }
            else if (percentile == 100)
            {
                stat_string = "18/100";
                //(void)strcpy(stat_string, "18/100");
            }
            else
            {
                stat_string = $" 18/{percentile,2:d}";
                //(void)sprintf(stat_string, " 18/%02d", percentile);
            }
        }

        // Print character stat in given row, column -RAK-
        public void displayCharacterStats(int stat)
        {
            var py = State.Instance.py;
            this.statsAsString(py.stats.used[stat], out var text);
            this.terminal.putString(this.stat_names[stat], new Coord_t(6 + stat, (int)STAT_COLUMN));
            this.terminal.putString(text, new Coord_t(6 + stat, (int)STAT_COLUMN + 6));
        }

        // Print character info in given row, column -RAK-
        // The longest title is 13 characters, so only pad to 13
        private void printCharacterInfoInField(string info, Coord_t coord)
        {
            // blank out the current field space
            this.terminal.putString(this.blank_string.Substring(0, BLANK_LENGTH - 13), coord);
            //putString(&blank_string[BLANK_LENGTH - 13], coord);

            this.terminal.putString(info, coord);
        }

        // Print long number with header at given row, column
        private void printHeaderLongNumber(string header, int num, Coord_t coord)
        {
            var str = $"{header}: {num,6:d}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%s: %6d", header, num);
            this.terminal.putString(str, coord);
        }

        // Print long number (7 digits of space) with header at given row, column
        private void printHeaderLongNumber7Spaces(string header, int num, Coord_t coord)
        {
            var str = $"{header}: {num,7:d}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%s: %7d", header, num);
            this.terminal.putString(str, coord);
        }

        // Print number with header at given row, column -RAK-
        private void printHeaderNumber(string header, int num, Coord_t coord)
        {
            var str = $"{header}: {num,6:d}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%s: %6d", header, num);
            this.terminal.putString(str, coord);
        }

        // Print long number at given row, column
        private void printLongNumber(int num, Coord_t coord)
        {
            var str = $"{num,6:d}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%6d", num);
            this.terminal.putString(str, coord);
        }

        // Print number at given row, column -RAK-
        private void printNumber(int num, Coord_t coord)
        {
            var str = $"{num,6:d}";
            //vtype_t str = { '\0' };
            //(void)sprintf(str, "%6d", num);
            this.terminal.putString(str, coord);
        }

        // Prints title of character -RAK-
        public void printCharacterTitle()
        {
            this.printCharacterInfoInField(playerRankTitle(), new Coord_t(4, (int)STAT_COLUMN));
        }

        // Prints level -RAK-
        public void printCharacterLevel()
        {
            var py = State.Instance.py;
            this.printNumber((int)py.misc.level, new Coord_t(13, (int)STAT_COLUMN + 6));
        }

        // Prints players current mana points. -RAK-
        public void printCharacterCurrentMana()
        {
            var py = State.Instance.py;
            this.printNumber(py.misc.current_mana, new Coord_t(15, (int)STAT_COLUMN + 6));
        }

        // Prints Max hit points -RAK-
        public void printCharacterMaxHitPoints()
        {
            var py = State.Instance.py;
            this.printNumber(py.misc.max_hp, new Coord_t(16, (int)STAT_COLUMN + 6));
        }

        // Prints players current hit points -RAK-
        public void printCharacterCurrentHitPoints()
        {
            var py = State.Instance.py;
            this.printNumber(py.misc.current_hp, new Coord_t(17, (int)STAT_COLUMN + 6));
        }

        // prints current AC -RAK-
        public void printCharacterCurrentArmorClass()
        {
            var py = State.Instance.py;
            this.printNumber(py.misc.display_ac, new Coord_t(19, (int)STAT_COLUMN + 6));
        }

        // Prints current gold -RAK-
        public void printCharacterGoldValue()
        {
            var py = State.Instance.py;
            this.printLongNumber(py.misc.au, new Coord_t(20, (int)STAT_COLUMN + 6));
        }

        // Prints depth in stat area -RAK-
        public void printCharacterCurrentDepth()
        {
            var dg = State.Instance.dg;
            string depths;
            //vtype_t depths = { '\0' };

            var depth = dg.current_level * 50;

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

            this.terminal.putStringClearToEOL(depths, new Coord_t(23, 65));
        }

        // Prints status of hunger -RAK-
        public void printCharacterHungerStatus()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_WEAK) != 0u)
            {
                this.terminal.putString("Weak  ", new Coord_t(23, 0));
            }
            else if ((py.flags.status & Config.player_status.PY_HUNGRY) != 0u)
            {
                this.terminal.putString("Hungry", new Coord_t(23, 0));
            }
            else
            {
                this.terminal.putString(this.blank_string.Substring(0, BLANK_LENGTH - 6), new Coord_t(23, 0));
                //putString(&blank_string[BLANK_LENGTH - 6], new Coord_t(23, 0));
            }
        }

        // Prints Blind status -RAK-
        public void printCharacterBlindStatus()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_BLIND) != 0u)
            {
                this.terminal.putString("Blind", new Coord_t(23, 7));
            }
            else
            {
                this.terminal.putString(this.blank_string.Substring(0, BLANK_LENGTH - 5), new Coord_t(23, 7));
                //putString(&blank_string[BLANK_LENGTH - 5], new Coord_t(23, 7));
            }
        }

        // Prints Confusion status -RAK-
        public void printCharacterConfusedState()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_CONFUSED) != 0u)
            {
                this.terminal.putString("Confused", new Coord_t(23, 13));
            }
            else
            {
                this.terminal.putString(this.blank_string.Substring(BLANK_LENGTH - 8), new Coord_t(23, 13));
                //putString(&blank_string[BLANK_LENGTH - 8], new Coord_t(23, 13));
            }
        }

        // Prints Fear status -RAK-
        public void printCharacterFearState()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_FEAR) != 0u)
            {
                this.terminal.putString("Afraid", new Coord_t(23, 22));
            }
            else
            {
                //putString(&blank_string[BLANK_LENGTH - 6], new Coord_t(23, 22));
                this.terminal.putString(this.blank_string.Substring(BLANK_LENGTH - 6), new Coord_t(23, 22));
            }
        }

        // Prints Poisoned status -RAK-
        public void printCharacterPoisonedState()
        {
            var py = State.Instance.py;
            if ((py.flags.status & Config.player_status.PY_POISONED) != 0u)
            {
                this.terminal.putString("Poisoned", new Coord_t(23, 29));
            }
            else
            {
                this.terminal.putString(this.blank_string.Substring(0, BLANK_LENGTH - 8), new Coord_t(23, 29));
                //putString(&blank_string[BLANK_LENGTH - 8], new Coord_t(23, 29));
            }
        }

        // Prints Searching, Resting, Paralysis, or 'count' status -RAK-
        public void printCharacterMovementState()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            py.flags.status &= ~Config.player_status.PY_REPEAT;

            if (py.flags.paralysis > 1)
            {
                this.terminal.putString("Paralysed", new Coord_t(23, 38));
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
                    rest_string = $"Rest {py.flags.rest,-5:d}";
                    //(void)sprintf(rest_string, "Rest %-5d", py.flags.rest);
                }
                else
                {
                    rest_string = "Rest";
                    //(void)strcpy(rest_string, "Rest");
                }

                this.terminal.putString(rest_string, new Coord_t(23, 38));

                return;
            }

            if (game.command_count > 0)
            {
                string repeat_string;
                //char repeat_string[16];

                if (Config.options.display_counts)
                {
                    repeat_string = $"Repeat {game.command_count,-3:d}";
                    //(void)sprintf(repeat_string, "Repeat %-3d", game.command_count);
                }
                else
                {
                    repeat_string = "Repeat";
                    //(void)strcpy(repeat_string, "Repeat");
                }

                py.flags.status |= Config.player_status.PY_REPEAT;

                this.terminal.putString(repeat_string, new Coord_t(23, 38));

                if ((py.flags.status & Config.player_status.PY_SEARCH) != 0u)
                {
                    this.terminal.putString("Search", new Coord_t(23, 38));
                }

                return;
            }

            if ((py.flags.status & Config.player_status.PY_SEARCH) != 0u)
            {
                this.terminal.putString("Searching", new Coord_t(23, 38));
                return;
            }

            // "repeat 999" is 10 characters
            this.terminal.putString(this.blank_string.Substring(0, BLANK_LENGTH - 10), new Coord_t(23, 38));
            //putString(&blank_string[BLANK_LENGTH - 10], new Coord_t(23, 38});
        }

        // Prints the speed of a character. -CJS-
        public void printCharacterSpeed()
        {
            var py = State.Instance.py;
            var speed = py.flags.speed;

            // Search mode.
            if ((py.flags.status & Config.player_status.PY_SEARCH) != 0u)
            {
                speed--;
            }

            if (speed > 1)
            {
                this.terminal.putString("Very Slow", new Coord_t(23, 49));
            }
            else if (speed == 1)
            {
                this.terminal.putString("Slow     ", new Coord_t(23, 49));
            }
            else if (speed == 0)
            {
                this.terminal.putString(this.blank_string.Substring(0, BLANK_LENGTH - 9), new Coord_t(23, 49));
                //putString(&blank_string[BLANK_LENGTH - 9], new Coord_t(23, 49));
            }
            else if (speed == -1)
            {
                this.terminal.putString("Fast     ", new Coord_t(23, 49));
            }
            else
            {
                this.terminal.putString("Very Fast", new Coord_t(23, 49));
            }
        }

        public void printCharacterStudyInstruction()
        {
            var py = State.Instance.py;

            py.flags.status &= ~Config.player_status.PY_STUDY;

            if (py.flags.new_spells_to_learn == 0)
            {
                this.terminal.putString(this.blank_string.Substring(BLANK_LENGTH - 5), new Coord_t(23, 59));
                //putString(&blank_string[BLANK_LENGTH - 5], new Coord_t(23, 59));
            }
            else
            {
                this.terminal.putString("Study", new Coord_t(23, 59));
            }
        }

        // Prints winner status on display -RAK-
        public void printCharacterWinner()
        {
            var game = State.Instance.game;
            if ((game.noscore & 0x2) != 0)
            {
                if (game.wizard_mode)
                {
                    this.terminal.putString("Is wizard  ", new Coord_t(22, 0));
                }
                else
                {
                    this.terminal.putString("Was wizard ", new Coord_t(22, 0));
                }
            }
            else if ((game.noscore & 0x1) != 0)
            {
                this.terminal.putString("Resurrected", new Coord_t(22, 0));
            }
            else if ((game.noscore & 0x4) != 0)
            {
                this.terminal.putString("Duplicate", new Coord_t(22, 0));
            }
            else if (game.total_winner)
            {
                this.terminal.putString("*Winner*   ", new Coord_t(22, 0));
            }
        }

        // Prints character-screen info -RAK-
        public void printCharacterStatsBlock()
        {
            var py = State.Instance.py;

            this.printCharacterInfoInField(Library.Instance.Player.character_races[(int)py.misc.race_id].name, new Coord_t(2, (int)STAT_COLUMN));
            this.printCharacterInfoInField(Library.Instance.Player.classes[(int)py.misc.class_id].title, new Coord_t(3, (int)STAT_COLUMN));
            this.printCharacterInfoInField(playerRankTitle(), new Coord_t(4, (int)STAT_COLUMN));

            for (var i = 0; i < 6; i++)
            {
                this.displayCharacterStats(i);
            }

            this.printHeaderNumber("LEV ", (int)py.misc.level, new Coord_t(13, (int)STAT_COLUMN));
            this.printHeaderLongNumber("EXP ", py.misc.exp, new Coord_t(14, (int)STAT_COLUMN));
            this.printHeaderNumber("MANA", py.misc.current_mana, new Coord_t(15, (int)STAT_COLUMN));
            this.printHeaderNumber("MHP ", py.misc.max_hp, new Coord_t(16, (int)STAT_COLUMN));
            this.printHeaderNumber("CHP ", py.misc.current_hp, new Coord_t(17, (int)STAT_COLUMN));
            this.printHeaderNumber("AC  ", py.misc.display_ac, new Coord_t(19, (int)STAT_COLUMN));
            this.printHeaderLongNumber("GOLD", py.misc.au, new Coord_t(20, (int)STAT_COLUMN));
            this.printCharacterWinner();

            var status = py.flags.status;

            if (((Config.player_status.PY_HUNGRY | Config.player_status.PY_WEAK) & status) != 0u)
            {
                this.printCharacterHungerStatus();
            }

            if ((status & Config.player_status.PY_BLIND) != 0u)
            {
                this.printCharacterBlindStatus();
            }

            if ((status & Config.player_status.PY_CONFUSED) != 0u)
            {
                this.printCharacterConfusedState();
            }

            if ((status & Config.player_status.PY_FEAR) != 0u)
            {
                this.printCharacterFearState();
            }

            if ((status & Config.player_status.PY_POISONED) != 0u)
            {
                this.printCharacterPoisonedState();
            }

            if (((Config.player_status.PY_SEARCH | Config.player_status.PY_REST) & status) != 0u)
            {
                this.printCharacterMovementState();
            }

            // if speed non zero, print it, modify speed if Searching
            var speed = py.flags.speed - (int)((status & Config.player_status.PY_SEARCH) >> 8);
            if (speed != 0)
            {
                this.printCharacterSpeed();
            }

            // display the study field
            this.printCharacterStudyInstruction();
        }

        // Prints the following information on the screen. -JWT-
        public void printCharacterInformation()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            this.terminal.clearScreen();

            this.terminal.putString("Name        :", new Coord_t(2, 1));
            this.terminal.putString("Race        :", new Coord_t(3, 1));
            this.terminal.putString("Sex         :", new Coord_t(4, 1));
            this.terminal.putString("Class       :", new Coord_t(5, 1));

            if (!game.character_generated)
            {
                return;
            }

            this.terminal.putString(py.misc.name, new Coord_t(2, 15));
            this.terminal.putString(Library.Instance.Player.character_races[(int)py.misc.race_id].name, new Coord_t(3, 15));
            this.terminal.putString(playerGetGenderLabel(), new Coord_t(4, 15));
            this.terminal.putString(Library.Instance.Player.classes[(int)py.misc.class_id].title, new Coord_t(5, 15));
        }

        // Prints the following information on the screen. -JWT-
        public void printCharacterStats()
        {
            var py = State.Instance.py;
            for (var i = 0; i < 6; i++)
            {
                this.statsAsString(py.stats.used[i], out var buf);
                this.terminal.putString(this.stat_names[i], new Coord_t(2 + i, 61));
                this.terminal.putString(buf, new Coord_t(2 + i, 66));

                if (py.stats.max[i] > py.stats.current[i])
                {
                    this.statsAsString(py.stats.max[i], out buf);
                    this.terminal.putString(buf, new Coord_t(2 + i, 73));
                }
            }

            this.printHeaderNumber("+ To Hit    ", py.misc.display_to_hit, new Coord_t(9, 1));
            this.printHeaderNumber("+ To Damage ", py.misc.display_to_damage, new Coord_t(10, 1));
            this.printHeaderNumber("+ To AC     ", py.misc.display_to_ac, new Coord_t(11, 1));
            this.printHeaderNumber("  Total AC  ", py.misc.display_ac, new Coord_t(12, 1));
        }

        // Returns a rating of x depending on y -JWT-
        private string statRating(int y, int x)
        {
            switch (x / y)
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
        public void printCharacterVitalStatistics()
        {
            var py = State.Instance.py;
            this.printHeaderNumber("Age          ", (int)py.misc.age, new Coord_t(2, 38));
            this.printHeaderNumber("Height       ", (int)py.misc.height, new Coord_t(3, 38));
            this.printHeaderNumber("Weight       ", (int)py.misc.weight, new Coord_t(4, 38));
            this.printHeaderNumber("Social Class ", (int)py.misc.social_class, new Coord_t(5, 38));
        }

        // Prints the following information on the screen. -JWT-
        public void printCharacterLevelExperience()
        {
            var py = State.Instance.py;
            this.printHeaderLongNumber7Spaces("Level      ", (int)py.misc.level, new Coord_t(9, 28));
            this.printHeaderLongNumber7Spaces("Experience ", py.misc.exp, new Coord_t(10, 28));
            this.printHeaderLongNumber7Spaces("Max Exp    ", py.misc.max_exp, new Coord_t(11, 28));

            if (py.misc.level >= PLAYER_MAX_LEVEL)
            {
                this.terminal.putStringClearToEOL("Exp to Adv.: *******", new Coord_t(12, 28));
            }
            else
            {
                this.printHeaderLongNumber7Spaces("Exp to Adv.", (int)(py.base_exp_levels[py.misc.level - 1] * py.misc.experience_factor / 100), new Coord_t(12, 28));
            }

            this.printHeaderLongNumber7Spaces("Gold       ", py.misc.au, new Coord_t(13, 28));
            this.printHeaderNumber("Max Hit Points ", py.misc.max_hp, new Coord_t(9, 52));
            this.printHeaderNumber("Cur Hit Points ", py.misc.current_hp, new Coord_t(10, 52));
            this.printHeaderNumber("Max Mana       ", py.misc.mana, new Coord_t(11, 52));
            this.printHeaderNumber("Cur Mana       ", py.misc.current_mana, new Coord_t(12, 52));
        }

        // Prints ratings on certain abilities -RAK-
        public void printCharacterAbilities()
        {
            var py = State.Instance.py;
            var class_level_adj = Library.Instance.Player.class_level_adj;
            this.terminal.clearToBottom(14);

            var xbth = py.misc.bth + py.misc.plusses_to_hit * (int)BTH_PER_PLUS_TO_HIT_ADJUST + class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.BTH] * (int)py.misc.level;
            var xbthb = py.misc.bth_with_bows + py.misc.plusses_to_hit * (int)BTH_PER_PLUS_TO_HIT_ADJUST + class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.BTHB] * (int)py.misc.level;

            // this results in a range from 0 to 29
            var xfos = 40 - py.misc.fos;
            if (xfos < 0)
            {
                xfos = 0;
            }

            var xsrh = py.misc.chance_in_search;

            // this results in a range from 0 to 9
            var xstl = py.misc.stealth_factor + 1;
            var xdis = py.misc.disarm + 2 * playerDisarmAdjustment() + playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT) +
                       class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DISARM] * (int)py.misc.level / 3;
            var xsave =
                py.misc.saving_throw + playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.WIS) + class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.SAVE] * (int)py.misc.level / 3;
            var xdev =
                py.misc.saving_throw + playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT) + class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DEVICE] * (int)py.misc.level / 3;

            var xinfra = $"{py.flags.see_infra * 10} feet";
            //vtype_t xinfra = { '\0' };
            //(void)sprintf(xinfra, "%d feet", py.flags.see_infra * 10);

            this.terminal.putString("(Miscellaneous Abilities)", new Coord_t(15, 25));
            this.terminal.putString("Fighting    :", new Coord_t(16, 1));
            this.terminal.putString(this.statRating(12, xbth), new Coord_t(16, 15));
            this.terminal.putString("Bows/Throw  :", new Coord_t(17, 1));
            this.terminal.putString(this.statRating(12, xbthb), new Coord_t(17, 15));
            this.terminal.putString("Saving Throw:", new Coord_t(18, 1));
            this.terminal.putString(this.statRating(6, xsave), new Coord_t(18, 15));

            this.terminal.putString("Stealth     :", new Coord_t(16, 28));
            this.terminal.putString(this.statRating(1, xstl), new Coord_t(16, 42));
            this.terminal.putString("Disarming   :", new Coord_t(17, 28));
            this.terminal.putString(this.statRating(8, xdis), new Coord_t(17, 42));
            this.terminal.putString("Magic Device:", new Coord_t(18, 28));
            this.terminal.putString(this.statRating(6, xdev), new Coord_t(18, 42));

            this.terminal.putString("Perception  :", new Coord_t(16, 55));
            this.terminal.putString(this.statRating(3, xfos), new Coord_t(16, 69));
            this.terminal.putString("Searching   :", new Coord_t(17, 55));
            this.terminal.putString(this.statRating(6, xsrh), new Coord_t(17, 69));
            this.terminal.putString("Infra-Vision:", new Coord_t(18, 55));
            this.terminal.putString(xinfra, new Coord_t(18, 69));
        }

        // Used to display the character on the screen. -RAK-
        public void printCharacter()
        {
            this.printCharacterInformation();
            this.printCharacterVitalStatistics();
            this.printCharacterStats();
            this.printCharacterLevelExperience();
            this.printCharacterAbilities();
        }

        // Gets a name for the character -JWT-
        public void getCharacterName()
        {
            var py = State.Instance.py;
            this.terminal.putStringClearToEOL("Enter your player's name  [press <RETURN> when finished]", new Coord_t(21, 2));

            this.terminal.putString(this.blank_string.Substring(0, BLANK_LENGTH - 23), new Coord_t(2, 15));
            //putString(&blank_string[BLANK_LENGTH - 23], new Coord_t(2, 15));

            var getStringInputResult = this.terminal.getStringInput(out var name, new Coord_t(2, 15), 23);
            py.misc.name = name;
            if (!getStringInputResult || string.IsNullOrEmpty(py.misc.name))
            {
                this.terminal.getDefaultPlayerName(out var defaultName);
                py.misc.name = defaultName;
                this.terminal.putString(py.misc.name, new Coord_t(2, 15));
            }

            this.terminal.clearToBottom(20);
        }

        // Changes the name of the character -JWT-
        public void changeCharacterName()
        {
            //vtype_t temp = { '\0' };
            var flag = false;

            this.printCharacter();

            while (!flag)
            {
                this.terminal.putStringClearToEOL("<f>ile character description. <c>hange character name.", new Coord_t(21, 2));

                switch (this.terminal.getKeyInput())
                {
                    case 'c':
                        this.getCharacterName();
                        flag = true;
                        break;
                    case 'f':
                        this.terminal.putStringClearToEOL("File name:", new Coord_t(0, 0));

                        string temp;
                        if (this.terminal.getStringInput(out temp, new Coord_t(0, 10), 60) && temp[0] != 0)
                        {
                            if (this.gameFiles.outputPlayerCharacterToFile(temp))
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
                        this.terminal.terminalBellSound();
                        break;
                }
            }
        }

        // Increases hit points and level -RAK-
        private void playerGainLevel()
        {
            var py = State.Instance.py;
            py.misc.level++;

            var msg = $"Welcome to level {(int)py.misc.level}";
            //vtype_t msg = { '\0' };
            //(void)sprintf(msg, "Welcome to level %d.", (int)py.misc.level);
            this.terminal.printMessage(msg);

            playerCalculateHitPoints();

            var new_exp = (int)(py.base_exp_levels[py.misc.level - 1] * py.misc.experience_factor / 100);

            if (py.misc.exp > new_exp)
            {
                // lose some of the 'extra' exp when gaining several levels at once
                var dif_exp = py.misc.exp - new_exp;
                py.misc.exp = new_exp + dif_exp / 2;
            }

            this.printCharacterLevel();
            this.printCharacterTitle();

            var player_class = Library.Instance.Player.classes[(int)py.misc.class_id];

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
        public void displayCharacterExperience()
        {
            var py = State.Instance.py;
            if (py.misc.exp > Config.player.PLAYER_MAX_EXP)
            {
                py.misc.exp = Config.player.PLAYER_MAX_EXP;
            }

            while (py.misc.level < PLAYER_MAX_LEVEL && (int)(py.base_exp_levels[py.misc.level - 1] * py.misc.experience_factor / 100) <= py.misc.exp)
            {
                this.playerGainLevel();
            }

            if (py.misc.exp > py.misc.max_exp)
            {
                py.misc.max_exp = py.misc.exp;
            }

            this.printLongNumber(py.misc.exp, new Coord_t(14, (int)STAT_COLUMN + 6));
        }
    }
}
