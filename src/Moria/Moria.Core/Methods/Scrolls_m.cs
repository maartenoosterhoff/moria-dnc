using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_m;

namespace Moria.Core.Methods
{
    public static class Scrolls_m
    {
        public static void SetDependencies(
            IDice dice,
            IMonsterManager monsterManager,
            IPlayerMagic playerMagic,
            IRnd rnd,
            IUiInventory uiInventory
        )
        {
            Scrolls_m.dice = dice;
            Scrolls_m.monsterManager = monsterManager;
            Scrolls_m.playerMagic = playerMagic;
            Scrolls_m.rnd = rnd;
            Scrolls_m.uiInventory = uiInventory;
        }

        private static IDice dice;
        private static IMonsterManager monsterManager;
        private static IPlayerMagic playerMagic;
        private static IRnd rnd;
        private static IUiInventory uiInventory;

        // Note: naming of all the scroll functions needs verifying -MRC-

        public static bool playerCanReadScroll(ref int item_pos_start, ref int item_pos_end)
        {
            var py = State.Instance.py;
            if (py.flags.blind > 0)
            {
                printMessage("You can't see to read the scroll.");
                return false;
            }

            if (playerNoLight())
            {
                printMessage("You have no light to read by.");
                return false;
            }

            if (py.flags.confused > 0)
            {
                printMessage("You are too confused to read a scroll.");
                return false;
            }

            if (py.pack.unique_items == 0)
            {
                printMessage("You are not carrying anything!");
                return false;
            }

            if (!inventoryFindRange((int)TV_SCROLL1, (int)TV_SCROLL2, ref item_pos_start, ref item_pos_end))
            {
                printMessage("You are not carrying any scrolls!");
                return false;
            }

            return true;
        }

        static int inventoryItemIdOfCursedEquipment()
        {
            var py = State.Instance.py;

            var item_count = 0;
            var items = new int[6];

            if (py.inventory[(int)PlayerEquipment.Body].category_id != TV_NOTHING)
            {
                items[item_count] = (int)PlayerEquipment.Body;
                item_count++;
            }
            if (py.inventory[(int)PlayerEquipment.Arm].category_id != TV_NOTHING)
            {
                items[item_count] = (int)PlayerEquipment.Arm;
                item_count++;
            }
            if (py.inventory[(int)PlayerEquipment.Outer].category_id != TV_NOTHING)
            {
                items[item_count] = (int)PlayerEquipment.Outer;
                item_count++;
            }
            if (py.inventory[(int)PlayerEquipment.Hands].category_id != TV_NOTHING)
            {
                items[item_count] = (int)PlayerEquipment.Hands;
                item_count++;
            }
            if (py.inventory[(int)PlayerEquipment.Head].category_id != TV_NOTHING)
            {
                items[item_count] = (int)PlayerEquipment.Head;
                item_count++;
            }
            // also enchant boots
            if (py.inventory[(int)PlayerEquipment.Feet].category_id != TV_NOTHING)
            {
                items[item_count] = (int)PlayerEquipment.Feet;
                item_count++;
            }

            var item_id = 0;

            if (item_count > 0)
            {
                item_id = items[rnd.randomNumber(item_count) - 1];
            }

            if ((py.inventory[(int)PlayerEquipment.Body].flags & Config.treasure_flags.TR_CURSED) != 0u)
            {
                item_id = (int)PlayerEquipment.Body;
            }
            else if ((py.inventory[(int)PlayerEquipment.Arm].flags & Config.treasure_flags.TR_CURSED) != 0u)
            {
                item_id = (int)PlayerEquipment.Arm;
            }
            else if ((py.inventory[(int)PlayerEquipment.Outer].flags & Config.treasure_flags.TR_CURSED) != 0u)
            {
                item_id = (int)PlayerEquipment.Outer;
            }
            else if ((py.inventory[(int)PlayerEquipment.Head].flags & Config.treasure_flags.TR_CURSED) != 0u)
            {
                item_id = (int)PlayerEquipment.Head;
            }
            else if ((py.inventory[(int)PlayerEquipment.Hands].flags & Config.treasure_flags.TR_CURSED) != 0u)
            {
                item_id = (int)PlayerEquipment.Hands;
            }
            else if ((py.inventory[(int)PlayerEquipment.Feet].flags & Config.treasure_flags.TR_CURSED) != 0u)
            {
                item_id = (int)PlayerEquipment.Feet;
            }

            return item_id;
        }

