using System.Diagnostics;

namespace Moria.Core.Structures
{
    public class Coord_t
    {
        public Coord_t()
        {
        }

        public Coord_t(int y, int x)
        {
            this.x = x;
            this.y = y;
        }

        public Coord_t Clone()
        {
            return new Coord_t(this.y, this.x);
        }

        private int _x;
        public int x
        {
            get => _x;
            set
            {
                if (value > 80)
                {
                    Debugger.Break();
                }

                _x = value;
            }
        }

        private int _y;

        public int y
        {
            get => _y;
            set
            {
                if (value > 23)
                {
                    Debugger.Break();
                }

                _y = value;
            }
        }
    }
}
