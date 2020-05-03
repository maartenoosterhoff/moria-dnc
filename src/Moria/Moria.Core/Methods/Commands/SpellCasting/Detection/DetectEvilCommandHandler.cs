﻿using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Detection
{
    public class DetectEvilCommandHandler :
        ICommandHandler<DetectEvilCommand>,
        ICommandHandler<DetectEvilCommand, bool>
    {
        void ICommandHandler<DetectEvilCommand>.Handle(DetectEvilCommand command)
        {
            this.spellDetectEvil();
        }

        bool ICommandHandler<DetectEvilCommand, bool>.Handle(DetectEvilCommand command)
        {
            return this.spellDetectEvil();
        }

        // Display evil creatures on current panel -RAK-
        private bool spellDetectEvil()
        {
            var detected = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (Ui_m.coordInsidePanel(new Coord_t(monster.pos.y, monster.pos.x)) && (Library.Instance.Creatures.creatures_list[(int)monster.creature_id].defenses & Config.monsters_defense.CD_EVIL) != 0)
                {
                    monster.lit = true;

                    detected = true;

                    // works correctly even if hallucinating
                    Ui_io_m.panelPutTile((char)Library.Instance.Creatures.creatures_list[(int)monster.creature_id].sprite, new Coord_t(monster.pos.y, monster.pos.x));
                }
            }

            if (detected)
            {
                Ui_io_m.printMessage("You sense the presence of evil!");
                Ui_io_m.printMessage(/*CNIL*/null);

                // must unlight every monster just lighted
                Monster_m.updateMonsters(false);
            }

            return detected;
        }
    }
}