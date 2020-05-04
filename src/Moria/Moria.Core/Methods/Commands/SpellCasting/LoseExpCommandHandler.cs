using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseExpCommandHandler : ICommandHandler<LoseExpCommand>
    {
        public void Handle(LoseExpCommand command)
        {
            this.spellLoseEXP(command.Adjustment);
        }

        // Lose experience -RAK-
        private void spellLoseEXP(int adjustment)
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
            Ui_m.displayCharacterExperience();

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

                Player_stats_m.playerCalculateHitPoints();

                var character_class = Library.Instance.Player.classes[(int)py.misc.class_id];

                if (character_class.class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
                {
                    Player_m.playerCalculateAllowedSpellsCount((int)PlayerAttr.INT);
                    Player_m.playerGainMana((int)PlayerAttr.INT);
                }
                else if (character_class.class_to_use_mage_spells == Config.spells.SPELL_TYPE_PRIEST)
                {
                    Player_m.playerCalculateAllowedSpellsCount((int)PlayerAttr.WIS);
                    Player_m.playerGainMana((int)PlayerAttr.WIS);
                }
                Ui_m.printCharacterLevel();
                Ui_m.printCharacterTitle();
            }
        }
    }
}