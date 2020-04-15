namespace Moria.Core.Methods
{
    public static class Helpers_m
    {
        // Returns position of first set bit and clears that bit -RAK-
        public static int getAndClearFirstBit(ref uint flag)
        {
            uint mask = 0x1;

            for (int i = 0; i < 32 /* sizeof(int) */; i++)
            {
                if ((flag & mask) != 0u)
                {
                    flag &= ~mask;
                    return i;
                }
                mask <<= 1;
            }

            // no one bits found
            return -1;
        }

        public static bool isVowel(char ch)
        {
            switch (ch)
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                case 'A':
                case 'E':
                case 'I':
                case 'O':
                case 'U':
                    return true;
                default:
                    return false;
            }
        }
    }
}
