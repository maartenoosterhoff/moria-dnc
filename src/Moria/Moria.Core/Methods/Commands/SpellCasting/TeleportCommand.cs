namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class TeleportCommand : ICommand
    {
        public TeleportCommand()
        {
        }

        public TeleportCommand(int newDistance)
        {
            this.NewDistance = newDistance;
        }

        public int NewDistance { get; set; }
    }
}
