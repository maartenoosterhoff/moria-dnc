using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Player;
using Moria.Core.Methods.Commands.SpellCasting;
using Moria.Core.Methods.Commands.SpellCasting.Attacking;
using Moria.Core.Methods.Commands.SpellCasting.Defending;
using Moria.Core.Methods.Commands.SpellCasting.Destroying;
using Moria.Core.Methods.Commands.SpellCasting.Detection;
using Moria.Core.Methods.Commands.SpellCasting.Lighting;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Treasure_c;

namespace Moria.Core.Methods
{
    public interface IMageSpells
    {
        void getAndCastMagicSpell();
    }

    public class Mage_spells_m : IMageSpells
    {
        private readonly IDice dice;
        private readonly IEventPublisher eventPublisher;
        private readonly IGame game;
        private readonly IHelpers helpers;
        private readonly IInventoryManager inventoryManager;
        private readonly IMonster monster;
        private readonly IPlayerMagic playerMagic;
        private readonly IRnd rnd;
        private readonly ISpells spells;
        private readonly ITerminal terminal;
        private readonly ITerminalEx terminalEx;
        private readonly IUiInventory uiInventory;

        public Mage_spells_m(
            IDice dice,
            IEventPublisher eventPublisher,
            IGame game,
            IHelpers helpers,
            IInventoryManager inventoryManager,
            IMonster monster,
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
            this.monster = monster;
            this.playerMagic = playerMagic;
            this.rnd = rnd;
            this.spells = spells;
            this.terminal = terminal;
            this.terminalEx = terminalEx;
            this.uiInventory = uiInventory;
        }

        private bool canReadSpells()
        {
            var py = State.Instance.py;
            if (py.flags.blind > 0)
            {
                this.terminal.printMessage("You can't see to read your spell book!");
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

            if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells != Config.spells.SPELL_TYPE_MAGE)
            {
                this.terminal.printMessage("You can't cast spells!");
                return false;
            }

            return true;
        }

