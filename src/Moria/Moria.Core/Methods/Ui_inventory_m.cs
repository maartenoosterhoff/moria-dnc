using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Ui_io_m;

namespace Moria.Core.Methods
{
    public interface IUiInventory
    {
        int displayEquipment(bool weighted, int column);
        int displayInventory(int item_id_start, int item_id_end, bool weighted, int column, int[] mask);

        bool inventoryGetInputForItemId(ref int command_key_id, string prompt, int item_id_start, int item_id_end,
            int[] mask, string message);
        void inventoryExecuteCommand(char command);
        string playerItemWearingDescription(int body_location);
    }

    public class Ui_inventory_m : IUiInventory
    {
        private void inventoryItemWeightText(ref string text, int item_id)
        {
            var py = State.Instance.py;
            var total_weight = (int)(py.inventory[item_id].weight * py.inventory[item_id].items_count);
            var quotient = total_weight / 10;
            var remainder = total_weight % 10;

            text = $"{quotient:d3}.{remainder:d} lb";
            //(void)sprintf(text, "%3d.%d lb", quotient, remainder);
        }

        // Displays inventory items from `item_id_start` to `item_id_end` -RAK-
        // Designed to keep the display as far to the right as possible. -CJS-
        // The parameter col gives a column at which to start, but if the display
        // does not fit, it may be moved left.  The return value is the left edge
        // used. If mask is non-zero, then only display those items which have a
        // non-zero entry in the mask array.
        public int displayInventory(int item_id_start, int item_id_end, bool weighted, int column, int[] mask)
        {
            var py = State.Instance.py;
            var descriptions = new string[23];

            var len = 79 - column;

            int lim;
            if (weighted)
            {
                lim = 68;
            }
            else
            {
                lim = 76;
            }

            // Generate the descriptions text
            for (var i = item_id_start; i <= item_id_end; i++)
            {
                if (mask != null && (mask[i] == 0))
                {
                    continue;
                }

                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                itemDescription(ref description, py.inventory[i], true);

                // Truncate if too long.
                if (description.Length > lim)
                {
                    description = description.Substring(0, lim);
                }
                //description[lim] = 0;

                descriptions[i] = $"{(char)('a' + i):c}) {description}";
                //(void)sprintf(descriptions[i], "%c) %s", 'a' + i, description);

                var l = descriptions[i].Length + 2;
                //                int l = (int)strlen(descriptions[i]) + 2;

                if (weighted)
                {
                    l += 9;
                }

                if (l > len)
                {
                    len = l;
                }
            }

            column = 79 - len;
            if (column < 0)
            {
                column = 0;
            }

            var current_line = 1;

            // Print the descriptions
            for (var i = item_id_start; i <= item_id_end; i++)
            {
                if (mask != null && (mask[i] == 0))
                {
                    continue;
                }

                // don't need first two spaces if in first column
                if (column == 0)
                {
                    putStringClearToEOL(descriptions[i], new Coord_t(current_line, column));
                }
                else
                {
                    putString("  ", new Coord_t(current_line, column));
                    putStringClearToEOL(descriptions[i], new Coord_t(current_line, column + 2));
                }

                if (weighted)
                {
                    var text = string.Empty;
                    //obj_desc_t text = { '\0' };
                    inventoryItemWeightText(ref text, i);
                    putStringClearToEOL(text, new Coord_t(current_line, 71));
                }

                current_line++;
            }

            return column;
        }

        // Return a string describing how a given equipment item is carried. -CJS-
        public string playerItemWearingDescription(int body_location)
        {
            switch ((PlayerEquipment)body_location)
            {
                case PlayerEquipment.Wield:
                    return "wielding";
                case PlayerEquipment.Head:
                    return "wearing on your head";
                case PlayerEquipment.Neck:
                    return "wearing around your neck";
                case PlayerEquipment.Body:
                    return "wearing on your body";
                case PlayerEquipment.Arm:
                    return "wearing on your arm";
                case PlayerEquipment.Hands:
                    return "wearing on your hands";
                case PlayerEquipment.Right:
                    return "wearing on your right hand";
                case PlayerEquipment.Left:
                    return "wearing on your left hand";
                case PlayerEquipment.Feet:
                    return "wearing on your feet";
                case PlayerEquipment.Outer:
                    return "wearing about your body";
                case PlayerEquipment.Light:
                    return "using to light the way";
                case PlayerEquipment.Auxiliary:
                    return "holding ready by your side";
                default:
                    return "carrying in your pack";
            }
        }

        private string itemPositionDescription(int position_id, uint weight)
        {
            var py = State.Instance.py;

            switch ((PlayerEquipment)position_id)
            {
                case PlayerEquipment.Wield:
                    if (py.stats.used[(int)PlayerAttr.STR] * 15 < weight)
                    {
                        return "Just lifting";
                    }

                    return "Wielding";
                case PlayerEquipment.Head:
                    return "On head";
                case PlayerEquipment.Neck:
                    return "Around neck";
                case PlayerEquipment.Body:
                    return "On body";
                case PlayerEquipment.Arm:
                    return "On arm";
                case PlayerEquipment.Hands:
                    return "On hands";
                case PlayerEquipment.Right:
                    return "On right hand";
                case PlayerEquipment.Left:
                    return "On left hand";
                case PlayerEquipment.Feet:
                    return "On feet";
                case PlayerEquipment.Outer:
                    return "About body";
                case PlayerEquipment.Light:
                    return "Light source";
                case PlayerEquipment.Auxiliary:
                    return "Spare weapon";
                default:
                    return "Unknown value";
            }
        }

