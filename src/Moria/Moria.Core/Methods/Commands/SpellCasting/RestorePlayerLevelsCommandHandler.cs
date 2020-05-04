using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class RestorePlayerLevelsCommandHandler :
        ICommandHandler<RestorePlayerLevelsCommand>,
        ICommandHandler<RestorePlayerLevelsCommand, bool>
    {
        private readonly ITerminal terminal;

        public RestorePlayerLevelsCommandHandler(ITerminal terminal)
        {
            this.terminal = terminal;
        }

        void ICommandHandler<RestorePlayerLevelsCommand>.Handle(RestorePlayerLevelsCommand command)
        {
            this.spellRestorePlayerLevels();
        }

        bool ICommandHandler<RestorePlayerLevelsCommand, bool>.Handle(RestorePlayerLevelsCommand command)
        {
            return this.spellRestorePlayerLevels();
        }

        // Restores any drained experience -RAK-
        private bool spellRestorePlayerLevels()
        {
            var py = State.Instance.py;

            if (py.misc.max_exp > py.misc.exp)
            {
                this.terminal.printMessage("You feel your life energies returning.");

                // this while loop is not redundant, ptr_exp may reduce the exp level
                while (py.misc.exp < py.misc.max_exp)
                {
                    py.misc.exp = py.misc.max_exp;
                    Ui_m.displayCharacterExperience();
                }

                return true;
            }

            return false;
        }
    }
}