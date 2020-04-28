using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using static Moria.Core.Constants.Identification_c;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Store_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;

namespace Moria.Core.Methods
{
    public interface IStoreInventory
    {
        int storeItemSellPrice(Store_t store, ref int min_price, ref int max_price, Inventory_t item);
        void storeCarryItem(int store_id, ref int index_id, Inventory_t item);
        void storeDestroyItem(int store_id, int item_id, bool only_one_of);
        bool storeCheckPlayerItemsCount(Store_t store, Inventory_t item);
        int storeItemValue(Inventory_t item);
        void storeMaintenance();
    }

    public class Store_inventory_m : IStoreInventory
    {
        public Store_inventory_m(
            IGameObjects gameObjects,
            IGameObjectsPush gameObjectsPush,
            IInventoryManager inventoryManager,
            IRnd rnd,
            ITreasure treasure
        )
        {
            this.gameObjects = gameObjects;
            this.gameObjectsPush = gameObjectsPush;
            this.inventoryManager = inventoryManager;
            this.rnd = rnd;
            this.treasure = treasure;
        }

        private readonly IGameObjects gameObjects;
        private readonly IGameObjectsPush gameObjectsPush;
        private readonly IInventoryManager inventoryManager;
        private readonly IRnd rnd;
        private readonly ITreasure treasure;

        // Initialize and up-keep the store's inventory. -RAK-
        public void storeMaintenance()
        {
            for (var store_id = 0; store_id < MAX_STORES; store_id++)
            {
                var store = State.Instance.stores[store_id];

                store.insults_counter = 0;
                if (store.unique_items_counter >= Config.stores.STORE_MIN_AUTO_SELL_ITEMS)
                {
                    var turnaround = rnd.randomNumber(Config.stores.STORE_STOCK_TURN_AROUND);
                    if (store.unique_items_counter >= Config.stores.STORE_MAX_AUTO_BUY_ITEMS)
                    {
                        turnaround += 1 + (int)store.unique_items_counter - (int)Config.stores.STORE_MAX_AUTO_BUY_ITEMS;
                    }
                    turnaround--;
                    while (turnaround >= 0)
                    {
                        storeDestroyItem(store_id, rnd.randomNumber(store.unique_items_counter) - 1, false);
                        turnaround--;
                    }
                }

                if (store.unique_items_counter <= Config.stores.STORE_MAX_AUTO_BUY_ITEMS)
                {
                    var turnaround = rnd.randomNumber(Config.stores.STORE_STOCK_TURN_AROUND);
                    if (store.unique_items_counter < Config.stores.STORE_MIN_AUTO_SELL_ITEMS)
                    {
                        turnaround += (int)Config.stores.STORE_MIN_AUTO_SELL_ITEMS - (int)store.unique_items_counter;
                    }

                    var max_cost = Library.Instance.StoreOwners.store_owners[(int)store.owner_id].max_cost;

                    turnaround--;
                    while (turnaround >= 0)
                    {
                        storeItemCreate(store_id, max_cost);
                        turnaround--;
                    }
                }
            }
        }

        // Returns the value for any given object -RAK-
        public int storeItemValue(Inventory_t item)
        {
            int value;

            if ((item.identification & Config.identification.ID_DAMD) != 0)
            {
                // don't purchase known cursed items
                value = 0;
            }
            else if ((item.category_id >= TV_BOW && item.category_id <= TV_SWORD) || (item.category_id >= TV_BOOTS && item.category_id <= TV_SOFT_ARMOR))
            {
                value = getWeaponArmorBuyPrice(item);
            }
            else if (item.category_id >= TV_SLING_AMMO && item.category_id <= TV_SPIKE)
            {
                value = getAmmoBuyPrice(item);
            }
            else if (item.category_id == TV_SCROLL1 || item.category_id == TV_SCROLL2 || item.category_id == TV_POTION1 || item.category_id == TV_POTION2)
            {
                value = getPotionScrollBuyPrice(item);
            }
            else if (item.category_id == TV_FOOD)
            {
                value = getFoodBuyPrice(item);
            }
            else if (item.category_id == TV_AMULET || item.category_id == TV_RING)
            {
                value = getRingAmuletBuyPrice(item);
            }
            else if (item.category_id == TV_STAFF || item.category_id == TV_WAND)
            {
                value = getWandStaffBuyPrice(item);
            }
            else if (item.category_id == TV_DIGGING)
            {
                value = getPickShovelBuyPrice(item);
            }
            else
            {
                value = item.cost;
            }

            // Multiply value by number of items if it is a group stack item.
            // Do not include torches here.
            if (item.sub_category_id > ITEM_GROUP_MIN)
            {
                value = value * (int)item.items_count;
            }

            return value;
        }

