using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Identification_c;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Recall_m;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Ui_io_m;

namespace Moria.Core.Methods
{
    public static class Identification_m
    {
        public static void SetDependencies(
            IRnd rnd,
            IStd std,
            IUiInventory uiInventory
        )
        {
            Identification_m.rnd = rnd;
            Identification_m.std = std;
            Identification_m.uiInventory = uiInventory;
        }

        private static IRnd rnd;
        private static IStd std;
        private static IUiInventory uiInventory;

        public static string objectDescription(char command)
        {
            // every printing ASCII character is listed here, in the
            // order in which they appear in the ASCII character set.
            switch (command)
            {
                case ' ':
                    return "  - An open pit.";
                case '!':
                    return "! - A potion.";
                case '"':
                    return "\" - An amulet, periapt, or necklace.";
                case '#':
                    return "# - A stone wall.";
                case '$':
                    return "$ - Treasure.";
                case '%':
                    if (!Config.options.highlight_seams)
                    {
                        return "% - Not used.";
                    }
                    return "% - A magma or quartz vein.";
                case '&':
                    return "& - Treasure chest.";
                case '\'':
                    return "' - An open door.";
                case '(':
                    return "( - Soft armor.";
                case ')':
                    return ") - A shield.";
                case '*':
                    return "* - Gems.";
                case '+':
                    return "+ - A closed door.";
                case ',':
                    return ", - Food or mushroom patch.";
                case '-':
                    return "- - A wand";
                case '.':
                    return ". - Floor.";
                case '/':
                    return "/ - A pole weapon.";
                //        case '0':
                //            return "0 - Not used.";
                case '1':
                    return "1 - Entrance to General Store.";
                case '2':
                    return "2 - Entrance to Armory.";
                case '3':
                    return "3 - Entrance to Weaponsmith.";
                case '4':
                    return "4 - Entrance to Temple.";
                case '5':
                    return "5 - Entrance to Alchemy shop.";
                case '6':
                    return "6 - Entrance to Magic-Users store.";
                // case '7':
                //     return "7 - Not used.";
                // case '8':
                //     return "8 - Not used.";
                // case '9':
                //     return "9 - Not used.";
                case ':':
                    return ": - Rubble.";
                case ';':
                    return "; - A loose rock.";
                case '<':
                    return "< - An up staircase.";
                case '=':
                    return "= - A ring.";
                case '>':
                    return "> - A down staircase.";
                case '?':
                    return "? - A scroll.";
                case '@':
                    return State.Instance.py.misc.name;
                case 'A':
                    return "A - Giant Ant Lion.";
                case 'B':
                    return "B - The Balrog.";
                case 'C':
                    return "C - Gelatinous Cube.";
                case 'D':
                    return "D - An Ancient Dragon (Beware).";
                case 'E':
                    return "E - Elemental.";
                case 'F':
                    return "F - Giant Fly.";
                case 'G':
                    return "G - Ghost.";
                case 'H':
                    return "H - Hobgoblin.";
                // case 'I':
                //     return "I - Invisible Stalker.";
                case 'J':
                    return "J - Jelly.";
                case 'K':
                    return "K - Killer Beetle.";
                case 'L':
                    return "L - Lich.";
                case 'M':
                    return "M - Mummy.";
                // case 'N':
                //     return "N - Not used.";
                case 'O':
                    return "O - Ooze.";
                case 'P':
                    return "P - Giant humanoid.";
                case 'Q':
                    return "Q - Quylthulg (Pulsing Flesh Mound).";
                case 'R':
                    return "R - Reptile.";
                case 'S':
                    return "S - Giant Scorpion.";
                case 'T':
                    return "T - Troll.";
                case 'U':
                    return "U - Umber Hulk.";
                case 'V':
                    return "V - Vampire.";
                case 'W':
                    return "W - Wight or Wraith.";
                case 'X':
                    return "X - Xorn.";
                case 'Y':
                    return "Y - Yeti.";
                // case 'Z':
                //     return "Z - Not used.";
                case '[':
                    return "[ - Hard armor.";
                case '\\':
                    return "\\ - A hafted weapon.";
                case ']':
                    return "] - Misc. armor.";
                case '^':
                    return "^ - A trap.";
                case '_':
                    return "_ - A staff.";
                // case '`':
                //     return "` - Not used.";
                case 'a':
                    return "a - Giant Ant.";
                case 'b':
                    return "b - Giant Bat.";
                case 'c':
                    return "c - Giant Centipede.";
                case 'd':
                    return "d - Dragon.";
                case 'e':
                    return "e - Floating Eye.";
                case 'f':
                    return "f - Giant Frog.";
                case 'g':
                    return "g - Golem.";
                case 'h':
                    return "h - Harpy.";
                case 'i':
                    return "i - Icky Thing.";
                case 'j':
                    return "j - Jackal.";
                case 'k':
                    return "k - Kobold.";
                case 'l':
                    return "l - Giant Louse.";
                case 'm':
                    return "m - Mold.";
                case 'n':
                    return "n - Naga.";
                case 'o':
                    return "o - Orc or Ogre.";
                case 'p':
                    return "p - Person (Humanoid).";
                case 'q':
                    return "q - Quasit.";
                case 'r':
                    return "r - Rodent.";
                case 's':
                    return "s - Skeleton.";
                case 't':
                    return "t - Giant Tick.";
                // case 'u':
                //     return "u - Not used.";
                // case 'v':
                //     return "v - Not used.";
                case 'w':
                    return "w - Worm or Worm Mass.";
                // case 'x':
                //     return "x - Not used.";
                case 'y':
                    return "y - Yeek.";
                case 'z':
                    return "z - Zombie.";
                case '{':
                    return "{ - Arrow, bolt, or bullet.";
                case '|':
                    return "| - A sword or dagger.";
                case '}':
                    return "} - Bow, crossbow, or sling.";
                case '~':
                    return "~ - Miscellaneous item.";
                default:
                    return "Not Used.";
            }
        }

