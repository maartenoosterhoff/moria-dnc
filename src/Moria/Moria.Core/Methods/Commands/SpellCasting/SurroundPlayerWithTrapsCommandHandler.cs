using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class SurroundPlayerWithTrapsCommandHandler :
        ICommandHandler<SurroundPlayerWithTrapsCommand>,
        ICommandHandler<SurroundPlayerWithTrapsCommand, bool>
    {
        private readonly IDungeon dungeon;
        private readonly IDungeonPlacer dungeonPlacer;
        private readonly IRnd rnd;

        public SurroundPlayerWithTrapsCommandHandler(
            IDungeon dungeon,
            IDungeonPlacer dungeonPlacer,
            IRnd rnd
        )
        {
            this.dungeon = dungeon;
            this.dungeonPlacer = dungeonPlacer;
            this.rnd = rnd;
        }

        void ICommandHandler<SurroundPlayerWithTrapsCommand>.Handle(SurroundPlayerWithTrapsCommand command)
        {
            this.spellSurroundPlayerWithTraps();
        }

        bool ICommandHandler<SurroundPlayerWithTrapsCommand, bool>.Handle(SurroundPlayerWithTrapsCommand command)
        {
            return this.spellSurroundPlayerWithTraps();
        }

        // Surround the fool with traps (chuckle) -RAK-
        private bool spellSurroundPlayerWithTraps()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var coord = new Coord_t(0, 0);

            for (coord.y = py.pos.y - 1; coord.y <= py.pos.y + 1; coord.y++)
            {
                for (coord.x = py.pos.x - 1; coord.x <= py.pos.x + 1; coord.x++)
                {
                    // Don't put a trap under the player, since this can lead to
                    // strange situations, e.g. falling through a trap door while
                    // trying to rest, setting off a falling rock trap and ending
                    // up under the rock.
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

                        this.dungeonPlacer.dungeonSetTrap(coord, this.rnd.randomNumber(Config.dungeon_objects.MAX_TRAPS) - 1);

                        // don't let player gain exp from the newly created traps
                        game.treasure.list[tile.treasure_id].misc_use = 0;

                        // open pits are immediately visible, so call dungeonLiteSpot
                        this.dungeon.dungeonLiteSpot(coord);
                    }
                }
            }

            // traps are always placed, so just return true
            return true;
        }
    }
}