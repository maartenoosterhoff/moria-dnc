using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class SpeedAllMonstersCommandHandler :
        ICommandHandler<SpeedAllMonstersCommand>,
        ICommandHandler<SpeedAllMonstersCommand, bool>
    {
        private readonly IDungeonLos dungeonLos;
        private readonly IRnd rnd;

        public SpeedAllMonstersCommandHandler(
            IDungeonLos dungeonLos,
            IRnd rnd
        )
        {
            this.dungeonLos = dungeonLos;
            this.rnd = rnd;
        }

        void ICommandHandler<SpeedAllMonstersCommand>.Handle(SpeedAllMonstersCommand command)
        {
            this.spellSpeedAllMonsters(command.Speed);
        }

        bool ICommandHandler<SpeedAllMonstersCommand, bool>.Handle(SpeedAllMonstersCommand command)
        {
            return this.spellSpeedAllMonsters(command.Speed);
        }

        // Change speed of any creature . -RAK-
        // NOTE: cannot slow a winning creature (BALROG)
        private bool spellSpeedAllMonsters(int speed)
        {
            var py = State.Instance.py;

            var speedy = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

                if (monster.distance_from_player > Config.monsters.MON_MAX_SIGHT || !this.dungeonLos.los(py.pos, monster.pos))
                {
                    continue; // do nothing
                }

                if (speed > 0)
                {
                    monster.speed += speed;
                    monster.sleep_count = 0;

                    if (monster.lit)
                    {
                        speedy = true;
                        Monster_m.printMonsterActionText(name, "starts moving faster.");
                    }
                }
                else if (this.rnd.randomNumber(Monster_c.MON_MAX_LEVELS) > creature.level)
                {
                    monster.speed += speed;
                    monster.sleep_count = 0;

                    if (monster.lit)
                    {
                        speedy = true;
                        Monster_m.printMonsterActionText(name, "starts moving slower.");
                    }
                }
                else if (monster.lit)
                {
                    monster.sleep_count = 0;
                    Monster_m.printMonsterActionText(name, "is unaffected.");
                }
            }

            return speedy;
        }
    }
}