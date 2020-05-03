using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class DisarmAllInDirectionCommand : ICommand
    {
        public DisarmAllInDirectionCommand(Coord_t coord, int direction)
        {
            this.Coord = coord;
            this.Direction = direction;
        }

        public Coord_t Coord { get; }

        public int Direction { get; }
    }
}