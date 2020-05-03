namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class SpeedAllMonstersCommand : ICommand
    {
        public SpeedAllMonstersCommand(int speed)
        {
            this.Speed = speed;
        }

        public int Speed { get; }
    }
}