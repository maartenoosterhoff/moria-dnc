using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Ui_io_m;

namespace Moria.Core.Methods
{
    public interface IInventory
    {
        bool inventoryCanCarryItemCount(Inventory_t item);
        void inventoryDropItem(int item_id, bool drop_all);
        int inventoryCarryItem(Inventory_t new_item);
        bool inventoryCanCarryItem(Inventory_t item);
        bool setNull(Inventory_t item);
        bool setLightningDestroyableItems(Inventory_t item);
        bool setAcidDestroyableItems(Inventory_t item);
        bool setFrostDestroyableItems(Inventory_t item);
        bool setFireDestroyableItems(Inventory_t item);
        void damageLightningBolt(int damage, string creature_name);
        void damagePoisonedGas(int damage, string creature_name);
        void damageAcid(int damage, string creature_name);
        void damageCold(int damage, string creature_name);
        void damageFire(int damage, string creature_name);
        void damageCorrodingGas(string creature_name);
        bool inventoryDiminishLightAttack(bool noticed);
        bool inventoryDiminishChargesAttack(uint creature_level, ref int monster_hp, bool noticed);
    }

    public class Inventory_m : IInventory
    {
        public Inventory_m(
            IGameObjects gameObjects,
            IDungeon dungeon,
            IInventoryManager inventoryManager,
            IRnd rnd
        )
        {
            this.gameObjects = gameObjects;
            this.dungeon = dungeon;
            this.inventoryManager = inventoryManager;
            this.rnd = rnd;
        }

        private readonly IGameObjects gameObjects;
        private readonly IDungeon dungeon;
        private readonly IInventoryManager inventoryManager;
        private readonly IRnd rnd;

        // Drops an item from inventory to given location -RAK-
        public void inventoryDropItem(int item_id, bool drop_all)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            if (dg.floor[py.pos.y][py.pos.x].treasure_id != 0)
            {
                dungeon.dungeonDeleteObject(py.pos);
            }

            var treasure_id = this.gameObjects.popt();

            var item = py.inventory[item_id];
            game.treasure.list[treasure_id] = item;

            dg.floor[py.pos.y][py.pos.x].treasure_id = (uint)treasure_id;

            if (item_id >= (int)PlayerEquipment.Wield)
            {
                playerTakeOff(item_id, -1);
            }
            else
            {
                if (drop_all || item.items_count == 1)
                {
                    py.pack.weight -= (int)item.weight * (int)item.items_count;
                    py.pack.unique_items--;

                    while (item_id < py.pack.unique_items)
                    {
                        py.inventory[item_id] = py.inventory[item_id + 1].Clone();
                        item_id++;
                    }

                    inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, py.inventory[py.pack.unique_items]);
                }
                else
                {
                    game.treasure.list[treasure_id].items_count = 1;
                    py.pack.weight -= (int)item.weight;
                    item.items_count--;
                }

                var prt1 = string.Empty;
                var prt2 = string.Empty;
                itemDescription(ref prt1, game.treasure.list[treasure_id], true);
                prt2 = $"Dropped {prt1}";
                //(void)sprintf(prt2, "Dropped %s", prt1);
                printMessage(prt2);
            }

