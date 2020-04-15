using Moria.Core.Constants;
using Moria.Core.Structures;
using Moria.Core.Utils;

namespace Moria.Core.Data
{
    public static class Creature_d
    {
        public static Creature_t[] creatures_list =
            ArrayInitializer.Initialize<Creature_t>(Monster_c.MON_MAX_CREATURES);

        public static MonsterAttack_t[] monster_attacks =
            ArrayInitializer.Initialize<MonsterAttack_t>(Monster_c.MON_ATTACK_TYPES);
    }
}
