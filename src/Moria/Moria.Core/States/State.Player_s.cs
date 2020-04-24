using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Player_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public Player_t py { get; set; }

        public string[][] class_rank_titles { get; set; } =
            ArrayInitializer.InitializeWithDefault<string>(PLAYER_MAX_CLASSES,
                PLAYER_MAX_LEVEL);

        public Race_t[] character_races { get; set; } =
            ArrayInitializer.Initialize<Race_t>(PLAYER_MAX_RACES);

        public Background_t[] character_backgrounds { get; set; } =
            ArrayInitializer.Initialize<Background_t>(PLAYER_MAX_BACKGROUNDS);

        public Class_t[] classes { get; set; } =
            ArrayInitializer.Initialize<Class_t>(PLAYER_MAX_CLASSES);

        public int[][] class_level_adj { get; set; } =
            ArrayInitializer.InitializeWithDefault<int>(PLAYER_MAX_CLASSES,
                CLASS_MAX_LEVEL_ADJUST);

        public uint[][] class_base_provisions { get; set; } =
            ArrayInitializer.InitializeWithDefault<uint>(PLAYER_MAX_CLASSES, 5);
    }
}
