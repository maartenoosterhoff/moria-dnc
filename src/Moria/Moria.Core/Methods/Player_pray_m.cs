using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Player;
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
using static Moria.Core.Methods.Player_stats_m;

namespace Moria.Core.Methods
{
    public interface IPlayerPray
    {
        void pray();
    }

    public class Player_pray_m : IPlayerPray
    {
        private readonly IDice dice;
        private readonly IEventPublisher eventPublisher;
        private readonly IGame game;
        private readonly IHelpers helpers;
        private readonly IInventoryManager inventoryManager;
        private readonly IPlayerMagic playerMagic;
        private readonly IRnd rnd;
        private readonly ISpells spells;
        private readonly ITerminal terminal;
        private readonly ITerminalEx terminalEx;
        private readonly IUiInventory uiInventory;

        public Player_pray_m(
            IDice dice,
            IEventPublisher eventPublisher,
            IGame game,
            IHelpers helpers,
            IInventoryManager inventoryManager,
            IPlayerMagic playerMagic,
            IRnd rnd,
            ISpells spells,
            ITerminal terminal,
            ITerminalEx terminalEx,
            IUiInventory uiInventory
        )
        {
            this.dice = dice;
            this.eventPublisher = eventPublisher;
            this.game = game;
            this.helpers = helpers;
            this.inventoryManager = inventoryManager;
            this.playerMagic = playerMagic;
            this.rnd = rnd;
            this.spells = spells;
            this.terminal = terminal;
            this.terminalEx = terminalEx;
            this.uiInventory = uiInventory;
        }

        private bool playerCanPray(ref int item_pos_begin, ref int item_pos_end)
        {
            var py = State.Instance.py;
            if (py.flags.blind > 0)
            {
                this.terminal.printMessage("You can't see to read your prayer!");
                return false;
            }

            if (this.helpers.playerNoLight())
            {
                this.terminal.printMessage("You have no light to read by.");
                return false;
            }

            if (py.flags.confused > 0)
            {
                this.terminal.printMessage("You are too confused.");
                return false;
            }

            if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells != Config.spells.SPELL_TYPE_PRIEST)
            {
                this.terminal.printMessage("Pray hard enough and your prayers may be answered.");
                return false;
            }

            if (py.pack.unique_items == 0)
            {
                this.terminal.printMessage("But you are not carrying anything!");
                return false;
            }

            if (!this.inventoryManager.inventoryFindRange((int)TV_PRAYER_BOOK, TV_NEVER, out item_pos_begin, out item_pos_end))
            {
                this.terminal.printMessage("You are not carrying any Holy Books!");
                return false;
            }

            return true;
        }



