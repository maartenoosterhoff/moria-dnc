using Moria.Core.Constants;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using Moria.Core.Utils;

namespace Moria.Core.Data
{
    public static class Treasure_d
    {
        public static DungeonObject_t[] game_objects =
            ArrayInitializer.Initialize<DungeonObject_t>(Game_c.MAX_OBJECTS_IN_GAME);

        public static string[] special_item_names = new string[(int)SpecialNameIds.SN_ARRAY_SIZE];
    }
}
