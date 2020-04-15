namespace Moria.Core.Structures
{
    public class Monster_t
    {
        public int hp { get; set; }
        public int sleep_count { get; set; }
        public int speed { get; set; }
        public uint creature_id { get; set; }

        public Coord_t pos { get; set; } = new Coord_t();
        public uint distance_from_player { get; set; }

        public bool lit { get; set; }

        public uint stunned_amount { get; set; }
        public uint confused_amount { get; set; }
    }

    /*

// Creature_t is a base data object.
// Holds the base game data for any given creature in the game such
// as: Kobold, Orc, Giant Red Ant, Quasit, Young Black Dragon, etc.
typedef struct {
    int16_t hp;           // Hit points
    int16_t sleep_count;  // Inactive counter
    int16_t speed;        // Movement speed
    uint16_t creature_id; // Pointer into creature

    // Note: fy, fx, and cdis constrain dungeon size to less than 256 by 256
    Coord_t pos;                  // (y,x) Pointer into map
    uint8_t distance_from_player; // Current distance from player

    bool lit;
    uint8_t stunned_amount;
    uint8_t confused_amount;
} Monster_t;
*/
}
