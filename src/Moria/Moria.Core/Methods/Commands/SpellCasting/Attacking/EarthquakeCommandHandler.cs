using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Monster;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class EarthquakeCommandHandler : ICommandHandler<EarthquakeCommand>
    {
        private readonly IDice dice;
        private readonly IDungeon dungeon;
        private readonly IEventPublisher eventPublisher;
        private readonly IMonster monster;
        private readonly IRnd rnd;
        private readonly ITerminalEx terminalEx;

        public EarthquakeCommandHandler(
            IDice dice,
            IDungeon dungeon,
            IEventPublisher eventPublisher,
            IMonster monster,
            IRnd rnd,
            ITerminalEx terminalEx
        )
        {
            this.dice = dice;
            this.dungeon = dungeon;
            this.eventPublisher = eventPublisher;
            this.monster = monster;
            this.rnd = rnd;
            this.terminalEx = terminalEx;
        }

        public void Handle(EarthquakeCommand command)
        {
            this.spellEarthquake();
        }

        // This is a fun one.  In a given block, pick some walls and
        // turn them into open spots.  Pick some open spots and dg.game_turn
        // them into walls.  An "Earthquake" effect. -RAK-
        private void spellEarthquake()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var coord = new Coord_t(0, 0);

            for (coord.y = py.pos.y - 8; coord.y <= py.pos.y + 8; coord.y++)
            {
                for (coord.x = py.pos.x - 8; coord.x <= py.pos.x + 8; coord.x++)
                {
                    if ((coord.y != py.pos.y || coord.x != py.pos.x) && this.dungeon.coordInBounds(coord) && this.rnd.randomNumber(8) == 1)
                    {
                        var tile = dg.floor[coord.y][coord.x];

                        if (tile.treasure_id != 0)
                        {
                            this.dungeon.dungeonDeleteObject(coord);
                        }

                        if (tile.creature_id > 1)
                        {
                            this.earthquakeHitsMonster((int)tile.creature_id);
                        }

                        if (tile.feature_id >= Dungeon_tile_c.MIN_CAVE_WALL && tile.feature_id != Dungeon_tile_c.TILE_BOUNDARY_WALL)
                        {
                            tile.feature_id = Dungeon_tile_c.TILE_CORR_FLOOR;
                            tile.permanent_light = false;
                            tile.field_mark = false;
                        }
                        else if (tile.feature_id <= Dungeon_tile_c.MAX_CAVE_FLOOR)
                        {
                            var tmp = this.rnd.randomNumber(10);

                            if (tmp < 6)
                            {
                                tile.feature_id = Dungeon_tile_c.TILE_QUARTZ_WALL;
                            }
                            else if (tmp < 9)
                            {
                                tile.feature_id = Dungeon_tile_c.TILE_MAGMA_WALL;
                            }
                            else
                            {
                                tile.feature_id = Dungeon_tile_c.TILE_GRANITE_WALL;
                            }

                            tile.field_mark = false;
                        }

                        this.dungeon.dungeonLiteSpot(coord);
                    }
                }
            }
        }

        private void earthquakeHitsMonster(int monster_id)
        {
            var monster = State.Instance.monsters[monster_id];
            var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

            if ((creature.movement & Config.monsters_move.CM_PHASE) == 0u)
            {
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

                var name = this.monster.monsterNameDescription(creature.name, monster.lit);

                this.monster.printMonsterActionText(name, "wails out in pain!");

                var creature_id = this.eventPublisher.PublishWithOutputInt(
                    new TakeHitCommand(monster_id, damage)
                );
                if (creature_id >= 0)
//                if (Monster_m.monsterTakeHit(monster_id, damage) >= 0)
                {
                    this.monster.printMonsterActionText(name, "is embedded in the rock.");
                    this.terminalEx.displayCharacterExperience();
                }
            }
            else if (creature.sprite == 'E' || creature.sprite == 'X')
            {
                // must be an earth elemental or an earth spirit, or a
                // Xorn increase its hit points
                monster.hp += this.dice.diceRoll(new Dice_t(4, 8));
            }
        }
    }
}