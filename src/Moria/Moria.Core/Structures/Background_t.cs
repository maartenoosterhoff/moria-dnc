namespace Moria.Core.Structures
{
    public class Background_t
    {
        public string info { get; set; }
        public uint roll { get; set; }
        public uint chart { get; set; }
        public uint next { get; set; }
        public uint bonus { get; set; }
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
