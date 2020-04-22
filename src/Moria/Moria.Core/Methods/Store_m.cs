using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using System.Diagnostics.Tracing;
using System.Linq;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Store_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dice_m;
using static Moria.Core.Methods.Dungeon_los_m;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Mage_spells_m;
using static Moria.Core.Methods.Player_magic_m;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Store_inventory_m;
using static Moria.Core.Methods.Std_m;
using static Moria.Core.Methods.Player_run_m;
using static Moria.Core.Methods.Player_eat_m;
using static Moria.Core.Methods.Player_traps_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Ui_inventory_m;

namespace Moria.Core.Methods
{
    public static class Store_m
    {
        // Initializes the stores with owners -RAK-
        public static void storeInitializeOwners()
        {
            int count = (int)(MAX_OWNERS / MAX_STORES);

            for (int store_id = 0; store_id < MAX_STORES; store_id++)
            {
                var store = State.Instance.stores[store_id];

                store.owner_id = (uint)(MAX_STORES * (randomNumber(count) - 1) + store_id);
                store.insults_counter = 0;
                store.turns_left_before_closing = 0;
                store.unique_items_counter = 0;
                store.good_purchases = 0;
                store.bad_purchases = 0;

                foreach (var item in store.inventory)
                {
                    inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, item.item);
                    item.cost = 0;
                }
            }
        }

        // Comments vary. -RAK-
        // Comment one : Finished haggling
        public static void printSpeechFinishedHaggling()
        {
            printMessage(State.Instance.speech_sale_accepted[randomNumber(14) - 1]);
        }

        // %A1 is offer, %A2 is asking.
        public static void printSpeechSellingHaggle(int offer, int asking, int final)
        {
            var comment = string.Empty;
            //vtype_t comment = { '\0' };

            if (final > 0)
            {
                comment = State.Instance.speech_selling_haggle_final[randomNumber(3) - 1];
                //(void)strcpy(comment, speech_selling_haggle_final[randomNumber(3) - 1]);
            }
            else
            {
                comment = State.Instance.speech_selling_haggle[randomNumber(16) - 1];
                //(void)strcpy(comment, speech_selling_haggle[randomNumber(16) - 1]);
            }

            insertNumberIntoString(ref comment, "%A1", offer, false);
            insertNumberIntoString(ref comment, "%A2", asking, false);
            printMessage(comment);
        }

        public static void printSpeechBuyingHaggle(int offer, int asking, int final)
        {
            var comment = string.Empty;
            //vtype_t comment = { '\0' };

            if (final > 0)
            {
                comment = State.Instance.speech_buying_haggle_final[randomNumber(3) - 1];
                //(void)strcpy(comment, speech_buying_haggle_final[randomNumber(3) - 1]);
            }
            else
            {
                comment = State.Instance.speech_buying_haggle[randomNumber(15) - 1];
                //(void)strcpy(comment, speech_buying_haggle[randomNumber(15) - 1]);
            }

            insertNumberIntoString(ref comment, "%A1", offer, false);
            insertNumberIntoString(ref comment, "%A2", asking, false);
            printMessage(comment);
        }

        // Kick 'da bum out. -RAK-
        public static void printSpeechGetOutOfMyStore()
        {
            int comment = randomNumber(5) - 1;
            printMessage(State.Instance.speech_insulted_haggling_done[comment]);
            printMessage(State.Instance.speech_get_out_of_my_store[comment]);
        }

        public static void printSpeechTryAgain()
        {
            printMessage(State.Instance.speech_haggling_try_again[randomNumber(10) - 1]);
        }

        public static void printSpeechSorry()
        {
            printMessage(State.Instance.speech_sorry[randomNumber(5) - 1]);
        }

        // Displays the set of commands -RAK-
        static void displayStoreCommands()
        {
            putStringClearToEOL("You may:", new Coord_t(20, 0));
            putStringClearToEOL(" p) Purchase an item.           b) Browse store's inventory.", new Coord_t(21, 0));
            putStringClearToEOL(" s) Sell an item.               i/e/t/w/x) Inventory/Equipment Lists.", new Coord_t(22, 0));
            putStringClearToEOL("ESC) Exit from Building.        ^R) Redraw the screen.", new Coord_t(23, 0));
        }

        // Displays the set of commands -RAK-
        public static void displayStoreHaggleCommands(int haggle_type)
        {
            if (haggle_type == -1)
            {
                putStringClearToEOL("Specify an asking-price in gold pieces.", new Coord_t(21, 0));
            }
            else
            {
                putStringClearToEOL("Specify an offer in gold pieces.", new Coord_t(21, 0));
            }

            putStringClearToEOL("ESC) Quit Haggling.", new Coord_t(22, 0));
            eraseLine(new Coord_t(23, 0)); // clear last line
        }

