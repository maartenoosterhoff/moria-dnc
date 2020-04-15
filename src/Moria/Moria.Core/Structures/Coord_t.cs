namespace Moria.Core.Structures
{
    public class Coord_t
    {
        public Coord_t()
        {
        }

        public Coord_t(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x { get; set; }
        public int y { get; set; }
    }
}
