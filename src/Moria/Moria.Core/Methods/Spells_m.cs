using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.SpellCasting.Defending;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Mage_spells_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Spells_m
    {
        public static void SetDependencies(
            IDice dice,
            IDungeon dungeon,
            IDungeonLos dungeonLos,
            IDungeonPlacer dungeonPlacer,
            IGameObjects gameObjects,
            IHelpers helpers,
            IInventory inventory,
            IInventoryManager inventoryManager,
            IMonsterManager monsterManager,
            IRnd rnd,
            IUiInventory uiInventory,

            IEventPublisher eventPublisher
        )
        {
            Spells_m.dice = dice;
            Spells_m.dungeon = dungeon;
            Spells_m.dungeonLos = dungeonLos;
            Spells_m.dungeonPlacer = dungeonPlacer;
            Spells_m.gameObjects = gameObjects;
            Spells_m.helpers = helpers;
            Spells_m.inventory = inventory;
            Spells_m.inventoryManager = inventoryManager;
            Spells_m.monsterManager = monsterManager;
            Spells_m.rnd = rnd;
            Spells_m.uiInventory = uiInventory;

            Spells_m.eventPublisher = eventPublisher;
        }

        private static IDice dice;
        private static IDungeon dungeon;
        private static IDungeonLos dungeonLos;
        private static IDungeonPlacer dungeonPlacer;
        private static IGameObjects gameObjects;
        private static IHelpers helpers;
        private static IInventory inventory;
        private static IInventoryManager inventoryManager;
        private static IMonsterManager monsterManager;
        private static IRnd rnd;
        private static IUiInventory uiInventory;

        private static IEventPublisher eventPublisher;

        // Returns spell pointer -RAK-
        public static bool spellGetId(int[] spell_ids, int number_of_choices, ref int spell_id, ref int spell_chance, string prompt, int first_spell)
        {
            var py = State.Instance.py;
            var magic_spells = Library.Instance.Player.magic_spells;
            var spell_names = Library.Instance.Player.spell_names;
            spell_id = -1;

            var str = string.Empty;
            //vtype_t str = { '\0' };
            str = string.Format(
                "(Spells {0}-{1}, *=List, <ESCAPE>=exit) {2}",
                (char)(spell_ids[0] + 'a' - first_spell),
                (char)(spell_ids[number_of_choices - 1] + 'a' - first_spell),
                prompt
            );
            //(void)sprintf(str, "(Spells %c-%c, *=List, <ESCAPE>=exit) %s", spell_ids[0] + 'a' - first_spell, spell_ids[number_of_choices - 1] + 'a' - first_spell, prompt);

            var spell_found = false;
            var redraw = false;

            var offset = Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells == (int)Config.spells.SPELL_TYPE_MAGE ? (int)Config.spells.NAME_OFFSET_SPELLS : (int)Config.spells.NAME_OFFSET_PRAYERS;

            var choice = '\0';

            while (!spell_found && getCommand(str, out choice))
            {
                if (char.IsUpper(choice))
                //if (isupper((int)choice) != 0)
                {
                    spell_id = choice - 'A' + first_spell;

                    // verify that this is in spells[], at most 22 entries in class_to_use_mage_spells[]
                    int test_spell_id;
                    for (test_spell_id = 0; test_spell_id < number_of_choices; test_spell_id++)
                    {
                        if (spell_id == spell_ids[test_spell_id])
                        {
                            break;
                        }
                    }

                    if (test_spell_id == number_of_choices)
                    {
                        spell_id = -2;
                    }
                    else
                    {
                        var spell = magic_spells[(int)py.misc.class_id - 1][spell_id];

                        var tmp_str = $"Cast {spell_names[spell_id + offset]} ({spell.mana_required} mana, {spellChanceOfSuccess(spell_id)}% fail)?";
                        //vtype_t tmp_str = { '\0' };
                        //(void)sprintf(tmp_str, "Cast %s (%d mana, %d%% fail)?", spell_names[spell_id + offset], spell.mana_required, spellChanceOfSuccess(spell_id));
                        if (getInputConfirmation(tmp_str))
                        {
                            spell_found = true;
                        }
                        else
                        {
                            spell_id = -1;
                        }
                    }
                }
                else if (char.IsLower(choice))
                {
                    spell_id = choice - 'a' + first_spell;

                    // verify that this is in spells[], at most 22 entries in class_to_use_mage_spells[]
                    int test_spell_id;
                    for (test_spell_id = 0; test_spell_id < number_of_choices; test_spell_id++)
                    {
                        if (spell_id == spell_ids[test_spell_id])
                        {
                            break;
                        }
                    }

                    if (test_spell_id == number_of_choices)
                    {
                        spell_id = -2;
                    }
                    else
                    {
                        spell_found = true;
                    }
                }
                else if (choice == '*')
                {
                    // only do this drawing once
                    if (!redraw)
                    {
                        terminalSaveScreen();
                        redraw = true;
                        displaySpellsList(spell_ids, number_of_choices, false, first_spell);
                    }
                }
                else if (char.IsLetter(choice))
                //else if (isalpha((int)choice) != 0)
                {
                    spell_id = -2;
                }
                else
                {
                    spell_id = -1;
                    terminalBellSound();
                }

                if (spell_id == -2)
                {
                    var spellOrPrayer = offset == Config.spells.NAME_OFFSET_SPELLS ? "spell" : "prayer";
                    var tmp_str = $"You don't know that {spellOrPrayer}.";
                    //vtype_t tmp_str = { '\0' };
                    //(void)sprintf(tmp_str, "You don't know that %s.", (offset == Config.spells.NAME_OFFSET_SPELLS ? "spell" : "prayer"));
                    printMessage(tmp_str);
                }
            }

            if (redraw)
            {
                terminalRestoreScreen();
            }

            messageLineClear();

            if (spell_found)
            {
                spell_chance = spellChanceOfSuccess(spell_id);
            }

            return spell_found;
        }

        // Return spell number and failure chance -RAK-
        // returns -1 if no spells in book
        // returns  1 if choose a spell in book to cast
        // returns  0 if don't choose a spell, i.e. exit with an escape
        // TODO: split into two functions; getting spell ID and casting an actual spell
        public static int castSpellGetId(string prompt, int item_id, ref int spell_id, ref int spell_chance)
        {
            var py = State.Instance.py;
            // NOTE: `flags` gets set again, since getAndClearFirstBit modified it
            var flags = py.inventory[item_id].flags;
            var first_spell = helpers.getAndClearFirstBit(ref flags);
            flags = py.inventory[item_id].flags & py.flags.spells_learnt;

            // TODO(cook) move access to `magic_spells[]` directly to the for loop it's used in, below?
            var spells = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1];

            var spell_count = 0;
            var spell_list = new int[31];

            while (flags != 0u)
            {
                var pos = helpers.getAndClearFirstBit(ref flags);

                if (spells[pos].level_required <= py.misc.level)
                {
                    spell_list[spell_count] = pos;
                    spell_count++;
                }
            }

            if (spell_count == 0)
            {
                return -1;
            }

            var result = 0;
            if (spellGetId(spell_list, spell_count, ref spell_id, ref spell_chance, prompt, first_spell))
            {
                result = 1;
            }

            if (result != 0 && Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1][spell_id].mana_required > py.misc.current_mana)
            {
                if (Library.Instance.Player.classes[(int)py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
                {
                    result = (int)(getInputConfirmation("You summon your limited strength to cast this one! Confirm?") ? 1 : 0);
                }
                else
                {
                    result = (int)(getInputConfirmation("The gods may think you presumptuous for this! Confirm?") ? 1 : 0);
                }
            }

            return result;
        }

        public static void dungeonLightAreaAroundFloorTile(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var spot = new Coord_t(0, 0);

            for (spot.y = coord.y - 1; spot.y <= coord.y + 1; spot.y++)
            {
                for (spot.x = coord.x - 1; spot.x <= coord.x + 1; spot.x++)
                {
                    var tile = dg.floor[spot.y][spot.x];

                    if (tile.feature_id >= MIN_CAVE_WALL)
                    {
                        tile.permanent_light = true;
                    }
                    else if (tile.treasure_id != 0 && game.treasure.list[tile.treasure_id].category_id >= TV_MIN_VISIBLE &&
                             game.treasure.list[tile.treasure_id].category_id <= TV_MAX_VISIBLE)
                    {
                        tile.field_mark = true;
                    }
                }
            }
        }

        // Map the current area plus some -RAK-
        public static void spellMapCurrentArea()
        {
            var dg = State.Instance.dg;

            var row_min = dg.panel.top - rnd.randomNumber(10);
            var row_max = dg.panel.bottom + rnd.randomNumber(10);
            var col_min = dg.panel.left - rnd.randomNumber(20);
            var col_max = dg.panel.right + rnd.randomNumber(20);

            var coord = new Coord_t(0, 0);

            for (coord.y = row_min; coord.y <= row_max; coord.y++)
            {
                for (coord.x = col_min; coord.x <= col_max; coord.x++)
                {
                    if (dungeon.coordInBounds(coord) && dg.floor[coord.y][coord.x].feature_id <= MAX_CAVE_FLOOR)
                    {
                        dungeonLightAreaAroundFloorTile(coord);
                    }
                }
            }

            drawDungeonPanel();
        }

        // Identify an object -RAK-
        public static bool spellIdentifyItem()
        {
            var py = State.Instance.py;
            var item_id = 0;
            if (!uiInventory.inventoryGetInputForItemId(out item_id, "Item you wish identified?", 0, (int)PLAYER_INVENTORY_SIZE, /*CNIL*/null, /*CNIL*/null))
            {
                return false;
            }

            itemIdentify(py.inventory[item_id], ref item_id);

            var item = py.inventory[item_id];
            spellItemIdentifyAndRemoveRandomInscription(item);

            var description = string.Empty;
            //obj_desc_t description = { '\0' };
            itemDescription(ref description, item, true);

            var msg = string.Empty;
            //obj_desc_t msg = { '\0' };
            if (item_id >= (int)PlayerEquipment.Wield)
            {
                playerRecalculateBonuses();
                msg = $"{uiInventory.playerItemWearingDescription(item_id)}: {description}";
                //(void)sprintf(msg, "%s: %s", playerItemWearingDescription(item_id), description);
            }
            else
            {
                msg = $"{item_id + 97} {description}";
                //(void)sprintf(msg, "%c %s", item_id + 97, description);
            }
            printMessage(msg);

            return true;
        }

        // Get all the monsters on the level pissed off. -RAK-
        public static bool spellAggravateMonsters(int affect_distance)
        {
            var aggravated = false;

            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                monster.sleep_count = 0;

                if (monster.distance_from_player <= affect_distance && monster.speed < 2)
                {
                    monster.speed++;
                    aggravated = true;
                }
            }

            if (aggravated)
            {
                printMessage("You hear a sudden stirring in the distance!");
            }

            return aggravated;
        }

        // Surround the fool with traps (chuckle) -RAK-
        public static bool spellSurroundPlayerWithTraps()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var coord = new Coord_t(0, 0);

            for (coord.y = py.pos.y - 1; coord.y <= py.pos.y + 1; coord.y++)
            {
                for (coord.x = py.pos.x - 1; coord.x <= py.pos.x + 1; coord.x++)
                {
                    // Don't put a trap under the player, since this can lead to
                    // strange situations, e.g. falling through a trap door while
                    // trying to rest, setting off a falling rock trap and ending
                    // up under the rock.
                    if (coord.y == py.pos.y && coord.x == py.pos.x)
                    {
                        continue;
                    }

                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.feature_id <= MAX_CAVE_FLOOR)
                    {
                        if (tile.treasure_id != 0)
                        {
                            dungeon.dungeonDeleteObject(coord);
                        }

                        dungeonPlacer.dungeonSetTrap(coord, rnd.randomNumber(Config.dungeon_objects.MAX_TRAPS) - 1);

                        // don't let player gain exp from the newly created traps
                        game.treasure.list[tile.treasure_id].misc_use = 0;

                        // open pits are immediately visible, so call dungeonLiteSpot
                        dungeon.dungeonLiteSpot(coord);
                    }
                }
            }

            // traps are always placed, so just return true
            return true;
        }

        // Surround the player with doors. -RAK-
        public static bool spellSurroundPlayerWithDoors()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var created = false;

            var coord = new Coord_t(0, 0);

            for (coord.y = py.pos.y - 1; coord.y <= py.pos.y + 1; coord.y++)
            {
                for (coord.x = py.pos.x - 1; coord.x <= py.pos.x + 1; coord.x++)
                {
                    // Don't put a door under the player!
                    if (coord.y == py.pos.y && coord.x == py.pos.x)
                    {
                        continue;
                    }

                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.feature_id <= MAX_CAVE_FLOOR)
                    {
                        if (tile.treasure_id != 0)
                        {
                            dungeon.dungeonDeleteObject(coord);
                        }

                        var free_id = gameObjects.popt();
                        tile.feature_id = TILE_BLOCKED_FLOOR;
                        tile.treasure_id = (uint)free_id;

                        inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, game.treasure.list[free_id]);
                        dungeon.dungeonLiteSpot(coord);

                        created = true;
                    }
                }
            }

            return created;
        }

        

        

       

        

        // Disarms all traps/chests in a given direction -RAK-
        public static bool spellDisarmAllInDirection(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var distance = 0;
            var disarmed = false;

            Tile_t tile = null;

            do
            {
                tile = dg.floor[coord.y][coord.x];

                // note, must continue up to and including the first non open space,
                // because secret doors have feature_id greater than MAX_OPEN_SPACE
                if (tile.treasure_id != 0)
                {
                    var item = game.treasure.list[tile.treasure_id];

                    if (item.category_id == TV_INVIS_TRAP || item.category_id == TV_VIS_TRAP)
                    {
                        if (dungeon.dungeonDeleteObject(coord))
                        {
                            disarmed = true;
                        }
                    }
                    else if (item.category_id == TV_CLOSED_DOOR)
                    {
                        // Locked or jammed doors become merely closed.
                        item.misc_use = 0;
                    }
                    else if (item.category_id == TV_SECRET_DOOR)
                    {
                        tile.field_mark = true;
                        dungeon.trapChangeVisibility(coord);
                        disarmed = true;
                    }
                    else if (item.category_id == TV_CHEST && item.flags != 0)
                    {
                        disarmed = true;
                        printMessage("Click!");

                        item.flags &= ~(Config.treasure_chests.CH_TRAPPED | Config.treasure_chests.CH_LOCKED);
                        item.special_name_id = (int)SpecialNameIds.SN_UNLOCKED;

                        spellItemIdentifyAndRemoveRandomInscription(item);
                    }
                }

                // move must be at end because want to light up current spot
                helpers.movePosition(direction, ref coord);

                distance++;
            } while (distance <= Config.treasure.OBJECT_BOLTS_MAX_RANGE && tile.feature_id <= MAX_OPEN_SPACE);

            return disarmed;
        }

        // Return flags for given type area affect -RAK-
        public static void spellGetAreaAffectFlags(int spell_type, out uint weapon_type, out int harm_type, out Func<Inventory_t, bool> destroy)
        {
            switch ((MagicSpellFlags)spell_type)
            {
                case MagicSpellFlags.MagicMissile:
                    weapon_type = 0;
                    harm_type = 0;
                    destroy = inventory.setNull;
                    break;
                case MagicSpellFlags.Lightning:
                    weapon_type = Config.monsters_spells.CS_BR_LIGHT;
                    harm_type = (int)Config.monsters_defense.CD_LIGHT;
                    destroy = inventory.setLightningDestroyableItems;
                    break;
                case MagicSpellFlags.PoisonGas:
                    weapon_type = Config.monsters_spells.CS_BR_GAS;
                    harm_type = (int)Config.monsters_defense.CD_POISON;
                    destroy = inventory.setNull;
                    break;
                case MagicSpellFlags.Acid:
                    weapon_type = Config.monsters_spells.CS_BR_ACID;
                    harm_type = (int)Config.monsters_defense.CD_ACID;
                    destroy = inventory.setAcidDestroyableItems;
                    break;
                case MagicSpellFlags.Frost:
                    weapon_type = Config.monsters_spells.CS_BR_FROST;
                    harm_type = (int)Config.monsters_defense.CD_FROST;
                    destroy = inventory.setFrostDestroyableItems;
                    break;
                case MagicSpellFlags.Fire:
                    weapon_type = Config.monsters_spells.CS_BR_FIRE;
                    harm_type = (int)Config.monsters_defense.CD_FIRE;
                    destroy = inventory.setFireDestroyableItems;
                    break;
                case MagicSpellFlags.HolyOrb:
                    weapon_type = 0;
                    harm_type = (int)Config.monsters_defense.CD_EVIL;
                    destroy = inventory.setNull;
                    break;
                default:
                    weapon_type = 0;
                    harm_type = 0;
                    destroy = inventory.setNull;
                    printMessage("ERROR in spellGetAreaAffectFlags()\n");
                    break;
            }
        }



        

        

        

        

        // Recharge a wand, staff, or rod.  Sometimes the item breaks. -RAK-
        public static bool spellRechargeItem(int number_of_charges)
        {
            var py = State.Instance.py;
            int item_pos_start = 0, item_pos_end = 0;
            if (!inventoryManager.inventoryFindRange((int)TV_STAFF, (int)TV_WAND, ref item_pos_start, ref item_pos_end))
            {
                printMessage("You have nothing to recharge.");
                return false;
            }

            var item_id = 0;
            if (!uiInventory.inventoryGetInputForItemId(out item_id, "Recharge which item?", item_pos_start, item_pos_end, /*CNIL*/null, /*CNIL*/null))
            {
                return false;
            }

            var item = py.inventory[item_id];

            // recharge  I = recharge(20) = 1/6  failure for empty 10th level wand
            // recharge II = recharge(60) = 1/10 failure for empty 10th level wand
            //
            // make it harder to recharge high level, and highly charged wands,
            // note that `fail_chance` can be negative, so check its value before
            // trying to call rnd.randomNumber().
            var fail_chance = number_of_charges + 50 - (int)item.depth_first_found - item.misc_use;

            // Automatic failure.
            if (fail_chance < 19)
            {
                fail_chance = 1;
            }
            else
            {
                fail_chance = rnd.randomNumber(fail_chance / 10);
            }

            if (fail_chance == 1)
            {
                printMessage("There is a bright flash of light.");
                inventoryManager.inventoryDestroyItem(item_id);
            }
            else
            {
                number_of_charges = number_of_charges / ((int)item.depth_first_found + 2) + 1;
                item.misc_use += 2 + rnd.randomNumber(number_of_charges);

                if (spellItemIdentified(item))
                {
                    spellItemRemoveIdentification(item);
                }

                itemIdentificationClearEmpty(item);
            }

            return true;
        }

        

        

        

        

        

        // Turn stone to mud, delete wall. -RAK-
        public static bool spellWallToMud(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var distance = 0;
            var turned = false;
            var finished = false;

            while (!finished)
            {
                helpers.movePosition(direction, ref coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                // note, this ray can move through walls as it turns them to mud
                if (distance == Config.treasure.OBJECT_BOLTS_MAX_RANGE)
                {
                    finished = true;
                }

                if (tile.feature_id >= MIN_CAVE_WALL && tile.feature_id != TILE_BOUNDARY_WALL)
                {
                    finished = true;

                    playerTunnelWall(coord, 1, 0);

                    if (dungeon.caveTileVisible(coord))
                    {
                        turned = true;
                        printMessage("The wall turns into mud.");
                    }
                }
                else if (tile.treasure_id != 0 && tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;

                    if (coordInsidePanel(coord) && dungeon.caveTileVisible(coord))
                    {
                        turned = true;

                        var description = string.Empty;
                        itemDescription(ref description, game.treasure.list[tile.treasure_id], false);

                        var out_val = $"The {description} turns into mud.";
                        //obj_desc_t out_val = { '\0' };
                        //(void)sprintf(out_val, "The %s turns into mud.", description);
                        printMessage(out_val);
                    }

                    if (game.treasure.list[tile.treasure_id].category_id == TV_RUBBLE)
                    {
                        dungeon.dungeonDeleteObject(coord);
                        if (rnd.randomNumber(10) == 1)
                        {
                            dungeonPlacer.dungeonPlaceRandomObjectAt(coord, false);
                            if (dungeon.caveTileVisible(coord))
                            {
                                printMessage("You have found something!");
                            }
                        }
                        dungeon.dungeonLiteSpot(coord);
                    }
                    else
                    {
                        dungeon.dungeonDeleteObject(coord);
                    }
                }

                if (tile.creature_id > 1)
                {
                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if ((creature.defenses & Config.monsters_defense.CD_STONE) != 0)
                    {
                        var name = monsterNameDescription(creature.name, monster.lit);

                        // Should get these messages even if the monster is not visible.
                        var creature_id = monsterTakeHit((int)tile.creature_id, 100);
                        if (creature_id >= 0)
                        {
                            State.Instance.creature_recall[creature_id].defenses |= Config.monsters_defense.CD_STONE;
                            printMonsterActionText(name, "dissolves!");
                            displayCharacterExperience(); // print msg before calling prt_exp
                        }
                        else
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_STONE;
                            printMonsterActionText(name, "grunts in pain!");
                        }
                        finished = true;
                    }
                }
            }

            return turned;
        }

        

        

        // Create a wall. -RAK-
        public static bool spellBuildWall(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var built = false;
            var finished = false;

            while (!finished)
            {
                helpers.movePosition(direction, ref coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue; // we're done here, break out of the loop
                }

                if (tile.treasure_id != 0)
                {
                    dungeon.dungeonDeleteObject(coord);
                }

                if (tile.creature_id > 1)
                {
                    finished = true;

                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if ((creature.movement & Config.monsters_move.CM_PHASE) == 0u)
                    {
                        // monster does not move, can't escape the wall
                        int damage;
                        if ((creature.movement & Config.monsters_move.CM_ATTACK_ONLY) != 0u)
                        {
                            // this will kill everything
                            damage = 3000;
                        }
                        else
                        {
                            damage = dice.diceRoll(new Dice_t(4, 8));
                        }

                        var name = monsterNameDescription(creature.name, monster.lit);

                        printMonsterActionText(name, "wails out in pain!");

                        if (monsterTakeHit((int)tile.creature_id, damage) >= 0)
                        {
                            printMonsterActionText(name, "is embedded in the rock.");
                            displayCharacterExperience();
                        }
                    }
                    else if (creature.sprite == 'E' || creature.sprite == 'X')
                    {
                        // must be an earth elemental, an earth spirit,
                        // or a Xorn to increase its hit points
                        monster.hp += dice.diceRoll(new Dice_t(4, 8));
                    }
                }

                tile.feature_id = TILE_MAGMA_WALL;
                tile.field_mark = false;

                // Permanently light this wall if it is lit by player's lamp.
                tile.permanent_light = tile.temporary_light || tile.permanent_light;
                dungeon.dungeonLiteSpot(coord);

                built = true;
            }

            return built;
        }

        // Replicate a creature -RAK-
        public static bool spellCloneMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var distance = 0;
            var finished = false;

            while (!finished)
            {
                helpers.movePosition(direction, ref coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                }
                else if (tile.creature_id > 1)
                {
                    State.Instance.monsters[tile.creature_id].sleep_count = 0;

                    // monptr of 0 is safe here, since can't reach here from creatures
                    return monsterMultiply(coord, (int)State.Instance.monsters[tile.creature_id].creature_id, 0);
                }
            }

            return false;
        }

        

        // Teleport player to spell casting creature -RAK-
        public static void spellTeleportPlayerTo(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            var distance = 1;
            var counter = 0;

            var rnd_coord = new Coord_t(0, 0);

            do
            {
                rnd_coord.y = coord.y + (rnd.randomNumber(2 * distance + 1) - (distance + 1));
                rnd_coord.x = coord.x + (rnd.randomNumber(2 * distance + 1) - (distance + 1));
                counter++;
                if (counter > 9)
                {
                    counter = 0;
                    distance++;
                }
            } while (!dungeon.coordInBounds(rnd_coord) || dg.floor[rnd_coord.y][rnd_coord.x].feature_id >= MIN_CLOSED_SPACE || dg.floor[rnd_coord.y][rnd_coord.x].creature_id >= 2);

            dungeon.dungeonMoveCreatureRecord(py.pos, rnd_coord);

            var spot = new Coord_t(0, 0);
            for (spot.y = py.pos.y - 1; spot.y <= py.pos.y + 1; spot.y++)
            {
                for (spot.x = py.pos.x - 1; spot.x <= py.pos.x + 1; spot.x++)
                {
                    dg.floor[spot.y][spot.x].temporary_light = false;
                    dungeon.dungeonLiteSpot(spot);
                }
            }

            dungeon.dungeonLiteSpot(py.pos);

            //py.pos.y = rnd_coord.y;
            //py.pos.x = rnd_coord.x;
            py.pos = rnd_coord;

            dungeonResetView();

            // light creatures
            updateMonsters(false);
        }

        

        // Change players hit points in some manner -RAK-
        public static bool spellChangePlayerHitPoints(int adjustment)
        {
            var py = State.Instance.py;
            if (py.misc.current_hp >= py.misc.max_hp)
            {
                return false;
            }

            py.misc.current_hp += adjustment;
            if (py.misc.current_hp > py.misc.max_hp)
            {
                py.misc.current_hp = py.misc.max_hp;
                py.misc.current_hp_fraction = 0;
            }
            printCharacterCurrentHitPoints();

            adjustment = adjustment / 5;

            if (adjustment < 3)
            {
                if (adjustment == 0)
                {
                    printMessage("You feel a little better.");
                }
                else
                {
                    printMessage("You feel better.");
                }
            }
            else
            {
                if (adjustment < 7)
                {
                    printMessage("You feel much better.");
                }
                else
                {
                    printMessage("You feel very good.");
                }
            }

            return true;
        }

        

        

        

        

        

        // Lose a strength point. -RAK-
        public static void spellLoseSTR()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_str)
            {
                playerStatRandomDecrease((int)PlayerAttr.STR);
                printMessage("You feel very sick.");
            }
            else
            {
                printMessage("You feel sick for a moment,  it passes.");
            }
        }

        // Lose an intelligence point. -RAK-
        public static void spellLoseINT()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_int)
            {
                playerStatRandomDecrease((int)PlayerAttr.INT);
                printMessage("You become very dizzy.");
            }
            else
            {
                printMessage("You become dizzy for a moment,  it passes.");
            }
        }

        // Lose a wisdom point. -RAK-
        public static void spellLoseWIS()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_wis)
            {
                playerStatRandomDecrease((int)PlayerAttr.WIS);
                printMessage("You feel very naive.");
            }
            else
            {
                printMessage("You feel naive for a moment,  it passes.");
            }
        }

        // Lose a dexterity point. -RAK-
        public static void spellLoseDEX()
        {
            var py = State.Instance.py;

            if (!py.flags.sustain_dex)
            {
                playerStatRandomDecrease((int)PlayerAttr.DEX);
                printMessage("You feel very sore.");
            }
            else
            {
                printMessage("You feel sore for a moment,  it passes.");
            }
        }

        // Lose a constitution point. -RAK-
        public static void spellLoseCON()
        {
            var py = State.Instance.py;
            if (!py.flags.sustain_con)
            {
                playerStatRandomDecrease((int)PlayerAttr.CON);
                printMessage("You feel very sick.");
            }
            else
            {
                printMessage("You feel sick for a moment,  it passes.");
            }
        }

        // Lose a charisma point. -RAK-
        public static void spellLoseCHR()
        {
            var py = State.Instance.py;
            if (!py.flags.sustain_chr)
            {
                playerStatRandomDecrease((int)PlayerAttr.CHR);
                printMessage("Your skin starts to itch.");
            }
            else
            {
                printMessage("Your skin starts to itch, but feels better now.");
            }
        }

        // Lose experience -RAK-
        public static void spellLoseEXP(int adjustment)
        {
            var py = State.Instance.py;
            if (adjustment > py.misc.exp)
            {
                py.misc.exp = 0;
            }
            else
            {
                py.misc.exp -= adjustment;
            }
            displayCharacterExperience();

            var exp = 0;
            while ((int)(py.base_exp_levels[exp] * py.misc.experience_factor / 100) <= py.misc.exp)
            {
                exp++;
            }

            // increment exp once more, because level 1 exp is stored in player_base_exp_levels[0]
            exp++;

            if (py.misc.level != exp)
            {
                py.misc.level = (uint)exp;

                playerCalculateHitPoints();

                var character_class = Library.Instance.Player.classes[(int)py.misc.class_id];

                if (character_class.class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
                {
                    playerCalculateAllowedSpellsCount((int)PlayerAttr.INT);
                    playerGainMana((int)PlayerAttr.INT);
                }
                else if (character_class.class_to_use_mage_spells == Config.spells.SPELL_TYPE_PRIEST)
                {
                    playerCalculateAllowedSpellsCount((int)PlayerAttr.WIS);
                    playerGainMana((int)PlayerAttr.WIS);
                }
                printCharacterLevel();
                printCharacterTitle();
            }
        }

        // Enchants a plus onto an item. -RAK-
        // `limit` param is the maximum bonus allowed; usually 10,
        // but weapon's maximum damage when enchanting melee weapons to damage.
        public static bool spellEnchantItem(ref int plusses, int max_bonus_limit)
        {
            // avoid rnd.randomNumber(0) call
            if (max_bonus_limit <= 0)
            {
                return false;
            }

            var chance = 0;

            if (plusses > 0)
            {
                chance = plusses;

                // very rarely allow enchantment over limit
                if (rnd.randomNumber(100) == 1)
                {
                    chance = rnd.randomNumber(chance) - 1;
                }
            }

            if (rnd.randomNumber(max_bonus_limit) > chance)
            {
                plusses += 1;
                return true;
            }

            return false;
        }

        // Removes curses from items in inventory -RAK-
        public static bool spellRemoveCurseFromAllItems()
        {
            var py = State.Instance.py;

            var removed = false;

            for (var id = (int)PlayerEquipment.Wield; id <= (int)PlayerEquipment.Outer; id++)
            {
                if ((py.inventory[id].flags & Config.treasure_flags.TR_CURSED) != 0u)
                {
                    py.inventory[id].flags &= ~Config.treasure_flags.TR_CURSED;
                    playerRecalculateBonuses();
                    removed = true;
                }
            }

            return removed;
        }

        // Restores any drained experience -RAK-
        public static bool spellRestorePlayerLevels()
        {
            var py = State.Instance.py;

            if (py.misc.max_exp > py.misc.exp)
            {
                printMessage("You feel your life energies returning.");

                // this while loop is not redundant, ptr_exp may reduce the exp level
                while (py.misc.exp < py.misc.max_exp)
                {
                    py.misc.exp = py.misc.max_exp;
                    displayCharacterExperience();
                }

                return true;
            }

            return false;
        }

    }
}