        public static void identifyGameObject()
        {
            var command = '\0';
            if (!getCommand("Enter character to be identified :", out command))
            {
                return;
            }

            putStringClearToEOL(objectDescription(command), new Coord_t(0, 0));

            recallMonsterAttributes(command);
        }

        // Initialize all Potions, wands, staves, scrolls, etc.
        public static void magicInitializeItemNames()
        {
            var game = State.Instance.game;

            rnd.seedSet(game.magic_seed);

            Library.Instance.Tables.initializeItemNames(rnd);

            rnd.seedResetToOldSeed();
        }

        public static int objectPositionOffset(int category_id, int sub_category_id)
        {
            switch ((uint)category_id)
            {
                case TV_AMULET:
                    return 0;
                case TV_RING:
                    return 1;
                case TV_STAFF:
                    return 2;
                case TV_WAND:
                    return 3;
                case TV_SCROLL1:
                case TV_SCROLL2:
                    return 4;
                case TV_POTION1:
                case TV_POTION2:
                    return 5;
                case TV_FOOD:
                    if ((sub_category_id & (ITEM_SINGLE_STACK_MIN - 1)) < MAX_MUSHROOMS)
                    {
                        return 6;
                    }
                    return -1;
                default:
                    return -1;
            }
        }

        public static void clearObjectTriedFlag(int id)
        {
            State.Instance.objects_identified[id] &= ~Config.identification.OD_TRIED;
        }

        public static void setObjectTriedFlag(int id)
        {
            State.Instance.objects_identified[id] |= Config.identification.OD_TRIED;
        }

        public static bool isObjectKnown(int id)
        {
            return (State.Instance.objects_identified[id] & Config.identification.OD_KNOWN1) != 0;
        }

        // Remove "Secret" symbol for identity of object
        public static void itemSetAsIdentified(int category_id, int sub_category_id)
        {
            var id = objectPositionOffset(category_id, sub_category_id);

            if (id < 0)
            {
                return;
            }

            id <<= 6;
            id += (sub_category_id & ((int)ITEM_SINGLE_STACK_MIN - 1));

            State.Instance.objects_identified[id] |= Config.identification.OD_KNOWN1;

            // clear the tried flag, since it is now known
            clearObjectTriedFlag(id);
        }

        // Remove an automatically generated inscription. -CJS-
        public static void unsample(Inventory_t item)
        {
            // this also used to clear config::identification::ID_DAMD flag, but I think it should remain set
            item.identification &= ~(Config.identification.ID_MAGIK | Config.identification.ID_EMPTY);

            var id = objectPositionOffset((int)item.category_id, (int)item.sub_category_id);

            if (id < 0)
            {
                return;
            }

            id <<= 6;
            id += ((int)item.sub_category_id & ((int)ITEM_SINGLE_STACK_MIN - 1));

            // clear the tried flag, since it is now known
            clearObjectTriedFlag(id);
        }

