using Moria.Core.Structures;
using static Moria.Core.Methods.Game_m;

namespace Moria.Core.Methods
{
    public static class Dice_m
    {
        public static int diceRoll(Dice_t dice)
        {
            var sum = 0;
            for (var i = 0; i < dice.dice; i++)
            {
                sum += randomNumber(dice.sides);
            }
            return sum;
        }

        public static int maxDiceRoll(Dice_t dice)
        {
            return (int)(dice.dice * dice.sides);
        }
    }
}
