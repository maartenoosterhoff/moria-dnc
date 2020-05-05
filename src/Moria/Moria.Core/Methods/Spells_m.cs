using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using Moria.Core.Data;

namespace Moria.Core.Methods
{
    public interface ISpells
    {
        int castSpellGetId(string prompt, int item_id, ref int spell_id, ref int spell_chance);

        void spellGetAreaAffectFlags(int spell_type, out uint weapon_type, out int harm_type,
            out Func<Inventory_t, bool> destroy);

        void displaySpellsList(int[] spell_ids, int number_of_choices, bool comment, int non_consecutive);
    }

    public class Spells_m : ISpells
    {
        private readonly IHelpers helpers;
        private readonly IInventory inventory;
        private readonly IMageSpells mageSpells;
        private readonly ITerminal terminal;

        public Spells_m(
            IHelpers helpers,
            IInventory inventory,
            IMageSpells mageSpells,
            ITerminal terminal
        )
        {
            this.helpers = helpers;
            this.inventory = inventory;
            this.mageSpells = mageSpells;
            this.terminal = terminal;
        }

        // Print list of spells -RAK-
        // if non_consecutive is  -1: spells numbered consecutively from 'a' to 'a'+num
        //                       >=0: spells numbered by offset from non_consecutive
        public void displaySpellsList(int[] spell_ids, int number_of_choices, bool comment, int non_consecutive)
        {
            var py = State.Instance.py;
            int col;
            if (comment)
            {
                col = 22;
            }
            else
            {
                col = 31;
            }

            int consecutive_offset;
            if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
            {
                consecutive_offset = (int)Config.spells.NAME_OFFSET_SPELLS;
            }
            else
            {
                consecutive_offset = (int)Config.spells.NAME_OFFSET_PRAYERS;
            }

            this.terminal.eraseLine(new Coord_t(1, col));
            this.terminal.putString("Name", new Coord_t(1, col + 5));
            this.terminal.putString("Lv Mana Fail", new Coord_t(1, col + 35));

            // only show the first 22 choices
            if (number_of_choices > 22)
            {
                number_of_choices = 22;
            }

            for (var i = 0; i < number_of_choices; i++)
            {
                var spell_id = spell_ids[i];
                var spell = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1][spell_id];

                var p = string.Empty;
                if (!comment)
                {
                    p = "";
                }
                else if ((py.flags.spells_forgotten & (1L << spell_id)) != 0)
                {
                    p = " forgotten";
                }
                else if ((py.flags.spells_learnt & (1L << spell_id)) == 0)
                {
                    p = " unknown";
                }
                else if ((py.flags.spells_worked & (1L << spell_id)) == 0)
                {
                    p = " untried";
                }
                else
                {
                    p = "";
                }

                // determine whether or not to leave holes in character choices, non_consecutive -1
                // when learning spells, consecutive_offset>=0 when asking which spell to cast.
                char spell_char;
                if (non_consecutive == -1)
                {
                    spell_char = (char)('a' + i);
                }
                else
                {
                    spell_char = (char)('a' + spell_id - non_consecutive);
                }

                var out_val = $"  {spell_char}) {Library.Instance.Player.spell_names[spell_id + consecutive_offset].PadRight(30)}{spell.level_required,2:d} {spell.mana_required,4:d} {this.mageSpells.spellChanceOfSuccess(spell_id),3:d}%{p}";
                //vtype_t out_val = { '\0' };
                //(void)sprintf(out_val,
                //    "  %c) %-30s%2d %4d %3d%%%s",
                //    spell_char,
                //    Library.Instance.Player.spell_names[spell_id + consecutive_offset],
                //    spell.level_required,
                //    spell.mana_required,
                //    spellChanceOfSuccess(spell_id),
                //    p);
                this.terminal.putStringClearToEOL(out_val, new Coord_t(2 + i, col));
            }
        }

        // Returns spell pointer -RAK-
        private bool spellGetId(int[] spell_ids, int number_of_choices, ref int spell_id, ref int spell_chance, string prompt, int first_spell)
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

            while (!spell_found && this.terminal.getCommand(str, out var choice))
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

