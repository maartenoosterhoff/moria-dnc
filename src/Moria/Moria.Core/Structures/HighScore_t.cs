namespace Moria.Core.Structures
{
    public class HighScore_t
    {
        public int points { get; set; }
        public int birth_date { get; set; }
        public int uid { get; set; }
        public int mhp { get; set; }
        public int chp { get; set; }
        public uint dungeon_depth { get; set; }
        public uint level { get; set; }
        public uint deepest_dungeon_depth { get; set; }
        public uint gender { get; set; }
        public uint race { get; set; }
        public uint character_class { get; set; }
        public string name { get; set; }
        public string died_from { get; set; }
    }

    /*
// HighScore_t is a score object used for saving to the high score file
// This structure is 64 bytes in size
typedef struct {
    int32_t points;
    int32_t birth_date;
    int16_t uid;
    int16_t mhp;
    int16_t chp;
    uint8_t dungeon_depth;
    uint8_t level;
    uint8_t deepest_dungeon_depth;
    uint8_t gender;
    uint8_t race;
    uint8_t character_class;
    char name[PLAYER_NAME_SIZE];
    char died_from[25];
} HighScore_t;
     */
}