        // Displays equipment items from r1 to end -RAK-
        // Keep display as far right as possible. -CJS-
        public int displayEquipment(bool weighted, int column)
        {
            var py = State.Instance.py;

            var descriptions = new string[PLAYER_INVENTORY_SIZE - (int)PlayerEquipment.Wield];
            //vtype_t descriptions[PLAYER_INVENTORY_SIZE - PlayerEquipment.Wield];

            var len = 79 - column;

            int lim;
            if (weighted)
            {
                lim = 52;
            }
            else
            {
                lim = 60;
            }

            // Range of equipment
            var line = 0;
            for (var i = (int)PlayerEquipment.Wield; i < PLAYER_INVENTORY_SIZE; i++)
            {
                if (py.inventory[i].category_id == TV_NOTHING)
                {
                    continue;
                }

                // Get position
                var position_description = itemPositionDescription(i, (uint)py.inventory[i].weight);

                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                itemDescription(ref description, py.inventory[i], true);

                // Truncate if necessary
                description = description.Substring(0, lim);
                //description[lim] = 0;

                descriptions[line] = $"{(char)(line + 'a')}) {position_description:s-14}: {description}";
                //(void)sprintf(descriptions[line], "%c) %-14s: %s", line + 'a', position_description, description);

                var l = descriptions[line].Length + 2;
                //int l = (int)strlen(descriptions[line]) + 2;

                if (weighted)
                {
                    l += 9;
                }

                if (l > len)
                {
                    len = l;
                }

                line++;
            }

            column = 79 - len;
            if (column < 0)
            {
                column = 0;
            }

            // Range of equipment
            line = 0;
            for (var i = (int)PlayerEquipment.Wield; i < PLAYER_INVENTORY_SIZE; i++)
            {
                if (py.inventory[i].category_id == TV_NOTHING)
                {
                    continue;
                }

                // don't need first two spaces when using whole screen
                if (column == 0)
                {
                    putStringClearToEOL(descriptions[line], new Coord_t(line + 1, column));
                }
                else
                {
                    putString("  ", new Coord_t(line + 1, column));
                    putStringClearToEOL(descriptions[line], new Coord_t(line + 1, column + 2));
                }

                if (weighted)
                {
                    var text = string.Empty;
                    //obj_desc_t text = { '\0' };
                    inventoryItemWeightText(ref text, i);
                    putStringClearToEOL(text, new Coord_t(line + 1, 71));
                }

                line++;
            }
            eraseLine(new Coord_t(line + 1, column));

            return column;
        }

        // TODO: Most of the functions below should probably not be part of the UI,
        // TODO: but have dependencies on "screen state" variables, so need to be here for now.

        // All inventory commands (wear, exchange, take off, drop, inventory and
        // equipment) are handled in an alternative command input mode, which accepts
        // any of the inventory commands.
        //
        // It is intended that this function be called several times in succession,
        // as some commands take up a turn, and the rest of moria must proceed in the
        // interim. A global variable is provided, game.doing_inventory_command, which is normally
        // zero; however if on return from inventoryExecuteCommand() it is expected that
        // inventoryExecuteCommand() should be called *again*, (being still in inventory command
        // input mode), then game.doing_inventory_command is set to the inventory command character
        // which should be used in the next call to inventoryExecuteCommand().
        //
        // On return, the screen is restored, but not flushed. Provided no flush of
        // the screen takes place before the next call to inventoryExecuteCommand(), the inventory
        // command screen is silently redisplayed, and no actual output takes place at
        // all. If the screen is flushed before a subsequent call, then the player is
        // prompted to see if we should continue. This allows the player to see any
        // changes that take place on the screen during inventory command input.
        //
        // The global variable, screen_has_changed, is cleared by inventoryExecuteCommand(), and set
        // when the screen is flushed. This is the means by which inventoryExecuteCommand() tell
        // if the screen has been flushed.
        //
        // The display of inventory items is kept to the right of the screen to
        // minimize the work done to restore the screen afterwards. -CJS-

        // Inventory command screen states.
        const int BLANK_SCR = 0;
        const int EQUIP_SCR = 1;
        const int INVEN_SCR = 2;
        const int WEAR_SCR = 3;
        const int HELP_SCR = 4;
        const int WRONG_SCR = 5;

        // Keep track of the state of the inventory screen.
        private int screen_state, screen_left, screen_base;
        private int wear_low, wear_high;

        private void uiCommandDisplayInventoryScreen(int new_screen)
        {
            var py = State.Instance.py;

            if (new_screen == screen_state)
            {
                return;
            }

            screen_state = new_screen;

            int line;

            switch (new_screen)
            {
                case BLANK_SCR:
                    line = 0;
                    break;
                case HELP_SCR:
                    if (screen_left > 52)
                    {
                        screen_left = 52;
                    }

                    putStringClearToEOL("  ESC: exit", new Coord_t(1, screen_left));
                    putStringClearToEOL("  w  : wear or wield object", new Coord_t(2, screen_left));
                    putStringClearToEOL("  t  : take off item", new Coord_t(3, screen_left));
                    putStringClearToEOL("  d  : drop object", new Coord_t(4, screen_left));
                    putStringClearToEOL("  x  : exchange weapons", new Coord_t(5, screen_left));
                    putStringClearToEOL("  i  : inventory of pack", new Coord_t(6, screen_left));
                    putStringClearToEOL("  e  : list used equipment", new Coord_t(7, screen_left));

                    line = 7;
                    break;
                case INVEN_SCR:
                    screen_left = displayInventory(0, py.pack.unique_items - 1, Config.options.show_inventory_weights, screen_left, /*CNIL*/null);
                    line = py.pack.unique_items;
                    break;
                case WEAR_SCR:
                    screen_left = displayInventory(wear_low, wear_high, Config.options.show_inventory_weights, screen_left, /*CNIL*/null);
                    line = wear_high - wear_low + 1;
                    break;
                case EQUIP_SCR:
                    screen_left = displayEquipment(Config.options.show_inventory_weights, screen_left);
                    line = py.equipment_count;
                    break;
                default:
                    line = 0;
                    break;
            }

            if (line >= screen_base)
            {
                screen_base = line + 1;
                eraseLine(new Coord_t(screen_base, screen_left));
                return;
            }

            line++;
            while (line <= screen_base)
            {
                eraseLine(new Coord_t(line, screen_left));
                line++;
            }
        }

