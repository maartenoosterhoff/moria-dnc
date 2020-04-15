namespace Moria.Core.Structures
{
    public class Spell_t
    {
        public uint level_required { get; set; }
        public uint mana_required { get; set; }
        public uint failure_chance { get; set; }
        public uint exp_gain_for_learning { get; set; }
    }

    /*
// Spell_t is a base data object.
// Holds the base game data for a spell
// Note: the names for the spells are stored in spell_names[] array at index i, +31 if priest
typedef struct {
    uint8_t level_required;
    uint8_t mana_required;
    uint8_t failure_chance;
    uint8_t exp_gain_for_learning; // 1/4 of exp gained for learning spell
} Spell_t;
     */
}
