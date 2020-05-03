using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Lighting
{
    public class StarlightCommandHandler : ICommandHandler<StarlightCommand>
    {
        private readonly IEventPublisher eventPublisher;

        public StarlightCommandHandler(IEventPublisher eventPublisher)
        {
            this.eventPublisher = eventPublisher;
        }

        public void Handle(StarlightCommand command)
        {
            this.spellStarlite(command.Coord);
        }

        // Light line in all directions -RAK-
        private void spellStarlite(Coord_t coord)
        {
            var py = State.Instance.py;
            if (py.flags.blind < 1)
            {
                Ui_io_m.printMessage("The end of the staff bursts into a blue shimmering light.");
            }

            for (var dir = 1; dir <= 9; dir++)
            {
                if (dir != 5)
                {
                    this.eventPublisher.Publish(new LightLineCommand(coord, dir));
                    //Spells_m.spellLightLine(coord, dir);
                }
            }
        }
    }
}