        // Displays a store's inventory -RAK-
        public static void displayStoreInventory(Store_t store, int item_pos_start)
        {
            int item_pos_end = ((item_pos_start / 12) + 1) * 12;
            if (item_pos_end > store.unique_items_counter)
            {
                item_pos_end = (int)store.unique_items_counter;
            }

            int item_line_num;

            for (item_line_num = (item_pos_start % 12); item_pos_start < item_pos_end; item_line_num++)
            {
                var item = store.inventory[item_pos_start].item;

                // Save the current number of items
                int current_item_count = (int)item.items_count;

                if (item.sub_category_id >= ITEM_SINGLE_STACK_MIN && item.sub_category_id <= ITEM_SINGLE_STACK_MAX)
                {
                    item.items_count = 1;
                }

                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                itemDescription(ref description, item, true);

                // Restore the number of items
                item.items_count = (uint)current_item_count;

                var msg = $"{'a' + item_line_num}) {description}";
                //obj_desc_t msg = { '\0' };
                //(void)sprintf(msg, "%c) %s", 'a' + item_line_num, description);
                putStringClearToEOL(msg, new Coord_t(item_line_num + 5, 0));

                current_item_count = store.inventory[item_pos_start].cost;

                if (current_item_count <= 0)
                {
                    int value = -current_item_count;
                    value = value * playerStatAdjustmentCharisma() / 100;
                    if (value <= 0)
                    {
                        value = 1;
                    }

                    msg = $"{value:d9}";
                    //(void)sprintf(msg, "%9d", value);
                }
                else
                {
                    msg = $"{current_item_count:d9} [Fixed]";
                    //(void)sprintf(msg, "%9d [Fixed]", current_item_count);
                }

                putStringClearToEOL(msg, new Coord_t(item_line_num + 5, 59));
                item_pos_start++;
            }

            if (item_line_num < 12)
            {
                for (int i = 0; i < (11 - item_line_num + 1); i++)
                {
                    // clear remaining lines
                    eraseLine(new Coord_t(i + item_line_num + 5, 0));
                }
            }

            if (store.unique_items_counter > 12)
            {
                putString("- cont. -", new Coord_t(17, 60));
            }
            else
            {
                eraseLine(new Coord_t(17, 60));
            }
        }

        // Re-displays only a single cost -RAK-
        public static void displaySingleCost(int store_id, int item_id)
        {
            int cost = State.Instance.stores[store_id].inventory[item_id].cost;

            var msg = string.Empty;
            //vtype_t msg = { '\0' };
            if (cost < 0)
            {
                int c = -cost;
                c = c * playerStatAdjustmentCharisma() / 100;
                msg = $"{c:d}";
                //(void)sprintf(msg, "%d", c);
            }
            else
            {
                msg = $"{cost:d9} [Fixed]";
                //(void)sprintf(msg, "%9d [Fixed]", cost);
            }
            putStringClearToEOL(msg, new Coord_t((item_id % 12) + 5, 59));
        }

        // Displays players gold -RAK-
        public static void displayPlayerRemainingGold()
        {
            var msg = $"Gold Remaining : {State.Instance.py.misc.au}";
            //vtype_t msg = { '\0' };
            //(void)sprintf(msg, "Gold Remaining : %d", py.misc.au);
            putStringClearToEOL(msg, new Coord_t(18, 17));
        }

        // Displays store -RAK-
        public static void displayStore(Store_t store, string owner_name, int current_top_item_id)
        {
            clearScreen();
            putString(owner_name, new Coord_t(3, 9));
            putString("Item", new Coord_t(4, 3));
            putString("Asking Price", new Coord_t(4, 60));
            displayPlayerRemainingGold();
            displayStoreCommands();
            displayStoreInventory(store, current_top_item_id);
        }

        // Get the ID of a store item and return it's value -RAK-
        // Returns true if the item was found.
        public static bool storeGetItemId(ref int item_id, string prompt, int item_pos_start, int item_pos_end)
        {
            item_id = -1;
            bool item_found = false;

            var msg = $"(Items {item_pos_start + 'a'}-{item_pos_end + 'a'}, ESC to exit) {prompt}";
            //vtype_t msg = { '\0' };
            //(void)sprintf(msg, "(Items %c-%c, ESC to exit) %s", item_pos_start + 'a', item_pos_end + 'a', prompt);

            char key_char = '\0';
            while (getCommand(msg, ref key_char))
            {
                key_char -= 'a';
                if (key_char >= item_pos_start && key_char <= item_pos_end)
                {
                    item_found = true;
                    item_id = key_char;
                    break;
                }
                terminalBellSound();
            }
            messageLineClear();

            return item_found;
        }

        // Increase the insult counter and get angry if too many -RAK-
        public static bool storeIncreaseInsults(int store_id)
        {
            Store_t store = State.Instance.stores[store_id];

            store.insults_counter++;

            if (store.insults_counter <= State.Instance.store_owners[store.owner_id].max_insults)
            {
                return false;
            }

            // customer angered the store owner with too many insults!
            printSpeechGetOutOfMyStore();
            store.insults_counter = 0;
            store.bad_purchases++;
            store.turns_left_before_closing = State.Instance.dg.game_turn + 2500 + randomNumber(2500);

            return true;
        }

        // Decrease insults -RAK-
        public static void storeDecreaseInsults(int store_id)
        {
            if (State.Instance.stores[store_id].insults_counter != 0)
            {
                State.Instance.stores[store_id].insults_counter--;
            }
        }

