namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class RechargeItemCommand : ICommand
    {
        public RechargeItemCommand(int number_of_charges)
        {
            this.NumberOfCharges = number_of_charges;
        }

        public int NumberOfCharges { get; }
    }
}