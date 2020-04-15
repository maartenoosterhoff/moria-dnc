using Moria.Core.Constants;
using Moria.Core.Structures;
using Moria.Core.Utils;

namespace Moria.Core.Data
{
    public static class Player_d
    {
        public static string[][] class_rank_titles =
            ArrayInitializer.InitializeWithDefault<string>(Player_c.PLAYER_MAX_CLASSES, Player_c.PLAYER_MAX_LEVEL);

        public static Race_t[] character_races =
            ArrayInitializer.Initialize<Race_t>(Player_c.PLAYER_MAX_RACES);

        public static Background_t[] character_backgrounds =
            ArrayInitializer.Initialize<Background_t>(Player_c.PLAYER_MAX_BACKGROUNDS);

        public static Class_t[] classes =
            ArrayInitializer.Initialize<Class_t>(Player_c.PLAYER_MAX_CLASSES);

        public static int[][] class_level_adj =
            ArrayInitializer.InitializeWithDefault<int>(Player_c.PLAYER_MAX_CLASSES, Player_c.CLASS_MAX_LEVEL_ADJUST);

        public static Spell_t[][] magic_spells =
            ArrayInitializer.InitializeWithDefault<Spell_t>(Player_c.PLAYER_MAX_CLASSES - 1, 31);

        public static string[] spell_names = new string[62];

        public static uint[][] class_base_provisions =
            ArrayInitializer.InitializeWithDefault<uint>(Player_c.PLAYER_MAX_CLASSES, 5);
    }
}