        public static bool scrollEnchantWeaponToHit()
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (item.category_id == TV_NOTHING)
            {
                return false;
            }

            var msg = string.Empty;
            var desc = string.Empty;
            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            itemDescription(ref desc, item, false);

            msg = $"Your {desc} glows faintly!";
            //(void)sprintf(msg, "Your %s glows faintly!", desc);
            printMessage(msg);

            var toHit = item.to_hit;
            var spellEnchantItemResult = spellEnchantItem(ref toHit, 10);
            item.to_hit = toHit;
            if (spellEnchantItemResult)
            {
                item.flags &= ~Config.treasure_flags.TR_CURSED;
                playerRecalculateBonuses();
            }
            else
            {
                printMessage("The enchantment fails.");
            }

            return true;
        }

        public static bool scrollEnchantWeaponToDamage()
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (item.category_id == TV_NOTHING)
            {
                return false;
            }

            var msg = string.Empty;
            var desc = string.Empty;
            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            itemDescription(ref desc, item, false);

            msg = $"Your {desc} glows faintly!";
            //(void)sprintf(msg, "Your %s glows faintly!", desc);
            printMessage(msg);

            int scroll_type;

            if (item.category_id >= TV_HAFTED && item.category_id <= TV_DIGGING)
            {
                scroll_type = dice.maxDiceRoll(item.damage);
            }
            else
            {
                // Bows' and arrows' enchantments should not be
                // limited by their low base damages
                scroll_type = 10;
            }

            var toDamage = item.to_damage;
            var spellEnchantItemResult = spellEnchantItem(ref toDamage, scroll_type);
            item.to_damage = toDamage;
            if (spellEnchantItemResult)
            {
                item.flags &= ~Config.treasure_flags.TR_CURSED;
                playerRecalculateBonuses();
            }
            else
            {
                printMessage("The enchantment fails.");
            }

