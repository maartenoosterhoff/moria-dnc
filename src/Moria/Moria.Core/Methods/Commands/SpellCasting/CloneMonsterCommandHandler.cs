using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class CloneMonsterCommandHandler :
        ICommandHandler<CloneMonsterCommand>,
        ICommandHandler<CloneMonsterCommand, bool>
    {
        private readonly IHelpers helpers;
        private readonly IMonster monster;

        public CloneMonsterCommandHandler(
            IHelpers helpers,
            IMonster monster
        )
        {
            this.helpers = helpers;
            this.monster = monster;
        }

        void ICommandHandler<CloneMonsterCommand>.Handle(CloneMonsterCommand command)
        {
            this.spellCloneMonster(
                command.Coord,
                command.Direction
            );
        }

        bool ICommandHandler<CloneMonsterCommand, bool>.Handle(CloneMonsterCommand command)
        {
            return this.spellCloneMonster(
                command.Coord,
                command.Direction
            );
        }

        // Replicate a creature -RAK-
        private bool spellCloneMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var finished = false;

            while (!finished)
            {
                this.helpers.movePosition(direction, ref coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE)
                {
                    finished = true;
                }
                else if (tile.creature_id > 1)
                {
                    State.Instance.monsters[tile.creature_id].sleep_count = 0;

                    // monptr of 0 is safe here, since can't reach here from creatures
                    return this.monster.monsterMultiply(coord, (int)State.Instance.monsters[tile.creature_id].creature_id, 0);
                }
            }

            return false;
        }
    }
}