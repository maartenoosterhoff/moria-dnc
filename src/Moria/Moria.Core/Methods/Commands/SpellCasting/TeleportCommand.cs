namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class TeleportCommand : ICommand
    {
        public TeleportCommand(int newDistance)
        {
            this.NewDistance = newDistance;
        }

        public int NewDistance { get; }
    }
}
