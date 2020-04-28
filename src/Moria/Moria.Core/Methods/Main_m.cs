using Moria.Core.Configs;
using Moria.Core.States;
using System;
using System.Linq;
using SimpleInjector;
using static Moria.Core.Constants.Version_c;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Ui_io_m;

namespace Moria.Core.Methods
{
    public static class Main_m
    {
        public const string usage_instructions = @"
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
            var container = BuildContainer();

            uint seed = 0;
            var new_game = false;
            var roguelike_keys = true;

            //// call this routine to grab a file pointer to the high score file
            //// and prepare things to relinquish setuid privileges
            //if (!initializeScoreFile())
            //{
            //    Console.WriteLine($"Can't open score file '{Config.files.scores}.");
            //    //std::cerr << "Can't open score file '" << config::files::scores << "'\n";
            //    return 1;
            //}

            if (!terminalInitialize())
            {
                return 1;
            }

            // check for user interface option
            var argsList = args.ToList();
            var argc = argsList.Count;
            while (argsList.Count > 0 && argsList[0][0] == '-')
            {
                switch (args[0][1])
                {
                    case 'v':
                        terminalRestore();
                        Console.WriteLine("{0:d}.{1:d}.{2:d}", CURRENT_VERSION_MAJOR, CURRENT_VERSION_MINOR, CURRENT_VERSION_PATCH);
                        //printf("%d.%d.%d\n", CURRENT_VERSION_MAJOR, CURRENT_VERSION_MINOR, CURRENT_VERSION_PATCH);
                        return 0;
                    case 'n':
                        new_game = true;
                        break;
                    case 'r':
                        roguelike_keys = false;
                        break;
                    case 'd':
                        Scores_m.showScoresScreen();
                        container.GetInstance<IGame>().exitProgram();
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
                        Console.WriteLine("Moria-DNC {0:d}.{1:d}.{2:d} is released under a GPL v2 license.", CURRENT_VERSION_MAJOR, CURRENT_VERSION_MINOR, CURRENT_VERSION_PATCH);
                        Console.WriteLine(usage_instructions);
                        return 0;
                }

                argsList.RemoveAt(0);
            }

            // Auto-restart of saved file
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                // (void) strcpy(config::files::save_game, argv[0]);
                //config::files::save_game = argv[0];
                Config.files.save_game = args[0];
            }

            try
            {
                // TODO: Use container.GetInstance<IGameRun>() when possible
                Game_run_m.startMoria((int)seed, new_game, roguelike_keys);
            }
            catch (MoriaExitRequestedException)
            {
                // Do nothing.
            }


            return 0;
        }

        public const int INT_MAX = 2147483647;

        public static bool parseGameSeed(string argv, ref uint seed)
        {

            if (!stringToNumber(argv, out var value))
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

        private static Container BuildContainer()
        {
            // Create instances
            var container = new Container();
            container.RegisterSingleton<ICharacter, Character_m>();
            container.RegisterSingleton<IDice, Dice_m>();
            container.RegisterSingleton<IGame, Game_m>();
            container.RegisterSingleton<IDungeon, Dungeon_m>();
            container.RegisterSingleton<IDungeonGenerate, Dungeon_generate_m>();
            container.RegisterSingleton<IInventory, Inventory_m>();
            container.RegisterSingleton<IInventoryManager, Inventory_manager_m>();
            container.RegisterSingleton<IMonsterManager, Monster_manager_m>();
            container.RegisterSingleton<IPlayerMagic, Player_magic_m>();
            container.RegisterSingleton<IRnd, Rnd_m>();
            container.RegisterSingleton<IRng, Rng_m>();
            container.RegisterSingleton<IStd, Std_m>();
            container.RegisterSingleton<IStoreInventory, Store_inventory_m>();
            container.RegisterSingleton<ITreasure, Treasure_m>();
            container.RegisterSingleton<IUiInventory, Ui_inventory_m>();
            container.RegisterSingleton<IWizard, Wizard_m>();

            container.Verify();

            // Set static dependencies (goal is to have none)
            Dungeon_los_m.SetDependencies(
                container.GetInstance<IGame>(),
                container.GetInstance<IStd>()
            );

            Game_run_m.SetDependencies(
                container.GetInstance<ICharacter>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IDungeonGenerate>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IStoreInventory>(),
                container.GetInstance<IUiInventory>(),
                container.GetInstance<IWizard>()
            );

            Game_death_m.SetDependencies(
                container.GetInstance<IGame>(),
                container.GetInstance<IUiInventory>()
            );

            Game_files_m.SetDependencies(
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<ITreasure>()
                    );

            Game_objects_m.SetDependencies(
                container.GetInstance<IDungeon>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IRnd>()
            );

            Identification_m.SetDependencies(
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IStd>(),
                container.GetInstance<IUiInventory>()
            );

            Mage_spells_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IUiInventory>()
            );

            Monster_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IStd>()
            );

            Player_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>()
            );

            Player_bash_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IStd>()
            );

            Player_eat_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IUiInventory>()
            );

            Player_move_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IRnd>()
            );

            Player_pray_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IUiInventory>()
            );

            Player_quaff_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IUiInventory>()
            );

            Player_run_m.SetDependencies(
                container.GetInstance<IDungeon>()
            );

            Player_stats_m.SetDependencies(
                container.GetInstance<IRnd>()
            );

            Player_throw_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IUiInventory>()
            );

            Player_traps_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IRnd>()
            );

            Player_tunnel_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IRnd>()
            );

            Scores_m.SetDependencies(
                container.GetInstance<IStoreInventory>()
            );

            Scrolls_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IUiInventory>()
            );

            Spells_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IUiInventory>()
            );

            Staffs_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IUiInventory>()
            );

            Store_m.SetDependencies(
                container.GetInstance<IInventory>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IStd>(),
                container.GetInstance<IStoreInventory>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IUiInventory>()
            );

            Ui_m.SetDependencies(
                container.GetInstance<IDungeon>()
            );

            return container;
        }
    }
}
