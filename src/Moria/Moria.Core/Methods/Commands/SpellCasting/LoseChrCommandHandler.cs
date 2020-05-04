using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseChrCommandHandler : ICommandHandler<LoseChrCommand>
    {
        public void Handle(LoseChrCommand command)
        {
            this.spellLoseCHR();
        }

        // Lose a charisma point. -RAK-

        private void spellLoseCHR()
        {
            var py = State.Instance.py;
            if (!py.flags.sustain_chr)
            {
                Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.CHR);
                Ui_io_m.printMessage("Your skin starts to itch.");
            }
            else
            {
                Ui_io_m.printMessage("Your skin starts to itch, but feels better now.");
            }
        }
    }
}