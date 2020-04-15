namespace Moria.Core.Constants
{
    public static class Monster_c
    {
        // Creature constants
        public const uint MON_MAX_CREATURES = 279; // Number of creatures defined for univ
        public const uint MON_ATTACK_TYPES = 215;   // Number of monster attack types.

        // With MON_TOTAL_ALLOCATIONS set to 101, it is possible to get compacting
        // monsters messages while breeding/cloning monsters.
        public const uint MON_TOTAL_ALLOCATIONS = 125; // Max that can be allocated
        public const uint MON_MAX_LEVELS = 40;         // Maximum level of creatures
        public const uint MON_MAX_ATTACKS = 4;         // Max num attacks (used in mons memory) -CJS-
    }
}
