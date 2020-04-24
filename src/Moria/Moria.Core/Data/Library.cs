namespace Moria.Core.Data
{
    public class Library
    {
        public static readonly Library Instance = new Library();

        public Creature_d Creatures { get; } = new Creature_d();

        public Treasure_d Treasure { get; } = new Treasure_d();

        public Tables_d Tables { get; } = new Tables_d();

        public Stores_d Stores { get; } = new Stores_d();

        public Store_owners_d StoreOwners { get; } = new Store_owners_d();
    }
}
