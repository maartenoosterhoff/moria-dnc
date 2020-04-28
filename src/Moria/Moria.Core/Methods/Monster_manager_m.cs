using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using System;
using Moria.Core.Data;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Methods.Ui_io_m;

namespace Moria.Core.Methods
{
    public interface IMonsterManager
    {
        bool monsterSummon(Coord_t coord, bool sleeping);
        void monsterPlaceWinning();
        void monsterPlaceNewWithinDistance(int number, int distance_from_source, bool sleeping);
        bool monsterSummonUndead(Coord_t coord);
        bool monsterPlaceNew(Coord_t coord, int creature_id, bool sleeping);
        bool compactMonsters();
    }

    public class Monster_manager_m : IMonsterManager
    {
        public Monster_manager_m(
            IDice dice,
            IDungeon dungeon,
            IRnd rnd,
            IStd std
        )
        {
            this.dice = dice;
            this.dungeon = dungeon;
            this.rnd = rnd;
            this.std = std;
        }

        private readonly IDice dice;
        private readonly IDungeon dungeon;
        private readonly IRnd rnd;
        private readonly IStd std;

        // Returns a pointer to next free space -RAK-
        // Returns -1 if could not allocate a monster.
        private int popm()
        {
            if (State.Instance.next_free_monster_id == MON_TOTAL_ALLOCATIONS)
            {
                if (!compactMonsters())
                {
                    return -1;
                }
            }

            State.Instance.next_free_monster_id++;
            return State.Instance.next_free_monster_id;
        }

        // Places a monster at given location -RAK-
        public bool monsterPlaceNew(Coord_t coord, int creature_id, bool sleeping)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            var monster_id = popm();

            // Check for case where could not allocate space for the monster
            if (monster_id == -1)
            {
                return false;
            }

            var monster = State.Instance.monsters[monster_id];
            var creatures_list = Library.Instance.Creatures.creatures_list;

            monster.pos.y = coord.y;
            monster.pos.x = coord.x;
            monster.creature_id = (uint)creature_id;

            if ((creatures_list[creature_id].defenses & Config.monsters_defense.CD_MAX_HP) != 0)
            {
                monster.hp = dice.maxDiceRoll(creatures_list[creature_id].hit_die);
            }
            else
            {
                monster.hp = dice.diceRoll(creatures_list[creature_id].hit_die);
            }

            // the creatures_list[] speed value is 10 greater, so that it can be a uint8_t
            monster.speed = ((int)creatures_list[creature_id].speed - 10 + py.flags.speed);
            monster.stunned_amount = 0;
            monster.distance_from_player = (uint)dungeon.coordDistanceBetween(py.pos, coord);
            monster.lit = false;

            dg.floor[coord.y][coord.x].creature_id = (uint)monster_id;

            if (sleeping)
            {
                if (creatures_list[creature_id].sleep_counter == 0)
                {
                    monster.sleep_count = 0;
                }
                else
                {
                    monster.sleep_count = (int)((creatures_list[creature_id].sleep_counter * 2) + rnd.randomNumber((int)creatures_list[creature_id].sleep_counter * 10));
                }
            }
            else
            {
                monster.sleep_count = 0;
            }

            return true;
        }

