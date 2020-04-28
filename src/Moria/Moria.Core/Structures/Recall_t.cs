using Moria.Core.Constants;

namespace Moria.Core.Structures
{
    public class Recall_t
    {
        public uint movement { get; set; }
        public uint spells { get; set; }
        public uint kills { get; set; }
        public uint deaths { get; set; }
        public uint defenses { get; set; }
        public uint wake { get; set; }
        public uint ignore { get; set; }
        public uint[] attacks { get; } = new uint[Monster_c.MON_MAX_ATTACKS];
    }

    /*
// Recall_t holds the player's known knowledge for any given monster, aka memories
typedef struct {
    uint32_t movement;
    uint32_t spells;
    uint16_t kills;
    uint16_t deaths;
    uint16_t defenses;
    uint8_t wake;
    uint8_t ignore;
    uint8_t attacks[MON_MAX_ATTACKS];
} Recall_t;
     */
}
