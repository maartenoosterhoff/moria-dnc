using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class DisarmAllInDirectionCommandHandler :
        ICommandHandler<DisarmAllInDirectionCommand>,
        ICommandHandler<DisarmAllInDirectionCommand, bool>
    {
        private readonly IDungeon dungeon;
        private readonly IHelpers helpers;
        private readonly ITerminal terminal;

        public DisarmAllInDirectionCommandHandler(
            IDungeon dungeon,
            IHelpers helpers,
            ITerminal terminal
        )
        {
            this.dungeon = dungeon;
            this.helpers = helpers;
            this.terminal = terminal;
        }

        void ICommandHandler<DisarmAllInDirectionCommand>.Handle(DisarmAllInDirectionCommand command)
        {
            this.spellDisarmAllInDirection(
                command.Coord,
                command.Direction
            );
        }

        bool ICommandHandler<DisarmAllInDirectionCommand, bool>.Handle(DisarmAllInDirectionCommand command)
        {
            return this.spellDisarmAllInDirection(
                command.Coord,
                command.Direction
            );
        }

        // Disarms all traps/chests in a given direction -RAK-
        private bool spellDisarmAllInDirection(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var distance = 0;
            var disarmed = false;

            Tile_t tile = null;

            do
            {
                tile = dg.floor[coord.y][coord.x];

                // note, must continue up to and including the first non open space,
                // because secret doors have feature_id greater than MAX_OPEN_SPACE
                if (tile.treasure_id != 0)
                {
                    var item = game.treasure.list[tile.treasure_id];

                    if (item.category_id == Treasure_c.TV_INVIS_TRAP || item.category_id == Treasure_c.TV_VIS_TRAP)
                    {
                        if (dungeon.dungeonDeleteObject(coord))
                        {
                            disarmed = true;
                        }
                    }
                    else if (item.category_id == Treasure_c.TV_CLOSED_DOOR)
                    {
                        // Locked or jammed doors become merely closed.
                        item.misc_use = 0;
                    }
                    else if (item.category_id == Treasure_c.TV_SECRET_DOOR)
                    {
                        tile.field_mark = true;
                        dungeon.trapChangeVisibility(coord);
                        disarmed = true;
                    }
                    else if (item.category_id == Treasure_c.TV_CHEST && item.flags != 0)
                    {
                        disarmed = true;
                        this.terminal.printMessage("Click!");

                        item.flags &= ~(Config.treasure_chests.CH_TRAPPED | Config.treasure_chests.CH_LOCKED);
                        item.special_name_id = (int)SpecialNameIds.SN_UNLOCKED;

                        Identification_m.spellItemIdentifyAndRemoveRandomInscription(item);
                    }
                }

                // move must be at end because want to light up current spot
                helpers.movePosition(direction, ref coord);

                distance++;
            } while (distance <= Config.treasure.OBJECT_BOLTS_MAX_RANGE && tile.feature_id <= Dungeon_tile_c.MAX_OPEN_SPACE);

            return disarmed;
        }
    }
}