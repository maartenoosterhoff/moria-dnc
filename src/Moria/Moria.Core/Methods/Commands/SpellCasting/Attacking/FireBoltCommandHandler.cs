using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class FireBoltCommandHandler : ICommandHandler<FireBoltCommand>
    {
        private readonly IDungeon dungeon;
        private readonly IHelpers helpers;

        public FireBoltCommandHandler(
            IDungeon dungeon,
            IHelpers helpers
        )
        {
            this.dungeon = dungeon;
            this.helpers = helpers;
        }
        public void Handle(FireBoltCommand command)
        {
            this.spellFireBolt(
                command.Coord,
                command.Direction,
                command.DamageHp,
                command.SpellType,
                command.SpellName
            );
        }

        // Shoot a bolt in a given direction -RAK-
        private void spellFireBolt(Coord_t coord, int direction, int damage_hp, int spell_type, string spell_name)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            int harm_type;
            uint weapon_type;
            Spells_m.spellGetAreaAffectFlags(spell_type, out weapon_type, out harm_type, out _);

            var old_coord = new Coord_t(0, 0);

            var distance = 0;
            var finished = false;

            while (!finished)
            {
                old_coord.y = coord.y;
                old_coord.x = coord.x;
                this.helpers.movePosition(direction, ref coord);

                distance++;

                var tile = dg.floor[coord.y][coord.x];

                this.dungeon.dungeonLiteSpot(old_coord);

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue; // we're done here, break out of the loop
                }

                if (tile.creature_id > 1)
                {
                    finished = true;
                    this.spellFireBoltTouchesMonster(tile, damage_hp, harm_type, weapon_type, spell_name);
                }
                else if (Ui_m.coordInsidePanel(coord) && py.flags.blind < 1)
                {
                    Ui_io_m.panelPutTile('*', coord);

                    // show the bolt
                    Ui_io_m.putQIO();
                }
            }
        }

        // Light up, draw, and check for monster damage when Fire Bolt touches it.
        private void spellFireBoltTouchesMonster(Tile_t tile, int damage, int harm_type, uint weapon_id, string bolt_name)
        {
            var monster = State.Instance.monsters[tile.creature_id];
            var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

            // light up monster and draw monster, temporarily set
            // permanent_light so that `monsterUpdateVisibility()` will work
            var saved_lit_status = tile.permanent_light;
            tile.permanent_light = true;
            Monster_m.monsterUpdateVisibility((int)tile.creature_id);
            tile.permanent_light = saved_lit_status;

            // draw monster and clear previous bolt
            Ui_io_m.putQIO();

            printBoltStrikesMonsterMessage(creature, bolt_name, monster.lit);

            if ((harm_type & creature.defenses) != 0)
            {
                damage = damage * 2;
                if (monster.lit)
                {
                    State.Instance.creature_recall[monster.creature_id].defenses |= (uint)harm_type;
                }
            }
            else if ((weapon_id & creature.spells) != 0u)
            {
                damage = damage / 4;
                if (monster.lit)
                {
                    State.Instance.creature_recall[monster.creature_id].spells |= weapon_id;
                }
            }

            var name = Monster_m.monsterNameDescription(creature.name, monster.lit);

            if (Monster_m.monsterTakeHit((int)tile.creature_id, damage) >= 0)
            {
                Monster_m.printMonsterActionText(name, "dies in a fit of agony.");
                Ui_m.displayCharacterExperience();
            }
            else if (damage > 0)
            {
                Monster_m.printMonsterActionText(name, "screams in agony.");
            }
        }

        private void printBoltStrikesMonsterMessage(Creature_t creature, string bolt_name, bool is_lit)
        {
            string monster_name;
            if (is_lit)
            {
                monster_name = "the " + creature.name;
            }
            else
            {
                monster_name = "it";
            }
            var msg = "The " + bolt_name + " strikes " + monster_name + ".";
            Ui_io_m.printMessage(msg);
        }
    }
}