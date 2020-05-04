using Moria.Core.Configs;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.Player
{
    public class DisturbCommandHandler : ICommandHandler<DisturbCommand>
    {
        private readonly ITerminal terminal;

        public DisturbCommandHandler(ITerminal terminal)
        {
            this.terminal = terminal;
        }

        public void Handle(DisturbCommand command)
        {
            this.playerDisturb(
                command.MajorDisturbance,
                command.LightDisturbance
            );
        }

        // Something happens to disturb the player. -CJS-
        // The first arg indicates a major disturbance, which affects search.
        // The second arg indicates a light change.
        private void playerDisturb(bool major_disturbance, bool light_disturbance)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.command_count = 0;

            if (major_disturbance && (py.flags.status & Config.player_status.PY_SEARCH) != 0u)
            {
                Player_m.playerSearchOff();
            }

            if (py.flags.rest != 0)
            {
                Player_m.playerRestOff();
            }

            if (light_disturbance || py.running_tracker != 0)
            {
                py.running_tracker = 0;
                Ui_m.dungeonResetView();
            }

            this.terminal.flushInputBuffer();
        }
    }
}