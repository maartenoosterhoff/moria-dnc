using System.IO;
using System.Text;
using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.Output
{
    public class RandomLevelObjectsToFileCommandHandler : ICommandHandler<RandomLevelObjectsToFileCommand>
    {
        private readonly IGameObjects gameObjects;
        private readonly IGameObjectsPush gameObjectsPush;
        private readonly IHelpers helpers;
        private readonly IIdentification identification;
        private readonly IInventoryManager inventoryManager;
        private readonly ITerminal terminal;
        private readonly ITreasure treasure;

        public RandomLevelObjectsToFileCommandHandler(
            IGameObjects gameObjects,
            IGameObjectsPush gameObjectsPush,
            IHelpers helpers,
            IIdentification identification,
            IInventoryManager inventoryManager,
            ITerminal terminal,
            ITreasure treasure
        )
        {
            this.gameObjects = gameObjects;
            this.gameObjectsPush = gameObjectsPush;
            this.helpers = helpers;
            this.identification = identification;
            this.inventoryManager = inventoryManager;
            this.terminal = terminal;
            this.treasure = treasure;
        }

        public void Handle(RandomLevelObjectsToFileCommand command)
        {
            this.outputRandomLevelObjectsToFile();
        }

        // Prints a list of random objects to a file. -RAK-
        // Note that the objects produced is a sampling of objects
        // which be expected to appear on that level.
        private void outputRandomLevelObjectsToFile()
        {
            var input = string.Empty;
            //obj_desc_t input = { 0 };

            this.terminal.putStringClearToEOL("Produce objects on what level?: ", new Coord_t(0, 0));
            if (!this.terminal.getStringInput(out input, new Coord_t(0, 32), 10))
            {
                return;
            }

            if (!this.helpers.stringToNumber(input, out var level))
            {
                return;
            }

            this.terminal.putStringClearToEOL("Produce how many objects?: ", new Coord_t(0, 0));
            if (!this.terminal.getStringInput(out input, new Coord_t(0, 27), 10))
            {
                return;
            }

            if (!this.helpers.stringToNumber(input, out var count))
            {
                return;
            }

            if (count < 1 || level < 0 || level > 1200)
            {
                this.terminal.putStringClearToEOL("Parameters no good.", new Coord_t(0, 0));
                return;
            }

            if (count > 10000)
            {
                count = 10000;
            }

            var small_objects = this.terminal.getInputConfirmation("Small objects only?");

            this.terminal.putStringClearToEOL("File name: ", new Coord_t(0, 0));

            //vtype_t filename = { 0 };
            if (!this.terminal.getStringInput(out var filename, new Coord_t(0, 11), 64))
            {
                return;
            }
            
            if (string.IsNullOrWhiteSpace(filename))
            {
                return;
            
            }



            //FILE* file_ptr = fopen(filename, "w");
            //if (file_ptr == nullptr)
            //{
            //    putStringClearToEOL("File could not be opened.", new Coord_t(0, 0));
            //    return;
            //}

            input = $"{count:d}";
            //(void)sprintf(input, "%d", count);
            this.terminal.putStringClearToEOL(input + " random objects being produced...", new Coord_t(0, 0));

            this.terminal.putQIO();

            var fileContents = new StringBuilder();
            fileContents.AppendLine("*** Random Object Sampling:");
            //(void)fprintf(file_ptr, "*** Random Object Sampling:\n");
            fileContents.AppendLine($"*** {count} objects");
            //(void)fprintf(file_ptr, "*** %d objects\n", count);
            fileContents.AppendLine($"*** For Level {level}");
            //(void)fprintf(file_ptr, "*** For Level %d\n", level);
            fileContents.AppendLine("");
            //(void)fprintf(file_ptr, "\n");
            fileContents.AppendLine("");
            //(void)fprintf(file_ptr, "\n");

            var treasure_id = this.gameObjects.popt();
            var game = State.Instance.game;

            for (var i = 0; i < count; i++)
            {
                var object_id = this.gameObjects.itemGetRandomObjectId(level, small_objects);
                this.inventoryManager.inventoryItemCopyTo(State.Instance.sorted_objects[object_id], game.treasure.list[treasure_id]);

                this.treasure.magicTreasureMagicalAbility(treasure_id, level);

                var item = game.treasure.list[treasure_id];
                this.identification.itemIdentifyAsStoreBought(item);

                if ((item.flags & Config.treasure_flags.TR_CURSED) != 0u)
                {
                    this.identification.itemAppendToInscription(item, Config.identification.ID_DAMD);
                }

                this.identification.itemDescription(out input, item, true);

                fileContents.AppendLine($"{item.depth_first_found} {input}");
                //(void)fprintf(file_ptr, "%d %s\n", item.depth_first_found, input);
            }

            this.gameObjectsPush.pusht((uint)treasure_id);

            File.WriteAllText(filename, fileContents.ToString());
            //(void)fclose(file_ptr);

            this.terminal.putStringClearToEOL("Completed.", new Coord_t(0, 0));
        }
    }
}