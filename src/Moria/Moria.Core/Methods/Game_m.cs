using System;
using System.Net.Mime;
using Moria.Core.Configs;
using Moria.Core.States;
using static Moria.Core.Methods.Rng_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Constants.Game_c;
using static Moria.Core.Data.Tables_d;

namespace Moria.Core.Methods
{
    public static class Game_m
    {
        // gets a new random seed for the random number generator
        public static void seedsInitialize(uint seed)
        {
            uint clock_var;

            if (seed == 0)
            {
                clock_var = (uint)(DateTime.Now.Ticks % int.MaxValue);
            }
            else
            {
                clock_var = seed;
            }

            State.Instance.game.magic_seed = clock_var;

            clock_var += 8762;
            State.Instance.game.town_seed = clock_var;

            clock_var += 113452;
            setRandomSeed(clock_var);

            // make it a little more random
            for (clock_var = (uint)randomNumber(100); clock_var != 0; clock_var--)
            {
                rnd();
            }
        }

        // change to different random number generator state
        public static void seedSet(uint seed)
        {
            State.Instance.old_seed = getRandomSeed();

            // want reproducible state here
            setRandomSeed(seed);
        }

        // restore the normal random generator state
        public static void seedResetToOldSeed()
        {
            setRandomSeed(State.Instance.old_seed);
        }

        public static int randomNumber(uint max) => randomNumber((int)max);

        // Generates a random integer x where 1<=X<=MAXVAL -RAK-
        public static int randomNumber(int max)
        {
            return (rnd() % max) + 1;
        }

        public const int SHRT_MAX = 32767;

        public static int randomNumberNormalDistribution(uint mean, int standard) =>
            randomNumberNormalDistribution((int)mean, standard);

        // Generates a random integer number of NORMAL distribution -RAK-
        public static int randomNumberNormalDistribution(int mean, int standard)
        {
            // alternate randomNumberNormalDistribution() code, slower but much smaller since no table
            // 2 per 1,000,000 will be > 4*SD, max is 5*SD
            //
            // tmp = diceRoll(8, 99);             // mean 400, SD 81
            // tmp = (tmp - 400) * standard / 81;
            // return tmp + mean;

            var tmp = randomNumber(SHRT_MAX);
            int offset;

            // off scale, assign random value between 4 and 5 times SD
            if (tmp == SHRT_MAX)
            {
                offset = 4 * standard + randomNumber(standard);

                // one half are negative
                if (randomNumber(2) == 1)
                {
                    offset = -offset;
                }

                return mean + offset;
            }

            // binary search normal normal_table to get index that
            // matches tmp this takes up to 8 iterations.
            var low = 0;
            var iindex = (int)NORMAL_TABLE_SIZE >> 1;
            var high = (int)NORMAL_TABLE_SIZE;

            while (true)
            {
                if (normal_table[iindex] == tmp || high == low + 1)
                {
                    break;
                }

                if (normal_table[iindex] > tmp)
                {
                    high = iindex;
                    iindex = low + ((iindex - low) >> 1);
                }
                else
                {
                    low = iindex;
                    iindex = iindex + ((high - iindex) >> 1);
                }
            }

            // might end up one below target, check that here
            if (normal_table[iindex] < tmp)
            {
                iindex = iindex + 1;
            }

            // normal_table is based on SD of 64, so adjust the
            // index value here, round the half way case up.
            offset = ((standard * iindex) + ((int)NORMAL_TABLE_SD >> 1)) / (int)NORMAL_TABLE_SD;

            // one half should be negative
            if (randomNumber(2) == 1)
            {
                offset = -offset;
            }

            return mean + offset;
        }

        public static int getRandomDirection()
        {
            int dir;

            do
            {
                dir = randomNumber(9);
            } while (dir == 5);

            return dir;
        }

        // map roguelike direction commands into numbers
        static char mapRoguelikeKeysToKeypad(char command)
        {
            switch (command)
            {
                case 'h':
                    return '4';
                case 'y':
                    return '7';
                case 'k':
                    return '8';
                case 'u':
                    return '9';
                case 'l':
                    return '6';
                case 'n':
                    return '3';
                case 'j':
                    return '2';
                case 'b':
                    return '1';
                case '.':
                    return '5';
                default:
                    return command;
            }
        }

        // Prompts for a direction -RAK-
        // Direction memory added, for repeated commands.  -CJS
        public static bool getDirectionWithMemory(string prompt, ref int direction)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            // used in counted commands. -CJS-
            if (game.use_last_direction)
            {
                direction = py.prev_dir;
                return true;
            }

            if (string.IsNullOrEmpty(prompt))
            {
                prompt = "Which direction?";
            }

            char command = '\0';

            while (true)
            {
                // Don't end a counted command. -CJS-
                int save = game.command_count;

                if (!getCommand(prompt, out command))
                {
                    game.player_free_turn = true;
                    return false;
                }

                game.command_count = save;

                if (Config.options.use_roguelike_keys)
                {
                    command = mapRoguelikeKeysToKeypad(command);
                }

                if (command >= '1' && command <= '9' && command != '5')
                {
                    py.prev_dir = (char)(command - '0');
                    direction = py.prev_dir;
                    return true;
                }

                terminalBellSound();
            }
        }

        // Similar to getDirectionWithMemory(), except that no memory exists,
        // and it is allowed to enter the null direction. -CJS-
        public static bool getAllDirections(string prompt, ref int direction)
        {
            var game = State.Instance.game;
            char command = '\0';

            while (true)
            {
                if (!getCommand(prompt, out command))
                {
                    game.player_free_turn = true;
                    return false;
                }

                if (Config.options.use_roguelike_keys)
                {
                    command = mapRoguelikeKeysToKeypad(command);
                }

                if (command >= '1' && command <= '9')
                {
                    direction = command - '0';
                    return true;
                }

                terminalBellSound();
            }
        }

        // Restore the terminal and exit
        public static void exitProgram()
        {
            flushInputBuffer();
            terminalRestore();

            throw new MoriaExitRequestedException();
        }

        //// Abort the program with a message displayed on the terminal.
        //public static void abortProgram(string msg)
        //{
        //    flushInputBuffer();
        //    terminalRestore();
        //
        //    printf("Program was manually aborted with the message:\n");
        //    printf("%s\n", msg);
        //
        //    exit(0);
        //}
    }
}