        // Have insulted while haggling -RAK-
        // Returns true if the store owner was angered.
        public static bool storeHaggleInsults(int store_id)
        {
            if (storeIncreaseInsults(store_id))
            {
                return true;
            }

            printSpeechTryAgain();

            // keep insult separate from rest of haggle
            printMessage(/*CNIL*/ null);

            return false;
        }

        // Returns true if the customer made a valid offer
        public static bool storeGetHaggle(string prompt, ref int new_offer, int offer_count)
        {
            bool valid_offer = true;

            if (offer_count == 0)
            {
                State.Instance.store_last_increment = 0;
            }

            bool increment = false;
            int adjustment = 0;

            var prompt_len = prompt.Length;
            int start_len = prompt_len;

            string p = null;
            var msg = string.Empty;
            var last_offer_str = string.Empty;
            //char* p = nullptr;
            //vtype_t msg = { '\0' };
            //vtype_t last_offer_str = { '\0' };

            // Get a customers new offer
            while (valid_offer && adjustment == 0)
            {
                putStringClearToEOL(prompt, new Coord_t(0, 0));

                if ((offer_count != 0) && State.Instance.store_last_increment != 0)
                {
                    var abs_store_last_increment = (int)std_abs(std_intmax_t(State.Instance.store_last_increment));

                    last_offer_str = $"[{((State.Instance.store_last_increment < 0) ? '-' : '+')}{abs_store_last_increment}] ";
                    //(void)sprintf(last_offer_str, "[%c%d] ", (State.Instance.store_last_increment < 0) ? '-' : '+', abs_store_last_increment);
                    putStringClearToEOL(last_offer_str, new Coord_t(0, start_len));

                    prompt_len = start_len + last_offer_str.Length;
                    //prompt_len = start_len + (int)strlen(last_offer_str);
                }

                if (!getStringInput(ref msg, new Coord_t(0, prompt_len), 40))
                {
                    // customer aborted, i.e. pressed escape
                    valid_offer = false;
                }

                for (p = msg; *p == ' '; p++)   // TOFIX
                {
                    // fast forward to next space character
                }
                if (*p == '+' || *p == '-')
                {
                    increment = true;
                }

                if ((offer_count != 0) && increment)
                {
                    stringToNumber(msg, ref adjustment);

                    // Don't accept a zero here.  Turn off increment if it was zero
                    // because a zero will not exit.  This can be zero if the user
                    // did not type a number after the +/- sign.
                    if (adjustment == 0)
                    {
                        increment = false;
                    }
                    else
                    {
                        State.Instance.store_last_increment = (int)adjustment;
                    }
                }
                else if ((offer_count != 0) && string.IsNullOrEmpty(msg))
                {
                    adjustment = State.Instance.store_last_increment;
                    increment = true;
                }
                else
                {
                    stringToNumber(msg, ref adjustment);
                }

                // don't allow incremental haggling, if player has not made an offer yet
                if (valid_offer && offer_count == 0 && increment)
                {
                    printMessage("You haven't even made your first offer yet!");
                    adjustment = 0;
                    increment = false;
                }
            }

            if (valid_offer)
            {
                if (increment)
                {
                    new_offer += adjustment;
                }
                else
                {
                    new_offer = adjustment;
                }
            }
            else
            {
                messageLineClear();
            }

            return valid_offer;
        }

        public static BidState storeReceiveOffer(int store_id, string prompt, ref int new_offer, int last_offer, int offer_count, int factor)
        {
            BidState status = BidState.Received;

            bool done = false;
            while (!done)
            {
                if (storeGetHaggle(prompt, ref new_offer, offer_count))
                {
                    // customer submitted valid offer
                    if (new_offer * factor >= last_offer * factor)
                    {
                        done = true;
                    }
                    else if (storeHaggleInsults(store_id))
                    {
                        // customer angered the store owner!
                        status = BidState.Insulted;
                        done = true;
                    }
                    else
                    {
                        // new_offer rejected, reset new_offer so that incremental
                        // haggling works correctly
                        new_offer = last_offer;
                    }
                }
                else
                {
                    // customer aborted offer
                    status = BidState.Rejected;
                    done = true;
                }
            }

            return status;
        }

        public static void storePurchaseCustomerAdjustment(ref int min_sell, ref int max_sell)
        {
            int charisma = playerStatAdjustmentCharisma();

            max_sell = max_sell * charisma / 100;
            if (max_sell <= 0)
            {
                max_sell = 1;
            }

            min_sell = min_sell * charisma / 100;
            if (min_sell <= 0)
            {
                min_sell = 1;
            }
        }

