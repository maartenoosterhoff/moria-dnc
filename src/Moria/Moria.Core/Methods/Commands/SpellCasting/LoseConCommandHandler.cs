using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseConCommandHandler : ICommandHandler<LoseConCommand>
    {
        public void Handle(LoseConCommand command)
        {
            this.spellLoseCON();
        }

        // Lose a constitution point. -RAK-
        private void spellLoseCON()
        {
            var py = State.Instance.py;
            if (!py.flags.sustain_con)
            {
                Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.CON);
                Ui_io_m.printMessage("You feel very sick.");
            }
            else
            {
                Ui_io_m.printMessage("You feel sick for a moment,  it passes.");
            }
        }
    }
}