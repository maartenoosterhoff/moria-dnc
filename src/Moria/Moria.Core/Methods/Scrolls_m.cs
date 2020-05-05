using Moria.Core.Configs;
using Moria.Core.Methods.Commands.SpellCasting;
using Moria.Core.Methods.Commands.SpellCasting.Attacking;
using Moria.Core.Methods.Commands.SpellCasting.Defending;
using Moria.Core.Methods.Commands.SpellCasting.Destroying;
using Moria.Core.Methods.Commands.SpellCasting.Detection;
using Moria.Core.Methods.Commands.SpellCasting.Lighting;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Player_m;

namespace Moria.Core.Methods
{
    public interface IScrolls
    {
        void scrollRead();
    }

    public class Scrolls_m : IScrolls
    {
        private readonly IDice dice;
        private readonly IEventPublisher eventPublisher;
        private readonly IHelpers helpers;
        private readonly IIdentification identification;
        private readonly IInventoryManager inventoryManager;
        private readonly IMonster monster;
        private readonly IMonsterManager monsterManager;
        private readonly IPlayerMagic playerMagic;
        private readonly IRnd rnd;
        private readonly ITerminal terminal;
        private readonly ITerminalEx terminalEx;
        private readonly IUiInventory uiInventory;

        public Scrolls_m(
            IDice dice,
            IEventPublisher eventPublisher,
            IHelpers helpers,
            IIdentification identification,
            IInventoryManager inventoryManager,
            IMonster monster,
            IMonsterManager monsterManager,
            IPlayerMagic playerMagic,
            IRnd rnd,
            ITerminal terminal,
            ITerminalEx terminalEx,
            IUiInventory uiInventory
        )
        {
            this.dice = dice;
            this.eventPublisher = eventPublisher;
            this.helpers = helpers;
            this.identification = identification;
            this.inventoryManager = inventoryManager;
            this.monster = monster;
            this.monsterManager = monsterManager;
            this.playerMagic = playerMagic;
            this.rnd = rnd;
            this.terminal = terminal;
            this.terminalEx = terminalEx;
            this.uiInventory = uiInventory;
        }

        // Note: naming of all the scroll functions needs verifying -MRC-

        private bool playerCanReadScroll(ref int item_pos_start, ref int item_pos_end)
        {
            var py = State.Instance.py;
            if (py.flags.blind > 0)
            {
                this.terminal.printMessage("You can't see to read the scroll.");
                return false;
            }

            if (this.helpers.playerNoLight())
            {
                this.terminal.printMessage("You have no light to read by.");
                return false;
            }

            if (py.flags.confused > 0)
            {
                this.terminal.printMessage("You are too confused to read a scroll.");
                return false;
            }

            if (py.pack.unique_items == 0)
            {
                this.terminal.printMessage("You are not carrying anything!");
                return false;
            }

            if (!this.inventoryManager.inventoryFindRange((int)TV_SCROLL1, (int)TV_SCROLL2, out item_pos_start, out item_pos_end))
            {
                this.terminal.printMessage("You are not carrying any scrolls!");
                return false;
            }

            return true;
        }

        private int inventoryItemIdOfCursedEquipment()
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
                item_id = items[this.rnd.randomNumber(item_count) - 1];
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

        private bool scrollEnchantWeaponToHit()
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (item.category_id == TV_NOTHING)
            {
                return false;
            }

            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            this.identification.itemDescription(out var desc, item, false);

            var msg = $"Your {desc} glows faintly!";
            //(void)sprintf(msg, "Your %s glows faintly!", desc);
            this.terminal.printMessage(msg);

            var toHit = item.to_hit;
            var command = new EnchantItemCommand(toHit, 10);
            var spellEnchantItemResult = this.eventPublisher.PublishWithOutputBool(command);
            toHit = command.Plusses;
            //var spellEnchantItemResult = spellEnchantItem(ref toHit, 10);
            item.to_hit = toHit;
            if (spellEnchantItemResult)
            {
                item.flags &= ~Config.treasure_flags.TR_CURSED;
                playerRecalculateBonuses();
            }
            else
            {
                this.terminal.printMessage("The enchantment fails.");
            }

            return true;
        }

        private bool scrollEnchantWeaponToDamage()
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (item.category_id == TV_NOTHING)
            {
                return false;
            }

            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            this.identification.itemDescription(out var desc, item, false);