        // Used to verify if this really is the item we wish to -CJS-
        // wear or read.
        private bool verify(string prompt, int item)
        {
            var py = State.Instance.py;
            var description = string.Empty;
            //obj_desc_t description = { '\0' };
            itemDescription(ref description, py.inventory[item], true);

            // change the period to a question mark
            description = description.Substring(0, description.Length - 1) + "?";
            //description[strlen(description) - 1] = '?';

            var msg = $"{prompt} {description}";
            //obj_desc_t msg = { '\0' };
            //(void)sprintf(msg, "%s %s", prompt, description);

            return getInputConfirmation(msg);
        }

        private void setInventoryCommandScreenState(char command)
        {
            var game = State.Instance.game;
            // Take up where we left off after a previous inventory command. -CJS-
            if (game.doing_inventory_command != 0)
            {
                // If the screen has been flushed, we need to redraw. If the command
                // is a simple ' ' to recover the screen, just quit. Otherwise, check
                // and see what the user wants.
                if (State.Instance.screen_has_changed)
                {
                    if (command == ' ' || !getInputConfirmation("Continuing with inventory command?"))
                    {
                        game.doing_inventory_command = 0;
                        return;
                    }
                    screen_left = 50;
                    screen_base = 0;
                }

                var saved_state = screen_state;
                screen_state = WRONG_SCR;
                uiCommandDisplayInventoryScreen(saved_state);

                return;
            }

            screen_left = 50;
            screen_base = 0;

            // this forces exit of inventoryExecuteCommand() if selecting is not set true
            screen_state = BLANK_SCR;
        }

        private bool uiCommandInventoryTakeOffItem(bool selecting)
        {
            var py = State.Instance.py;
            var game = State.Instance.game;

            if (py.equipment_count == 0)
            {
                printMessage("You are not using any equipment.");
                // don't print message restarting inven command after taking off something, it is confusing
                return selecting;
            }

            if (py.pack.unique_items >= (int)PlayerEquipment.Wield && (game.doing_inventory_command == 0))
            {
                printMessage("You will have to drop something first.");
                return selecting;
            }

            if (screen_state != BLANK_SCR)
            {
                uiCommandDisplayInventoryScreen(EQUIP_SCR);
            }

            return true;
        }

        private bool uiCommandInventoryDropItem(ref char command, bool selecting)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            if (py.pack.unique_items == 0 && py.equipment_count == 0)
            {
                printMessage("But you're not carrying anything.");
                return selecting;
            }

            if (dg.floor[py.pos.y][py.pos.x].treasure_id != 0)
            {
                printMessage("There's no room to drop anything here.");
                return selecting;
            }

            if ((screen_state == EQUIP_SCR && py.equipment_count > 0) || py.pack.unique_items == 0)
            {
                if (screen_state != BLANK_SCR)
                {
                    uiCommandDisplayInventoryScreen(EQUIP_SCR);
                }

                command = 'r';
            }
            else if (screen_state != BLANK_SCR)
            {
                uiCommandDisplayInventoryScreen(INVEN_SCR);
            }

            return true;
        }

        private bool uiCommandInventoryWearWieldItem(bool selecting)
        {
            var py = State.Instance.py;

            // Note: simple loop to get the global wear_low value
            for (wear_low = 0; wear_low < py.pack.unique_items && py.inventory[wear_low].category_id > TV_MAX_WEAR; wear_low++)
                ;

            // Note: simple loop to get the global wear_high value
            for (wear_high = wear_low; wear_high < py.pack.unique_items && py.inventory[wear_high].category_id >= TV_MIN_WEAR; wear_high++)
                ;

            wear_high--;

            if (wear_low > wear_high)
            {
                printMessage("You have nothing to wear or wield.");
                return selecting;
            }

            if (screen_state != BLANK_SCR && screen_state != INVEN_SCR)
            {
                uiCommandDisplayInventoryScreen(WEAR_SCR);
            }

            return true;
        }

        private void uiCommandInventoryUnwieldItem()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            if (py.inventory[(int)PlayerEquipment.Wield].category_id == TV_NOTHING && py.inventory[(int)PlayerEquipment.Auxiliary].category_id == TV_NOTHING)
            {
                printMessage("But you are wielding no weapons.");
                return;
            }

            if ((py.inventory[(int)PlayerEquipment.Wield].flags & Config.treasure_flags.TR_CURSED) != 0u)
            {
                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                itemDescription(ref description, py.inventory[(int)PlayerEquipment.Wield], false);

                var msg = $"The {description} you are wielding appears to be cursed.";
                //obj_desc_t msg = { '\0' };
                //(void)sprintf(msg, "The %s you are wielding appears to be cursed.", description);

                printMessage(msg);

                return;
            }

            game.player_free_turn = false;

            var saved_item = py.inventory[(int)PlayerEquipment.Auxiliary];
            py.inventory[(int)PlayerEquipment.Auxiliary] = py.inventory[(int)PlayerEquipment.Wield];
            py.inventory[(int)PlayerEquipment.Wield] = saved_item;

            if (screen_state == EQUIP_SCR)
            {
                screen_left = displayEquipment(Config.options.show_inventory_weights, screen_left);
            }

