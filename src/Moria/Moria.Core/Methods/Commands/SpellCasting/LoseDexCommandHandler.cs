using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseDexCommandHandler : ICommandHandler<LoseDexCommand>
    {
        private readonly ITerminal terminal;
        private readonly IEventPublisher eventPublisher;

        public LoseDexCommandHandler(
            ITerminal terminal,
            IEventPublisher eventPublisher
        )
        {
            this.terminal = terminal;
            this.eventPublisher = eventPublisher;
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
                this.eventPublisher.Publish(new StatRandomDecreaseCommand((int)PlayerAttr.DEX));
                //Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.DEX);
                this.terminal.printMessage("You feel very sore.");
            }
            else
            {
                this.terminal.printMessage("You feel sore for a moment,  it passes.");
            }
        }
    }
}