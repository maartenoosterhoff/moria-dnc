namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class EnchantItemCommand : ICommand
    {
        public EnchantItemCommand(int plusses, int max_bonus_limit)
        {
            this.Plusses = plusses;
            this.MaxBonusLimit = max_bonus_limit;
        }

        public int Plusses { get; set; }

        public int MaxBonusLimit { get; }
    }
}