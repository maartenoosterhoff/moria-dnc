using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Monster;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class ChangeMonsterHitPointsCommandHandler :
        ICommandHandler<ChangeMonsterHitPointsCommand>,
        ICommandHandler<ChangeMonsterHitPointsCommand, bool>
    {
        private readonly IEventPublisher eventPublisher;
        private readonly IHelpers helpers;

        public ChangeMonsterHitPointsCommandHandler(
            IEventPublisher eventPublisher,
            IHelpers helpers
        )
        {
            this.eventPublisher = eventPublisher;
            this.helpers = helpers;
        }

        void ICommandHandler<ChangeMonsterHitPointsCommand>.Handle(ChangeMonsterHitPointsCommand command)
        {
            this.spellChangeMonsterHitPoints(
                command.Coord,
                command.Direction,
                command.DamageHp
            );
        }

        bool ICommandHandler<ChangeMonsterHitPointsCommand, bool>.Handle(ChangeMonsterHitPointsCommand command)
        {
            return this.spellChangeMonsterHitPoints(
                command.Coord,
                command.Direction,
                command.DamageHp
            );
        }

        // Increase or decrease a creatures hit points -RAK-
        private bool spellChangeMonsterHitPoints(Coord_t coord, int direction, int damage_hp)
        {
            var dg = State.Instance.dg;

            var distance = 0;
            var changed = false;
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

                    var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

                    var creature_id = this.eventPublisher.PublishWithOutputInt(
                        new TakeHitCommand((int) tile.creature_id, damage_hp)
                    );
                    if (creature_id >= 0)
                    //if (Monster_m.monsterTakeHit((int)tile.creature_id, damage_hp) >= 0)
                    {
                        Monster_m.printMonsterActionText(name, "dies in a fit of agony.");
                        Ui_m.displayCharacterExperience();
                    }
                    else if (damage_hp > 0)
                    {
                        Monster_m.printMonsterActionText(name, "screams in agony.");
                    }

                    changed = true;
                }
            }

            return changed;
        }
    }
}