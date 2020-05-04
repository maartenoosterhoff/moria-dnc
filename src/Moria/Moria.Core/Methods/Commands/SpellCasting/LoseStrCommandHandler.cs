using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseStrCommandHandler : ICommandHandler<LoseStrCommand>
    {
        public void Handle(LoseStrCommand command)
        {
            this.spellLoseSTR();
        }

        // Lose a strength point. -RAK-

        private void spellLoseSTR()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_str)
            {
                Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.STR);
                Ui_io_m.printMessage("You feel very sick.");
            }
            else
            {
                Ui_io_m.printMessage("You feel sick for a moment,  it passes.");
            }
        }
    }
}