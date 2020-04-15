namespace Moria.Core.Structures
{
    public class Tile_t
    {
        public uint creature_id { get; set; }
        public uint treasure_id { get; set; }
        public uint feature_id { get; set; }

        public bool perma_lit_room { get; set; } = true;
        public bool field_mark { get; set; } = true;
        public bool permanent_light { get; set; } = true;
        public bool temporary_light { get; set; } = true;
    }

    /*
// Tile_t holds data about a specific tile in the dungeon.
typedef struct {
    uint8_t creature_id; // ID for any creature occupying the tile
    uint8_t treasure_id; // ID for any treasure item occupying the tile
    uint8_t feature_id;  // ID of cave feature; walls, floors, open space, etc.

    bool perma_lit_room : 1;  // Room should be lit with perm light, walls with this set should be perm lit after tunneled out.
    bool field_mark : 1;      // Field mark, used for traps/doors/stairs, object is hidden if fm is false.
    bool permanent_light : 1; // Permanent light, used for walls and lighted rooms.
    bool temporary_light : 1; // Temporary light, used for player's lamp light,etc.
} Tile_t;
     */
}