            playerAdjustBonusesForItem(py.inventory[(int)PlayerEquipment.Auxiliary], -1);  // Subtract bonuses
            playerAdjustBonusesForItem(py.inventory[(int)PlayerEquipment.Wield], 1); // Add bonuses

            if (py.inventory[(int)PlayerEquipment.Wield].category_id != TV_NOTHING)
            {
                var msg_label = "Primary weapon   : ";
                //obj_desc_t msg_label = { '\0' };
                //(void)strcpy(msg_label, "Primary weapon   : ");

                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                itemDescription(ref description, py.inventory[(int)PlayerEquipment.Wield], true);

                printMessage(msg_label + description);
            }
            else
            {
                printMessage("No primary weapon.");
            }

            // this is a new weapon, so clear the heavy flag
            py.weapon_is_heavy = false;
            playerStrength();
        }

        // look for item whose inscription matches "which"
        private int inventoryGetItemMatchingInscription(char which, char command, int from, int to)
        {
            var py = State.Instance.py;
            int item;

            if (which >= '0' && which <= '9' && command != 'r' && command != 't')
            {
                int m;

                // Note: simple loop to get id
                for (m = from; m <= to && m < PLAYER_INVENTORY_SIZE && ((py.inventory[m].inscription[0] != which) || (py.inventory[m].inscription[1] != '\0')); m++)
                    ;

                if (m <= to)
                {
                    item = m;
                }
                else
                {
                    item = -1;
                }
            }
            else if (which >= 'A' && which <= 'Z')
            {
                item = which - 'A';
            }
            else
            {
                item = which - 'a';
            }

            return item;
        }

        private void buildCommandHeading(ref string str, int from, int to, string swap, char command, string prompt)
        {
            from = from + 'a';
            to = to + 'a';

            var list_items = string.Empty;
            if (screen_state == BLANK_SCR)
            {
                list_items = ", * to list";
            }

            var digits = "";
            if (command == 'w' || command == 'd')
            {
                digits = ", 0-9";
            }

            str = $"({from}-{to}{list_items}{swap}{digits}, space to break, ESC to exit) {prompt} which one?";
            //(void)sprintf(str, "(%c-%c%s%s%s, space to break, ESC to exit) %s which one?", from, to, list_items, swap, digits, prompt);
        }

        private void drawInventoryScreenForCommand(char command)
        {
            if (command == 't' || command == 'r')
            {
                uiCommandDisplayInventoryScreen(EQUIP_SCR);
            }
            else if (command == 'w' && screen_state != INVEN_SCR)
            {
                uiCommandDisplayInventoryScreen(WEAR_SCR);
            }
            else
            {
                uiCommandDisplayInventoryScreen(INVEN_SCR);
            }
        }

        private void swapInventoryScreenForDrop()
        {
            if (screen_state == EQUIP_SCR)
            {
                uiCommandDisplayInventoryScreen(INVEN_SCR);
            }
            else if (screen_state == INVEN_SCR)
            {
                uiCommandDisplayInventoryScreen(EQUIP_SCR);
            }
        }

        private int inventoryGetSlotToWearEquipment(int item)
        {
            var py = State.Instance.py;
            int slot;

            // Slot for equipment
            switch (py.inventory[item].category_id)
            {
                case TV_SLING_AMMO:
                case TV_BOLT:
                case TV_ARROW:
                case TV_BOW:
                case TV_HAFTED:
                case TV_POLEARM:
                case TV_SWORD:
                case TV_DIGGING:
                case TV_SPIKE:
                    slot = (int)PlayerEquipment.Wield;
                    break;
                case TV_LIGHT:
                    slot = (int)PlayerEquipment.Light;
                    break;
                case TV_BOOTS:
                    slot = (int)PlayerEquipment.Feet;
                    break;
                case TV_GLOVES:
                    slot = (int)PlayerEquipment.Hands;
                    break;
                case TV_CLOAK:
                    slot = (int)PlayerEquipment.Outer;
                    break;
                case TV_HELM:
                    slot = (int)PlayerEquipment.Head;
                    break;
                case TV_SHIELD:
                    slot = (int)PlayerEquipment.Arm;
                    break;
                case TV_HARD_ARMOR:
                case TV_SOFT_ARMOR:
                    slot = (int)PlayerEquipment.Body;
                    break;
                case TV_AMULET:
                    slot = (int)PlayerEquipment.Neck;
                    break;
                case TV_RING:
                    if (py.inventory[(int)PlayerEquipment.Right].category_id == TV_NOTHING)
                    {
                        slot = (int)PlayerEquipment.Right;
                    }
                    else if (py.inventory[(int)PlayerEquipment.Left].category_id == TV_NOTHING)
                    {
                        slot = (int)PlayerEquipment.Left;
                    }
                    else
                    {
                        slot = 0;

                        // Rings. Give choice over where they go.
                        do
                        {
                            var query = '\0';
                            if (!getCommand("Put ring on which hand (l/r/L/R)?", out query))
                            {
                                slot = -1;
                            }
                            else if (query == 'l')
                            {
                                slot = (int)PlayerEquipment.Left;
                            }
                            else if (query == 'r')
                            {
                                slot = (int)PlayerEquipment.Right;
                            }
                            else
                            {
                                if (query == 'L')
                                {
                                    slot = (int)PlayerEquipment.Left;
                                }
                                else if (query == 'R')
                                {
                                    slot = (int)PlayerEquipment.Right;
                                }
                                else
                                {
                                    terminalBellSound();
                                }
                                if ((slot != 0) && !verify("Replace", slot))
                                {
                                    slot = 0;
                                }
                            }
                        } while (slot == 0);
                    }
                    break;
                default:
                    slot = -1;
                    printMessage("IMPOSSIBLE: I don't see how you can use that.");
                    break;
            }

            return slot;
        }

        private void inventoryItemIsCursedMessage(int item_id)
        {
            var py = State.Instance.py;

            var description = string.Empty;
            //obj_desc_t description = { '\0' };
            itemDescription(ref description, py.inventory[item_id], false);

            var item_text = $"The {description} you are ";
            //obj_desc_t item_text = { '\0' };
            //(void)sprintf(item_text, "The %s you are ", description);

            if (item_id == (int)PlayerEquipment.Head)
            {
                item_text += "wielding ";
                //(void)strcat(item_text, "wielding ");
            }
            else
            {
                item_text += "wearing ";
                //(void)strcat(item_text, "wearing ");
            }

            printMessage(item_text + "appears to be cursed.");
            //printMessage(strcat(item_text, "appears to be cursed."));
        }

        private bool selectItemCommands(ref char command, ref char which, bool selecting)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            int item_to_take_off;
            var slot = 0;

            int from, to;
            string prompt = null;
            string swap = null;
            //const char* prompt = nullptr;
            //const char* swap = nullptr;

            while (selecting && game.player_free_turn)
            {
                swap = "";

                if (command == 'w')
                {
                    from = wear_low;
                    to = wear_high;
                    prompt = "Wear/Wield";
                }
                else
                {
                    from = 0;
                    if (command == 'd')
                    {
                        to = py.pack.unique_items - 1;
                        prompt = "Drop";

                        if (py.equipment_count > 0)
                        {
                            swap = ", / for Equip";
                        }
                    }
                    else
                    {
                        to = py.equipment_count - 1;

                        if (command == 't')
                        {
                            prompt = "Take off";
                        }
                        else
                        {
                            // command == 'r'

                            prompt = "Throw off";
                            if (py.pack.unique_items > 0)
                            {
                                swap = ", / for Inven";
                            }
                        }
                    }
                }

                if (from > to)
                {
                    selecting = false;
                    continue;
                }

                var heading_text = string.Empty;
                //obj_desc_t heading_text = { '\0' };
                buildCommandHeading(ref heading_text, from, to, swap, command, prompt);

                // Abort everything.
                if (!getCommand(heading_text, out which))
                {
                    which = ESCAPE;
                    selecting = false;
                    continue; // can we just return false from the function? -MRC-
                }

                // Draw the screen and maybe exit to main prompt.
                if (which == ' ' || which == '*')
                {
                    drawInventoryScreenForCommand(command);
                    if (which == ' ')
                    {
                        selecting = false;
                    }
                    continue;
                }

                // Swap screens (for drop)
                if (which == '/' && (swap[0] != 0))
                {
                    if (command == 'd')
                    {
                        command = 'r';
                    }
                    else
                    {
                        command = 'd';
                    }
                    swapInventoryScreenForDrop();
                    continue;
                }

                // look for item whose inscription matches "which"
                var item_id = inventoryGetItemMatchingInscription(which, command, from, to);

                if (item_id < from || item_id > to)
                {
                    terminalBellSound();
                    continue;
                }

                // Found an item!

                if (command == 'r' || command == 't')
                {
                    // Get its place in the equipment list.
                    item_to_take_off = item_id;
                    item_id = 21;

                    do
                    {
                        item_id++;
                        if (py.inventory[item_id].category_id != TV_NOTHING)
                        {
                            item_to_take_off--;
                        }
                    } while (item_to_take_off >= 0);

                    if ((char.IsUpper((char)which)) && !verify(prompt, item_id))
                    {
                        item_id = -1;
                    }
                    else if ((py.inventory[item_id].flags & Config.treasure_flags.TR_CURSED) != 0u)
                    {
                        item_id = -1;
                        printMessage("Hmmm, it seems to be cursed.");
                    }
                    else if (command == 't' && !inventoryCanCarryItemCount(py.inventory[item_id]))
                    {
                        if (dg.floor[py.pos.y][py.pos.x].treasure_id != 0)
                        {
                            item_id = -1;
                            printMessage("You can't carry it.");
                        }
                        else if (getInputConfirmation("You can't carry it.  Drop it?"))
                        {
                            command = 'r';
                        }
                        else
                        {
                            item_id = -1;
                        }
                    }

                    if (item_id >= 0)
                    {
                        if (command == 'r')
                        {
                            inventoryDropItem(item_id, true);
                            // As a safety measure, set the player's inven
                            // weight to 0, when the last object is dropped.
                            if (py.pack.unique_items == 0 && py.equipment_count == 0)
                            {
                                py.pack.weight = 0;
                            }
                        }
                        else
                        {
                            slot = inventoryCarryItem(py.inventory[item_id]);
                            playerTakeOff(item_id, slot);
                        }

                        playerStrength();

                        game.player_free_turn = false;

                        if (command == 'r')
                        {
                            selecting = false;
                        }
                    }
                }
                else if (command == 'w')
                {
                    // Wearing. Go to a bit of trouble over replacing existing equipment.

                    if ((char.IsUpper((char)which)) && !verify(prompt, item_id))
                    {
                        item_id = -1;
                    }
                    else
                    {
                        slot = inventoryGetSlotToWearEquipment(item_id);
                        if (slot == -1)
                        {
                            item_id = -1;
                        }
                    }

                    if (item_id >= 0 && py.inventory[slot].category_id != TV_NOTHING)
                    {
                        if ((py.inventory[slot].flags & Config.treasure_flags.TR_CURSED) != 0u)
                        {
                            inventoryItemIsCursedMessage(slot);
                            item_id = -1;
                        }
                        else if (py.inventory[item_id].sub_category_id == ITEM_GROUP_MIN && py.inventory[item_id].items_count > 1 && !inventoryCanCarryItemCount(py.inventory[slot]))
                        {
                            // this can happen if try to wield a torch,
                            // and have more than one in inventory
                            printMessage("You will have to drop something first.");
                            item_id = -1;
                        }
                    }

                    // OK. Wear it.
                    if (item_id >= 0)
                    {
                        game.player_free_turn = false;

                        // first remove new item from inventory
                        var saved_item = py.inventory[item_id];
                        var item = saved_item;

                        wear_high--;

                        // Fix for torches
                        if (item.items_count > 1 && item.sub_category_id <= ITEM_SINGLE_STACK_MAX)
                        {
                            item.items_count = 1;
                            wear_high++;
                        }

                        py.pack.weight += (int)(item.weight * item.items_count);

                        // Subtracts weight
                        inventoryDestroyItem(item_id);

                        // Second, add old item to inv and remove
                        // from equipment list, if necessary.
                        item = py.inventory[slot];
                        if (item.category_id != TV_NOTHING)
                        {
                            var saved_counter = py.pack.unique_items;

                            item_to_take_off = inventoryCarryItem(item);

                            // If item removed did not stack with anything
                            // in inventory, then increment wear_high.
                            if (py.pack.unique_items != saved_counter)
                            {
                                wear_high++;
                            }

                            playerTakeOff(slot, item_to_take_off);
                        }

                        // third, wear new item
                        //*item = saved_item;
                        py.inventory[item_id] = item;
                        py.equipment_count++;

                        playerAdjustBonusesForItem(item, 1);

                        //const char* text = nullptr;
                        string text = null;
                        if (slot == (int)PlayerEquipment.Wield)
                        {
                            text = "You are wielding";
                        }
                        else if (slot == (int)PlayerEquipment.Light)
                        {
                            text = "Your light source is";
                        }
                        else
                        {
                            text = "You are wearing";
                        }

                        var description = string.Empty;
                        //obj_desc_t description = { '\0' };
                        itemDescription(ref description, item, true);

                        // Get the right equipment letter.
                        item_to_take_off = (int)PlayerEquipment.Wield;
                        item_id = 0;

                        while (item_to_take_off != slot)
                        {
                            if (py.inventory[item_to_take_off++].category_id != TV_NOTHING)
                            {
                                item_id++;
                            }
                        }

                        var msg = $"{text} {description} ({(char)('a' + item_id)})";
                        //obj_desc_t msg = { '\0' };
                        //(void)sprintf(msg, "%s %s (%c)", text, description, 'a' + item_id);
                        printMessage(msg);

                        // this is a new weapon, so clear heavy flag
                        if (slot == (int)PlayerEquipment.Wield)
                        {
                            py.weapon_is_heavy = false;
                        }
                        playerStrength();

                        if ((item.flags & Config.treasure_flags.TR_CURSED) != 0u)
                        {
                            printMessage("Oops! It feels deathly cold!");
                            itemAppendToInscription(item, Config.identification.ID_DAMD);

                            // To force a cost of 0, even if unidentified.
                            item.cost = -1;
                        }
                    }
                }
                else
                {
                    // command == 'd'

                    // NOTE: initializing to `ESCAPE` as warnings were being given. -MRC-
                    var query = ESCAPE;

                    if (py.inventory[item_id].items_count > 1)
                    {
                        var description = string.Empty;
                        //obj_desc_t description = { '\0' };
                        itemDescription(ref description, py.inventory[item_id], true);
                        description = description.Substring(0, description.Length - 1) + "?";
                        //description[strlen(description) - 1] = '?';

                        var msg = $"Drop all {description} [y/n]";
                        //obj_desc_t msg = { '\0' };
                        //(void)sprintf(msg, "Drop all %s [y/n]", description);
                        msg = description.Substring(description.Length - 1) + ".";
                        //msg[strlen(description) - 1] = '.';

                        putStringClearToEOL(msg, new Coord_t(0, 0));

                        query = getKeyInput();

                        if (query != 'y' && query != 'n')
                        {
                            if (query != ESCAPE)
                            {
                                terminalBellSound();
                            }
                            messageLineClear();
                            item_id = -1;
                        }
                    }
                    else if ((char.IsUpper((char)which)) && !verify(prompt, item_id))
                    {
                        item_id = -1;
                    }
                    else
                    {
                        query = 'y';
                    }

                    if (item_id >= 0)
                    {
                        game.player_free_turn = false;

                        inventoryDropItem(item_id, query == 'y');
                        playerStrength();
                    }

                    selecting = false;

                    // As a safety measure, set the player's inven weight
                    // to 0, when the last object is dropped.
                    if (py.pack.unique_items == 0 && py.equipment_count == 0)
                    {
                        py.pack.weight = 0;
                    }
                }

                if (!game.player_free_turn && screen_state == BLANK_SCR)
                {
                    selecting = false;
                }
            }

            return selecting;
        }

        // Put an appropriate header.
        private void inventoryDisplayAppropriateHeader()
        {
            var py = State.Instance.py;
            if (screen_state == INVEN_SCR)
            {
                var msg = string.Empty;
                //obj_desc_t msg = { '\0' };
                var w_quotient = py.pack.weight / 10;
                var w_remainder = py.pack.weight % 10;

                if (!Config.options.show_inventory_weights || py.pack.unique_items == 0)
                {
                    msg = $"You are carrying {w_quotient:d}.{w_remainder:d} pounds. In your pack there is {(py.pack.unique_items == 0 ? "nothing." : "-")}";
                    //(void)sprintf(msg, "You are carrying %d.%d pounds. In your pack there is %s", w_quotient, w_remainder, (py.pack.unique_items == 0 ? "nothing." : "-"));
                }
                else
                {
                    var l_quotient = playerCarryingLoadLimit() / 10;
                    var l_remainder = playerCarryingLoadLimit() % 10;

                    msg = $"You are carrying {w_quotient:d}.{w_remainder:d} pounds. Your capacity is {l_quotient:d}.{l_remainder:d} pounds. In your pack is -";
                    //(void)sprintf(msg, "You are carrying %d.%d pounds. Your capacity is %d.%d pounds. In your pack is -", w_quotient, w_remainder, l_quotient, l_remainder);
                }

                putStringClearToEOL(msg, new Coord_t(0, 0));
            }
            else if (screen_state == WEAR_SCR)
            {
                if (wear_high < wear_low)
                {
                    putStringClearToEOL("You have nothing you could wield.", new Coord_t(0, 0));
                }
                else
                {
                    putStringClearToEOL("You could wield -", new Coord_t(0, 0));
                }
            }
            else if (screen_state == EQUIP_SCR)
            {
                if (State.Instance.py.equipment_count == 0)
                {
                    putStringClearToEOL("You are not using anything.", new Coord_t(0, 0));
                }
                else
                {
                    putStringClearToEOL("You are using -", new Coord_t(0, 0));
                }
            }
            else
            {
                putStringClearToEOL("Allowed commands:", new Coord_t(0, 0));
            }

            eraseLine(new Coord_t(screen_base, screen_left));
        }

        public void uiCommandDisplayInventory()
        {
            if (State.Instance.py.pack.unique_items == 0)
            {
                printMessage("You are not carrying anything.");
            }
            else
            {
                uiCommandDisplayInventoryScreen(INVEN_SCR);
            }
        }

        private void uiCommandDisplayEquipment()
        {
            var py = State.Instance.py;
            if (py.equipment_count == 0)
            {
                printMessage("You are not using any equipment.");
            }
            else
            {
                uiCommandDisplayInventoryScreen(EQUIP_SCR);
            }
        }

        // This does all the work.
        public void inventoryExecuteCommand(char command)
        {
            var game = State.Instance.game;

            game.player_free_turn = true;

            terminalSaveScreen();
            setInventoryCommandScreenState(command);

            do
            {
                if (char.IsUpper(command))
                {
                    command = (char)char.ToLower((char)command);
                }

                // Simple command getting and screen selection.
                var selecting = false;
                switch (command)
                {
                    case 'i':
                        uiCommandDisplayInventory();
                        break;
                    case 'e':
                        uiCommandDisplayEquipment();
                        break;
                    case 't':
                        selecting = uiCommandInventoryTakeOffItem(selecting);
                        break;
                    case 'd':
                        selecting = uiCommandInventoryDropItem(ref command, selecting);
                        break;
                    case 'w':
                        selecting = uiCommandInventoryWearWieldItem(selecting);
                        break;
                    case 'x':
                        uiCommandInventoryUnwieldItem();
                        break;
                    case ' ':
                        // Dummy command to return again to main prompt.
                        break;
                    case '?':
                        uiCommandDisplayInventoryScreen(HELP_SCR);
                        break;
                    default:
                        // Nonsense command
                        terminalBellSound();
                        break;
                }

                // Clear the game.doing_inventory_command flag here, instead of at beginning, so that
                // can use it to control when messages above appear.
                game.doing_inventory_command = 0;

                // Keep looking for objects to drop/wear/take off/throw off
                var which = 'z';

                selecting = selectItemCommands(ref command, ref which, selecting);

                if (which == ESCAPE || screen_state == BLANK_SCR)
                {
                    command = ESCAPE;
                }
                else if (!game.player_free_turn)
                {
                    // Save state for recovery if they want to call us again next turn.
                    // Otherwise, set a dummy command to recover screen.
                    if (selecting)
                    {
                        game.doing_inventory_command = command;
                    }
                    else
                    {
                        game.doing_inventory_command = ' ';
                    }

                    // flush last message before clearing screen_has_changed and exiting
                    printMessage(/*CNIL*/null);

                    // This lets us know if the world changes
                    State.Instance.screen_has_changed = false;

                    command = ESCAPE;
                }
                else
                {
                    inventoryDisplayAppropriateHeader();

                    putString("e/i/t/w/x/d/?/ESC:", new Coord_t(screen_base, 60));
                    command = getKeyInput();

                    eraseLine(new Coord_t(screen_base, screen_left));
                }
            } while (command != ESCAPE);

            if (screen_state != BLANK_SCR)
            {
                terminalRestoreScreen();
            }

            playerRecalculateBonuses();
        }

        // Get the ID of an item and return the CTR value of it -RAK-
        public bool inventoryGetInputForItemId(ref int command_key_id, string prompt, int item_id_start, int item_id_end, int[] mask, string message)
        {
            var py = State.Instance.py;
            var screen_id = 1;
            var full = false;

            if (item_id_end > (int)PlayerEquipment.Wield)
            {
                full = true;

                if (py.pack.unique_items == 0)
                {
                    screen_id = 0;
                    item_id_end = py.equipment_count - 1;
                }
                else
                {
                    item_id_end = py.pack.unique_items - 1;
                }
            }

            if (py.pack.unique_items < 1 && (!full || py.equipment_count < 1))
            {
                putStringClearToEOL("You are not carrying anything.", new Coord_t(0, 0));
                return false;
            }

            command_key_id = 0;

            var item_found = false;
            var redraw_screen = false;

            do
            {
                if (redraw_screen)
                {
                    if (screen_id > 0)
                    {
                        displayInventory(item_id_start, item_id_end, false, 80, mask);
                    }
                    else
                    {
                        displayEquipment(false, 80);
                    }
                }

                var description = string.Empty;
                //vtype_t description = { '\0' };

                if (full)
                {
                    description = string.Format(
                        "({0}: {1:c}-{2:c},{3:s}{4:s} / for {5:s}, or ESC) {6:s}", //
                        (screen_id > 0 ? "Inven" : "Equip"), //
                        (char)(item_id_start + 'a'), //
                        (char)(item_id_end + 'a'), //
                        (screen_id > 0 ? " 0-9," : ""), //
                        (redraw_screen ? "" : " * to see,"), //
                        (screen_id > 0 ? "Equip" : "Inven"), //
                        prompt //
                    );
                    //(void)sprintf(description,                            //
                    //               "(%s: %c-%c,%s%s / for %s, or ESC) %s", //
                    //               (screen_id > 0 ? "Inven" : "Equip"),    //
                    //               item_id_start + 'a',                    //
                    //               item_id_end + 'a',                      //
                    //               (screen_id > 0 ? " 0-9," : ""),         //
                    //               (redraw_screen ? "" : " * to see,"),    //
                    //               (screen_id > 0 ? "Equip" : "Inven"),    //
                    //               prompt                                  //
                    //);
                }
                else
                {
                    description = string.Format(
                        "(Items {0:c}-{1:c},{2:s}{3:s} ESC to exit) {4:s}",             //
                        (char)(item_id_start + 'a'),                             //
                        (char)(item_id_end + 'a'),                               //
                        (screen_id > 0 ? " 0-9," : ""),                  //
                        (redraw_screen ? "" : " * for inventory list,"), //
                        prompt                                           //
                    );
                    //(void)sprintf(description,                                     //
                    //               "(Items %c-%c,%s%s ESC to exit) %s",             //
                    //               item_id_start + 'a',                             //
                    //               item_id_end + 'a',                               //
                    //               (screen_id > 0 ? " 0-9," : ""),                  //
                    //               (redraw_screen ? "" : " * for inventory list,"), //
                    //               prompt                                           //
                    //);
                }

                putStringClearToEOL(description, new Coord_t(0, 0));

                var command_finished = false;

                while (!command_finished)
                {
                    var which = getKeyInput();

                    switch (which)
                    {
                        case ESCAPE:
                            screen_id = -1;
                            command_finished = true;

                            State.Instance.game.player_free_turn = true;

                            break;
                        case '/':
                            if (full)
                            {
                                if (screen_id > 0)
                                {
                                    if (py.equipment_count == 0)
                                    {
                                        putStringClearToEOL("But you're not using anything -more-", new Coord_t(0, 0));
                                        getKeyInput();
                                    }
                                    else
                                    {
                                        screen_id = 0;
                                        command_finished = true;

                                        if (redraw_screen)
                                        {
                                            item_id_end = py.equipment_count;

                                            while (item_id_end < py.pack.unique_items)
                                            {
                                                item_id_end++;
                                                eraseLine(new Coord_t(item_id_end, 0));
                                            }
                                        }
                                        item_id_end = py.equipment_count - 1;
                                    }

                                    putStringClearToEOL(description, new Coord_t(0, 0));
                                }
                                else
                                {
                                    if (py.pack.unique_items == 0)
                                    {
                                        putStringClearToEOL("But you're not carrying anything -more-", new Coord_t(0, 0));
                                        getKeyInput();
                                    }
                                    else
                                    {
                                        screen_id = 1;
                                        command_finished = true;

                                        if (redraw_screen)
                                        {
                                            item_id_end = py.pack.unique_items;

                                            while (item_id_end < py.equipment_count)
                                            {
                                                item_id_end++;
                                                eraseLine(new Coord_t(item_id_end, 0));
                                            }
                                        }
                                        item_id_end = py.pack.unique_items - 1;
                                    }
                                }
                            }
                            break;
                        case '*':
                            if (!redraw_screen)
                            {
                                command_finished = true;
                                terminalSaveScreen();
                                redraw_screen = true;
                            }
                            break;
                        default:
                            // look for item whose inscription matches "which"
                            if (which >= '0' && which <= '9' && screen_id != 0)
                            {
                                int m;

                                // Note: loop to find the inventory item
                                for (m = item_id_start; m < (int)PlayerEquipment.Wield && (py.inventory[m].inscription[0] != which || py.inventory[m].inscription[1] != '\0'); m++)
                                    ;

                                if (m < (int)PlayerEquipment.Wield)
                                {
                                    command_key_id = m;
                                }
                                else
                                {
                                    command_key_id = -1;
                                }
                            }
                            else if (char.IsUpper((char)which))
                            {
                                command_key_id = which - 'A';
                            }
                            else
                            {
                                command_key_id = which - 'a';
                            }

                            if (command_key_id >= item_id_start && command_key_id <= item_id_end && (mask == null || (mask[command_key_id] != 0)))
                            {
                                if (screen_id == 0)
                                {
                                    item_id_start = 21;
                                    item_id_end = command_key_id;

                                    do
                                    {
                                        // Note: a simple loop to find first inventory item
                                        item_id_start++;
                                        while (py.inventory[item_id_start].category_id == TV_NOTHING)
                                        {
                                            item_id_start++;
                                        }

                                        item_id_end--;
                                    } while (item_id_end >= 0);

                                    command_key_id = item_id_start;
                                }

                                if ((char.IsUpper((char)which) && !verify("Try", command_key_id)))
                                {
                                    screen_id = -1;
                                    command_finished = true;

                                    State.Instance.game.player_free_turn = true;

                                    break;
                                }

                                screen_id = -1;
                                command_finished = true;

                                item_found = true;
                            }
                            else if (!string.IsNullOrEmpty(message))
                            //else if (message != nullptr)
                            {
                                printMessage(message);

                                // Set command_finished to force redraw of the question.
                                command_finished = true;
                            }
                            else
                            {
                                terminalBellSound();
                            }
                            break;
                    }
                }
            } while (screen_id >= 0);

            if (redraw_screen)
            {
                terminalRestoreScreen();
            }

            messageLineClear();

            return item_found;
        }
    }
}
