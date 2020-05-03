using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class MassGenocideCommandHandler :
        ICommandHandler<MassGenocideCommand>,
        ICommandHandler<MassGenocideCommand, bool>
    {
        private readonly IDungeon dungeon;

        public MassGenocideCommandHandler(IDungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        void ICommandHandler<MassGenocideCommand>.Handle(MassGenocideCommand command)
        {
            this.spellMassGenocide();
        }

        bool ICommandHandler<MassGenocideCommand, bool>.Handle(MassGenocideCommand command)
        {
            return this.spellMassGenocide();
        }

        // Delete all creatures within max_sight distance -RAK-
        // NOTE : Winning creatures cannot be killed by genocide.
        private bool spellMassGenocide()
        {
            var killed = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                if (monster.distance_from_player <= Config.monsters.MON_MAX_SIGHT && (creature.movement & Config.monsters_move.CM_WIN) == 0)
                {
                    killed = true;
                    this.dungeon.dungeonDeleteMonster(id);
                }
            }

            return killed;
        }
    }
}