        private int getWeaponArmorBuyPrice(Inventory_t item)
        {
            if (!spellItemIdentified(item))
            {
                return Library.Instance.Treasure.game_objects[(int)item.id].cost;
            }

            if (item.category_id >= TV_BOW && item.category_id <= TV_SWORD)
            {
                if (item.to_hit < 0 || item.to_damage < 0 || item.to_ac < 0)
                {
                    return 0;
                }

                return item.cost + (item.to_hit + item.to_damage + item.to_ac) * 100;
            }

            if (item.to_ac < 0)
            {
                return 0;
            }

            return item.cost + item.to_ac * 100;
        }

        private int getAmmoBuyPrice(Inventory_t item)
        {
            if (!spellItemIdentified(item))
            {
                return Library.Instance.Treasure.game_objects[(int)item.id].cost;
            }

            if (item.to_hit < 0 || item.to_damage < 0 || item.to_ac < 0)
            {
                return 0;
            }

            // use 5, because missiles generally appear in groups of 20,
            // so 20 * 5 == 100, which is comparable to weapon bonus above
            return item.cost + (item.to_hit + item.to_damage + item.to_ac) * 5;
        }

        private int getPotionScrollBuyPrice(Inventory_t item)
        {
            if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                return 20;
            }

            return item.cost;
        }

        private int getFoodBuyPrice(Inventory_t item)
        {
            if (item.sub_category_id < ITEM_SINGLE_STACK_MIN + MAX_MUSHROOMS &&
                !itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                return 1;
            }

            return item.cost;
        }

        private int getRingAmuletBuyPrice(Inventory_t item)
        {
            // player does not know what type of ring/amulet this is
            if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                return 45;
            }

            // player knows what type of ring, but does not know whether it
            // is cursed or not, if refuse to buy cursed objects here, then
            // player can use this to 'identify' cursed objects
            if (!spellItemIdentified(item))
            {
                return Library.Instance.Treasure.game_objects[(int)item.id].cost;
            }

