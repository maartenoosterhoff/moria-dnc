using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseDexCommandHandler : ICommandHandler<LoseDexCommand>
    {
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
                Ui_io_m.printMessage("You feel very sore.");
            }
            else
            {
                Ui_io_m.printMessage("You feel sore for a moment,  it passes.");
            }
        }
    }
}