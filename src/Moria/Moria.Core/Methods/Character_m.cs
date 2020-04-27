using Moria.Core.Data;
using Moria.Core.Resources;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Game_files_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Character_m
    {
        // Generates character's stats -JWT-
        public static void characterGenerateStats()
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
                    dice[i] = randomNumber(3 + i % 3);
                    total += dice[i];
                }
            } while (total <= 42 || total >= 54);

            for (var i = 0; i < 6; i++)
            {
                py.stats.max[i] = (uint)(5 + dice[3 * i] + dice[3 * i + 1] + dice[3 * i + 2]);
            }
        }

        public static uint decrementStat(int adjustment, uint current_stat)
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
                    stat = stat - (uint)randomNumber(6) - 2;
                }
                else if (stat > 18)
                {
                    stat = stat - (uint)randomNumber(15) - 5;
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

        public static uint incrementStat(int adjustment, uint current_stat)
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
                    stat += (uint)randomNumber(15) + 5;
                }
                else if (stat < 108)
                {
                    stat += (uint)randomNumber(6) + 2;
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
        public static uint createModifyPlayerStat(uint stat, int adjustment)
        {
            if (adjustment < 0)
            {
                return decrementStat(adjustment, stat);
            }
            return incrementStat(adjustment, stat);
        }

        // generate all stats and modify for race. needed in a separate
        // module so looping of character selection would be allowed -RGM-
        static void characterGenerateStatsAndRace()
        {
            var py = State.Instance.py;
            var race = Library.Instance.Player.character_races[(int)py.misc.race_id];

            characterGenerateStats();
            py.stats.max[(int)PlayerAttr.STR] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.STR], race.str_adjustment);
            py.stats.max[(int)PlayerAttr.INT] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.INT], race.int_adjustment);
            py.stats.max[(int)PlayerAttr.WIS] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.WIS], race.wis_adjustment);
            py.stats.max[(int)PlayerAttr.DEX] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.DEX], race.dex_adjustment);
            py.stats.max[(int)PlayerAttr.CON] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.CON], race.con_adjustment);
            py.stats.max[(int)PlayerAttr.CHR] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.CHR], race.chr_adjustment);

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
        public static void displayCharacterRaces()
        {
            clearToBottom(20);
            putString("Choose a race (? for Help):", new Coord_t(20, 2));

            var coord = new Coord_t(21, 2);

            for (var i = 0; i < PLAYER_MAX_RACES; i++)
            {
                var description = $"{(char)(i + 'a')}) {Library.Instance.Player.character_races[i].name}";
                putString(description, coord);

                coord.x += 15;
                if (coord.x > 70)
                {
                    coord.x = 2;
                    coord.y++;
                }
            }
        }

        // Allows player to select a race -JWT-
        public static void characterChooseRace()
        {
            displayCharacterRaces();

            var race_id = 0;

            while (true)
            {
                moveCursor(new Coord_t(20, 30));

                var input = getKeyInput();
                race_id = input - 'a';

                if (race_id < PLAYER_MAX_RACES && race_id >= 0)
                {
                    break;
                }

                if (input == '?')
                {
                    displayTextHelpFile(DataFilesResource.welcome /*Config.files.welcome_screen*/);
                }
                else
                {
                    terminalBellSound();
                }
            }

            State.Instance.py.misc.race_id = (uint)race_id;

            putString(Library.Instance.Player.character_races[race_id].name, new Coord_t(3, 15));
        }

        // Will print the history of a character -JWT-
        public static void displayCharacterHistory()
        {
            putString("Character Background", new Coord_t(14, 27));

            for (var i = 0; i < 4; i++)
            {
                putStringClearToEOL(State.Instance.py.misc.history[i], new Coord_t(i + 15, 10));
            }
        }

        // Clear the previous history strings
        public static void playerClearHistory()
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
        public static void characterGetHistory()
        {
            var py = State.Instance.py;
            var history_id = py.misc.race_id * 3 + 1;
            var social_class = randomNumber(4);

            var history_block = string.Empty;

            var background_id = 0;

            var flag = false;


            // Get a block of history text
            do
            {
                flag = false;
                while (!flag)
                {
                    if (Library.Instance.Player.character_backgrounds[background_id].chart == history_id)
                    {
                        var test_roll = randomNumber(100);

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

            playerClearHistory();

            // Process block of history text for pretty output
            var cursor_start = 0;
            var cursor_end = history_block.Length - 1;// (int)strlen(history_block) - 1;
            while (history_block[cursor_end] == ' ')
            {
                cursor_end--;
            }

            var line_number = 0;
            var new_cursor_start = 0;
            var current_cursor_position = 0;

            flag = false;
            while (!flag)
            {
                while (history_block[cursor_start] == ' ')
                {
                    cursor_start++;
                }

                current_cursor_position = cursor_end - cursor_start + 1;

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
        static void characterSetGender()
        {
            clearToBottom(20);
            putString("Choose a sex (? for Help):", new Coord_t(20, 2));
            putString("m) Male       f) Female", new Coord_t(21, 2));

            var is_set = false;
            while (!is_set)
            {
                moveCursor(new Coord_t(20, 29));

                // speed not important here
                var input = getKeyInput();

                if (input == 'f' || input == 'F')
                {
                    playerSetGender(false);
                    putString("Female", new Coord_t(4, 15));
                    is_set = true;
                }
                else if (input == 'm' || input == 'M')
                {
                    playerSetGender(true);
                    putString("Male", new Coord_t(4, 15));
                    is_set = true;
                }
                else if (input == '?')
                {
                    displayTextHelpFile(DataFilesResource.welcome /*Config.files.welcome_screen*/);
                }
                else
                {
                    terminalBellSound();
                }
            }
        }

        // Computes character's age, height, and weight -JWT-
        static void characterSetAgeHeightWeight()
        {
            var py = State.Instance.py;

            var race = Library.Instance.Player.character_races[(int)py.misc.race_id];

            py.misc.age = (uint)(race.base_age + randomNumber(race.max_age));

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

            py.misc.height = (uint)randomNumberNormalDistribution(height_base, height_mod);
            py.misc.weight = (uint)randomNumberNormalDistribution(weight_base, weight_mod);
            py.misc.disarm = race.disarm_chance_base + playerDisarmAdjustment();
        }

        // Prints the classes for a given race: Rogue, Mage, Priest, etc.,
        // shown during the character creation screens.
        public static int displayRaceClasses(uint race_id, uint[] class_list)
        {
            var coord = new Coord_t(21, 2);

            var class_id = 0;

            uint mask = 0x1;

            clearToBottom(20);
            putString("Choose a class (? for Help):", new Coord_t(20, 2));

            for (uint i = 0; i < PLAYER_MAX_CLASSES; i++)
            {
                if ((Library.Instance.Player.character_races[(int)race_id].classes_bit_field & mask) != 0u)
                {
                    var description = $"{(char)(class_id + 'a')}) {Library.Instance.Player.classes[(int)i].title}";
                    // (void)sprintf(description, "%c) %s", class_id + 'a', classes[i].title);
                    putString(description, coord);
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

        public static void generateCharacterClass(uint class_id)
        {
            var py = State.Instance.py;
            py.misc.class_id = class_id;

            var klass = Library.Instance.Player.classes[(int)py.misc.class_id];

            clearToBottom(20);
            putString(klass.title, new Coord_t(5, 15));

            // Adjust the stats for the class adjustment -RAK-
            py.stats.max[(int)PlayerAttr.STR] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.STR], klass.strength);
            py.stats.max[(int)PlayerAttr.INT] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.INT], klass.intelligence);
            py.stats.max[(int)PlayerAttr.WIS] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.WIS], klass.wisdom);
            py.stats.max[(int)PlayerAttr.DEX] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.DEX], klass.dexterity);
            py.stats.max[(int)PlayerAttr.CON] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.CON], klass.constitution);
            py.stats.max[(int)PlayerAttr.CHR] = createModifyPlayerStat(py.stats.max[(int)PlayerAttr.CHR], klass.charisma);

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
            py.misc.max_hp = (playerStatAdjustmentConstitution() + (int)py.misc.hit_die);
            py.misc.current_hp = py.misc.max_hp;
            py.misc.current_hp_fraction = 0;

            // Initialize hit_points array.
            // Put bounds on total possible hp, only succeed
            // if it is within 1/8 of average value.
            var min_value = (PLAYER_MAX_LEVEL * 3 / 8 * (py.misc.hit_die - 1)) + PLAYER_MAX_LEVEL;
            var max_value = (PLAYER_MAX_LEVEL * 5 / 8 * (py.misc.hit_die - 1)) + PLAYER_MAX_LEVEL;
            py.base_hp_levels[0] = py.misc.hit_die;

            do
            {
                for (var i = 1; i < PLAYER_MAX_LEVEL; i++)
                {
                    py.base_hp_levels[i] = (uint)randomNumber(py.misc.hit_die);
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
        public static void characterGetClass()
        {
            var py = State.Instance.py;
            var class_list = new uint[PLAYER_MAX_CLASSES];
            var class_count = displayRaceClasses(py.misc.race_id, class_list);

            // Reset the class ID
            py.misc.class_id = 0;

            var is_set = false;

            while (!is_set)
            {
                moveCursor(new Coord_t(20, 31));

                var input = getKeyInput();
                var class_id = input - 'a';

                if (class_id < class_count && class_id >= 0)
                {
                    is_set = true;
                    generateCharacterClass(class_list[class_id]);
                }
                else if (input == '?')
                {
                    displayTextHelpFile(DataFilesResource.welcome /*Config.files.welcome_screen*/);
                }
                else
                {
                    terminalBellSound();
                }
            }
        }

        // Given a stat value, return a monetary value,
        // which affects the amount of gold a player has.
        public static int monetaryValueCalculatedFromStat(uint stat)
        {
            return 5 * ((int)stat - 10);
        }

        public static void playerCalculateStartGold()
        {
            var py = State.Instance.py;

            var value = monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.STR]);
            value += monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.INT]);
            value += monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.WIS]);
            value += monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.CON]);
            value += monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.DEX]);

            // Social Class adjustment
            var new_gold = py.misc.social_class * 6 + randomNumber(25) + 325;

            // Stat adjustment
            new_gold -= value;

            // Charisma adjustment
            new_gold += monetaryValueCalculatedFromStat(py.stats.max[(int)PlayerAttr.CHR]);

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
        public static void characterCreate()
        {
            printCharacterInformation();
            characterChooseRace();
            characterSetGender();

            // here we start a loop giving a player a choice of characters -RGM-
            var done = false;
            while (!done)
            {
                characterGenerateStatsAndRace();
                characterGetHistory();
                characterSetAgeHeightWeight();
                displayCharacterHistory();
                printCharacterVitalStatistics();
                printCharacterStats();

                clearToBottom(20);
                putString("Hit space to re-roll or ESC to accept characteristics: ", new Coord_t(20, 2));

                while (true)
                {
                    var input = getKeyInput();
                    if (input == ESCAPE)
                    {
                        done = true;
                        break;
                    }
                    else if (input == ' ')
                    {
                        break;
                    }
                    else
                    {
                        terminalBellSound();
                    }
                }
            }

            characterGetClass();
            playerCalculateStartGold();
            printCharacterStats();
            printCharacterLevelExperience();
            printCharacterAbilities();
            getCharacterName();

            putStringClearToEOL("[ press any key to continue, or Q to exit ]", new Coord_t(23, 17));
            if (getKeyInput() == 'Q')
            {
                exitProgram();
            }
            eraseLine(new Coord_t(23, 0));
        }

    }
}
