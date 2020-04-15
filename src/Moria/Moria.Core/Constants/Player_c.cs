namespace Moria.Core.Constants
{
    public static class Player_c
    {
        // this depends on the fact that py_class_level_adj::CLASS_SAVE values are all the same,
        // if not, then should add a separate column for this
        public const uint CLASS_MISC_HIT = 4;
        public const uint CLASS_MAX_LEVEL_ADJUST = 5;

        // Player constants
        public const uint PLAYER_MAX_LEVEL = 40;        // Maximum possible character level
        public const uint PLAYER_MAX_CLASSES = 6;       // Number of defined classes
        public const uint PLAYER_MAX_RACES = 8;         // Number of defined races
        public const uint PLAYER_MAX_BACKGROUNDS = 128; // Number of types of histories for univ

        // Base to hit constants
        public const uint BTH_PER_PLUS_TO_HIT_ADJUST = 3; // Adjust BTH per plus-to-hit

        public const uint PLAYER_NAME_SIZE = 27;
    }
}
