using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Store_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public Store_t[] stores { get; set; } =
            ArrayInitializer.Initialize<Store_t>(MAX_STORES);

        public int store_last_increment;
    }
}
