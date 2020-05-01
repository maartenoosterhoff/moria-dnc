using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class DetectMonstersCommandHandler : ICommandHandler<DetectMonstersCommand>
    {
        public void Handle(DetectMonstersCommand command)
        {
            this.spellDetectMonsters();
        }

        // Display all creatures on the current panel -RAK-
        private bool spellDetectMonsters()
        {
            var detected = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (Ui_m.coordInsidePanel(new Coord_t(monster.pos.y, monster.pos.x)) && (Library.Instance.Creatures.creatures_list[(int)monster.creature_id].movement & Config.monsters_move.CM_INVISIBLE) == 0)
                {
                    monster.lit = true;
                    detected = true;

                    // works correctly even if hallucinating
                    Ui_io_m.panelPutTile((char)Library.Instance.Creatures.creatures_list[(int)monster.creature_id].sprite, new Coord_t(monster.pos.y, monster.pos.x));
                }
            }

            if (detected)
            {
                Ui_io_m.printMessage("You sense the presence of monsters!");
                Ui_io_m.printMessage(/*CNIL*/null);

                // must unlight every monster just lighted
                Monster_m.updateMonsters(false);
            }

            return detected;
        }
    }
}