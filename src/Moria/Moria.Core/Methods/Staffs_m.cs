using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Monster_manager_m;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_magic_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Player_m;

namespace Moria.Core.Methods
{
    public static class Staffs_m
    {
        public static void SetDependencies(
            IDice dice,
            IGame game,
            IRnd rnd,
            IUiInventory uiInventory
        )
        {
            Staffs_m.dice = dice;
            Staffs_m.game = game;
            Staffs_m.rnd = rnd;
            Staffs_m.uiInventory = uiInventory;
        }

        private static IDice dice;
        private static IGame game;
        private static IRnd rnd;
        private static IUiInventory uiInventory;

        public static bool staffPlayerIsCarrying(ref int item_pos_start, ref int item_pos_end)
        {
            var py = State.Instance.py;
            if (py.pack.unique_items == 0)
            {
                printMessage("But you are not carrying anything.");
                return false;
            }

            if (!inventoryFindRange((int)TV_STAFF, (int)TV_NEVER, ref item_pos_start, ref item_pos_end))
            {
                printMessage("You are not carrying any staffs.");
                return false;
            }

            return true;
        }

        public static bool staffPlayerCanUse(Inventory_t item)
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
            if (chance < Config.player.PLAYER_USE_DEVICE_DIFFICULTY && rnd.randomNumber((int)Config.player.PLAYER_USE_DEVICE_DIFFICULTY - chance + 1) == 1)
            {
                chance = (int)Config.player.PLAYER_USE_DEVICE_DIFFICULTY;
            }

            if (chance < 1)
            {
                chance = 1;
            }

            if (rnd.randomNumber(chance) < Config.player.PLAYER_USE_DEVICE_DIFFICULTY)
            {
                printMessage("You failed to use the staff properly.");
                return false;
            }

            if (item.misc_use < 1)
            {
                printMessage("The staff has no charges left.");
                if (!spellItemIdentified(item))
                {
                    itemAppendToInscription(item, Config.identification.ID_EMPTY);
                }
                return false;
            }

            return true;
        }

        public static bool staffDischarge(Inventory_t item)
        {
            var py = State.Instance.py;

            var identified = false;

            item.misc_use--;

            var flags = item.flags;
            while (flags != 0)
            {
                switch ((StaffSpellTypes)(getAndClearFirstBit(ref flags) + 1))
                {
                    case StaffSpellTypes.StaffLight:
                        identified = spellLightArea(py.pos);
                        break;
                    case StaffSpellTypes.DetectDoorsStairs:
                        identified = spellDetectSecretDoorssWithinVicinity();
                        break;
                    case StaffSpellTypes.TrapLocation:
                        identified = spellDetectTrapsWithinVicinity();
                        break;
                    case StaffSpellTypes.TreasureLocation:
                        identified = spellDetectTreasureWithinVicinity();
                        break;
                    case StaffSpellTypes.ObjectLocation:
                        identified = spellDetectObjectsWithinVicinity();
                        break;
                    case StaffSpellTypes.Teleportation:
                        playerTeleport(100);
                        identified = true;
                        break;
                    case StaffSpellTypes.Earthquakes:
                        identified = true;
                        spellEarthquake();
                        break;
                    case StaffSpellTypes.Summoning:
                        identified = false;

                        for (var i = 0; i < rnd.randomNumber(4); i++)
                        {
                            var coord = py.pos;
                            identified |= monsterSummon(coord, false);
                        }
                        break;
                    case StaffSpellTypes.Destruction:
                        identified = true;
                        spellDestroyArea(py.pos);
                        break;
                    case StaffSpellTypes.Starlight:
                        identified = true;
                        spellStarlite(py.pos);
                        break;
                    case StaffSpellTypes.HasteMonsters:
                        identified = spellSpeedAllMonsters(1);
                        break;
                    case StaffSpellTypes.SlowMonsters:
                        identified = spellSpeedAllMonsters(-1);
                        break;
                    case StaffSpellTypes.SleepMonsters:
                        identified = spellSleepAllMonsters();
                        break;
                    case StaffSpellTypes.CureLightWounds:
                        identified = spellChangePlayerHitPoints(rnd.randomNumber(8));
                        break;
                    case StaffSpellTypes.DetectInvisible:
                        identified = spellDetectInvisibleCreaturesWithinVicinity();
                        break;
                    case StaffSpellTypes.Speed:
                        if (py.flags.fast == 0)
                        {
                            identified = true;
                        }
                        py.flags.fast += rnd.randomNumber(30) + 15;
                        break;
                    case StaffSpellTypes.Slowness:
                        if (py.flags.slow == 0)
                        {
                            identified = true;
                        }
                        py.flags.slow += rnd.randomNumber(30) + 15;
                        break;
                    case StaffSpellTypes.MassPolymorph:
                        identified = spellMassPolymorph();
                        break;
                    case StaffSpellTypes.RemoveCurse:
                        if (spellRemoveCurseFromAllItems())
                        {
                            if (py.flags.blind < 1)
                            {
                                printMessage("The staff glows blue for a moment..");
                            }
                            identified = true;
                        }
                        break;
                    case StaffSpellTypes.DetectEvil:
                        identified = spellDetectEvil();
                        break;
                    case StaffSpellTypes.Curing:
                        if (playerCureBlindness() || playerCurePoison() || playerCureConfusion())
                        {
                            identified = true;
                        }
                        break;
                    case StaffSpellTypes.DispelEvil:
                        identified = spellDispelCreature((int)Config.monsters_defense.CD_EVIL, 60);
                        break;
                    case StaffSpellTypes.Darkness:
                        identified = spellDarkenArea(py.pos);
                        break;
                    case StaffSpellTypes.StoreBoughtFlag:
                        // store bought flag
                        break;
                    default:
                        // All cases are handled, so this should never be reached!
                        printMessage("Internal error in staffs()");
                        break;
                }
            }

            return identified;
        }

