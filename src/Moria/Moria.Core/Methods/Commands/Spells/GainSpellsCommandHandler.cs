using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.Spells
{
    public class GainSpellsCommandHandler : ICommandHandler<GainSpellsCommand>
    {
        private readonly IHelpers helpers;
        private readonly IRnd rnd;

        public GainSpellsCommandHandler(
            IHelpers helpers,
            IRnd rnd
        )
        {
            this.helpers = helpers;
            this.rnd = rnd;
        }

        public void Handle(GainSpellsCommand command)
        {
            this.playerGainSpells();
        }

        // gain spells when player wants to -JW-
        private void playerGainSpells()
        {
            var py = State.Instance.py;
            // Priests don't need light because they get spells from their god, so only
            // fail when can't see if player has Config.spells.SPELL_TYPE_MAGE spells. This check is done below.
            if (py.flags.confused > 0)
            {
                Ui_io_m.printMessage("You are too confused.");
                return;
            }

            var new_spells = (int)py.flags.new_spells_to_learn;
            var diff_spells = 0;

            int stat, offset;

            if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
            {
                // People with Config.spells.SPELL_TYPE_MAGE spells can't learn spell_bank if they can't read their books.
                if (!this.playerCanRead())
                {
                    return;
                }
                stat = (int)PlayerAttr.INT;
                offset = (int)Config.spells.NAME_OFFSET_SPELLS;
            }
            else
            {
                stat = (int)PlayerAttr.WIS;
                offset = (int)Config.spells.NAME_OFFSET_PRAYERS;
            }

            var last_known = this.lastKnownSpell();

            if (new_spells == 0)
            {
                var tmp_str = $"You can't learn any new {(stat == (int)PlayerAttr.INT ? "spell" : "prayer")}s!";
                //vtype_t tmp_str = { '\0' };
                //(void)sprintf(tmp_str, "You can't learn any new %ss!", (stat == PlayerAttr.INT ? "spell" : "prayer"));
                Ui_io_m.printMessage(tmp_str);

                State.Instance.game.player_free_turn = true;
                return;
            }

            uint spell_flag;

            // determine which spells player can learn
            // mages need the book to learn a spell, priests do not need the book
            if (stat == (int)PlayerAttr.INT)
            {
                spell_flag = this.playerDetermineLearnableSpells();
            }
            else
            {
                spell_flag = 0x7FFFFFFF;
            }

            // clear bits for spells already learned
            spell_flag &= ~py.flags.spells_learnt;

            var spell_id = 0;
            var spell_bank = new int[31];
            uint mask = 0x1;

            // TODO(cook) move access to `magic_spells[]` directly to the for loop it's used in, below?
            var spells = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1];

            for (var i = 0; spell_flag != 0u; mask <<= 1, i++)
            {
                if ((spell_flag & mask) != 0u)
                {
                    spell_flag &= ~mask;
                    if (spells[i].level_required <= py.misc.level)
                    {
                        spell_bank[spell_id] = i;
                        spell_id++;
                    }
                }
            }

            if (new_spells > spell_id)
            {
                Ui_io_m.printMessage("You seem to be missing a book.");

                diff_spells = new_spells - spell_id;
                new_spells = spell_id;
            }

            if (new_spells == 0)
            {
                // do nothing
            }
            else if (stat == (int)PlayerAttr.INT)
            {
                // get to choose which mage spells will be learned
                Ui_io_m.terminalSaveScreen();
                Ui_m.displaySpellsList(spell_bank, spell_id, false, -1);

                while (new_spells != 0 && Ui_io_m.getCommand("Learn which spell?", out var query))
                {
                    var c = query - 'a';

                    // test j < 23 in case i is greater than 22, only 22 spells
                    // are actually shown on the screen, so limit choice to those
                    if (c >= 0 && c < spell_id && c < 22)
                    {
                        new_spells--;

                        py.flags.spells_learnt |= 1u << spell_bank[c];
                        py.flags.spells_learned_order[last_known] = (uint)spell_bank[c];
                        last_known++;

                        for (; c <= spell_id - 1; c++)
                        {
                            spell_bank[c] = spell_bank[c + 1];
                        }

                        spell_id--;

                        Ui_io_m.eraseLine(new Coord_t(c + 1, 31));
                        Ui_m.displaySpellsList(spell_bank, spell_id, false, -1);
                    }
                    else
                    {
                        Ui_io_m.terminalBellSound();
                    }
                }

                Ui_io_m.terminalRestoreScreen();
            }
            else
            {
                // pick a prayer at random
                while (new_spells != 0)
                {
                    var id = this.rnd.randomNumber(spell_id) - 1;
                    py.flags.spells_learnt |= 1u << spell_bank[id];
                    py.flags.spells_learned_order[last_known] = (uint)spell_bank[id];
                    last_known++;

                    var tmp_str = $"You have learned the prayer of {Library.Instance.Player.spell_names[spell_bank[id] + offset]}.";
                    //vtype_t tmp_str = { '\0' };
                    //(void)sprintf(tmp_str, "You have learned the prayer of %s.", Library.Instance.Player.spell_names[spell_bank[id] + offset]);
                    Ui_io_m.printMessage(tmp_str);

                    for (; id <= spell_id - 1; id++)
                    {
                        spell_bank[id] = spell_bank[id + 1];
                    }

                    spell_id--;
                    new_spells--;
                }
            }

            py.flags.new_spells_to_learn = (uint)(new_spells + diff_spells);

            if (py.flags.new_spells_to_learn == 0)
            {
                py.flags.status |= Config.player_status.PY_STUDY;
            }

            // set the mana for first level characters when they learn their first spell.
            if (py.misc.mana == 0)
            {
                Player_m.playerGainMana(stat);
            }
        }

        private bool playerCanRead()
        {
            var py = State.Instance.py;
            if (py.flags.blind > 0)
            {
                Ui_io_m.printMessage("You can't see to read your spell book!");
                return false;
            }

            if (this.helpers.playerNoLight())
            {
                Ui_io_m.printMessage("You have no light to read by.");
                return false;
            }

            return true;
        }

        private int lastKnownSpell()
        {
            var py = State.Instance.py;
            for (var last_known = 0; last_known < 32; last_known++)
            {
                if (py.flags.spells_learned_order[last_known] == 99)
                {
                    return last_known;
                }
            }

            // We should never actually reach this, but just in case... -MRC-
            return 0;
        }

        private uint /*was: int */playerDetermineLearnableSpells()
        {
            var py = State.Instance.py;

            uint spell_flag = 0;

            for (var i = 0; i < py.pack.unique_items; i++)
            {
                if (py.inventory[i].category_id == Treasure_c.TV_MAGIC_BOOK)
                {
                    spell_flag |= py.inventory[i].flags;
                }
            }

            return (uint)/*was (int)*/spell_flag;
        }
    }
}