using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Monster_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public Recall_t[] creature_recall { get; set; } =
            ArrayInitializer.Initialize<Recall_t>(MON_MAX_CREATURES); // Monster memories. -CJS-

        public string[] recall_description_attack_type { get; set; } = new string[25];
        public string[] recall_description_attack_method { get; set; } = new string[20];
        public string[] recall_description_how_much { get; set; } = new string[8];
        public string[] recall_description_move { get; set; } = new string[6];
        public string[] recall_description_spell { get; set; } = new string[15];
        public string[] recall_description_breath { get; set; } = new string[5];
        public string[] recall_description_weakness { get; set; } = new string[6];

        public string roff_buffer { get; set; }= string.Empty;
        public int roff_print_line { get; set; } = 0;                 // Place to print line now being loaded.

    }
}
