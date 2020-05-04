using Moria.Core.Configs;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class CreateFoodCommandHandler:ICommandHandler<CreateFoodCommand>
    {
        private readonly IDungeonPlacer dungeonPlacer;
        private readonly IInventoryManager inventoryManager;
        private readonly ITerminal terminal;

        public CreateFoodCommandHandler(
            IDungeonPlacer dungeonPlacer,
            IInventoryManager inventoryManager,
            ITerminal terminal
        )
        {
            this.dungeonPlacer = dungeonPlacer;
            this.inventoryManager = inventoryManager;
            this.terminal = terminal;
        }

        public void Handle(CreateFoodCommand command)
        {
            this.spellCreateFood();
        }

        // Create some high quality mush for the player. -RAK-
        private void spellCreateFood()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            // Note: must take reference to this location as dungeonPlaceRandomObjectAt()
            // below, changes the tile values.
            var tile = dg.floor[py.pos.y][py.pos.x];

            // take no action here, don't want to destroy object under player
            if (tile.treasure_id != 0)
            {
                // set player_free_turn so that scroll/spell points won't be used
                game.player_free_turn = true;

                this.terminal.printMessage("There is already an object under you.");

                return;
            }

            this.dungeonPlacer.dungeonPlaceRandomObjectAt(py.pos, false);
            this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_MUSH, game.treasure.list[tile.treasure_id]);
        }
    }
}