using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods
{
    public static class Ui_io_m
    {
        public static void clearToBottom(int row)
        {
            (void)move(row, 0);
            clrtobot();
        }

        // move cursor to a given y, x position
        public static void moveCursor(Coord_t coord)
        {
            (void)move(coord.y, coord.x);
        }

        public static void terminalSaveScreen()
        {
            overwrite(stdscr, save_screen);
        }

        // Outputs a char to a given interpolated y, x position -RAK-
        // sign bit of a character used to indicate standout mode. -CJS
        public static void panelPutTile(char ch, Coord_t coord)
        {
            var dg = State.Instance.dg;

            // Real coords convert to screen positions
            coord.y -= dg.panel.row_prt;
            coord.x -= dg.panel.col_prt;

            if (mvaddch(coord.y, coord.x, ch) == ERR)
            {
                abort();
            }
        }

        // Clears screen
        public static void clearScreen()
        {
            if (message_ready_to_print)
            {
                printMessage(CNIL);
            }
            (void)clear();
        }

        public static void addChar(char ch, Coord_t coord)
        {
            if (mvaddch(coord.y, coord.x, ch) == ERR)
            {
                abort();
            }
        }

        // Dump IO to buffer -RAK-
        public static void putString(string out_str, Coord_t coord)
        {
            // truncate the string, to make sure that it won't go past right edge of screen.
            if (coord.x > 79)
            {
                coord.x = 79;
            }

            vtype_t str = { '\0' };
            (void)strncpy(str, out_str, (size_t)(79 - coord.x));
            str[79 - coord.x] = '\0';

            if (mvaddstr(coord.y, coord.x, str) == ERR)
            {
                abort();
            }
        }

        // Returns a single character input from the terminal. -CJS-
        //
        // This silently consumes ^R to redraw the screen and reset the
        // terminal, so that this operation can always be performed at
        // any input prompt. getKeyInput() never returns ^R.
        public static char getKeyInput()
        {
            putQIO();               // Dump IO buffer
            game.command_count = 0; // Just to be safe -CJS-

            while (true)
            {
                int ch = getch();

                // some machines may not sign extend.
                if (ch == EOF)
                {
                    // avoid infinite loops while trying to call getKeyInput() for a -more- prompt.
                    message_ready_to_print = false;

                    eof_flag++;

                    (void)refresh();

                    if (!game.character_generated || game.character_saved)
                    {
                        endGame();
                    }

                    playerDisturb(1, 0);

                    if (eof_flag > 100)
                    {
                        // just in case, to make sure that the process eventually dies
                        panic_save = true;

                        (void)strcpy(game.character_died_from, "(end of input: panic saved)");
                        if (!saveGame())
                        {
                            (void)strcpy(game.character_died_from, "panic: unexpected eof");
                            game.character_is_dead = true;
                        }
                        endGame();
                    }
                    return ESCAPE;
                }

                if (ch != CTRL_KEY('R'))
                {
                    return (char)ch;
                }

                (void)wrefresh(curscr);
                moriaTerminalInitialize();
            }
        }

        public static void terminalRestoreScreen()
        {
            overwrite(save_screen, stdscr);
            touchwin(stdscr);
        }

        public static void terminalBellSound()
        {
            putQIO();

            // The player can turn off beeps if they find them annoying.
            if (config::options::error_beep_sound)
            {
                return write(1, "\007", 1);
            }

            return 0;
        }

        // Open and display a text help file
        // File perusal, primitive, but portable -CJS-
        public static void displayTextHelpFile(string filename)
        {
            FILE* file = fopen(filename.c_str(), "r");
            if (file == nullptr)
            {
                putStringClearToEOL("Can not find help file '" + filename + "'.", new Coord_t(0, 0));
                return;
            }

            terminalSaveScreen();

            constexpr uint8_t max_line_length = 80;
            char line_buffer[max_line_length];
            char input;

            while (feof(file) == 0)
            {
                clearScreen();

                for (int i = 0; i < 23; i++)
                {
                    if (fgets(line_buffer, max_line_length - 1, file) != CNIL)
                    {
                        putString(line_buffer, Coord_t(i, 0));
                    }
                }

                putStringClearToEOL("[ press any key to continue ]", Coord_t(23, 23));
                input = getKeyInput();
                if (input == ESCAPE)
                {
                    break;
                }
            }

            (void)fclose(file);

            terminalRestoreScreen();
        }

        // Outputs a line to a given y, x position -RAK-
        public static void putStringClearToEOL(string str, Coord_t coord)
        {
            if (coord.y == MSG_LINE && message_ready_to_print)
            {
                printMessage(CNIL);
            }

            (void)move(coord.y, coord.x);
            clrtoeol();
            putString(str.c_str(), coord);
        }

        // Clears given line of text -RAK-
        public static void eraseLine(Coord_t coord)
        {
            if (coord.y == MSG_LINE && message_ready_to_print)
            {
                printMessage(CNIL);
            }

            (void)move(coord.y, coord.x);
            clrtoeol();
        }

        // Outputs message to top line of screen
        // These messages are kept for later reference.
        public static void printMessage(string msg)
        {
            int new_len = 0;
            int old_len = 0;
            bool combine_messages = false;

            if (message_ready_to_print)
            {
                old_len = (int)strlen(messages[last_message_id]) + 1;

                // If the new message and the old message are short enough,
                // we want display them together on the same line.  So we
                // don't flush the old message in this case.

                if (msg != nullptr)
                {
                    new_len = (int)strlen(msg);
                }
                else
                {
                    new_len = 0;
                }

                if ((msg == nullptr) || new_len + old_len + 2 >= 73)
                {
                    // ensure that the complete -more- message is visible.
                    if (old_len > 73)
                    {
                        old_len = 73;
                    }

                    putString(" -more-", Coord_t{ MSG_LINE, old_len});

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
                (void)move(MSG_LINE, 0);
                clrtoeol();
            }

            // Make the null string a special case. -CJS-

            if (msg == nullptr)
            {
                message_ready_to_print = false;
                return;
            }

            game.command_count = 0;
            message_ready_to_print = true;

            // If the new message and the old message are short enough,
            // display them on the same line.

            if (combine_messages)
            {
                putString(msg, Coord_t{ MSG_LINE, old_len + 2});
                strcat(messages[last_message_id], "  ");
                strcat(messages[last_message_id], msg);
            }
            else
            {
                messageLinePrintMessage(msg);
                last_message_id++;

                if (last_message_id >= MESSAGE_HISTORY_SIZE)
                {
                    last_message_id = 0;
                }

                (void)strncpy(messages[last_message_id], msg, MORIA_MESSAGE_SIZE);
                messages[last_message_id][MORIA_MESSAGE_SIZE - 1] = '\0';
            }
        }
    }
}
