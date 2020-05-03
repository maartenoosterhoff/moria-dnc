using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class SleepMonsterCommandHandler :
        ICommandHandler<SleepMonsterCommand>,
        ICommandHandler<SleepMonsterCommand, bool>
    {
        private readonly IHelpers helpers;
        private readonly IRnd rnd;

        public SleepMonsterCommandHandler(
            IHelpers helpers,
            IRnd rnd
        )
        {
            this.helpers = helpers;
            this.rnd = rnd;
        }

        void ICommandHandler<SleepMonsterCommand>.Handle(SleepMonsterCommand command)
        {
            this.spellSleepMonster(
                command.Coord,
                command.Direction
            );
        }

        bool ICommandHandler<SleepMonsterCommand, bool>.Handle(SleepMonsterCommand command)
        {
            return this.spellSleepMonster(
                command.Coord,
                command.Direction
            );
        }

        // Sleep a creature. -RAK-
        private bool spellSleepMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var asleep = false;
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

                    if (this.rnd.randomNumber(Monster_c.MON_MAX_LEVELS) < creature.level || (creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                    {
                        if (monster.lit && (creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_NO_SLEEP;
                        }

                        Monster_m.printMonsterActionText(name, "is unaffected.");
                    }
                    else
                    {
                        monster.sleep_count = 500;

                        asleep = true;

                        Monster_m.printMonsterActionText(name, "falls asleep.");
                    }
                }
            }

            return asleep;
        }
    }
}