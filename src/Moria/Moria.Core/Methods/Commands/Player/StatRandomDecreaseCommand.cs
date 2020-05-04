namespace Moria.Core.Methods.Commands.Player
{
    public class StatRandomDecreaseCommand : ICommand
    {
        public StatRandomDecreaseCommand(int stat)
        {
            this.Stat = stat;
        }

        public int Stat { get; }
    }
}