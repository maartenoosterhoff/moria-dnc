using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Defending
{
    public class TeleportAwayMonsterInDirectionCommandHandler :
        ICommandHandler<TeleportAwayMonsterInDirectionCommand>,
        ICommandHandler<TeleportAwayMonsterInDirectionCommand, bool>
    {
        private readonly IEventPublisher eventPublisher;
        private readonly IHelpers helpers;

        public TeleportAwayMonsterInDirectionCommandHandler(
            IEventPublisher eventPublisher,
            IHelpers helpers
        )
        {
            this.eventPublisher = eventPublisher;
            this.helpers = helpers;
        }

        void ICommandHandler<TeleportAwayMonsterInDirectionCommand>.Handle(TeleportAwayMonsterInDirectionCommand command)
        {
            this.spellTeleportAwayMonsterInDirection(
                command.Coord,
                command.Direction
            );
        }

        bool ICommandHandler<TeleportAwayMonsterInDirectionCommand, bool>.Handle(TeleportAwayMonsterInDirectionCommand command)
        {
            return this.spellTeleportAwayMonsterInDirection(
                command.Coord,
                command.Direction
            );
        }

        // Teleport all creatures in a given direction away -RAK-
        private bool spellTeleportAwayMonsterInDirection(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;

            var distance = 0;
            var teleported = false;
            var finished = false;

            while (!finished)
            {
                this.helpers.movePosition(direction, ref coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue;
                }

                if (tile.creature_id > 1)
                {
                    // wake it up
                    State.Instance.monsters[tile.creature_id].sleep_count = 0;

                    this.eventPublisher.Publish(new TeleportAwayMonsterCommand((int)tile.creature_id, (int)Config.monsters.MON_MAX_SIGHT));
                    //spellTeleportAwayMonster((int)tile.creature_id, (int)Config.monsters.MON_MAX_SIGHT);

                    teleported = true;
                }
            }

            return teleported;
        }
    }
}