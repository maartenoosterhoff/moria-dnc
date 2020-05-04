namespace Moria.Core.Methods.Commands.SpellCasting.Defending
{
    public class ChangePlayerHitPointsCommand : ICommand
    {
        public ChangePlayerHitPointsCommand(int adjustment)
        {
            this.Adjustment = adjustment;
        }

        public int Adjustment { get; }
    }
}
