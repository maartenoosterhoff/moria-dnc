namespace Moria.Core.Structures
{
    public class Spell_t
    {
        public Spell_t(
            uint level_required,
            uint mana_required,
            uint failure_chance,
            uint exp_gain_for_learning
        )
        {
            this.level_required = level_required;
            this.mana_required = mana_required;
            this.failure_chance = failure_chance;
            this.exp_gain_for_learning = exp_gain_for_learning;
        }

        public uint level_required { get; }
        public uint mana_required { get; }
        public uint failure_chance { get; }
        public uint exp_gain_for_learning { get; }
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
