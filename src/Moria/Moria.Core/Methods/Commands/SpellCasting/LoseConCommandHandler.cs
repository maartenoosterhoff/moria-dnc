using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseConCommandHandler : ICommandHandler<LoseConCommand>
    {
        private readonly ITerminal terminal;
        private readonly IEventPublisher eventPublisher;

        public LoseConCommandHandler(
            ITerminal terminal,
            IEventPublisher eventPublisher
        )
        {
            this.terminal = terminal;
            this.eventPublisher = eventPublisher;
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
                this.eventPublisher.Publish(new StatRandomDecreaseCommand((int)PlayerAttr.CON));
                //Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.CON);
                this.terminal.printMessage("You feel very sick.");
            }
            else
            {
                this.terminal.printMessage("You feel sick for a moment,  it passes.");
            }
        }
    }
}