        // Recite a prayers.
        private void playerRecitePrayer(int prayer_type)
        {
            var py = State.Instance.py;
            var dir = 0;

            switch ((PriestSpellTypes)(prayer_type + 1))
            {
                case PriestSpellTypes.DetectEvil:
                    this.eventPublisher.Publish(new DetectEvilCommand());
                    //spellDetectEvil();
                    break;
                case PriestSpellTypes.CureLightWounds:
                    this.eventPublisher.Publish(new ChangePlayerHitPointsCommand(this.dice.diceRoll(new Dice_t(3, 3))
                    ));
                    //spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(3, 3)));
                    break;
                case PriestSpellTypes.Bless:
                    this.playerMagic.playerBless(this.rnd.randomNumber(12) + 12);
                    break;
                case PriestSpellTypes.RemoveFear:
                    this.playerMagic.playerRemoveFear();
                    break;
                case PriestSpellTypes.CallLight:
                    this.eventPublisher.Publish(new LightAreaCommand(py.pos));
                    //spellLightArea(py.pos);
                    break;
                case PriestSpellTypes.FindTraps:
                    this.eventPublisher.Publish(new DetectTrapsWithinVicinityCommand());
                    //spellDetectTrapsWithinVicinity();
                    break;
                case PriestSpellTypes.DetectDoorsStairs:
                    this.eventPublisher.Publish(new DetectSecretDoorsWithinVicinityCommand());
                    //spellDetectSecretDoorssWithinVicinity();
                    break;
                case PriestSpellTypes.SlowPoison:
                    this.eventPublisher.Publish(new SlowPoisonCommand());
                    //spellSlowPoison();
                    break;
                case PriestSpellTypes.BlindCreature:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(new ConfuseMonsterCommand(
                            py.pos,
                            dir
                        ));
                        //spellConfuseMonster(py.pos, dir);
                    }
                    break;
                case PriestSpellTypes.Portal:
                    this.eventPublisher.Publish(new TeleportCommand((int)(py.misc.level * 3)));
                    //playerTeleport((int)py.misc.level * 3);
                    break;
                case PriestSpellTypes.CureMediumWounds:
                    this.eventPublisher.Publish(new ChangePlayerHitPointsCommand(this.dice.diceRoll(new Dice_t(4, 4))
                    ));
                    //spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(4, 4)));
                    break;
                case PriestSpellTypes.Chant:
                    this.playerMagic.playerBless(this.rnd.randomNumber(24) + 24);
                    break;
                case PriestSpellTypes.Sanctuary:
                    monsterSleep(py.pos);
                    break;
                case PriestSpellTypes.CreateFood:
                    this.eventPublisher.Publish(new CreateFoodCommand());
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
                    py.flags.heat_resistance += this.rnd.randomNumber(10) + 10;
                    py.flags.cold_resistance += this.rnd.randomNumber(10) + 10;
                    break;
                case PriestSpellTypes.NeutralizePoison:
                    this.playerMagic.playerCurePoison();
                    break;
                case PriestSpellTypes.OrbOfDraining:
                    if (this.game.getDirectionWithMemory(/*CNIL*/ null, ref dir))
                    {
                        this.eventPublisher.Publish(
                            new FireBallCommand(
                                py.pos, dir, (int)(this.dice.diceRoll(new Dice_t(3, 6)) + py.misc.level), (int)MagicSpellFlags.HolyOrb, "Black Sphere"
                            )
                        );
                        //spellFireBall(py.pos, dir, (int)(dice.diceRoll(new Dice_t(3, 6)) + py.misc.level), (int)MagicSpellFlags.HolyOrb, "Black Sphere");
                    }
                    break;
                case PriestSpellTypes.CureSeriousWounds:
                    this.eventPublisher.Publish(new ChangePlayerHitPointsCommand(this.dice.diceRoll(new Dice_t(8, 4))
                    ));
                    //spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(8, 4)));
                    break;
                case PriestSpellTypes.SenseInvisible:
                    this.playerMagic.playerDetectInvisible(this.rnd.randomNumber(24) + 24);
                    break;
                case PriestSpellTypes.ProtectFromEvil:
                    this.playerMagic.playerProtectEvil();
                    break;
                case PriestSpellTypes.Earthquake:
                    this.eventPublisher.Publish(new EarthquakeCommand());
                    //spellEarthquake();
                    break;
                case PriestSpellTypes.SenseSurroundings:
                    this.eventPublisher.Publish(new MapCurrentAreaCommand());
                    //spellMapCurrentArea();
                    break;
                case PriestSpellTypes.CureCriticalWounds:
                    this.eventPublisher.Publish(new ChangePlayerHitPointsCommand(this.dice.diceRoll(new Dice_t(16, 4))
                    ));
                    //spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(16, 4)));
                    break;
                case PriestSpellTypes.TurnUndead:
                    this.eventPublisher.Publish(new TurnUndeadCommand());
                    //spellTurnUndead();
                    break;
                case PriestSpellTypes.Prayer:
                    this.playerMagic.playerBless(this.rnd.randomNumber(48) + 48);
                    break;
                case PriestSpellTypes.DispelUndead:
                    this.eventPublisher.Publish(new DispelCreatureCommand(
                        (int)Config.monsters_defense.CD_UNDEAD, (int)(3 * py.misc.level)
                    ));
                    //spellDispelCreature((int)Config.monsters_defense.CD_UNDEAD, (int)(3 * py.misc.level));
                    break;
                case PriestSpellTypes.Heal:
                    this.eventPublisher.Publish(new ChangePlayerHitPointsCommand(
                        200
                    ));
                    //spellChangePlayerHitPoints(200);
                    break;
                case PriestSpellTypes.DispelEvil:
                    this.eventPublisher.Publish(new DispelCreatureCommand(
                        (int)Config.monsters_defense.CD_EVIL, (int)(3 * py.misc.level)
                    ));
                    //spellDispelCreature((int)Config.monsters_defense.CD_EVIL, (int)(3 * py.misc.level));
                    break;
                case PriestSpellTypes.GlyphOfWarding:
                    this.eventPublisher.Publish(new WardingGlyphCommand());
                    //spellWardingGlyph();
                    break;
                case PriestSpellTypes.HolyWord:
                    this.playerMagic.playerRemoveFear();
                    this.playerMagic.playerCurePoison();
                    this.eventPublisher.Publish(new ChangePlayerHitPointsCommand(
                        1000
                    ));
                    //spellChangePlayerHitPoints(1000);

                    for (var i = (int)PlayerAttr.STR; i <= (int)PlayerAttr.CHR; i++)
                    {
                        playerStatRestore(i);
                    }

                    this.eventPublisher.Publish(new DispelCreatureCommand(
                        (int)Config.monsters_defense.CD_EVIL, (int)(4 * py.misc.level)
                    ));
                    //spellDispelCreature((int)Config.monsters_defense.CD_EVIL, (int)(4 * py.misc.level));
                    this.eventPublisher.Publish(new TurnUndeadCommand());
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
        public void pray()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;
            game.player_free_turn = true;

            int item_pos_begin = 0, item_pos_end = 0;
            if (!this.playerCanPray(ref item_pos_begin, ref item_pos_end))
            {
                return;
            }

            if (!this.uiInventory.inventoryGetInputForItemId(out var item_id, "Use which Holy Book?", item_pos_begin, item_pos_end, /*CNIL*/ null, /*CNIL*/ null))
            {
                return;
            }

            int choice = 0, chance = 0;
            var result = this.spells.castSpellGetId("Recite which prayer?", item_id, ref choice, ref chance);
            if (result < 0)
            {
                this.terminal.printMessage("You don't know any prayers in that book.");
                return;
            }
            if (result == 0)
            {
                return;
            }

            if (this.rnd.randomNumber(100) < chance)
            {
                this.terminal.printMessage("You lost your concentration!");
                return;
            }


            var spell = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1][choice];

