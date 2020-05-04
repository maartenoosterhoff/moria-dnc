using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseIntCommandHandler : ICommandHandler<LoseIntCommand>
    {
        private readonly ITerminal terminal;
        private readonly IEventPublisher eventPublisher;

        public LoseIntCommandHandler(
            ITerminal terminal,
            IEventPublisher eventPublisher
        )
        {
            this.terminal = terminal;
            this.eventPublisher = eventPublisher;
        }

        public void Handle(LoseIntCommand command)
        {
            this.spellLoseINT();
        }

        // Lose an intelligence point. -RAK-

        private void spellLoseINT()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_int)
            {
                this.eventPublisher.Publish(new StatRandomDecreaseCommand((int)PlayerAttr.INT));
                //Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.INT);
                this.terminal.printMessage("You become very dizzy.");
            }
            else
            {
                this.terminal.printMessage("You become dizzy for a moment,  it passes.");
            }
        }
    }
}