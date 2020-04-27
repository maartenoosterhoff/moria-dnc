namespace Moria.Core.Structures
{
    public class Inventory_t
    {
        public uint id { get; set; }
        public uint special_name_id { get; set; }
        public string inscription { get; set; }
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
        public Dice_t damage { get; set; }
        public uint depth_first_found { get; set; }
        public uint identification { get; set; }
    }
    /*
// Inventory_t is created for an item the player may wear about
// their person, or store in their inventory pack.
//
// Only damage, ac, and tchar are constant; level could possibly be made
// constant by changing index instead; all are used rarely.
//
// Extra fields x and y for location in dungeon would simplify pusht().
//
// Making inscrip[] a pointer and malloc-ing space does not work, there are
// two many places where `Inventory_t` are copied, which results in dangling
// pointers, so we use a char array for them instead.
typedef struct {
    uint16_t id;                    // Index to object_list
    uint8_t special_name_id;        // Object special name
    char inscription[INSCRIP_SIZE]; // Object inscription
    uint32_t flags;                 // Special flags
    uint8_t category_id;            // Category number (tval)
    uint8_t sprite;                 // Character representation - ASCII symbol (tchar)
    int16_t misc_use;               // Misc. use variable (p1)
    int32_t cost;                   // Cost of item
    uint8_t sub_category_id;        // Sub-category number
    uint8_t items_count;            // Number of items
    uint16_t weight;                // Weight
    int16_t to_hit;                 // Plusses to hit
    int16_t to_damage;              // Plusses to damage
    int16_t ac;                     // Normal AC
    int16_t to_ac;                  // Plusses to AC
    Dice_t damage;                  // Damage when hits
    uint8_t depth_first_found;      // Dungeon level item first found
    uint8_t identification;         // Identify information
} Inventory_t;
*/
}