        // Haggling routine -RAK-
        public static BidState storePurchaseHaggle(int store_id, ref int price, Inventory_t item)
        {
            BidState status = BidState.Received;

            int new_price = 0;

            Store_t store = State.Instance.stores[store_id];
            Owner_t owner = State.Instance.store_owners[store.owner_id];

            int max_sell = 0, min_sell = 0;
            int cost = storeItemSellPrice(store, ref min_sell, ref max_sell, item);

            storePurchaseCustomerAdjustment(ref min_sell, ref max_sell);

            // cast max_inflate to signed so that subtraction works correctly
            int max_buy = cost * (200 - (int)owner.max_inflate) / 100;
            if (max_buy <= 0)
            {
                max_buy = 1;
            }

            displayStoreHaggleCommands(1);

            int final_asking_price = min_sell;
            int current_asking_price = max_sell;

            string comment = "Asking";
            bool accepted_without_haggle = false;
            int offers_count = 0; // this prevents incremental haggling on first try

            // go right to final price if player has bargained well
            if (storeNoNeedToBargain(State.Instance.stores[store_id], final_asking_price))
            {
                printMessage("After a long bargaining session, you agree upon the price.");
                current_asking_price = min_sell;
                comment = "Final offer";
                accepted_without_haggle = true;

                // Set up automatic increment, so that a return will accept the final price.
                State.Instance.store_last_increment = (int)min_sell;
                offers_count = 1;
            }

            int min_offer = max_buy;
            int last_offer = min_offer;
            int new_offer = 0;

            int min_per = (int)owner.haggles_per;
            int max_per = min_per * 3;

            int final_flag = 0;

            bool rejected = false;
            bool bidding_open;

            while (!rejected)
            {
                do
                {
                    bidding_open = true;

                    var msg = $"{comment} :  {current_asking_price:d}";
                    //vtype_t msg = { '\0' };
                    //(void)sprintf(msg, "%s :  %d", comment, current_asking_price);
                    putString(msg, new Coord_t(1, 0));

                    status = storeReceiveOffer(store_id, "What do you offer? ", ref new_offer, last_offer, offers_count, 1);

                    if (status != BidState.Received)
                    {
                        rejected = true;
                    }
                    else
                    {
                        // review the received bid

                        if (new_offer > current_asking_price)
                        {
                            printSpeechSorry();

                            // rejected, reset new_offer for incremental haggling
                            new_offer = last_offer;

                            // If the automatic increment is large enough to overflow,
                            // then the player must have made a mistake.  Clear it
                            // because it is useless.
                            if (last_offer + State.Instance.store_last_increment > current_asking_price)
                            {
                                State.Instance.store_last_increment = 0;
                            }
                        }
                        else if (new_offer == current_asking_price)
                        {
                            rejected = true;
                            new_price = new_offer;
                        }
                        else
                        {
                            bidding_open = false;
                        }
                    }
                } while (!rejected && bidding_open);

                if (!rejected)
                {
                    int adjustment = (new_offer - last_offer) * 100 / (current_asking_price - last_offer);

                    if (adjustment < min_per)
                    {
                        rejected = storeHaggleInsults(store_id);
                        if (rejected)
                        {
                            status = BidState.Insulted;
                        }
                    }
                    else if (adjustment > max_per)
                    {
                        adjustment = adjustment * 75 / 100;
                        if (adjustment < max_per)
                        {
                            adjustment = max_per;
                        }
                    }

                    adjustment = ((current_asking_price - new_offer) * (adjustment + randomNumber(5) - 3) / 100) + 1;

                    // don't let the price go up
                    if (adjustment > 0)
                    {
                        current_asking_price -= adjustment;
                    }

                    if (current_asking_price < final_asking_price)
                    {
                        current_asking_price = final_asking_price;
                        comment = "Final Offer";

                        // Set the automatic haggle increment so that RET will give
                        // a new_offer equal to the final_asking_price price.
                        State.Instance.store_last_increment = (int)(final_asking_price - new_offer);
                        final_flag++;

                        if (final_flag > 3)
                        {
                            if (storeIncreaseInsults(store_id))
                            {
                                status = BidState.Insulted;
                            }
                            else
                            {
                                status = BidState.Rejected;
                            }
                            rejected = true;
                        }
                    }
                    else if (new_offer >= current_asking_price)
                    {
                        rejected = true;
                        new_price = new_offer;
                    }

                    if (!rejected)
                    {
                        last_offer = new_offer;
                        offers_count++; // enable incremental haggling

                        eraseLine(new Coord_t(1, 0));

                        var msg = $"Your last offer: {last_offer:d}";
                        //vtype_t msg = { '\0' };
                        //(void)sprintf(msg, "Your last offer : %d", last_offer);
                        putString(msg, new Coord_t(1, 39));

                        printSpeechSellingHaggle(last_offer, current_asking_price, final_flag);

                        // If the current increment would take you over the store's
                        // price, then decrease it to an exact match.
                        if (current_asking_price - last_offer < State.Instance.store_last_increment)
                        {
                            State.Instance.store_last_increment = (int)(current_asking_price - last_offer);
                        }
                    }
                }
            }

            // update bargaining info
            if (status == BidState.Received && !accepted_without_haggle)
            {
                storeUpdateBargainingSkills(State.Instance.stores[store_id], new_price, final_asking_price);
            }

            price = new_price; // update callers price before returning

            return status;
        }

