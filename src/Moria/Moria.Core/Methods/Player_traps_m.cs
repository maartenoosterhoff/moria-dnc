using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_move_m;

namespace Moria.Core.Methods
{
    public static class Player_traps_m
    {
        public static void SetDependencies(
            IDice dice,
            IDungeon dungeon,
            IGame game,
            IMonsterManager monsterManager,
            IRnd rnd
        )
        {
            Player_traps_m.dice = dice;
            Player_traps_m.dungeon = dungeon;
            Player_traps_m.game = game;
            Player_traps_m.monsterManager = monsterManager;
            Player_traps_m.rnd = rnd;
        }

        private static IDice dice;
        private static IDungeon dungeon;
        private static IGame game;
        private static IMonsterManager monsterManager;
        private static IRnd rnd;

        private static int playerTrapDisarmAbility()
        {
            var py = State.Instance.py;

            var ability = py.misc.disarm;
            ability += 2;
            ability *= playerDisarmAdjustment();
            ability += playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT);
            ability += Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DISARM] * (int)py.misc.level / 3;

            if (py.flags.blind > 0 || playerNoLight())
            {
                ability = ability / 10;
            }

            if (py.flags.confused > 0)
            {
                ability = ability / 10;
            }

            if (py.flags.image > 0)
            {
                ability = ability / 10;
            }

            return ability;
        }

        private static void playerDisarmFloorTrap(Coord_t coord, int total, int level, int dir, int misc_use)
        {
            var py = State.Instance.py;

            var confused = py.flags.confused;

            if (total + 100 - level > rnd.randomNumber(100))
            {
                printMessage("You have disarmed the trap.");
                py.misc.exp += misc_use;
                dungeon.dungeonDeleteObject(coord);

                // make sure we move onto the trap even if confused
                py.flags.confused = 0;
                playerMove(dir, false);
                py.flags.confused = (int)confused;

                displayCharacterExperience();
                return;
            }

            // avoid rnd.randomNumber(0) call
            if (total > 5 && rnd.randomNumber(total) > 5)
            {
                printMessageNoCommandInterrupt("You failed to disarm the trap.");
                return;
            }

            printMessage("You set the trap off!");

            // make sure we move onto the trap even if confused
            py.flags.confused = 0;
            playerMove(dir, false);
            py.flags.confused += confused;
        }

        private static void playerDisarmChestTrap(Coord_t coord, int total, Inventory_t item)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            if (!spellItemIdentified(item))
            {
                game.player_free_turn = true;
                printMessage("I don't see a trap.");

                return;
            }

            if ((item.flags & Config.treasure_chests.CH_TRAPPED) != 0u)
            {
                var level = (int)item.depth_first_found;

                if ((total - level) > rnd.randomNumber(100))
                {
                    item.flags &= ~Config.treasure_chests.CH_TRAPPED;

                    if ((item.flags & Config.treasure_chests.CH_LOCKED) != 0u)
                    {
                        item.special_name_id = (int)SpecialNameIds.SN_LOCKED;
                    }
                    else
                    {
                        item.special_name_id = (int)SpecialNameIds.SN_DISARMED;
                    }

                    printMessage("You have disarmed the chest.");

                    spellItemIdentifyAndRemoveRandomInscription(item);
                    py.misc.exp += level;

                    displayCharacterExperience();
                }
                else if ((total > 5) && (rnd.randomNumber(total) > 5))
                {
                    printMessageNoCommandInterrupt("You failed to disarm the chest.");
                }
                else
                {
                    printMessage("You set a trap off!");
                    spellItemIdentifyAndRemoveRandomInscription(item);
                    chestTrap(coord);
                }
                return;
            }

            printMessage("The chest was not trapped.");
            game.player_free_turn = true;
        }

        // Disarms a trap -RAK-
        public static void playerDisarmTrap()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            var dir = 0;
            if (!Player_traps_m.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
            {
                return;
            }

            var coord = py.pos.Clone();
            playerMovePosition(dir, ref coord);

            var tile = dg.floor[coord.y][coord.x];

            var no_disarm = false;

            if (tile.creature_id > 1 && tile.treasure_id != 0 &&
                (game.treasure.list[tile.treasure_id].category_id == TV_VIS_TRAP || game.treasure.list[tile.treasure_id].category_id == TV_CHEST))
            {
                objectBlockedByMonster((int)tile.creature_id);
            }
            else if (tile.treasure_id != 0)
            {
                var disarm_ability = playerTrapDisarmAbility();

                var item = game.treasure.list[tile.treasure_id];

                if (item.category_id == TV_VIS_TRAP)
                {
                    playerDisarmFloorTrap(coord, disarm_ability, (int)item.depth_first_found, dir, item.misc_use);
                }
                else if (item.category_id == TV_CHEST)
                {
                    playerDisarmChestTrap(coord, disarm_ability, item);
                }
                else
                {
                    no_disarm = true;
                }
            }
            else
            {
                no_disarm = true;
            }

            if (no_disarm)
            {
                printMessage("I do not see anything to disarm there.");
                game.player_free_turn = true;
            }
        }

        private static void chestLooseStrength()
        {
            var py = State.Instance.py;

            printMessage("A small needle has pricked you!");

            if (py.flags.sustain_str)
            {
                printMessage("You are unaffected.");
                return;
            }

            playerStatRandomDecrease((int)PlayerAttr.STR);

            playerTakesHit(dice.diceRoll(new Dice_t(1, 4)), "a poison needle");

            printMessage("You feel weakened!");
        }

        private static void chestPoison()
        {
            printMessage("A small needle has pricked you!");

            playerTakesHit(dice.diceRoll(new Dice_t(1, 6)), "a poison needle");

            State.Instance.py.flags.poisoned += 10 + rnd.randomNumber(20);
        }

        private static void chestParalysed()
        {
            var py = State.Instance.py;
            printMessage("A puff of yellow gas surrounds you!");

            if (py.flags.free_action)
            {
                printMessage("You are unaffected.");
                return;
            }

            printMessage("You choke and pass out.");
            py.flags.paralysis = (int)(10 + rnd.randomNumber(20));
        }

        private static void chestSummonMonster(Coord_t coord)
        {
            var position = new Coord_t(0, 0);

            for (var i = 0; i < 3; i++)
            {
                position.y = coord.y;
                position.x = coord.x;
                monsterManager.monsterSummon(position, false);
            }
        }

        private static void chestExplode(Coord_t coord)
        {
            printMessage("There is a sudden explosion!");

            dungeon.dungeonDeleteObject(coord);

            playerTakesHit(dice.diceRoll(new Dice_t(5, 8)), "an exploding chest");
        }

        // Chests have traps too. -RAK-
        // Note: Chest traps are based on the FLAGS value
        public static void chestTrap(Coord_t coord)
        {
            var game = State.Instance.game;
            var dg = State.Instance.dg;
            var flags = game.treasure.list[dg.floor[coord.y][coord.x].treasure_id].flags;

            if ((flags & Config.treasure_chests.CH_LOSE_STR) != 0u)
            {
                chestLooseStrength();
            }

            if ((flags & Config.treasure_chests.CH_POISON) != 0u)
            {
                chestPoison();
            }

            if ((flags & Config.treasure_chests.CH_PARALYSED) != 0u)
            {
                chestParalysed();
            }

            if ((flags & Config.treasure_chests.CH_SUMMON) != 0u)
            {
                chestSummonMonster(coord);
            }

            if ((flags & Config.treasure_chests.CH_EXPLODE) != 0u)
            {
                chestExplode(coord);
            }
        }

    }
}
