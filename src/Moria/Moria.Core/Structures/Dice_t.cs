namespace Moria.Core.Structures
{
    public class Dice_t
    {
        public Dice_t(uint dice, uint sides)
        {
            this.dice = dice;
            this.sides = sides;
        }

        public uint dice { get; private set; }

        public uint sides { get; private set; }

        public void SetDice(uint dice)
        {
            this.dice = dice;
        }

        public void SetSides(uint sides)
        {
            this.sides = sides;
        }
    }

/*
    typedef struct {
        uint8_t dice;
        uint8_t sides;
    } Dice_t;

    int diceRoll(Dice_t const &dice);
    int maxDiceRoll(Dice_t const &dice);

    // generates damage for 2d6 style dice rolls
    int diceRoll(Dice_t const &dice) {
        auto sum = 0;
        for (auto i = 0; i < dice.dice; i++) {
            sum += randomNumber(dice.sides);
        }
        return sum;
    }

    // Returns max dice roll value -RAK-
    int maxDiceRoll(Dice_t const &dice) {
        return dice.dice * dice.sides;
    }
 */
}