            var msg = $"Your {desc} glows faintly!";
            //(void)sprintf(msg, "Your %s glows faintly!", desc);
            this.terminal.printMessage(msg);

            int scroll_type;

            if (item.category_id >= TV_HAFTED && item.category_id <= TV_DIGGING)
            {
                scroll_type = this.dice.maxDiceRoll(item.damage);
            }
            else
            {
                // Bows' and arrows' enchantments should not be
                // limited by their low base damages
                scroll_type = 10;
            }

            var toDamage = item.to_damage;
            var command = new EnchantItemCommand(toDamage, scroll_type);
            var spellEnchantItemResult = this.eventPublisher.PublishWithOutputBool(command);
            toDamage = command.Plusses;
            //var spellEnchantItemResult = spellEnchantItem(ref toDamage, scroll_type);
            item.to_damage = toDamage;
            if (spellEnchantItemResult)
            {
                item.flags &= ~Config.treasure_flags.TR_CURSED;
                playerRecalculateBonuses();
            }
            else
            {
                this.terminal.printMessage("The enchantment fails.");
            }

            return true;
        }

        private bool scrollEnchantItemToAC()
        {
            var py = State.Instance.py;
            var item_id = this.inventoryItemIdOfCursedEquipment();

            if (item_id <= 0)
            {
                return false;
            }

            var item = py.inventory[item_id];

            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            this.identification.itemDescription(out var desc, item, false);

            var msg = $"Your {desc} glows faintly!";
            this.terminal.printMessage(msg);

            var toAc = item.to_ac;
            var command = new EnchantItemCommand(toAc, 10);
            var spellEnchantItemResult = this.eventPublisher.PublishWithOutputBool(command);
            toAc = command.Plusses;
            //var spellEnchantItemResult = spellEnchantItem(ref toAc, 10);
            item.to_ac = toAc;
            if (spellEnchantItemResult)
            {
                item.flags &= ~Config.treasure_flags.TR_CURSED;
                playerRecalculateBonuses();
            }
            else
            {
                this.terminal.printMessage("The enchantment fails.");
            }

            return true;
        }

        private int scrollIdentifyItem(int item_id, ref bool is_used_up)
        {
            var py = State.Instance.py;

            this.terminal.printMessage("This is an identify scroll.");

            is_used_up = this.eventPublisher.PublishWithOutputBool(new IdentifyItemCommand());
            //is_used_up = spellIdentifyItem();

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

        private bool scrollRemoveCurse()
        {
            if (this.eventPublisher.PublishWithOutputBool(new RemoveCurseFromAllItemsCommand()))
            //if (spellRemoveCurseFromAllItems())
            {
                this.terminal.printMessage("You feel as if someone is watching over you.");
                return true;
            }
            return false;
        }

        private bool scrollSummonMonster()
        {
            var py = State.Instance.py;
            var identified = false;
            var coord = new Coord_t(0, 0);

            for (var i = 0; i < this.rnd.randomNumber(3); i++)
            {
                coord.y = py.pos.y;
                coord.x = py.pos.x;
                identified |= this.monsterManager.monsterSummon(coord, false);
            }

            return identified;
        }

        private void scrollTeleportLevel()
        {
            var dg = State.Instance.dg;

            dg.current_level += -3 + 2 * this.rnd.randomNumber(2);
            if (dg.current_level < 1)
            {
                dg.current_level = 1;
            }
            dg.generate_new_level = true;
        }

        private bool scrollConfuseMonster()
        {
            var py = State.Instance.py;

            if (!py.flags.confuse_monster)
            {
                this.terminal.printMessage("Your hands begin to glow.");
                py.flags.confuse_monster = true;
                return true;
            }
            return false;
        }

        private bool scrollEnchantWeapon()
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (item.category_id == TV_NOTHING)
            {
                return false;
            }

            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            this.identification.itemDescription(out var desc, item, false);

            var msg = $"Your {desc} glows brightly!";
            //(void)sprintf(msg, "Your %s glows brightly!", desc);
            this.terminal.printMessage(msg);

            var enchanted = false;

            for (var i = 0; i < this.rnd.randomNumber(2); i++)
            {
                var toHit = item.to_hit;
                var command = new EnchantItemCommand(toHit, 10);
                var spellEnchantItemResult = this.eventPublisher.PublishWithOutputBool(command);
                toHit = command.Plusses;
                //var spellEnchantItemResult = spellEnchantItem(ref toHit, 10);
                item.to_hit = toHit;
                if (spellEnchantItemResult)
                {
                    enchanted = true;
                }
            }

            int scroll_type;

            if (item.category_id >= TV_HAFTED && item.category_id <= TV_DIGGING)
            {
                scroll_type = this.dice.maxDiceRoll(item.damage);
            }
            else
            {
                // Bows' and arrows' enchantments should not be limited
                // by their low base damages
                scroll_type = 10;
            }

            for (var i = 0; i < this.rnd.randomNumber(2); i++)
            {
                var toDamage = item.to_damage;
                var command = new EnchantItemCommand(toDamage, scroll_type);
                var spellEnchantItemResult = this.eventPublisher.PublishWithOutputBool(command);
                toDamage = command.Plusses;
                //var spellEnchantItemResult = spellEnchantItem(ref toDamage, scroll_type);
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
                this.terminal.printMessage("The enchantment fails.");
            }

            return true;
        }

        private bool scrollCurseWeapon()
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (item.category_id == TV_NOTHING)
            {
                return false;
            }

            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            this.identification.itemDescription(out var desc, item, false);

            var msg = $"Your {desc} glows black, fades.";
            //(void)sprintf(msg, "Your %s glows black, fades.", desc);
            this.terminal.printMessage(msg);

            this.identification.itemRemoveMagicNaming(item);

            item.to_hit = (int)(-this.rnd.randomNumber(5) - this.rnd.randomNumber(5));
            item.to_damage = (int)(-this.rnd.randomNumber(5) - this.rnd.randomNumber(5));
            item.to_ac = 0;

            // Must call playerAdjustBonusesForItem() before set (clear) flags, and
            // must call playerRecalculateBonuses() after set (clear) flags, so that
            // all attributes will be properly turned off.
            playerAdjustBonusesForItem(item, -1);
            item.flags = Config.treasure_flags.TR_CURSED;
            playerRecalculateBonuses();

            return true;
        }

        private bool scrollEnchantArmor()
        {
            var py = State.Instance.py;

            var item_id = this.inventoryItemIdOfCursedEquipment();

            if (item_id <= 0)
            {
                return false;
            }

            var item = py.inventory[item_id];

            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            this.identification.itemDescription(out var desc, item, false);

            var msg = $"Your {desc} glows brightly!";
            //(void)sprintf(msg, "Your %s glows brightly!", desc);
            this.terminal.printMessage(msg);

            var enchanted = false;

            for (var i = 0; i < this.rnd.randomNumber(2) + 1; i++)
            {
                var toAc = item.to_ac;
                var command = new EnchantItemCommand(toAc, 10);
                var spellEnchantItemResult = this.eventPublisher.PublishWithOutputBool(command);
                toAc = command.Plusses;
                //var spellEnchantItemResult = spellEnchantItem(ref toAc, 10);
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
                this.terminal.printMessage("The enchantment fails.");
            }

            return true;
        }

        private bool scrollCurseArmor()
        {
            var py = State.Instance.py;
            int item_id;

            if (py.inventory[(int)PlayerEquipment.Body].category_id != TV_NOTHING && this.rnd.randomNumber(4) == 1)
            {
                item_id = (int)PlayerEquipment.Body;
            }
            else if (py.inventory[(int)PlayerEquipment.Arm].category_id != TV_NOTHING && this.rnd.randomNumber(3) == 1)
            {
                item_id = (int)PlayerEquipment.Arm;
            }
            else if (py.inventory[(int)PlayerEquipment.Outer].category_id != TV_NOTHING && this.rnd.randomNumber(3) == 1)
            {
                item_id = (int)PlayerEquipment.Outer;
            }
            else if (py.inventory[(int)PlayerEquipment.Head].category_id != TV_NOTHING && this.rnd.randomNumber(3) == 1)
            {
                item_id = (int)PlayerEquipment.Head;
            }
            else if (py.inventory[(int)PlayerEquipment.Hands].category_id != TV_NOTHING && this.rnd.randomNumber(3) == 1)
            {
                item_id = (int)PlayerEquipment.Hands;
            }
            else if (py.inventory[(int)PlayerEquipment.Feet].category_id != TV_NOTHING && this.rnd.randomNumber(3) == 1)
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

            //obj_desc_t msg = { '\0' };
            //obj_desc_t desc = { '\0' };
            this.identification.itemDescription(out var desc, item, false);

            var msg = $"Your {desc} glows black, fades.";
            //(void)sprintf(msg, "Your %s glows black, fades.", desc);
            this.terminal.printMessage(msg);

            this.identification.itemRemoveMagicNaming(item);

            item.flags = Config.treasure_flags.TR_CURSED;
            item.to_hit = 0;
            item.to_damage = 0;
            item.to_ac = -this.rnd.randomNumber(5) - this.rnd.randomNumber(5);

            playerRecalculateBonuses();

            return true;
        }

        private bool scrollSummonUndead()
        {
            var py = State.Instance.py;
            var identified = false;
            var coord = new Coord_t(0, 0);

            for (var i = 0; i < this.rnd.randomNumber(3); i++)
            {
                coord.y = py.pos.y;
                coord.x = py.pos.x;
                identified |= this.monsterManager.monsterSummonUndead(coord);
            }

            return identified;
        }

        private void scrollWordOfRecall()
        {
            var py = State.Instance.py;
            if (py.flags.word_of_recall == 0)
            {
                py.flags.word_of_recall = 25 + this.rnd.randomNumber(30);
            }

            this.terminal.printMessage("The air about you becomes charged.");
        }

        // Scrolls for the reading -RAK-
        public void scrollRead()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            int item_pos_start = 0, item_pos_end = 0;
            if (!this.playerCanReadScroll(ref item_pos_start, ref item_pos_end))
            {
                return;
            }

            var item_id = 0;
            if (!this.uiInventory.inventoryGetInputForItemId(out item_id, "Read which scroll?", item_pos_start, item_pos_end, /*CNIL*/null, /*CNIL*/null))
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
                var scroll_type = this.helpers.getAndClearFirstBit(ref item_flags) + 1;

                if (item.category_id == TV_SCROLL2)
                {
                    scroll_type += 32;
                }

                switch (scroll_type)
                {
                    case 1:
                        identified = this.scrollEnchantWeaponToHit();
                        break;
                    case 2:
                        identified = this.scrollEnchantWeaponToDamage();
                        break;
                    case 3:
                        identified = this.scrollEnchantItemToAC();
                        break;
                    case 4:
                        item_id = this.scrollIdentifyItem(item_id, ref used_up);
                        identified = true;
                        break;
                    case 5:
                        identified = this.scrollRemoveCurse();
                        break;
                    case 6:
                        identified = this.eventPublisher.PublishWithOutputBool(new LightAreaCommand(py.pos));
                        //identified = spellLightArea(py.pos);
                        break;
                    case 7:
                        identified = this.scrollSummonMonster();
                        break;
                    case 8:
                        this.eventPublisher.Publish(new TeleportCommand(10));
                        //playerTeleport(10); // Teleport Short, aka Phase Door
                        identified = true;
                        break;
                    case 9:
                        this.eventPublisher.Publish(new TeleportCommand(100));
                        //playerTeleport(100); // Teleport Long
                        identified = true;
                        break;
                    case 10:
                        this.scrollTeleportLevel();
                        identified = true;
                        break;
                    case 11:
                        identified = this.scrollConfuseMonster();
                        break;
                    case 12:
                        this.eventPublisher.Publish(new MapCurrentAreaCommand());
                        //spellMapCurrentArea();
                        identified = true;
                        break;
                    case 13:
                        identified = this.monster.monsterSleep(py.pos);
                        break;
                    case 14:
                        this.eventPublisher.Publish(new WardingGlyphCommand());
                        //spellWardingGlyph();
                        identified = true;
                        break;
                    case 15:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectTreasureWithinVicinityCommand());
                        //identified = spellDetectTreasureWithinVicinity();
                        break;
                    case 16:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectObjectsWithinVicinityCommand());
                        //identified = spellDetectObjectsWithinVicinity();
                        break;
                    case 17:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectTrapsWithinVicinityCommand());
                        //identified = spellDetectTrapsWithinVicinity();
                        break;
                    case 18:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectSecretDoorsWithinVicinityCommand());
                        //identified = spellDetectSecretDoorssWithinVicinity();
                        break;
                    case 19:
                        this.terminal.printMessage("This is a mass genocide scroll.");
                        this.eventPublisher.Publish(new MassGenocideCommand());
                        //spellMassGenocide();
                        identified = true;
                        break;
                    case 20:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectInvisibleCreaturesWithinVicinityCommand());
                        //identified = spellDetectInvisibleCreaturesWithinVicinity();
                        break;
                    case 21:
                        this.terminal.printMessage("There is a high pitched humming noise.");
                        this.eventPublisher.Publish(new AggravateMonstersCommand(20));
                        //spellAggravateMonsters(20);
                        identified = true;
                        break;
                    case 22:
                        identified = this.eventPublisher.PublishWithOutputBool(new SurroundPlayerWithTrapsCommand());
                        //identified = spellSurroundPlayerWithTraps();
                        break;
                    case 23:
                        identified = this.eventPublisher.PublishWithOutputBool(new DestroyAdjacentDoorsTrapsCommand());
                        //identified = spellDestroyAdjacentDoorsTraps();
                        break;
                    case 24:
                        identified = this.eventPublisher.PublishWithOutputBool(new SurroundPlayerWithDoorsCommand());
                        //identified = spellSurroundPlayerWithDoors();
                        break;
                    case 25:
                        this.terminal.printMessage("This is a Recharge-Item scroll.");
                        used_up = this.eventPublisher.PublishWithOutputBool(new RechargeItemCommand(60));
                        //used_up = spellRechargeItem(60);
                        identified = true;
                        break;
                    case 26:
                        this.terminal.printMessage("This is a genocide scroll.");
                        this.eventPublisher.Publish(new GenocideCommand());
                        //spellGenocide();
                        identified = true;
                        break;
                    case 27:
                        identified = this.eventPublisher.PublishWithOutputBool(new DarkenAreaCommand(py.pos));
                        //identified = spellDarkenArea(py.pos);
                        break;
                    case 28:
                        identified = this.playerMagic.playerProtectEvil();
                        break;
                    case 29:
                        this.eventPublisher.Publish(new CreateFoodCommand());
                        //spellCreateFood();
                        identified = true;
                        break;
                    case 30:
                        identified = this.eventPublisher.PublishWithOutputBool(new DispelCreatureCommand(
                            (int)Config.monsters_defense.CD_UNDEAD, 60
                        ));
                        //identified = spellDispelCreature((int)Config.monsters_defense.CD_UNDEAD, 60);
                        break;
                    case 33:
                        identified = this.scrollEnchantWeapon();
                        break;
                    case 34:
                        identified = this.scrollCurseWeapon();
                        break;
                    case 35:
                        identified = this.scrollEnchantArmor();
                        break;
                    case 36:
                        identified = this.scrollCurseArmor();
                        break;
                    case 37:
                        identified = this.scrollSummonUndead();
                        break;
                    case 38:
                        this.playerMagic.playerBless(this.rnd.randomNumber(12) + 6);
                        identified = true;
                        break;
                    case 39:
                        this.playerMagic.playerBless(this.rnd.randomNumber(24) + 12);
                        identified = true;
                        break;
                    case 40:
                        this.playerMagic.playerBless(this.rnd.randomNumber(48) + 24);
                        identified = true;
                        break;
                    case 41:
                        this.scrollWordOfRecall();
                        identified = true;
                        break;
                    case 42:
                        this.eventPublisher.Publish(new DestroyAreaCommand(py.pos));
                        //spellDestroyArea(py.pos);
                        identified = true;
                        break;
                    default:
                        this.terminal.printMessage("Internal error in scroll()");
                        break;
                }
            }

            item = py.inventory[item_id];

            if (identified)
            {
                if (!this.identification.itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
                {
                    // round half-way case up
                    py.misc.exp += (int)((item.depth_first_found + (py.misc.level >> 1)) / py.misc.level);
                    this.terminalEx.displayCharacterExperience();

                    this.identification.itemIdentify(py.inventory[item_id], ref item_id);
                }
            }
            else if (!this.identification.itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                this.identification.itemSetAsTried(item);
            }

            if (used_up)
            {
                this.identification.itemTypeRemainingCountDescription(item_id);
                this.inventoryManager.inventoryDestroyItem(item_id);
            }
        }
    }
}
