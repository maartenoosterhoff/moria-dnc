using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.SpellCasting;
using Moria.Core.Methods.Commands.SpellCasting.Attacking;
using Moria.Core.Methods.Commands.SpellCasting.Defending;
using Moria.Core.Methods.Commands.SpellCasting.Destroying;
using Moria.Core.Methods.Commands.SpellCasting.Detection;
using Moria.Core.Methods.Commands.SpellCasting.Lighting;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Player_stats_m;

namespace Moria.Core.Methods
{
    public interface IStaffs
    {
        void staffUse();
        void wandAim();
    }

    public class Staffs_m : IStaffs
    {
        private readonly IDice dice;
        private readonly IEventPublisher eventPublisher;
        private readonly IGame game;
        private readonly IHelpers helpers;
        private readonly IIdentification identification;
        private readonly IInventoryManager inventoryManager;
        private readonly IMonsterManager monsterManager;
        private readonly IPlayerMagic playerMagic;
        private readonly IRnd rnd;
        private readonly ITerminal terminal;
        private readonly ITerminalEx terminalEx;
        private readonly IUiInventory uiInventory;

        public Staffs_m(
            IDice dice,
            IEventPublisher eventPublisher,
            IGame game,
            IHelpers helpers,
            IIdentification identification,
            IInventoryManager inventoryManager,
            IMonsterManager monsterManager,
            IPlayerMagic playerMagic,
            IRnd rnd,
            ITerminal terminal,
            ITerminalEx terminalEx,
            IUiInventory uiInventory
        )
        {
            this.dice = dice;
            this.eventPublisher = eventPublisher;
            this.game = game;
            this.helpers = helpers;
            this.identification = identification;
            this.inventoryManager = inventoryManager;
            this.monsterManager = monsterManager;
            this.playerMagic = playerMagic;
            this.rnd = rnd;
            this.terminal = terminal;
            this.terminalEx = terminalEx;
            this.uiInventory = uiInventory;
        }
       
        private bool staffPlayerIsCarrying(out int item_pos_start, out int item_pos_end)
        {
            var py = State.Instance.py;
            item_pos_start = -1;
            item_pos_end = -1;
            if (py.pack.unique_items == 0)
            {
                this.terminal.printMessage("But you are not carrying anything.");
                return false;
            }

            if (!this.inventoryManager.inventoryFindRange((int)TV_STAFF, (int)TV_NEVER, out item_pos_start, out item_pos_end))
            {
                this.terminal.printMessage("You are not carrying any staffs.");
                return false;
            }

            return true;
        }

        private bool staffPlayerCanUse(Inventory_t item)
        {
            var py = State.Instance.py;

            var chance = py.misc.saving_throw;
            chance += playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT);
            chance -= (int)item.depth_first_found - 5;
            chance += Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DEVICE] * (int)py.misc.level / 3;

            if (py.flags.confused > 0)
            {
                chance = chance / 2;
            }

            // Give everyone a slight chance
            if (chance < Config.player.PLAYER_USE_DEVICE_DIFFICULTY && this.rnd.randomNumber((int)Config.player.PLAYER_USE_DEVICE_DIFFICULTY - chance + 1) == 1)
            {
                chance = (int)Config.player.PLAYER_USE_DEVICE_DIFFICULTY;
            }

            if (chance < 1)
            {
                chance = 1;
            }

            if (this.rnd.randomNumber(chance) < Config.player.PLAYER_USE_DEVICE_DIFFICULTY)
            {
                this.terminal.printMessage("You failed to use the staff properly.");
                return false;
            }

            if (item.misc_use < 1)
            {
                this.terminal.printMessage("The staff has no charges left.");
                if (!this.identification.spellItemIdentified(item))
                {
                    this.identification.itemAppendToInscription(item, Config.identification.ID_EMPTY);
                }
                return false;
            }

