using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Lighting
{
    public class LightAreaCommand : ICommand
    {
        public LightAreaCommand(Coord_t coord)
        {
            this.Coord = coord;
        }

        public Coord_t Coord { get; }
    }
}
