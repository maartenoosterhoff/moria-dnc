namespace Moria.Core.Methods.Commands.SpellCasting.Defending
{
    public class TeleportAwayMonsterCommand : ICommand
    {
        public TeleportAwayMonsterCommand(int monster_id, int distance_from_player)
        {
            this.MonsterId = monster_id;
            this.DistanceFromPlayer = distance_from_player;
        }

        public int MonsterId { get; }

        public int DistanceFromPlayer { get; }
    }
}