        public static void storeSellCustomerAdjustment(Owner_t owner, ref int cost, ref int min_buy, ref int max_buy, ref int max_sell)
        {
            var py = State.Instance.py;
            cost = cost * (200 - playerStatAdjustmentCharisma()) / 100;
            cost = cost * (200 - (int)State.Instance.race_gold_adjustments[owner.race][py.misc.race_id]) / 100;
            if (cost < 1)
            {
                cost = 1;
            }

            max_sell = cost * (int)owner.max_inflate / 100;

            // cast max_inflate to signed so that subtraction works correctly
            max_buy = cost * (200 - (int)owner.max_inflate) / 100;
            min_buy = cost * (200 - (int)owner.min_inflate) / 100;
            if (min_buy < 1)
            {
                min_buy = 1;
            }
            if (max_buy < 1)
            {
                max_buy = 1;
            }
            if (min_buy < max_buy)
            {
                min_buy = max_buy;
            }
        }

        // Haggling routine -RAK-
        public static BidState storeSellHaggle(int store_id, ref int price, Inventory_t item)
        {
            BidState status = BidState.Received;

            int new_price = 0;

            Store_t store = State.Instance.stores[store_id];
            int cost = storeItemValue(item);

            bool rejected = false;

            int max_gold = 0;
            int min_per = 0;
            int max_per = 0;
            int max_sell = 0;
            int min_buy = 0;
            int max_buy = 0;

            if (cost < 1)
            {
                status = BidState.Offended;
                rejected = true;
            }
            else
            {
                Owner_t owner = State.Instance.store_owners[store.owner_id];

                storeSellCustomerAdjustment(owner, ref cost, ref min_buy, ref max_buy, ref max_sell);

                min_per = (int)owner.haggles_per;
                max_per = min_per * 3;
                max_gold = owner.max_cost;
            }

            int final_asking_price = 0;
            int current_asking_price = 0;

            int final_flag = 0;

            string comment = null;
            //const char* comment = nullptr;
            bool accepted_without_haggle = false;

            if (!rejected)
            {
                displayStoreHaggleCommands(-1);

                int offer_count = 0; // this prevents incremental haggling on first try

                if (max_buy > max_gold)
                {
                    final_flag = 1;
                    comment = "Final Offer";

                    // Disable the automatic haggle increment on RET.
                    State.Instance.store_last_increment = 0;
                    current_asking_price = max_gold;
                    final_asking_price = max_gold;
                    printMessage("I am sorry, but I have not the money to afford such a fine item.");
                    accepted_without_haggle = true;
                }
                else
                {
                    current_asking_price = max_buy;
                    final_asking_price = min_buy;

                    if (final_asking_price > max_gold)
                    {
                        final_asking_price = max_gold;
                    }

                    comment = "Offer";

                    // go right to final price if player has bargained well
                    if (storeNoNeedToBargain(State.Instance.stores[store_id], final_asking_price))
                    {
                        printMessage("After a long bargaining session, you agree upon the price.");
                        current_asking_price = final_asking_price;
                        comment = "Final offer";
                        accepted_without_haggle = true;

                        // Set up automatic increment, so that a return
                        // will accept the final price.
                        State.Instance.store_last_increment = (int)final_asking_price;
                        offer_count = 1;
                    }
                }

                int min_offer = max_sell;
                int last_offer = min_offer;
                int new_offer = 0;

                if (current_asking_price < 1)
                {
                    current_asking_price = 1;
                }

                bool bidding_open;

                do
                {
                    do
                    {
                        bidding_open = true;

                        var msg = $"{{comment}} :  {current_asking_price:d}";
                        //vtype_t msg = { '\0' };
                        //(void)sprintf(msg, "%s :  %d", comment, current_asking_price);
                        putString(msg, new Coord_t(1, 0));

                        status = storeReceiveOffer(store_id, "What price do you ask? ", ref new_offer, last_offer, offer_count, -1);

                        if (status != BidState.Received)
                        {
                            rejected = true;
                        }
                        else
                        {
                            // review the received bid

                            if (new_offer < current_asking_price)
                            {
                                printSpeechSorry();

                                // rejected, reset new_offer for incremental haggling
                                new_offer = last_offer;

                                // If the automatic increment is large enough to
                                // overflow, then the player must have made a mistake.
                                // Clear it because it is useless.
                                if (last_offer + State.Instance.store_last_increment < current_asking_price)
                                {
                                    State.Instance.store_last_increment = 0;
                                }
                            }
                            else if (new_offer == current_asking_price)
                            {
                                rejected = true;
                                new_price = new_offer;
                            }
                            else
                            {
                                bidding_open = false;
                            }
                        }
                    } while (!rejected && bidding_open);

                    if (!rejected)
                    {
                        int adjustment = (last_offer - new_offer) * 100 / (last_offer - current_asking_price);

                        if (adjustment < min_per)
                        {
                            rejected = storeHaggleInsults(store_id);
                            if (rejected)
                            {
                                status = BidState.Insulted;
                            }
                        }
                        else if (adjustment > max_per)
                        {
                            adjustment = adjustment * 75 / 100;
                            if (adjustment < max_per)
                            {
                                adjustment = max_per;
                            }
                        }

                        adjustment = ((new_offer - current_asking_price) * (adjustment + randomNumber(5) - 3) / 100) + 1;

                        // don't let the price go down
                        if (adjustment > 0)
                        {
                            current_asking_price += adjustment;
                        }

                        if (current_asking_price > final_asking_price)
                        {
                            current_asking_price = final_asking_price;
                            comment = "Final Offer";

                            // Set the automatic haggle increment so that RET will give
                            // a new_offer equal to the final_asking_price price.
                            State.Instance.store_last_increment = (int)(final_asking_price - new_offer);
                            final_flag++;

                            if (final_flag > 3)
                            {
                                if (storeIncreaseInsults(store_id))
                                {
                                    status = BidState.Insulted;
                                }
                                else
                                {
                                    status = BidState.Rejected;
                                }
                                rejected = true;
                            }
                        }
                        else if (new_offer <= current_asking_price)
                        {
                            rejected = true;
                            new_price = new_offer;
                        }

                        if (!rejected)
                        {
                            last_offer = new_offer;
                            offer_count++; // enable incremental haggling

                            eraseLine(new Coord_t(1, 0));
                            var msg = $"Your last bid {last_offer:d}";
                            //vtype_t msg = { '\0' };
                            //(void)sprintf(msg, "Your last bid %d", last_offer);
                            putString(msg, new Coord_t(1, 39));

                            printSpeechBuyingHaggle(current_asking_price, last_offer, final_flag);

                            // If the current decrement would take you under the store's
                            // price, then increase it to an exact match.
                            if (current_asking_price - last_offer > State.Instance.store_last_increment)
                            {
                                State.Instance.store_last_increment = (int)(current_asking_price - last_offer);
                            }
                        }
                    }
                } while (!rejected);
            }

            // update bargaining info
            if (status == BidState.Received && !accepted_without_haggle)
            {
                storeUpdateBargainingSkills(State.Instance.stores[store_id], new_price, final_asking_price);
            }

            price = new_price; // update callers price before returning

            return status;
        }

