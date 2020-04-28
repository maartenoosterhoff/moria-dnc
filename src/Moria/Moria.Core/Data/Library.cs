namespace Moria.Core.Data
{

    public interface ILibrary
    {
        Creature_d Creatures { get; }
        Treasure_d Treasure { get; }
        Tables_d Tables { get; }
        Stores_d Stores { get; }
        Store_owners_d StoreOwners { get; }
        Recall_d Recall { get; }
        Player_d Player { get; }
    }

    public class Library : ILibrary
    {
        public static readonly ILibrary Instance = new Library();

        public Creature_d Creatures { get; } = new Creature_d();

        public Treasure_d Treasure { get; } = new Treasure_d();

        public Tables_d Tables { get; } = new Tables_d();

        public Stores_d Stores { get; } = new Stores_d();

        public Store_owners_d StoreOwners { get; } = new Store_owners_d();

        public Recall_d Recall { get; } = new Recall_d();

        public Player_d Player { get; } = new Player_d();
    }
}
