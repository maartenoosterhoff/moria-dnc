using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseWisCommandHandler : ICommandHandler<LoseWisCommand>
    {
        private readonly ITerminal terminal;
        private readonly IEventPublisher eventPublisher;

        public LoseWisCommandHandler(
            ITerminal terminal,
            IEventPublisher eventPublisher
        )
        {
            this.terminal = terminal;
            this.eventPublisher = eventPublisher;
        }

        public void Handle(LoseWisCommand command)
        {
            this.spellLoseWIS();
        }

        // Lose a wisdom point. -RAK-
        private void spellLoseWIS()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_wis)
            {
                this.eventPublisher.Publish(new StatRandomDecreaseCommand((int)PlayerAttr.WIS));
                //Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.WIS);
                this.terminal.printMessage("You feel very naive.");
            }
            else
            {
                this.terminal.printMessage("You feel naive for a moment,  it passes.");
            }
        }
    }
}