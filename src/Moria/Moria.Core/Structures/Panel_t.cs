namespace Moria.Core.Structures
{
    public class Panel_t
    {
        public int row { get; set; }
        public int col { get; set; }
        
        public int top { get; set; }
        public int bottom { get; set; }
        public int left { get; set; }
        public int right { get; set; }

        public int col_prt { get; set; }
        public int row_prt { get; set; }

        public int max_rows { get; set; }
        public int max_cols { get; set; }
    }

    /*
// Panel_t holds data about a screen panel (the dungeon display)
// Screen panels calculated from the dungeon/screen dimensions
typedef struct {
    int row;
    int col;

    int top;
    int bottom;
    int left;
    int right;

    int col_prt;
    int row_prt;

    int16_t max_rows;
    int16_t max_cols;
} Panel_t;
*/
}
