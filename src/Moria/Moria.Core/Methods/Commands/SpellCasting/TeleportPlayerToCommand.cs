using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class TeleportPlayerToCommand : ICommand
    {
        public TeleportPlayerToCommand(Coord_t coord)
        {
            this.Coord = coord;
        }

        public Coord_t Coord { get; }
    }

    public class TeleportPlayerToCommandHandler : ICommandHandler<TeleportPlayerToCommand>
    {
        private readonly IDungeon dungeon;
        private readonly IRnd rnd;

        public TeleportPlayerToCommandHandler(
            IDungeon dungeon,
            IRnd rnd
        )
        {
            this.dungeon = dungeon;
            this.rnd = rnd;
        }
        public void Handle(TeleportPlayerToCommand command)
        {
            this.spellTeleportPlayerTo(command.Coord);
        }

        // Teleport player to spell casting creature -RAK-
        private void spellTeleportPlayerTo(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            var distance = 1;
            var counter = 0;

            var rnd_coord = new Coord_t(0, 0);

            do
            {
                rnd_coord.y = coord.y + (this.rnd.randomNumber(2 * distance + 1) - (distance + 1));
                rnd_coord.x = coord.x + (this.rnd.randomNumber(2 * distance + 1) - (distance + 1));
                counter++;
                if (counter > 9)
                {
                    counter = 0;
                    distance++;
                }
            } while (!this.dungeon.coordInBounds(rnd_coord) || dg.floor[rnd_coord.y][rnd_coord.x].feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE || dg.floor[rnd_coord.y][rnd_coord.x].creature_id >= 2);

            this.dungeon.dungeonMoveCreatureRecord(py.pos, rnd_coord);

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

            //py.pos.y = rnd_coord.y;
            //py.pos.x = rnd_coord.x;
            py.pos = rnd_coord;

            Ui_m.dungeonResetView();

            // light creatures
            Monster_m.updateMonsters(false);
        }
    }
}
