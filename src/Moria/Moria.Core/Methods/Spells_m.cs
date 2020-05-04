using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.SpellCasting.Defending;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Mage_spells_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Spells_m
    {
        public static void SetDependencies(
            IDice dice,
            IDungeon dungeon,
            IHelpers helpers,
            IInventory inventory,
            IInventoryManager inventoryManager,
            IRnd rnd,
            IUiInventory uiInventory
        )
        {
            Spells_m.dice = dice;
            Spells_m.dungeon = dungeon;
            Spells_m.helpers = helpers;
            Spells_m.inventory = inventory;
            Spells_m.inventoryManager = inventoryManager;
            Spells_m.rnd = rnd;
            Spells_m.uiInventory = uiInventory;
        }

        private static IDice dice;
        private static IDungeon dungeon;
        private static IHelpers helpers;
        private static IInventory inventory;
        private static IInventoryManager inventoryManager;
        private static IRnd rnd;
        private static IUiInventory uiInventory;

        // Returns spell pointer -RAK-
        public static bool spellGetId(int[] spell_ids, int number_of_choices, ref int spell_id, ref int spell_chance, string prompt, int first_spell)
        {
            var py = State.Instance.py;
            var magic_spells = Library.Instance.Player.magic_spells;
            var spell_names = Library.Instance.Player.spell_names;
            spell_id = -1;

            var str = string.Empty;
            //vtype_t str = { '\0' };
            str = string.Format(
                "(Spells {0}-{1}, *=List, <ESCAPE>=exit) {2}",
                (char)(spell_ids[0] + 'a' - first_spell),
                (char)(spell_ids[number_of_choices - 1] + 'a' - first_spell),
                prompt
            );
            //(void)sprintf(str, "(Spells %c-%c, *=List, <ESCAPE>=exit) %s", spell_ids[0] + 'a' - first_spell, spell_ids[number_of_choices - 1] + 'a' - first_spell, prompt);

            var spell_found = false;
            var redraw = false;

            var offset = Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells == (int)Config.spells.SPELL_TYPE_MAGE ? (int)Config.spells.NAME_OFFSET_SPELLS : (int)Config.spells.NAME_OFFSET_PRAYERS;

            var choice = '\0';

            while (!spell_found && getCommand(str, out choice))
            {
                if (char.IsUpper(choice))
                //if (isupper((int)choice) != 0)
                {
                    spell_id = choice - 'A' + first_spell;

                    // verify that this is in spells[], at most 22 entries in class_to_use_mage_spells[]
                    int test_spell_id;
                    for (test_spell_id = 0; test_spell_id < number_of_choices; test_spell_id++)
                    {
                        if (spell_id == spell_ids[test_spell_id])
                        {
                            break;
                        }
                    }

                    if (test_spell_id == number_of_choices)
                    {
                        spell_id = -2;
                    }
                    else
                    {
                        var spell = magic_spells[(int)py.misc.class_id - 1][spell_id];

                        var tmp_str = $"Cast {spell_names[spell_id + offset]} ({spell.mana_required} mana, {spellChanceOfSuccess(spell_id)}% fail)?";
                        //vtype_t tmp_str = { '\0' };
                        //(void)sprintf(tmp_str, "Cast %s (%d mana, %d%% fail)?", spell_names[spell_id + offset], spell.mana_required, spellChanceOfSuccess(spell_id));
                        if (getInputConfirmation(tmp_str))
                        {
                            spell_found = true;
                        }
                        else
                        {
                            spell_id = -1;
                        }
                    }
                }
                else if (char.IsLower(choice))
                {
                    spell_id = choice - 'a' + first_spell;

                    // verify that this is in spells[], at most 22 entries in class_to_use_mage_spells[]
                    int test_spell_id;
                    for (test_spell_id = 0; test_spell_id < number_of_choices; test_spell_id++)
                    {
                        if (spell_id == spell_ids[test_spell_id])
                        {
                            break;
                        }
                    }

                    if (test_spell_id == number_of_choices)
                    {
                        spell_id = -2;
                    }
                    else
                    {
                        spell_found = true;
                    }
                }
                else if (choice == '*')
                {
                    // only do this drawing once
                    if (!redraw)
                    {
                        terminalSaveScreen();
                        redraw = true;
                        displaySpellsList(spell_ids, number_of_choices, false, first_spell);
                    }
                }
                else if (char.IsLetter(choice))
                //else if (isalpha((int)choice) != 0)
                {
                    spell_id = -2;
                }
                else
                {
                    spell_id = -1;
                    terminalBellSound();
                }

                if (spell_id == -2)
                {
                    var spellOrPrayer = offset == Config.spells.NAME_OFFSET_SPELLS ? "spell" : "prayer";
                    var tmp_str = $"You don't know that {spellOrPrayer}.";
                    //vtype_t tmp_str = { '\0' };
                    //(void)sprintf(tmp_str, "You don't know that %s.", (offset == Config.spells.NAME_OFFSET_SPELLS ? "spell" : "prayer"));
                    printMessage(tmp_str);
                }
            }

            if (redraw)
            {
                terminalRestoreScreen();
            }

            messageLineClear();

            if (spell_found)
            {
                spell_chance = spellChanceOfSuccess(spell_id);
            }

            return spell_found;
        }

        // Return spell number and failure chance -RAK-
        // returns -1 if no spells in book
        // returns  1 if choose a spell in book to cast
        // returns  0 if don't choose a spell, i.e. exit with an escape
        // TODO: split into two functions; getting spell ID and casting an actual spell
        public static int castSpellGetId(string prompt, int item_id, ref int spell_id, ref int spell_chance)
        {
            var py = State.Instance.py;
            // NOTE: `flags` gets set again, since getAndClearFirstBit modified it
            var flags = py.inventory[item_id].flags;
            var first_spell = helpers.getAndClearFirstBit(ref flags);
            flags = py.inventory[item_id].flags & py.flags.spells_learnt;

            // TODO(cook) move access to `magic_spells[]` directly to the for loop it's used in, below?
            var spells = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1];

            var spell_count = 0;
            var spell_list = new int[31];

            while (flags != 0u)
            {
                var pos = helpers.getAndClearFirstBit(ref flags);

                if (spells[pos].level_required <= py.misc.level)
                {
                    spell_list[spell_count] = pos;
                    spell_count++;
                }
            }

            if (spell_count == 0)
            {
                return -1;
            }

            var result = 0;
            if (spellGetId(spell_list, spell_count, ref spell_id, ref spell_chance, prompt, first_spell))
            {
                result = 1;
            }

            if (result != 0 && Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1][spell_id].mana_required > py.misc.current_mana)
            {
                if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
                {
                    result = (int)(getInputConfirmation("You summon your limited strength to cast this one! Confirm?") ? 1 : 0);
                }
                else
                {
                    result = (int)(getInputConfirmation("The gods may think you presumptuous for this! Confirm?") ? 1 : 0);
                }
            }

            return result;
        }

        // Return flags for given type area affect -RAK-
        public static void spellGetAreaAffectFlags(int spell_type, out uint weapon_type, out int harm_type, out Func<Inventory_t, bool> destroy)
        {
            switch ((MagicSpellFlags)spell_type)
            {
                case MagicSpellFlags.MagicMissile:
                    weapon_type = 0;
                    harm_type = 0;
                    destroy = inventory.setNull;
                    break;
                case MagicSpellFlags.Lightning:
                    weapon_type = Config.monsters_spells.CS_BR_LIGHT;
                    harm_type = (int)Config.monsters_defense.CD_LIGHT;
                    destroy = inventory.setLightningDestroyableItems;
                    break;
                case MagicSpellFlags.PoisonGas:
                    weapon_type = Config.monsters_spells.CS_BR_GAS;
                    harm_type = (int)Config.monsters_defense.CD_POISON;
                    destroy = inventory.setNull;
                    break;
                case MagicSpellFlags.Acid:
                    weapon_type = Config.monsters_spells.CS_BR_ACID;
                    harm_type = (int)Config.monsters_defense.CD_ACID;
                    destroy = inventory.setAcidDestroyableItems;
                    break;
                case MagicSpellFlags.Frost:
                    weapon_type = Config.monsters_spells.CS_BR_FROST;
                    harm_type = (int)Config.monsters_defense.CD_FROST;
                    destroy = inventory.setFrostDestroyableItems;
                    break;
                case MagicSpellFlags.Fire:
                    weapon_type = Config.monsters_spells.CS_BR_FIRE;
                    harm_type = (int)Config.monsters_defense.CD_FIRE;
                    destroy = inventory.setFireDestroyableItems;
                    break;
                case MagicSpellFlags.HolyOrb:
                    weapon_type = 0;
                    harm_type = (int)Config.monsters_defense.CD_EVIL;
                    destroy = inventory.setNull;
                    break;
                default:
                    weapon_type = 0;
                    harm_type = 0;
                    destroy = inventory.setNull;
                    printMessage("ERROR in spellGetAreaAffectFlags()\n");
                    break;
            }
        }

        

        // Create a wall. -RAK-
        public static bool spellBuildWall(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var built = false;
            var finished = false;

            while (!finished)
            {
                helpers.movePosition(direction, ref coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue; // we're done here, break out of the loop
                }

                if (tile.treasure_id != 0)
                {
                    dungeon.dungeonDeleteObject(coord);
                }

                if (tile.creature_id > 1)
                {
                    finished = true;

                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if ((creature.movement & Config.monsters_move.CM_PHASE) == 0u)
                    {
                        // monster does not move, can't escape the wall
                        int damage;
                        if ((creature.movement & Config.monsters_move.CM_ATTACK_ONLY) != 0u)
                        {
                            // this will kill everything
                            damage = 3000;
                        }
                        else
                        {
                            damage = dice.diceRoll(new Dice_t(4, 8));
                        }

                        var name = monsterNameDescription(creature.name, monster.lit);

                        printMonsterActionText(name, "wails out in pain!");

                        if (monsterTakeHit((int)tile.creature_id, damage) >= 0)
                        {
                            printMonsterActionText(name, "is embedded in the rock.");
                            displayCharacterExperience();
                        }
                    }
                    else if (creature.sprite == 'E' || creature.sprite == 'X')
                    {
                        // must be an earth elemental, an earth spirit,
                        // or a Xorn to increase its hit points
                        monster.hp += dice.diceRoll(new Dice_t(4, 8));
                    }
                }

                tile.feature_id = TILE_MAGMA_WALL;
                tile.field_mark = false;

                // Permanently light this wall if it is lit by player's lamp.
                tile.permanent_light = tile.temporary_light || tile.permanent_light;
                dungeon.dungeonLiteSpot(coord);

                built = true;
            }

            return built;
        }

        // Replicate a creature -RAK-
        public static bool spellCloneMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var finished = false;

            while (!finished)
            {
                helpers.movePosition(direction, ref coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                }
                else if (tile.creature_id > 1)
                {
                    State.Instance.monsters[tile.creature_id].sleep_count = 0;

                    // monptr of 0 is safe here, since can't reach here from creatures
                    return monsterMultiply(coord, (int)State.Instance.monsters[tile.creature_id].creature_id, 0);
                }
            }

            return false;
        }

        

        

        // Lose a strength point. -RAK-
        public static void spellLoseSTR()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_str)
            {
                playerStatRandomDecrease((int)PlayerAttr.STR);
                printMessage("You feel very sick.");
            }
            else
            {
                printMessage("You feel sick for a moment,  it passes.");
            }
        }

        // Lose an intelligence point. -RAK-
        public static void spellLoseINT()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_int)
            {
                playerStatRandomDecrease((int)PlayerAttr.INT);
                printMessage("You become very dizzy.");
            }
            else
            {
                printMessage("You become dizzy for a moment,  it passes.");
            }
        }

        // Lose a wisdom point. -RAK-
        public static void spellLoseWIS()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_wis)
            {
                playerStatRandomDecrease((int)PlayerAttr.WIS);
                printMessage("You feel very naive.");
            }
            else
            {
                printMessage("You feel naive for a moment,  it passes.");
            }
        }

        // Lose a dexterity point. -RAK-
        public static void spellLoseDEX()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_dex)
            {
                playerStatRandomDecrease((int)PlayerAttr.DEX);
                printMessage("You feel very sore.");
            }
            else
            {
                printMessage("You feel sore for a moment,  it passes.");
            }
        }

        // Lose a constitution point. -RAK-
        public static void spellLoseCON()
        {
            var py = State.Instance.py;
            if (!py.flags.sustain_con)
            {
                playerStatRandomDecrease((int)PlayerAttr.CON);
                printMessage("You feel very sick.");
            }
            else
            {
                printMessage("You feel sick for a moment,  it passes.");
            }
        }

        // Lose a charisma point. -RAK-
        public static void spellLoseCHR()
        {
            var py = State.Instance.py;
            if (!py.flags.sustain_chr)
            {
                playerStatRandomDecrease((int)PlayerAttr.CHR);
                printMessage("Your skin starts to itch.");
            }
            else
            {
                printMessage("Your skin starts to itch, but feels better now.");
            }
        }

        // Lose experience -RAK-
        public static void spellLoseEXP(int adjustment)
        {
            var py = State.Instance.py;
            if (adjustment > py.misc.exp)
            {
                py.misc.exp = 0;
            }
            else
            {
                py.misc.exp -= adjustment;
            }
            displayCharacterExperience();

            var exp = 0;
            while ((int)(py.base_exp_levels[exp] * py.misc.experience_factor / 100) <= py.misc.exp)
            {
                exp++;
            }

            // increment exp once more, because level 1 exp is stored in player_base_exp_levels[0]
            exp++;

            if (py.misc.level != exp)
            {
                py.misc.level = (uint)exp;

                playerCalculateHitPoints();

                var character_class = Library.Instance.Player.classes[(int)py.misc.class_id];

                if (character_class.class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
                {
                    playerCalculateAllowedSpellsCount((int)PlayerAttr.INT);
                    playerGainMana((int)PlayerAttr.INT);
                }
                else if (character_class.class_to_use_mage_spells == Config.spells.SPELL_TYPE_PRIEST)
                {
                    playerCalculateAllowedSpellsCount((int)PlayerAttr.WIS);
                    playerGainMana((int)PlayerAttr.WIS);
                }
                printCharacterLevel();
                printCharacterTitle();
            }
        }

        // Enchants a plus onto an item. -RAK-
        // `limit` param is the maximum bonus allowed; usually 10,
        // but weapon's maximum damage when enchanting melee weapons to damage.
        public static bool spellEnchantItem(ref int plusses, int max_bonus_limit)
        {
            // avoid rnd.randomNumber(0) call
            if (max_bonus_limit <= 0)
            {
                return false;
            }

            var chance = 0;

            if (plusses > 0)
            {
                chance = plusses;

                // very rarely allow enchantment over limit
                if (rnd.randomNumber(100) == 1)
                {
                    chance = rnd.randomNumber(chance) - 1;
                }
            }

            if (rnd.randomNumber(max_bonus_limit) > chance)
            {
                plusses += 1;
                return true;
            }

            return false;
        }

        // Removes curses from items in inventory -RAK-
        public static bool spellRemoveCurseFromAllItems()
        {
            var py = State.Instance.py;

            var removed = false;

            for (var id = (int)PlayerEquipment.Wield; id <= (int)PlayerEquipment.Outer; id++)
            {
                if ((py.inventory[id].flags & Config.treasure_flags.TR_CURSED) != 0u)
                {
                    py.inventory[id].flags &= ~Config.treasure_flags.TR_CURSED;
                    playerRecalculateBonuses();
                    removed = true;
                }
            }

            return removed;
        }

        // Restores any drained experience -RAK-
        public static bool spellRestorePlayerLevels()
        {
            var py = State.Instance.py;

            if (py.misc.max_exp > py.misc.exp)
            {
                printMessage("You feel your life energies returning.");

                // this while loop is not redundant, ptr_exp may reduce the exp level
                while (py.misc.exp < py.misc.max_exp)
                {
                    py.misc.exp = py.misc.max_exp;
                    displayCharacterExperience();
                }

                return true;
            }

            return false;
        }
    }
}