        // Remove "Secret" symbol for identity of plusses
        public static void spellItemIdentifyAndRemoveRandomInscription(Inventory_t item)
        {
            unsample(item);
            item.identification |= Config.identification.ID_KNOWN2;
        }

        public static bool spellItemIdentified(Inventory_t item)
        {
            return (item.identification & Config.identification.ID_KNOWN2) != 0;
        }

        public static void spellItemRemoveIdentification(Inventory_t item)
        {
            item.identification &= ~Config.identification.ID_KNOWN2;
        }

        public static void itemIdentificationClearEmpty(Inventory_t item)
        {
            item.identification &= ~Config.identification.ID_EMPTY;
        }

        public static void itemIdentifyAsStoreBought(Inventory_t item)
        {
            item.identification |= Config.identification.ID_STORE_BOUGHT;
            spellItemIdentifyAndRemoveRandomInscription(item);
        }

        public static bool itemStoreBought(int identification)
        {
            return (identification & Config.identification.ID_STORE_BOUGHT) != 0;
        }

        // Items which don't have a 'color' are always known / itemSetAsIdentified(),
        // so that they can be carried in order in the inventory.
        public static bool itemSetColorlessAsIdentified(int category_id, int sub_category_id, int identification)
        {
            var id = objectPositionOffset(category_id, sub_category_id);

            if (id < 0)
            {
                return Config.identification.OD_KNOWN1 != 0u;
            }
            if (itemStoreBought(identification))
            {
                return Config.identification.OD_KNOWN1 != 0u;
            }

            id <<= 6;
            id += (sub_category_id & ((int)ITEM_SINGLE_STACK_MIN - 1));

            return isObjectKnown(id);
        }

        // Somethings been sampled -CJS-
        public static void itemSetAsTried(Inventory_t item)
        {
            var id = objectPositionOffset((int)item.category_id, (int)item.sub_category_id);

            if (id < 0)
            {
                return;
            }

            id <<= 6;
            id += ((int)item.sub_category_id & ((int)ITEM_SINGLE_STACK_MIN - 1));

            setObjectTriedFlag(id);
        }

        // Somethings been identified.
        // Extra complexity by CJS so that it can merge store/dungeon objects when appropriate.
        public static void itemIdentify(Inventory_t item, ref int item_id)
        {
            if ((item.flags & Config.treasure_flags.TR_CURSED) != 0u)
            {
                itemAppendToInscription(item, Config.identification.ID_DAMD);
            }

            if (itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                return;
            }

            itemSetAsIdentified((int)item.category_id, (int)item.sub_category_id);

            var cat_id = (int)item.category_id;
            var sub_cat_id = (int)item.sub_category_id;

            // no merging possible
            if (sub_cat_id < ITEM_SINGLE_STACK_MIN || sub_cat_id >= ITEM_GROUP_MIN)
            {
                return;
            }

            int j;
            var py = State.Instance.py;

            for (var i = 0; i < py.pack.unique_items; i++)
            {
                var t_ptr = py.inventory[i];

                if (t_ptr.category_id == cat_id &&
                    t_ptr.sub_category_id == sub_cat_id &&
                    i != item_id &&
                    ((int)t_ptr.items_count + (int)item.items_count) < 256)
                {
                    // make *item_id the smaller number
                    if (item_id > i)
                    {
                        j = item_id;
                        item_id = i;
                        i = j;
                    }

                    printMessage("You combine similar objects from the shop and dungeon.");

                    py.inventory[item_id].items_count += py.inventory[i].items_count;
                    py.pack.unique_items--;

                    for (j = i; j < py.pack.unique_items; j++)
                    {
                        py.inventory[j] = py.inventory[j + 1];
                    }

                    inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, py.inventory[j]);
                }
            }
        }

        // If an object has lost magical properties,
        // remove the appropriate portion of the name. -CJS-
        public static void itemRemoveMagicNaming(Inventory_t item)
        {
            item.special_name_id = (int)SpecialNameIds.SN_NULL;
        }

        public static int bowDamageValue(int misc_use)
        {
            if (misc_use == 1 || misc_use == 2)
            {
                return 2;
            }
            if (misc_use == 3 || misc_use == 5)
            {
                return 3;
            }
            if (misc_use == 4 || misc_use == 6)
            {
                return 4;
            }
            return -1;
        }

