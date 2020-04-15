using Moria.Core.Constants;
using Moria.Core.Structures;
using Moria.Core.Utils;

namespace Moria.Core.Data
{
    public static class Store_owners_d
    {
        public static Owner_t[] store_owners =
            ArrayInitializer.Initialize<Owner_t>(Store_c.MAX_OWNERS);

        public static string[] speech_sale_accepted = new string[14];

        public static string[] speech_selling_haggle_final = new string[3];

        public static string[] speech_selling_haggle = new string[16];

        public static string[] speech_buying_haggle_final = new string[3];

        public static string[] speech_buying_haggle = new string[15];

        public static string[] speech_insulted_haggling_done = new string[5];

        public static string[] speech_get_out_of_my_store = new string[3];

        public static string[] speech_haggling_try_again = new string[10];

        public static string[] speech_sorry = new string[3];
    }
}
