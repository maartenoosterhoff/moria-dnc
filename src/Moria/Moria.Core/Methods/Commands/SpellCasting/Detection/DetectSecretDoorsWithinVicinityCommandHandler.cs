using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Detection
{
    public class DetectSecretDoorsWithinVicinityCommandHandler :
        ICommandHandler<DetectSecretDoorsWithinVicinityCommand>,
        ICommandHandler<DetectSecretDoorsWithinVicinityCommand, bool>
    {
        private readonly IDungeon dungeon;

        public DetectSecretDoorsWithinVicinityCommandHandler(IDungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        void ICommandHandler<DetectSecretDoorsWithinVicinityCommand>.Handle(DetectSecretDoorsWithinVicinityCommand command)
        {
            this.spellDetectSecretDoorssWithinVicinity();
        }

        bool ICommandHandler<DetectSecretDoorsWithinVicinityCommand, bool>.Handle(DetectSecretDoorsWithinVicinityCommand command)
        {
            return this.spellDetectSecretDoorssWithinVicinity();
        }

        // Locates and displays all secret doors on current panel -RAK-
        private bool spellDetectSecretDoorssWithinVicinity()
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

                    if (game.treasure.list[tile.treasure_id].category_id == Treasure_c.TV_SECRET_DOOR)
                    {
                        // Secret doors

                        tile.field_mark = true;
                        this.dungeon.trapChangeVisibility(coord);
                        detected = true;
                    }
                    else if ((game.treasure.list[tile.treasure_id].category_id == Treasure_c.TV_UP_STAIR || game.treasure.list[tile.treasure_id].category_id == Treasure_c.TV_DOWN_STAIR) && !tile.field_mark)
                    {
                        // Staircases

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