        // Set the `description` for an inventory item.
        // The `add_prefix` param indicates that an article must be added.
        // Note that since out_val can easily exceed 80 characters, itemDescription
        // must always be called with a obj_desc_t as the first parameter.
        public static void itemDescription(ref string description, Inventory_t item, bool add_prefix)
        {
            var indexx = (int)item.sub_category_id & ((int)ITEM_SINGLE_STACK_MIN - 1);

            // base name, modifier string
            var basenm = Library.Instance.Treasure.game_objects[(int)item.id].name;
            string modstr = null;

            var damstr = string.Empty;

            var append_name = false;
            var modify = !itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification);
            var misc_type = ItemMiscUse.Ignored;

            switch (item.category_id)
            {
                case TV_MISC:
                case TV_CHEST:
                    break;
                case TV_SLING_AMMO:
                case TV_BOLT:
                case TV_ARROW:
                    damstr = $" ({item.damage.dice}d{item.damage.sides})";
                    //(void)sprintf(damstr, " (%dd%d)", item.damage.dice, item.damage.sides);
                    break;
                case TV_LIGHT:
                    misc_type = ItemMiscUse.Light;
                    break;
                case TV_SPIKE:
                    break;
                case TV_BOW:
                    damstr = $" (x{bowDamageValue(item.misc_use)})";
                    //(void)sprintf(damstr, " (x%d)", bowDamageValue(item.misc_use));
                    break;
                case TV_HAFTED:
                case TV_POLEARM:
                case TV_SWORD:
                    damstr = $" ({item.damage.dice}d{item.damage.sides})";
                    //(void)sprintf(damstr, " (%dd%d)", item.damage.dice, item.damage.sides);
                    misc_type = ItemMiscUse.Flags;
                    break;
                case TV_DIGGING:
                    misc_type = ItemMiscUse.ZPlusses;
                    damstr = $" ({item.damage.sides}d{item.damage.sides})";
                    //(void)sprintf(damstr, " (%dd%d)", item.damage.sides, item.damage.sides);
                    break;
                case TV_BOOTS:
                case TV_GLOVES:
                case TV_CLOAK:
                case TV_HELM:
                case TV_SHIELD:
                case TV_HARD_ARMOR:
                case TV_SOFT_ARMOR:
                    break;
                case TV_AMULET:
                    if (modify)
                    {
                        basenm = "& {0} Amulet";
                        modstr = Library.Instance.Tables.amulets[indexx];
                    }
                    else
                    {
                        basenm = "& Amulet";
                        append_name = true;
                    }
                    misc_type = ItemMiscUse.Plusses;
                    break;
                case TV_RING:
                    if (modify)
                    {
                        basenm = "& {0} Ring";
                        modstr = Library.Instance.Tables.rocks[indexx];
                    }
                    else
                    {
                        basenm = "& Ring";
                        append_name = true;
                    }
                    misc_type = ItemMiscUse.Plusses;
                    break;
                case TV_STAFF:
                    if (modify)
                    {
                        basenm = "& {0} Staff";
                        modstr = Library.Instance.Tables.woods[indexx];
                    }
                    else
                    {
                        basenm = "& Staff";
                        append_name = true;
                    }
                    misc_type = ItemMiscUse.Charges;
                    break;
                case TV_WAND:
                    if (modify)
                    {
                        basenm = "& {0} Wand";
                        modstr = Library.Instance.Tables.metals[indexx];
                    }
                    else
                    {
                        basenm = "& Wand";
                        append_name = true;
                    }
                    misc_type = ItemMiscUse.Charges;
                    break;
                case TV_SCROLL1:
                case TV_SCROLL2:
                    if (modify)
                    {
                        basenm = "& Scroll~ titled \"{0}\"";
                        modstr = Library.Instance.Tables.magic_item_titles[indexx];
                    }
                    else
                    {
                        basenm = "& Scroll~";
                        append_name = true;
                    }
                    break;
                case TV_POTION1:
                case TV_POTION2:
                    if (modify)
                    {
                        basenm = "& {0} Potion~";
                        modstr = Library.Instance.Tables.colors[indexx];
                    }
                    else
                    {
                        basenm = "& Potion~";
                        append_name = true;
                    }
                    break;
                case TV_FLASK:
                    break;
                case TV_FOOD:
                    if (modify)
                    {
                        if (indexx <= 15)
                        {
                            basenm = "& {0} Mushroom~";
                        }
                        else if (indexx <= 20)
                        {
                            basenm = "& Hairy {0} Mold~";
                        }
                        if (indexx <= 20)
                        {
                            modstr = Library.Instance.Tables.mushrooms[indexx];
                        }
                    }
                    else
                    {
                        append_name = true;
                        if (indexx <= 15)
                        {
                            basenm = "& Mushroom~";
                        }
                        else if (indexx <= 20)
                        {
                            basenm = "& Hairy Mold~";
                        }
                        else
                        {
                            // Ordinary food does not have a name appended.
                            append_name = false;
                        }
                    }
                    break;
                case TV_MAGIC_BOOK:
                    modstr = basenm;
                    basenm = "& Book~ of Magic Spells {0}";
                    break;
                case TV_PRAYER_BOOK:
                    modstr = basenm;
                    basenm = "& Holy Book~ of Prayers {0}";
                    break;
                case TV_OPEN_DOOR:
                case TV_CLOSED_DOOR:
                case TV_SECRET_DOOR:
                case TV_RUBBLE:
                    break;
                case TV_GOLD:
                case TV_INVIS_TRAP:
                case TV_VIS_TRAP:
                case TV_UP_STAIR:
                case TV_DOWN_STAIR:
                    description = Library.Instance.Treasure.game_objects[(int)item.id].name;
                    description += ".";
                    // (void)strcpy(description, game_objects[item.id].name);
                    // (void)strcat(description, ".");
                    return;
                case TV_STORE_DOOR:
                    description = $"the entrance to the {Library.Instance.Treasure.game_objects[(int)item.id].name}.";
                    //(void)sprintf(description, "the entrance to the %s.", game_objects[item.id].name);
                    return;
                default:
                    description = "Error in objdes()";
                    //(void)strcpy(description, "Error in objdes()");
                    return;
            }

