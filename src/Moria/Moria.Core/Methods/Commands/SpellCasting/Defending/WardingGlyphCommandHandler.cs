using Moria.Core.Configs;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting.Defending
{
    public class WardingGlyphCommandHandler : ICommandHandler<WardingGlyphCommand>
    {
        private readonly IGameObjects gameObjects;
        private readonly IInventoryManager inventoryManager;

        public WardingGlyphCommandHandler(
            IGameObjects gameObjects,
            IInventoryManager inventoryManager
        )
        {
            this.gameObjects = gameObjects;
            this.inventoryManager = inventoryManager;
        }

        public void Handle(WardingGlyphCommand command)
        {
            this.spellWardingGlyph();
        }

        // Leave a glyph of warding. Creatures will not pass over! -RAK-
        private void spellWardingGlyph()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            if (dg.floor[py.pos.y][py.pos.x].treasure_id == 0)
            {
                var free_id = this.gameObjects.popt();
                dg.floor[py.pos.y][py.pos.x].treasure_id = (uint)free_id;
                this.inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_SCARE_MON, game.treasure.list[free_id]);
            }
        }
    }
}