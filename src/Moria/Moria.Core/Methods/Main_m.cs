using Moria.Core.Configs;
using Moria.Core.States;
using System;
using System.Linq;
using static Moria.Core.Constants.Version_c;
using static Moria.Core.Methods.Game_files_m;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Game_run_m;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Scores_m;
using static Moria.Core.Methods.Ui_io_m;

namespace Moria.Core.Methods
{
    public static class Main_m
    {
        public static string usage_instructions = @"
Usage:
    umoria[OPTIONS] SAVEGAME

SAVEGAME is an optional save game filename(default: game.sav)

Options:
    -n Force start of new game
    -r Disable classic roguelike keys(default: enabled)
    -d Display high scores and exit
    -s NUMBER    Game Seed, as a decimal number(max: 2147483647)

    -v Print version info and exit
    -h Display this message
)";

        // Initialize, restore, and get the ball rolling. -RAK-
        public static int main(string[] args)
        {
            uint seed = 0;
            bool new_game = false;
            bool roguelike_keys = true;

            // call this routine to grab a file pointer to the high score file
            // and prepare things to relinquish setuid privileges
            if (!initializeScoreFile())
            {
                Console.WriteLine($"Can't open score file '{Config.files.scores}.");
                //std::cerr << "Can't open score file '" << config::files::scores << "'\n";
                return 1;
            }

            if (!terminalInitialize())
            {
                return 1;
            }

            // check for user interface option
            var argsList = args.ToList();
            int argc = argsList.Count;
            while (argsList.Count > 0 && argsList[0][0] == '-')
            {
                switch (args[0][1])
                {
                    case 'v':
                        terminalRestore();
                        Console.WriteLine("{0}.{1}.{2}", CURRENT_VERSION_MAJOR, CURRENT_VERSION_MINOR, CURRENT_VERSION_PATCH);
                        //printf("%d.%d.%d\n", CURRENT_VERSION_MAJOR, CURRENT_VERSION_MINOR, CURRENT_VERSION_PATCH);
                        return 0;
                    case 'n':
                        new_game = true;
                        break;
                    case 'r':
                        roguelike_keys = false;
                        break;
                    case 'd':
                        showScoresScreen();
                        exitProgram();
                        break;
                    case 's':
                        // No NUMBER provided?
                        if (argsList.Count < 2)
                        //if (argv[1] == nullptr)
                        {
                            break;
                        }

                        // Move onto the NUMBER value
                        argsList.RemoveAt(0);

                        if (!parseGameSeed(argsList[0], ref seed))
                        {
                            terminalRestore();
                            Console.WriteLine("Game seed must be a decimal number between 1 and 2147483647.");
                            return -1;
                        }

                        break;
                    case 'w':
                        State.Instance.game.to_be_wizard = true;
                        break;
                    default:
                        terminalRestore();

                        Console.WriteLine("Robert A. Koeneke's classic dungeon crawler.");
                        Console.WriteLine("Moria-DNC {0}.{1}.{2} is released under a GPL v2 license.", CURRENT_VERSION_MAJOR, CURRENT_VERSION_MINOR, CURRENT_VERSION_PATCH);
                        Console.WriteLine(usage_instructions);
                        return 0;
                }

                argsList.RemoveAt(0);
            }

            // Auto-restart of saved file
            if (args[0] != null)
            {
                // (void) strcpy(config::files::save_game, argv[0]);
                //config::files::save_game = argv[0];
                Config.files.save_game = args[0];
            }

            startMoria((int)seed, new_game, roguelike_keys);

            return 0;
        }

        public const int INT_MAX = 2147483647;

        public static bool parseGameSeed(string argv, ref uint seed)
        {
            int value = 0;

            if (!stringToNumber(argv, ref value))
            {
                return false;
            }
            if (value <= 0 || value > INT_MAX)
            {
                return false;
            }

            seed = (uint)value;

            return true;
        }

    }
}
