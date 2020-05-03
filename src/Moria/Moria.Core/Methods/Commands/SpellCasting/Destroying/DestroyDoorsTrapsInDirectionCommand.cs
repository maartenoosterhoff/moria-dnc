using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Destroying
{
    public class DestroyDoorsTrapsInDirectionCommand : ICommand
    {
        public DestroyDoorsTrapsInDirectionCommand(Coord_t coord, int direction)
        {
            this.Coord = coord;
            this.Direction = direction;
        }

        public Coord_t Coord { get; }

        public int Direction { get; }

    }
}
