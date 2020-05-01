using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class StarlightCommand : ICommand
    {
        public StarlightCommand(Coord_t coord)
        {
            this.Coord = coord;
        }

        public Coord_t Coord { get; }
    }
}
