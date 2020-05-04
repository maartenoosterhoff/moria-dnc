using Moria.Core.Configs;
using Moria.Core.Methods.Commands.SpellCasting;
using Moria.Core.Methods.Commands.SpellCasting.Defending;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Constants.Std_c;
using static Moria.Core.Methods.Player_eat_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_stats_m;

namespace Moria.Core.Methods
{
    public static class Player_quaff_m
    {
        public static void SetDependencies(
            IDice dice,
            IHelpers helpers,
            IInventoryManager inventoryManager,
            IPlayerMagic playerMagic,
            IRnd rnd,
            IUiInventory uiInventory,

            IEventPublisher eventPublisher
        )
        {
            Player_quaff_m.dice = dice;
            Player_quaff_m.helpers = helpers;
            Player_quaff_m.inventoryManager = inventoryManager;
            Player_quaff_m.playerMagic = playerMagic;
            Player_quaff_m.rnd = rnd;
            Player_quaff_m.uiInventory = uiInventory;

            Player_quaff_m.eventPublisher = eventPublisher;
        }

        private static IDice dice;
        private static IHelpers helpers;
        private static IInventoryManager inventoryManager;
        private static IPlayerMagic playerMagic;
        private static IRnd rnd;
        private static IUiInventory uiInventory;

        private static IEventPublisher eventPublisher;

