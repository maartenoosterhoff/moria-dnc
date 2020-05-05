using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class TeleportCommandHandler : ICommandHandler<TeleportCommand>
    {
        private readonly IDungeon dungeon;
        private readonly IRnd rnd;
        private readonly ITerminalEx terminalEx;

        public TeleportCommandHandler(
            IDungeon dungeon,
            IRnd rnd,
            ITerminalEx terminalEx
        )
        {
            this.dungeon = dungeon;
            this.rnd = rnd;
            this.terminalEx = terminalEx;
        }

        public void Handle(TeleportCommand command)
        {
            this.playerTeleport(command.NewDistance);
        }

        // Teleport the player to a new location -RAK-
        private void playerTeleport(int new_distance)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            var location = new Coord_t(0, 0);

            do
            {
                location.y = this.rnd.randomNumber(dg.height) - 1;
                location.x = this.rnd.randomNumber(dg.width) - 1;

                while (this.dungeon.coordDistanceBetween(location, py.pos) > new_distance)
                {
                    location.y += (py.pos.y - location.y) / 2;
                    location.x += (py.pos.x - location.x) / 2;
                }
            } while (dg.floor[location.y][location.x].feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE || dg.floor[location.y][location.x].creature_id >= 2);

            this.dungeon.dungeonMoveCreatureRecord(py.pos, location);

            var spot = new Coord_t(0, 0);
            for (spot.y = py.pos.y - 1; spot.y <= py.pos.y + 1; spot.y++)
            {
                for (spot.x = py.pos.x - 1; spot.x <= py.pos.x + 1; spot.x++)
                {
                    dg.floor[spot.y][spot.x].temporary_light = false;
                    this.dungeon.dungeonLiteSpot(spot);
                }
            }

            this.dungeon.dungeonLiteSpot(py.pos);

            //py.pos.y = location.y;
            //py.pos.x = location.x;
            py.pos = location;

            this.terminalEx.dungeonResetView();
            Monster_m.updateMonsters(false);

            State.Instance.game.teleport_player = false;
        }
    }
}