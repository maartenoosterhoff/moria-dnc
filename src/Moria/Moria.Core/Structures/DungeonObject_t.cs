namespace Moria.Core.Structures
{
    public class DungeonObject_t
    {
        public string name { get; set; }
        public uint flags { get; set; }
        public uint category_id { get; set; }
        public uint sprite { get; set; }
        public int misc_use { get; set; }
        public int cost { get; set; }
        public uint sub_category_id { get; set; }
        public uint items_count { get; set; }
        public uint weight { get; set; }
        public int to_hit { get; set; }
        public int to_damage { get; set; }
        public int ac { get; set; }
        public int to_ac { get; set; }
        public Dice_t damage { get; set; } = new Dice_t();
        public uint depth_first_found { get; set; }
    }

    /*
// DungeonObject_t is a base data object.
// This holds data for any non-living object in the game such as
// stairs, rubble, doors, gold, potions, weapons, wands, etc.
typedef struct {
    const char *name;          // Object name
    uint32_t flags;            // Special flags
    uint8_t category_id;       // Category number (tval)
    uint8_t sprite;            // Character representation - ASCII symbol (tchar)
    int16_t misc_use;          // Misc. use variable (p1)
    int32_t cost;              // Cost of item
    uint8_t sub_category_id;   // Sub-category number (subval)
    uint8_t items_count;       // Number of items
    uint16_t weight;           // Weight
    int16_t to_hit;            // Plusses to hit
    int16_t to_damage;         // Plusses to damage
    int16_t ac;                // Normal AC
    int16_t to_ac;             // Plusses to AC
    Dice_t damage;             // Damage when hits
    uint8_t depth_first_found; // Dungeon level item first found
} DungeonObject_t;
*/
}
