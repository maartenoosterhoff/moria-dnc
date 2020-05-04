using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseChrCommandHandler : ICommandHandler<LoseChrCommand>
    {
        private readonly ITerminal terminal;
        private readonly IEventPublisher eventPublisher;

        public LoseChrCommandHandler(
            ITerminal terminal,
            IEventPublisher eventPublisher
        )
        {
            this.terminal = terminal;
            this.eventPublisher = eventPublisher;
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
                this.eventPublisher.Publish(new StatRandomDecreaseCommand((int)PlayerAttr.CHR));
                //Player_stats_m.playerStatRandomDecrease((int)PlayerAttr.CHR);
                this.terminal.printMessage("Your skin starts to itch.");
            }
            else
            {
                this.terminal.printMessage("Your skin starts to itch, but feels better now.");
            }
        }
    }
}