using System.Collections.Generic;

namespace Moria.Core.Structures
{
    public class MonsterAttack_t
    {
        public MonsterAttack_t(uint type_id, uint description_id, Dice_t dice)
        {
            this.type_id = type_id;
            this.description_id = description_id;
            this.dice = dice;
        }

        public uint type_id { get; }
        public uint description_id { get; }

        public Dice_t dice { get; }
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
