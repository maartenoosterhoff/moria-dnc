using Moria.Core.Constants;
using Moria.Core.Utils;

namespace Moria.Core.Structures
{
    public class Store_t
    {
        public int turns_left_before_closing { get; set; }
        public int insults_counter { get; set; }
        public uint owner_id { get; set; }
        public uint unique_items_counter { get; set; }
        public uint good_purchases { get; set; }
        public uint bad_purchases { get; set; }

        public InventoryRecord_t[] inventory { get; set; } =
            ArrayInitializer.Initialize<InventoryRecord_t>(Store_c.STORE_MAX_DISCRETE_ITEMS);
    }

    /*
// Store_t holds all the data for any given store in the game
typedef struct {
    int32_t turns_left_before_closing;
    int16_t insults_counter;
    uint8_t owner_id;
    uint8_t unique_items_counter;
    uint16_t good_purchases;
    uint16_t bad_purchases;
    InventoryRecord_t inventory[STORE_MAX_DISCRETE_ITEMS];
} Store_t;
     */
}
