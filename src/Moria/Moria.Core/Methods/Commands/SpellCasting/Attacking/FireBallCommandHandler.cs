using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Monster;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class FireBallCommandHandler : ICommandHandler<FireBallCommand>
    {
        private readonly IDungeon dungeon;
        private readonly IDungeonLos dungeonLos;
        private readonly IEventPublisher eventPublisher;
        private readonly IHelpers helpers;
        private readonly ISpells spells;
        private readonly ITerminal terminal;

        public FireBallCommandHandler(
            IDungeon dungeon,
            IDungeonLos dungeonLos,
            IEventPublisher eventPublisher,
            IHelpers helpers,
            ISpells spells,
            ITerminal terminal
        )
        {
            this.dungeon = dungeon;
            this.dungeonLos = dungeonLos;
            this.eventPublisher = eventPublisher;
            this.helpers = helpers;
            this.spells = spells;
            this.terminal = terminal;
        }

        public void Handle(FireBallCommand command)
        {
            this.spellFireBall(
                command.Coord,
                command.Direction,
                command.DamageHp,
                command.SpellType,
                command.SpellName
            );
        }

        // Shoot a ball in a given direction.  Note that balls have an area affect. -RAK-
        private void spellFireBall(Coord_t coord, int direction, int damage_hp, int spell_type, string spell_name)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            var total_hits = 0;
            var total_kills = 0;
            var max_distance = 2;

            this.spells.spellGetAreaAffectFlags(spell_type, out var weapon_type, out var harm_type, out var destroy);

            var old_coord = new Coord_t(0, 0);
            var spot = new Coord_t(0, 0);

            var distance = 0;
            var finished = false;

            while (!finished)
            {
                old_coord.y = coord.y;
                old_coord.x = coord.x;
                this.helpers.movePosition(direction, ref coord);

                distance++;

                this.dungeon.dungeonLiteSpot(old_coord);

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE)
                {
                    finished = true;
                    continue;
                }

                var tile = dg.floor[coord.y][coord.x];

                if (tile.feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE || tile.creature_id > 1)
                {
                    finished = true;

                    if (tile.feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE)
                    {
                        coord.y = old_coord.y;
                        coord.x = old_coord.x;
                    }

                    // The ball hits and explodes.

                    // The explosion.
                    for (var row = coord.y - max_distance; row <= coord.y + max_distance; row++)
                    {
                        for (var col = coord.x - max_distance; col <= coord.x + max_distance; col++)
                        {
                            spot.y = row;
                            spot.x = col;

                            if (this.dungeon.coordInBounds(spot) && this.dungeon.coordDistanceBetween(coord, spot) <= max_distance && this.dungeonLos.los(coord, spot))
                            {
                                tile = dg.floor[spot.y][spot.x];

                                if (tile.treasure_id != 0 && destroy(game.treasure.list[tile.treasure_id]))
                                {
                                    this.dungeon.dungeonDeleteObject(spot);
                                }

                                if (tile.feature_id <= Dungeon_tile_c.MAX_OPEN_SPACE)
                                {
                                    if (tile.creature_id > 1)
                                    {
                                        var monster = State.Instance.monsters[tile.creature_id];
                                        var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                                        // lite up creature if visible, temp set permanent_light so that monsterUpdateVisibility works
                                        var saved_lit_status = tile.permanent_light;
                                        tile.permanent_light = true;
                                        Monster_m.monsterUpdateVisibility((int)tile.creature_id);

                                        total_hits++;
                                        var damage = damage_hp;

                                        if ((harm_type & creature.defenses) != 0)
                                        {
                                            damage = damage * 2;
                                            if (monster.lit)
                                            {
                                                State.Instance.creature_recall[monster.creature_id].defenses |= (uint)harm_type;
                                            }
                                        }
                                        else if ((weapon_type & creature.spells) != 0u)
                                        {
                                            damage = damage / 4;
                                            if (monster.lit)
                                            {
                                                State.Instance.creature_recall[monster.creature_id].spells |= weapon_type;
                                            }
                                        }

                                        damage /= (this.dungeon.coordDistanceBetween(spot, coord) + 1);

                                        var creature_id = this.eventPublisher.PublishWithOutputInt(
                                            new TakeHitCommand((int)tile.creature_id, damage)
                                        );
                                        if (creature_id >= 0)
                                        //if (Monster_m.monsterTakeHit((int)tile.creature_id, damage) >= 0)
                                        {
                                            total_kills++;
                                        }
                                        tile.permanent_light = saved_lit_status;
                                    }
                                    else if (Ui_m.coordInsidePanel(spot) && py.flags.blind < 1)
                                    {
                                        this.terminal.panelPutTile('*', spot);
                                    }
                                }
                            }
                        }
                    }

                    // show ball of whatever
                    this.terminal.putQIO();

                    for (var row = coord.y - 2; row <= coord.y + 2; row++)
                    {
                        for (var col = coord.x - 2; col <= coord.x + 2; col++)
                        {
                            spot.y = row;
                            spot.x = col;

                            if (this.dungeon.coordInBounds(spot) && Ui_m.coordInsidePanel(spot) && this.dungeon.coordDistanceBetween(coord, spot) <= max_distance)
                            {
                                this.dungeon.dungeonLiteSpot(spot);
                            }
                        }
                    }
                    // End explosion.

                    if (total_hits == 1)
                    {
                        this.terminal.printMessage("The " + spell_name + " envelops a creature!");
                    }
                    else if (total_hits > 1)
                    {
                        this.terminal.printMessage("The " + spell_name + " envelops several creatures!");
                    }

                    if (total_kills == 1)
                    {
                        this.terminal.printMessage("There is a scream of agony!");
                    }
                    else if (total_kills > 1)
                    {
                        this.terminal.printMessage("There are several screams of agony!");
                    }

                    if (total_kills >= 0)
                    {
                        Ui_m.displayCharacterExperience();
                    }
                    // End ball hitting.
                }
                else if (Ui_m.coordInsidePanel(coord) && py.flags.blind < 1)
                {
                    this.terminal.panelPutTile('*', coord);

                    // show bolt
                    this.terminal.putQIO();
                }
            }
        }
    }
}