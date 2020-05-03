using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class MassPolymorphCommandHandler :
        ICommandHandler<MassPolymorphCommand>,
        ICommandHandler<MassPolymorphCommand, bool>
    {
        private readonly IDungeon dungeon;
        private readonly IMonsterManager monsterManager;
        private readonly IRnd rnd;

        public MassPolymorphCommandHandler(
            IDungeon dungeon,
            IMonsterManager monsterManager,
            IRnd rnd
        )
        {
            this.dungeon = dungeon;
            this.monsterManager = monsterManager;
            this.rnd = rnd;
        }

        void ICommandHandler<MassPolymorphCommand>.Handle(MassPolymorphCommand command)
        {
            throw new System.NotImplementedException();
        }

        bool ICommandHandler<MassPolymorphCommand, bool>.Handle(MassPolymorphCommand command)
        {
            throw new System.NotImplementedException();
        }

        // Polymorph any creature that player can see. -RAK-
        // NOTE: cannot polymorph a winning creature (BALROG)
        private bool spellMassPolymorph()
        {
            var morphed = false;
            var coord = new Coord_t(0, 0);

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (monster.distance_from_player <= Config.monsters.MON_MAX_SIGHT)
                {
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if ((creature.movement & Config.monsters_move.CM_WIN) == 0)
                    {
                        coord.y = monster.pos.y;
                        coord.x = monster.pos.x;
                        this.dungeon.dungeonDeleteMonster(id);

                        // Place_monster() should always return true here.
                        morphed = this.monsterManager.monsterPlaceNew(coord, this.rnd.randomNumber(State.Instance.monster_levels[Monster_c.MON_MAX_LEVELS] - State.Instance.monster_levels[0]) - 1 + State.Instance.monster_levels[0], false);
                    }
                }
            }

            return morphed;
        }
    }
}