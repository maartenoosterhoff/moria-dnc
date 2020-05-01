using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacks
{
    public class FireBoltCommand : ICommand
    {
        public FireBoltCommand(Coord_t coord, int direction, int damageHp, int spellType, string spellName)
        {
            this.Coord = coord;
            this.Direction = direction;
            this.DamageHp = damageHp;
            this.SpellType = spellType;
            this.SpellName = spellName;
        }

        public Coord_t Coord { get; }
        public int Direction { get; }
        public int DamageHp { get; }
        public int SpellType { get; }
        public string SpellName { get; }
    }
}