            var tmp_val = string.Empty;

            if (!string.IsNullOrEmpty(modstr))
            {
                tmp_val = string.Format(basenm, modstr);
            }
            else
            {
                tmp_val = basenm;
            }
            //if (modstr != CNIL)
            //{
            //    (void)sprintf(tmp_val, basenm, modstr);
            //}
            //else
            //{
            //    (void)strcpy(tmp_val, basenm);
            //}

            if (append_name)
            {
                tmp_val += " of ";
                tmp_val += Library.Instance.Treasure.game_objects[(int)item.id].name;
                //(void)strcat(tmp_val, " of ");
                //(void)strcat(tmp_val, game_objects[item.id].name);
            }

            if (item.items_count != 1)
            {
                tmp_val = tmp_val.Replace("ch~", "ches");
                tmp_val = tmp_val.Replace("~", "s");
                //insertStringIntoString(tmp_val, "ch~", "ches");
                //insertStringIntoString(tmp_val, "~", "s");
            }
            else
            {
                tmp_val = tmp_val.Replace("~", string.Empty);
                //insertStringIntoString(tmp_val, "~", CNIL);
            }

            if (!add_prefix)
            {
                if (tmp_val.StartsWith("some"))
                //if (strncmp("some", tmp_val, 4) == 0)
                {
                    description = tmp_val.Substring(5);
                    //(void)strcpy(description, &tmp_val[5]);
                }
                else if (tmp_val[0] == '&')
                {
                    // eliminate the '& ' at the beginning
                    tmp_val = tmp_val.Substring(2);
                    //(void)strcpy(description, &tmp_val[2]);
                }
                else
                {
                    description = tmp_val;
                    //(void)strcpy(description, tmp_val);
                }
                return;
            }

            var tmp_str = string.Empty;

            // TODO(cook): `spellItemIdentified()` is called several times in this
            // function, but `item` is immutable, so we should be able to call and
            // assign it once, then use that value everywhere below.
            if (item.special_name_id != (int)SpecialNameIds.SN_NULL && spellItemIdentified(item))
            {
                tmp_val += " ";
                tmp_val += Library.Instance.Treasure.special_item_names[(int)item.special_name_id];
                //(void)strcat(tmp_val, " ");
                //(void)strcat(tmp_val, special_item_names[item.special_name_id]);
            }

            if (!string.IsNullOrEmpty(damstr))
            //if (damstr[0] != '\0')
            {
                tmp_val += damstr;
                //(void)strcat(tmp_val, damstr);
            }

