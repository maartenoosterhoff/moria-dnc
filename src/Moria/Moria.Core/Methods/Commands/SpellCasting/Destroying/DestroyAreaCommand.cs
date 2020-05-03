using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Destroying
{
    public class DestroyAreaCommand : ICommand
    {
        public DestroyAreaCommand(Coord_t coord)
        {
            this.Coord = coord;
        }

        public Coord_t Coord { get; }
    }
}
