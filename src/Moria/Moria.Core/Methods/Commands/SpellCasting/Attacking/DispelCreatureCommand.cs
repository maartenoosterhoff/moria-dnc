namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class DispelCreatureCommand : ICommand
    {
        public DispelCreatureCommand(int creature_defense, int damage)
        {
            this.CreatureDefense = creature_defense;
            this.Damage = damage;
        }

        public int CreatureDefense { get; }

        public int Damage { get; }
    }
}