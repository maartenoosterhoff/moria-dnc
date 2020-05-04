namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class EnchantItemCommandHandler :
        ICommandHandler<EnchantItemCommand>,
        ICommandHandler<EnchantItemCommand, bool>
    {
        private readonly IRnd rnd;

        public EnchantItemCommandHandler(IRnd rnd)
        {
            this.rnd = rnd;
        }

        void ICommandHandler<EnchantItemCommand>.Handle(EnchantItemCommand command)
        {
            var plusses = command.Plusses;
            this.spellEnchantItem(
                ref plusses,
                command.MaxBonusLimit
            );
            command.Plusses = plusses;
        }

        bool ICommandHandler<EnchantItemCommand, bool>.Handle(EnchantItemCommand command)
        {
            var plusses = command.Plusses;
            var result = this.spellEnchantItem(
                ref plusses,
                command.MaxBonusLimit
            );
            command.Plusses = plusses;

            return result;
        }

        // Enchants a plus onto an item. -RAK-
        // `limit` param is the maximum bonus allowed; usually 10,
        // but weapon's maximum damage when enchanting melee weapons to damage.
        private bool spellEnchantItem(ref int plusses, int max_bonus_limit)
        {
            // avoid rnd.randomNumber(0) call
            if (max_bonus_limit <= 0)
            {
                return false;
            }

            var chance = 0;

            if (plusses > 0)
            {
                chance = plusses;

                // very rarely allow enchantment over limit
                if (this.rnd.randomNumber(100) == 1)
                {
                    chance = this.rnd.randomNumber(chance) - 1;
                }
            }

            if (this.rnd.randomNumber(max_bonus_limit) > chance)
            {
                plusses += 1;
                return true;
            }

            return false;
        }
    }
}