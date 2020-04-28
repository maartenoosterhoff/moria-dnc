using System;
using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Inventory_c;

namespace Moria.Core.Methods
{
    public interface IInventoryManager
    {
        void inventoryItemCopyTo(int from_item_id, Inventory_t to_item);
        void inventoryDestroyItem(int item_id);
        uint inventoryCollectAllItemFlags();
        int inventoryDamageItem(Func<Inventory_t, bool> item_type, int chance_percentage);
        void inventoryTakeOneItem(ref Inventory_t to_item, Inventory_t from_item);
        bool executeDisenchantAttack();
    }

    public class Inventory_manager_m : IInventoryManager
    {
        private readonly IRnd rnd;

        public Inventory_manager_m(
            IRnd rnd
        )
        {
            this.rnd = rnd;
        }

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

        public uint inventoryCollectAllItemFlags()
        {
            var py = State.Instance.py;
            uint flags = 0;

            for (var i = (int)PlayerEquipment.Wield; i < (int)PlayerEquipment.Light; i++)
            {
                flags |= py.inventory[i].flags;
            }

            return flags;
        }

        // Destroy an item in the inventory -RAK-
        public void inventoryDestroyItem(int item_id)
        {
            var py = State.Instance.py;

            var item = py.inventory[item_id];

            if (item.items_count > 1 && item.sub_category_id <= ITEM_SINGLE_STACK_MAX)
            {
                item.items_count--;
                py.pack.weight -= (int)item.weight;
            }
            else
            {
                py.pack.weight -= (int)item.weight * (int)item.items_count;

                for (var i = item_id; i < py.pack.unique_items - 1; i++)
                {
                    py.inventory[i] = py.inventory[i + 1];
                }

                inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, py.inventory[py.pack.unique_items - 1]);
                py.pack.unique_items--;
            }

            py.flags.status |= Config.player_status.PY_STR_WGT;
        }

        // Destroys a type of item on a given percent chance -RAK-
        public int inventoryDamageItem(Func<Inventory_t, bool> item_type, int chance_percentage)
        {
            var py = State.Instance.py;

            var damage = 0;

            for (var i = 0; i < py.pack.unique_items; i++)
            {
                if (item_type(py.inventory[i]) &&
                    rnd.randomNumber(100) < chance_percentage)
                    //if ((*item_type)(&py.inventory[i]) && rnd.randomNumber(100) < chance_percentage)
                {
                    inventoryDestroyItem(i);
                    damage++;
                }
            }

            return damage;
        }

        // Copies the object in the second argument over the first argument.
        // However, the second always gets a number of one except for ammo etc.
        public void inventoryTakeOneItem(ref Inventory_t to_item, Inventory_t from_item)
        {
            to_item = from_item;

            if (to_item.items_count > 1 &&
                to_item.sub_category_id >= ITEM_SINGLE_STACK_MIN &&
                to_item.sub_category_id <= ITEM_SINGLE_STACK_MAX)
            {
                to_item.items_count = 1;
            }
        }

        public bool executeDisenchantAttack()
        {
            int item_id;

            switch (rnd.randomNumber(7))
            {
                case 1:
                    item_id = (int)PlayerEquipment.Wield;
                    break;
                case 2:
                    item_id = (int)PlayerEquipment.Body;
                    break;
                case 3:
                    item_id = (int)PlayerEquipment.Arm;
                    break;
                case 4:
                    item_id = (int)PlayerEquipment.Outer;
                    break;
                case 5:
                    item_id = (int)PlayerEquipment.Hands;
                    break;
                case 6:
                    item_id = (int)PlayerEquipment.Head;
                    break;
                case 7:
                    item_id = (int)PlayerEquipment.Feet;
                    break;
                default:
                    return false;
            }

            var success = false;
            var py = State.Instance.py;
            var item = py.inventory[item_id];

            if (item.to_hit > 0)
            {
                item.to_hit -= rnd.randomNumber(2);

                // don't send it below zero
                if (item.to_hit < 0)
                {
                    item.to_hit = 0;
                }
                success = true;
            }
            if (item.to_damage > 0)
            {
                item.to_damage -= rnd.randomNumber(2);

                // don't send it below zero
                if (item.to_damage < 0)
                {
                    item.to_damage = 0;
                }
                success = true;
            }
            if (item.to_ac > 0)
            {
                item.to_ac -= rnd.randomNumber(2);

                // don't send it below zero
                if (item.to_ac < 0)
                {
                    item.to_ac = 0;
                }
                success = true;
            }

            return success;
        }
    }
}