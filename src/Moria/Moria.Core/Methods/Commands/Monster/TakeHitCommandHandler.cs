using Moria.Core.Configs;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.Monster
{
    public class TakeHitCommandHandler :
        ICommandHandler<TakeHitCommand, int>
    {
        private readonly IDungeon dungeon;

        public TakeHitCommandHandler(IDungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public int Handle(TakeHitCommand command)
        {
            return this.monsterTakeHit(
                command.MonsterId,
                command.Damage
            );
        }

        // Decreases monsters hit points and deletes monster if needed.
        // (Picking on my babies.) -RAK-
        private int monsterTakeHit(int monster_id, int damage)
        {
            var monsters = State.Instance.monsters;
            var py = State.Instance.py;
            var creatures_list = Library.Instance.Creatures.creatures_list;

            var monster = monsters[monster_id];
            var creature = creatures_list[(int)monster.creature_id];

            monster.sleep_count = 0;
            monster.hp -= damage;

            if (monster.hp >= 0)
            {
                return -1;
            }

            var treasure_flags = Monster_m.monsterDeath(new Coord_t(monster.pos.y, monster.pos.x), creature.movement);

            var memory = State.Instance.creature_recall[monster.creature_id];

            if (py.flags.blind < 1 && monster.lit || (creature.movement & Config.monsters_move.CM_WIN) != 0u)
            {
                var tmp = (uint)((memory.movement & Config.monsters_move.CM_TREASURE) >> (int)Config.monsters_move.CM_TR_SHIFT);

                if (tmp > (treasure_flags & Config.monsters_move.CM_TREASURE) >> (int)Config.monsters_move.CM_TR_SHIFT)
                {
                    treasure_flags = (uint)((treasure_flags & ~Config.monsters_move.CM_TREASURE) | (tmp << (int)Config.monsters_move.CM_TR_SHIFT));
                }

                memory.movement = (uint)((memory.movement & ~Config.monsters_move.CM_TREASURE) | treasure_flags);

                if (memory.kills < Std_c.SHRT_MAX)
                {
                    memory.kills++;
                }
            }

            Player_m.playerGainKillExperience(creature);

            // can't call displayCharacterExperience() here, as that would result in "new level"
            // message appearing before "monster dies" message.
            var m_take_hit = (int)monster.creature_id;

            // in case this is called from within updateMonsters(), this is a horrible
            // hack, the monsters/updateMonsters() code needs to be rewritten.
            if (State.Instance.hack_monptr < monster_id)
            {
                this.dungeon.dungeonDeleteMonster(monster_id);
            }
            else
            {
                this.dungeon.dungeonDeleteMonsterFix1(monster_id);
            }

            return m_take_hit;
        }
    }
}