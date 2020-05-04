using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseDexCommandHandler : ICommandHandler<LoseDexCommand>
    {
        private readonly ITerminal terminal;

        public LoseDexCommandHandler(
            ITerminal terminal
        )
        {
            this.terminal = terminal;
        }

        public void Handle(LoseDexCommand command)
        {
            this.spellLoseDEX();
        }

        // Lose a dexterity point. -RAK-

        private void spellLoseDEX()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_dex)
            {
                Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.DEX);
                this.terminal.printMessage("You feel very sore.");
            }
            else
            {
                this.terminal.printMessage("You feel sore for a moment,  it passes.");
            }
        }
    }
}