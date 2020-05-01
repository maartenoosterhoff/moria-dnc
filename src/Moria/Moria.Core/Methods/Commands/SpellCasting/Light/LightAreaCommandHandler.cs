using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Light
{
    public class LightAreaCommandHandler :
        ICommandHandler<LightAreaCommand>,
        ICommandHandler<LightAreaCommand, bool>
    {
        private readonly IDungeon dungeon;

        public LightAreaCommandHandler(IDungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        void ICommandHandler<LightAreaCommand>.Handle(LightAreaCommand command)
        {
            this.spellLightArea(command.Coord);
        }

        bool ICommandHandler<LightAreaCommand, bool>.Handle(LightAreaCommand command)
        {
            return this.spellLightArea(command.Coord);
        }

        // Light an area: -RAK-
        //     1.  If corridor  light immediate area
        //     2.  If room      light entire room plus immediate area.
        private bool spellLightArea(Coord_t coord)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            if (py.flags.blind < 1)
            {
                Ui_io_m.printMessage("You are surrounded by a white light.");
            }

            // NOTE: this is not changed anywhere. A bug or correct? -MRC-
            var lit = true;

            if (dg.floor[coord.y][coord.x].perma_lit_room && dg.current_level > 0)
            {
                this.dungeon.dungeonLightRoom(coord);
            }

            // Must always light immediate area, because one might be standing on
            // the edge of a room, or next to a destroyed area, etc.
            var spot = new Coord_t(0, 0);
            for (spot.y = coord.y - 1; spot.y <= coord.y + 1; spot.y++)
            {
                for (spot.x = coord.x - 1; spot.x <= coord.x + 1; spot.x++)
                {
                    dg.floor[spot.y][spot.x].permanent_light = true;
                    this.dungeon.dungeonLiteSpot(spot);
                }
            }

            return lit;
        }
    }
}