namespace Moria.Core.Structures
{
    public class InventoryRecord_t
    {
        public InventoryRecord_t Clone()
        {
            return new InventoryRecord_t
            {
                cost = cost,
                item = item.Clone()
            };
        }
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
