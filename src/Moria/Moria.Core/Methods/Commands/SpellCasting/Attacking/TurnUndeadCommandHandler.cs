using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class TurnUndeadCommandHandler :
        ICommandHandler<TurnUndeadCommand>,
        ICommandHandler<TurnUndeadCommand, bool>
    {
        private readonly IDungeonLos dungeonLos;
        private readonly IMonster monster;
        private readonly IRnd rnd;

        public TurnUndeadCommandHandler(
            IDungeonLos dungeonLos,
            IMonster monster,
            IRnd rnd
        )
        {
            this.dungeonLos = dungeonLos;
            this.monster = monster;
            this.rnd = rnd;
        }

        void ICommandHandler<TurnUndeadCommand>.Handle(TurnUndeadCommand command)
        {
            this.spellTurnUndead();
        }

        bool ICommandHandler<TurnUndeadCommand, bool>.Handle(TurnUndeadCommand command)
        {
            return this.spellTurnUndead();
        }

        // Attempt to turn (confuse) undead creatures. -RAK-
        private bool spellTurnUndead()
        {
            var py = State.Instance.py;
            var turned = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                if (monster.distance_from_player <= Config.monsters.MON_MAX_SIGHT && (creature.defenses & Config.monsters_defense.CD_UNDEAD) != 0 && this.dungeonLos.los(py.pos, monster.pos))
                {
                    var name = this.monster.monsterNameDescription(creature.name, monster.lit);

                    if (py.misc.level + 1 > creature.level || this.rnd.randomNumber(5) == 1)
                    {
                        if (monster.lit)
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_UNDEAD;

                            turned = true;

                            this.monster.printMonsterActionText(name, "runs frantically!");
                        }

                        monster.confused_amount = (uint)py.misc.level;
                    }
                    else if (monster.lit)
                    {
                        this.monster.printMonsterActionText(name, "is unaffected.");
                    }
                }
            }

            return turned;
        }
    }
}