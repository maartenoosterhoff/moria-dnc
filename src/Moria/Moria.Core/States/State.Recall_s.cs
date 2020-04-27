using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Monster_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public Recall_t[] creature_recall { get; set; } =
            ArrayInitializer.Initialize<Recall_t>(MON_MAX_CREATURES); // Monster memories. -CJS-

        public string roff_buffer { get; set; } = new string(' ', 80);

        public string roff_buffer_pointer { get; set; }

        public int roff_print_line { get; set; } // Place to print line now being loaded.

    }
}
