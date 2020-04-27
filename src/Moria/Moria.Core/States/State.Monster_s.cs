using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Monster_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public int hack_monptr;

        public Monster_t[] monsters { get; } = ArrayInitializer.Initialize<Monster_t>(MON_TOTAL_ALLOCATIONS);
        public int[] monster_levels { get; } = new int[MON_MAX_LEVELS + 1];

        public Monster_t blank_monster { get; } = new Monster_t();
        public int next_free_monster_id { get; set; }
        public int monster_multiply_total { get; set; }
    }
}
