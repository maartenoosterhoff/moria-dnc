namespace Moria.Core.Structures
{
    public class Owner_t
    {
        public Owner_t(
            string name,
            int max_cost,
            uint max_inflate,
            uint min_inflate,
            uint haggles_per,
            uint race,
            uint max_insults
        )
        {
            this.name = name;
            this.max_cost = max_cost;
            this.max_inflate = max_inflate;
            this.min_inflate = min_inflate;
            this.haggles_per = haggles_per;
            this.race = race;
            this.max_insults = max_insults;
        }

        public string name { get; }
        public int max_cost { get; }
        public uint max_inflate { get; }
        public uint min_inflate { get; }
        public uint haggles_per { get; }
        public uint race { get; }
        public uint max_insults { get; }
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
