using Moria.Core.Configs;
using Moria.Core.States;
using System;
using System.Linq;
using Moria.Core.Methods.Commands;
using Moria.Core.Utils;
using SimpleInjector;
using static Moria.Core.Constants.Version_c;

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
            var terminal = container.GetInstance<ITerminal>();

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

            if (!terminal.terminalInitialize())
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
                        terminal.terminalRestore();
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

                        if (!parseGameSeed(container.GetInstance<IHelpers>(), argsList[0], ref seed))
                        {
                            terminal.terminalRestore();
                            Console.WriteLine("Game seed must be a decimal number between 1 and 2147483647.");
                            return -1;
                        }

                        break;
                    case 'w':
                        State.Instance.game.to_be_wizard = true;
                        break;
                    default:
                        terminal.terminalRestore();

                        Console.WriteLine("Robert A. Koeneke's classic dungeon crawler.");
                        Console.WriteLine("Moria-DNC {0:d}.{1:d}.{2:d} is released under a GPL v2 license.", CURRENT_VERSION_MAJOR, CURRENT_VERSION_MINOR, CURRENT_VERSION_PATCH);
                        Console.WriteLine(usage_instructions);
                        return 0;
                }

                argsList.RemoveAt(0);
            }

            // Auto-restart of saved file
            if (argsList.Count > 0 && !string.IsNullOrEmpty(argsList[0]))
            {
                // (void) strcpy(config::files::save_game, argv[0]);
                //config::files::save_game = argv[0];
                Config.files.save_game = argsList[0];
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

        public static bool parseGameSeed(IHelpers helpers, string argv, ref uint seed)
        {

            if (!helpers.stringToNumber(argv, out var value))
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
            container.RegisterSingleton<IDungeon, Dungeon_m>();
            container.RegisterSingleton<IDungeonGenerator, DungeonGenerator>();
            container.RegisterSingleton<IDungeonLos, Dungeon_los_m>();
            container.RegisterSingleton<IDungeonPlacer, Dungeon_placer_m>();
            container.RegisterSingleton<IGame, Game_m>();
            container.RegisterSingleton<IGameFiles, Game_files_m>();
            container.RegisterSingleton<IGameSave, Game_save_m>();
            container.RegisterSingleton<IGameObjects, Game_objects_m>();
            container.RegisterSingleton<IGameObjectsPush, Game_objects_push_m>();
            container.RegisterSingleton<IHelpers, Helpers_m>();
            container.RegisterSingleton<IIdentification, Identification_m>();
            container.RegisterSingleton<IInventory, Inventory_m>();
            container.RegisterSingleton<IInventoryManager, Inventory_manager_m>();
            container.RegisterSingleton<IMonsterManager, Monster_manager_m>();
            container.RegisterSingleton<IPlayerMagic, Player_magic_m>();
            container.RegisterSingleton<IPlayerPray, Player_pray_m>();
            container.RegisterSingleton<IPlayerQuaff, Player_quaff_m>();
            container.RegisterSingleton<IPlayerTunnel, Player_tunnel_m>();
            container.RegisterSingleton<IRecall, Recall_m>();
            container.RegisterSingleton<IRnd, Rnd_m>();
            container.RegisterSingleton<IRng, Rng_m>();
            container.RegisterSingleton<IStd, Std_m>();
            container.RegisterSingleton<ISpells, Spells_m>();
            container.RegisterSingleton<IStoreInventory, Store_inventory_m>();
            container.RegisterSingleton<ITerminal, Ui_io_m>();
            container.RegisterSingleton<ITerminalEx, Ui_m>();
            container.RegisterSingleton<ITreasure, Treasure_m>();
            container.RegisterSingleton<IUiInventory, Ui_inventory_m>();
            container.RegisterSingleton<IWizard, Wizard_m>();

            container.RegisterSingleton<IConsoleWrapper, ConsoleWrapper>();
            container.RegisterDecorator<IConsoleWrapper, StateSavingDecorator>(Lifestyle.Singleton);

            container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();
            container.Register(typeof(ICommandHandler<>), typeof(ICommandHandler<>).Assembly);
            container.Register(typeof(ICommandHandler<,>), typeof(ICommandHandler<,>).Assembly);

            container.Verify();

            // Set static dependencies (goal is to have none)
            Game_run_m.SetDependencies(
                container.GetInstance<ICharacter>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IDungeonLos>(),
                container.GetInstance<IDungeonGenerator>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IGameFiles>(),
                container.GetInstance<IGameSave>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IIdentification>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IPlayerPray>(),
                container.GetInstance<IPlayerQuaff>(),
                container.GetInstance<IPlayerTunnel>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ISpells>(),
                container.GetInstance<IStoreInventory>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IUiInventory>(),
                container.GetInstance<IWizard>(),

                container.GetInstance<IEventPublisher>()
            );

            Mage_spells_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ISpells>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IUiInventory>(),

                container.GetInstance<IEventPublisher>()
            );

            Monster_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IDungeonLos>(),
                container.GetInstance<IDungeonPlacer>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IStd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IEventPublisher>()
            );

            Player_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IIdentification>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IEventPublisher>()
            );

            Player_bash_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<IStd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IEventPublisher>()
            );

            Player_eat_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IIdentification>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IUiInventory>(),
                container.GetInstance<IEventPublisher>()
            );

            Player_move_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IDungeonPlacer>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IIdentification>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IEventPublisher>()
            );

            Player_run_m.SetDependencies(
                container.GetInstance<IDungeon>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<IEventPublisher>()
            );

            Player_stats_m.SetDependencies(
                container.GetInstance<IRnd>(),
                container.GetInstance<ITerminalEx>()
            );

            Player_throw_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IGameObjects>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IIdentification>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IUiInventory>(),
                container.GetInstance<IEventPublisher>()
            );

            Player_traps_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IDungeon>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IIdentification>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IEventPublisher>()
            );

            Scores_m.SetDependencies(
                container.GetInstance<IStoreInventory>()
            );

            Scrolls_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IIdentification>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IUiInventory>(),

                container.GetInstance<IEventPublisher>()
            );

            Staffs_m.SetDependencies(
                container.GetInstance<IDice>(),
                container.GetInstance<IGame>(),
                container.GetInstance<IHelpers>(),
                container.GetInstance<IIdentification>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IMonsterManager>(),
                container.GetInstance<IPlayerMagic>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IUiInventory>(),

                container.GetInstance<IEventPublisher>()
            );

            Store_m.SetDependencies(
                container.GetInstance<IHelpers>(),
                container.GetInstance<IIdentification>(),
                container.GetInstance<IInventory>(),
                container.GetInstance<IInventoryManager>(),
                container.GetInstance<IStd>(),
                container.GetInstance<IStoreInventory>(),
                container.GetInstance<IRnd>(),
                container.GetInstance<ITerminal>(),
                container.GetInstance<ITerminalEx>(),
                container.GetInstance<IUiInventory>()
            );

            return container;
        }

        private class SimpleInjectorEventPublisher : IEventPublisher
        {
            private readonly Container container;

            public SimpleInjectorEventPublisher(Container container)
            {
                this.container = container;
            }

            public void Publish<TCommand>(TCommand command) where TCommand : ICommand
            {
                var handlerType = typeof(ICommandHandler<>).MakeGenericType(typeof(TCommand));
                var handler = (ICommandHandler<TCommand>)this.container.GetInstance(handlerType);
                handler.Handle(command);
            }

            public bool PublishWithOutputBool<TCommand>(TCommand command) where TCommand : ICommand
            {
                var handlerType = typeof(ICommandHandler<,>).MakeGenericType(typeof(TCommand), typeof(bool));
                var handler = (ICommandHandler<TCommand, bool>)this.container.GetInstance(handlerType);
                return handler.Handle(command);
            }

            public int PublishWithOutputInt<TCommand>(TCommand command) where TCommand : ICommand
            {
                var handlerType = typeof(ICommandHandler<,>).MakeGenericType(typeof(TCommand), typeof(int));
                var handler = (ICommandHandler<TCommand, int>)this.container.GetInstance(handlerType);
                return handler.Handle(command);
            }
        }
    }

    public interface IEventPublisher
    {
        void Publish<TCommand>(TCommand command) where TCommand : ICommand;

        bool PublishWithOutputBool<TCommand>(TCommand command) where TCommand : ICommand;

        int PublishWithOutputInt<TCommand>(TCommand command) where TCommand : ICommand;
    }
}
