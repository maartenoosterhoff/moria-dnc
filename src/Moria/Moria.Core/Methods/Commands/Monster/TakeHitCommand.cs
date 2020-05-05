namespace Moria.Core.Methods.Commands.Monster
{
    public class TakeHitCommand : ICommand
    {
        public TakeHitCommand(int monster_id, int damage)
        {
            this.MonsterId = monster_id;
            this.Damage = damage;
        }

        public int MonsterId { get; }

        public int Damage { get; }
    }
}