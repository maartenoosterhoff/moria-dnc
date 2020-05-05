using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Monster;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class DrainLifeFromMonsterCommandHandler :
        ICommandHandler<DrainLifeFromMonsterCommand>,
        ICommandHandler<DrainLifeFromMonsterCommand, bool>
    {
        private readonly IEventPublisher eventPublisher;
        private readonly IHelpers helpers;

        public DrainLifeFromMonsterCommandHandler(
            IEventPublisher eventPublisher,
            IHelpers helpers
        )
        {
            this.eventPublisher = eventPublisher;
            this.helpers = helpers;
        }

        void ICommandHandler<DrainLifeFromMonsterCommand>.Handle(DrainLifeFromMonsterCommand command)
        {
            this.spellDrainLifeFromMonster(
                command.Coord,
                command.Direction
            );
        }

        bool ICommandHandler<DrainLifeFromMonsterCommand, bool>.Handle(DrainLifeFromMonsterCommand command)
        {
            return this.spellDrainLifeFromMonster(
                command.Coord,
                command.Direction
            );
        }

        // Drains life; note it must be living. -RAK-
        private bool spellDrainLifeFromMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;

            var distance = 0;
            var drained = false;
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

                    if ((creature.defenses & Config.monsters_defense.CD_UNDEAD) == 0)
                    {
                        var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

                        var creature_id = this.eventPublisher.PublishWithOutputInt(
                            new TakeHitCommand((int)tile.creature_id, 75)
                        );
                        if (creature_id >= 0)
                        //if (Monster_m.monsterTakeHit((int)tile.creature_id, 75) >= 0)
                        {
                            Monster_m.printMonsterActionText(name, "dies in a fit of agony.");
                            Ui_m.displayCharacterExperience();
                        }
                        else
                        {
                            Monster_m.printMonsterActionText(name, "screams in agony.");
                        }

                        drained = true;
                    }
                    else
                    {
                        State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_UNDEAD;
                    }
                }
            }

            return drained;
        }
    }
}