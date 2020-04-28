namespace Moria.Core.Methods
{
    public interface IStd
    {
        int std_abs(int value);
        int std_intmax_t(int value);
    }

    public class Std_m : IStd
    {
        public int std_abs(int value)
        {
            if (value >= 0)
            {
                return value;
            }

            return -1 * value;
        }

        public int std_intmax_t(int value)
        {
            return value;
        }
    }
}
