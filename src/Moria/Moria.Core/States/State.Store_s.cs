using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Store_c;

namespace Moria.Core.States
{
    public partial class State
    {
        //public uint[][] race_gold_adjustments { get; set; } =
        //    ArrayInitializer.InitializeWithDefault<uint>(PLAYER_MAX_RACES,
        //        PLAYER_MAX_RACES);

        //public Owner_t[] store_owners { get; set; } =
        //    ArrayInitializer.Initialize<Owner_t>(MAX_OWNERS);

        public Store_t[] stores { get; set; } =
            ArrayInitializer.Initialize<Store_t>(MAX_STORES);

        //public uint[][] store_choices { get; set; } =
        //    ArrayInitializer.InitializeWithDefault<uint>(MAX_STORES, STORE_MAX_ITEM_TYPES);

        //public string[] speech_sale_accepted { get; set; } = new string[14];
        //public string[] speech_selling_haggle_final { get; set; } = new string[3];
        //public string[] speech_selling_haggle { get; set; } = new string[16];
        //public string[] speech_buying_haggle_final { get; set; } = new string[3];
        //public string[] speech_buying_haggle { get; set; } = new string[15];
        //public string[] speech_insulted_haggling_done { get; set; } = new string[5];
        //public string[] speech_get_out_of_my_store { get; set; } = new string[5];
        //public string[] speech_haggling_try_again { get; set; } = new string[10];
        //public string[] speech_sorry { get; set; } = new string[5];

        public int store_last_increment;
    }
}
