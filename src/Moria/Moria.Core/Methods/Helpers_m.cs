using System;
using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures;

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

        bool movePosition(int dir, ref Coord_t coord);

        bool playerNoLight();

        bool coordInsidePanel(Coord_t coord);

        bool coordOutsidePanel(Coord_t coord, bool force);
    }

    public class Helpers_m : IHelpers
    {
        private readonly IEventPublisher eventPublisher;

        public Helpers_m(
            IEventPublisher eventPublisher
        )
        {
            this.eventPublisher = eventPublisher;
        }

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

        // Given direction "dir", returns new row, column location -RAK-
        public bool movePosition(int dir, ref Coord_t coord)
        {
            var dg = State.Instance.dg;

            var new_coord = new Coord_t(0, 0);

            switch (dir)
            {
                case 1:
                    new_coord.y = coord.y + 1;
                    new_coord.x = coord.x - 1;
                    break;
                case 2:
                    new_coord.y = coord.y + 1;
                    new_coord.x = coord.x;
                    break;
                case 3:
                    new_coord.y = coord.y + 1;
                    new_coord.x = coord.x + 1;
                    break;
                case 4:
                    new_coord.y = coord.y;
                    new_coord.x = coord.x - 1;
                    break;
                case 5:
                    new_coord.y = coord.y;
                    new_coord.x = coord.x;
                    break;
                case 6:
                    new_coord.y = coord.y;
                    new_coord.x = coord.x + 1;
                    break;
                case 7:
                    new_coord.y = coord.y - 1;
                    new_coord.x = coord.x - 1;
                    break;
                case 8:
                    new_coord.y = coord.y - 1;
                    new_coord.x = coord.x;
                    break;
                case 9:
                    new_coord.y = coord.y - 1;
                    new_coord.x = coord.x + 1;
                    break;
                default:
                    new_coord.y = 0;
                    new_coord.x = 0;
                    break;
            }

            var can_move = false;

            if (new_coord.y >= 0 && new_coord.y < dg.height && new_coord.x >= 0 && new_coord.x < dg.width)
            {
                coord = new_coord;
                can_move = true;
            }

            return can_move;
        }

        // Returns true if player has no light -RAK-
        public bool playerNoLight()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            return !dg.floor[py.pos.y][py.pos.x].temporary_light && !dg.floor[py.pos.y][py.pos.x].permanent_light;
        }

        // Is the given coordinate within the screen panel boundaries -RAK-
        public bool coordInsidePanel(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var valid_y = coord.y >= dg.panel.top && coord.y <= dg.panel.bottom;
            var valid_x = coord.x >= dg.panel.left && coord.x <= dg.panel.right;

            return valid_y && valid_x;
        }

        // Calculates current boundaries -RAK-
        private void panelBounds()
        {
            var dg = State.Instance.dg;
            dg.panel.top = dg.panel.row * ((int)Dungeon_c.SCREEN_HEIGHT / 2);
            dg.panel.bottom = dg.panel.top + (int)Dungeon_c.SCREEN_HEIGHT - 1;
            dg.panel.row_prt = dg.panel.top - 1;
            dg.panel.left = dg.panel.col * ((int)Dungeon_c.SCREEN_WIDTH / 2);
            dg.panel.right = dg.panel.left + (int)Dungeon_c.SCREEN_WIDTH - 1;
            dg.panel.col_prt = dg.panel.left - 13;
        }

        // Given an row (y) and col (x), this routine detects -RAK-
        // when a move off the screen has occurred and figures new borders.
        // `force` forces the panel bounds to be recalculated, useful for 'W'here.
        public bool coordOutsidePanel(Coord_t coord, bool force)
        {
            var dg = State.Instance.dg;
            var panel = new Coord_t(dg.panel.row, dg.panel.col);

            if (force || coord.y < dg.panel.top + 2 || coord.y > dg.panel.bottom - 2)
            {
                panel.y = (coord.y - (int)Dungeon_c.SCREEN_HEIGHT / 4) / ((int)Dungeon_c.SCREEN_HEIGHT / 2);

                if (panel.y > dg.panel.max_rows)
                {
                    panel.y = dg.panel.max_rows;
                }
                else if (panel.y < 0)
                {
                    panel.y = 0;
                }
            }

            if (force || coord.x < dg.panel.left + 3 || coord.x > dg.panel.right - 3)
            {
                panel.x = (coord.x - (int)Dungeon_c.SCREEN_WIDTH / 4) / ((int)Dungeon_c.SCREEN_WIDTH / 2);
                if (panel.x > dg.panel.max_cols)
                {
                    panel.x = dg.panel.max_cols;
                }
                else if (panel.x < 0)
                {
                    panel.x = 0;
                }
            }

            if (panel.y != dg.panel.row || panel.x != dg.panel.col)
            {
                dg.panel.row = panel.y;
                dg.panel.col = panel.x;
                this.panelBounds();

                // stop movement if any
                if (Config.options.find_bound)
                {
                    this.eventPublisher.Publish(new EndRunningCommand());
                    //playerEndRunning();
                }

                // Yes, the coordinates are beyond the current panel boundary
                return true;
            }

            return false;
        }
    }
}