        // Get the number of store items to display on the screen
        public static int storeItemsToDisplay(int store_counter, int current_top_item_id)
        {
            if (current_top_item_id == 12)
            {
                return store_counter - 1 - 12;
            }

            if (store_counter > 11)
            {
                return 11;
            }

            return store_counter - 1;
        }

        // Buy an item from a store -RAK-
        // Returns true is the owner kicks out the customer
        public static bool storePurchaseAnItem(int store_id, ref int current_top_item_id)
        {
            var py = State.Instance.py;
            bool kick_customer = false; // don't kick them out of the store!

            var store = State.Instance.stores[store_id];

            if (store.unique_items_counter < 1)
            {
                printMessage("I am currently out of stock.");
                return false;
            }

            int item_id = 0;
            int item_count = storeItemsToDisplay((int)store.unique_items_counter, current_top_item_id);
            if (!storeGetItemId(ref item_id, "Which item are you interested in? ", 0, item_count))
            {
                return false;
            }

            // Get the item number to be bought

            item_id = item_id + current_top_item_id; // true item_id

            Inventory_t sell_item = new Inventory_t();
            inventoryTakeOneItem(ref sell_item, store.inventory[item_id].item);

            if (!inventoryCanCarryItemCount(sell_item))
            {
                putStringClearToEOL("You cannot carry that many different items.", new Coord_t(0, 0));
                return false;
            }

            BidState status = BidState.Received;
            int price = 0;

            if (store.inventory[item_id].cost > 0)
            {
                price = store.inventory[item_id].cost;
            }
            else
            {
                status = storePurchaseHaggle(store_id, ref price, sell_item);
            }

            if (status == BidState.Insulted)
            {
                kick_customer = true;
            }
            else if (status == BidState.Received)
            {
                if (py.misc.au >= price)
                {
                    printSpeechFinishedHaggling();
                    storeDecreaseInsults(store_id);
                    py.misc.au -= price;

                    int new_item_id = inventoryCarryItem(sell_item);
                    int saved_store_counter = (int)store.unique_items_counter;

                    storeDestroyItem(store_id, item_id, true);

                    var description = string.Empty;
                    //obj_desc_t description = { '\0' };
                    itemDescription(ref description, py.inventory[new_item_id], true);

                    var msg = $"You have {description:s} ({new_item_id + 'a':c}";
                    //obj_desc_t msg = { '\0' };
                    //(void)sprintf(msg, "You have %s (%c)", description, new_item_id + 'a');
                    putStringClearToEOL(msg, new Coord_t(0, 0));

                    playerStrength();

                    if (current_top_item_id >= store.unique_items_counter)
                    {
                        current_top_item_id = 0;
                        displayStoreInventory(State.Instance.stores[store_id], current_top_item_id);
                    }
                    else
                    {
                        InventoryRecord_t store_item = store.inventory[item_id];

                        if (saved_store_counter == store.unique_items_counter)
                        {
                            if (store_item.cost < 0)
                            {
                                store_item.cost = price;
                                displaySingleCost(store_id, item_id);
                            }
                        }
                        else
                        {
                            displayStoreInventory(State.Instance.stores[store_id], item_id);
                        }
                    }
                    displayPlayerRemainingGold();
                }
                else
                {
                    if (storeIncreaseInsults(store_id))
                    {
                        kick_customer = true;
                    }
                    else
                    {
                        printSpeechFinishedHaggling();
                        printMessage("Liar!  You have not the gold!");
                    }
                }
            }

            // Less intuitive, but looks better here than in storePurchaseHaggle.
            displayStoreCommands();
            eraseLine(new Coord_t(1, 0));

            return kick_customer;
        }

