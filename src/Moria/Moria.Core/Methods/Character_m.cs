﻿using Moria.Core.Data;
using Moria.Core.Resources;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Player_m;

namespace Moria.Core.Methods
{
    public interface ICharacter
    {
        void characterCreate();
    }

    public class Character_m : ICharacter
    {
        private readonly IGame game;
        private readonly IGameFiles gameFiles;
        private readonly IRnd rnd;
        private readonly ITerminal terminal;
        private readonly ITerminalEx terminalEx;

        public Character_m(
            IGame game,
            IGameFiles gameFiles,
            IRnd rnd,
            ITerminal terminal,
            ITerminalEx terminalEx
        )
        {
            this.game = game;
            this.gameFiles = gameFiles;
            this.rnd = rnd;
            this.terminal = terminal;
            this.terminalEx = terminalEx;
        }

        // Generates character's stats -JWT-
        private void characterGenerateStats()
        {
            var py = State.Instance.py;
            int total;
            var dice = new int[18];

            do
            {
                total = 0;
                for (var i = 0; i < 18; i++)
                {
                    // Roll 3,4,5 sided dice once each
                    dice[i] = this.rnd.randomNumber(3 + i % 3);
                    total += dice[i];
                }
            } while (total <= 42 || total >= 54);

            for (var i = 0; i < 6; i++)
            {
                py.stats.max[i] = (uint)(5 + dice[3 * i] + dice[3 * i + 1] + dice[3 * i + 2]);
            }
        }

        private uint decrementStat(int adjustment, uint current_stat)
        {
            var stat = current_stat;
            for (var i = 0; i > adjustment; i--)
            {
                if (stat > 108)
                {
                    stat--;
                }
                else if (stat > 88)
                {
                    stat = stat - (uint)this.rnd.randomNumber(6) - 2;
                }
                else if (stat > 18)
                {
                    stat = stat - (uint)this.rnd.randomNumber(15) - 5;
                    if (stat < 18)
                    {
                        stat = 18;
                    }
                }
                else if (stat > 3)
                {
                    stat--;
                }
            }
            return stat;
        }

        private uint incrementStat(int adjustment, uint current_stat)
        {
            var stat = current_stat;
            for (var i = 0; i < adjustment; i++)
            {
                if (stat < 18)
                {
                    stat++;
                }
                else if (stat < 88)
                {
                    stat += (uint)this.rnd.randomNumber(15) + 5;
                }
                else if (stat < 108)
                {
                    stat += (uint)this.rnd.randomNumber(6) + 2;
                }
                else if (stat < 118)
                {
                    stat++;
                }
            }
            return stat;
        }

        // Changes stats by given amount -JWT-
        // During character creation we adjust player stats based
        // on their Race and Class...with a little randomness!
        private uint createModifyPlayerStat(uint stat, int adjustment)
        {
            if (adjustment < 0)
            {
                return this.decrementStat(adjustment, stat);
            }
            return this.incrementStat(adjustment, stat);
        }

