using Moria.Core.Constants;
using Moria.Core.Utils;

namespace Moria.Core.Data
{
    public static class Tables_d
    {
        public static string[] colors = new string[Identification_c.MAX_COLORS];

        public static string[] mushrooms = new string[Identification_c.MAX_MUSHROOMS];

        public static string[] woods = new string[Identification_c.MAX_WOODS];

        public static string[] metals = new string[Identification_c.MAX_METALS];

        public static string[] rocks = new string[Identification_c.MAX_ROCKS];

        public static string[] amulets = new string[Identification_c.MAX_AMULETS];

        public static string[] syllables = new string[Identification_c.MAX_SYLLABLES];

        public static uint[][] blows =
            ArrayInitializer.InitializeWithDefault<uint>(7, 6);

        public static uint[] normal_table = new uint[Game_c.NORMAL_TABLE_SIZE];
    }
}
