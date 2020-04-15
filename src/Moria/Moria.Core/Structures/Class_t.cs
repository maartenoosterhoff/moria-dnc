namespace Moria.Core.Structures
{
    public class Class_t
    {
        public string title { get; set; }
        public uint hit_points { get; set; }
        public uint disarm_traps { get; set; }
        public uint searching { get; set; }
        public uint stealth { get; set; }
        public uint fos { get; set; }
        public uint base_to_hit { get; set; }
        public uint base_to_hit_with_bows { get; set; }
        public uint saving_throw { get; set; }
        public int strength { get; set; }
        public int intelligence { get; set; }
        public int wisdom { get; set; }
        public int dexterity { get; set; }
        public int constitution { get; set; }
        public int charisma { get; set; }
        public uint class_to_use_mage_spells { get; set; }
        public uint experience_factor { get; set; }
        public uint min_level_for_spell_casting { get; set; }
    }

    /*
// Class type for the generated player character
typedef struct {
    const char *title;                   // type of class
    uint8_t hit_points;                  // Adjust hit points
    uint8_t disarm_traps;                // mod disarming traps
    uint8_t searching;                   // modifier to searching
    uint8_t stealth;                     // modifier to stealth
    uint8_t fos;                         // modifier to freq-of-search
    uint8_t base_to_hit;                 // modifier to base to hit
    uint8_t base_to_hit_with_bows;       // modifier to base to hit - bows
    uint8_t saving_throw;                // Class modifier to save
    int16_t strength;                    // Class modifier for strength
    int16_t intelligence;                // Class modifier for intelligence
    int16_t wisdom;                      // Class modifier for wisdom
    int16_t dexterity;                   // Class modifier for dexterity
    int16_t constitution;                // Class modifier for constitution
    int16_t charisma;                    // Class modifier for charisma
    uint8_t class_to_use_mage_spells;    // class use mage spells
    uint8_t experience_factor;           // Class experience factor
    uint8_t min_level_for_spell_casting; // First level where class can use spells.
} Class_t;
     */
}
