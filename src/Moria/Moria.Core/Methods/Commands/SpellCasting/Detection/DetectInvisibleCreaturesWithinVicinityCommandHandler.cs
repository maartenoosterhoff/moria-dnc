using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Detection
{
    public class DetectInvisibleCreaturesWithinVicinityCommandHandler :
        ICommandHandler<DetectInvisibleCreaturesWithinVicinityCommand, bool>
    {
        private readonly ITerminal terminal;

        public DetectInvisibleCreaturesWithinVicinityCommandHandler(
            ITerminal terminal
        )
        {
            this.terminal = terminal;
        }

        public bool Handle(DetectInvisibleCreaturesWithinVicinityCommand command)
        {
            return this.spellDetectInvisibleCreaturesWithinVicinity();
        }

        // Locates and displays all invisible creatures on current panel -RAK-
        private bool spellDetectInvisibleCreaturesWithinVicinity()
        {
            var detected = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (Ui_m.coordInsidePanel(new Coord_t(monster.pos.y, monster.pos.x)) &&
                    (Library.Instance.Creatures.creatures_list[(int)monster.creature_id].movement & Config.monsters_move.CM_INVISIBLE) != 0u)
                {
                    monster.lit = true;

                    // works correctly even if hallucinating
                    this.terminal.panelPutTile((char)Library.Instance.Creatures.creatures_list[(int)monster.creature_id].sprite, new Coord_t(monster.pos.y, monster.pos.x));

                    detected = true;
                }
            }

            if (detected)
            {
                this.terminal.printMessage("You sense the presence of invisible creatures!");
                this.terminal.printMessage(/*CNIL*/null);

                // must unlight every monster just lighted
                Monster_m.updateMonsters(false);
            }

            return detected;
        }
    }
}