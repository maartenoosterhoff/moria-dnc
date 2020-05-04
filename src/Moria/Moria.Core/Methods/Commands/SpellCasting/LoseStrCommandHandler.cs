using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseStrCommandHandler : ICommandHandler<LoseStrCommand>
    {
        private readonly ITerminal terminal;
        private readonly IEventPublisher eventPublisher;

        public LoseStrCommandHandler(
            ITerminal terminal,
            IEventPublisher eventPublisher
        )
        {
            this.terminal = terminal;
            this.eventPublisher = eventPublisher;
        }

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
                this.eventPublisher.Publish(new StatRandomDecreaseCommand((int)PlayerAttr.STR));
                //Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.STR);
                this.terminal.printMessage("You feel very sick.");
            }
            else
            {
                this.terminal.printMessage("You feel sick for a moment,  it passes.");
            }
        }
    }
}