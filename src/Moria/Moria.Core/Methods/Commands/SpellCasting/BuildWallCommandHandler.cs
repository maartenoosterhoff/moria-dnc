using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Monster;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class BuildWallCommandHandler :
        ICommandHandler<BuildWallCommand>,
        ICommandHandler<BuildWallCommand, bool>
    {
        private readonly IDice dice;
        private readonly IEventPublisher eventPublisher;
        private readonly IDungeon dungeon;
        private readonly IHelpers helpers;

        public BuildWallCommandHandler(
            IDice dice,
            IEventPublisher eventPublisher,
            IDungeon dungeon,
            IHelpers helpers
        )
        {
            this.dice = dice;
            this.eventPublisher = eventPublisher;
            this.dungeon = dungeon;
            this.helpers = helpers;
        }

        void ICommandHandler<BuildWallCommand>.Handle(BuildWallCommand command)
        {
            this.spellBuildWall(
                command.Coord,
                command.Direction
            );
        }

        bool ICommandHandler<BuildWallCommand, bool>.Handle(BuildWallCommand command)
        {
            return this.spellBuildWall(
                command.Coord,
                command.Direction
            );
        }

        // Create a wall. -RAK-
        private bool spellBuildWall(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var built = false;
            var finished = false;

            while (!finished)
            {
                this.helpers.movePosition(direction, ref coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue; // we're done here, break out of the loop
                }

                if (tile.treasure_id != 0)
                {
                    this.dungeon.dungeonDeleteObject(coord);
                }

                if (tile.creature_id > 1)
                {
                    finished = true;

                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if ((creature.movement & Config.monsters_move.CM_PHASE) == 0u)
                    {
                        // monster does not move, can't escape the wall
                        int damage;
                        if ((creature.movement & Config.monsters_move.CM_ATTACK_ONLY) != 0u)
                        {
                            // this will kill everything
                            damage = 3000;
                        }
                        else
                        {
                            damage = this.dice.diceRoll(new Dice_t(4, 8));
                        }

                        var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

                        Monster_m.printMonsterActionText(name, "wails out in pain!");

                        var creature_id = this.eventPublisher.PublishWithOutputInt(
                            new TakeHitCommand((int)tile.creature_id, damage)
                        );
                        if (creature_id >= 0)
                        //if (Monster_m.monsterTakeHit((int)tile.creature_id, damage) >= 0)
                        {
                            Monster_m.printMonsterActionText(name, "is embedded in the rock.");
                            Ui_m.displayCharacterExperience();
                        }
                    }
                    else if (creature.sprite == 'E' || creature.sprite == 'X')
                    {
                        // must be an earth elemental, an earth spirit,
                        // or a Xorn to increase its hit points
                        monster.hp += this.dice.diceRoll(new Dice_t(4, 8));
                    }
                }

                tile.feature_id = Dungeon_tile_c.TILE_MAGMA_WALL;
                tile.field_mark = false;

                // Permanently light this wall if it is lit by player's lamp.
                tile.permanent_light = tile.temporary_light || tile.permanent_light;
                this.dungeon.dungeonLiteSpot(coord);

                built = true;
            }

            return built;
        }
    }
}