            return item.cost;
        }

        private int getWandStaffBuyPrice(Inventory_t item)
        {
            if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                if (item.category_id == TV_WAND)
                {
                    return 50;
                }

                return 70;
            }

            if (spellItemIdentified(item))
            {
                return item.cost + (item.cost / 20) * item.misc_use;
            }

            return item.cost;
        }

        private int getPickShovelBuyPrice(Inventory_t item)
        {
            if (!spellItemIdentified(item))
            {
                return Library.Instance.Treasure.game_objects[(int)item.id].cost;
            }

            if (item.misc_use < 0)
            {
                return 0;
            }

            // some digging tools start with non-zero `misc_use` values, so only
            // multiply the plusses by 100, make sure result is positive
            var value = item.cost + (item.misc_use - Library.Instance.Treasure.game_objects[(int)item.id].misc_use) * 100;

            if (value < 0)
            {
                value = 0;
            }

            return value;
        }

        // Asking price for an item -RAK-
        public int storeItemSellPrice(Store_t store, ref int min_price, ref int max_price, Inventory_t item)
        {
            var py = State.Instance.py;

            var price = storeItemValue(item);

            // check `item.cost` in case it is cursed, check `price` in case it is damaged
            // don't let the item get into the store inventory
            if (item.cost < 1 || price < 1)
            {
                return 0;
            }

            var owner = Library.Instance.StoreOwners.store_owners[(int)store.owner_id];

            price = price * (int)Library.Instance.Stores.race_gold_adjustments[(int)owner.race][(int)py.misc.race_id] / 100;
            if (price < 1)
            {
                price = 1;
            }

            max_price = price * (int)owner.max_inflate / 100;
            min_price = price * (int)owner.min_inflate / 100;

            if (min_price > max_price)
            {
                min_price = max_price;
            }

            return price;
        }

        // Check to see if they will be carrying too many objects -RAK-
        public bool storeCheckPlayerItemsCount(Store_t store, Inventory_t item)
        {
            if (store.unique_items_counter < STORE_MAX_DISCRETE_ITEMS)
            {
                return true;
            }

            if (item.sub_category_id < ITEM_SINGLE_STACK_MIN)
            {
                return false;
            }

            var store_check = false;

            for (var i = 0; i < store.unique_items_counter; i++)
            {
                var store_item = store.inventory[i].item;

                // note: items with sub_category_id of gte ITEM_SINGLE_STACK_MAX only stack
                // if their `sub_category_id`s match
                if (store_item.category_id == item.category_id &&
                    store_item.sub_category_id == item.sub_category_id &&
                    (int)(store_item.items_count + item.items_count) < 256 &&
                    (item.sub_category_id < ITEM_GROUP_MIN || store_item.misc_use == item.misc_use))
                {
                    store_check = true;
                }
            }

            return store_check;
        }

        // Insert INVEN_MAX at given location
        private void storeItemInsert(int store_id, int pos, int i_cost, Inventory_t item)
        {
            var store = State.Instance.stores[store_id];

            for (var i = (int)store.unique_items_counter - 1; i >= pos; i--)
            {
                store.inventory[i + 1] = store.inventory[i];
            }

            store.inventory[pos].item = item;
            store.inventory[pos].cost = -i_cost;
            store.unique_items_counter++;
        }

        // Add the item in INVEN_MAX to stores inventory. -RAK-
        public void storeCarryItem(int store_id, ref int index_id, Inventory_t item)
        {
            index_id = -1;

            var store = State.Instance.stores[store_id];

            int item_cost = 0, dummy = 9;
            if (storeItemSellPrice(store, ref dummy, ref item_cost, item) < 1)
            {
                return;
            }

            var item_id = 0;
            var item_num = (int)item.items_count;
            var item_category = (int)item.category_id;
            var item_sub_catagory = (int)item.sub_category_id;

            var flag = false;
            do
            {
                var store_item = store.inventory[item_id].item;

                if (item_category == store_item.category_id)
                {
                    if (item_sub_catagory == store_item.sub_category_id && // Adds to other item
                        item_sub_catagory >= ITEM_SINGLE_STACK_MIN &&
                        (item_sub_catagory < ITEM_GROUP_MIN || store_item.misc_use == item.misc_use))
                    {
                        index_id = item_id;
                        store_item.items_count += (uint)item_num;

                        // must set new cost for group items, do this only for items
                        // strictly greater than group_min, not for torches, this
                        // must be recalculated for entire group
                        if (item_sub_catagory > ITEM_GROUP_MIN)
                        {
                            storeItemSellPrice(store, ref dummy, ref item_cost, store_item);
                            store.inventory[item_id].cost = -item_cost;
                        }
                        else if (store_item.items_count > 24)
                        {
                            // must let group objects (except torches) stack over 24
                            // since there may be more than 24 in the group
                            store_item.items_count = 24;
                        }
                        flag = true;
                    }
                }
                else if (item_category > store_item.category_id)
                { // Insert into list
                    storeItemInsert(store_id, item_id, item_cost, item);
                    flag = true;
                    index_id = item_id;
                }
                item_id++;
            } while (item_id < store.unique_items_counter && !flag);

            // Becomes last item in list
            if (!flag)
            {
                storeItemInsert(store_id, (int)store.unique_items_counter, item_cost, item);
                index_id = (int)store.unique_items_counter - 1;
            }
        }

        // Destroy an item in the stores inventory.  Note that if
        // `only_one_of` is false, an entire slot is destroyed -RAK-
        public void storeDestroyItem(int store_id, int item_id, bool only_one_of)
        {
            var store = State.Instance.stores[store_id];
            var store_item = store.inventory[item_id].item;

            uint number;

            // for single stackable objects, only destroy one half on average,
            // this will help ensure that general store and alchemist have
            // reasonable selection of objects
            if (store_item.sub_category_id >= ITEM_SINGLE_STACK_MIN && store_item.sub_category_id <= ITEM_SINGLE_STACK_MAX)
            {
                if (only_one_of)
                {
                    number = 1;
                }
                else
                {
                    number = (uint)rnd.randomNumber((int)store_item.items_count);
                }
            }
            else
            {
                number = store_item.items_count;
            }

            if (number != store_item.items_count)
            {
                store_item.items_count -= number;
            }
            else
            {
                for (var i = item_id; i < store.unique_items_counter - 1; i++)
                {
                    store.inventory[i] = store.inventory[i + 1];
                }
                this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, store.inventory[store.unique_items_counter - 1].item);
                store.inventory[store.unique_items_counter - 1].cost = 0;
                store.unique_items_counter--;
            }
        }

        // Creates an item and inserts it into store's inven -RAK-
        private void storeItemCreate(int store_id, int max_cost)
        {
            var game = State.Instance.game;
            var free_id = this.gameObjects.popt();

            for (var tries = 0; tries <= 3; tries++)
            {
                var id = (int)Library.Instance.Stores.store_choices[store_id][rnd.randomNumber(STORE_MAX_ITEM_TYPES) - 1];
                this.inventoryManager.inventoryItemCopyTo(id, game.treasure.list[free_id]);
                treasure.magicTreasureMagicalAbility(free_id, (int)Config.treasure.LEVEL_TOWN_OBJECTS);

                var item = game.treasure.list[free_id];

                if (storeCheckPlayerItemsCount(State.Instance.stores[store_id], item))
                {
                    // Item must be good: cost > 0.
                    if (item.cost > 0 && item.cost < max_cost)
                    {
                        // equivalent to calling spellIdentifyItem(),
                        // except will not change the objects_identified array.
                        itemIdentifyAsStoreBought(item);

                        var dummy = 0;
                        storeCarryItem(store_id, ref dummy, item);

                        tries = 10;
                    }
                }
            }

            this.gameObjectsPush.pusht((uint)free_id);
        }
    }
}
