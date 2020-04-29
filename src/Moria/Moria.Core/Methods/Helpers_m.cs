﻿using System;

namespace Moria.Core.Methods
{
    public interface IHelpers
    {
        int getAndClearFirstBit(ref uint flag);
        bool isVowel(char ch);
        bool stringToNumber(string str, out int number);
        void insertNumberIntoString(ref string to_string, string from_string, int number, bool show_sign);
        void insertStringIntoString(ref string to_string, string from_string, string str_to_insert);
        void humanDateString(out string day);
    }

    public class Helpers_m : IHelpers
    {
        // Returns position of first set bit and clears that bit -RAK-
        public int getAndClearFirstBit(ref uint flag)
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
        public void insertNumberIntoString(ref string to_string, string from_string, int number, bool show_sign)
        {
            var replacement = $"{number:d}";
            if (show_sign && number >= 0)
            {
                replacement = $"+{number:d}";
            }

            to_string = to_string.Replace(from_string, replacement);
        }

        // Inserts a string into a string
        public void insertStringIntoString(ref string to_string, string from_string, string str_to_insert)
        {
        }

        public bool isVowel(char ch)
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
        public bool stringToNumber(string str, out int number)
        {
            return int.TryParse(str, out number);
        }

        public void humanDateString(out string day)
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
