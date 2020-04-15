using Moria.Core.Constants;
using Moria.Core.Utils;

namespace Moria.Core.Data
{
    public static class Stores_d
    {
        public static uint[][] race_gold_adjustments =
            ArrayInitializer.InitializeWithDefault<uint>(Player_c.PLAYER_MAX_RACES, Player_c.PLAYER_MAX_RACES);

        public static uint[][] store_choices =
            ArrayInitializer.InitializeWithDefault<uint>(Store_c.MAX_STORES, Store_c.STORE_MAX_ITEM_TYPES);


    }
}