            py.flags.status |= Config.player_status.PY_STR_WGT;
        }

        public bool inventoryDiminishLightAttack(bool noticed)
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Light];

            if (item.misc_use > 0)
            {
                item.misc_use -= (250 + rnd.randomNumber(250));

                if (item.misc_use < 1)
                {
                    item.misc_use = 1;
                }

                if (py.flags.blind < 1)
                {
                    printMessage("Your light dims.");
                }
                else
                {
                    noticed = false;
                }
            }
            else
            {
                noticed = false;
            }

            return noticed;
        }

        public bool inventoryDiminishChargesAttack(uint creature_level, ref int monster_hp, bool noticed)
        {
            var py = State.Instance.py;
            var item = py.inventory[rnd.randomNumber(py.pack.unique_items) - 1];

            var has_charges = item.category_id == TV_STAFF || item.category_id == TV_WAND;

            if (has_charges && item.misc_use > 0)
            {
                monster_hp += (int)creature_level * item.misc_use;
                item.misc_use = 0;
                if (!spellItemIdentified(item))
                {
                    itemAppendToInscription(item, Config.identification.ID_EMPTY);
                }
                printMessage("Energy drains from your pack!");
            }
            else
            {
                noticed = false;
            }

            return noticed;
        }

        // this code must be identical to the inventoryCarryItem() code below
        public bool inventoryCanCarryItemCount(Inventory_t item)
        {
            var py = State.Instance.py;
            if (py.pack.unique_items < (int)PlayerEquipment.Wield)
            {
                return true;
            }

            if (item.sub_category_id < ITEM_SINGLE_STACK_MIN)
            {
                return false;
            }

            for (var i = 0; i < py.pack.unique_items; i++)
            {
                var same_character = py.inventory[i].category_id == item.category_id;
                var same_category = py.inventory[i].sub_category_id == item.sub_category_id;

                // make sure the number field doesn't overflow
                // NOTE: convert to bigger types before addition -MRC-
                var same_number = ((uint)(py.inventory[i].items_count) + (uint)(item.items_count)) < 256;

                // they always stack (sub_category_id < 192), or else they have same `misc_use`
                var same_group = item.sub_category_id < ITEM_GROUP_MIN || py.inventory[i].misc_use == item.misc_use;

                // only stack if both or neither are identified
                // TODO(cook): is it correct that they should be equal to each other, regardless of true/false value?
                var inventory_item_is_colorless = itemSetColorlessAsIdentified((int)py.inventory[i].category_id, (int)py.inventory[i].sub_category_id, (int)py.inventory[i].identification);
                var item_is_colorless = itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification);
                var identification = inventory_item_is_colorless == item_is_colorless;

                if (same_character && same_category && same_number && same_group && identification)
                {
                    return true;
                }
            }

            return false;
        }

        // return false if picking up an object would change the players speed
        public bool inventoryCanCarryItem(Inventory_t item)
        {
            var py = State.Instance.py;

            var limit = playerCarryingLoadLimit();
            var new_weight = (int)item.items_count * (int)item.weight + py.pack.weight;

            if (limit < new_weight)
            {
                limit = new_weight / (limit + 1);
            }
            else
            {
                limit = 0;
            }

            return py.pack.heaviness == limit;
        }

        // Add an item to players inventory.  Return the
        // item position for a description if needed. -RAK-
        // this code must be identical to the inventoryCanCarryItemCount() code above
        public int inventoryCarryItem(Inventory_t new_item)
        {
            var py = State.Instance.py;
            var is_known = itemSetColorlessAsIdentified((int)new_item.category_id, (int)new_item.sub_category_id, (int)new_item.identification);
            var is_always_known = objectPositionOffset((int)new_item.category_id, (int)new_item.sub_category_id) == -1;

            int slot_id;

            // Now, check to see if player can carry object
            for (slot_id = 0; slot_id < PLAYER_INVENTORY_SIZE; slot_id++)
            {
                var item = py.inventory[slot_id];

                var is_same_category = new_item.category_id == item.category_id && new_item.sub_category_id == item.sub_category_id;
                var not_too_many_items = (int)(item.items_count + new_item.items_count) < 256;

                // only stack if both or neither are identified
                var same_known_status = itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification) == is_known;

                if (is_same_category && new_item.sub_category_id >= ITEM_SINGLE_STACK_MIN && not_too_many_items &&
                    (new_item.sub_category_id < ITEM_GROUP_MIN || item.misc_use == new_item.misc_use) && same_known_status)
                {
                    item.items_count += new_item.items_count;
                    break;
                }

                if ((is_same_category && is_always_known) || new_item.category_id > item.category_id)
                {
                    // For items which are always `is_known`, i.e. never have a 'color',
                    // insert them into the inventory in sorted order.
                    for (var i = py.pack.unique_items - 1; i >= slot_id; i--)
                    {
                        py.inventory[i + 1] = py.inventory[i].Clone();
                    }
                    py.inventory[slot_id] = new_item;
                    py.pack.unique_items++;
                    break;
                }
            }

            py.pack.weight += (int)(new_item.items_count * new_item.weight);
            py.flags.status |= Config.player_status.PY_STR_WGT;

            return slot_id;
        }

        // AC gets worse -RAK-
        // Note: This routine affects magical AC bonuses so
        // that stores can detect the damage.
        private bool damageMinusAC(uint typ_dam)
        {
            var py = State.Instance.py;
            var items_count = 0;
            var items = new uint[6];
            //uint8_t items[6];

            if (py.inventory[(int)PlayerEquipment.Body].category_id != TV_NOTHING)
            {
                items[items_count] = (int)PlayerEquipment.Body;
                items_count++;
            }

            if (py.inventory[(int)PlayerEquipment.Arm].category_id != TV_NOTHING)
            {
                items[items_count] = (int)PlayerEquipment.Arm;
                items_count++;
            }

            if (py.inventory[(int)PlayerEquipment.Outer].category_id != TV_NOTHING)
            {
                items[items_count] = (int)PlayerEquipment.Outer;
                items_count++;
            }

            if (py.inventory[(int)PlayerEquipment.Hands].category_id != TV_NOTHING)
            {
                items[items_count] = (int)PlayerEquipment.Hands;
                items_count++;
            }

            if (py.inventory[(int)PlayerEquipment.Head].category_id != TV_NOTHING)
            {
                items[items_count] = (int)PlayerEquipment.Head;
                items_count++;
            }

            // also affect boots
            if (py.inventory[(int)PlayerEquipment.Feet].category_id != TV_NOTHING)
            {
                items[items_count] = (int)PlayerEquipment.Feet;
                items_count++;
            }

            var minus = false;

            if (items_count == 0)
            {
                return minus;
            }

            var item_id = items[rnd.randomNumber(items_count) - 1];

            var description = string.Empty;
            var msg = string.Empty;
            //obj_desc_t description = { '\0' };
            //obj_desc_t msg = { '\0' };

            if ((py.inventory[item_id].flags & typ_dam) != 0u)
            {
                minus = true;

                itemDescription(ref description, py.inventory[item_id], false);
                msg = $"Your {description} resists damage!";
                //(void)sprintf(msg, "Your %s resists damage!", description);
                printMessage(msg);
            }
            else if (py.inventory[item_id].ac + py.inventory[item_id].to_ac > 0)
            {
                minus = true;

                itemDescription(ref description, py.inventory[item_id], false);
                msg = $"Your {description} is damaged!";
                //(void)sprintf(msg, "Your %s is damaged!", description);
                printMessage(msg);

                py.inventory[item_id].to_ac--;
                playerRecalculateBonuses();
            }

            return minus;
        }

        // Functions to emulate the original Pascal sets
        public bool setNull(Inventory_t item)
        {
            //(void)item; // silence warnings 
            return false;
        }

        private bool setCorrodableItems(Inventory_t item)
        {
            switch (item.category_id)
            {
                case TV_SWORD:
                case TV_HELM:
                case TV_SHIELD:
                case TV_HARD_ARMOR:
                case TV_WAND:
                    return true;
                default:
                    return false;
            }
        }

        private bool setFlammableItems(Inventory_t item)
        {
            switch (item.category_id)
            {
                case TV_ARROW:
                case TV_BOW:
                case TV_HAFTED:
                case TV_POLEARM:
                case TV_BOOTS:
                case TV_GLOVES:
                case TV_CLOAK:
                case TV_SOFT_ARMOR:
                    // Items of (RF) should not be destroyed.
                    return (item.flags & Config.treasure_flags.TR_RES_FIRE) == 0;
                case TV_STAFF:
                case TV_SCROLL1:
                case TV_SCROLL2:
                    return true;
                default:
                    return false;
            }
        }

        private bool setAcidAffectedItems(Inventory_t item)
        {
            switch (item.category_id)
            {
                case TV_MISC:
                case TV_CHEST:
                    return true;
                case TV_BOLT:
                case TV_ARROW:
                case TV_BOW:
                case TV_HAFTED:
                case TV_POLEARM:
                case TV_BOOTS:
                case TV_GLOVES:
                case TV_CLOAK:
                case TV_SOFT_ARMOR:
                    return (item.flags & Config.treasure_flags.TR_RES_ACID) == 0;
                default:
                    return false;
            }
        }

        public bool setFrostDestroyableItems(Inventory_t item)
        {
            return (item.category_id == TV_POTION1 || item.category_id == TV_POTION2 || item.category_id == TV_FLASK);
        }

        public bool setLightningDestroyableItems(Inventory_t item)
        {
            return (item.category_id == TV_RING || item.category_id == TV_WAND || item.category_id == TV_SPIKE);
        }

        public bool setAcidDestroyableItems(Inventory_t item)
        {
            switch (item.category_id)
            {
                case TV_ARROW:
                case TV_BOW:
                case TV_HAFTED:
                case TV_POLEARM:
                case TV_BOOTS:
                case TV_GLOVES:
                case TV_CLOAK:
                case TV_HELM:
                case TV_SHIELD:
                case TV_HARD_ARMOR:
                case TV_SOFT_ARMOR:
                    return (item.flags & Config.treasure_flags.TR_RES_ACID) == 0;
                case TV_STAFF:
                case TV_SCROLL1:
                case TV_SCROLL2:
                case TV_FOOD:
                case TV_OPEN_DOOR:
                case TV_CLOSED_DOOR:
                    return true;
                default:
                    return false;
            }
        }

        public bool setFireDestroyableItems(Inventory_t item)
        {
            switch (item.category_id)
            {
                case TV_ARROW:
                case TV_BOW:
                case TV_HAFTED:
                case TV_POLEARM:
                case TV_BOOTS:
                case TV_GLOVES:
                case TV_CLOAK:
                case TV_SOFT_ARMOR:
                    return (item.flags & Config.treasure_flags.TR_RES_FIRE) == 0;
                case TV_STAFF:
                case TV_SCROLL1:
                case TV_SCROLL2:
                case TV_POTION1:
                case TV_POTION2:
                case TV_FLASK:
                case TV_FOOD:
                case TV_OPEN_DOOR:
                case TV_CLOSED_DOOR:
                    return true;
                default:
                    return false;
            }
        }

        // Corrode the unsuspecting person's armor -RAK-
        public void damageCorrodingGas(string creature_name)
        {
            if (!damageMinusAC(Config.treasure_flags.TR_RES_ACID))
            {
                playerTakesHit(rnd.randomNumber(8), creature_name);
            }

            if (inventoryManager.inventoryDamageItem(setCorrodableItems, 5) > 0)
            {
                printMessage("There is an acrid smell coming from your pack.");
            }
        }

        // Poison gas the idiot. -RAK-
        public void damagePoisonedGas(int damage, string creature_name)
        {
            var py = State.Instance.py;

            playerTakesHit(damage, creature_name);

            py.flags.poisoned += 12 + rnd.randomNumber(damage);
        }

        // Burn the fool up. -RAK-
        public void damageFire(int damage, string creature_name)
        {
            var py = State.Instance.py;

            if (py.flags.resistant_to_fire)
            {
                damage = damage / 3;
            }

            if (py.flags.heat_resistance > 0)
            {
                damage = damage / 3;
            }

            playerTakesHit(damage, creature_name);

            if (inventoryManager.inventoryDamageItem(setFlammableItems, 3) > 0)
            {
                printMessage("There is smoke coming from your pack!");
            }
        }

        // Freeze them to death. -RAK-
        public void damageCold(int damage, string creature_name)
        {
            var py = State.Instance.py;

            if (py.flags.resistant_to_cold)
            {
                damage = damage / 3;
            }

            if (py.flags.cold_resistance > 0)
            {
                damage = damage / 3;
            }

            playerTakesHit(damage, creature_name);

            if (inventoryManager.inventoryDamageItem(setFrostDestroyableItems, 5) > 0)
            {
                printMessage("Something shatters inside your pack!");
            }
        }

        // Lightning bolt the sucker away. -RAK-
        public void damageLightningBolt(int damage, string creature_name)
        {
            var py = State.Instance.py;

            if (py.flags.resistant_to_light)
            {
                damage = damage / 3;
            }

            playerTakesHit(damage, creature_name);

            if (inventoryManager.inventoryDamageItem(setLightningDestroyableItems, 3) > 0)
            {
                printMessage("There are sparks coming from your pack!");
            }
        }

        // Throw acid on the hapless victim -RAK-
        public void damageAcid(int damage, string creature_name)
        {
            var py = State.Instance.py;

            var flag = 0;

            if (damageMinusAC(Config.treasure_flags.TR_RES_ACID))
            {
                flag = 1;
            }

            if (py.flags.resistant_to_acid)
            {
                flag += 2;
            }

            playerTakesHit(damage / (flag + 1), creature_name);

            if (inventoryManager.inventoryDamageItem(setAcidAffectedItems, 3) > 0)
            {
                printMessage("There is an acrid smell coming from your pack!");
            }
        }
    }
}