        // Use a staff. -RAK-
        public static void staffUse()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            int item_pos_start = 0, item_pos_end = 0;
            if (!staffPlayerIsCarrying(ref item_pos_start, ref item_pos_end))
            {
                return;
            }

            var item_id = 0;
            if (!uiInventory.inventoryGetInputForItemId(ref item_id, "Use which staff?", item_pos_start, item_pos_end, /*CNIL*/ null, /*CNIL*/ null))
            {
                return;
            }

            // From here on player uses up a turn
            game.player_free_turn = false;

            var item = py.inventory[item_id];

            if (!staffPlayerCanUse(item))
            {
                return;
            }

            var identified = staffDischarge(item);

            if (identified)
            {
                if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
                {
                    // round half-way case up
                    py.misc.exp += (int)((item.depth_first_found + (py.misc.level >> 1)) / py.misc.level);

                    displayCharacterExperience();

                    itemIdentify(py.inventory[item_id], ref item_id);
                }
            }
            else if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                itemSetAsTried(item);
            }

            itemChargesRemainingDescription(item_id);
        }

        public static bool wandDischarge(Inventory_t item, int direction)
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
                switch ((WandSpellTypes)(getAndClearFirstBit(ref flags) + 1))
                {
                    case WandSpellTypes.WandLight:
                        printMessage("A line of blue shimmering light appears.");
                        spellLightLine(py.pos, direction);
                        identified = true;
                        break;
                    case WandSpellTypes.LightningBolt:
                        spellFireBolt(coord, direction, dice.diceRoll(new Dice_t(4, 8)), (int)MagicSpellFlags.Lightning, spell_names[8]);
                        identified = true;
                        break;
                    case WandSpellTypes.FrostBolt:
                        spellFireBolt(coord, direction, dice.diceRoll(new Dice_t(6, 8)), (int)MagicSpellFlags.Frost, spell_names[14]);
                        identified = true;
                        break;
                    case WandSpellTypes.FireBolt:
                        spellFireBolt(coord, direction, dice.diceRoll(new Dice_t(9, 8)), (int)MagicSpellFlags.Fire, spell_names[22]);
                        identified = true;
                        break;
                    case WandSpellTypes.StoneToMud:
                        identified = spellWallToMud(coord, direction);
                        break;
                    case WandSpellTypes.Polymorph:
                        identified = spellPolymorphMonster(coord, direction);
                        break;
                    case WandSpellTypes.HealMonster:
                        identified = spellChangeMonsterHitPoints(coord, direction, -dice.diceRoll(new Dice_t(4, 6)));
                        break;
                    case WandSpellTypes.HasteMonster:
                        identified = spellSpeedMonster(coord, direction, 1);
                        break;
                    case WandSpellTypes.SlowMonster:
                        identified = spellSpeedMonster(coord, direction, -1);
                        break;
                    case WandSpellTypes.ConfuseMonster:
                        identified = spellConfuseMonster(coord, direction);
                        break;
                    case WandSpellTypes.SleepMonster:
                        identified = spellSleepMonster(coord, direction);
                        break;
                    case WandSpellTypes.DrainLife:
                        identified = spellDrainLifeFromMonster(coord, direction);
                        break;
                    case WandSpellTypes.TrapDoorDestruction:
                        identified = spellDestroyDoorsTrapsInDirection(coord, direction);
                        break;
                    case WandSpellTypes.WandMagicMissile:
                        spellFireBolt(coord, direction, dice.diceRoll(new Dice_t(2, 6)), (int)MagicSpellFlags.MagicMissile, spell_names[0]);
                        identified = true;
                        break;
                    case WandSpellTypes.WallBuilding:
                        identified = spellBuildWall(coord, direction);
                        break;
                    case WandSpellTypes.CloneMonster:
                        identified = spellCloneMonster(coord, direction);
                        break;
                    case WandSpellTypes.TeleportAway:
                        identified = spellTeleportAwayMonsterInDirection(coord, direction);
                        break;
                    case WandSpellTypes.Disarming:
                        identified = spellDisarmAllInDirection(coord, direction);
                        break;
                    case WandSpellTypes.LightningBall:
                        spellFireBall(coord, direction, 32, (int)MagicSpellFlags.Lightning, "Lightning Ball");
                        identified = true;
                        break;
                    case WandSpellTypes.ColdBall:
                        spellFireBall(coord, direction, 48, (int)MagicSpellFlags.Frost, "Cold Ball");
                        identified = true;
                        break;
                    case WandSpellTypes.FireBall:
                        spellFireBall(coord, direction, 72, (int)MagicSpellFlags.Fire, spell_names[28]);
                        identified = true;
                        break;
                    case WandSpellTypes.StinkingCloud:
                        spellFireBall(coord, direction, 12, (int)MagicSpellFlags.PoisonGas, spell_names[6]);
                        identified = true;
                        break;
                    case WandSpellTypes.AcidBall:
                        spellFireBall(coord, direction, 60, (int)MagicSpellFlags.Acid, "Acid Ball");
                        identified = true;
                        break;
                    case WandSpellTypes.Wonder:
                        flags = (uint)(1L << (rnd.randomNumber(23) - 1));
                        break;
                    default:
                        // All cases are handled, so this should never be reached!
                        printMessage("Internal error in wands()");
                        break;
                }
            }

            return identified;
        }

        // Wands for the aiming.
        public static void wandAim()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            if (py.pack.unique_items == 0)
            {
                printMessage("But you are not carrying anything.");
                return;
            }

            int item_pos_start = 0, item_pos_end = 0;
            if (!inventoryFindRange((int)TV_WAND, TV_NEVER, ref item_pos_start, ref item_pos_end))
            {
                printMessage("You are not carrying any wands.");
                return;
            }

            var item_id = 0;
            if (!uiInventory.inventoryGetInputForItemId(ref item_id, "Aim which wand?", item_pos_start, item_pos_end, /*CNIL*/ null, /*CNIL*/null))
            {
                return;
            }

            game.player_free_turn = false;

            var direction = 0;
            if (!Staffs_m.game.getDirectionWithMemory(/*CNIL*/null, ref direction))
            {
                return;
            }

            if (py.flags.confused > 0)
            {
                printMessage("You are confused.");
                direction = rnd.getRandomDirection();
            }

            var item = py.inventory[item_id];

            var player_class_lev_adj = Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DEVICE] * (int)py.misc.level / 3;
            var chance = py.misc.saving_throw + playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT) - (int)item.depth_first_found + player_class_lev_adj;

            if (py.flags.confused > 0)
            {
                chance = chance / 2;
            }

            if (chance < Config.player.PLAYER_USE_DEVICE_DIFFICULTY && rnd.randomNumber((int)Config.player.PLAYER_USE_DEVICE_DIFFICULTY - chance + 1) == 1)
            {
                chance = (int)Config.player.PLAYER_USE_DEVICE_DIFFICULTY; // Give everyone a slight chance
            }

            if (chance <= 0)
            {
                chance = 1;
            }

            if (rnd.randomNumber(chance) < Config.player.PLAYER_USE_DEVICE_DIFFICULTY)
            {
                printMessage("You failed to use the wand properly.");
                return;
            }

            if (item.misc_use < 1)
            {
                printMessage("The wand has no charges left.");
                if (!spellItemIdentified(item))
                {
                    itemAppendToInscription(item, Config.identification.ID_EMPTY);
                }
                return;
            }

            var identified = wandDischarge(item, direction);

            if (identified)
            {
                if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
                {
                    // round half-way case up
                    py.misc.exp += (int)((item.depth_first_found + (py.misc.level >> 1)) / py.misc.level);
                    displayCharacterExperience();

                    itemIdentify(py.inventory[item_id], ref item_id);
                }
            }
            else if (!itemSetColorlessAsIdentified((int)item.category_id, (int)item.sub_category_id, (int)item.identification))
            {
                itemSetAsTried(item);
            }

            itemChargesRemainingDescription(item_id);
        }

    }
}
