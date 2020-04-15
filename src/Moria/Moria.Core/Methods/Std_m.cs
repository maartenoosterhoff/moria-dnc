namespace Moria.Core.Methods
{
    public static class Std_m
    {
        public static int std_abs(int value)
        {
            if (value >= 0)
            {
                return value;
            }

            return -1 * value;
        }

        public static int std_intmax_t(int value)
        {
            return value;
        }
    }
}
