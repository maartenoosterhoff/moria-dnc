using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dice_m;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Monster_manager_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_move_m;

namespace Moria.Core.Methods
{
    public static class Player_traps_m
    {
        static int playerTrapDisarmAbility()
        {
            var py = State.Instance.py;

            int ability = py.misc.disarm;
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

        public static void playerDisarmFloorTrap(Coord_t coord, int total, int level, int dir, int misc_use)
        {
            var py = State.Instance.py;

            int confused = py.flags.confused;

            if (total + 100 - level > randomNumber(100))
            {
                printMessage("You have disarmed the trap.");
                py.misc.exp += misc_use;
                dungeonDeleteObject(coord);

                // make sure we move onto the trap even if confused
                py.flags.confused = 0;
                playerMove(dir, false);
                py.flags.confused = (int)confused;

                displayCharacterExperience();
                return;
            }

            // avoid randomNumber(0) call
            if (total > 5 && randomNumber(total) > 5)
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

        public static void playerDisarmChestTrap(Coord_t coord, int total, Inventory_t item)
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
                int level = (int)item.depth_first_found;

                if ((total - level) > randomNumber(100))
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
                else if ((total > 5) && (randomNumber(total) > 5))
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

            int dir = 0;
            if (!getDirectionWithMemory(/*CNIL*/null, ref dir))
            {
                return;
            }

            Coord_t coord = py.pos;
            playerMovePosition(dir, coord);

            var tile = dg.floor[coord.y][coord.x];

            bool no_disarm = false;

            if (tile.creature_id > 1 && tile.treasure_id != 0 &&
                (game.treasure.list[tile.treasure_id].category_id == TV_VIS_TRAP || game.treasure.list[tile.treasure_id].category_id == TV_CHEST))
            {
                objectBlockedByMonster((int)tile.creature_id);
            }
            else if (tile.treasure_id != 0)
            {
                int disarm_ability = playerTrapDisarmAbility();

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

        static void chestLooseStrength()
        {
            var py = State.Instance.py;

            printMessage("A small needle has pricked you!");

            if (py.flags.sustain_str)
            {
                printMessage("You are unaffected.");
                return;
            }

            playerStatRandomDecrease((int)PlayerAttr.STR);

            playerTakesHit(diceRoll(new Dice_t(1, 4)), "a poison needle");

            printMessage("You feel weakened!");
        }

        public static void chestPoison()
        {
            printMessage("A small needle has pricked you!");

            playerTakesHit(diceRoll(new Dice_t(1, 6)), "a poison needle");

            State.Instance.py.flags.poisoned += 10 + randomNumber(20);
        }

        public static void chestParalysed()
        {
            var py = State.Instance.py;
            printMessage("A puff of yellow gas surrounds you!");

            if (py.flags.free_action)
            {
                printMessage("You are unaffected.");
                return;
            }

            printMessage("You choke and pass out.");
            py.flags.paralysis = (int)(10 + randomNumber(20));
        }

        public static void chestSummonMonster(Coord_t coord)
        {
            Coord_t position = new Coord_t(0, 0);

            for (int i = 0; i < 3; i++)
            {
                position.y = coord.y;
                position.x = coord.x;
                monsterSummon(position, false);
            }
        }

        public static void chestExplode(Coord_t coord)
        {
            printMessage("There is a sudden explosion!");

            dungeonDeleteObject(coord);

            playerTakesHit(diceRoll(new Dice_t(5, 8)), "an exploding chest");
        }

        // Chests have traps too. -RAK-
        // Note: Chest traps are based on the FLAGS value
        public static void chestTrap(Coord_t coord)
        {
            var game = State.Instance.game;
            var dg = State.Instance.dg;
            uint flags = game.treasure.list[dg.floor[coord.y][coord.x].treasure_id].flags;

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