            return true;
        }

        private bool staffDischarge(Inventory_t item)
        {
            var py = State.Instance.py;

            var identified = false;

            item.misc_use--;

            var flags = item.flags;
            while (flags != 0)
            {
                switch ((StaffSpellTypes)(this.helpers.getAndClearFirstBit(ref flags) + 1))
                {
                    case StaffSpellTypes.StaffLight:
                        identified = this.eventPublisher.PublishWithOutputBool(new LightAreaCommand(py.pos));
                        //identified = spellLightArea(py.pos);
                        break;
                    case StaffSpellTypes.DetectDoorsStairs:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectSecretDoorsWithinVicinityCommand());
                        //identified = spellDetectSecretDoorssWithinVicinity();
                        break;
                    case StaffSpellTypes.TrapLocation:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectTrapsWithinVicinityCommand());
                        //identified = spellDetectTrapsWithinVicinity();
                        break;
                    case StaffSpellTypes.TreasureLocation:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectTreasureWithinVicinityCommand());
                        //identified = spellDetectTreasureWithinVicinity();
                        break;
                    case StaffSpellTypes.ObjectLocation:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectObjectsWithinVicinityCommand());
                        //identified = spellDetectObjectsWithinVicinity();
                        break;
                    case StaffSpellTypes.Teleportation:
                        this.eventPublisher.Publish(new TeleportCommand(100));
                        //playerTeleport(100);
                        identified = true;
                        break;
                    case StaffSpellTypes.Earthquakes:
                        identified = true;
                        this.eventPublisher.Publish(new EarthquakeCommand());
                        //spellEarthquake();
                        break;
                    case StaffSpellTypes.Summoning:
                        identified = false;

                        for (var i = 0; i < this.rnd.randomNumber(4); i++)
                        {
                            var coord = py.pos;
                            identified |= this.monsterManager.monsterSummon(coord, false);
                        }
                        break;
                    case StaffSpellTypes.Destruction:
                        identified = true;
                        this.eventPublisher.Publish(new DestroyAreaCommand(py.pos));
                        //spellDestroyArea(py.pos);
                        break;
                    case StaffSpellTypes.Starlight:
                        identified = true;
                        this.eventPublisher.Publish(new StarlightCommand(py.pos));
                        //spellStarlite(py.pos);
                        break;
                    case StaffSpellTypes.HasteMonsters:
                        identified = this.eventPublisher.PublishWithOutputBool(new SpeedAllMonstersCommand(
                            1
                        ));
                        //identified = spellSpeedAllMonsters(1);
                        break;
                    case StaffSpellTypes.SlowMonsters:
                        identified = this.eventPublisher.PublishWithOutputBool(new SpeedAllMonstersCommand(
                            -1
                        ));
                        //identified = spellSpeedAllMonsters(-1);
                        break;
                    case StaffSpellTypes.SleepMonsters:
                        identified = this.eventPublisher.PublishWithOutputBool(new SleepAllMonstersCommand());
                        //identified = spellSleepAllMonsters();
                        break;
                    case StaffSpellTypes.CureLightWounds:
                        identified = this.eventPublisher.PublishWithOutputBool(new ChangePlayerHitPointsCommand(this.rnd.randomNumber(8)
                        ));
                        //identified = spellChangePlayerHitPoints(rnd.randomNumber(8));
                        break;
                    case StaffSpellTypes.DetectInvisible:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectInvisibleCreaturesWithinVicinityCommand());
                        //identified = spellDetectInvisibleCreaturesWithinVicinity();
                        break;
                    case StaffSpellTypes.Speed:
                        if (py.flags.fast == 0)
                        {
                            identified = true;
                        }
                        py.flags.fast += this.rnd.randomNumber(30) + 15;
                        break;
                    case StaffSpellTypes.Slowness:
                        if (py.flags.slow == 0)
                        {
                            identified = true;
                        }
                        py.flags.slow += this.rnd.randomNumber(30) + 15;
                        break;
                    case StaffSpellTypes.MassPolymorph:
                        identified = this.eventPublisher.PublishWithOutputBool(new MassPolymorphCommand());
                        //identified = spellMassPolymorph();
                        break;
                    case StaffSpellTypes.RemoveCurse:
                        if (this.eventPublisher.PublishWithOutputBool(new RemoveCurseFromAllItemsCommand()))
                        //if (spellRemoveCurseFromAllItems())
                        {
                            if (py.flags.blind < 1)
                            {
                                this.terminal.printMessage("The staff glows blue for a moment..");
                            }
                            identified = true;
                        }
                        break;
                    case StaffSpellTypes.DetectEvil:
                        identified = this.eventPublisher.PublishWithOutputBool(new DetectEvilCommand());
                        //identified = spellDetectEvil();
                        break;
                    case StaffSpellTypes.Curing:
                        if (this.playerMagic.playerCureBlindness() || this.playerMagic.playerCurePoison() || this.playerMagic.playerCureConfusion())
                        {
                            identified = true;
                        }
                        break;
                    case StaffSpellTypes.DispelEvil:
                        identified = this.eventPublisher.PublishWithOutputBool(new DispelCreatureCommand(
                            (int)Config.monsters_defense.CD_EVIL, 60
                        ));
                        //identified = spellDispelCreature((int)Config.monsters_defense.CD_EVIL, 60);
                        break;
                    case StaffSpellTypes.Darkness:
                        identified = this.eventPublisher.PublishWithOutputBool(new DarkenAreaCommand(py.pos)); 
                        //identified = spellDarkenArea(py.pos);
                        break;
                    case StaffSpellTypes.StoreBoughtFlag:
                        // store bought flag
                        break;
                    default:
                        // All cases are handled, so this should never be reached!
                        this.terminal.printMessage("Internal error in staffs()");
                        break;
                }
            }

            return identified;
        }

        // Use a staff. -RAK-
        public void staffUse()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            if (!this.staffPlayerIsCarrying(out var item_pos_start, out var item_pos_end))
            {
                return;
            }

            if (!this.uiInventory.inventoryGetInputForItemId(out var item_id, "Use which staff?", item_pos_start, item_pos_end, /*CNIL*/ null, /*CNIL*/ null))
            {
                return;
            }

            // From here on player uses up a turn
            game.player_free_turn = false;

            var item = py.inventory[item_id];

            if (!this.staffPlayerCanUse(item))
            {
                return;
            }

            var identified = this.staffDischarge(item);

            if (identified)
            {
                if (!this.identification.itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
                {
                    // round half-way case up
                    py.misc.exp += (int)((item.depth_first_found + (py.misc.level >> 1)) / py.misc.level);

                    this.terminalEx.displayCharacterExperience();

                    this.identification.itemIdentify(py.inventory[item_id], ref item_id);
                }
            }
            else if (!this.identification.itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                this.identification.itemSetAsTried(item);
            }

            this.identification.itemChargesRemainingDescription(item_id);
        }

        private bool wandDischarge(Inventory_t item, int direction)
        {
            var py = State.Instance.py;
            var spell_names = Library.Instance.Player.spell_names;

            // decrement "use" variable
            item.misc_use--;

            var identified = false;
            var flags = item.flags;

            var coord = new Coord_t(0, 0);

            while (flags != 0)
            {
                coord.y = py.pos.y;
                coord.x = py.pos.x;

                // Wand types
                switch ((WandSpellTypes)(this.helpers.getAndClearFirstBit(ref flags) + 1))
                {
                    case WandSpellTypes.WandLight:
                        this.terminal.printMessage("A line of blue shimmering light appears.");
                        this.eventPublisher.Publish(new LightLineCommand(
                            py.pos, direction
                        ));
                        //spellLightLine(py.pos, direction);
                        identified = true;
                        break;
                    case WandSpellTypes.LightningBolt:
                        this.eventPublisher.Publish(
                            new FireBoltCommand(
                                coord,
                                direction, this.dice.diceRoll(new Dice_t(4, 8)),
                                (int)MagicSpellFlags.Lightning,
                                spell_names[8]
                            )
                        );
                        //spellFireBolt(coord, direction, dice.diceRoll(new Dice_t(4, 8)), (int)MagicSpellFlags.Lightning, spell_names[8]);
                        identified = true;
                        break;
                    case WandSpellTypes.FrostBolt:
                        this.eventPublisher.Publish(
                            new FireBoltCommand(
                                coord,
                                direction, this.dice.diceRoll(new Dice_t(6, 8)),
                                (int)MagicSpellFlags.Frost,
                                spell_names[14]
                            )
                        );
                        //spellFireBolt(coord, direction, dice.diceRoll(new Dice_t(6, 8)), (int)MagicSpellFlags.Frost, spell_names[14]);
                        identified = true;
                        break;
                    case WandSpellTypes.FireBolt:
                        this.eventPublisher.Publish(
                            new FireBoltCommand(
                                coord,
                                direction, this.dice.diceRoll(new Dice_t(9, 8)),
                                (int)MagicSpellFlags.Fire,
                                spell_names[22]
                            )
                        );
                        //spellFireBolt(coord, direction, dice.diceRoll(new Dice_t(9, 8)), (int)MagicSpellFlags.Fire, spell_names[22]);
                        identified = true;
                        break;
                    case WandSpellTypes.StoneToMud:
                        identified = this.eventPublisher.PublishWithOutputBool(new WallToMudCommand(
                            coord, direction
                        ));
                        //identified = spellWallToMud(coord, direction);
                        break;
                    case WandSpellTypes.Polymorph:
                        identified = this.eventPublisher.PublishWithOutputBool(new PolymorphMonsterCommand(
                            coord, direction
                        ));
                        //identified = spellPolymorphMonster(coord, direction);
                        break;
                    case WandSpellTypes.HealMonster:
                        identified = this.eventPublisher.PublishWithOutputBool(new ChangeMonsterHitPointsCommand(
                            coord, direction, -this.dice.diceRoll(new Dice_t(4, 6))
                        ));
                        //identified = spellChangeMonsterHitPoints(coord, direction, -dice.diceRoll(new Dice_t(4, 6)));
                        break;
                    case WandSpellTypes.HasteMonster:
                        identified = this.eventPublisher.PublishWithOutputBool(new SpeedMonsterCommand(
                            coord, direction, 1
                        ));
                        //identified = spellSpeedMonster(coord, direction, 1);
                        break;
                    case WandSpellTypes.SlowMonster:
                        identified = this.eventPublisher.PublishWithOutputBool(new SpeedMonsterCommand(
                            py.pos, direction, -1
                        ));
                        //identified = spellSpeedMonster(coord, direction, -1);
                        break;
                    case WandSpellTypes.ConfuseMonster:
                        identified = this.eventPublisher.PublishWithOutputBool(new ConfuseMonsterCommand(
                            coord,
                            direction
                        ));
                        //identified = spellConfuseMonster(coord, direction);
                        break;
                    case WandSpellTypes.SleepMonster:
                        identified = this.eventPublisher.PublishWithOutputBool(new SleepMonsterCommand(
                            coord, direction
                        ));
                        //identified = spellSleepMonster(coord, direction);
                        break;
                    case WandSpellTypes.DrainLife:
                        identified = this.eventPublisher.PublishWithOutputBool(new DrainLifeFromMonsterCommand(
                            coord, direction
                        ));
                        //identified = spellDrainLifeFromMonster(coord, direction);
                        break;
                    case WandSpellTypes.TrapDoorDestruction:
                        identified = this.eventPublisher.PublishWithOutputBool(new DestroyDoorsTrapsInDirectionCommand(
                            coord, direction
                        ));
                        //identified = spellDestroyDoorsTrapsInDirection(coord, direction);
                        break;
                    case WandSpellTypes.WandMagicMissile:
                        this.eventPublisher.Publish(
                            new FireBoltCommand(
                                coord,
                                direction, this.dice.diceRoll(new Dice_t(2, 6)),
                                (int)MagicSpellFlags.MagicMissile,
                                spell_names[0]
                            )
                        );
                        //spellFireBolt(coord, direction, dice.diceRoll(new Dice_t(2, 6)), (int)MagicSpellFlags.MagicMissile, spell_names[0]);
                        identified = true;
                        break;
                    case WandSpellTypes.WallBuilding:
                        identified = this.eventPublisher.PublishWithOutputBool(new BuildWallCommand(
                            coord, direction
                        ));
                        //identified = spellBuildWall(coord, direction);
                        break;
                    case WandSpellTypes.CloneMonster:
                        identified = this.eventPublisher.PublishWithOutputBool(new CloneMonsterCommand(
                            coord, direction
                        ));
                        //identified = spellCloneMonster(coord, direction);
                        break;
                    case WandSpellTypes.TeleportAway:
                        identified = this.eventPublisher.PublishWithOutputBool(new TeleportAwayMonsterInDirectionCommand(
                            coord, direction
                        ));
                        //identified = spellTeleportAwayMonsterInDirection(coord, direction);
                        break;
                    case WandSpellTypes.Disarming:
                        identified = this.eventPublisher.PublishWithOutputBool(new DisarmAllInDirectionCommand(
                            coord, direction
                        ));
                        //identified = spellDisarmAllInDirection(coord, direction);
                        break;
                    case WandSpellTypes.LightningBall:
                        this.eventPublisher.Publish(
                            new FireBallCommand(
                                coord, direction, 32, (int)MagicSpellFlags.Lightning, "Lightning Ball"
                            )
                        );
                        //spellFireBall(coord, direction, 32, (int)MagicSpellFlags.Lightning, "Lightning Ball");
                        identified = true;
                        break;
                    case WandSpellTypes.ColdBall:
                        this.eventPublisher.Publish(
                            new FireBallCommand(
                                coord, direction, 48, (int)MagicSpellFlags.Frost, "Cold Ball"
                            )
                        );
                        //spellFireBall(coord, direction, 48, (int)MagicSpellFlags.Frost, "Cold Ball");
                        identified = true;
                        break;
                    case WandSpellTypes.FireBall:
                        this.eventPublisher.Publish(
                            new FireBallCommand(
                                coord, direction, 72, (int)MagicSpellFlags.Fire, spell_names[28]
                            )
                        );
                        //spellFireBall(coord, direction, 72, (int)MagicSpellFlags.Fire, spell_names[28]);
                        identified = true;
                        break;
                    case WandSpellTypes.StinkingCloud:
                        this.eventPublisher.Publish(
                            new FireBallCommand(
                                coord, direction, 12, (int)MagicSpellFlags.PoisonGas, spell_names[6]
                            )
                        );
                        //spellFireBall(coord, direction, 12, (int)MagicSpellFlags.PoisonGas, spell_names[6]);
                        identified = true;
                        break;
                    case WandSpellTypes.AcidBall:
                        this.eventPublisher.Publish(
                            new FireBallCommand(
                                coord, direction, 60, (int)MagicSpellFlags.Acid, "Acid Ball"
                            )
                        );
                        //spellFireBall(coord, direction, 60, (int)MagicSpellFlags.Acid, "Acid Ball");
                        identified = true;
                        break;
                    case WandSpellTypes.Wonder:
                        flags = (uint)(1L << (this.rnd.randomNumber(23) - 1));
                        break;
                    default:
                        // All cases are handled, so this should never be reached!
                        this.terminal.printMessage("Internal error in wands()");
                        break;
                }
            }

            return identified;
        }

        // Wands for the aiming.
        public void wandAim()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            if (py.pack.unique_items == 0)
            {
                this.terminal.printMessage("But you are not carrying anything.");
                return;
            }

            if (!this.inventoryManager.inventoryFindRange((int)TV_WAND, TV_NEVER, out var item_pos_start, out var item_pos_end))
            {
                this.terminal.printMessage("You are not carrying any wands.");
                return;
            }

            if (!this.uiInventory.inventoryGetInputForItemId(out var item_id, "Aim which wand?", item_pos_start, item_pos_end, /*CNIL*/ null, /*CNIL*/null))
            {
                return;
            }

            game.player_free_turn = false;

            var direction = 0;
            if (!this.game.getDirectionWithMemory(/*CNIL*/null, ref direction))
            {
                return;
            }

            if (py.flags.confused > 0)
            {
                this.terminal.printMessage("You are confused.");
                direction = this.rnd.getRandomDirection();
            }

            var item = py.inventory[item_id];

            var player_class_lev_adj = Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DEVICE] * (int)py.misc.level / 3;
            var chance = py.misc.saving_throw + playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT) - (int)item.depth_first_found + player_class_lev_adj;

            if (py.flags.confused > 0)
            {
                chance = chance / 2;
            }

            if (chance < Config.player.PLAYER_USE_DEVICE_DIFFICULTY && this.rnd.randomNumber((int)Config.player.PLAYER_USE_DEVICE_DIFFICULTY - chance + 1) == 1)
            {
                chance = (int)Config.player.PLAYER_USE_DEVICE_DIFFICULTY; // Give everyone a slight chance
            }

            if (chance <= 0)
            {
                chance = 1;
            }

            if (this.rnd.randomNumber(chance) < Config.player.PLAYER_USE_DEVICE_DIFFICULTY)
            {
                this.terminal.printMessage("You failed to use the wand properly.");
                return;
            }

            if (item.misc_use < 1)
            {
                this.terminal.printMessage("The wand has no charges left.");
                if (!this.identification.spellItemIdentified(item))
                {
                    this.identification.itemAppendToInscription(item, Config.identification.ID_EMPTY);
                }
                return;
            }

            var identified = this.wandDischarge(item, direction);

            if (identified)
            {
                if (!this.identification.itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
                {
                    // round half-way case up
                    py.misc.exp += (int)((item.depth_first_found + (py.misc.level >> 1)) / py.misc.level);
                    this.terminalEx.displayCharacterExperience();

                    this.identification.itemIdentify(py.inventory[item_id], ref item_id);
                }
            }
            else if (!this.identification.itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                this.identification.itemSetAsTried(item);
            }

            this.identification.itemChargesRemainingDescription(item_id);
        }
    }
}