            if (spellItemIdentified(item))
            {
                var abs_to_hit = (int)std.std_abs(std.std_intmax_t(item.to_hit));
                var abs_to_damage = (int)std.std_abs(std.std_intmax_t(item.to_damage));

                if ((item.identification & Config.identification.ID_SHOW_HIT_DAM) != 0)
                {
                    tmp_str = string.Format(
                        " ({0}{1},{2}{3})",
                        (item.to_hit < 0) ? '-' : '+', abs_to_hit, (item.to_damage < 0) ? '-' : '+', abs_to_damage
                    );
                    //(void)sprintf(tmp_str, " (%c%d,%c%d)", (item.to_hit < 0) ? '-' : '+', abs_to_hit, (item.to_damage < 0) ? '-' : '+', abs_to_damage);
                }
                else if (item.to_hit != 0)
                {
                    tmp_str = string.Format(
                        "({0}{1})",
                        (item.to_hit < 0) ? '-' : '+', abs_to_hit
                    );
                    //(void)sprintf(tmp_str, " (%c%d)", (item.to_hit < 0) ? '-' : '+', abs_to_hit);
                }
                else if (item.to_damage != 0)
                {
                    tmp_str = string.Format(
                        "({0}{1})",
                        (item.to_damage < 0) ? '-' : '+', abs_to_damage
                    );
                    //(void)sprintf(tmp_str, " (%c%d)", (item.to_damage < 0) ? '-' : '+', abs_to_damage);
                }
                else
                {
                    tmp_str = string.Empty;
                }

                tmp_val += tmp_str;
                //(void)strcat(tmp_val, tmp_str);
            }

            // Crowns have a zero base AC, so make a special test for them.
            var abs_to_ac = (int)std.std_abs(std.std_intmax_t(item.to_ac));
            if (item.ac != 0 || item.category_id == TV_HELM)
            {
                tmp_str = $" [{item.ac}";
                //(void)sprintf(tmp_str, " [%d", item.ac);
                tmp_val += tmp_str;
                //(void)strcat(tmp_val, tmp_str);
                if (spellItemIdentified(item))
                {
                    // originally used %+d, but several machines don't support it
                    tmp_str = string.Format(
                        ",{0}{1}",
                        (item.to_ac < 0) ? '-' : '+', abs_to_ac
                    );
                    //(void)sprintf(tmp_str, ",%c%d", (item.to_ac < 0) ? '-' : '+', abs_to_ac);
                    tmp_val += tmp_str;
                    //(void)strcat(tmp_val, tmp_str);
                }

                tmp_val += "]";
                //(void)strcat(tmp_val, "]");
            }
            else if (item.to_ac != 0 && spellItemIdentified(item))
            {
                // originally used %+d, but several machines don't support it
                tmp_str = string.Format(
                    " [{0}{1}]",
                    (item.to_ac < 0) ? '-' : '+', abs_to_ac
                );
                //(void)sprintf(tmp_str, " [%c%d]", (item.to_ac < 0) ? '-' : '+', abs_to_ac);
                tmp_val += tmp_str;
                //(void)strcat(tmp_val, tmp_str);
            }

            // override defaults, check for `misc_type` flags in the `item.identification` field
            if ((item.identification & Config.identification.ID_NO_SHOW_P1) != 0)
            {
                misc_type = ItemMiscUse.Ignored;
            }
            else if ((item.identification & Config.identification.ID_SHOW_P1) != 0)
            {
                misc_type = ItemMiscUse.ZPlusses;
            }

            tmp_str = string.Empty;

            if (misc_type == ItemMiscUse.Light)
            {
                tmp_str = $" with {item.misc_use} turns of light";
                //(void)sprintf(tmp_str, " with %d turns of light", item.misc_use);
            }
            else if (misc_type == ItemMiscUse.Ignored)
            {
                // NOOP
            }
            else if (spellItemIdentified(item))
            {
                var abs_misc_use = (int)std.std_abs(std.std_intmax_t(item.misc_use));

                if (misc_type == ItemMiscUse.ZPlusses)
                {
                    // originally used %+d, but several machines don't support it
                    tmp_str = string.Format(" ({0}{1})", (item.misc_use < 0) ? '-' : '+', abs_misc_use);
                    //(void)sprintf(tmp_str, " (%c%d)", (item.misc_use < 0) ? '-' : '+', abs_misc_use);
                }
                else if (misc_type == ItemMiscUse.Charges)
                {
                    tmp_str = string.Format(" ({0} charges)", item.misc_use);
                    //(void)sprintf(tmp_str, " (%d charges)", item.misc_use);
                }
                else if (item.misc_use != 0)
                {
                    if (misc_type == ItemMiscUse.Plusses)
                    {
                        tmp_str = string.Format(" ({0}{1})", (item.misc_use < 0) ? '-' : '+', abs_misc_use);
                        //(void)sprintf(tmp_str, " (%c%d)", (item.misc_use < 0) ? '-' : '+', abs_misc_use);
                    }
                    else if (misc_type == ItemMiscUse.Flags)
                    {
                        if ((item.flags & Config.treasure_flags.TR_STR) != 0u)
                        {
                            tmp_str = string.Format(" ({0}{1} to STR)", (item.misc_use < 0) ? '-' : '+', abs_misc_use);
                            //(void)sprintf(tmp_str, " (%c%d to STR)", (item.misc_use < 0) ? '-' : '+', abs_misc_use);
                        }
                        else if ((item.flags & Config.treasure_flags.TR_STEALTH) != 0u)
                        {
                            tmp_str = string.Format(" ({0}{1} to stealth)", (item.misc_use < 0) ? '-' : '+', abs_misc_use);
                            //(void)sprintf(tmp_str, " (%c%d to stealth)", (item.misc_use < 0) ? '-' : '+', abs_misc_use);
                        }
                    }
                }
            }