        // Functions to emulate the original Pascal sets
        public static bool setGeneralStoreItems(uint item_id)
        {
            switch (item_id)
            {
                case TV_DIGGING:
                case TV_BOOTS:
                case TV_CLOAK:
                case TV_FOOD:
                case TV_FLASK:
                case TV_LIGHT:
                case TV_SPIKE:
                    return true;
                default:
                    return false;
            }
        }

        public static bool setArmoryItems(uint item_id)
        {
            switch (item_id)
            {
                case TV_BOOTS:
                case TV_GLOVES:
                case TV_HELM:
                case TV_SHIELD:
                case TV_HARD_ARMOR:
                case TV_SOFT_ARMOR:
                    return true;
                default:
                    return false;
            }
        }

        public static bool setWeaponsmithItems(uint item_id)
        {
            switch (item_id)
            {
                case TV_SLING_AMMO:
                case TV_BOLT:
                case TV_ARROW:
                case TV_BOW:
                case TV_HAFTED:
                case TV_POLEARM:
                case TV_SWORD:
                    return true;
                default:
                    return false;
            }
        }

        public static bool setTempleItems(uint item_id)
        {
            switch (item_id)
            {
                case TV_HAFTED:
                case TV_SCROLL1:
                case TV_SCROLL2:
                case TV_POTION1:
                case TV_POTION2:
                case TV_PRAYER_BOOK:
                    return true;
                default:
                    return false;
            }
        }

        public static bool setAlchemistItems(uint item_id)
        {
            switch (item_id)
            {
                case TV_SCROLL1:
                case TV_SCROLL2:
                case TV_POTION1:
                case TV_POTION2:
                    return true;
                default:
                    return false;
            }
        }

        public static bool setMagicShopItems(uint item_id)
        {
            switch (item_id)
            {
                case TV_AMULET:
                case TV_RING:
                case TV_STAFF:
                case TV_WAND:
                case TV_SCROLL1:
                case TV_SCROLL2:
                case TV_POTION1:
                case TV_POTION2:
                case TV_MAGIC_BOOK:
                    return true;
                default:
                    return false;
            }
        }

        // Each store will buy only certain items, based on TVAL
        private static Func<uint, bool>[] store_buy = {
            setGeneralStoreItems,
            setArmoryItems,
            setWeaponsmithItems,
            setTempleItems,
            setAlchemistItems,
            setMagicShopItems
        };
        //        bool (* store_buy[MAX_STORES])(uint8_t) = {
        //    setGeneralStoreItems, setArmoryItems, setWeaponsmithItems, setTempleItems, setAlchemistItems, setMagicShopItems,
        //};

        // Sell an item to the store -RAK-
        // Returns true is the owner kicks out the customer
        public static bool storeSellAnItem(int store_id, ref int current_top_item_id)
        {
            var py = State.Instance.py;

            bool kick_customer = false; // don't kick them out of the store!

            int first_item = py.pack.unique_items;
            int last_item = -1;

            int[] mask = new int[(int)PlayerEquipment.Wield];

            for (int counter = 0; counter < py.pack.unique_items; counter++)
            {
                bool flag = store_buy[store_id](py.inventory[counter].category_id);

                if (flag)
                {
                    mask[counter] = (char)1;

                    if (counter < first_item)
                    {
                        first_item = counter;
                    }
                    if (counter > last_item)
                    {
                        last_item = counter;
                    }
                }
                else
                {
                    mask[counter] = (char)0;
                }
            }


            if (last_item == -1)
            {
                printMessage("You have nothing to sell to this store!");
                return false;
            }

            int item_id = 0;
            if (!inventoryGetInputForItemId(ref item_id, "Which one? ", first_item, last_item, mask, "I do not buy such items."))
            {
                return false;
            }

            Inventory_t sold_item = new Inventory_t();
            inventoryTakeOneItem(ref sold_item, py.inventory[item_id]);

            var description = string.Empty;
            itemDescription(ref description, sold_item, true);

            var msg = $"Selling {description:s} ({item_id + 'a':c}";
            //obj_desc_t msg = { '\0' };
            //(void)sprintf(msg, "Selling %s (%c)", description, item_id + 'a');
            printMessage(msg);

            if (!storeCheckPlayerItemsCount(State.Instance.stores[store_id], sold_item))
            {
                printMessage("I have not the room in my store to keep it.");
                return false;
            }

            int price = 0;

            BidState status = storeSellHaggle(store_id, ref price, sold_item);

            if (status == BidState.Insulted)
            {
                kick_customer = true;
            }
            else if (status == BidState.Offended)
            {
                printMessage("How dare you!");
                printMessage("I will not buy that!");
                kick_customer = storeIncreaseInsults(store_id);
            }
            else if (status == BidState.Received)
            {
                // bid received, and accepted!

                printSpeechFinishedHaggling();
                storeDecreaseInsults(store_id);
                py.misc.au += price;

                // identify object in inventory to set objects_identified array
                itemIdentify(py.inventory[item_id], ref item_id);

                // retake sold_item so that it will be identified
                inventoryTakeOneItem(ref sold_item, py.inventory[item_id]);

                // call spellItemIdentifyAndRemoveRandomInscription for store item, so charges/pluses are known
                spellItemIdentifyAndRemoveRandomInscription(sold_item);
                inventoryDestroyItem(item_id);

                itemDescription(ref description, sold_item, true);
                msg = $"You've sold {description:s}";
                //(void)sprintf(msg, "You've sold %s", description);
                printMessage(msg);

                int item_pos_id = 0;
                storeCarryItem(store_id, ref item_pos_id, sold_item);

                playerStrength();

                if (item_pos_id >= 0)
                {
                    if (item_pos_id < 12)
                    {
                        if (current_top_item_id < 12)
                        {
                            displayStoreInventory(State.Instance.stores[store_id], item_pos_id);
                        }
                        else
                        {
                            current_top_item_id = 0;
                            displayStoreInventory(State.Instance.stores[store_id], current_top_item_id);
                        }
                    }
                    else if (current_top_item_id > 11)
                    {
                        displayStoreInventory(State.Instance.stores[store_id], item_pos_id);
                    }
                    else
                    {
                        current_top_item_id = 12;
                        displayStoreInventory(State.Instance.stores[store_id], current_top_item_id);
                    }
                }
                displayPlayerRemainingGold();
            }

            // Less intuitive, but looks better here than in storeSellHaggle.
            eraseLine(new Coord_t(1, 0));
            displayStoreCommands();

            return kick_customer;
        }

