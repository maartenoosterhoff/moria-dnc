using System;
using System.Threading;
using Moria.Core.Configs;
using Moria.Core.Methods.Commands;
using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Constants.Types_c;

namespace Moria.Core.Methods
{
    public interface ITerminal
    {
        void flushInputBuffer();
        void printMessage(string msg);

        void putQIO();

        void panelPutTile(char ch, Coord_t coord);

        bool getCommand(string prompt, out char command);

        bool terminalInitialize();
        int terminalBellSound();

        void terminalRestoreScreen();

        void terminalSaveScreen();

        void eraseLine(Coord_t coord);

        void putString(string out_str, Coord_t coord);

        void waitForContinueKey(int line_number);

        bool getStringInput(out string in_str, Coord_t coord, int slen);

        char getKeyInput();

        void clearScreen();

        void clearToBottom(int row);

        void moveCursor(Coord_t coord);

        void putStringClearToEOL(string str, Coord_t coord);

        void panelMoveCursor(Coord_t coord);

        void addChar(char ch, Coord_t coord);

        bool getInputConfirmation(string prompt);

        void messageLineClear();

        void getDefaultPlayerName(out string buffer);

        int getConsoleWidth();

        void printMessageNoCommandInterrupt(string msg);

        void terminalRestore();

        bool checkForNonBlockingKeyPress(int microseconds);
    }

    public class Ui_io_m : ITerminal
    {
        private readonly IConsoleWrapper console;
        private readonly IEventPublisher eventPublisher;

        public Ui_io_m(
            IConsoleWrapper console,
            IEventPublisher eventPublisher
        )
        {
            this.console = console;
            this.eventPublisher = eventPublisher;
        }

        //private readonly IConsoleWrapper console = new StateSavingDecorator(new ConsoleWrapper());

        private char CTRL_KEY(char x) => (char)(x & 0x1F);

        private bool isprint(char c)
        {
            const string keys = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ";
            const string specialKeys = @"!""#$%&'()*+,-./:;<=>?@[\]^_`{|}~";
            return keys.IndexOf(c) >= 0 || specialKeys.IndexOf(c) >= 0;
        }

        public void mvcur(int y, int x) => this.move(y, x);

        private void initscr()
        {
            this.console.clear();
            this.console.move(0,0);
        }

        private void refresh()
        {
            // Do... nothing?
        }

        public int getConsoleWidth() => this.console.WindowWidth;

        private bool getch(out char c) => this.getch(out c, TimeSpan.MaxValue/*.FromMilliseconds(10)*/);

        private bool getch(out char c, TimeSpan timeout)
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
                c = this.CTRL_KEY(input.KeyChar);
                return true;
            }

