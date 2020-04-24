using System.Runtime.CompilerServices;

namespace Moria.Core.Structures
{
    public class DungeonObject_t
    {
        public DungeonObject_t(
            string name,
            long flags,
            uint category_id,
            uint sprite,
            int misc_use,
            int cost,
            uint sub_category_id,
            uint items_count,
            uint weight,
            int to_hit,
            int to_damage,
            int ac,
            int to_ac,
            uint damage_dice,
            uint damage_sides,
            uint depth_first_found
        ) : this(
            name,
            (uint)flags,
            category_id,
            sprite,
            misc_use,
            cost,
            sub_category_id,
            items_count,
            weight,
            to_hit,
            to_damage,
            ac,
            to_ac,
            new Dice_t(damage_dice, damage_sides),
            depth_first_found
        )
        {
        }

        public DungeonObject_t(
            string name,
            uint flags,
            uint category_id,
            uint sprite,
            int misc_use,
            int cost,
            uint sub_category_id,
            uint items_count,
            uint weight,
            int to_hit,
            int to_damage,
            int ac,
            int to_ac,
            Dice_t damage,
            uint depth_first_found
        )
        {
            this.name = name;
            this.flags = flags;
            this.category_id = category_id;
            this.sprite = sprite;
            this.misc_use = misc_use;
            this.cost = cost;
            this.sub_category_id = sub_category_id;
            this.items_count = items_count;
            this.weight = weight;
            this.to_hit = to_hit;
            this.to_damage = to_damage;
            this.ac = ac;
            this.to_ac = to_ac;
            this.damage = damage;
            this.depth_first_found = depth_first_found;
        }

        public DungeonObject_t ApplyCostAdjustment(int cost)
        {
            return new DungeonObject_t(
                this.name,
                this.flags,
                this.category_id,
                this.sprite,
                this.misc_use,
                cost,
                this.sub_category_id,
                this.items_count,
                this.weight,
                this.to_hit,
                this.to_damage,
                this.ac,
                this.to_ac,
                this.damage,
                this.depth_first_found
            );
        }

        public string name { get; }
        public uint flags { get; }
        public uint category_id { get; }
        public uint sprite { get; }
        public int misc_use { get; }
        public int cost { get; }
        public uint sub_category_id { get; }
        public uint items_count { get; }
        public uint weight { get; }
        public int to_hit { get; }
        public int to_damage { get; }
        public int ac { get; }
        public int to_ac { get; }
        public Dice_t damage { get; }
        public uint depth_first_found { get; }
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
