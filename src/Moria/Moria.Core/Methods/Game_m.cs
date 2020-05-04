﻿using Moria.Core.Configs;
using Moria.Core.States;

namespace Moria.Core.Methods
{
    public interface IGame
    {
        void exitProgram();
        bool getAllDirections(string prompt, ref int direction);
        bool getDirectionWithMemory(string prompt, ref int direction);

    }
    
    public class Game_m : IGame
    {
        private readonly ITerminal terminal;

        public Game_m(ITerminal terminal)
        {
            this.terminal = terminal;
        }

        // map roguelike direction commands into numbers
        private char mapRoguelikeKeysToKeypad(char command)
        {
            switch (command)
            {
                case 'h':
                    return '4';
                case 'y':
                    return '7';
                case 'k':
                    return '8';
                case 'u':
                    return '9';
                case 'l':
                    return '6';
                case 'n':
                    return '3';
                case 'j':
                    return '2';
                case 'b':
                    return '1';
                case '.':
                    return '5';
                default:
                    return command;
            }
        }

        // Prompts for a direction -RAK-
        // Direction memory added, for repeated commands.  -CJS
        public bool getDirectionWithMemory(string prompt, ref int direction)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            // used in counted commands. -CJS-
            if (game.use_last_direction)
            {
                direction = py.prev_dir;
                return true;
            }

            if (string.IsNullOrEmpty(prompt))
            {
                prompt = "Which direction?";
            }

            while (true)
            {
                // Don't end a counted command. -CJS-
                var save = game.command_count;

                if (!this.terminal.getCommand(prompt, out var command))
                {
                    game.player_free_turn = true;
                    return false;
                }

                game.command_count = save;

                if (Config.options.use_roguelike_keys)
                {
                    command = this.mapRoguelikeKeysToKeypad(command);
                }

                if (command >= '1' && command <= '9' && command != '5')
                {
                    py.prev_dir = (char)(command - '0');
                    direction = py.prev_dir;
                    return true;
                }

                this.terminal.terminalBellSound();
            }
        }

        // Similar to getDirectionWithMemory(), except that no memory exists,
        // and it is allowed to enter the null direction. -CJS-
        public bool getAllDirections(string prompt, ref int direction)
        {
            var game = State.Instance.game;
            var command = '\0';

            while (true)
            {
                if (!this.terminal.getCommand(prompt, out command))
                {
                    game.player_free_turn = true;
                    return false;
                }

                if (Config.options.use_roguelike_keys)
                {
                    command = this.mapRoguelikeKeysToKeypad(command);
                }

                if (command >= '1' && command <= '9')
                {
                    direction = command - '0';
                    return true;
                }

                this.terminal.terminalBellSound();
            }
        }

        // Restore the terminal and exit
        public void exitProgram()
        {
            this.terminal.flushInputBuffer();
            this.terminal.terminalRestore();

            throw new MoriaExitRequestedException();
        }

        //// Abort the program with a message displayed on the terminal.
        //private void abortProgram(string msg)
        //{
        //    flushInputBuffer();
        //    terminalRestore();
        //
        //    printf("Program was manually aborted with the message:\n");
        //    printf("%s\n", msg);
        //
        //    exit(0);
        //}
    }
}
