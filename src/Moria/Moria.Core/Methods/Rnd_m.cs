using System;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;

using static Moria.Core.Constants.Std_c;

namespace Moria.Core.Methods
{
    public interface IRnd
    {
        int getRandomDirection();
        int randomNumber(int max);
        int randomNumber(uint max);
        int randomNumberNormalDistribution(int mean, int standard);
        int randomNumberNormalDistribution(uint mean, int standard);
        void seedsInitialize(uint seed);
        void seedSet(uint seed);
        void seedResetToOldSeed();
    }

    public class Rnd_m : IRnd
    {
        public Rnd_m(IRng rng)
        {
            this.rng = rng;
        }

        private IRng rng;

        // gets a new random seed for the random number generator
        public void seedsInitialize(uint seed)
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
            rng.setRandomSeed(clock_var);

            // make it a little more random
            for (clock_var = (uint)randomNumber(100); clock_var != 0; clock_var--)
            {
                rng.rnd();
            }
        }

        // change to different random number generator state
        public void seedSet(uint seed)
        {
            State.Instance.old_seed = rng.getRandomSeed();

            // want reproducible state here
            rng.setRandomSeed(seed);
        }

        // restore the normal random generator state
        public void seedResetToOldSeed()
        {
            rng.setRandomSeed(State.Instance.old_seed);
        }

        public int randomNumber(uint max) => randomNumber((int)max);

        // Generates a random integer x where 1<=X<=MAXVAL -RAK-
        public int randomNumber(int max)
        {
            return (rng.rnd() % max) + 1;
        }

        public int randomNumberNormalDistribution(uint mean, int standard) =>
            randomNumberNormalDistribution((int)mean, standard);

        // Generates a random integer number of NORMAL distribution -RAK-
        public int randomNumberNormalDistribution(int mean, int standard)
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
            var iindex = (int)Game_c.NORMAL_TABLE_SIZE >> 1;
            var high = (int)Game_c.NORMAL_TABLE_SIZE;
            var normal_table = Library.Instance.Tables.normal_table;

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
            offset = ((standard * iindex) + ((int)Game_c.NORMAL_TABLE_SD >> 1)) / (int)Game_c.NORMAL_TABLE_SD;

            // one half should be negative
            if (randomNumber(2) == 1)
            {
                offset = -offset;
            }

            return mean + offset;
        }

        public int getRandomDirection()
        {
            int dir;

            do
            {
                dir = randomNumber(9);
            } while (dir == 5);

            return dir;
        }
    }
}