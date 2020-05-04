﻿using Moria.Core.Configs;
using Moria.Core.Methods.Commands.SpellCasting;
using Moria.Core.Methods.Commands.SpellCasting.Defending;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_stats_m;

namespace Moria.Core.Methods
{
    public static class Player_eat_m
    {
        public static void SetDependencies(
            IDice dice,
            IHelpers helpers,
            IInventoryManager inventoryManager,
            IPlayerMagic playerMagic,
            IRnd rnd,
            ITerminal terminal,
            IUiInventory uiInventory,

            IEventPublisher eventPublisher
        )
        {
            Player_eat_m.dice = dice;
            Player_eat_m.helpers = helpers;
            Player_eat_m.inventoryManager = inventoryManager;
            Player_eat_m.playerMagic = playerMagic;
            Player_eat_m.rnd = rnd;
            Player_eat_m.terminal = terminal;
            Player_eat_m.uiInventory = uiInventory;

            Player_eat_m.eventPublisher = eventPublisher;
        }

        private static IDice dice;
        private static IHelpers helpers;
        private static IInventoryManager inventoryManager;
        private static IPlayerMagic playerMagic;
        private static IRnd rnd;
        private static ITerminal terminal;
        private static IUiInventory uiInventory;

        private static IEventPublisher eventPublisher;

        // Eat some food. -RAK-
        public static void playerEat()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            if (py.pack.unique_items == 0)
            {
                terminal.printMessage("But you are not carrying anything.");
                return;
            }

            if (!inventoryManager.inventoryFindRange((int)TV_FOOD, TV_NEVER, out var item_pos_start, out var item_pos_end))
            {
                terminal.printMessage("You are not carrying any food.");
                return;
            }

            if (!uiInventory.inventoryGetInputForItemId(out var item_id, "Eat what?", item_pos_start, item_pos_end, /*CNIL*/null, /*CNIL*/null))
            {
                return;
            }

            game.player_free_turn = false;

            var identified = false;

            var item = py.inventory[item_id];
            var item_flags = item.flags;

