using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseConCommandHandler : ICommandHandler<LoseConCommand>
    {
        private readonly ITerminal terminal;

        public LoseConCommandHandler(
            ITerminal terminal
        )
        {
            this.terminal = terminal;
        }

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
                this.terminal.printMessage("You feel very sick.");
            }
            else
            {
                this.terminal.printMessage("You feel sick for a moment,  it passes.");
            }
        }
    }
}