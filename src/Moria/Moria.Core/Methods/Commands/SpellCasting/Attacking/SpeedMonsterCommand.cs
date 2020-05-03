using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class SpeedMonsterCommand : ICommand
    {
        public SpeedMonsterCommand(Coord_t coord, int direction, int damage_hp)
        {
            this.Coord = coord;
            this.Direction = direction;
            this.DamageHp = damage_hp;
        }

        public Coord_t Coord { get; }
        
        public int Direction { get; }

        public int DamageHp { get; }
    }
}