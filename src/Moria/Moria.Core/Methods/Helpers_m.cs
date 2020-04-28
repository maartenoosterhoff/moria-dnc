using System;

namespace Moria.Core.Methods
{
    public static class Helpers_m
    {
        // Returns position of first set bit and clears that bit -RAK-
        public static int getAndClearFirstBit(ref uint flag)
        {
            uint mask = 0x1;

            for (var i = 0; i < 32 /* sizeof(int) */; i++)
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

        // Insert a long number into a string (was `insert_lnum()` function)
        public static void insertNumberIntoString(ref string to_string, string from_string, int number, bool show_sign)
        {
            throw new NotImplementedException();
        }

        // Inserts a string into a string
        public static void insertStringIntoString(ref string to_string, string from_string, string str_to_insert)
        {
            throw new NotImplementedException();
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

        // http://rus.har.mn/blog/2014-05-19/strtol-error-checking/
        public static bool stringToNumber(string str, out int number)
        {
            return int.TryParse(str, out number);
        }

        public static void humanDateString(out string day)
        {
            /*
    time_t now = time(nullptr);
    struct tm *datetime = localtime(&now);

#ifdef _WIN32
    strftime(day, 11, "%a %b %d", datetime);
#else
    strftime(day, 11, "%a %b %e", datetime);
#endif
*/
            var now = DateTime.Now;
            // a = writes abbreviated weekday name, e.g. Fri (locale dependent)
            // b = writes abbreviated month name, e.g. Oct (locale dependent)
            // d = writes day of the month as a decimal number (range [01,31])

            day = now.ToString("ddd MMM d");
        }
    }
}