        static bool playerDrinkPotion(uint flags, uint item_type)
        {
            var py = State.Instance.py;
            var identified = false;

            while (flags != 0)
            {
                var potion_id = helpers.getAndClearFirstBit(ref flags) + 1;

                if (item_type == TV_POTION2)
                {
                    potion_id += 32;
                }

                // Potions
                switch ((PotionSpellTypes)potion_id)
                {
                    case PotionSpellTypes.Strength:
                        if (playerStatRandomIncrease((int)PlayerAttr.STR))
                        {
                            printMessage("Wow!  What bulging muscles!");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.Weakness:
                        eventPublisher.Publish(new LoseStrCommand());
                        //spellLoseSTR();
                        identified = true;
                        break;
                    case PotionSpellTypes.RestoreStrength:
                        if (playerStatRestore((int)PlayerAttr.STR))
                        {
                            printMessage("You feel warm all over.");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.Intelligence:
                        if (playerStatRandomIncrease((int)PlayerAttr.INT))
                        {
                            printMessage("Aren't you brilliant!");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.LoseIntelligence:
                        eventPublisher.Publish(new LoseIntCommand());
                        //spellLoseINT();
                        identified = true;
                        break;
                    case PotionSpellTypes.RestoreIntelligence:
                        if (playerStatRestore((int)PlayerAttr.INT))
                        {
                            printMessage("You have have a warm feeling.");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.Wisdom:
                        if (playerStatRandomIncrease((int)PlayerAttr.WIS))
                        {
                            printMessage("You suddenly have a profound thought!");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.LoseWisdom:
                        eventPublisher.Publish(new LoseWisCommand());
                        //spellLoseWIS();
                        identified = true;
                        break;
                    case PotionSpellTypes.RestoreWisdom:
                        if (playerStatRestore((int)PlayerAttr.WIS))
                        {
                            printMessage("You feel your wisdom returning.");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.Charisma:
                        if (playerStatRandomIncrease((int)PlayerAttr.CHR))
                        {
                            printMessage("Gee, ain't you cute!");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.Ugliness:
                        eventPublisher.Publish(new LoseChrCommand());
                        //spellLoseCHR();
                        identified = true;
                        break;
                    case PotionSpellTypes.RestoreCharisma:
                        if (playerStatRestore((int)PlayerAttr.CHR))
                        {
                            printMessage("You feel your looks returning.");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.CureLightWounds:
                        identified = eventPublisher.PublishWithOutputBool(new ChangePlayerHitPointsCommand(
                            dice.diceRoll(new Dice_t(2, 7))
                        ));
                        //identified = spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(2, 7)));
                        break;
                    case PotionSpellTypes.CureSeriousWounds:
                        identified = eventPublisher.PublishWithOutputBool(new ChangePlayerHitPointsCommand(
                            dice.diceRoll(new Dice_t(4, 7))
                        ));
                        //identified = spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(4, 7)));
                        break;
                    case PotionSpellTypes.CureCriticalWounds:
                        identified = eventPublisher.PublishWithOutputBool(new ChangePlayerHitPointsCommand(
                            dice.diceRoll(new Dice_t(6, 7))
                        ));
                        //identified = spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(6, 7)));
                        break;
                    case PotionSpellTypes.Healing:
                        identified = eventPublisher.PublishWithOutputBool(new ChangePlayerHitPointsCommand(
                            1000
                        ));
                        //identified = spellChangePlayerHitPoints(1000);
                        break;
                    case PotionSpellTypes.Constitution:
                        if (playerStatRandomIncrease((int)PlayerAttr.CON))
                        {
                            printMessage("You feel tingly for a moment.");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.GainExperience:
                        if (py.misc.exp < Config.player.PLAYER_MAX_EXP)
                        {
                            var exp = (uint)(py.misc.exp / 2 + 10);
                            if (exp > 100000u)
                            {
                                exp = 100000u;
                            }
                            py.misc.exp += (int)exp;

                            printMessage("You feel more experienced.");
                            displayCharacterExperience();
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.Sleep:
                        if (!py.flags.free_action)
                        {
                            // paralysis must == 0, otherwise could not drink potion
                            printMessage("You fall asleep.");
                            py.flags.paralysis += rnd.randomNumber(4) + 4;
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.Blindness:
                        if (py.flags.blind == 0)
                        {
                            printMessage("You are covered by a veil of darkness.");
                            identified = true;
                        }
                        py.flags.blind += rnd.randomNumber(100) + 100;
                        break;
                    case PotionSpellTypes.Confusion:
                        if (py.flags.confused == 0)
                        {
                            printMessage("Hey!  This is good stuff!  * Hick! *");
                            identified = true;
                        }
                        py.flags.confused += rnd.randomNumber(20) + 12;
                        break;
                    case PotionSpellTypes.Poison:
                        if (py.flags.poisoned == 0)
                        {
                            printMessage("You feel very sick.");
                            identified = true;
                        }
                        py.flags.poisoned += rnd.randomNumber(15) + 10;
                        break;
                    case PotionSpellTypes.HasteSelf:
                        if (py.flags.fast == 0)
                        {
                            identified = true;
                        }
                        py.flags.fast += rnd.randomNumber(25) + 15;
                        break;
                    case PotionSpellTypes.Slowness:
                        if (py.flags.slow == 0)
                        {
                            identified = true;
                        }
                        py.flags.slow += rnd.randomNumber(25) + 15;
                        break;
                    case PotionSpellTypes.Dexterity:
                        if (playerStatRandomIncrease((int)PlayerAttr.DEX))
                        {
                            printMessage("You feel more limber!");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.RestoreDexterity:
                        if (playerStatRestore((int)PlayerAttr.DEX))
                        {
                            printMessage("You feel less clumsy.");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.RestoreConstitution:
                        if (playerStatRestore((int)PlayerAttr.CON))
                        {
                            printMessage("You feel your health returning!");
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.CureBlindness:
                        identified = playerMagic.playerCureBlindness();
                        break;
                    case PotionSpellTypes.CureConfusion:
                        identified = playerMagic.playerCureConfusion();
                        break;
                    case PotionSpellTypes.CurePoison:
                        identified = playerMagic.playerCurePoison();
                        break;
                    // case 33: break; // this is no longer useful, now that there is a 'G'ain magic spells command
                    case PotionSpellTypes.LoseExperience:
                        if (py.misc.exp > 0)
                        {
                            printMessage("You feel your memories fade.");

                            // Lose between 1/5 and 2/5 of your experience
                            var exp = py.misc.exp / 5;

                            if (py.misc.exp > SHRT_MAX)
                            {
                                const int intMax = +2147483647;
                                var scale = (int)(intMax / py.misc.exp);
                                exp += rnd.randomNumber((int)scale) * py.misc.exp / (scale * 5);
                            }
                            else
                            {
                                exp += rnd.randomNumber((int)py.misc.exp) / 5;
                            }
                            eventPublisher.Publish(new LoseExpCommand(exp));
                            //spellLoseEXP(exp);
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.SaltWater:
                        playerMagic.playerCurePoison();
                        if (py.flags.food > 150)
                        {
                            py.flags.food = 150;
                        }
                        py.flags.paralysis = 4;

                        printMessage("The potion makes you vomit!");
                        identified = true;
                        break;
                    case PotionSpellTypes.Invulnerability:
                        if (py.flags.invulnerability == 0)
                        {
                            identified = true;
                        }
                        py.flags.invulnerability += rnd.randomNumber(10) + 10;
                        break;
                    case PotionSpellTypes.Heroism:
                        if (py.flags.heroism == 0)
                        {
                            identified = true;
                        }
                        py.flags.heroism += rnd.randomNumber(25) + 25;
                        break;
                    case PotionSpellTypes.SuperHeroism:
                        if (py.flags.super_heroism == 0)
                        {
                            identified = true;
                        }
                        py.flags.super_heroism += rnd.randomNumber(25) + 25;
                        break;
                    case PotionSpellTypes.Boldness:
                        identified = playerMagic.playerRemoveFear();
                        break;
                    case PotionSpellTypes.RestoreLifeLevels:
                        identified = eventPublisher.PublishWithOutputBool(new RestorePlayerLevelsCommand());
                        //identified = spellRestorePlayerLevels();
                        break;
                    case PotionSpellTypes.ResistHeat:
                        if (py.flags.heat_resistance == 0)
                        {
                            identified = true;
                        }
                        py.flags.heat_resistance += rnd.randomNumber(10) + 10;
                        break;
                    case PotionSpellTypes.ResistCold:
                        if (py.flags.cold_resistance == 0)
                        {
                            identified = true;
                        }
                        py.flags.cold_resistance += rnd.randomNumber(10) + 10;
                        break;
                    case PotionSpellTypes.DetectInvisible:
                        if (py.flags.detect_invisible == 0)
                        {
                            identified = true;
                        }
                        playerMagic.playerDetectInvisible(rnd.randomNumber(12) + 12);
                        break;
                    case PotionSpellTypes.SlowPoison:
                        identified = eventPublisher.PublishWithOutputBool(new SlowPoisonCommand());
                        //identified = spellSlowPoison();
                        break;
                    case PotionSpellTypes.NeutralizePoison:
                        identified = playerMagic.playerCurePoison();
                        break;
                    case PotionSpellTypes.RestoreMana:
                        if (py.misc.current_mana < py.misc.mana)
                        {
                            py.misc.current_mana = py.misc.mana;
                            printMessage("Your feel your head clear.");
                            printCharacterCurrentMana();
                            identified = true;
                        }
                        break;
                    case PotionSpellTypes.InfraVision:
                        if (py.flags.timed_infra == 0)
                        {
                            printMessage("Your eyes begin to tingle.");
                            identified = true;
                        }
                        py.flags.timed_infra += 100 + rnd.randomNumber(100);
                        break;
                    default:
                        // All cases are handled, so this should never be reached!
                        printMessage("Internal error in potion()");
                        break;
                }
            }

            return identified;
        }

        // Potions for the quaffing -RAK-
        public static void quaff()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            if (py.pack.unique_items == 0)
            {
                printMessage("But you are not carrying anything.");
                return;
            }

            int item_pos_begin = 0, item_pos_end = 0;
            if (!inventoryManager.inventoryFindRange((int)TV_POTION1, (int)TV_POTION2, ref item_pos_begin, ref item_pos_end))
            {
                printMessage("You are not carrying any potions.");
                return;
            }

            var item_id = 0;
            if (!uiInventory.inventoryGetInputForItemId(out item_id, "Quaff which potion?", item_pos_begin, item_pos_end, /*CNIL*/null, /*CNIL*/null))
            {
                return;
            }

            game.player_free_turn = false;

            bool identified;
            var item = py.inventory[item_id];

            if (item.flags == 0)
            {
                printMessage("You feel less thirsty.");
                identified = true;
            }
            else
            {
                identified = playerDrinkPotion(item.flags, item.category_id);
            }

            if (identified)
            {
                if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
                {
                    // round half-way case up
                    py.misc.exp += (int)((item.depth_first_found + (py.misc.level >> 1)) / py.misc.level);
                    displayCharacterExperience();

                    itemIdentify(py.inventory[item_id], ref item_id);
                    item = py.inventory[item_id];
                }
            }
            else if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                itemSetAsTried(item);
            }

            playerIngestFood(item.misc_use);
            itemTypeRemainingCountDescription(item_id);
            inventoryManager.inventoryDestroyItem(item_id);
        }
    }
}
