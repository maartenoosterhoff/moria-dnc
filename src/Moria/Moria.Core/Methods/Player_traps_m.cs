using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_move_m;

namespace Moria.Core.Methods
{
    public interface IPlayerTraps
    {
        void playerDisarmTrap();
        void chestTrap(Coord_t coord);
    }

    public class Player_traps_m : IPlayerTraps
    {
        private readonly IDice dice;
        private readonly IDungeon dungeon;
        private readonly IEventPublisher eventPublisher;
        private readonly IGame game;
        private readonly IHelpers helpers;
        private readonly IIdentification identification;
        private readonly IMonsterManager monsterManager;
        private readonly IRnd rnd;
        private readonly ITerminal terminal;
        private readonly ITerminalEx terminalEx;

        public Player_traps_m(
            IDice dice,
            IDungeon dungeon,
            IEventPublisher eventPublisher,
            IGame game,
            IHelpers helpers,
            IIdentification identification,
            IMonsterManager monsterManager,
            IRnd rnd,
            ITerminal terminal,
            ITerminalEx terminalEx
        )
        {
            this.dice = dice;
            this.dungeon = dungeon;
            this.eventPublisher = eventPublisher;
            this.game = game;
            this.helpers = helpers;
            this.identification = identification;
            this.monsterManager = monsterManager;
            this.rnd = rnd;
            this.terminal = terminal;
            this.terminalEx = terminalEx;
        }

        private int playerTrapDisarmAbility()
        {
            var py = State.Instance.py;

            var ability = py.misc.disarm;
            ability += 2;
            ability *= playerDisarmAdjustment();
            ability += playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT);
            ability += Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DISARM] * (int)py.misc.level / 3;

            if (py.flags.blind > 0 || this.helpers.playerNoLight())
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

        private void playerDisarmFloorTrap(Coord_t coord, int total, int level, int dir, int misc_use)
        {
            var py = State.Instance.py;

            var confused = py.flags.confused;

            if (total + 100 - level > this.rnd.randomNumber(100))
            {
                this.terminal.printMessage("You have disarmed the trap.");
                py.misc.exp += misc_use;
                this.dungeon.dungeonDeleteObject(coord);

                // make sure we move onto the trap even if confused
                py.flags.confused = 0;
                playerMove(dir, false);
                py.flags.confused = (int)confused;

                this.terminalEx.displayCharacterExperience();
                return;
            }

            // avoid rnd.randomNumber(0) call
            if (total > 5 && this.rnd.randomNumber(total) > 5)
            {
                this.terminal.printMessageNoCommandInterrupt("You failed to disarm the trap.");
                return;
            }

            this.terminal.printMessage("You set the trap off!");

            // make sure we move onto the trap even if confused
            py.flags.confused = 0;
            playerMove(dir, false);
            py.flags.confused += confused;
        }

        private void playerDisarmChestTrap(Coord_t coord, int total, Inventory_t item)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            if (!this.identification.spellItemIdentified(item))
            {
                game.player_free_turn = true;
                this.terminal.printMessage("I don't see a trap.");

                return;
            }

