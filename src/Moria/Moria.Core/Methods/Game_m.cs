using System;
using Moria.Core.States;
using static Moria.Core.Methods.Rng_m;
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

        public static int randomNumber(uint max) => randomNumber((int) max);

        // Generates a random integer x where 1<=X<=MAXVAL -RAK-
        public static int randomNumber(int max)
        {
            return (rnd() % max) + 1;
        }

        public const int SHRT_MAX = 32767;

        public static int randomNumberNormalDistribution(uint mean, int standard) =>
            randomNumberNormalDistribution((int) mean, standard);

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

        // Restore the terminal and exit
        public static void exitProgram()
        {
            flushInputBuffer();
            terminalRestore();
            exit(0);
        }
    }
}
