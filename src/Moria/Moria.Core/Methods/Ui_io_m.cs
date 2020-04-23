using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Constants.Types_c;
using static Moria.Core.Configs.Config;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Game_save_m;
using static Moria.Core.Methods.Game_death_m;
using static Moria.Core.Methods.Player_m;

namespace Moria.Core.Methods
{
    public static class Ui_io_m
    {
        public static bool isprint(char c)
        {
            const string keys = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ";
            const string specialKeys = @"!""#$%&'()*+,-./:;<=>?@[\]^_`{|}~";
            return keys.IndexOf(c) >= 0 || specialKeys.IndexOf(c) >= 0;
        }

        public static void mvcur(int y, int x) => move(y, x);
        public static void initscr() { }
        public static void refresh()
        {
            // Do... nothing?
        }

        public static bool getch(out char c) => getch(out c, TimeSpan.FromMilliseconds(10));

        public static bool getch(out char c, TimeSpan timeout)
        {
            var duration = TimeSpan.Zero;
            while (!Console.KeyAvailable && duration <= timeout)
            {
                var ts = TimeSpan.FromMilliseconds(10);
                Thread.Sleep(ts);
                duration += ts;
            }

            if (!Console.KeyAvailable)
            {
                c = '\0';
                return false;
            }
            var input = Console.ReadKey(true);
            if ((input.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                c = CTRL_KEY(input.KeyChar);
            }

            c = input.KeyChar;
            return true;
        }

        public static void move(int y, int x)
        {
            Console.SetCursorPosition(y, x);
        }

        public static void mvaddch(int y, int x, char c)
        {
            Console.SetCursorPosition(y, x);
            Console.Write(c);
        }

        public static void mvaddstr(int y, int x, string s)
        {
            Console.SetCursorPosition(y, x);
            Console.Write(s);
        }

        public static void addch(char c)
        {
            Console.Write(c);
        }

        public static void addstr(string s)
        {
            Console.Write(s);
        }

        public static void getyx(out int y, out int x)
        {
            y = Console.CursorLeft;
            x = Console.CursorTop;
        }

        public static void clear() => Console.Clear();

        public static void clrtoeol()
        {
            // TODO;
        }

        public static void moriaTerminalInitialize()
        {
            Console.TreatControlCAsInput = true;

            // this was commented
            // cbreak();           // <curses.h> use raw() instead as it disables Ctrl chars
            
            // this is done with treat-control-c-as-input=true
            //raw();                 // <curses.h> disable control characters. I.e. Ctrl-C does not work!

            // this is done by using console-readkey(true)
            //noecho();              // <curses.h> do not echo typed characters
            
            // TOFIX
            //nonl();                // <curses.h> disable translation return/newline for detection of return key

            //keypad(stdscr, false); // <curses.h> disable keypad input as we handle that ourselves
            //                       // curs_set(0);        // <curses.h> sets the appearance of the cursor based on the value of visibility
            //curses_on = true;
        }

        // initializes the terminal / curses routines
        public static bool terminalInitialize()
        {
            initscr();

            // Check we have enough screen. -CJS-
            if (Console.WindowHeight < 24 || Console.WindowWidth < 80)
            //if (LINES < 24 || COLS < 80)
            {
                Console.WriteLine("Screen too small for moria.");
                //(void)printf("Screen too small for moria.\n");
                return false;
            }

            //save_screen = newwin(0, 0, 0, 0);
            //if (save_screen == nullptr)
            //{
            //    (void)printf("Out of memory in starting up curses.\n");
            //    return false;
            //}

            moriaTerminalInitialize();

            clear();
            refresh();

            return true;
        }

        // Put the terminal in the original mode. -CJS-
        public static void terminalRestore()
        {
            //if (!curses_on)
            //{
            //    return;
            //}

            // Dump any remaining buffer
            putQIO();

            // this moves curses to bottom right corner
            int y = 0;
            int x = 0;
            getyx(out y, out x);
            //getyx(stdscr, y, x);
            //mvcur(y, x, Console.WindowHeight - 1, 0);

            //// exit curses
            //endwin();
            //(void)fflush(stdout);
            //
            //curses_on = false;
        }

        public static void terminalSaveScreen()
        {
            Console.
            overwrite(stdscr, save_screen);
        }

        public static void terminalRestoreScreen()
        {
            overwrite(save_screen, stdscr);
            touchwin(stdscr);
        }

        public static int terminalBellSound()
        {
            putQIO();

            // The player can turn off beeps if they find them annoying.
            if (Config.options.error_beep_sound)
            {
                Console.Beep();
                //return write(1, "\007", 1);
                return 1;
            }

            return 0;
        }

        // Dump the IO buffer to terminal -RAK-
        public static void putQIO()
        {
            // Let inventoryExecuteCommand() know something has changed.
            State.Instance.screen_has_changed = true;

            refresh();
        }

        // Flush the buffer -RAK-
        public static void flushInputBuffer()
        {
            if (eof_flag != 0)
            {
                return;
            }

            while (checkForNonBlockingKeyPress(0))
            {
            }
        }

        // Clears screen
        public static void clearScreen()
        {
            if (State.Instance.message_ready_to_print)
            {
                printMessage(/*CNIL*/null);
            }
            clear();
        }

        public static void clearLine(int row)
        {
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, row);
        }

