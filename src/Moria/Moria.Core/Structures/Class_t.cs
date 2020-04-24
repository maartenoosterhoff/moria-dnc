namespace Moria.Core.Structures
{
    public class Class_t
    {
        public Class_t(
            string title,
            uint hit_points,
            uint disarm_traps,
            uint searching,
            uint stealth,
            uint fos,
            uint base_to_hit,
            uint base_to_hit_with_bows,
            uint saving_throw,
            int strength,
            int intelligence,
            int wisdom,
            int dexterity,
            int constitution,
            int charisma,
            uint class_to_use_mage_spells,
            uint experience_factor,
            uint min_level_for_spell_casting
        )
        {
            this.title = title;
            this.hit_points = hit_points;
            this.disarm_traps = disarm_traps;
            this.searching = searching;
            this.stealth = stealth;
            this.fos = fos;
            this.base_to_hit = base_to_hit;
            this.base_to_hit_with_bows = base_to_hit_with_bows;
            this.saving_throw = saving_throw;
            this.strength = strength;
            this.intelligence = intelligence;
            this.wisdom = wisdom;
            this.dexterity = dexterity;
            this.constitution = constitution;
            this.charisma = charisma;
            this.class_to_use_mage_spells = class_to_use_mage_spells;
            this.experience_factor = experience_factor;
            this.min_level_for_spell_casting = min_level_for_spell_casting;
        }
        public string title { get; }
        public uint hit_points { get; }
        public uint disarm_traps { get; }
        public uint searching { get; }
        public uint stealth { get; }
        public uint fos { get; }
        public uint base_to_hit { get; }
        public uint base_to_hit_with_bows { get; }
        public uint saving_throw { get; }
        public int strength { get; }
        public int intelligence { get; }
        public int wisdom { get; }
        public int dexterity { get; }
        public int constitution { get; }
        public int charisma { get; }
        public uint class_to_use_mage_spells { get; }
        public uint experience_factor { get; }
        public uint min_level_for_spell_casting { get; }
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
