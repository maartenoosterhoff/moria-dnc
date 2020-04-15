namespace Moria.Core.Methods
{
    public static class Rng_m
    {
        private const int RNG_M = int.MaxValue;
        private const int RNG_A = 16807;
        private const int RNG_Q = RNG_M / RNG_A;
        private const int RNG_R = RNG_M % RNG_A;

        private static uint rnd_seed;

        public static uint getRandomSeed() => rnd_seed;

        public static void setRandomSeed(uint seed)
        {
            // set seed to value between 1 and m-1
            rnd_seed = (seed % (RNG_M - 1)) + 1;
        }

        // returns a pseudo-random number from set 1, 2, ..., RNG_M - 1
        public static int rnd()
        {
            var high = (int)(rnd_seed / RNG_Q);
            var low = (int)(rnd_seed % RNG_Q);
            var test = (int)(RNG_A * low - RNG_R * high);

            if (test > 0)
            {
                rnd_seed = (uint)test;
            }
            else
            {
                rnd_seed = (uint)(test + RNG_M);
            }
            return (int)rnd_seed;
        }
    }
}