using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class BreathCommandHandler : ICommandHandler<BreathCommand>
    {
        private readonly IDungeon dungeon;
        private readonly IDungeonLos dungeonLos;
        private readonly IHelpers helpers;
        private readonly IInventory inventory;
        private readonly ISpells spells;
        private readonly ITerminal terminal;

        public BreathCommandHandler(
            IDungeon dungeon,
            IDungeonLos dungeonLos,
            IHelpers helpers,
            IInventory inventory,
            ISpells spells,
            ITerminal terminal
        )
        {
            this.dungeon = dungeon;
            this.dungeonLos = dungeonLos;
            this.helpers = helpers;
            this.inventory = inventory;
            this.spells = spells;
            this.terminal = terminal;
        }

        public void Handle(BreathCommand command)
        {
            this.spellBreath(
                command.Coord,
                command.MonsterId,
                command.DamageHp,
                command.SpellType,
                command.SpellName
            );
        }

        // Breath weapon works like a spellFireBall(), but affects the player.
        // Note the area affect. -RAK-
        private void spellBreath(Coord_t coord, int monster_id, int damage_hp, int spell_type, string spell_name)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            var max_distance = 2;

            this.spells.spellGetAreaAffectFlags(spell_type, out var weapon_type, out var harm_type, out var destroy);

            var location = new Coord_t(0, 0);

            for (location.y = coord.y - 2; location.y <= coord.y + 2; location.y++)
            {
                for (location.x = coord.x - 2; location.x <= coord.x + 2; location.x++)
                {
                    if (this.dungeon.coordInBounds(location) && this.dungeon.coordDistanceBetween(coord, location) <= max_distance && this.dungeonLos.los(coord, location))
                    {
                        var tile = dg.floor[location.y][location.x];

                        if (tile.treasure_id != 0 && destroy(game.treasure.list[tile.treasure_id]))
                        {
                            this.dungeon.dungeonDeleteObject(location);
                        }

                        if (tile.feature_id <= Dungeon_tile_c.MAX_OPEN_SPACE)
                        {
                            // must test status bit, not py.flags.blind here, flag could have
                            // been set by a previous monster, but the breath should still
                            // be visible until the blindness takes effect
                            if (this.helpers.coordInsidePanel(location) && (py.flags.status & Config.player_status.PY_BLIND) == 0u)
                            {
                                this.terminal.panelPutTile('*', location);
                            }

                            if (tile.creature_id > 1)
                            {
                                var monster = State.Instance.monsters[tile.creature_id];
                                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                                var damage = damage_hp;

                                if ((harm_type & creature.defenses) != 0)
                                {
                                    damage = damage * 2;
                                }
                                else if ((weapon_type & creature.spells) != 0u)
                                {
                                    damage = damage / 4;
                                }

                                damage = damage / (this.dungeon.coordDistanceBetween(location, coord) + 1);

                                // can not call monsterTakeHit here, since player does not
                                // get experience for kill
                                monster.hp = (int)(monster.hp - damage);
                                monster.sleep_count = 0;

                                if (monster.hp < 0)
                                {
                                    var treasure_id = Monster_m.monsterDeath(new Coord_t(monster.pos.y, monster.pos.x), creature.movement);

                                    if (monster.lit)
                                    {
                                        var tmp = (uint)((State.Instance.creature_recall[monster.creature_id].movement & Config.monsters_move.CM_TREASURE) >> (int)Config.monsters_move.CM_TR_SHIFT);
                                        if (tmp > (treasure_id & Config.monsters_move.CM_TREASURE) >> (int)Config.monsters_move.CM_TR_SHIFT)
                                        {
                                            treasure_id = (uint)((treasure_id & ~Config.monsters_move.CM_TREASURE) | (tmp << (int)Config.monsters_move.CM_TR_SHIFT));
                                        }
                                        State.Instance.creature_recall[monster.creature_id].movement =
                                            (uint)(treasure_id | (State.Instance.creature_recall[monster.creature_id].movement & ~Config.monsters_move.CM_TREASURE));
                                    }

                                    // It ate an already processed monster. Handle normally.
                                    if (monster_id < tile.creature_id)
                                    {
                                        this.dungeon.dungeonDeleteMonster((int)tile.creature_id);
                                    }
                                    else
                                    {
                                        // If it eats this monster, an already processed monster
                                        // will take its place, causing all kinds of havoc.
                                        // Delay the kill a bit.
                                        this.dungeon.dungeonDeleteMonsterFix1((int)tile.creature_id);
                                    }
                                }
                            }
                            else if (tile.creature_id == 1)
                            {
                                var damage = damage_hp / (this.dungeon.coordDistanceBetween(location, coord) + 1);

                                // let's do at least one point of damage
                                // prevents rnd.randomNumber(0) problem with damagePoisonedGas, also
                                if (damage == 0)
                                {
                                    damage = 1;
                                }

                                switch ((MagicSpellFlags)spell_type)
                                {
                                    case MagicSpellFlags.Lightning:
                                        this.inventory.damageLightningBolt(damage, spell_name);
                                        break;
                                    case MagicSpellFlags.PoisonGas:
                                        this.inventory.damagePoisonedGas(damage, spell_name);
                                        break;
                                    case MagicSpellFlags.Acid:
                                        this.inventory.damageAcid(damage, spell_name);
                                        break;
                                    case MagicSpellFlags.Frost:
                                        this.inventory.damageCold(damage, spell_name);
                                        break;
                                    case MagicSpellFlags.Fire:
                                        this.inventory.damageFire(damage, spell_name);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            // show the ball of gas
            this.terminal.putQIO();

            var spot = new Coord_t(0, 0);
            for (spot.y = coord.y - 2; spot.y <= coord.y + 2; spot.y++)
            {
                for (spot.x = coord.x - 2; spot.x <= coord.x + 2; spot.x++)
                {
                    if (this.dungeon.coordInBounds(spot) && this.helpers.coordInsidePanel(spot) && this.dungeon.coordDistanceBetween(coord, spot) <= max_distance)
                    {
                        this.dungeon.dungeonLiteSpot(spot);
                    }
                }
            }
        }
    }
}