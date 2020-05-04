using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class WallToMudCommandHandler :
        ICommandHandler<WallToMudCommand>,
        ICommandHandler<WallToMudCommand, bool>
    {
        private readonly IDungeon dungeon;
        private readonly IDungeonPlacer dungeonPlacer;
        private readonly IHelpers helpers;
        private readonly IIdentification identification;
        private readonly IRnd rnd;
        private readonly ITerminal terminal;

        public WallToMudCommandHandler(
            IDungeon dungeon,
            IDungeonPlacer dungeonPlacer,
            IHelpers helpers,
            IIdentification identification,
            IRnd rnd,
            ITerminal terminal
        )
        {
            this.dungeon = dungeon;
            this.dungeonPlacer = dungeonPlacer;
            this.helpers = helpers;
            this.identification = identification;
            this.rnd = rnd;
            this.terminal = terminal;
        }
        void ICommandHandler<WallToMudCommand>.Handle(WallToMudCommand command)
        {
            this.spellWallToMud(
                command.Coord,
                command.Direction
            );
        }

        bool ICommandHandler<WallToMudCommand, bool>.Handle(WallToMudCommand command)
        {
            return this.spellWallToMud(
                command.Coord,
                command.Direction
            );
        }

        // Turn stone to mud, delete wall. -RAK-
        private bool spellWallToMud(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var distance = 0;
            var turned = false;
            var finished = false;

            while (!finished)
            {
                this.helpers.movePosition(direction, ref coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                // note, this ray can move through walls as it turns them to mud
                if (distance == Config.treasure.OBJECT_BOLTS_MAX_RANGE)
                {
                    finished = true;
                }

                if (tile.feature_id >= Dungeon_tile_c.MIN_CAVE_WALL && tile.feature_id != Dungeon_tile_c.TILE_BOUNDARY_WALL)
                {
                    finished = true;

                    Player_m.playerTunnelWall(coord, 1, 0);

                    if (this.dungeon.caveTileVisible(coord))
                    {
                        turned = true;
                        this.terminal.printMessage("The wall turns into mud.");
                    }
                }
                else if (tile.treasure_id != 0 && tile.feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE)
                {
                    finished = true;

                    if (Ui_m.coordInsidePanel(coord) && this.dungeon.caveTileVisible(coord))
                    {
                        turned = true;

                        this.identification.itemDescription(out var description, game.treasure.list[tile.treasure_id], false);

                        var out_val = $"The {description} turns into mud.";
                        //obj_desc_t out_val = { '\0' };
                        //(void)sprintf(out_val, "The %s turns into mud.", description);
                        this.terminal.printMessage(out_val);
                    }

                    if (game.treasure.list[tile.treasure_id].category_id == Treasure_c.TV_RUBBLE)
                    {
                        this.dungeon.dungeonDeleteObject(coord);
                        if (this.rnd.randomNumber(10) == 1)
                        {
                            this.dungeonPlacer.dungeonPlaceRandomObjectAt(coord, false);
                            if (this.dungeon.caveTileVisible(coord))
                            {
                                this.terminal.printMessage("You have found something!");
                            }
                        }

                        this.dungeon.dungeonLiteSpot(coord);
                    }
                    else
                    {
                        this.dungeon.dungeonDeleteObject(coord);
                    }
                }

                if (tile.creature_id > 1)
                {
                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if ((creature.defenses & Config.monsters_defense.CD_STONE) != 0)
                    {
                        var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

                        // Should get these messages even if the monster is not visible.
                        var creature_id = Monster_m.monsterTakeHit((int)tile.creature_id, 100);
                        if (creature_id >= 0)
                        {
                            State.Instance.creature_recall[creature_id].defenses |= Config.monsters_defense.CD_STONE;
                            Monster_m.printMonsterActionText(name, "dissolves!");
                            Ui_m.displayCharacterExperience(); // print msg before calling prt_exp
                        }
                        else
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_STONE;
                            Monster_m.printMonsterActionText(name, "grunts in pain!");
                        }
                        finished = true;
                    }
                }
            }

            return turned;
        }
    }
}