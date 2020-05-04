using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Defending
{
    public class ChangePlayerHitPointsCommandHandler :
        ICommandHandler<ChangePlayerHitPointsCommand>,
        ICommandHandler<ChangePlayerHitPointsCommand, bool>
    {
        private readonly ITerminal terminal;

        public ChangePlayerHitPointsCommandHandler(
            ITerminal terminal
        )
        {
            this.terminal = terminal;
        }

        void ICommandHandler<ChangePlayerHitPointsCommand>.Handle(ChangePlayerHitPointsCommand command)
        {
            this.spellChangePlayerHitPoints(command.Adjustment);
        }

        bool ICommandHandler<ChangePlayerHitPointsCommand, bool>.Handle(ChangePlayerHitPointsCommand command)
        {
            return this.spellChangePlayerHitPoints(command.Adjustment);
        }

        // Change players hit points in some manner -RAK-
        private bool spellChangePlayerHitPoints(int adjustment)
        {
            var py = State.Instance.py;
            if (py.misc.current_hp >= py.misc.max_hp)
            {
                return false;
            }

            py.misc.current_hp += adjustment;
            if (py.misc.current_hp > py.misc.max_hp)
            {
                py.misc.current_hp = py.misc.max_hp;
                py.misc.current_hp_fraction = 0;
            }
            Ui_m.printCharacterCurrentHitPoints();

            adjustment /= 5;

            if (adjustment < 3)
            {
                if (adjustment == 0)
                {
                    this.terminal.printMessage("You feel a little better.");
                }
                else
                {
                    this.terminal.printMessage("You feel better.");
                }
            }
            else
            {
                if (adjustment < 7)
                {
                    this.terminal.printMessage("You feel much better.");
                }
                else
                {
                    this.terminal.printMessage("You feel very good.");
                }
            }

            return true;
        }
    }
}