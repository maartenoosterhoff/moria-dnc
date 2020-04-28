using Moria.Core.Structures;

namespace Moria.Core.Methods
{
    public interface IDice
    {
        int diceRoll(Dice_t dice);
        int maxDiceRoll(Dice_t dice);
    }

    public class Dice_m : IDice
    {
        private readonly IRnd rnd;

        public Dice_m(IRnd rnd)
        {
            this.rnd = rnd;
        }
        
        public int diceRoll(Dice_t dice)
        {
            var sum = 0;
            for (var i = 0; i < dice.dice; i++)
            {
                sum += this.rnd.randomNumber(dice.sides);
            }
            return sum;
        }

        public int maxDiceRoll(Dice_t dice)
        {
            return (int)(dice.dice * dice.sides);
        }
    }
}
