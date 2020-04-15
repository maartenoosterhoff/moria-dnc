namespace Moria.Core.Structures
{
    public class MonsterAttack_t
    {
        public uint type_id { get; set; }
        public uint description_id { get; set; }

        public Dice_t dice { get; set; } = new Dice_t(0, 0);
    }

    /*

// MonsterAttack_t is a base data object.
// Holds the data for a monster's attack and damage type
typedef struct {
    uint8_t type_id;
    uint8_t description_id;
    Dice_t dice;
} MonsterAttack_t;

 */
}