            while (item_flags != 0)
            {
                switch ((FoodMagicTypes)(helpers.getAndClearFirstBit(ref item_flags) + 1))
                {
                    case FoodMagicTypes.Poison:
                        py.flags.poisoned += rnd.randomNumber(10) + (int)item.depth_first_found;
                        identified = true;
                        break;
                    case FoodMagicTypes.Blindness:
                        py.flags.blind += rnd.randomNumber(250) + 10 * (int)item.depth_first_found + 100;
                        drawCavePanel();
                        terminal.printMessage("A veil of darkness surrounds you.");
                        identified = true;
                        break;
                    case FoodMagicTypes.Paranoia:
                        py.flags.afraid += rnd.randomNumber(10) + (int)item.depth_first_found;
                        terminal.printMessage("You feel terrified!");
                        identified = true;
                        break;
                    case FoodMagicTypes.Confusion:
                        py.flags.confused += rnd.randomNumber(10) + (int)item.depth_first_found;
                        terminal.printMessage("You feel drugged.");
                        identified = true;
                        break;
                    case FoodMagicTypes.Hallucination:
                        py.flags.image += rnd.randomNumber(200) + 25 * (int)item.depth_first_found + 200;
                        terminal.printMessage("You feel drugged.");
                        identified = true;
                        break;
                    case FoodMagicTypes.CurePoison:
                        identified = playerMagic.playerCurePoison();
                        break;
                    case FoodMagicTypes.CureBlindness:
                        identified = playerMagic.playerCureBlindness();
                        break;
                    case FoodMagicTypes.CureParanoia:
                        if (py.flags.afraid > 1)
                        {
                            py.flags.afraid = 1;
                            identified = true;
                        }
                        break;
                    case FoodMagicTypes.CureConfusion:
                        identified = playerMagic.playerCureConfusion();
                        break;
                    case FoodMagicTypes.Weakness:
                        eventPublisher.Publish(new LoseStrCommand());
                        //spellLoseSTR();
                        identified = true;
                        break;
                    case FoodMagicTypes.Unhealth:
                        eventPublisher.Publish(new LoseConCommand());
                        //spellLoseCON();
                        identified = true;
                        break;
                    /*
#if 0 // 12 through 15 are no longer used
            case 12:
                lose_int();
                identified = true;
                break;
            case 13:
                lose_wis();
                identified = true;
                break;
            case 14:
                lose_dex();
                identified = true;
                break;
            case 15:
                lose_chr();
                identified = true;
                break;
#endif
*/
                    case FoodMagicTypes.RestoreSTR:
                        if (playerStatRestore((int)PlayerAttr.STR))
                        {
                            terminal.printMessage("You feel your strength returning.");
                            identified = true;
                        }
                        break;
                    case FoodMagicTypes.RestoreCON:
                        if (playerStatRestore((int)PlayerAttr.CON))
                        {
                            terminal.printMessage("You feel your health returning.");
                            identified = true;
                        }
                        break;
                    case FoodMagicTypes.RestoreINT:
                        if (playerStatRestore((int)PlayerAttr.INT))
                        {
                            terminal.printMessage("Your head spins a moment.");
                            identified = true;
                        }
                        break;
                    case FoodMagicTypes.RestoreWIS:
                        if (playerStatRestore((int)PlayerAttr.WIS))
                        {
                            terminal.printMessage("You feel your wisdom returning.");
                            identified = true;
                        }
                        break;
                    case FoodMagicTypes.RestoreDEX:
                        if (playerStatRestore((int)PlayerAttr.DEX))
                        {
                            terminal.printMessage("You feel more dexterous.");
                            identified = true;
                        }
                        break;
                    case FoodMagicTypes.RestoreCHR:
                        if (playerStatRestore((int)PlayerAttr.CHR))
                        {
                            terminal.printMessage("Your skin stops itching.");
                            identified = true;
                        }
                        break;
                    case FoodMagicTypes.FirstAid:
                        identified = eventPublisher.PublishWithOutputBool(new ChangePlayerHitPointsCommand(
                            rnd.randomNumber(6)
                        ));
                        //identified = spellChangePlayerHitPoints(rnd.randomNumber(6));
                        break;
                    case FoodMagicTypes.MinorCures:
                        identified = eventPublisher.PublishWithOutputBool(new ChangePlayerHitPointsCommand(
                            rnd.randomNumber(12)
                        ));
                        //identified = spellChangePlayerHitPoints(rnd.randomNumber(12));
                        break;
                    case FoodMagicTypes.LightCures:
                        identified = eventPublisher.PublishWithOutputBool(new ChangePlayerHitPointsCommand(
                            rnd.randomNumber(18)
                        ));
                        //identified = spellChangePlayerHitPoints(rnd.randomNumber(18));
                        break;
                    /*
#if 0 // 25 is no longer used
            case 25:
                identified = hp_player(damroll(3, 6));
                break;
#endif
*/
                    case FoodMagicTypes.MajorCures:
                        identified = eventPublisher.PublishWithOutputBool(new ChangePlayerHitPointsCommand(
                            dice.diceRoll(new Dice_t(3, 12))
                        ));
                        //identified = spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(3, 12)));
                        break;
                    case FoodMagicTypes.PoisonousFood:
                        playerTakesHit(rnd.randomNumber(18), "poisonous food.");
                        identified = true;
                        break;
                    /*
#if 0 // 28 through 30 are no longer used
            case 28:
                take_hit(randint(8), "poisonous food.");
                identified = true;
                break;
            case 29:
                take_hit(damroll(2, 8), "poisonous food.");
                identified = true;
                break;
            case 30:
                take_hit(damroll(3, 8), "poisonous food.");
                identified = true;
                break;
#endif
*/
                    default:
                        // All cases are handled, so this should never be reached!
                        terminal.printMessage("Internal error in playerEat()");
                        break;
                }
            }

            if (identified)
            {
                if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
                {
                    // use identified it, gain experience
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

            py.flags.status &= ~(Config.player_status.PY_WEAK | Config.player_status.PY_HUNGRY);

            printCharacterHungerStatus();

            itemTypeRemainingCountDescription(item_id);
            inventoryManager.inventoryDestroyItem(item_id);
        }

        // Add to the players food time -RAK-
        public static void playerIngestFood(int amount)
        {
            var py = State.Instance.py;
            if (py.flags.food < 0)
            {
                py.flags.food = 0;
            }

            py.flags.food += amount;

            if (py.flags.food > Config.player.PLAYER_FOOD_MAX)
            {
                terminal.printMessage("You are bloated from overeating.");

                // Calculate how much of amount is responsible for the bloating. Give the
                // player food credit for 1/50, and also slow them for that many turns.
                var extra = py.flags.food - (int)Config.player.PLAYER_FOOD_MAX;
                if (extra > amount)
                {
                    extra = amount;
                }
                var penalty = extra / 50;

                py.flags.slow += penalty;

                if (extra == amount)
                {
                    py.flags.food = (int)(py.flags.food - amount + penalty);
                }
                else
                {
                    py.flags.food = (int)(Config.player.PLAYER_FOOD_MAX + penalty);
                }
            }
            else if (py.flags.food > Config.player.PLAYER_FOOD_FULL)
            {
                terminal.printMessage("You are full.");
            }
        }

    }
}