        public static void clearToBottom(int row)
        {
            while (row <= Console.WindowHeight)
            {
                clearLine(row);
            }

            move(row, 0);

            //clrtobot(); // see above
        }

        // move cursor to a given y, x position
        public static void moveCursor(Coord_t coord)
        {
            //Console.SetCursorPosition(coord.y, coord.x);
            move(coord.y, coord.x);
        }

        public static void addChar(char ch, Coord_t coord)
        {
            mvaddch(coord.y, coord.x, ch);
            //if (mvaddch(coord.y, coord.x, ch) == ERR)
            //{
            //    abort();
            //}
        }

        // Dump IO to buffer -RAK-
        public static void putString(string out_str, Coord_t coord)
        {
            // truncate the string, to make sure that it won't go past right edge of screen.
            if (coord.x > 79)
            {
                coord.x = 79;
            }

            //vtype_t str = { '\0' };
            var str = out_str.Substring(0, 79 - coord.x);
            //(void)strncpy(str, out_str, (size_t)(79 - coord.x));
            //str[79 - coord.x] = '\0';

            mvaddstr(coord.y, coord.x, str);
            //if (mvaddstr(coord.y, coord.x, str) == ERR)
            //{
            //    abort();
            //}
        }

        // Outputs a line to a given y, x position -RAK-
        public static void putStringClearToEOL(string str, Coord_t coord)
        {
            if (coord.y == MSG_LINE && State.Instance.message_ready_to_print)
            {
                printMessage(/*CNIL*/ null);
            }

            move(coord.y, coord.x);
            clrtoeol();
            putString(str, coord);
        }

        // Clears given line of text -RAK-
        public static void eraseLine(Coord_t coord)
        {
            if (coord.y == MSG_LINE && State.Instance.message_ready_to_print)
            {
                printMessage(/*CNIL*/null);
            }

            move(coord.y, coord.x);
            clrtoeol();
        }

        // Moves the cursor to a given interpolated y, x position -RAK-
        public static void panelMoveCursor(Coord_t coord)
        {
            var dg = State.Instance.dg;

            // Real coords convert to screen positions
            coord.y -= dg.panel.row_prt;
            coord.x -= dg.panel.col_prt;

            move(coord.y, coord.x);
            //if (move(coord.y, coord.x) == ERR)
            //{
            //    abort();
            //}
        }

        // Outputs a char to a given interpolated y, x position -RAK-
        // sign bit of a character used to indicate standout mode. -CJS
        public static void panelPutTile(char ch, Coord_t coord)
        {
            var dg = State.Instance.dg;

            // Real coords convert to screen positions
            coord.y -= dg.panel.row_prt;
            coord.x -= dg.panel.col_prt;

            mvaddch(coord.y, coord.x, ch);
            //if (mvaddch(coord.y, coord.x, ch) == ERR)
            //{
            //    abort();
            //}
        }

        public static Coord_t currentCursorPosition()
        {
            int y, x;
            getyx(out y, out x);
            //getyx(stdscr, y, x);
            return new Coord_t(y, x);
        }

