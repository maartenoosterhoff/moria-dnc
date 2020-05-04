using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class MapCurrentAreaCommandHandler : ICommandHandler<MapCurrentAreaCommand>
    {
        private readonly IDungeon dungeon;
        private readonly IRnd rnd;

        public MapCurrentAreaCommandHandler(
            IDungeon dungeon,
            IRnd rnd
        )
        {
            this.dungeon = dungeon;
            this.rnd = rnd;
        }

        public void Handle(MapCurrentAreaCommand command)
        {
            this.spellMapCurrentArea();
        }

        // Map the current area plus some -RAK-
        private void spellMapCurrentArea()
        {
            var dg = State.Instance.dg;

            var row_min = dg.panel.top - this.rnd.randomNumber(10);
            var row_max = dg.panel.bottom + this.rnd.randomNumber(10);
            var col_min = dg.panel.left - this.rnd.randomNumber(20);
            var col_max = dg.panel.right + this.rnd.randomNumber(20);

            var coord = new Coord_t(0, 0);

            for (coord.y = row_min; coord.y <= row_max; coord.y++)
            {
                for (coord.x = col_min; coord.x <= col_max; coord.x++)
                {
                    if (this.dungeon.coordInBounds(coord) && dg.floor[coord.y][coord.x].feature_id <= Dungeon_tile_c.MAX_CAVE_FLOOR)
                    {
                        this.dungeonLightAreaAroundFloorTile(coord);
                    }
                }
            }

            Ui_m.drawDungeonPanel();
        }

        private void dungeonLightAreaAroundFloorTile(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var spot = new Coord_t(0, 0);

            for (spot.y = coord.y - 1; spot.y <= coord.y + 1; spot.y++)
            {
                for (spot.x = coord.x - 1; spot.x <= coord.x + 1; spot.x++)
                {
                    var tile = dg.floor[spot.y][spot.x];

                    if (tile.feature_id >= Dungeon_tile_c.MIN_CAVE_WALL)
                    {
                        tile.permanent_light = true;
                    }
                    else if (tile.treasure_id != 0 && game.treasure.list[tile.treasure_id].category_id >= Treasure_c.TV_MIN_VISIBLE &&
                             game.treasure.list[tile.treasure_id].category_id <= Treasure_c.TV_MAX_VISIBLE)
                    {
                        tile.field_mark = true;
                    }
                }
            }
        }
    }
}