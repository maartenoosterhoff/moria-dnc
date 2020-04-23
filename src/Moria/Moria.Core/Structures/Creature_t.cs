namespace Moria.Core.Structures
{
    public class Creature_t
    {
        public Creature_t(
            string name,
            long movement,
            long spells,
            long defenses,
            long kill_exp_value,
            uint sleep_counter,
            uint area_affect_radius,
            uint ac,
            uint speed,
            uint sprite,
            uint hit_die_dice,
            uint hit_die_sides,
            uint damage1,
            uint damage2,
            uint damage3,
            uint damage4,
            uint level
        ) : this(
            name,
            (uint)movement,
            (uint)spells,
            (uint)defenses,
            (uint)kill_exp_value,
            sleep_counter,
            area_affect_radius,
            ac,
            speed,
            sprite,
            new Dice_t(hit_die_dice, hit_die_sides),
            new[]
            {
                damage1,
                damage2,
                damage3,
                damage4
            },
            level
        )
        {

        }

        public Creature_t(
            string name,
            uint movement,
            uint spells,
            uint defenses,
            uint kill_exp_value,
            uint sleep_counter,
            uint area_affect_radius,
            uint ac,
            uint speed,
            uint sprite,
            Dice_t hit_die,
            uint[] damage,
            uint level
        )
        {
            this.name = name;
            this.movement = movement;
            this.spells = spells;
            this.defenses = defenses;
            this.kill_exp_value = kill_exp_value;
            this.sleep_counter = sleep_counter;
            this.area_affect_radius = area_affect_radius;
            this.ac = ac;
            this.speed = speed;
            this.sprite = sprite;
            this.hit_die = hit_die;
            this.damage = damage;
            this.level = level;
        }

        public string name { get; }

        public uint movement { get; }

        public uint spells { get; }

        public uint defenses { get; }

        public uint kill_exp_value { get; }

        public uint sleep_counter { get; }

        public uint area_affect_radius { get; }

        public uint ac { get; }

        public uint speed { get; }

        public uint sprite { get; }

        public Dice_t hit_die { get; }

        public uint[] damage { get; }

        public uint level { get; }
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