        // messageLinePrintMessage will print a line of text to the message line (0,0).
        // first clearing the line of any text!
        public static void messageLinePrintMessage(string message)
        {
            // save current cursor position
            Coord_t coord = currentCursorPosition();

            // move to beginning of message line, and clear it
            move(0, 0);
            clrtoeol();

            // truncate message if it's too long!
            //message.resize(79);
            message = message.Substring(0, 79);

            addstr(message);

            // restore cursor to old position
            move(coord.y, coord.x);
        }

        // deleteMessageLine will delete all text from the message line (0,0).
        // The current cursor position will be maintained.
        public static void messageLineClear()
        {
            // save current cursor position
            Coord_t coord = currentCursorPosition();

            // move to beginning of message line, and clear it
            move(0, 0);
            clrtoeol();

            // restore cursor to old position
            move(coord.y, coord.x);
        }

        // Outputs message to top line of screen
        // These messages are kept for later reference.
        public static void printMessage(string msg)
        {
            int new_len = 0;
            int old_len = 0;
            bool combine_messages = false;

            if (State.Instance.message_ready_to_print)
            {
                old_len = State.Instance.messages[State.Instance.last_message_id].Length + 1;
                //old_len = (int)strlen(State.Instance.messages[State.Instance.last_message_id]) + 1;

                // If the new message and the old message are short enough,
                // we want display them together on the same line.  So we
                // don't flush the old message in this case.

                if (!string.IsNullOrEmpty(msg))
                {
                    new_len = msg.Length;
                    //new_len = (int)strlen(msg);
                }
                else
                {
                    new_len = 0;
                }

                if ((string.IsNullOrEmpty(msg)) || new_len + old_len + 2 >= 73)
                {
                    // ensure that the complete -more- message is visible.
                    if (old_len > 73)
                    {
                        old_len = 73;
                    }

                    putString(" -more-", new Coord_t((int)MSG_LINE, old_len));

                    char in_char;
                    do
                    {
                        in_char = getKeyInput();
                    } while ((in_char != ' ') && (in_char != ESCAPE) && (in_char != '\n') && (in_char != '\r'));
                }
                else
                {
                    combine_messages = true;
                }
            }

            if (!combine_messages)
            {
                move((int)MSG_LINE, 0);
                clrtoeol();
            }

            // Make the null string a special case. -CJS-

            if (string.IsNullOrEmpty(msg))
            {
                State.Instance.message_ready_to_print = false;
                return;
            }

            var game = State.Instance.game;
            game.command_count = 0;
            State.Instance.message_ready_to_print = true;

            // If the new message and the old message are short enough,
            // display them on the same line.

            if (combine_messages)
            {
                putString(msg, new Coord_t((int)MSG_LINE, old_len + 2));
                //strcat(State.Instance.messages[State.Instance.last_message_id], "  ");
                //strcat(State.Instance.messages[State.Instance.last_message_id], msg);
                State.Instance.messages[State.Instance.last_message_id] += "  ";
                State.Instance.messages[State.Instance.last_message_id] += msg;

            }
            else
            {
                messageLinePrintMessage(msg);
                State.Instance.last_message_id++;

                if (State.Instance.last_message_id >= MESSAGE_HISTORY_SIZE)
                {
                    State.Instance.last_message_id = 0;
                }

                State.Instance.messages[State.Instance.last_message_id] = msg.Substring(0, (int)MORIA_MESSAGE_SIZE);
                //State.Instance.messages[State.Instance.last_message_id][MORIA_MESSAGE_SIZE - 1] = '\0';
            }
        }

        // Print a message so as not to interrupt a counted command. -CJS-
        public static void printMessageNoCommandInterrupt(string msg)
        {
            var game = State.Instance.game;

            // Save command count value
            int i = game.command_count;

            printMessage(msg);

            // Restore count value
            game.command_count = i;
        }

