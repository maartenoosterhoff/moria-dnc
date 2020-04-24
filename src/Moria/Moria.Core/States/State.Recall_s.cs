using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Monster_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public Recall_t[] creature_recall { get; set; } =
            ArrayInitializer.Initialize<Recall_t>(MON_MAX_CREATURES); // Monster memories. -CJS-

        public string roff_buffer { get; set; }= string.Empty;
        public int roff_print_line { get; set; } = 0;                 // Place to print line now being loaded.

    }
}