        // Entering a store -RAK-
        public static void storeEnter(int store_id)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;
            var stores = State.Instance.stores;
            Store_t store = stores[store_id];

            if (store.turns_left_before_closing >= dg.game_turn)
            {
                printMessage("The doors are locked.");
                return;
            }

            int current_top_item_id = 0;
            displayStore(stores[store_id], State.Instance.store_owners[store.owner_id].name, current_top_item_id);

            bool exit_store = false;
            while (!exit_store)
            {
                moveCursor(new Coord_t(20, 9));

                // clear the msg flag just like we do in dungeon.c
                State.Instance.message_ready_to_print = false;

                char command = '\0';
                if (getCommand("", ref command))
                {
                    int saved_chr;

                    switch (command)
                    {
                        case 'b':
                            if (current_top_item_id == 0)
                            {
                                if (store.unique_items_counter > 12)
                                {
                                    current_top_item_id = 12;
                                    displayStoreInventory(stores[store_id], current_top_item_id);
                                }
                                else
                                {
                                    printMessage("Entire inventory is shown.");
                                }
                            }
                            else
                            {
                                current_top_item_id = 0;
                                displayStoreInventory(stores[store_id], current_top_item_id);
                            }
                            break;
                        case 'E':
                        case 'e': // Equipment List
                        case 'I':
                        case 'i': // Inventory
                        case 'T':
                        case 't': // Take off
                        case 'W':
                        case 'w': // Wear
                        case 'X':
                        case 'x': // Switch weapon
                            saved_chr = (int)py.stats.used[(int)PlayerAttr.CHR];

                            do
                            {
                                inventoryExecuteCommand(command);
                                command = (char)game.doing_inventory_command;
                            } while (command != 0);

                            // redisplay store prices if charisma changes
                            if (saved_chr != py.stats.used[(int)PlayerAttr.CHR])
                            {
                                displayStoreInventory(stores[store_id], current_top_item_id);
                            }

                            game.player_free_turn = false; // No free moves here. -CJS-
                            break;
                        case 'p':
                            exit_store = storePurchaseAnItem(store_id, ref current_top_item_id);
                            break;
                        case 's':
                            exit_store = storeSellAnItem(store_id, ref current_top_item_id);
                            break;
                        default:
                            terminalBellSound();
                            break;
                    }
                }
                else
                {
                    exit_store = true;
                }
            }

            // Can't save and restore the screen because inventoryExecuteCommand() does that.
            drawCavePanel();
        }

        // eliminate need to bargain if player has haggled well in the past -DJB-
        public static bool storeNoNeedToBargain(Store_t store, int min_price)
        {
            if (store.good_purchases == SHRT_MAX)
            {
                return true;
            }

            int record = (int)((store.good_purchases - 3 * store.bad_purchases - 5));

            return ((record > 0) && (record * record > min_price / 50));
        }

        // update the bargain info -DJB-
        public static void storeUpdateBargainingSkills(Store_t store, int price, int min_price)
        {
            if (min_price < 10)
            {
                return;
            }

            if (price == min_price)
            {
                if (store.good_purchases < SHRT_MAX)
                {
                    store.good_purchases++;
                }
            }
            else
            {
                if (store.bad_purchases < SHRT_MAX)
                {
                    store.bad_purchases++;
                }
            }
        }

    }
}
