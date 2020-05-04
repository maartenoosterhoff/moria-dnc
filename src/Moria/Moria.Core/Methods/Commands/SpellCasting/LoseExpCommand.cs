namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class LoseExpCommand : ICommand
    {
        public LoseExpCommand(int adjustment)
        {
            this.Adjustment = adjustment;
        }

        public int Adjustment { get; }
    }
}