        // Returns a single character input from the terminal. -CJS-
        //
        // This silently consumes ^R to redraw the screen and reset the
        // terminal, so that this operation can always be performed at
        // any input prompt. getKeyInput() never returns ^R.
        public static char getKeyInput()
        {
            var game = State.Instance.game;

            putQIO();               // Dump IO buffer
            game.command_count = 0; // Just to be safe -CJS-

            while (true)
            {
                getch(out char ch);
                //int ch = getch();

                // some machines may not sign extend.
                if (ch == EOF)
                {
                    // avoid infinite loops while trying to call getKeyInput() for a -more- prompt.
                    State.Instance.message_ready_to_print = false;

                    eof_flag++;

                    refresh();

                    if (!game.character_generated || game.character_saved)
                    {
                        endGame();
                    }

                    playerDisturb(1, 0);

                    if (eof_flag > 100)
                    {
                        // just in case, to make sure that the process eventually dies
                        State.Instance.panic_save = true;

                        game.character_died_from = "(end of input: panic saved)";
                        //(void)strcpy(game.character_died_from, "(end of input: panic saved)");
                        if (!saveGame())
                        {
                            game.character_died_from = "panic: unexpected eof";
                            //(void)strcpy(game.character_died_from, "panic: unexpected eof");
                            game.character_is_dead = true;
                        }
                        endGame();
                    }
                    return ESCAPE;
                }

                if (ch != CTRL_KEY_R)
                {
                    return (char)ch;
                }

                //(void)wrefresh(curscr); // TOMAYBE
                moriaTerminalInitialize();
            }
        }

        // Prompts (optional) and returns ord value of input char
        // Function returns false if <ESCAPE> is input
        public static bool getCommand(string prompt, out char command)
        {
            if (!string.IsNullOrEmpty(prompt))
            {
                putStringClearToEOL(prompt, new Coord_t(0, 0));
            }
            command = getKeyInput();

            messageLineClear();

            return command != ESCAPE;
        }

        // Gets a string terminated by <RETURN>
        // Function returns false if <ESCAPE> is input
        public static bool getStringInput(out string in_str, Coord_t coord, int slen)
        {
            var in_str_value = "";
            Console.SetCursorPosition(coord.y, coord.x);
            //(void)move(coord.y, coord.x);

            for (int i = slen; i > 0; i--)
            {
                addch(' ');
            }

            Console.SetCursorPosition(coord.y, coord.x);
            //(void)move(coord.y, coord.x);

            int start_col = coord.x;
            int end_col = coord.x + slen - 1;

            if (end_col > 79)
            {
                end_col = 79;
            }

            char* p = in_str;

            bool flag = false;
            bool aborted = false;

            while (!flag && !aborted)
            {
                int key = getKeyInput();
                switch (key)
                {
                    case ESCAPE:
                        aborted = true;
                        break;
                    case CTRL_KEY_J:
                    case CTRL_KEY_M:
                        flag = true;
                        break;
                    case DELETE:
                    case CTRL_KEY_H:
                        if (coord.x > start_col)
                        {
                            coord.x--;
                            putString(" ", coord);
                            moveCursor(coord);
                            *--p = '\0';
                        }
                        break;
                    default:
                        if ((!isprint((char)key)) || coord.x > end_col)
                        {
                            terminalBellSound();
                        }
                        else
                        {
                            mvaddch(coord.y, coord.x, (char)key);
                            *p++ = (char)key;
                            coord.x++;
                        }
                        break;
                }
            }

            if (aborted)
            {
                in_str = null;
                return false;
            }

            in_str_value = in_str_value.TrimEnd();
            //// Remove trailing blanks
            //while (p > in_str && p[-1] == ' ')
            //{
            //    p--;
            //}
            //*p = '\0';

            in_str = in_str_value;
            return true;
        }

        // Used to verify a choice - user gets the chance to abort choice. -CJS-
        public static bool getInputConfirmation(string prompt)
        {
            putStringClearToEOL(prompt, new Coord_t(0, 0));

            int y, x;
            getyx(out y, out x);
            //getyx(stdscr, y, x);

            if (x > 73)
            {
                move(0, 73);
            }
            else if (y != 0)
            {
                // use `y` to prevent compiler warning.
            }

            addstr(" [y/n]");

            char input = ' ';
            while (input == ' ')
            {
                input = getKeyInput();
            }

            messageLineClear();

            return (input == 'Y' || input == 'y');
        }

        // Pauses for user response before returning -RAK-
        public static void waitForContinueKey(int line_number)
        {
            putStringClearToEOL("[ press any key to continue ]", new Coord_t(line_number, 23));
            getKeyInput();
            eraseLine(new Coord_t(line_number, 0));
        }

