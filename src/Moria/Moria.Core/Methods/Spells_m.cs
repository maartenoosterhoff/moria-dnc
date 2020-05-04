using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using Moria.Core.Data;
using static Moria.Core.Methods.Mage_spells_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Spells_m
    {
        public static void SetDependencies(
            IHelpers helpers,
            IInventory inventory
        )
        {
            Spells_m.helpers = helpers;
            Spells_m.inventory = inventory;
        }

        private static IHelpers helpers;
        private static IInventory inventory;

        // Returns spell pointer -RAK-
        private static bool spellGetId(int[] spell_ids, int number_of_choices, ref int spell_id, ref int spell_chance, string prompt, int first_spell)
        {
            var py = State.Instance.py;
            var magic_spells = Library.Instance.Player.magic_spells;
            var spell_names = Library.Instance.Player.spell_names;
            spell_id = -1;

            //vtype_t str = { '\0' };
            var str = string.Format(
                "(Spells {0}-{1}, *=List, <ESCAPE>=exit) {2}",
                (char)(spell_ids[0] + 'a' - first_spell),
                (char)(spell_ids[number_of_choices - 1] + 'a' - first_spell),
                prompt
            );
            //(void)sprintf(str, "(Spells %c-%c, *=List, <ESCAPE>=exit) %s", spell_ids[0] + 'a' - first_spell, spell_ids[number_of_choices - 1] + 'a' - first_spell, prompt);

            var spell_found = false;
            var redraw = false;

            var offset = Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells == (int)Config.spells.SPELL_TYPE_MAGE ? (int)Config.spells.NAME_OFFSET_SPELLS : (int)Config.spells.NAME_OFFSET_PRAYERS;

            while (!spell_found && getCommand(str, out var choice))
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
    }
}