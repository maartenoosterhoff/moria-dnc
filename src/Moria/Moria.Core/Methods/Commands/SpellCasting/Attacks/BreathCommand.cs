using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacks
{
    public class BreathCommand : ICommand
    {
        public BreathCommand(Coord_t coord, int monsterId, int damageHp, int spellType, string spellName)
        {
            this.Coord = coord;
            this.MonsterId = monsterId;
            this.DamageHp = damageHp;
            this.SpellType = spellType;
            this.SpellName = spellName;
        }

        public Coord_t Coord { get; }
        public int MonsterId { get; }
        public int DamageHp { get; }
        public int SpellType { get; }
        public string SpellName { get; }
    }
}
