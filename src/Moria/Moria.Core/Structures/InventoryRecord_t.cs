namespace Moria.Core.Structures
{
    public class InventoryRecord_t
    {
        public int cost { get; set; }
        public Inventory_t item { get; set; } = new Inventory_t();
    }

    /*
// InventoryRecord_t data for a store inventory item
typedef struct {
    int32_t cost;
    Inventory_t item;
} InventoryRecord_t;
     */
}
