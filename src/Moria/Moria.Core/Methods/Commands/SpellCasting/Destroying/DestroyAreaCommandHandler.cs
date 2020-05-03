using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Destroying
{
    public class DestroyAreaCommandHandler : ICommandHandler<DestroyAreaCommand>
    {
        private readonly IDungeon dungeon;
        private readonly IRnd rnd;

        public DestroyAreaCommandHandler(
            IDungeon dungeon,
            IRnd rnd
        )
        {
            this.dungeon = dungeon;
            this.rnd = rnd;
        }

        public void Handle(DestroyAreaCommand command)
        {
            this.spellDestroyArea(command.Coord);
        }

        // The spell of destruction. -RAK-
        // NOTE:
        //   Winning creatures that are deleted will be considered as teleporting to another level.
        //   This will NOT win the game.
        private void spellDestroyArea(Coord_t coord)
        {
            var dg = State.Instance.dg;

            if (dg.current_level > 0)
            {
                var spot = new Coord_t(0, 0);

                for (spot.y = coord.y - 15; spot.y <= coord.y + 15; spot.y++)
                {
                    for (spot.x = coord.x - 15; spot.x <= coord.x + 15; spot.x++)
                    {
                        if (this.dungeon.coordInBounds(spot) && dg.floor[spot.y][spot.x].feature_id != Dungeon_tile_c.TILE_BOUNDARY_WALL)
                        {
                            var distance = this.dungeon.coordDistanceBetween(spot, coord);

                            // clear player's spot, but don't put wall there
                            if (distance == 0)
                            {
                                this.replaceSpot(spot, 1);
                            }
                            else if (distance < 13)
                            {
                                this.replaceSpot(spot, this.rnd.randomNumber(6));
                            }
                            else if (distance < 16)
                            {
                                this.replaceSpot(spot, this.rnd.randomNumber(9));
                            }
                        }
                    }
                }
            }

            Ui_io_m.printMessage("There is a searing blast of light!");
            State.Instance.py.flags.blind += 10 + this.rnd.randomNumber(10);
        }

        private void replaceSpot(Coord_t coord, int typ)
        {
            var dg = State.Instance.dg;

            var tile = dg.floor[coord.y][coord.x];

            switch (typ)
            {
                case 1:
                case 2:
                case 3:
                    tile.feature_id = Dungeon_tile_c.TILE_CORR_FLOOR;
                    break;
                case 4:
                case 7:
                case 10:
                    tile.feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                    break;
                case 5:
                case 8:
                case 11:
                    tile.feature_id = Dungeon_tile_c.TILE_MAGMA_WALL;
                    break;
                case 6:
                case 9:
                case 12:
                    tile.feature_id = Dungeon_tile_c.TILE_QUARTZ_WALL;
                    break;
                default:
                    break;
            }

            tile.permanent_light = false;
            tile.field_mark = false;
            tile.perma_lit_room = false; // this is no longer part of a room

            if (tile.treasure_id != 0)
            {
                this.dungeon.dungeonDeleteObject(coord);
            }

            if (tile.creature_id > 1)
            {
                this.dungeon.dungeonDeleteMonster((int)tile.creature_id);
            }
        }
    }
}