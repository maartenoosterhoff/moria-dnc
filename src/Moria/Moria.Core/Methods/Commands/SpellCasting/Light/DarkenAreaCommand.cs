using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Light
{
    public class DarkenAreaCommand : ICommand
    {
        public DarkenAreaCommand(Coord_t coord)
        {
            this.Coord = coord;
        }

        public Coord_t Coord { get; }
    }
}
