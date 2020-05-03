using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.SpellCasting;
using Moria.Core.Methods.Commands.SpellCasting.Attacking;
using Moria.Core.Methods.Commands.SpellCasting.Defending;
using Moria.Core.Methods.Commands.SpellCasting.Detection;
using Moria.Core.Methods.Commands.SpellCasting.Lighting;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Player_pray_m
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
            Player_pray_m.dice = dice;
            Player_pray_m.game = game;
            Player_pray_m.helpers = helpers;
            Player_pray_m.inventoryManager = inventoryManager;
            Player_pray_m.playerMagic = playerMagic;
            Player_pray_m.rnd = rnd;
            Player_pray_m.uiInventory = uiInventory;

            Player_pray_m.eventPublisher = eventPublisher;
        }

        private static IDice dice;
        private static IGame game;
        private static IHelpers helpers;
        private static IInventoryManager inventoryManager;
        private static IPlayerMagic playerMagic;
        private static IRnd rnd;
        private static IUiInventory uiInventory;

        private static IEventPublisher eventPublisher;

        static bool playerCanPray(ref int item_pos_begin, ref int item_pos_end)
        {
            var py = State.Instance.py;
            if (py.flags.blind > 0)
            {
                printMessage("You can't see to read your prayer!");
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

            if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells != Config.spells.SPELL_TYPE_PRIEST)
            {
                printMessage("Pray hard enough and your prayers may be answered.");
                return false;
            }

            if (py.pack.unique_items == 0)
            {
                printMessage("But you are not carrying anything!");
                return false;
            }

            if (!inventoryManager.inventoryFindRange((int)TV_PRAYER_BOOK, TV_NEVER, ref item_pos_begin, ref item_pos_end))
            {
                printMessage("You are not carrying any Holy Books!");
                return false;
            }

            return true;
        }



        // Recite a prayers.
        static void playerRecitePrayer(int prayer_type)
        {
            var py = State.Instance.py;
            var dir = 0;

            switch ((PriestSpellTypes)(prayer_type + 1))
            {
                case PriestSpellTypes.DetectEvil:
                    eventPublisher.Publish(new DetectEvilCommand());
                    //spellDetectEvil();
                    break;
                case PriestSpellTypes.CureLightWounds:
                    spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(3, 3)));
                    break;
                case PriestSpellTypes.Bless:
                    playerMagic.playerBless(rnd.randomNumber(12) + 12);
                    break;
                case PriestSpellTypes.RemoveFear:
                    playerMagic.playerRemoveFear();
                    break;
                case PriestSpellTypes.CallLight:
                    eventPublisher.Publish(new LightAreaCommand(py.pos));
                    //spellLightArea(py.pos);
                    break;
                case PriestSpellTypes.FindTraps:
                    eventPublisher.Publish(new DetectTrapsWithinVicinityCommand());
                    //spellDetectTrapsWithinVicinity();
                    break;
                case PriestSpellTypes.DetectDoorsStairs:
                    eventPublisher.Publish(new DetectSecretDoorsWithinVicinityCommand());
                    //spellDetectSecretDoorssWithinVicinity();
                    break;
                case PriestSpellTypes.SlowPoison:
                    eventPublisher.Publish(new SlowPoisonCommand());
                    //spellSlowPoison();
                    break;
                case PriestSpellTypes.BlindCreature:
                    if (game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        eventPublisher.Publish(new ConfuseMonsterCommand(
                            py.pos,
                            dir
                        ));
                        //spellConfuseMonster(py.pos, dir);
                    }
                    break;
                case PriestSpellTypes.Portal:
                    eventPublisher.Publish(new TeleportCommand((int)(py.misc.level * 3)));
                    //playerTeleport((int)py.misc.level * 3);
                    break;
                case PriestSpellTypes.CureMediumWounds:
                    spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(4, 4)));
                    break;
                case PriestSpellTypes.Chant:
                    playerMagic.playerBless(rnd.randomNumber(24) + 24);
                    break;
                case PriestSpellTypes.Sanctuary:
                    monsterSleep(py.pos);
                    break;
                case PriestSpellTypes.CreateFood:
                    eventPublisher.Publish(new CreateFoodCommand());
                    //spellCreateFood();
                    break;
                case PriestSpellTypes.RemoveCurse:
                    foreach (var entry in py.inventory)
                    {
                        // only clear flag for items that are wielded or worn
                        if (entry.category_id >= TV_MIN_WEAR && entry.category_id <= TV_MAX_WEAR)
                        {
                            entry.flags &= ~Config.treasure_flags.TR_CURSED;
                        }
                    }
                    break;
                case PriestSpellTypes.ResistHeadCold:
                    py.flags.heat_resistance += rnd.randomNumber(10) + 10;
                    py.flags.cold_resistance += rnd.randomNumber(10) + 10;
                    break;
                case PriestSpellTypes.NeutralizePoison:
                    playerMagic.playerCurePoison();
                    break;
                case PriestSpellTypes.OrbOfDraining:
                    if (game.getDirectionWithMemory(/*CNIL*/ null, ref dir))
                    {
                        eventPublisher.Publish(
                            new FireBallCommand(
                                py.pos, dir, (int)(dice.diceRoll(new Dice_t(3, 6)) + py.misc.level), (int)MagicSpellFlags.HolyOrb, "Black Sphere"
                            )
                        );
                        //spellFireBall(py.pos, dir, (int)(dice.diceRoll(new Dice_t(3, 6)) + py.misc.level), (int)MagicSpellFlags.HolyOrb, "Black Sphere");
                    }
                    break;
                case PriestSpellTypes.CureSeriousWounds:
                    spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(8, 4)));
                    break;
                case PriestSpellTypes.SenseInvisible:
                    playerMagic.playerDetectInvisible(rnd.randomNumber(24) + 24);
                    break;
                case PriestSpellTypes.ProtectFromEvil:
                    playerMagic.playerProtectEvil();
                    break;
                case PriestSpellTypes.Earthquake:
                    eventPublisher.Publish(new EarthquakeCommand());
                    //spellEarthquake();
                    break;
                case PriestSpellTypes.SenseSurroundings:
                    spellMapCurrentArea();
                    break;
                case PriestSpellTypes.CureCriticalWounds:
                    spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(16, 4)));
                    break;
                case PriestSpellTypes.TurnUndead:
                    eventPublisher.Publish(new TurnUndeadCommand());
                    //spellTurnUndead();
                    break;
                case PriestSpellTypes.Prayer:
                    playerMagic.playerBless(rnd.randomNumber(48) + 48);
                    break;
                case PriestSpellTypes.DispelUndead:
                    eventPublisher.Publish(new DispelCreatureCommand(
                        (int)Config.monsters_defense.CD_UNDEAD, (int)(3 * py.misc.level)
                    ));
                    //spellDispelCreature((int)Config.monsters_defense.CD_UNDEAD, (int)(3 * py.misc.level));
                    break;
                case PriestSpellTypes.Heal:
                    spellChangePlayerHitPoints(200);
                    break;
                case PriestSpellTypes.DispelEvil:
                    eventPublisher.Publish(new DispelCreatureCommand(
                        (int)Config.monsters_defense.CD_EVIL, (int)(3 * py.misc.level)
                    ));
                    //spellDispelCreature((int)Config.monsters_defense.CD_EVIL, (int)(3 * py.misc.level));
                    break;
                case PriestSpellTypes.GlyphOfWarding:
                    eventPublisher.Publish(new WardingGlyphCommand());
                    //spellWardingGlyph();
                    break;
                case PriestSpellTypes.HolyWord:
                    playerMagic.playerRemoveFear();
                    playerMagic.playerCurePoison();
                    spellChangePlayerHitPoints(1000);

                    for (var i = (int)PlayerAttr.STR; i <= (int)PlayerAttr.CHR; i++)
                    {
                        playerStatRestore(i);
                    }

                    eventPublisher.Publish(new DispelCreatureCommand(
                        (int)Config.monsters_defense.CD_EVIL, (int)(4 * py.misc.level)
                    ));
                    //spellDispelCreature((int)Config.monsters_defense.CD_EVIL, (int)(4 * py.misc.level));
                    eventPublisher.Publish(new TurnUndeadCommand());
                    //spellTurnUndead();

                    if (py.flags.invulnerability < 3)
                    {
                        py.flags.invulnerability = 3;
                    }
                    else
                    {
                        py.flags.invulnerability++;
                    }
                    break;
                default:
                    // All cases are handled, so this should never be reached!
                    break;
            }
        }

        // Pray like HELL. -RAK-
        public static void pray()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;
            game.player_free_turn = true;

            int item_pos_begin = 0, item_pos_end = 0;
            if (!playerCanPray(ref item_pos_begin, ref item_pos_end))
            {
                return;
            }

            var item_id = 0;
            if (!uiInventory.inventoryGetInputForItemId(out item_id, "Use which Holy Book?", item_pos_begin, item_pos_end, /*CNIL*/ null, /*CNIL*/ null))
            {
                return;
            }

            int choice = 0, chance = 0;
            var result = castSpellGetId("Recite which prayer?", item_id, ref choice, ref chance);
            if (result < 0)
            {
                printMessage("You don't know any prayers in that book.");
                return;
            }
            if (result == 0)
            {
                return;
            }

            if (rnd.randomNumber(100) < chance)
            {
                printMessage("You lost your concentration!");
                return;
            }


            var spell = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1][choice];

            // NOTE: at least one function called by `playerRecitePrayer()` sets `player_free_turn = true`,
            // e.g. `spellCreateFood()`, so this check is required. -MRC-
            game.player_free_turn = false;
            playerRecitePrayer(choice);
            if (!game.player_free_turn)
            {
                if ((py.flags.spells_worked & (1L << choice)) == 0)
                {
                    py.misc.exp += (int)(spell.exp_gain_for_learning << 2);
                    displayCharacterExperience();
                    py.flags.spells_worked |= 1u << choice;
                }
            }

            if (!game.player_free_turn)
            {
                if (spell.mana_required > py.misc.current_mana)
                {
                    printMessage("You faint from fatigue!");
                    py.flags.paralysis = (int)rnd.randomNumber(5 * (int)(spell.mana_required - py.misc.current_mana));
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
                    py.misc.current_mana -= (int)spell.mana_required;
                }

                printCharacterCurrentMana();
            }
        }

    }
}