            c = input.KeyChar;
            return true;
        }

        private void move(int y, int x)
        {
            this.console.move(y, x);
        }

        private void mvaddch(int y, int x, char c)
        {
            this.console.move(y, x);
            this.console.addch(c);
        }

        private void mvaddstr(int y, int x, string s)
        {
            this.console.move(y, x);
            this.console.addstr(s);
        }

        private void addch(char c)
        {
            this.console.addch(c);
        }

        private void addstr(string s)
        {
            this.console.addstr(s);
        }

        private void getyx(out int y, out int x)
        {
            this.console.getyx(out y, out x);
        }

        private void clear() => this.console.clear();

        private void clrtoeol(int y, int x)
        {
            this.move(y, x);
            var length = 79 - x;
            this.addstr(new string(' ', length));
            this.move(y, x);
        }

        private void moriaTerminalInitialize()
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
        public bool terminalInitialize()
        {
            this.initscr();

            // Check we have enough screen. -CJS-
            if (this.console.WindowHeight < 24 || this.console.WindowWidth < 80)
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

            this.moriaTerminalInitialize();

            this.clear();
            this.refresh();

            return true;
        }

        // Put the terminal in the original mode. -CJS-
        public void terminalRestore()
        {
            //if (!curses_on)
            //{
            //    return;
            //}

            // Dump any remaining buffer
            this.putQIO();

            // this moves curses to bottom right corner
            var y = 0;
            var x = 0;
            this.getyx(out y, out x);
            //getyx(stdscr, y, x);
            //mvcur(y, x, Console.WindowHeight - 1, 0);

            //// exit curses
            //endwin();
            //(void)fflush(stdout);
            //
            //curses_on = false;
        }

        public void terminalSaveScreen()
        {
            this.console.saveTerminal();
        }

        public void terminalRestoreScreen()
        {
            this.console.restoreTerminal();
        }

        public int terminalBellSound()
        {
            this.putQIO();

            // The player can turn off beeps if they find them annoying.
            if (Config.options.error_beep_sound)
            {
                this.console.beep();
                //return write(1, "\007", 1);
                return 1;
            }

            return 0;
        }

        // Dump the IO buffer to terminal -RAK-
        public void putQIO()
        {
            // Let inventoryExecuteCommand() know something has changed.
            State.Instance.screen_has_changed = true;

            this.refresh();
        }

        // Flush the buffer -RAK-
        public void flushInputBuffer()
        {
            if (State.Instance.eof_flag != 0)
            {
                return;
            }

            while (this.checkForNonBlockingKeyPress(0))
            {
            }
        }

        // Clears screen
        public void clearScreen()
        {
            if (State.Instance.message_ready_to_print)
            {
                this.printMessage(/*CNIL*/null);
            }

            this.clear();
        }

        private void clearLine(int row)
        {
            this.move(row, 0);
            this.addstr(new string(' ', this.console.WindowWidth));
            this.move(row, 0);
        }

        public void clearToBottom(int row)
        {
            while (row < this.console.WindowHeight)
            {
                this.clearLine(row);
                row++;
            }

            this.move(row, 0);

            //clrtobot(); // see above
        }

        // move cursor to a given y, x position
        public void moveCursor(Coord_t coord)
        {
            this.move(coord.y, coord.x);
        }

        public void addChar(char ch, Coord_t coord)
        {
            this.mvaddch(coord.y, coord.x, ch);
            //if (mvaddch(coord.y, coord.x, ch) == ERR)
            //{
            //    abort();
            //}
        }

        // Dump IO to buffer -RAK-
        public void putString(string out_str, Coord_t coord)
        {
            out_str = out_str ?? string.Empty;
            // truncate the string, to make sure that it won't go past right edge of screen.
            if (coord.x > 79)
            {
                coord.x = 79;
            }

            //vtype_t str = { '\0' };
            var requestedLength = 79 - coord.x;
            if (requestedLength > out_str.Length)
            {
                requestedLength = out_str.Length;
            }
            var str = out_str.Substring(0, requestedLength);
            //(void)strncpy(str, out_str, (size_t)(79 - coord.x));
            //str[79 - coord.x] = '\0';

            this.mvaddstr(coord.y, coord.x, str);
            //if (mvaddstr(coord.y, coord.x, str) == ERR)
            //{
            //    abort();
            //}
        }

        // Outputs a line to a given y, x position -RAK-
        public void putStringClearToEOL(string str, Coord_t coord)
        {
            if (coord.y == MSG_LINE && State.Instance.message_ready_to_print)
            {
                this.printMessage(/*CNIL*/ null);
            }

            //move(coord.y, coord.x);
            this.clrtoeol(coord.y, coord.x);
            this.putString(str, coord);
        }

        // Clears given line of text -RAK-
        public void eraseLine(Coord_t coord)
        {
            if (coord.y == MSG_LINE && State.Instance.message_ready_to_print)
            {
                this.printMessage(/*CNIL*/null);
            }

            //move(coord.y, coord.x);
            this.clrtoeol(coord.y, coord.x);
        }

        // Moves the cursor to a given interpolated y, x position -RAK-
        public void panelMoveCursor(Coord_t coord)
        {
            var dg = State.Instance.dg;

            // Real coords convert to screen positions
            var y = coord.y - dg.panel.row_prt;
            var x = coord.x - dg.panel.col_prt;

            this.move(y, x);
            //if (move(coord.y, coord.x) == ERR)
            //{
            //    abort();
            //}
        }

        // Outputs a char to a given interpolated y, x position -RAK-
        // sign bit of a character used to indicate standout mode. -CJS
        public void panelPutTile(char ch, Coord_t coord)
        {
            var dg = State.Instance.dg;

            // Real coords convert to screen positions
            var y = coord.y - dg.panel.row_prt;
            var x = coord.x - dg.panel.col_prt;

            this.mvaddch(y, x, ch);
            //if (mvaddch(coord.y, coord.x, ch) == ERR)
            //{
            //    abort();
            //}
        }

        private Coord_t currentCursorPosition()
        {
            this.getyx(out var y, out var x);
            //getyx(stdscr, y, x);
            return new Coord_t(y, x);
        }

        // messageLinePrintMessage will print a line of text to the message line (0,0).
        // first clearing the line of any text!
        private void messageLinePrintMessage(string message)
        {
            // save current cursor position
            var coord = this.currentCursorPosition();

            // move to beginning of message line, and clear it
            this.clrtoeol(0, 0);

            // truncate message if it's too long!
            //message.resize(79);
            if (message.Length > 79)
            {
                message = message.Substring(0, 79);
            }

            this.addstr(message);

            // restore cursor to old position
            this.move(coord.y, coord.x);
        }

        // deleteMessageLine will delete all text from the message line (0,0).
        // The current cursor position will be maintained.
        public void messageLineClear()
        {
            // save current cursor position
            var coord = this.currentCursorPosition();

            // move to beginning of message line, and clear it
            this.clrtoeol(0, 0);

            // restore cursor to old position
            this.move(coord.y, coord.x);
        }

        // Outputs message to top line of screen
        // These messages are kept for later reference.
        public void printMessage(string msg)
        {
            var old_len = 0;
            var combine_messages = false;

            if (State.Instance.message_ready_to_print)
            {
                old_len = State.Instance.messages[State.Instance.last_message_id].Length + 1;
                //old_len = (int)strlen(State.Instance.messages[State.Instance.last_message_id]) + 1;

                // If the new message and the old message are short enough,
                // we want display them together on the same line.  So we
                // don't flush the old message in this case.

                int new_len;
                if (!string.IsNullOrEmpty(msg))
                {
                    new_len = msg.Length;
                    //new_len = (int)strlen(msg);
                }
                else
                {
                    new_len = 0;
                }

                if (string.IsNullOrEmpty(msg) || new_len + old_len + 2 >= 73)
                {
                    // ensure that the complete -more- message is visible.
                    if (old_len > 73)
                    {
                        old_len = 73;
                    }

                    this.putString(" -more-", new Coord_t((int)MSG_LINE, old_len));

                    char in_char;
                    do
                    {
                        in_char = this.getKeyInput();
                    } while (in_char != ' ' && in_char != ESCAPE && in_char != '\n' && in_char != '\r');
                }
                else
                {
                    combine_messages = true;
                }
            }

            if (!combine_messages)
            {
                //move((int)MSG_LINE, 0);
                this.clrtoeol((int)MSG_LINE, 0);
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
                this.putString(msg, new Coord_t((int)MSG_LINE, old_len + 2));
                //strcat(State.Instance.messages[State.Instance.last_message_id], "  ");
                //strcat(State.Instance.messages[State.Instance.last_message_id], msg);
                State.Instance.messages[State.Instance.last_message_id] += "  ";
                State.Instance.messages[State.Instance.last_message_id] += msg;

            }
            else
            {
                this.messageLinePrintMessage(msg);
                State.Instance.last_message_id++;

                if (State.Instance.last_message_id >= MESSAGE_HISTORY_SIZE)
                {
                    State.Instance.last_message_id = 0;
                }

                if (msg.Length > (int)MORIA_MESSAGE_SIZE)
                {
                    msg = msg.Substring(0, (int) MORIA_MESSAGE_SIZE);
                }
                State.Instance.messages[State.Instance.last_message_id] = msg;
                //State.Instance.messages[State.Instance.last_message_id][MORIA_MESSAGE_SIZE - 1] = '\0';
            }
        }

        // Print a message so as not to interrupt a counted command. -CJS-
        public void printMessageNoCommandInterrupt(string msg)
        {
            var game = State.Instance.game;

            // Save command count value
            var i = game.command_count;

            this.printMessage(msg);

            // Restore count value
            game.command_count = i;
        }

        // Returns a single character input from the terminal. -CJS-
        //
        // This silently consumes ^R to redraw the screen and reset the
        // terminal, so that this operation can always be performed at
        // any input prompt. getKeyInput() never returns ^R.
        public char getKeyInput()
        {
            this.putQIO();               // Dump IO buffer
            State.Instance.game.command_count = 0; // Just to be safe -CJS-

            while (true)
            {
                this.getch(out var ch);
                //int ch = getch();

                // some machines may not sign extend.
                const char EOF = unchecked((char)-1); // or '\0' ??
                if (ch == EOF)
                {
                    // avoid infinite loops while trying to call getKeyInput() for a -more- prompt.
                    State.Instance.message_ready_to_print = false;

                    State.Instance.eof_flag++;

                    this.refresh();

                    if (!State.Instance.game.character_generated || State.Instance.game.character_saved)
                    {
                        this.eventPublisher.Publish(new EndGameCommand());
                        //endGame();
                    }

                    this.eventPublisher.Publish(new DisturbCommand(true, false));
                    //playerDisturb(1, 0);

                    if (State.Instance.eof_flag > 100)
                    {
                        this.eventPublisher.Publish(new PanicSaveCommand());
                        //// just in case, to make sure that the process eventually dies
                        //State.Instance.panic_save = true;

                        //State.Instance.game.character_died_from = "(end of input: panic saved)";
                        ////(void)strcpy(game.character_died_from, "(end of input: panic saved)");
                        //if (!this.gameSave.saveGame())
                        //{
                        //    State.Instance.game.character_died_from = "panic: unexpected eof";
                        //    //(void)strcpy(game.character_died_from, "panic: unexpected eof");
                        //    State.Instance.game.character_is_dead = true;
                        //}

                        //this.eventPublisher.Publish(new EndGameCommand());
                        ////endGame();
                    }
                    return ESCAPE;
                }

                if (ch != CTRL_KEY_R)
                {
                    return (char)ch;
                }

                //(void)wrefresh(curscr); // TOMAYBE
                this.moriaTerminalInitialize();
            }
        }

        // Prompts (optional) and returns ord value of input char
        // Function returns false if <ESCAPE> is input
        public bool getCommand(string prompt, out char command)
        {
            if (!string.IsNullOrEmpty(prompt))
            {
                this.putStringClearToEOL(prompt, new Coord_t(0, 0));
            }
            command = this.getKeyInput();

            this.messageLineClear();

            return command != ESCAPE;
        }

        // Gets a string terminated by <RETURN>
        // Function returns false if <ESCAPE> is input
        public bool getStringInput(out string in_str, Coord_t coord, int slen)
        {
            var in_str_value = "";
            this.move(coord.y, coord.x);
            //(void)move(coord.y, coord.x);

            for (var i = slen; i > 0; i--)
            {
                this.addch(' ');
            }

            this.move(coord.y, coord.x);
            //(void)move(coord.y, coord.x);

            var start_col = coord.x;
            var end_col = coord.x + slen - 1;

            if (end_col > 79)
            {
                end_col = 79;
            }

            var p = 0; // index
            //char* p = in_str;

            var flag = false;
            var aborted = false;

            while (!flag && !aborted)
            {
                int key = this.getKeyInput();
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
                            this.putString(" ", coord);
                            this.moveCursor(coord);
                            p--;
                            in_str_value = in_str_value.Substring(0, in_str_value.Length - 1);
                            //*--p = '\0';
                        }
                        break;
                    default:
                        if (!this.isprint((char)key) || coord.x > end_col)
                        {
                            this.terminalBellSound();
                        }
                        else
                        {
                            this.mvaddch(coord.y, coord.x, (char)key);
                            p++;
                            in_str_value += (char)key;
                            //*p++ = (char)key;
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
        public bool getInputConfirmation(string prompt)
        {
            this.putStringClearToEOL(prompt, new Coord_t(0, 0));

            this.getyx(out var y, out var x);
            //getyx(stdscr, y, x);

            if (x > 73)
            {
                this.move(0, 73);
            }
            else if (y != 0)
            {
                // use `y` to prevent compiler warning.
            }

            this.addstr(" [y/n]");

            var input = ' ';
            while (input == ' ')
            {
                input = this.getKeyInput();
            }

            this.messageLineClear();

            return input == 'Y' || input == 'y';
        }

        // Pauses for user response before returning -RAK-
        public void waitForContinueKey(int line_number)
        {
            this.putStringClearToEOL("[ press any key to continue ]", new Coord_t(line_number, 23));
            this.getKeyInput();
            this.eraseLine(new Coord_t(line_number, 0));
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
        // might hack a accumulation of times to wait. When the accumulation reaches
        // a certain point, sleep for a second. There would need to be a way of resetting
        // the count, with a call made for commands like run or rest.
        public bool checkForNonBlockingKeyPress(int microseconds)
        {
            ////# ifdef _WIN32
            //(void)microseconds;
            //
            //// Ugly non-blocking read...Ugh! -MRC-
            //timeout(8);
            return false;
            return this.getch(out var result);
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
        public void getDefaultPlayerName(out string buffer)
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

        //// open a file just as does fopen, but allow a leading ~ to specify a home directory
        //public FILE* tfopen(string file, string mode)
        //{
        //    char expanded[1024];
        //    if (tilde(file, expanded))
        //    {
        //        return (fopen(expanded, mode));
        //    }
        //    errno = ENOENT;
        //    return nullptr;
        //}

        //// open a file just as does open, but expand a leading ~ into a home directory name
        //public int topen(string file, int flags, int mode)
        //{
        //    char expanded[1024];
        //    if (tilde(file, expanded))
        //    {
        //        return (open(expanded, flags, mode));
        //    }
        //    errno = ENOENT;
        //    return -1;
        //}

//        // expands a tilde at the beginning of a file name to a users home directory
//        public bool tilde(string file, ref string expanded)
//        {
//            if (file == nullptr)
//            {
//                return false;
//            }

//            *expanded = '\0';

//            if (*file == '~')
//            {
//                char user[128];
//        struct passwd * pw = nullptr;
//int i = 0;

//        user[0] = '\0';
//        file++;
//        while (* file != '/' && i<(int) sizeof(user)) {
//            user[i++] = * file++;
//        }
//    user[i] = '\0';
//        if (i == 0) {
//            char* login = getlogin();

//            if (login != nullptr) {
//                (void) strcpy(user, login);
//} else if ((pw = getpwuid(getuid())) == nullptr) {
//                return false;
//            }
//        }
//        if (pw == nullptr && (pw = getpwnam(user)) == nullptr) {
//            return false;
//        }
//        (void) strcpy(expanded, pw->pw_dir);
//    }

//    (void) strcat(expanded, file);

//    return true;
//}

//#endif

    }
}
