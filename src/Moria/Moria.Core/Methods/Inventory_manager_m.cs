using Moria.Core.Data;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods
{
    public interface IInventoryManager
    {
        void inventoryItemCopyTo(int from_item_id, Inventory_t to_item);
    }

    public class Inventory_manager_m : IInventoryManager
    {
        public void inventoryItemCopyTo(int from_item_id, Inventory_t to_item)
        {
            var from = Library.Instance.Treasure.game_objects[from_item_id];

            to_item.id = (uint)from_item_id;
            to_item.special_name_id = (int)SpecialNameIds.SN_NULL;
            to_item.inscription = string.Empty;
            to_item.flags = from.flags;
            to_item.category_id = from.category_id;
            to_item.sprite = from.sprite;
            to_item.misc_use = from.misc_use;
            to_item.cost = from.cost;
            to_item.sub_category_id = from.sub_category_id;
            to_item.items_count = from.items_count;
            to_item.weight = from.weight;
            to_item.to_hit = from.to_hit;
            to_item.to_damage = from.to_damage;
            to_item.ac = from.ac;
            to_item.to_ac = from.to_ac;
            to_item.damage = new Dice_t(from.damage.dice, from.damage.sides);
            //to_item.damage.sides = from.damage.sides;
            to_item.depth_first_found = from.depth_first_found;
            to_item.identification = 0;
        }
    }
}