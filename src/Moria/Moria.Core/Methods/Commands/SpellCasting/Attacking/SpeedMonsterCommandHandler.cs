using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class SpeedMonsterCommandHandler : 
        ICommandHandler<SpeedMonsterCommand>,
        ICommandHandler<SpeedMonsterCommand, bool>
    {
        private readonly IHelpers helpers;
        private readonly IRnd rnd;

        public SpeedMonsterCommandHandler(
            IHelpers helpers,
            IRnd rnd
        )
        {
            this.helpers = helpers;
            this.rnd = rnd;
        }

        void ICommandHandler<SpeedMonsterCommand>.Handle(SpeedMonsterCommand command)
        {
            throw new System.NotImplementedException();
        }

        bool ICommandHandler<SpeedMonsterCommand, bool>.Handle(SpeedMonsterCommand command)
        {
            throw new System.NotImplementedException();
        }

        // Increase or decrease a creatures speed -RAK-
        // NOTE: cannot slow a winning creature (BALROG)
        private bool spellSpeedMonster(Coord_t coord, int direction, int speed)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var changed = false;
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
                    finished = true;

                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

                    if (speed > 0)
                    {
                        monster.speed += speed;
                        monster.sleep_count = 0;

                        changed = true;

                        Monster_m.printMonsterActionText(name, "starts moving faster.");
                    }
                    else if (this.rnd.randomNumber(Monster_c.MON_MAX_LEVELS) > creature.level)
                    {
                        monster.speed += speed;
                        monster.sleep_count = 0;

                        changed = true;

                        Monster_m.printMonsterActionText(name, "starts moving slower.");
                    }
                    else
                    {
                        monster.sleep_count = 0;

                        Monster_m.printMonsterActionText(name, "is unaffected.");
                    }
                }
            }

            return changed;
        }
    }
}