            // NOTE: at least one function called by `playerRecitePrayer()` sets `player_free_turn = true`,
            // e.g. `spellCreateFood()`, so this check is required. -MRC-
            game.player_free_turn = false;
            this.playerRecitePrayer(choice);
            if (!game.player_free_turn)
            {
                if ((py.flags.spells_worked & (1L << choice)) == 0)
                {
                    py.misc.exp += (int)(spell.exp_gain_for_learning << 2);
                    this.terminalEx.displayCharacterExperience();
                    py.flags.spells_worked |= 1u << choice;
                }
            }

            if (!game.player_free_turn)
            {
                if (spell.mana_required > py.misc.current_mana)
                {
                    this.terminal.printMessage("You faint from fatigue!");
                    py.flags.paralysis = (int) this.rnd.randomNumber(5 * (int)(spell.mana_required - py.misc.current_mana));
                    py.misc.current_mana = 0;
                    py.misc.current_mana_fraction = 0;
                    if (this.rnd.randomNumber(3) == 1)
                    {
                        this.terminal.printMessage("You have damaged your health!");
                        this.eventPublisher.Publish(new StatRandomDecreaseCommand((int)PlayerAttr.CON));
                        //playerStatRandomDecrease((int)PlayerAttr.CON);
                    }
                }
                else
                {
                    py.misc.current_mana -= (int)spell.mana_required;
                }

                this.terminalEx.printCharacterCurrentMana();
            }
        }

    }
}
