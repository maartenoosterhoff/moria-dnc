using Moria.Core.Structures;
using Moria.Core.Utils;
using static Moria.Core.Constants.Player_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public Spell_t[][] magic_spells { get; set; } =
            ArrayInitializer.Initialize<Spell_t>(PLAYER_MAX_CLASSES - 1, 31);
        public string[] spell_names { get; set; } = new string[62];
    }
}
