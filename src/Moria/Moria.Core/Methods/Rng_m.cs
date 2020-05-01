namespace Moria.Core.Methods
{
    public interface IRng
    {
        uint getRandomSeed();
        void setRandomSeed(uint seed);
        int rnd();
    }

    public class Rng_m : IRng
    {
        private const int RNG_M = int.MaxValue;
        private const int RNG_A = 16807;
        private const int RNG_Q = RNG_M / RNG_A;
        private const int RNG_R = RNG_M % RNG_A;

        private uint rnd_seed;

        public uint getRandomSeed() => this.rnd_seed;

        public void setRandomSeed(uint seed)
        {
            // set seed to value between 1 and m-1
            this.rnd_seed = (seed % (RNG_M - 1)) + 1;
        }

        // returns a pseudo-random number from set 1, 2, ..., RNG_M - 1
        public int rnd()
        {
            var high = (int)(this.rnd_seed / RNG_Q);
            var low = (int)(this.rnd_seed % RNG_Q);
            var test = (int)(RNG_A * low - RNG_R * high);

            if (test > 0)
            {
                this.rnd_seed = (uint)test;
            }
            else
            {
                this.rnd_seed = (uint)(test + RNG_M);
            }
            return (int) this.rnd_seed;
        }
    }
}