        // Places a monster at given location -RAK-
        public void monsterPlaceWinning()
        {
            var game = State.Instance.game;
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            if (game.total_winner)
            {
                return;
            }

            var coord = new Coord_t(0, 0);

            do
            {
                coord.y = rnd.randomNumber(dg.height - 2);
                coord.x = rnd.randomNumber(dg.width - 2);
            } while (dg.floor[coord.y][coord.x].feature_id >= MIN_CLOSED_SPACE ||           //
                     dg.floor[coord.y][coord.x].creature_id != 0 ||                         //
                     dg.floor[coord.y][coord.x].treasure_id != 0 ||                         //
                     dungeon.coordDistanceBetween(coord, py.pos) <= Config.monsters.MON_MAX_SIGHT //
            );

            var creature_id = rnd.randomNumber(Config.monsters.MON_ENDGAME_MONSTERS) - 1 + State.Instance.monster_levels[MON_MAX_LEVELS];

            // TODO: duplicate code -MRC-
            // The following code is now exactly the same as monsterPlaceNew() except here
            // we `abort()` on failed placement, and do not set `monster->lit = false`.
            // Perhaps we can find a way to call `monsterPlaceNew()` instead of
            // duplicating everything here.

            var monster_id = popm();

            // Check for case where could not allocate space for the win monster, this should never happen.
            if (monster_id == -1)
            {
                throw new InvalidOperationException();
                //abort();
            }

            var monster = State.Instance.monsters[monster_id];

            monster.pos.y = coord.y;
            monster.pos.x = coord.x;
            monster.creature_id = (uint)creature_id;

            var creatures_list = Library.Instance.Creatures.creatures_list;

            if ((creatures_list[creature_id].defenses & Config.monsters_defense.CD_MAX_HP) != 0)
            {
                monster.hp = dice.maxDiceRoll(creatures_list[creature_id].hit_die);
            }
            else
            {
                monster.hp = dice.diceRoll(creatures_list[creature_id].hit_die);
            }

            // the creatures_list speed value is 10 greater, so that it can be a uint8_t
            monster.speed = ((int)creatures_list[creature_id].speed - 10 + py.flags.speed);
            monster.stunned_amount = 0;
            monster.distance_from_player = (uint)dungeon.coordDistanceBetween(py.pos, coord);

            dg.floor[coord.y][coord.x].creature_id = (uint)monster_id;

            monster.sleep_count = 0;
        }

        // Return a monster suitable to be placed at a given level. This
        // makes high level monsters (up to the given level) slightly more
        // common than low level monsters at any given level. -CJS-
        private int monsterGetOneSuitableForLevel(int level)
        {
            var monster_levels = State.Instance.monster_levels;
            if (level == 0)
            {
                return rnd.randomNumber(monster_levels[0]) - 1;
            }

            if (level > MON_MAX_LEVELS)
            {
                level = (int)MON_MAX_LEVELS;
            }

            if (rnd.randomNumber(Config.monsters.MON_CHANCE_OF_NASTY) == 1)
            {
                var abs_distribution = std.std_abs(std.std_intmax_t(rnd.randomNumberNormalDistribution(0, 4)));
                level += abs_distribution + 1;
                if (level > MON_MAX_LEVELS)
                {
                    level = (int)MON_MAX_LEVELS;
                }
            }
            else
            {
                // This code has been added to make it slightly more likely to get
                // the higher level monsters. Originally a uniform distribution over
                // all monsters of level less than or equal to the dungeon level.
                // This distribution makes a level n monster occur approx 2/n% of the
                // time on level n, and 1/n*n% are 1st level.
                var num = monster_levels[level] - monster_levels[0];
                var i = rnd.randomNumber(num) - 1;
                var j = rnd.randomNumber(num) - 1;
                if (j > i)
                {
                    i = j;
                }

                level = (int)Library.Instance.Creatures.creatures_list[i + monster_levels[0]].level;
            }

            return rnd.randomNumber(monster_levels[level] - monster_levels[level - 1]) - 1 + monster_levels[level - 1];
        }

        // Allocates a random monster -RAK-
        public void monsterPlaceNewWithinDistance(int number, int distance_from_source, bool sleeping)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var creatures_list = Library.Instance.Creatures.creatures_list;

            var position = new Coord_t(0, 0);

