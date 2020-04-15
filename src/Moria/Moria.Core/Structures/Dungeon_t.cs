using Moria.Core.Constants;
using Moria.Core.Utils;

namespace Moria.Core.Structures
{
    public class Dungeon_t
    {
        public int height { get; set; }
        public int width { get; set; }
        public Panel_t panel { get; set; } = new Panel_t();
        public int game_turn { get; set; }
        public int current_level { get; set; }
        public bool generate_new_level { get; set; }

        public Tile_t[][] floor { get; set; } =
            ArrayInitializer.Initialize<Tile_t>(Dungeon_c.MAX_HEIGHT, Dungeon_c.MAX_WIDTH);
    } 

    /*
typedef struct {
    // Dungeon size is either just big enough for town level, or the whole dungeon itself
    int16_t height;
    int16_t width;

    Panel_t panel;

    // Current turn of the game
    int32_t game_turn;

    // The current dungeon level
    int16_t current_level;

    // A `true` value means a new level will be generated on next loop iteration
    bool generate_new_level;

    // Floor definitions
    Tile_t floor[MAX_HEIGHT][MAX_WIDTH];
} Dungeon_t;
*/
}
