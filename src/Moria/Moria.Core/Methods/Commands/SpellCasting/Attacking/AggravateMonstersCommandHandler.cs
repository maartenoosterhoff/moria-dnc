using Moria.Core.Configs;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class AggravateMonstersCommandHandler :
        ICommandHandler<AggravateMonstersCommand>,
        ICommandHandler<AggravateMonstersCommand, bool>
    {
        void ICommandHandler<AggravateMonstersCommand>.Handle(AggravateMonstersCommand command)
        {
            this.spellAggravateMonsters(
                command.AffectDistance
            );
        }

        bool ICommandHandler<AggravateMonstersCommand, bool>.Handle(AggravateMonstersCommand command)
        {
            return this.spellAggravateMonsters(
                command.AffectDistance
            );
        }

        // Get all the monsters on the level pissed off. -RAK-
        private bool spellAggravateMonsters(int affect_distance)
        {
            var aggravated = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                monster.sleep_count = 0;

                if (monster.distance_from_player <= affect_distance && monster.speed < 2)
                {
                    monster.speed++;
                    aggravated = true;
                }
            }

            if (aggravated)
            {
                Ui_io_m.printMessage("You hear a sudden stirring in the distance!");
            }

            return aggravated;
        }
    }
}