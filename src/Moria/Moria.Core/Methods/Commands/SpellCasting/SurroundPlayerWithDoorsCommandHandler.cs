using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class SurroundPlayerWithDoorsCommandHandler :
        ICommandHandler<SurroundPlayerWithDoorsCommand>,
        ICommandHandler<SurroundPlayerWithDoorsCommand, bool>
    {
        private readonly IDungeon dungeon;
        private readonly IGameObjects gameObjects;
        private readonly IInventoryManager inventoryManager;

        public SurroundPlayerWithDoorsCommandHandler(
            IDungeon dungeon,
            IGameObjects gameObjects,
            IInventoryManager inventoryManager
        )
        {
            this.dungeon = dungeon;
            this.gameObjects = gameObjects;
            this.inventoryManager = inventoryManager;
        }

        void ICommandHandler<SurroundPlayerWithDoorsCommand>.Handle(SurroundPlayerWithDoorsCommand command)
        {
            this.spellSurroundPlayerWithDoors();
        }

        bool ICommandHandler<SurroundPlayerWithDoorsCommand, bool>.Handle(SurroundPlayerWithDoorsCommand command)
        {
            return this.spellSurroundPlayerWithDoors();
        }

        // Surround the player with doors. -RAK-
        private bool spellSurroundPlayerWithDoors()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var created = false;

            var coord = new Coord_t(0, 0);

            for (coord.y = py.pos.y - 1; coord.y <= py.pos.y + 1; coord.y++)
            {
                for (coord.x = py.pos.x - 1; coord.x <= py.pos.x + 1; coord.x++)
                {
                    // Don't put a door under the player!
                    if (coord.y == py.pos.y && coord.x == py.pos.x)
                    {
                        continue;
                    }

                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.feature_id <= Dungeon_tile_c.MAX_CAVE_FLOOR)
                    {
                        if (tile.treasure_id != 0)
                        {
                            this.dungeon.dungeonDeleteObject(coord);
                        }

                        var free_id = this.gameObjects.popt();
                        tile.feature_id = Dungeon_tile_c.TILE_BLOCKED_FLOOR;
                        tile.treasure_id = (uint)free_id;

                        this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, game.treasure.list[free_id]);
                        this.dungeon.dungeonLiteSpot(coord);

                        created = true;
                    }
                }
            }

            return created;
        }
    }
}