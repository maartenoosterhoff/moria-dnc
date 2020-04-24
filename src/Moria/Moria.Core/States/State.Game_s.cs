using Moria.Core.Structures;
using static Moria.Core.Constants.Game_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public Game_t game { get; } = new Game_t();
        public int[] sorted_objects { get; } = new int[MAX_DUNGEON_OBJECTS];
        public int[] treasure_levels { get; } = new int[TREASURE_MAX_LEVELS + 1];

        public uint old_seed;
    }
}
