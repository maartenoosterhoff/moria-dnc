using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Game_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public Dungeon_t dg { get; set; } = new Dungeon_t();

        public DungeonObject_t[] game_objects { get; set; } = new DungeonObject_t[MAX_OBJECTS_IN_GAME];

        public Coord_t[] doors_tk { get; set; } = ArrayInitializer.Initialize<Coord_t>(100);
        public int door_index { get; set; }
    }
}