        // generate all stats and modify for race. needed in a separate
        // module so looping of character selection would be allowed -RGM-
        void characterGenerateStatsAndRace()
        {
            var py = State.Instance.py;
            var race = Library.Instance.Player.character_races[(int)py.misc.race_id];

            this.characterGenerateStats();
            py.stats.max[(int)PlayerAttr.STR] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.STR], race.str_adjustment);
            py.stats.max[(int)PlayerAttr.INT] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.INT], race.int_adjustment);
            py.stats.max[(int)PlayerAttr.WIS] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.WIS], race.wis_adjustment);
            py.stats.max[(int)PlayerAttr.DEX] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.DEX], race.dex_adjustment);
            py.stats.max[(int)PlayerAttr.CON] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.CON], race.con_adjustment);
            py.stats.max[(int)PlayerAttr.CHR] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.CHR], race.chr_adjustment);

            py.misc.level = 1;

            for (var i = 0; i < 6; i++)
            {
                py.stats.current[i] = py.stats.max[i];
                playerSetAndUseStat(i);
            }

            py.misc.chance_in_search = race.search_chance_base;
            py.misc.bth = race.base_to_hit;
            py.misc.bth_with_bows = race.base_to_hit_bows;
            py.misc.fos = race.fos;
            py.misc.stealth_factor = race.stealth;
            py.misc.saving_throw = race.saving_throw_base;
            py.misc.hit_die = race.hit_points_base;
            py.misc.plusses_to_damage = playerDamageAdjustment();
            py.misc.plusses_to_hit = playerToHitAdjustment();
            py.misc.magical_ac = 0;
            py.misc.ac = playerArmorClassAdjustment();
            py.misc.experience_factor = race.exp_factor_base;
            py.flags.see_infra = (int)race.infra_vision;
        }

        // Prints a list of the available races: Human, Elf, etc.,
        // shown during the character creation screens.
        private void displayCharacterRaces()
        {
            this.terminal.clearToBottom(20);
            this.terminal.putString("Choose a race (? for Help):", new Coord_t(20, 2));

            var coord = new Coord_t(21, 2);

            for (var i = 0; i < PLAYER_MAX_RACES; i++)
            {
                var description = $"{(char)(i + 'a')}) {Library.Instance.Player.character_races[i].name}";
                this.terminal.putString(description, coord);

                coord.x += 15;
                if (coord.x > 70)
                {
                    coord.x = 2;
                    coord.y++;
                }
            }
        }

        // Allows player to select a race -JWT-
        private void characterChooseRace()
        {
            this.displayCharacterRaces();

            int race_id;

            while (true)
            {
                this.terminal.moveCursor(new Coord_t(20, 30));

                var input = this.terminal.getKeyInput();
                race_id = input - 'a';

                if (race_id < PLAYER_MAX_RACES && race_id >= 0)
                {
                    break;
                }

                if (input == '?')
                {
                    this.gameFiles.displayTextHelpFile(DataFilesResource.welcome /*Config.files.welcome_screen*/);
                }
                else
                {
                    this.terminal.terminalBellSound();
                }
            }

            State.Instance.py.misc.race_id = (uint)race_id;

            this.terminal.putString(Library.Instance.Player.character_races[race_id].name, new Coord_t(3, 15));
        }

        // Will print the history of a character -JWT-
        private void displayCharacterHistory()
        {
            this.terminal.putString("Character Background", new Coord_t(14, 27));

            for (var i = 0; i < 4; i++)
            {
                this.terminal.putStringClearToEOL(State.Instance.py.misc.history[i], new Coord_t(i + 15, 10));
            }
        }

        // Clear the previous history strings
        private void playerClearHistory()
        {
            State.Instance.py.misc.history = new string[4];
            //for (var  & entry : py.misc.history)
            //{
            //    entry[0] = '\0';
            //}
        }

        // Get the racial history, determines social class -RAK-
        //
        // Assumptions:
        //   - Each race has init history beginning at (race-1)*3+1
        //   - All history parts are in ascending order
        private void characterGetHistory()
        {
            var py = State.Instance.py;
            var history_id = py.misc.race_id * 3 + 1;
            var social_class = this.rnd.randomNumber(4);

            var history_block = string.Empty;

            var background_id = 0;

            bool flag;


            // Get a block of history text
            do
            {
                flag = false;
                while (!flag)
                {
                    if (Library.Instance.Player.character_backgrounds[background_id].chart == history_id)
                    {
                        var test_roll = this.rnd.randomNumber(100);

                        while (test_roll > Library.Instance.Player.character_backgrounds[background_id].roll)
                        {
                            background_id++;
                        }

                        var background = Library.Instance.Player.character_backgrounds[background_id];

                        history_block = $"{history_block}{background.info}";
                        //(void)strcat(history_block, background.info);
                        social_class += (int)background.bonus - 50;

                        if (history_id > background.next)
                        {
                            background_id = 0;
                        }

                        history_id = background.next;
                        flag = true;
                    }
                    else
                    {
                        background_id++;
                    }
                }
            } while (history_id >= 1);

            this.playerClearHistory();

            // Process block of history text for pretty output
            var cursor_start = 0;
            var cursor_end = history_block.Length - 1;// (int)strlen(history_block) - 1;
            while (history_block[cursor_end] == ' ')
            {
                cursor_end--;
            }

            var line_number = 0;
            var new_cursor_start = 0;

            flag = false;
            while (!flag)
            {
                while (history_block[cursor_start] == ' ')
                {
                    cursor_start++;
                }

                var current_cursor_position = cursor_end - cursor_start + 1;

                if (current_cursor_position > 60)
                {
                    current_cursor_position = 60;

                    while (history_block[cursor_start + current_cursor_position - 1] != ' ')
                    {
                        current_cursor_position--;
                    }

                    new_cursor_start = cursor_start + current_cursor_position;

                    while (history_block[cursor_start + current_cursor_position - 1] == ' ')
                    {
                        current_cursor_position--;
                    }
                }
                else
                {
                    flag = true;
                }

                py.misc.history[line_number] = history_block.Substring(cursor_start, current_cursor_position);
                // (void)strncpy(py.misc.history[line_number], &history_block[cursor_start], (size_t)current_cursor_position);
                // py.misc.history[line_number][current_cursor_position] = '\0';

                line_number++;
                cursor_start = new_cursor_start;
            }

            // Compute social class for player
            if (social_class > 100)
            {
                social_class = 100;
            }
            else if (social_class < 1)
            {
                social_class = 1;
            }

            py.misc.social_class = social_class;
        }

        // Gets the character's gender -JWT-
        void characterSetGender()
        {
            this.terminal.clearToBottom(20);
            this.terminal.putString("Choose a sex (? for Help):", new Coord_t(20, 2));
            this.terminal.putString("m) Male       f) Female", new Coord_t(21, 2));

            var is_set = false;
            while (!is_set)
            {
                this.terminal.moveCursor(new Coord_t(20, 29));

                // speed not important here
                var input = this.terminal.getKeyInput();

                if (input == 'f' || input == 'F')
                {
                    playerSetGender(false);
                    this.terminal.putString("Female", new Coord_t(4, 15));
                    is_set = true;
                }
                else if (input == 'm' || input == 'M')
                {
                    playerSetGender(true);
                    this.terminal.putString("Male", new Coord_t(4, 15));
                    is_set = true;
                }
                else if (input == '?')
                {
                    this.gameFiles.displayTextHelpFile(DataFilesResource.welcome /*Config.files.welcome_screen*/);
                }
                else
                {
                    this.terminal.terminalBellSound();
                }
            }
        }

        // Computes character's age, height, and weight -JWT-
        void characterSetAgeHeightWeight()
        {
            var py = State.Instance.py;

            var race = Library.Instance.Player.character_races[(int)py.misc.race_id];

            py.misc.age = (uint)(race.base_age + this.rnd.randomNumber(race.max_age));

            int height_base, height_mod, weight_base, weight_mod;
            if (playerIsMale())
            {
                height_base = (int)race.male_height_base;
                height_mod = (int)race.male_height_mod;
                weight_base = (int)race.male_weight_base;
                weight_mod = (int)race.male_weight_mod;
            }
            else
            {
                height_base = (int)race.female_height_base;
                height_mod = (int)race.female_height_mod;
                weight_base = (int)race.female_weight_base;
                weight_mod = (int)race.female_weight_mod;
            }

            py.misc.height = (uint)this.rnd.randomNumberNormalDistribution(height_base, height_mod);
            py.misc.weight = (uint)this.rnd.randomNumberNormalDistribution(weight_base, weight_mod);
            py.misc.disarm = race.disarm_chance_base + playerDisarmAdjustment();
        }

        // Prints the classes for a given race: Rogue, Mage, Priest, etc.,
        // shown during the character creation screens.
        private int displayRaceClasses(uint race_id, uint[] class_list)
        {
            var coord = new Coord_t(21, 2);

            var class_id = 0;

            uint mask = 0x1;

            this.terminal.clearToBottom(20);
            this.terminal.putString("Choose a class (? for Help):", new Coord_t(20, 2));

            for (uint i = 0; i < PLAYER_MAX_CLASSES; i++)
            {
                if ((Library.Instance.Player.character_races[(int)race_id].classes_bit_field & mask) != 0u)
                {
                    var description = $"{(char)(class_id + 'a')}) {Library.Instance.Player.classes[(int)i].title}";
                    // (void)sprintf(description, "%c) %s", class_id + 'a', classes[i].title);
                    this.terminal.putString(description, coord);
                    class_list[class_id] = i;

                    coord.x += 15;
                    if (coord.x > 70)
                    {
                        coord.x = 2;
                        coord.y++;
                    }
                    class_id++;
                }
                mask <<= 1;
            }

            return class_id;
        }

        private void generateCharacterClass(uint class_id)
        {
            var py = State.Instance.py;
            py.misc.class_id = class_id;

            var klass = Library.Instance.Player.classes[(int)py.misc.class_id];

            this.terminal.clearToBottom(20);
            this.terminal.putString(klass.title, new Coord_t(5, 15));

            // Adjust the stats for the class adjustment -RAK-
            py.stats.max[(int)PlayerAttr.STR] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.STR], klass.strength);
            py.stats.max[(int)PlayerAttr.INT] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.INT], klass.intelligence);
            py.stats.max[(int)PlayerAttr.WIS] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.WIS], klass.wisdom);
            py.stats.max[(int)PlayerAttr.DEX] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.DEX], klass.dexterity);
            py.stats.max[(int)PlayerAttr.CON] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.CON], klass.constitution);
            py.stats.max[(int)PlayerAttr.CHR] = this.createModifyPlayerStat(py.stats.max[(int)PlayerAttr.CHR], klass.charisma);

            for (var i = 0; i < 6; i++)
            {
                py.stats.current[i] = py.stats.max[i];
                playerSetAndUseStat(i);
            }

            // Real values
            py.misc.plusses_to_damage = playerDamageAdjustment();
            py.misc.plusses_to_hit = playerToHitAdjustment();
            py.misc.magical_ac = playerArmorClassAdjustment();
            py.misc.ac = 0;

            // Displayed values
            py.misc.display_to_damage = py.misc.plusses_to_damage;
            py.misc.display_to_hit = py.misc.plusses_to_hit;
            py.misc.display_to_ac = py.misc.magical_ac;
            py.misc.display_ac = py.misc.ac + py.misc.display_to_ac;

            // now set misc stats, do this after setting stats because of playerStatAdjustmentConstitution() for hit-points
            py.misc.hit_die += klass.hit_points;
            py.misc.max_hp = playerStatAdjustmentConstitution() + (int)py.misc.hit_die;
            py.misc.current_hp = py.misc.max_hp;
            py.misc.current_hp_fraction = 0;

            // Initialize hit_points array.
            // Put bounds on total possible hp, only succeed
            // if it is within 1/8 of average value.
            var min_value = PLAYER_MAX_LEVEL * 3 / 8 * (py.misc.hit_die - 1) + PLAYER_MAX_LEVEL;
            var max_value = PLAYER_MAX_LEVEL * 5 / 8 * (py.misc.hit_die - 1) + PLAYER_MAX_LEVEL;
            py.base_hp_levels[0] = py.misc.hit_die;

            do
            {
                for (var i = 1; i < PLAYER_MAX_LEVEL; i++)
                {
                    py.base_hp_levels[i] = (uint)this.rnd.randomNumber(py.misc.hit_die);
                    py.base_hp_levels[i] += py.base_hp_levels[i - 1];
                }
            } while (py.base_hp_levels[PLAYER_MAX_LEVEL - 1] < min_value || py.base_hp_levels[PLAYER_MAX_LEVEL - 1] > max_value);

            py.misc.bth += (int)klass.base_to_hit;
            py.misc.bth_with_bows += (int)klass.base_to_hit_with_bows; // RAK
            py.misc.chance_in_search += (int)klass.searching;
            py.misc.disarm += (int)klass.disarm_traps;
            py.misc.fos += (int)klass.fos;
            py.misc.stealth_factor += (int)klass.stealth;
            py.misc.saving_throw += (int)klass.saving_throw;
            py.misc.experience_factor += klass.experience_factor;
        }

        // Gets a character class -JWT-
        private void characterGetClass()
        {
            var py = State.Instance.py;
            var class_list = new uint[PLAYER_MAX_CLASSES];
            var class_count = this.displayRaceClasses(py.misc.race_id, class_list);

            // Reset the class ID
            py.misc.class_id = 0;

            var is_set = false;

            while (!is_set)
            {
                this.terminal.moveCursor(new Coord_t(20, 31));

                var input = this.terminal.getKeyInput();
                var class_id = input - 'a';

                if (class_id < class_count && class_id >= 0)
                {
                    is_set = true;
                    this.generateCharacterClass(class_list[class_id]);
                }
                else if (input == '?')
                {
                    this.gameFiles.displayTextHelpFile(DataFilesResource.welcome /*Config.files.welcome_screen*/);
                }
                else
                {
                    this.terminal.terminalBellSound();
                }
            }
        }

        // Given a stat value, return a monetary value,
        // which affects the amount of gold a player has.
        private int monetaryValueCalculatedFromStat(uint stat)
        {
            return 5 * ((int)stat - 10);
        }

        private void playerCalculateStartGold()
        {
            var py = State.Instance.py;

            var value = this.monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.STR]);
            value += this.monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.INT]);
            value += this.monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.WIS]);
            value += this.monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.CON]);
            value += this.monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.DEX]);

            // Social Class adjustment
            var new_gold = py.misc.social_class * 6 + this.rnd.randomNumber(25) + 325;

            // Stat adjustment
            new_gold -= value;

            // Charisma adjustment
            new_gold += this.monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.CHR]);

            // She charmed the banker into it! -CJS-
            if (!playerIsMale())
            {
                new_gold += 50;
            }

            // Minimum
            if (new_gold < 80)
            {
                new_gold = 80;
            }

            py.misc.au = new_gold;
        }

        // Main Character Creation Routine -JWT-
        public void characterCreate()
        {
            this.terminalEx.printCharacterInformation();
            this.characterChooseRace();
            this.characterSetGender();

            // here we start a loop giving a player a choice of characters -RGM-
            var done = false;
            while (!done)
            {
                this.characterGenerateStatsAndRace();
                this.characterGetHistory();
                this.characterSetAgeHeightWeight();
                this.displayCharacterHistory();
                this.terminalEx.printCharacterVitalStatistics();
                this.terminalEx.printCharacterStats();

                this.terminal.clearToBottom(20);
                this.terminal.putString("Hit space to re-roll or ESC to accept characteristics: ", new Coord_t(20, 2));

                while (true)
                {
                    var input = this.terminal.getKeyInput();
                    if (input == ESCAPE)
                    {
                        done = true;
                        break;
                    }

                    if (input == ' ')
                    {
                        break;
                    }
                    this.terminal.terminalBellSound();
                }
            }

            this.characterGetClass();
            this.playerCalculateStartGold();
            this.terminalEx.printCharacterStats();
            this.terminalEx.printCharacterLevelExperience();
            this.terminalEx.printCharacterAbilities();
            this.terminalEx.getCharacterName();

            this.terminal.putStringClearToEOL("[ press any key to continue, or Q to exit ]", new Coord_t(23, 17));
            if (this.terminal.getKeyInput() == 'Q')
            {
                this.game.exitProgram();
            }
            this.terminal.eraseLine(new Coord_t(23, 0));
        }
    }
}
