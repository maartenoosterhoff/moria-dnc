using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Detection
{
    public class DetectTreasureWithinVicinityCommandHandler : ICommandHandler<DetectTreasureWithinVicinityCommand, bool>
    {
        private readonly IDungeon dungeon;

        public DetectTreasureWithinVicinityCommandHandler(IDungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public bool Handle(DetectTreasureWithinVicinityCommand command)
        {
            return this.spellDetectTreasureWithinVicinity();
        }

        // Detect any treasure on the current panel -RAK-
        private bool spellDetectTreasureWithinVicinity()
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

                    if (tile.treasure_id != 0 && game.treasure.list[tile.treasure_id].category_id == Treasure_c.TV_GOLD && !this.dungeon.caveTileVisible(coord))
                    {
                        tile.field_mark = true;
                        this.dungeon.dungeonLiteSpot(coord);
                        detected = true;
                    }
                }
            }

            return detected;
        }
    }
}