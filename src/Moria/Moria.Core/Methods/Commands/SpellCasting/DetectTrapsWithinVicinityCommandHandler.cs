using System;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class DetectTrapsWithinVicinityCommandHandler : ICommandHandler<DetectTrapsWithinVicinityCommand, bool>, ICommandHandler<DetectTrapsWithinVicinityCommand>
    {
        private readonly IDungeon dungeon;

        public DetectTrapsWithinVicinityCommandHandler(IDungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        void ICommandHandler<DetectTrapsWithinVicinityCommand>.Handle(DetectTrapsWithinVicinityCommand command)
        {
            this.spellDetectTrapsWithinVicinity();
        }

        bool ICommandHandler<DetectTrapsWithinVicinityCommand, bool>.Handle(DetectTrapsWithinVicinityCommand command)
        {
            return this.spellDetectTrapsWithinVicinity();
        }

        // Locates and displays traps on current panel -RAK-
        private bool spellDetectTrapsWithinVicinity()
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var detected = false;

            var coord = new Coord_t(0, 0);

            for (coord.y = dg.panel.top; coord.y <= dg.panel.bottom; coord.y++)
            {
                for (coord.x = dg.panel.left; coord.x <= dg.panel.right; coord.x++)
                {
                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.treasure_id == 0)
                    {
                        continue;
                    }

                    if (game.treasure.list[tile.treasure_id].category_id == Treasure_c.TV_INVIS_TRAP)
                    {
                        tile.field_mark = true;
                        this.dungeon.trapChangeVisibility(coord);
                        detected = true;
                    }
                    else if (game.treasure.list[tile.treasure_id].category_id == Treasure_c.TV_CHEST)
                    {
                        var item = game.treasure.list[tile.treasure_id];
                        Identification_m.spellItemIdentifyAndRemoveRandomInscription(item);
                    }
                }
            }

            return detected;
        }
    }
}