            tmp_val += tmp_str;
            //(void)strcat(tmp_val, tmp_str);

            // ampersand is always the first character
            if (tmp_val[0] == '&')
            {
                // use &tmp_val[1], so that & does not appear in output
                if (item.items_count > 1)
                {
                    description += string.Format("{0}{1}", (int)item.items_count, tmp_val.Substring(1));
                    //(void)sprintf(description, "%d%s", (int)item.items_count, &tmp_val[1]);
                }
                else if (item.items_count < 1)
                {
                    description = string.Format("{0}{1}", "no more", tmp_val.Substring(1));
                    //(void)sprintf(description, "%s%s", "no more", &tmp_val[1]);
                }
                else if (isVowel(tmp_val[2]))
                {
                    description = $"an{tmp_val.Substring(1)}";
                    //(void)sprintf(description, "an%s", &tmp_val[1]);
                }
                else
                {
                    description = $"a {tmp_val.Substring(1)}";
                    //(void)sprintf(description, "a%s", &tmp_val[1]);
                }
            }
            else if (item.items_count < 1)
            {
                // handle 'no more' case specially

                // check for "some" at start
                if (tmp_val.StartsWith("some"))
                //if (strncmp("some", tmp_val, 4) == 0)
                {
                    description = $"no more {tmp_val.Substring(5)}";
                    //(void)sprintf(description, "no more %s", &tmp_val[5]);
                }
                else
                {
                    // here if no article
                    description = $"no more {tmp_val}";
                    //(void)sprintf(description, "no more %s", tmp_val);
                }
            }
            else
            {
                description = tmp_val;
                //(void)strcpy(description, tmp_val);
            }

            tmp_str = string.Empty;

            if ((indexx = objectPositionOffset((int)item.category_id, (int)item.sub_category_id)) >= 0)
            {
                indexx <<= 6;
                indexx += ((int)item.sub_category_id & ((int)ITEM_SINGLE_STACK_MIN - 1));

                // don't print tried string for store bought items
                if (((State.Instance.objects_identified[indexx] & Config.identification.OD_TRIED) != 0) && !itemStoreBought((int)item.identification))
                {
                    tmp_str = "tried ";
                    //(void)strcat(tmp_str, "tried ");
                }
            }

            if ((item.identification & (Config.identification.ID_MAGIK |
                                        Config.identification.ID_EMPTY |
                                        Config.identification.ID_DAMD)) != 0)
            {
                if ((item.identification & Config.identification.ID_MAGIK) != 0)
                {
                    tmp_str += "magik ";
                    //(void)strcat(tmp_str, "magik ");
                }
                if ((item.identification & Config.identification.ID_EMPTY) != 0)
                {
                    tmp_str += "empty ";
                    //(void)strcat(tmp_str, "empty ");
                }
                if ((item.identification & Config.identification.ID_DAMD) != 0)
                {
                    tmp_str += "damned ";
                    //(void)strcat(tmp_str, "damned ");
                }
            }

            if (!string.IsNullOrEmpty(item.inscription))
            //if (item.inscription[0] != '\0')
            {
                tmp_str += item.inscription;
                //(void)strcat(tmp_str, item.inscription);
            }
            else
            {
                tmp_str = tmp_str.Trim();
                //indexx = tmp_str.Length;
                //if (indexx > 0)
                //{
                //    // remove the extra blank at the end
                //    tmp_str[indexx - 1] = '\0';
                //}
            }

