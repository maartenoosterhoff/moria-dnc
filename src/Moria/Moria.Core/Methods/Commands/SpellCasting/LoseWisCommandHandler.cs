using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseWisCommandHandler : ICommandHandler<LoseWisCommand>
    {
        public void Handle(LoseWisCommand command)
        {
            this.spellLoseWIS();
        }

        // Lose a wisdom point. -RAK-
        private void spellLoseWIS()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_wis)
            {
                Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.WIS);
                Ui_io_m.printMessage("You feel very naive.");
            }
            else
            {
                Ui_io_m.printMessage("You feel naive for a moment,  it passes.");
            }
        }
    }
}