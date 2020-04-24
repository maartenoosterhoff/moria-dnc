namespace Moria.Core.Structures
{
    public class Background_t
    {
        public Background_t(
            string info,
            uint roll,
            uint chart,
            uint next,
            uint bonus
        )
        {
            this.info = info;
            this.roll = roll;
            this.chart = chart;
            this.next = next;
            this.bonus = bonus;
        }

        public string info { get; }
        public uint roll { get; }
        public uint chart { get; }
        public uint next { get; }
        public uint bonus { get; }
    }

    /*
// Class background for the generated player character
typedef struct {
    const char *info; // History information
    uint8_t roll;     // Die roll needed for history
    uint8_t chart;    // Table number
    uint8_t next;     // Pointer to next table
    uint8_t bonus;    // Bonus to the Social Class+50
} Background_t;
     */
}