        private void castSpell(int spell_id)
        {
            var py = State.Instance.py;

            var dir = 0;

            switch ((MageSpellId)spell_id)
            {
                case MageSpellId.MagicMissile:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(new FireBoltCommand(
                            py.pos,
                            dir, this.dice.diceRoll(new Dice_t(2, 6)),
                            (int)MagicSpellFlags.MagicMissile,
                            Library.Instance.Player.spell_names[0]
                        ));
                        //spellFireBolt(py.pos, dir, dice.diceRoll(new Dice_t(2, 6)), (int)MagicSpellFlags.MagicMissile, Library.Instance.Player.spell_names[0]);
                    }
                    break;
                case MageSpellId.DetectMonsters:
                    this.eventPublisher.Publish(new DetectMonstersCommand());
                    //spellDetectMonsters();
                    break;
                case MageSpellId.PhaseDoor:
                    this.eventPublisher.Publish(new TeleportCommand(10));
                    //playerTeleport(10);
                    break;
                case MageSpellId.LightArea:
                    this.eventPublisher.Publish(new LightAreaCommand(py.pos));
                    //spellLightArea(py.pos);
                    break;
                case MageSpellId.CureLightWounds:
                    this.eventPublisher.Publish(new ChangePlayerHitPointsCommand(this.dice.diceRoll(new Dice_t(4, 4))
                    ));
                    //spellChangePlayerHitPoints(dice.diceRoll(new Dice_t(4, 4)));
                    break;
                case MageSpellId.FindHiddenTrapsDoors:
                    this.eventPublisher.PublishWithOutputBool(new DetectSecretDoorsWithinVicinityCommand());
                    //spellDetectSecretDoorssWithinVicinity();
                    this.eventPublisher.Publish(new DetectTrapsWithinVicinityCommand());
                    //spellDetectTrapsWithinVicinity();
                    break;
                case MageSpellId.StinkingCloud:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(
                            new FireBallCommand(
                                py.pos, dir, 12, (int)MagicSpellFlags.PoisonGas, Library.Instance.Player.spell_names[6]
                            )
                        );
                        //spellFireBall(py.pos, dir, 12, (int)MagicSpellFlags.PoisonGas, Library.Instance.Player.spell_names[6]);
                    }
                    break;
                case MageSpellId.Confusion:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(new ConfuseMonsterCommand(
                            py.pos,
                            dir
                        ));
                        //spellConfuseMonster(py.pos, dir);
                    }
                    break;
                case MageSpellId.LightningBolt:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(
                            new FireBoltCommand(
                                py.pos,
                                dir, this.dice.diceRoll(new Dice_t(4, 8)),
                                (int)MagicSpellFlags.Lightning,
                                Library.Instance.Player.spell_names[8]
                            )
                        );
                        //spellFireBolt(py.pos, dir, dice.diceRoll(new Dice_t(4, 8)), (int)MagicSpellFlags.Lightning, Library.Instance.Player.spell_names[8]);
                    }
                    break;
                case MageSpellId.TrapDoorDestruction:
                    this.eventPublisher.Publish(new DestroyAdjacentDoorsTrapsCommand());
                    //spellDestroyAdjacentDoorsTraps();
                    break;
                case MageSpellId.Sleep1:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(new SleepMonsterCommand(
                            py.pos, dir
                        ));
                        //spellSleepMonster(py.pos, dir);
                    }
                    break;
                case MageSpellId.CurePoison:
                    this.playerMagic.playerCurePoison();
                    break;
                case MageSpellId.TeleportSelf:
                    this.eventPublisher.Publish(new TeleportCommand((int)(py.misc.level * 5)));
                    //playerTeleport((int)py.misc.level * 5);
                    break;
                case MageSpellId.RemoveCurse:
                    for (var id = 22; id < PLAYER_INVENTORY_SIZE; id++)
                    {
                        py.inventory[id].flags = py.inventory[id].flags & ~Config.treasure_flags.TR_CURSED;
                    }
                    break;
                case MageSpellId.FrostBolt:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(
                            new FireBoltCommand(
                                py.pos,
                                dir, this.dice.diceRoll(new Dice_t(6, 8)),
                                (int)MagicSpellFlags.Frost,
                                Library.Instance.Player.spell_names[14]
                            )
                        );
                        //spellFireBolt(py.pos, dir, dice.diceRoll(new Dice_t(6, 8)), (int)MagicSpellFlags.Frost, Library.Instance.Player.spell_names[14]);
                    }
                    break;
                case MageSpellId.WallToMud:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(new WallToMudCommand(
                            py.pos, dir
                        ));
                        //spellWallToMud(py.pos, dir);
                    }
                    break;
                case MageSpellId.CreateFood:
                    this.eventPublisher.Publish(new CreateFoodCommand());
                    //spellCreateFood();
                    break;
                case MageSpellId.RechargeItem1:
                    this.eventPublisher.Publish(new RechargeItemCommand(20));
                    //spellRechargeItem(20);
                    break;
                case MageSpellId.Sleep2:
                    this.monster.monsterSleep(py.pos);
                    break;
                case MageSpellId.PolymorphOther:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(new PolymorphMonsterCommand(
                            py.pos, dir
                        ));
                        //spellPolymorphMonster(py.pos, dir);
                    }
                    break;
                case MageSpellId.IdentifyItem:
                    this.eventPublisher.Publish(new IdentifyItemCommand());
                    //spellIdentifyItem();
                    break;
                case MageSpellId.Sleep3:
                    this.eventPublisher.Publish(new SleepAllMonstersCommand());
                    //spellSleepAllMonsters();
                    break;
                case MageSpellId.FireBolt:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(
                            new FireBoltCommand(
                                py.pos,
                                dir, this.dice.diceRoll(new Dice_t(9, 8)),
                                (int)MagicSpellFlags.Fire,
                                Library.Instance.Player.spell_names[22]
                            )
                        );
                        //spellFireBolt(py.pos, dir, dice.diceRoll(new Dice_t(9, 8)), (int)MagicSpellFlags.Fire, Library.Instance.Player.spell_names[22]);
                    }
                    break;
                case MageSpellId.SpeedMonster:
                    if (this.game.getDirectionWithMemory(/*CNIL*/ null, ref dir))
                    {
                        this.eventPublisher.Publish(new SpeedMonsterCommand(
                            py.pos, dir, -1
                        ));
                        //spellSpeedMonster(py.pos, dir, -1);
                    }
                    break;
                case MageSpellId.FrostBall:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(
                            new FireBallCommand(
                                py.pos, dir, 48, (int)MagicSpellFlags.Frost, Library.Instance.Player.spell_names[24]
                            )
                        );
                        //spellFireBall(py.pos, dir, 48, (int)MagicSpellFlags.Frost, Library.Instance.Player.spell_names[24]);
                    }
                    break;
                case MageSpellId.RechargeItem2:
                    this.eventPublisher.Publish(new RechargeItemCommand(60));
                    //spellRechargeItem(60);
                    break;
                case MageSpellId.TeleportOther:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(new TeleportAwayMonsterInDirectionCommand(
                            py.pos, dir
                        ));
                        //spellTeleportAwayMonsterInDirection(py.pos, dir);
                    }
                    break;
                case MageSpellId.HasteSelf:
                    py.flags.fast += this.rnd.randomNumber(20) + (int)py.misc.level;
                    break;
                case MageSpellId.FireBall:
                    if (this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
                    {
                        this.eventPublisher.Publish(
                            new FireBallCommand(
                                py.pos, dir, 72, (int)MagicSpellFlags.Fire, Library.Instance.Player.spell_names[28]
                            )
                        );
                        //spellFireBall(py.pos, dir, 72, (int)MagicSpellFlags.Fire, Library.Instance.Player.spell_names[28]);
                    }
                    break;
                case MageSpellId.WordOfDestruction:
                    this.eventPublisher.Publish(new DestroyAreaCommand(py.pos));
                    //spellDestroyArea(py.pos);
                    break;
                case MageSpellId.Genocide:
                    this.eventPublisher.Publish(new GenocideCommand());
                    //spellGenocide();
                    break;
                default:
                    // All cases are handled, so this should never be reached!
                    break;
            }
        }

        // Throw a magic spell -RAK-
        public void getAndCastMagicSpell()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            if (!this.canReadSpells())
            {
                return;
            }

            if (!this.inventoryManager.inventoryFindRange((int)TV_MAGIC_BOOK, TV_NEVER, out var i, out var j))
            {
                this.terminal.printMessage("But you are not carrying any spell-books!");
                return;
            }

            if (!this.uiInventory.inventoryGetInputForItemId(out var item_val, "Use which spell-book?", i, j, null/*CNIL*/, /*CNIL*/null))
            {
                return;
            }

            int choice = 0, chance = 0;
            var result = this.spells.castSpellGetId("Cast which spell?", item_val, ref choice, ref chance);
            if (result < 0)
            {
                this.terminal.printMessage("You don't know any spells in that book.");
                return;
            }
            if (result == 0)
            {
                return;
            }

            game.player_free_turn = false;

            var magic_spell = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1][choice];

            if (this.rnd.randomNumber(100) < chance)
            {
                this.terminal.printMessage("You failed to get the spell off!");
            }
            else
            {
                this.castSpell(choice + 1);

                if ((py.flags.spells_worked & (1L << choice)) == 0)
                {
                    py.misc.exp += (int)(magic_spell.exp_gain_for_learning << 2);
                    py.flags.spells_worked = py.flags.spells_worked | (1u << choice);

                    this.terminalEx.displayCharacterExperience();
                }
            }

            if (magic_spell.mana_required > py.misc.current_mana)
            {
                this.terminal.printMessage("You faint from the effort!");

                py.flags.paralysis = this.rnd.randomNumber(5 * ((int)magic_spell.mana_required - py.misc.current_mana));
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
                py.misc.current_mana -= (int)magic_spell.mana_required;
            }

            this.terminalEx.printCharacterCurrentMana();
        }
    }
}
