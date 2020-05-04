using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting.Destroying
{
    public class DestroyAdjacentDoorsTrapsCommandHandler :
        ICommandHandler<DestroyAdjacentDoorsTrapsCommand>,
        ICommandHandler<DestroyAdjacentDoorsTrapsCommand, bool>
    {
        private readonly IDungeon dungeon;
        private readonly ITerminal terminal;

        public DestroyAdjacentDoorsTrapsCommandHandler(
            IDungeon dungeon,
            ITerminal terminal
        )
        {
            this.dungeon = dungeon;
            this.terminal = terminal;
        }

        void ICommandHandler<DestroyAdjacentDoorsTrapsCommand>.Handle(DestroyAdjacentDoorsTrapsCommand command)
        {
            this.spellDestroyAdjacentDoorsTraps();
        }

        bool ICommandHandler<DestroyAdjacentDoorsTrapsCommand, bool>.Handle(DestroyAdjacentDoorsTrapsCommand command)
        {
            return this.spellDestroyAdjacentDoorsTraps();
        }

        // Destroys any adjacent door(s)/trap(s) -RAK-
        private bool spellDestroyAdjacentDoorsTraps()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            var destroyed = false;

            var coord = new Coord_t(0, 0);

            for (coord.y = py.pos.y - 1; coord.y <= py.pos.y + 1; coord.y++)
            {
                for (coord.x = py.pos.x - 1; coord.x <= py.pos.x + 1; coord.x++)
                {
                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.treasure_id == 0)
                    {
                        continue;
                    }

                    var item = game.treasure.list[tile.treasure_id];

                    if (item.category_id >= Treasure_c.TV_INVIS_TRAP && item.category_id <= Treasure_c.TV_CLOSED_DOOR && item.category_id != Treasure_c.TV_RUBBLE || item.category_id == Treasure_c.TV_SECRET_DOOR)
                    {
                        if (this.dungeon.dungeonDeleteObject(coord))
                        {
                            destroyed = true;
                        }
                    }
                    else if (item.category_id == Treasure_c.TV_CHEST && item.flags != 0)
                    {
                        // destroy traps on chest and unlock
                        item.flags &= ~(Config.treasure_chests.CH_TRAPPED | Config.treasure_chests.CH_LOCKED);
                        item.special_name_id = (int)SpecialNameIds.SN_UNLOCKED;

                        destroyed = true;

                        if (this.terminal != null) this.terminal.printMessage("You have disarmed the chest.");
                        Identification_m.spellItemIdentifyAndRemoveRandomInscription(item);
                    }
                }
            }

            return destroyed;
        }
    }
}