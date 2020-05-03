using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class PolymorphMonsterCommandHandler :
        ICommandHandler<PolymorphMonsterCommand>,
        ICommandHandler<PolymorphMonsterCommand, bool>
    {
        private readonly IDungeon dungeon;
        private readonly IHelpers helpers;
        private readonly IMonsterManager monsterManager;
        private readonly IRnd rnd;

        public PolymorphMonsterCommandHandler(
            IDungeon dungeon,
            IHelpers helpers,
            IMonsterManager monsterManager,
            IRnd rnd
        )
        {
            this.dungeon = dungeon;
            this.helpers = helpers;
            this.monsterManager = monsterManager;
            this.rnd = rnd;
        }

        void ICommandHandler<PolymorphMonsterCommand>.Handle(PolymorphMonsterCommand command)
        {
            throw new System.NotImplementedException();
        }

        bool ICommandHandler<PolymorphMonsterCommand, bool>.Handle(PolymorphMonsterCommand command)
        {
            throw new System.NotImplementedException();
        }

        // Polymorph a monster -RAK-
        // NOTE: cannot polymorph a winning creature (BALROG)
        private bool spellPolymorphMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var morphed = false;
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
                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if (this.rnd.randomNumber(Monster_c.MON_MAX_LEVELS) > creature.level)
                    {
                        finished = true;

                        this.dungeon.dungeonDeleteMonster((int)tile.creature_id);

                        // Place_monster() should always return true here.
                        morphed = this.monsterManager.monsterPlaceNew(coord, this.rnd.randomNumber(State.Instance.monster_levels[Monster_c.MON_MAX_LEVELS] - State.Instance.monster_levels[0]) - 1 + State.Instance.monster_levels[0], false);

                        // don't test tile.field_mark here, only permanent_light/temporary_light
                        if (morphed && Ui_m.coordInsidePanel(coord) && (tile.temporary_light || tile.permanent_light))
                        {
                            morphed = true;
                        }
                    }
                    else
                    {
                        var name = Monster_m.monsterNameDescription(creature.name, monster.lit);
                        Monster_m.printMonsterActionText(name, "is unaffected.");
                    }
                }
            }

            return morphed;
        }
    }
}