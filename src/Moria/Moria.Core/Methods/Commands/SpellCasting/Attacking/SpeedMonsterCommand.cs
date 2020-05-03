using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class SpeedMonsterCommand : ICommand
    {
        public SpeedMonsterCommand(Coord_t coord, int direction, int speed)
        {
            this.Coord = coord;
            this.Direction = direction;
            this.Speed = speed;
        }

        public Coord_t Coord { get; }
        
        public int Direction { get; }

        public int Speed { get; }
    }
}