            return true;
        }

        public static bool scrollEnchantItemToAC()
        {
            var py = State.Instance.py;
            var item_id = inventoryItemIdOfCursedEquipment();

            if (item_id <= 0)
            {
                return false;
            }

            var item = py.inventory[item_id];

            var msg = string.Empty;
            var desc = string.Empty;
            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            itemDescription(ref desc, item, false);

            msg = $"Your {desc} glows faintly!";
            printMessage(msg);

            var toAc = item.to_ac;
            var spellEnchantItemResult = spellEnchantItem(ref toAc, 10);
            item.to_ac = toAc;
            if (spellEnchantItemResult)
            {
                item.flags &= ~Config.treasure_flags.TR_CURSED;
                playerRecalculateBonuses();
            }
            else
            {
                printMessage("The enchantment fails.");
            }

            return true;
        }

        public static int scrollIdentifyItem(int item_id, ref bool is_used_up)
        {
            var py = State.Instance.py;

            printMessage("This is an identify scroll.");

            is_used_up = spellIdentifyItem();

            // The identify may merge objects, causing the identify scroll
            // to move to a different place.  Check for that here.  It can
            // move arbitrarily far if an identify scroll was used on
            // another identify scroll, but it always moves down.
            var item = py.inventory[item_id];
            while (item_id > 0 && (item.category_id != TV_SCROLL1 || item.flags != 0x00000008))
            {
                item_id--;
                item = py.inventory[item_id];
            }

            return item_id;
        }

        public static bool scrollRemoveCurse()
        {
            if (spellRemoveCurseFromAllItems())
            {
                printMessage("You feel as if someone is watching over you.");
                return true;
            }
            return false;
        }

        public static bool scrollSummonMonster()
        {
            var py = State.Instance.py;
            var identified = false;
            var coord = new Coord_t(0, 0);

            for (var i = 0; i < rnd.randomNumber(3); i++)
            {
                coord.y = py.pos.y;
                coord.x = py.pos.x;
                identified |= monsterManager.monsterSummon(coord, false);
            }

            return identified;
        }

        public static void scrollTeleportLevel()
        {
            var dg = State.Instance.dg;

            dg.current_level += (-3) + 2 * rnd.randomNumber(2);
            if (dg.current_level < 1)
            {
                dg.current_level = 1;
            }
            dg.generate_new_level = true;
        }

        public static bool scrollConfuseMonster()
        {
            var py = State.Instance.py;

            if (!py.flags.confuse_monster)
            {
                printMessage("Your hands begin to glow.");
                py.flags.confuse_monster = true;
                return true;
            }
            return false;
        }

        public static bool scrollEnchantWeapon()
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (item.category_id == TV_NOTHING)
            {
                return false;
            }

            var msg = string.Empty;
            var desc = string.Empty;
            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            itemDescription(ref desc, item, false);

            msg = $"Your {desc} glows brightly!";
            //(void)sprintf(msg, "Your %s glows brightly!", desc);
            printMessage(msg);

            var enchanted = false;

            for (var i = 0; i < rnd.randomNumber(2); i++)
            {
                var toHit = item.to_hit;
                var spellEnchantItemResult = spellEnchantItem(ref toHit, 10);
                item.to_hit = toHit;
                if (spellEnchantItemResult)
                {
                    enchanted = true;
                }
            }

            int scroll_type;

            if (item.category_id >= TV_HAFTED && item.category_id <= TV_DIGGING)
            {
                scroll_type = dice.maxDiceRoll(item.damage);
            }
            else
            {
                // Bows' and arrows' enchantments should not be limited
                // by their low base damages
                scroll_type = 10;
            }

            for (var i = 0; i < rnd.randomNumber(2); i++)
            {
                var toDamage = item.to_damage;
                var spellEnchantItemResult = spellEnchantItem(ref toDamage, scroll_type);
                item.to_damage = toDamage;
                if (spellEnchantItemResult)
                {
                    enchanted = true;
                }
            }

            if (enchanted)
            {
                item.flags &= ~Config.treasure_flags.TR_CURSED;
                playerRecalculateBonuses();
            }
            else
            {
                printMessage("The enchantment fails.");
            }

            return true;
        }

        public static bool scrollCurseWeapon()
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (item.category_id == TV_NOTHING)
            {
                return false;
            }

            var msg = string.Empty;
            var desc = string.Empty;
            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            itemDescription(ref desc, item, false);

            msg = $"Your {desc} glows black, fades.";
            //(void)sprintf(msg, "Your %s glows black, fades.", desc);
            printMessage(msg);

            itemRemoveMagicNaming(item);

            item.to_hit = (int)(-rnd.randomNumber(5) - rnd.randomNumber(5));
            item.to_damage = (int)(-rnd.randomNumber(5) - rnd.randomNumber(5));
            item.to_ac = 0;

            // Must call playerAdjustBonusesForItem() before set (clear) flags, and
            // must call playerRecalculateBonuses() after set (clear) flags, so that
            // all attributes will be properly turned off.
            playerAdjustBonusesForItem(item, -1);
            item.flags = Config.treasure_flags.TR_CURSED;
            playerRecalculateBonuses();

            return true;
        }

        public static bool scrollEnchantArmor()
        {
            var py = State.Instance.py;

            var item_id = inventoryItemIdOfCursedEquipment();

            if (item_id <= 0)
            {
                return false;
            }

            var item = py.inventory[item_id];

            var msg = string.Empty;
            var desc = string.Empty;
            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            itemDescription(ref desc, item, false);

            msg = $"Your {desc} glows brightly!";
            //(void)sprintf(msg, "Your %s glows brightly!", desc);
            printMessage(msg);

            var enchanted = false;

            for (var i = 0; i < rnd.randomNumber(2) + 1; i++)
            {
                var toAc = item.to_ac;
                var spellEnchantItemResult = spellEnchantItem(ref toAc, 10);
                item.to_ac = toAc;
                if (spellEnchantItemResult)
                {
                    enchanted = true;
                }
            }

            if (enchanted)
            {
                item.flags &= ~Config.treasure_flags.TR_CURSED;
                playerRecalculateBonuses();
            }
            else
            {
                printMessage("The enchantment fails.");
            }

            return true;
        }

        public static bool scrollCurseArmor()
        {
            var py = State.Instance.py;
            int item_id;

            if (py.inventory[(int)PlayerEquipment.Body].category_id != TV_NOTHING && rnd.randomNumber(4) == 1)
            {
                item_id = (int)PlayerEquipment.Body;
            }
            else if (py.inventory[(int)PlayerEquipment.Arm].category_id != TV_NOTHING && rnd.randomNumber(3) == 1)
            {
                item_id = (int)PlayerEquipment.Arm;
            }
            else if (py.inventory[(int)PlayerEquipment.Outer].category_id != TV_NOTHING && rnd.randomNumber(3) == 1)
            {
                item_id = (int)PlayerEquipment.Outer;
            }
            else if (py.inventory[(int)PlayerEquipment.Head].category_id != TV_NOTHING && rnd.randomNumber(3) == 1)
            {
                item_id = (int)PlayerEquipment.Head;
            }
            else if (py.inventory[(int)PlayerEquipment.Hands].category_id != TV_NOTHING && rnd.randomNumber(3) == 1)
            {
                item_id = (int)PlayerEquipment.Hands;
            }
            else if (py.inventory[(int)PlayerEquipment.Feet].category_id != TV_NOTHING && rnd.randomNumber(3) == 1)
            {
                item_id = (int)PlayerEquipment.Feet;
            }
            else if (py.inventory[(int)PlayerEquipment.Body].category_id != TV_NOTHING)
            {
                item_id = (int)PlayerEquipment.Body;
            }
            else if (py.inventory[(int)PlayerEquipment.Arm].category_id != TV_NOTHING)
            {
                item_id = (int)PlayerEquipment.Arm;
            }
            else if (py.inventory[(int)PlayerEquipment.Outer].category_id != TV_NOTHING)
            {
                item_id = (int)PlayerEquipment.Outer;
            }
            else if (py.inventory[(int)PlayerEquipment.Head].category_id != TV_NOTHING)
            {
                item_id = (int)PlayerEquipment.Head;
            }
            else if (py.inventory[(int)PlayerEquipment.Hands].category_id != TV_NOTHING)
            {
                item_id = (int)PlayerEquipment.Hands;
            }
            else if (py.inventory[(int)PlayerEquipment.Feet].category_id != TV_NOTHING)
            {
                item_id = (int)PlayerEquipment.Feet;
            }
            else
            {
                item_id = 0;
            }

            if (item_id <= 0)
            {
                return false;
            }

            var item = py.inventory[item_id];

            var msg = string.Empty;
            var desc = string.Empty;
            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            itemDescription(ref desc, item, false);

            msg = $"Your {desc} glows black, fades.";
            //(void)sprintf(msg, "Your %s glows black, fades.", desc);
            printMessage(msg);

            itemRemoveMagicNaming(item);

            item.flags = Config.treasure_flags.TR_CURSED;
            item.to_hit = 0;
            item.to_damage = 0;
            item.to_ac = (-rnd.randomNumber(5) - rnd.randomNumber(5));

            playerRecalculateBonuses();

            return true;
        }

        public static bool scrollSummonUndead()
        {
            var py = State.Instance.py;
            var identified = false;
            var coord = new Coord_t(0, 0);

            for (var i = 0; i < rnd.randomNumber(3); i++)
            {
                coord.y = py.pos.y;
                coord.x = py.pos.x;
                identified |= monsterManager.monsterSummonUndead(coord);
            }

            return identified;
        }

        public static void scrollWordOfRecall()
        {
            var py = State.Instance.py;
            if (py.flags.word_of_recall == 0)
            {
                py.flags.word_of_recall = (25 + rnd.randomNumber(30));
            }
            printMessage("The air about you becomes charged.");
        }

        // Scrolls for the reading -RAK-
        public static void scrollRead()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            int item_pos_start = 0, item_pos_end = 0;
            if (!playerCanReadScroll(ref item_pos_start, ref item_pos_end))
            {
                return;
            }

            var item_id = 0;
            if (!uiInventory.inventoryGetInputForItemId(ref item_id, "Read which scroll?", item_pos_start, item_pos_end, /*CNIL*/null, /*CNIL*/null))
            {
                return;
            }

            // From here on, no free turn for the player
            game.player_free_turn = false;

            var used_up = true;
            var identified = false;

            var item = py.inventory[item_id];
            var item_flags = item.flags;

            while (item_flags != 0)
            {
                var scroll_type = getAndClearFirstBit(ref item_flags) + 1;

                if (item.category_id == TV_SCROLL2)
                {
                    scroll_type += 32;
                }

                switch (scroll_type)
                {
                    case 1:
                        identified = scrollEnchantWeaponToHit();
                        break;
                    case 2:
                        identified = scrollEnchantWeaponToDamage();
                        break;
                    case 3:
                        identified = scrollEnchantItemToAC();
                        break;
                    case 4:
                        item_id = scrollIdentifyItem(item_id, ref used_up);
                        identified = true;
                        break;
                    case 5:
                        identified = scrollRemoveCurse();
                        break;
                    case 6:
                        identified = spellLightArea(py.pos);
                        break;
                    case 7:
                        identified = scrollSummonMonster();
                        break;
                    case 8:
                        playerTeleport(10); // Teleport Short, aka Phase Door
                        identified = true;
                        break;
                    case 9:
                        playerTeleport(100); // Teleport Long
                        identified = true;
                        break;
                    case 10:
                        scrollTeleportLevel();
                        identified = true;
                        break;
                    case 11:
                        identified = scrollConfuseMonster();
                        break;
                    case 12:
                        spellMapCurrentArea();
                        identified = true;
                        break;
                    case 13:
                        identified = monsterSleep(py.pos);
                        break;
                    case 14:
                        spellWardingGlyph();
                        identified = true;
                        break;
                    case 15:
                        identified = spellDetectTreasureWithinVicinity();
                        break;
                    case 16:
                        identified = spellDetectObjectsWithinVicinity();
                        break;
                    case 17:
                        identified = spellDetectTrapsWithinVicinity();
                        break;
                    case 18:
                        identified = spellDetectSecretDoorssWithinVicinity();
                        break;
                    case 19:
                        printMessage("This is a mass genocide scroll.");
                        spellMassGenocide();
                        identified = true;
                        break;
                    case 20:
                        identified = spellDetectInvisibleCreaturesWithinVicinity();
                        break;
                    case 21:
                        printMessage("There is a high pitched humming noise.");
                        spellAggravateMonsters(20);
                        identified = true;
                        break;
                    case 22:
                        identified = spellSurroundPlayerWithTraps();
                        break;
                    case 23:
                        identified = spellDestroyAdjacentDoorsTraps();
                        break;
                    case 24:
                        identified = spellSurroundPlayerWithDoors();
                        break;
                    case 25:
                        printMessage("This is a Recharge-Item scroll.");
                        used_up = spellRechargeItem(60);
                        identified = true;
                        break;
                    case 26:
                        printMessage("This is a genocide scroll.");
                        spellGenocide();
                        identified = true;
                        break;
                    case 27:
                        identified = spellDarkenArea(py.pos);
                        break;
                    case 28:
                        identified = playerMagic.playerProtectEvil();
                        break;
                    case 29:
                        spellCreateFood();
                        identified = true;
                        break;
                    case 30:
                        identified = spellDispelCreature((int)Config.monsters_defense.CD_UNDEAD, 60);
                        break;
                    case 33:
                        identified = scrollEnchantWeapon();
                        break;
                    case 34:
                        identified = scrollCurseWeapon();
                        break;
                    case 35:
                        identified = scrollEnchantArmor();
                        break;
                    case 36:
                        identified = scrollCurseArmor();
                        break;
                    case 37:
                        identified = scrollSummonUndead();
                        break;
                    case 38:
                        playerMagic.playerBless(rnd.randomNumber(12) + 6);
                        identified = true;
                        break;
                    case 39:
                        playerMagic.playerBless(rnd.randomNumber(24) + 12);
                        identified = true;
                        break;
                    case 40:
                        playerMagic.playerBless(rnd.randomNumber(48) + 24);
                        identified = true;
                        break;
                    case 41:
                        scrollWordOfRecall();
                        identified = true;
                        break;
                    case 42:
                        spellDestroyArea(py.pos);
                        identified = true;
                        break;
                    default:
                        printMessage("Internal error in scroll()");
                        break;
                }
            }

            item = py.inventory[item_id];

            if (identified)
            {
                if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
                {
                    // round half-way case up
                    py.misc.exp += (int)((item.depth_first_found + (py.misc.level >> 1)) / py.misc.level);
                    displayCharacterExperience();

                    itemIdentify(py.inventory[item_id], ref item_id);
                }
            }
            else if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                itemSetAsTried(item);
            }

            if (used_up)
            {
                itemTypeRemainingCountDescription(item_id);
                inventoryDestroyItem(item_id);
            }
        }

    }
}
