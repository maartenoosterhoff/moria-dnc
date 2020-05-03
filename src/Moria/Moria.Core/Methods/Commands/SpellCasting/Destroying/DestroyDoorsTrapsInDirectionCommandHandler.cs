using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting.Destroying
{
    public class DestroyDoorsTrapsInDirectionCommandHandler :
        ICommandHandler<DestroyDoorsTrapsInDirectionCommand>,
        ICommandHandler<DestroyDoorsTrapsInDirectionCommand, bool>
    {
        private readonly IDungeon dungeon;
        private readonly IHelpers helpers;

        public DestroyDoorsTrapsInDirectionCommandHandler(
            IDungeon dungeon,
            IHelpers helpers
        )
        {
            this.dungeon = dungeon;
            this.helpers = helpers;
        }

        void ICommandHandler<DestroyDoorsTrapsInDirectionCommand>.Handle(DestroyDoorsTrapsInDirectionCommand command)
        {
            this.spellDestroyDoorsTrapsInDirection(
                command.Coord,
                command.Direction
            );
        }

        bool ICommandHandler<DestroyDoorsTrapsInDirectionCommand, bool>.Handle(DestroyDoorsTrapsInDirectionCommand command)
        {
            return this.spellDestroyDoorsTrapsInDirection(
                command.Coord,
                command.Direction
            );
        }

        // Destroy all traps and doors in a given direction -RAK-
        private bool spellDestroyDoorsTrapsInDirection(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var destroyed = false;
            var distance = 0;

            Tile_t tile;

            do
            {
                this.helpers.movePosition(direction, ref coord);
                distance++;

                tile = dg.floor[coord.y][coord.x];

                // must move into first closed spot, as it might be a secret door
                if (tile.treasure_id != 0)
                {
                    var item = game.treasure.list[tile.treasure_id];

                    if (item.category_id == Treasure_c.TV_INVIS_TRAP ||
                        item.category_id == Treasure_c.TV_CLOSED_DOOR ||
                        item.category_id == Treasure_c.TV_VIS_TRAP ||
                        item.category_id == Treasure_c.TV_OPEN_DOOR ||
                        item.category_id == Treasure_c.TV_SECRET_DOOR
                    )
                    {
                        if (this.dungeon.dungeonDeleteObject(coord))
                        {
                            destroyed = true;
                            Ui_io_m.printMessage("There is a bright flash of light!");
                        }
                    }
                    else if (item.category_id == Treasure_c.TV_CHEST && item.flags != 0)
                    {
                        destroyed = true;
                        Ui_io_m.printMessage("Click!");

                        item.flags &= ~(Config.treasure_chests.CH_TRAPPED | Config.treasure_chests.CH_LOCKED);
                        item.special_name_id = (int)SpecialNameIds.SN_UNLOCKED;

                        Identification_m.spellItemIdentifyAndRemoveRandomInscription(item);
                    }
                }
            } while (distance <= Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id <= Dungeon_tile_c.MAX_OPEN_SPACE);

            return destroyed;
        }
    }
}