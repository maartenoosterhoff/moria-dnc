using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class TeleportPlayerToCommand : ICommand
    {
        public TeleportPlayerToCommand(Coord_t coord)
        {
            this.Coord = coord;
        }

        public Coord_t Coord { get; }
    }
}
