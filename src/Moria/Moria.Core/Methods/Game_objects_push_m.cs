using Moria.Core.Configs;
using Moria.Core.States;

namespace Moria.Core.Methods
{
    public interface IGameObjectsPush
    {
        void pusht(uint treasure_id);
    }
    
    public class Game_objects_push_m : IGameObjectsPush
    {
        public Game_objects_push_m(
            IInventoryManager inventoryManager
        )
        {
            this.inventoryManager = inventoryManager;
        }

        private readonly IInventoryManager inventoryManager;

        // Pushes a record back onto free space list -RAK-
        // `dungeonDeleteObject()` should always be called instead, unless the object
        // in question is not in the dungeon, e.g. in store1.c and files.c
        public void pusht(uint treasure_id)
        {
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            if (treasure_id != game.treasure.current_id - 1)
            {
                game.treasure.list[treasure_id] = game.treasure.list[game.treasure.current_id - 1];

                // must change the treasure_id in the cave of the object just moved
                for (var y = 0; y < dg.height; y++)
                {
                    for (var x = 0; x < dg.width; x++)
                    {
                        if (dg.floor[y][x].treasure_id == game.treasure.current_id - 1)
                        {
                            dg.floor[y][x].treasure_id = treasure_id;
                        }
                    }
                }
            }
            game.treasure.current_id--;

            inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, game.treasure.list[game.treasure.current_id]);
        }
    }
}