using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.SpellCasting.Defending
{
    public class TeleportAwayMonsterCommandHandler : ICommandHandler<TeleportAwayMonsterCommand>
    {
        private readonly IDungeon dungeon;
        private readonly IMonster monster;
        private readonly IRnd rnd;

        public TeleportAwayMonsterCommandHandler(
            IDungeon dungeon,
            IMonster monster,
            IRnd rnd
        )
        {
            this.dungeon = dungeon;
            this.monster = monster;
            this.rnd = rnd;
        }

        public void Handle(TeleportAwayMonsterCommand command)
        {
            this.spellTeleportAwayMonster(
                command.MonsterId,
                command.DistanceFromPlayer
            );
        }

        // Move the creature record to a new location -RAK-
        private void spellTeleportAwayMonster(int monster_id, int distance_from_player)
        {
            var dg = State.Instance.dg;
            var counter = 0;

            var coord = new Coord_t(0, 0);
            var monster = State.Instance.monsters[monster_id];

            do
            {
                do
                {
                    coord.y = monster.pos.y + (this.rnd.randomNumber(2 * distance_from_player + 1) - (distance_from_player + 1));
                    coord.x = monster.pos.x + (this.rnd.randomNumber(2 * distance_from_player + 1) - (distance_from_player + 1));
                } while (!this.dungeon.coordInBounds(coord));

                counter++;
                if (counter > 9)
                {
                    counter = 0;
                    distance_from_player += 5;
                }
            } while (dg.floor[coord.y][coord.x].feature_id >= Dungeon_tile_c.MIN_CLOSED_SPACE || dg.floor[coord.y][coord.x].creature_id != 0);

            this.dungeon.dungeonMoveCreatureRecord(new Coord_t(monster.pos.y, monster.pos.x), coord);
            this.dungeon.dungeonLiteSpot(new Coord_t(monster.pos.y, monster.pos.x));

            monster.pos.y = coord.y;
            monster.pos.x = coord.x;

            // this is necessary, because the creature is
            // not currently visible in its new position.
            monster.lit = false;
            monster.distance_from_player = (uint) this.dungeon.coordDistanceBetween(State.Instance.py.pos, coord);

            this.monster.monsterUpdateVisibility(monster_id);
        }
    }
}