        // Provides for a timeout on input. Does a non-blocking read, consuming the data if
        // any, and then returns 1 if data was read, zero otherwise.
        //
        // Porting:
        //
        // In systems without the select call, but with a sleep for fractional numbers of
        // seconds, one could sleep for the time and then check for input.
        //
        // In systems which can only sleep for whole number of seconds, you might sleep by
        // writing a lot of nulls to the terminal, and waiting for them to drain, or you
        // might hack a static accumulation of times to wait. When the accumulation reaches
        // a certain point, sleep for a second. There would need to be a way of resetting
        // the count, with a call made for commands like run or rest.
        public static bool checkForNonBlockingKeyPress(int microseconds)
        {
            ////# ifdef _WIN32
            //(void)microseconds;
            //
            //// Ugly non-blocking read...Ugh! -MRC-
            //timeout(8);
            return getch(out var result);
            //int result = getch();
            //timeout(-1);

            //return result > 0;
            //#else
            //    struct timeval tbuf {};
            //    int ch;
            //    int smask;

            //    // Return true if a read on descriptor 1 will not block.
            //    tbuf.tv_sec = 0;
            //    tbuf.tv_usec = microseconds;

            //    smask = 1; // i.e. (1 << 0)
            //    if (select(1, (fd_set*) &smask, (fd_set*) nullptr, (fd_set *) nullptr, &tbuf) == 1) {
            //        ch = getch();
            //        // check for EOF errors here, select sometimes works even when EOF
            //        if (ch == -1) {
            //            eof_flag++;
            //            return false;
            //        }
            //        return true;
            //    }

            //    return false;
            //#endif
        }

        // Find a default user name from the system.
        public static void getDefaultPlayerName(out string buffer)
        {
            buffer = Environment.UserName;
            //// Gotta have some name
            //const char* default_name = "X";

            //# ifdef _WIN32
            //            unsigned long bufCharCount = PLAYER_NAME_SIZE;

            //            if (!GetUserName(buffer, &bufCharCount))
            //            {
            //                (void)strcpy(buffer, defaultName);
            //            }
            //#else
            //            char* p = getlogin();

            //            if ((p != nullptr) && (p[0] != 0))
            //            {
            //                (void)strcpy(buffer, p);
            //            }
            //            else
            //            {
            //        struct passwd * pwline = getpwuid((int) getuid());
            //        if (pwline != nullptr) {
            //            (void) strcpy(buffer, pwline->pw_name);
            //    }
            //}

            //    if (buffer[0] == 0) {
            //        (void) strcpy(buffer, default_name);
            //    }
            //#endif
        }

        //# ifndef _WIN32
        //        // On unix based systems we should expand `~` to the users home path,
        //        // otherwise on Windows we can ignore all of this. -MRC-

        //        // undefine these so that tfopen and topen will work
        //#undef fopen
        //#undef open

        // open a file just as does fopen, but allow a leading ~ to specify a home directory
        public static FILE* tfopen(string file, string mode)
        {
            char expanded[1024];
            if (tilde(file, expanded))
            {
                return (fopen(expanded, mode));
            }
            errno = ENOENT;
            return nullptr;
        }

        // open a file just as does open, but expand a leading ~ into a home directory name
        public static int topen(string file, int flags, int mode)
        {
            char expanded[1024];
            if (tilde(file, expanded))
            {
                return (open(expanded, flags, mode));
            }
            errno = ENOENT;
            return -1;
        }

        // expands a tilde at the beginning of a file name to a users home directory
        public static bool tilde(string file, ref string expanded)
        {
            if (file == nullptr)
            {
                return false;
            }

            *expanded = '\0';

            if (*file == '~')
            {
                char user[128];
        struct passwd * pw = nullptr;
int i = 0;

        user[0] = '\0';
        file++;
        while (* file != '/' && i<(int) sizeof(user)) {
            user[i++] = * file++;
        }
    user[i] = '\0';
        if (i == 0) {
            char* login = getlogin();

            if (login != nullptr) {
                (void) strcpy(user, login);
} else if ((pw = getpwuid(getuid())) == nullptr) {
                return false;
            }
        }
        if (pw == nullptr && (pw = getpwnam(user)) == nullptr) {
            return false;
        }
        (void) strcpy(expanded, pw->pw_dir);
    }

    (void) strcat(expanded, file);

    return true;
}

#endif

    }
}
