using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class GenocideCommandHandler : ICommandHandler<GenocideCommand>
    {
        private readonly IDungeon dungeon;

        public GenocideCommandHandler(IDungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public void Handle(GenocideCommand command)
        {
            this.spellGenocide();
        }

        // Delete all creatures of a given type from level. -RAK-
        // This does not keep creatures of type from appearing later.
        // NOTE : Winning creatures can not be killed by genocide.
        private bool spellGenocide()
        {
            if (!Ui_io_m.getCommand("Which type of creature do you wish exterminated?", out var creature_char))
            {
                return false;
            }

            var killed = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                if (creature_char == Library.Instance.Creatures.creatures_list[(int)monster.creature_id].sprite)
                {
                    if ((creature.movement & Config.monsters_move.CM_WIN) == 0)
                    {
                        killed = true;
                        this.dungeon.dungeonDeleteMonster(id);
                    }
                    else
                    {
                        // genocide is a powerful spell, so we will let the player
                        // know the names of the creatures they did not destroy,
                        // this message makes no sense otherwise
                        Ui_io_m.printMessage("The " + creature.name + " is unaffected.");
                    }
                }
            }

            return killed;
        }
    }
}