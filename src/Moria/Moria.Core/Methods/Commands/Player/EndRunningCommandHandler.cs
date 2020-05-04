using Moria.Core.States;

namespace Moria.Core.Methods.Commands.Player
{
    public class EndRunningCommandHandler : ICommandHandler<EndRunningCommand>
    {
        private readonly IDungeon dungeon;

        public EndRunningCommandHandler(IDungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public void Handle(EndRunningCommand command)
        {
            this.playerEndRunning();
        }

        // Switch off the run flag - and get the light correct. -CJS-
        private void playerEndRunning()
        {
            var py = State.Instance.py;
            if (py.running_tracker == 0)
            {
                return;
            }

            py.running_tracker = 0;

            this.dungeon.dungeonMoveCharacterLight(py.pos, py.pos);
        }
    }
}