            for (var i = 0; i < number; i++)
            {
                do
                {
                    position.y = rnd.randomNumber(dg.height - 2);
                    position.x = rnd.randomNumber(dg.width - 2);
                } while (dg.floor[position.y][position.x].feature_id >= MIN_CLOSED_SPACE || //
                         dg.floor[position.y][position.x].creature_id != 0 ||               //
                         dungeon.coordDistanceBetween(position, py.pos) <= distance_from_source     //
                );

                var l = monsterGetOneSuitableForLevel(dg.current_level);

                // Dragons are always created sleeping here,
                // so as to give the player a sporting chance.
                if (creatures_list[l].sprite == 'd' || creatures_list[l].sprite == 'D')
                {
                    sleeping = true;
                }

                // Place_monster() should always return true here.
                // It does not matter if it fails though.
                monsterPlaceNew(position, l, sleeping);
            }
        }

        private bool placeMonsterAdjacentTo(int monster_id, Coord_t coord, bool slp)
        {
            var dg = State.Instance.dg;
            var placed = false;

            var position = new Coord_t(0, 0);

            for (var i = 0; i <= 9; i++)
            {
                position.y = coord.y - 2 + rnd.randomNumber(3);
                position.x = coord.x - 2 + rnd.randomNumber(3);

                if (dungeon.coordInBounds(position))
                {
                    if (dg.floor[position.y][position.x].feature_id <= MAX_OPEN_SPACE &&
                        dg.floor[position.y][position.x].creature_id == 0)
                    {
                        // Place_monster() should always return true here.
                        if (!monsterPlaceNew(position, monster_id, slp))
                        {
                            return false;
                        }

                        coord.y = position.y;
                        coord.x = position.x;

                        placed = true;
                        i = 9;
                    }
                }
            }

            return placed;
        }

        // Places creature adjacent to given location -RAK-
        public bool monsterSummon(Coord_t coord, bool sleeping)
        {
            var dg = State.Instance.dg;

            var monster_id = monsterGetOneSuitableForLevel(dg.current_level + (int)Config.monsters.MON_SUMMONED_LEVEL_ADJUST);
            return placeMonsterAdjacentTo(monster_id, coord, sleeping);
        }

        // Places undead adjacent to given location -RAK-
        public bool monsterSummonUndead(Coord_t coord)
        {
            int monster_id;
            var max_levels = State.Instance.monster_levels[MON_MAX_LEVELS];

            do
            {
                monster_id = rnd.randomNumber(max_levels) - 1;
                for (var i = 0; i <= 19;)
                {
                    if ((Library.Instance.Creatures.creatures_list[monster_id].defenses & Config.monsters_defense.CD_UNDEAD) != 0)
                    {
                        i = 20;
                        max_levels = 0;
                    }
                    else
                    {
                        monster_id++;
                        if (monster_id > max_levels)
                        {
                            i = 20;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            } while (max_levels != 0);

            return placeMonsterAdjacentTo(monster_id, coord, false);
        }

        // Compact monsters -RAK-
        // Return true if any monsters were deleted, false if could not delete any monsters.
        public bool compactMonsters()
        {
            printMessage("Compacting monsters...");

            var cur_dis = 66;

            var delete_any = false;
            while (!delete_any)
            {
                for (var i = State.Instance.next_free_monster_id - 1; i >= Config.monsters.MON_MIN_INDEX_ID; i--)
                {
                    if (cur_dis < State.Instance.monsters[i].distance_from_player && rnd.randomNumber(3) == 1)
                    {
                        if ((Library.Instance.Creatures.creatures_list[(int)State.Instance.monsters[i].creature_id].movement & Config.monsters_move.CM_WIN) != 0u)
                        {
                            // Never compact away the Balrog!!
                        }
                        else if (State.Instance.hack_monptr < i)
                        {
                            // in case this is called from within updateMonsters(), this is a horrible
                            // hack, the monsters/updateMonsters() code needs to be rewritten.
                            dungeon.dungeonDeleteMonster(i);
                            delete_any = true;
                        }
                        else
                        {
                            // dungeonDeleteMonsterFix1() does not decrement next_free_monster_id,
                            // so don't set delete_any if this was called.
                            dungeon.dungeonDeleteMonsterFix1(i);
                        }
                    }
                }

                if (!delete_any)
                {
                    cur_dis -= 6;

                    // Can't delete any monsters, return failure.
                    if (cur_dis < 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}
