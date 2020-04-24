namespace Moria.Core.Structures
{
    public class Race_t
    {
        public Race_t(
            string name,
            int str_adjustment,
            int int_adjustment,
            int wis_adjustment,
            int dex_adjustment,
            int con_adjustment,
            int chr_adjustment,
            uint base_age,
            uint max_age,
            uint male_height_base,
            uint male_height_mod,
            uint male_weight_base,
            uint male_weight_mod,
            uint female_height_base,
            uint female_height_mod,
            uint female_weight_base,
            uint female_weight_mod,
            int disarm_chance_base,
            int search_chance_base,
            int stealth,
            int fos,
            int base_to_hit,
            int base_to_hit_bows,
            int saving_throw_base,
            uint hit_points_base,
            uint infra_vision,
            uint exp_factor_base,
            uint classes_bit_field        )
        {
            this.name = name;
            this.str_adjustment = str_adjustment;
            this.int_adjustment = int_adjustment;
            this.wis_adjustment = wis_adjustment;
            this.dex_adjustment = dex_adjustment;
            this.con_adjustment = con_adjustment;
            this.chr_adjustment = chr_adjustment;
            this.base_age = base_age;
            this.max_age = max_age;
            this.male_height_base = male_height_base;
            this.male_height_mod = male_height_mod;
            this.male_weight_base = male_weight_base;
            this.male_weight_mod = male_weight_mod;
            this.female_height_base = female_height_base;
            this.female_height_mod = female_height_mod;
            this.female_weight_base = female_weight_base;
            this.female_weight_mod = female_weight_mod;
            this.disarm_chance_base = disarm_chance_base;
            this.search_chance_base = search_chance_base;
            this.stealth = stealth;
            this.fos = fos;
            this.base_to_hit = base_to_hit;
            this.base_to_hit_bows = base_to_hit_bows;
            this.saving_throw_base = saving_throw_base;
            this.hit_points_base = hit_points_base;
            this.infra_vision = infra_vision;
            this.exp_factor_base = exp_factor_base;
            this.classes_bit_field = classes_bit_field;
        }
        public string name { get; set; }
        public int str_adjustment { get; set; }
        public int int_adjustment { get; set; }
        public int wis_adjustment { get; set; }
        public int dex_adjustment { get; set; }
        public int con_adjustment { get; set; }
        public int chr_adjustment { get; set; }
        public uint base_age { get; set; }
        public uint max_age { get; set; }
        public uint male_height_base { get; set; }
        public uint male_height_mod { get; set; }
        public uint male_weight_base { get; set; }
        public uint male_weight_mod { get; set; }
        public uint female_height_base { get; set; }
        public uint female_height_mod { get; set; }
        public uint female_weight_base { get; set; }
        public uint female_weight_mod { get; set; }
        public int disarm_chance_base { get; set; }
        public int search_chance_base { get; set; }
        public int stealth { get; set; }
        public int fos { get; set; }
        public int base_to_hit { get; set; }
        public int base_to_hit_bows { get; set; }
        public int saving_throw_base { get; set; }
        public uint hit_points_base { get; set; }
        public uint infra_vision { get; set; }
        public uint exp_factor_base { get; set; }
        public uint classes_bit_field { get; set; }
    }

    /*
// Race type for the generated player character
typedef struct {
    const char *name;       // Type of race
    int16_t str_adjustment; // adjustments
    int16_t int_adjustment;
    int16_t wis_adjustment;
    int16_t dex_adjustment;
    int16_t con_adjustment;
    int16_t chr_adjustment;
    uint8_t base_age;           // Base age of character
    uint8_t max_age;            // Maximum age of character
    uint8_t male_height_base;   // base height for males
    uint8_t male_height_mod;    // mod height for males
    uint8_t male_weight_base;   // base weight for males
    uint8_t male_weight_mod;    // mod weight for males
    uint8_t female_height_base; // base height females
    uint8_t female_height_mod;  // mod height for females
    uint8_t female_weight_base; // base weight for female
    uint8_t female_weight_mod;  // mod weight for females
    int16_t disarm_chance_base; // base chance to disarm
    int16_t search_chance_base; // base chance for search
    int16_t stealth;            // Stealth of character
    int16_t fos;                // frequency of auto search
    int16_t base_to_hit;        // adj base chance to hit
    int16_t base_to_hit_bows;   // adj base to hit with bows
    int16_t saving_throw_base;  // Race base for saving throw
    uint8_t hit_points_base;    // Base hit points for race
    uint8_t infra_vision;       // See infra-red
    uint8_t exp_factor_base;    // Base experience factor
    uint8_t classes_bit_field;  // Bit field for class types
} Race_t;
     */
}
