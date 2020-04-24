using Moria.Core.Structures;
using Moria.Core.Utils;

namespace Moria.Core.States
{
    public partial class State
    {
        public Dungeon_t dg { get; } = new Dungeon_t();
        public Coord_t[] doors_tk { get; } = ArrayInitializer.Initialize<Coord_t>(100);

        public int door_index { get; set; }
    }
}
