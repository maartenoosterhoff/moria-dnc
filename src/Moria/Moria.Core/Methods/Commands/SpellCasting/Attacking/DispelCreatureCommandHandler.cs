using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class DispelCreatureCommandHandler :
        ICommandHandler<DispelCreatureCommand>,
        ICommandHandler<DispelCreatureCommand, bool>
    {
        private readonly IDungeonLos dungeonLos;
        private readonly IRnd rnd;

        public DispelCreatureCommandHandler(
            IDungeonLos dungeonLos,
            IRnd rnd
        )
        {
            this.dungeonLos = dungeonLos;
            this.rnd = rnd;
        }

        void ICommandHandler<DispelCreatureCommand>.Handle(DispelCreatureCommand command)
        {
            throw new System.NotImplementedException();
        }

        bool ICommandHandler<DispelCreatureCommand, bool>.Handle(DispelCreatureCommand command)
        {
            throw new System.NotImplementedException();
        }

        // Attempts to destroy a type of creature.  Success depends on
        // the creatures level VS. the player's level -RAK-
        private bool spellDispelCreature(int creature_defense, int damage)
        {
            var py = State.Instance.py;
            var creatures_list = Library.Instance.Creatures.creatures_list;
            var dispelled = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (monster.distance_from_player <= Config.monsters.MON_MAX_SIGHT && (creature_defense & creatures_list[(int)monster.creature_id].defenses) != 0 && this.dungeonLos.los(py.pos, monster.pos))
                {
                    var creature = creatures_list[(int)monster.creature_id];

                    State.Instance.creature_recall[monster.creature_id].defenses |= (uint)creature_defense;

                    dispelled = true;

                    var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

                    var hit = Monster_m.monsterTakeHit(id, this.rnd.randomNumber(damage));

                    // Should get these messages even if the monster is not visible.
                    if (hit >= 0)
                    {
                        Monster_m.printMonsterActionText(name, "dissolves!");
                    }
                    else
                    {
                        Monster_m.printMonsterActionText(name, "shudders.");
                    }

                    if (hit >= 0)
                    {
                        Ui_m.displayCharacterExperience();
                    }
                }
            }

            return dispelled;
        }
    }
}