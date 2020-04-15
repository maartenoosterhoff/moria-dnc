namespace Moria.Core.Structures
{
    public class Game_t
    {
        public uint magic_seed { get; set; } = 0; // Seed for initializing magic items (Potions, Wands, Staves, Scrolls, etc.)
        public uint town_seed { get; set; } = 0;  // Seed for town generation

        public bool character_generated { get; set; } = false; // Don't save score until character generation is finished
        public bool character_saved { get; set; } = false;     // Prevents save on kill after saving a character
        public bool character_is_dead { get; set; } = false;   // `true` if character has died

        public bool total_winner { get; set; } = false; // Character beat the Balrog

        public bool teleport_player { get; set; } = false;  // Handle teleport traps
        public bool player_free_turn { get; set; } = false; // Player has a free turn, so do not move creatures

        public bool to_be_wizard { get; set; } = false; // Player requests to be Wizard - used during startup, when -w option used
        public bool wizard_mode { get; set; } = false;  // Character is a Wizard when true
        public int noscore { get; set; } = 0;       // Don't save a score for this game. -CJS-

        public bool use_last_direction { get; set; } = false;  // `true` when repeat commands should use last known direction
        public int doing_inventory_command { get; set; } = 0; // Track inventory commands -CJS-
        public char last_command { get; set; } = ' ';          // Save of the previous player command
        public int command_count { get; set; } = 0;            // How many times to repeat a specific command -CJS-

        public string character_died_from = null; // What the character died from: starvation, Bat, etc.

        public Game_treasure_t treasure { get; set; } = new Game_treasure_t();
    }

    /*
typedef struct {
    uint32_t magic_seed = 0; // Seed for initializing magic items (Potions, Wands, Staves, Scrolls, etc.)
    uint32_t town_seed = 0;  // Seed for town generation

    bool character_generated = false; // Don't save score until character generation is finished
    bool character_saved = false;     // Prevents save on kill after saving a character
    bool character_is_dead = false;   // `true` if character has died

    bool total_winner = false; // Character beat the Balrog

    bool teleport_player = false;  // Handle teleport traps
    bool player_free_turn = false; // Player has a free turn, so do not move creatures

    bool to_be_wizard = false; // Player requests to be Wizard - used during startup, when -w option used
    bool wizard_mode = false;  // Character is a Wizard when true
    int16_t noscore = 0;       // Don't save a score for this game. -CJS-

    bool use_last_direction = false;  // `true` when repeat commands should use last known direction
    char doing_inventory_command = 0; // Track inventory commands -CJS-
    char last_command = ' ';          // Save of the previous player command
    int command_count = 0;            // How many times to repeat a specific command -CJS-

    vtype_t character_died_from = {'\0'}; // What the character died from: starvation, Bat, etc.

    struct {
        int16_t current_id = 0; // Current treasure heap ptr
        Inventory_t list[LEVEL_MAX_OBJECTS]{};
    } treasure;
} Game_t;
     */
}
