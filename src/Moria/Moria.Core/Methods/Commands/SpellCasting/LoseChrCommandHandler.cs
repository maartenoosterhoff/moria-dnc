using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseChrCommandHandler : ICommandHandler<LoseChrCommand>
    {
        private readonly ITerminal terminal;

        public LoseChrCommandHandler(
            ITerminal terminal
        )
        {
            this.terminal = terminal;
        }

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
                this.terminal.printMessage("Your skin starts to itch.");
            }
            else
            {
                this.terminal.printMessage("Your skin starts to itch, but feels better now.");
            }
        }
    }
}