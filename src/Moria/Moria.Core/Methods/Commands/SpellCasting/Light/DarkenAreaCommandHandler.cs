using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Light
{
    public class DarkenAreaCommandHandler :
        ICommandHandler<DarkenAreaCommand>,
        ICommandHandler<DarkenAreaCommand, bool>
    {
        private readonly IDungeon dungeon;

        public DarkenAreaCommandHandler(IDungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        void ICommandHandler<DarkenAreaCommand>.Handle(DarkenAreaCommand command)
        {
            this.spellDarkenArea(command.Coord);
        }

        bool ICommandHandler<DarkenAreaCommand, bool>.Handle(DarkenAreaCommand command)
        {
            return this.spellDarkenArea(command.Coord);
        }

        // Darken an area, opposite of light area -RAK-
        private bool spellDarkenArea(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var darkened = false;

            var spot = new Coord_t(0, 0);

            if (dg.floor[coord.y][coord.x].perma_lit_room && dg.current_level > 0)
            {
                const int half_height = (int)(Dungeon_c.SCREEN_HEIGHT / 2);
                const int half_width = (int)(Dungeon_c.SCREEN_WIDTH / 2);
                var start_row = coord.y / half_height * half_height + 1;
                var start_col = coord.x / half_width * half_width + 1;
                var end_row = start_row + half_height - 1;
                var end_col = start_col + half_width - 1;

                for (spot.y = start_row; spot.y <= end_row; spot.y++)
                {
                    for (spot.x = start_col; spot.x <= end_col; spot.x++)
                    {
                        var tile = dg.floor[spot.y][spot.x];

                        if (tile.perma_lit_room && tile.feature_id <= Dungeon_tile_c.MAX_CAVE_FLOOR)
                        {
                            tile.permanent_light = false;
                            tile.feature_id = Dungeon_tile_c.TILE_DARK_FLOOR;

                            this.dungeon.dungeonLiteSpot(spot);

                            if (!this.dungeon.caveTileVisible(spot))
                            {
                                darkened = true;
                            }
                        }
                    }
                }
            }
            else
            {
                for (spot.y = coord.y - 1; spot.y <= coord.y + 1; spot.y++)
                {
                    for (spot.x = coord.x - 1; spot.x <= coord.x + 1; spot.x++)
                    {
                        var tile = dg.floor[spot.y][spot.x];

                        if (tile.feature_id == Dungeon_tile_c.TILE_CORR_FLOOR && tile.permanent_light)
                        {
                            // permanent_light could have been set by star-lite wand, etc
                            tile.permanent_light = false;
                            darkened = true;
                        }
                    }
                }
            }

            if (darkened && py.flags.blind < 1)
            {
                Ui_io_m.printMessage("Darkness surrounds you.");
            }

            return darkened;
        }
    }
}