            if (!string.IsNullOrEmpty(tmp_str))
            //if (tmp_str[0] != 0)
            {

                tmp_val = $" {{{tmp_str}}}";
                //(void)sprintf(tmp_val, " {%s}", tmp_str);
                description += tmp_val;
                //(void)strcat(description, tmp_val);
            }

            description += ".";
            //(void)strcat(description, ".");
        }

        // Describe number of remaining charges. -RAK-
        public static void itemChargesRemainingDescription(int item_id)
        {
            var py = State.Instance.py;

            if (!spellItemIdentified(py.inventory[item_id]))
            {
                return;
            }

            var rem_num = py.inventory[item_id].misc_use;

            var out_val = string.Empty;
            //vtype_t out_val = { '\0' };
            out_val = $"You have {rem_num} charges remaining.";
            //(void)sprintf(out_val, "You have %d charges remaining.", rem_num);
            printMessage(out_val);
        }

        // Describe amount of item remaining. -RAK-
        public static void itemTypeRemainingCountDescription(int item_id)
        {
            var py = State.Instance.py;

            var item = py.inventory[item_id];

            item.items_count--;

            var tmp_str = string.Empty;
            //obj_desc_t tmp_str = { '\0' };
            itemDescription(ref tmp_str, item, true);

            item.items_count++;

            // the string already has a dot at the end.
            var out_val = string.Empty;
            //obj_desc_t out_val = { '\0' };
            out_val = $"You have {tmp_str}";
            //(void)sprintf(out_val, "You have %s", tmp_str);
            printMessage(out_val);
        }

        // Add a comment to an object description. -CJS-
        public static void itemInscribe()
        {
            var py = State.Instance.py;
            if (py.pack.unique_items == 0 && py.equipment_count == 0)
            {
                printMessage("You are not carrying anything to inscribe.");
                return;
            }

            var item_id = 0;
            if (!uiInventory.inventoryGetInputForItemId(ref item_id, "Which one? ", 0, (int)PLAYER_INVENTORY_SIZE, null /*CNIL*/, null /*CNIL*/))
            {
                return;
            }

            var msg = string.Empty;
            //obj_desc_t msg = { '\0' };
            itemDescription(ref msg, py.inventory[item_id], true);

            var inscription = string.Empty;
            //obj_desc_t inscription = { '\0' };
            inscription = $"Inscribing {msg}";
            //(void)sprintf(inscription, "Inscribing %s", msg);

            printMessage(inscription);

            if (!string.IsNullOrEmpty(py.inventory[item_id].inscription))
            //if (py.inventory[item_id].inscription[0] != '\0')
            {
                inscription = $"Replace {py.inventory[item_id].inscription} New inscription:";
                //(void)sprintf(inscription, "Replace %s New inscription:", py.inventory[item_id].inscription);
            }
            else
            {
                inscription = "Inscription: ";
                //(void)strcpy(inscription, "Inscription: ");
            }

            var msg_len = 78 - msg.Length;//(int)strlen(msg);
            if (msg_len > 12)
            {
                msg_len = 12;
            }

            putStringClearToEOL(inscription, new Coord_t(0, 0));

            if (getStringInput(out inscription, new Coord_t(0, inscription.Length), msg_len))
            {
                itemReplaceInscription(py.inventory[item_id], inscription);
            }
        }

        // Append an additional comment to an object description. -CJS-
        public static void itemAppendToInscription(Inventory_t item, uint item_ident_type)
        {
            item.identification |= item_ident_type;
        }

        // Replace any existing comment in an object description with a new one. -CJS-
        public static void itemReplaceInscription(Inventory_t item, string inscription)
        {
            item.inscription = inscription;
            //(void)strcpy(item.inscription, inscription);
        }

        public static void objectBlockedByMonster(int monster_id)
        {
            var description = string.Empty;
            var msg = string.Empty;

            var monster = State.Instance.monsters[monster_id];
            var name = Library.Instance.Creatures.creatures_list[(int)monster.creature_id].name;

            if (monster.lit)
            {
                description = $"The {name}";
                //(void)sprintf(description, "The %s", name);
            }
            else
            {
                description = "Something";
                //(void)strcpy(description, "Something");
            }

            msg = $"{description} is in your way!";
            //(void)sprintf(msg, "%s is in your way!", description);
            printMessage(msg);
        }
    }
}
