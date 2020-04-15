using Moria.Core.Constants;
using Moria.Core.Utils;

namespace Moria.Core.Structures
{
    public class Game_treasure_t
    {
        public int current_id { get; set; } = 0; // Current treasure heap ptr

        public Inventory_t[] list { get; set; } =
            ArrayInitializer.Initialize<Inventory_t>(Game_c.LEVEL_MAX_OBJECTS);
    }
}