using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class ConfuseMonsterCommandHandler :
        ICommandHandler<ConfuseMonsterCommand>,
        ICommandHandler<ConfuseMonsterCommand, bool>
    {
        private readonly IHelpers helpers;
        private readonly IMonster monster;
        private readonly IRnd rnd;

        public ConfuseMonsterCommandHandler(
            IHelpers helpers,
            IMonster monster,
            IRnd rnd
        )
        {
            this.helpers = helpers;
            this.monster = monster;
            this.rnd = rnd;
        }

        void ICommandHandler<ConfuseMonsterCommand>.Handle(ConfuseMonsterCommand command)
        {
            this.spellConfuseMonster(
                command.Coord,
                command.Direction
            );
        }

        bool ICommandHandler<ConfuseMonsterCommand, bool>.Handle(ConfuseMonsterCommand command)
        {
            return this.spellConfuseMonster(
                command.Coord,
                command.Direction
            );
        }

        // Confuse a creature -RAK-
        private bool spellConfuseMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var confused = false;
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

                    var name = this.monster.monsterNameDescription(creature.name, monster.lit);

                    if (this.rnd.randomNumber(Monster_c.MON_MAX_LEVELS) < creature.level || (creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                    {
                        if (monster.lit && (creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_NO_SLEEP;
                        }

                        // Monsters which resisted the attack should wake up.
                        // Monsters with innate resistance ignore the attack.
                        if ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) == 0)
                        {
                            monster.sleep_count = 0;
                        }

                        this.monster.printMonsterActionText(name, "is unaffected.");
                    }
                    else
                    {
                        if (monster.confused_amount != 0u)
                        {
                            monster.confused_amount += 3;
                        }
                        else
                        {
                            monster.confused_amount = (uint)(2 + this.rnd.randomNumber(16));
                        }
                        monster.sleep_count = 0;

                        confused = true;

                        this.monster.printMonsterActionText(name, "appears confused.");
                    }
                }
            }

            return confused;
        }
    }
}