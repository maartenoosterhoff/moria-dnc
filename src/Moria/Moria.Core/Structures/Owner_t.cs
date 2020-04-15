namespace Moria.Core.Structures
{
    public class Owner_t
    {
        public string name { get; set; }
        public int max_cost { get; set; }
        public uint max_inflate { get; set; }
        public uint min_inflate { get; set; }
        public uint haggles_per { get; set; }
        public uint race { get; set; }
        public uint max_insults { get; set; }
    }

    /*
// Owner_t holds data about a given store owner
typedef struct {
    const char *name;
    int16_t max_cost;
    uint8_t max_inflate;
    uint8_t min_inflate;
    uint8_t haggles_per;
    uint8_t race;
    uint8_t max_insults;
} Owner_t;
     */

}