            if ((item.flags & Config.treasure_chests.CH_TRAPPED) != 0u)
            {
                var level = (int)item.depth_first_found;

                if (total - level > this.rnd.randomNumber(100))
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

                    this.terminal.printMessage("You have disarmed the chest.");

                    this.identification.spellItemIdentifyAndRemoveRandomInscription(item);
                    py.misc.exp += level;

                    this.terminalEx.displayCharacterExperience();
                }
                else if (total > 5 && this.rnd.randomNumber(total) > 5)
                {
                    this.terminal.printMessageNoCommandInterrupt("You failed to disarm the chest.");
                }
                else
                {
                    this.terminal.printMessage("You set a trap off!");
                    this.identification.spellItemIdentifyAndRemoveRandomInscription(item);
                    this.chestTrap(coord);
                }
                return;
            }

            this.terminal.printMessage("The chest was not trapped.");
            game.player_free_turn = true;
        }

        // Disarms a trap -RAK-
        public void playerDisarmTrap()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            var dir = 0;
            if (!this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
            {
                return;
            }

            var coord = py.pos.Clone();
            this.helpers.movePosition(dir, ref coord);

            var tile = dg.floor[coord.y][coord.x];

            var no_disarm = false;

            if (tile.creature_id > 1 && tile.treasure_id != 0 &&
                (game.treasure.list[tile.treasure_id].category_id == TV_VIS_TRAP || game.treasure.list[tile.treasure_id].category_id == TV_CHEST))
            {
                this.identification.objectBlockedByMonster((int)tile.creature_id);
            }
            else if (tile.treasure_id != 0)
            {
                var disarm_ability = this.playerTrapDisarmAbility();

                var item = game.treasure.list[tile.treasure_id];

                if (item.category_id == TV_VIS_TRAP)
                {
                    this.playerDisarmFloorTrap(coord, disarm_ability, (int)item.depth_first_found, dir, item.misc_use);
                }
                else if (item.category_id == TV_CHEST)
                {
                    this.playerDisarmChestTrap(coord, disarm_ability, item);
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
                this.terminal.printMessage("I do not see anything to disarm there.");
                game.player_free_turn = true;
            }
        }

        private void chestLooseStrength()
        {
            var py = State.Instance.py;

            this.terminal.printMessage("A small needle has pricked you!");

            if (py.flags.sustain_str)
            {
                this.terminal.printMessage("You are unaffected.");
                return;
            }

            this.eventPublisher.Publish(new StatRandomDecreaseCommand((int)PlayerAttr.STR));
            //playerStatRandomDecrease((int)PlayerAttr.STR);

            playerTakesHit(this.dice.diceRoll(new Dice_t(1, 4)), "a poison needle");

            this.terminal.printMessage("You feel weakened!");
        }

        private void chestPoison()
        {
            this.terminal.printMessage("A small needle has pricked you!");

            playerTakesHit(this.dice.diceRoll(new Dice_t(1, 6)), "a poison needle");

            State.Instance.py.flags.poisoned += 10 + this.rnd.randomNumber(20);
        }

        private void chestParalysed()
        {
            var py = State.Instance.py;
            this.terminal.printMessage("A puff of yellow gas surrounds you!");

            if (py.flags.free_action)
            {
                this.terminal.printMessage("You are unaffected.");
                return;
            }

            this.terminal.printMessage("You choke and pass out.");
            py.flags.paralysis = (int)(10 + this.rnd.randomNumber(20));
        }

        private void chestSummonMonster(Coord_t coord)
        {
            var position = new Coord_t(0, 0);

            for (var i = 0; i < 3; i++)
            {
                position.y = coord.y;
                position.x = coord.x;
                this.monsterManager.monsterSummon(position, false);
            }
        }

        private void chestExplode(Coord_t coord)
        {
            this.terminal.printMessage("There is a sudden explosion!");

            this.dungeon.dungeonDeleteObject(coord);

            playerTakesHit(this.dice.diceRoll(new Dice_t(5, 8)), "an exploding chest");
        }

        // Chests have traps too. -RAK-
        // Note: Chest traps are based on the FLAGS value
        public void chestTrap(Coord_t coord)
        {
            var game = State.Instance.game;
            var dg = State.Instance.dg;
            var flags = game.treasure.list[dg.floor[coord.y][coord.x].treasure_id].flags;

            if ((flags & Config.treasure_chests.CH_LOSE_STR) != 0u)
            {
                this.chestLooseStrength();
            }

            if ((flags & Config.treasure_chests.CH_POISON) != 0u)
            {
                this.chestPoison();
            }

            if ((flags & Config.treasure_chests.CH_PARALYSED) != 0u)
            {
                this.chestParalysed();
            }

            if ((flags & Config.treasure_chests.CH_SUMMON) != 0u)
            {
                this.chestSummonMonster(coord);
            }

            if ((flags & Config.treasure_chests.CH_EXPLODE) != 0u)
            {
                this.chestExplode(coord);
            }
        }
    }
}
