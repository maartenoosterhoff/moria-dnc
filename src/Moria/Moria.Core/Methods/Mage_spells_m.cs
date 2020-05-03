using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.SpellCasting;
using Moria.Core.Methods.Commands.SpellCasting.Attacking;
using Moria.Core.Methods.Commands.SpellCasting.Destroying;
using Moria.Core.Methods.Commands.SpellCasting.Detection;
using Moria.Core.Methods.Commands.SpellCasting.Lighting;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Mage_spells_m
    {
        public static void SetDependencies(
            IDice dice,
            IGame game,
            IHelpers helpers,
            IInventoryManager inventoryManager,
            IPlayerMagic playerMagic,
            IRnd rnd,
            IUiInventory uiInventory,

            IEventPublisher eventPublisher
        )
        {
            Mage_spells_m.dice = dice;
            Mage_spells_m.game = game;
            Mage_spells_m.helpers = helpers;
            Mage_spells_m.inventoryManager = inventoryManager;
            Mage_spells_m.playerMagic = playerMagic;
            Mage_spells_m.rnd = rnd;
            Mage_spells_m.uiInventory = uiInventory;

            Mage_spells_m.eventPublisher = eventPublisher;
        }

        private static IDice dice;
        private static IGame game;
        private static IHelpers helpers;
        private static IInventoryManager inventoryManager;
        private static IPlayerMagic playerMagic;
        private static IRnd rnd;
        private static IUiInventory uiInventory;

        private static IEventPublisher eventPublisher;

        public static bool canReadSpells()
        {
            var py = State.Instance.py;
            if (py.flags.blind > 0)
            {
                printMessage("You can't see to read your spell book!");
                return false;
            }

            if (helpers.playerNoLight())
            {
                printMessage("You have no light to read by.");
                return false;
            }

            if (py.flags.confused > 0)
            {
                printMessage("You are too confused.");
                return false;
            }

            if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells != Config.spells.SPELL_TYPE_MAGE)
            {
                printMessage("You can't cast spells!");
                return false;
            }

            return true;
        }

        static void castSpell(int spell_id)
        {
            var py = State.Instance.py;

            var dir = 0;

            switch ((MageSpellId)spell_id)
            {
                case MageSpellId.MagicMissile:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        eventPublisher.Publish(new FireBoltCommand(
                            py.pos,
                            dir,
                            dice.diceRoll(new Dice_t(2, 6)),
                            (int)MagicSpellFlags.MagicMissile,
                            Library.Instance.Player.spell_names[0]
                        ));
                        //spellFireBolt(py.pos, dir, dice.diceRoll(new Dice_t(2, 6)), (int)MagicSpellFlags.MagicMissile, Library.Instance.Player.spell_names[0]);
                    }
                    break;
                case MageSpellId.DetectMonsters:
                    eventPublisher.Publish(new DetectMonstersCommand());
                    //spellDetectMonsters();
                    break;
                case MageSpellId.PhaseDoor:
                    eventPublisher.Publish(new TeleportCommand(10));
                    //playerTeleport(10);
                    break;
                case MageSpellId.LightArea:
                    eventPublisher.Publish(new LightAreaCommand(py.pos));
                    //spellLightArea(py.pos);
                    break;
                case MageSpellId.CureLightWounds:
                    spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(4, 4)));
                    break;
                case MageSpellId.FindHiddenTrapsDoors:
                    eventPublisher.PublishWithOutputBool(new DetectSecretDoorsWithinVicinityCommand());
                    //spellDetectSecretDoorssWithinVicinity();
                    eventPublisher.Publish(new DetectTrapsWithinVicinityCommand());
                    //spellDetectTrapsWithinVicinity();
                    break;
                case MageSpellId.StinkingCloud:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        eventPublisher.Publish(
                            new FireBallCommand(
                                py.pos, dir, 12, (int)MagicSpellFlags.PoisonGas, Library.Instance.Player.spell_names[6]
                            )
                        );
                        //spellFireBall(py.pos, dir, 12, (int)MagicSpellFlags.PoisonGas, Library.Instance.Player.spell_names[6]);
                    }
                    break;
                case MageSpellId.Confusion:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        spellConfuseMonster(py.pos, dir);
                    }
                    break;
                case MageSpellId.LightningBolt:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        eventPublisher.Publish(
                            new FireBoltCommand(
                                py.pos,
                                dir,
                                dice.diceRoll(new Dice_t(4, 8)),
                                (int)MagicSpellFlags.Lightning,
                                Library.Instance.Player.spell_names[8]
                            )
                        );
                        //spellFireBolt(py.pos, dir, dice.diceRoll(new Dice_t(4, 8)), (int)MagicSpellFlags.Lightning, Library.Instance.Player.spell_names[8]);
                    }
                    break;
                case MageSpellId.TrapDoorDestruction:
                    eventPublisher.Publish(new DestroyAdjacentDoorsTrapsCommand());
                    //spellDestroyAdjacentDoorsTraps();
                    break;
                case MageSpellId.Sleep1:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        spellSleepMonster(py.pos, dir);
                    }
                    break;
                case MageSpellId.CurePoison:
                    playerMagic.playerCurePoison();
                    break;
                case MageSpellId.TeleportSelf:
                    eventPublisher.Publish(new TeleportCommand((int)(py.misc.level * 5)));
                    //playerTeleport((int)py.misc.level * 5);
                    break;
                case MageSpellId.RemoveCurse:
                    for (var id = 22; id < PLAYER_INVENTORY_SIZE; id++)
                    {
                        py.inventory[id].flags = py.inventory[id].flags & ~Config.treasure_flags.TR_CURSED;
                    }
                    break;
                case MageSpellId.FrostBolt:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        eventPublisher.Publish(
                            new FireBoltCommand(
                                py.pos,
                                dir,
                                dice.diceRoll(new Dice_t(6, 8)),
                                (int)MagicSpellFlags.Frost,
                                Library.Instance.Player.spell_names[14]
                            )
                        );
                        //spellFireBolt(py.pos, dir, dice.diceRoll(new Dice_t(6, 8)), (int)MagicSpellFlags.Frost, Library.Instance.Player.spell_names[14]);
                    }
                    break;
                case MageSpellId.WallToMud:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        spellWallToMud(py.pos, dir);
                    }
                    break;
                case MageSpellId.CreateFood:
                    eventPublisher.Publish(new CreateFoodCommand());
                    //spellCreateFood();
                    break;
                case MageSpellId.RechargeItem1:
                    spellRechargeItem(20);
                    break;
                case MageSpellId.Sleep2:
                    monsterSleep(py.pos);
                    break;
                case MageSpellId.PolymorphOther:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        spellPolymorphMonster(py.pos, dir);
                    }
                    break;
                case MageSpellId.IdentifyItem:
                    spellIdentifyItem();
                    break;
                case MageSpellId.Sleep3:
                    spellSleepAllMonsters();
                    break;
                case MageSpellId.FireBolt:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        eventPublisher.Publish(
                            new FireBoltCommand(
                                py.pos,
                                dir,
                                dice.diceRoll(new Dice_t(9, 8)),
                                (int)MagicSpellFlags.Fire,
                                Library.Instance.Player.spell_names[22]
                            )
                        );
                        //spellFireBolt(py.pos, dir, dice.diceRoll(new Dice_t(9, 8)), (int)MagicSpellFlags.Fire, Library.Instance.Player.spell_names[22]);
                    }
                    break;
                case MageSpellId.SpeedMonster:
                    if (game.getDirectionWithMemory(/*CNIL*/ null, ref dir))
                    {
                        spellSpeedMonster(py.pos, dir, -1);
                    }
                    break;
                case MageSpellId.FrostBall:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        eventPublisher.Publish(
                            new FireBallCommand(
                                py.pos, dir, 48, (int)MagicSpellFlags.Frost, Library.Instance.Player.spell_names[24]
                            )
                        );
                        //spellFireBall(py.pos, dir, 48, (int)MagicSpellFlags.Frost, Library.Instance.Player.spell_names[24]);
                    }
                    break;
                case MageSpellId.RechargeItem2:
                    spellRechargeItem(60);
                    break;
                case MageSpellId.TeleportOther:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        spellTeleportAwayMonsterInDirection(py.pos, dir);
                    }
                    break;
                case MageSpellId.HasteSelf:
                    py.flags.fast += rnd.randomNumber(20) + (int)py.misc.level;
                    break;
                case MageSpellId.FireBall:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        eventPublisher.Publish(
                            new FireBallCommand(
                                py.pos, dir, 72, (int)MagicSpellFlags.Fire, Library.Instance.Player.spell_names[28]
                            )
                        );
                        //spellFireBall(py.pos, dir, 72, (int)MagicSpellFlags.Fire, Library.Instance.Player.spell_names[28]);
                    }
                    break;
                case MageSpellId.WordOfDestruction:
                    spellDestroyArea(py.pos);
                    break;
                case MageSpellId.Genocide:
                    spellGenocide();
                    break;
                default:
                    // All cases are handled, so this should never be reached!
                    break;
            }
        }

        // Throw a magic spell -RAK-
        public static void getAndCastMagicSpell()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            if (!canReadSpells())
            {
                return;
            }

            int i = 0, j = 0;
            if (!inventoryManager.inventoryFindRange((int)TV_MAGIC_BOOK, TV_NEVER, ref i, ref j))
            {
                printMessage("But you are not carrying any spell-books!");
                return;
            }

            var item_val = 0;
            if (!uiInventory.inventoryGetInputForItemId(out item_val, "Use which spell-book?", i, j, null/*CNIL*/, /*CNIL*/null))
            {
                return;
            }

            int choice = 0, chance = 0;
            var result = castSpellGetId("Cast which spell?", item_val, ref choice, ref chance);
            if (result < 0)
            {
                printMessage("You don't know any spells in that book.");
                return;
            }
            if (result == 0)
            {
                return;
            }

            game.player_free_turn = false;

            var magic_spell = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1][choice];

            if (rnd.randomNumber(100) < chance)
            {
                printMessage("You failed to get the spell off!");
            }
            else
            {
                castSpell(choice + 1);

                if ((py.flags.spells_worked & (1L << choice)) == 0)
                {
                    py.misc.exp += (int)(magic_spell.exp_gain_for_learning << 2);
                    py.flags.spells_worked = py.flags.spells_worked | (1u << choice);

                    displayCharacterExperience();
                }
            }

            if (magic_spell.mana_required > py.misc.current_mana)
            {
                printMessage("You faint from the effort!");

                py.flags.paralysis = rnd.randomNumber(5 * ((int)magic_spell.mana_required - py.misc.current_mana));
                py.misc.current_mana = 0;
                py.misc.current_mana_fraction = 0;

                if (rnd.randomNumber(3) == 1)
                {
                    printMessage("You have damaged your health!");
                    playerStatRandomDecrease((int)PlayerAttr.CON);
                }
            }
            else
            {
                py.misc.current_mana -= (int)magic_spell.mana_required;
            }

            printCharacterCurrentMana();
        }

        // Returns spell chance of failure for class_to_use_mage_spells -RAK-
        public static int spellChanceOfSuccess(int spell_id)
        {
            var py = State.Instance.py;

            var spell = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1][spell_id];

            var chance = (int)(spell.failure_chance - 3 * (py.misc.level - spell.level_required));

            int stat;
            if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
            {
                stat = (int)PlayerAttr.INT;
            }
            else
            {
                stat = (int)PlayerAttr.WIS;
            }

            chance -= 3 * (playerStatAdjustmentWisdomIntelligence(stat) - 1);

            if (spell.mana_required > py.misc.current_mana)
            {
                chance += 5 * ((int)spell.mana_required - py.misc.current_mana);
            }

            if (chance > 95)
            {
                chance = 95;
            }
            else if (chance < 5)
            {
                chance = 5;
            }

            return chance;
        }
    }
}
