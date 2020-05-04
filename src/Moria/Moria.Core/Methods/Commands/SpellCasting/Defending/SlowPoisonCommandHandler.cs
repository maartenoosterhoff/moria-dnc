using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Defending
{
    public class SlowPoisonCommandHandler :
        ICommandHandler<SlowPoisonCommand>,
        ICommandHandler<SlowPoisonCommand, bool>
    {
        private readonly ITerminal terminal;

        public SlowPoisonCommandHandler(ITerminal terminal)
        {
            this.terminal = terminal;
        }

        void ICommandHandler<SlowPoisonCommand>.Handle(SlowPoisonCommand command)
        {
            this.spellSlowPoison();
        }

        bool ICommandHandler<SlowPoisonCommand, bool>.Handle(SlowPoisonCommand command)
        {
            return this.spellSlowPoison();
        }

        // Slow Poison -RAK-
        private bool spellSlowPoison()
        {
            var py = State.Instance.py;
            if (py.flags.poisoned > 0)
            {
                py.flags.poisoned = (int)(py.flags.poisoned / 2);
                if (py.flags.poisoned < 1)
                {
                    py.flags.poisoned = 1;
                }
                this.terminal.printMessage("The effect of the poison has been reduced.");
                return true;
            }

            return false;
        }
    }
}