                        var tmp_str = $"Cast {spell_names[spell_id + offset]} ({spell.mana_required} mana, {this.mageSpells.spellChanceOfSuccess(spell_id)}% fail)?";
                        //vtype_t tmp_str = { '\0' };
                        //(void)sprintf(tmp_str, "Cast %s (%d mana, %d%% fail)?", spell_names[spell_id + offset], spell.mana_required, spellChanceOfSuccess(spell_id));
                        if (this.terminal.getInputConfirmation(tmp_str))
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
                        this.terminal.terminalSaveScreen();
                        redraw = true;
                        this.displaySpellsList(spell_ids, number_of_choices, false, first_spell);
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
                    this.terminal.terminalBellSound();
                }

                if (spell_id == -2)
                {
                    var spellOrPrayer = offset == Config.spells.NAME_OFFSET_SPELLS ? "spell" : "prayer";
                    var tmp_str = $"You don't know that {spellOrPrayer}.";
                    //vtype_t tmp_str = { '\0' };
                    //(void)sprintf(tmp_str, "You don't know that %s.", (offset == Config.spells.NAME_OFFSET_SPELLS ? "spell" : "prayer"));
                    this.terminal.printMessage(tmp_str);
                }
            }

            if (redraw)
            {
                this.terminal.terminalRestoreScreen();
            }

            this.terminal.messageLineClear();

            if (spell_found)
            {
                spell_chance = this.mageSpells.spellChanceOfSuccess(spell_id);
            }

            return spell_found;
        }

        // Return spell number and failure chance -RAK-
        // returns -1 if no spells in book
        // returns  1 if choose a spell in book to cast
        // returns  0 if don't choose a spell, i.e. exit with an escape
        // TODO: split into two functions; getting spell ID and casting an actual spell
        public int castSpellGetId(string prompt, int item_id, ref int spell_id, ref int spell_chance)
        {
            var py = State.Instance.py;
            // NOTE: `flags` gets set again, since getAndClearFirstBit modified it
            var flags = py.inventory[item_id].flags;
            var first_spell = this.helpers.getAndClearFirstBit(ref flags);
            flags = py.inventory[item_id].flags & py.flags.spells_learnt;

            // TODO(cook) move access to `magic_spells[]` directly to the for loop it's used in, below?
            var spells = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1];

            var spell_count = 0;
            var spell_list = new int[31];

            while (flags != 0u)
            {
                var pos = this.helpers.getAndClearFirstBit(ref flags);

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
            if (this.spellGetId(spell_list, spell_count, ref spell_id, ref spell_chance, prompt, first_spell))
            {
                result = 1;
            }

            if (result != 0 && Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1][spell_id].mana_required > py.misc.current_mana)
            {
                if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
                {
                    result = (int)(this.terminal.getInputConfirmation("You summon your limited strength to cast this one! Confirm?") ? 1 : 0);
                }
                else
                {
                    result = (int)(this.terminal.getInputConfirmation("The gods may think you presumptuous for this! Confirm?") ? 1 : 0);
                }
            }

            return result;
        }

        // Return flags for given type area affect -RAK-
        public void spellGetAreaAffectFlags(int spell_type, out uint weapon_type, out int harm_type, out Func<Inventory_t, bool> destroy)
        {
            switch ((MagicSpellFlags)spell_type)
            {
                case MagicSpellFlags.MagicMissile:
                    weapon_type = 0;
                    harm_type = 0;
                    destroy = this.inventory.setNull;
                    break;
                case MagicSpellFlags.Lightning:
                    weapon_type = Config.monsters_spells.CS_BR_LIGHT;
                    harm_type = (int)Config.monsters_defense.CD_LIGHT;
                    destroy = this.inventory.setLightningDestroyableItems;
                    break;
                case MagicSpellFlags.PoisonGas:
                    weapon_type = Config.monsters_spells.CS_BR_GAS;
                    harm_type = (int)Config.monsters_defense.CD_POISON;
                    destroy = this.inventory.setNull;
                    break;
                case MagicSpellFlags.Acid:
                    weapon_type = Config.monsters_spells.CS_BR_ACID;
                    harm_type = (int)Config.monsters_defense.CD_ACID;
                    destroy = this.inventory.setAcidDestroyableItems;
                    break;
                case MagicSpellFlags.Frost:
                    weapon_type = Config.monsters_spells.CS_BR_FROST;
                    harm_type = (int)Config.monsters_defense.CD_FROST;
                    destroy = this.inventory.setFrostDestroyableItems;
                    break;
                case MagicSpellFlags.Fire:
                    weapon_type = Config.monsters_spells.CS_BR_FIRE;
                    harm_type = (int)Config.monsters_defense.CD_FIRE;
                    destroy = this.inventory.setFireDestroyableItems;
                    break;
                case MagicSpellFlags.HolyOrb:
                    weapon_type = 0;
                    harm_type = (int)Config.monsters_defense.CD_EVIL;
                    destroy = this.inventory.setNull;
                    break;
                default:
                    weapon_type = 0;
                    harm_type = 0;
                    destroy = this.inventory.setNull;
                    this.terminal.printMessage("ERROR in spellGetAreaAffectFlags()\n");
                    break;
            }
        }
    }
}