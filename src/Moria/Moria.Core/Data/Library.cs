namespace Moria.Core.Data
{
    public class Library
    {
        public static readonly Library Instance = new Library();

        public Creature_d Creatures { get; } = new Creature_d();

        public Treasure_d Treasure { get; } = new Treasure_d();
    }
}
