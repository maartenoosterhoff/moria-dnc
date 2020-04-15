namespace Moria.Core.Structures
{
    public class Creature_t
    {
        public string name { get; set; }

        public uint movement { get; set; }

        public uint spells { get; set; }

        public uint defenses { get; set; }

        public uint kill_exp_value { get; set; }

        public uint sleep_counter { get; set; }

        public uint area_affect_radius { get; set; }

        public uint ac { get; set; }

        public uint speed { get; set; }

        public uint sprite { get; set; }

        public Dice_t hit_die { get; set; } = new Dice_t();

        public uint[] damage { get; set; } = new uint[4];

        public uint level { get; set; }
    }

/*

typedef struct {
    const char *name;           // Description of creature
    uint32_t movement;          // Bit field
    uint32_t spells;            // Creature spells
    uint16_t defenses;          // Bit field
    uint16_t kill_exp_value;    // Exp value for kill
    uint8_t sleep_counter;      // Inactive counter / 10
    uint8_t area_affect_radius; // Area affect radius
    uint8_t ac;                 // AC
    uint8_t speed;              // Movement speed+10 (NOTE: +10 so that it can be an unsigned int)
    uint8_t sprite;             // Character representation (cchar)
    Dice_t hit_die;             // Creatures hit die
    uint8_t damage[4];          // Type attack and damage
    uint8_t level;              // Level of creature
} Creature_t;

*/
}
