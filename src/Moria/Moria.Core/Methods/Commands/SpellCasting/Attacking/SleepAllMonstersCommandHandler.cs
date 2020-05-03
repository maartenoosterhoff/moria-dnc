using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class SleepAllMonstersCommandHandler :
        ICommandHandler<SleepAllMonstersCommand>,
        ICommandHandler<SleepAllMonstersCommand, bool>
    {
        private readonly IDungeonLos dungeonLos;
        private readonly IRnd rnd;

        public SleepAllMonstersCommandHandler(
            IDungeonLos dungeonLos,
            IRnd rnd
        )
        {
            this.dungeonLos = dungeonLos;
            this.rnd = rnd;
        }

        void ICommandHandler<SleepAllMonstersCommand>.Handle(SleepAllMonstersCommand command)
        {
            this.spellSleepAllMonsters();
        }

        bool ICommandHandler<SleepAllMonstersCommand, bool>.Handle(SleepAllMonstersCommand command)
        {
            return this.spellSleepAllMonsters();
        }

        // Sleep any creature . -RAK-
        private bool spellSleepAllMonsters()
        {
            var py = State.Instance.py;

            var asleep = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

                if (monster.distance_from_player > Config.monsters.MON_MAX_SIGHT || !this.dungeonLos.los(py.pos, monster.pos))
                {
                    continue; // do nothing
                }

                if (this.rnd.randomNumber(Monster_c.MON_MAX_LEVELS) < creature.level || (creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                {
                    if (monster.lit)
                    {
                        if ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_NO_SLEEP;
                        }
                        Monster_m.printMonsterActionText(name, "is unaffected.");
                    }
                }
                else
                {
                    monster.sleep_count = 500;
                    if (monster.lit)
                    {
                        asleep = true;
                        Monster_m.printMonsterActionText(name, "falls asleep.");
                    }
                }
            }

            return asleep;
        }
    }
}