using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Lighting
{
    public class LightLineCommandHandler : ICommandHandler<LightLineCommand>
    {
        private readonly IDice dice;
        private readonly IDungeon dungeon;
        private readonly IHelpers helpers;

        public LightLineCommandHandler(
            IDice dice,
            IDungeon dungeon,
            IHelpers helpers
        )
        {
            this.dice = dice;
            this.dungeon = dungeon;
            this.helpers = helpers;
        }

        public void Handle(LightLineCommand command)
        {
            this.spellLightLine(
                command.Coord,
                command.Direction
            );
        }

        // Leave a line of light in given dir, blue light can sometimes hurt creatures. -RAK-
        private void spellLightLine(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var finished = false;

            var tmp_coord = new Coord_t(0, 0);

            while (!finished)
            {
                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE)
                {
                    this.helpers.movePosition(direction, ref coord);
                    finished = true;
                    continue; // we're done here, break out of the loop
                }

                if (!tile.permanent_light && !tile.temporary_light)
                {
                    // set permanent_light so that dungeonLiteSpot will work
                    tile.permanent_light = true;

                    // coord y/x need to be maintained, so copy them
                    tmp_coord.y = coord.y;
                    tmp_coord.x = coord.x;

                    if (tile.feature_id == Dungeon_tile_c.TILE_LIGHT_FLOOR)
                    {
                        if (Ui_m.coordInsidePanel(tmp_coord))
                        {
                            this.dungeon.dungeonLightRoom(tmp_coord);
                        }
                    }
                    else
                    {
                        this.dungeon.dungeonLiteSpot(tmp_coord);
                    }
                }

                // set permanent_light in case temporary_light was true above
                tile.permanent_light = true;

                if (tile.creature_id > 1)
                {
                    this.spellLightLineTouchesMonster((int)tile.creature_id);
                }

                // move must be at end because want to light up current tmp_coord
                this.helpers.movePosition(direction, ref coord);
                distance++;
            }
        }

        // Update monster when light line spell touches it.
        private void spellLightLineTouchesMonster(int monster_id)
        {
            var monster = State.Instance.monsters[monster_id];
            var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

            // light up and draw monster
            Monster_m.monsterUpdateVisibility(monster_id);

            var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

            if ((creature.defenses & Config.monsters_defense.CD_LIGHT) != 0)
            {
                if (monster.lit)
                {
                    State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_LIGHT;
                }

                if (Monster_m.monsterTakeHit(monster_id, this.dice.diceRoll(new Dice_t(2, 8))) >= 0)
                {
                    Monster_m.printMonsterActionText(name, "shrivels away in the light!");
                    Ui_m.displayCharacterExperience();
                }
                else
                {
                    Monster_m.printMonsterActionText(name, "cringes from the light!");
                }
            }
        }
    }
}