﻿using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using Moria.Core.Data;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dice_m;
using static Moria.Core.Methods.Dungeon_los_m;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Mage_spells_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Monster_manager_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_inventory_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Spells_m
    {
        // Returns spell pointer -RAK-
        public static bool spellGetId(int[] spell_ids, int number_of_choices, ref int spell_id, ref int spell_chance, string prompt, int first_spell)
        {
            var py = State.Instance.py;
            var magic_spells = State.Instance.magic_spells;
            var spell_names = State.Instance.spell_names;
            spell_id = -1;

            string str = string.Empty;
            //vtype_t str = { '\0' };
            str = string.Format(
                "(Spells {0}-{1}, *=List, <ESCAPE>=exit) {2}",
                spell_ids[0] + 'a' - first_spell, spell_ids[number_of_choices - 1] + 'a' - first_spell, prompt
            );
            //(void)sprintf(str, "(Spells %c-%c, *=List, <ESCAPE>=exit) %s", spell_ids[0] + 'a' - first_spell, spell_ids[number_of_choices - 1] + 'a' - first_spell, prompt);

            bool spell_found = false;
            bool redraw = false;

            int offset = (State.Instance.classes[py.misc.class_id].class_to_use_mage_spells == (int)Config.spells.SPELL_TYPE_MAGE ? (int)Config.spells.NAME_OFFSET_SPELLS : (int)Config.spells.NAME_OFFSET_PRAYERS);

            char choice = '\0';

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
                        var spell = magic_spells[py.misc.class_id - 1][spell_id];

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
                    var spellOrPrayer = (offset == Config.spells.NAME_OFFSET_SPELLS ? "spell" : "prayer");
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
            uint flags = py.inventory[item_id].flags;
            int first_spell = getAndClearFirstBit(ref flags);
            flags = py.inventory[item_id].flags & py.flags.spells_learnt;

            // TODO(cook) move access to `magic_spells[]` directly to the for loop it's used in, below?
            var spells = State.Instance.magic_spells[py.misc.class_id - 1];

            int spell_count = 0;
            var spell_list = new int[31];

            while (flags != 0u)
            {
                int pos = getAndClearFirstBit(ref flags);

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

            int result = 0;
            if (spellGetId(spell_list, spell_count, ref spell_id, ref spell_chance, prompt, first_spell))
            {
                result = 1;
            }

            if ((result != 0) && State.Instance.magic_spells[py.misc.class_id - 1][spell_id].mana_required > py.misc.current_mana)
            {
                if (State.Instance.classes[py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
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

        // Following are spell procedure/functions -RAK-
        // These routines are commonly used in the scroll, potion, wands, and
        // staves routines, and are occasionally called from other areas.
        // Now included are creature spells also.           -RAK

        // Detect any treasure on the current panel -RAK-
        public static bool spellDetectTreasureWithinVicinity()
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            bool detected = false;

            Coord_t coord = new Coord_t(0, 0);

            for (coord.y = dg.panel.top; coord.y <= dg.panel.bottom; coord.y++)
            {
                for (coord.x = dg.panel.left; coord.x <= dg.panel.right; coord.x++)
                {
                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.treasure_id != 0 && game.treasure.list[tile.treasure_id].category_id == TV_GOLD && !caveTileVisible(coord))
                    {
                        tile.field_mark = true;
                        dungeonLiteSpot(coord);
                        detected = true;
                    }
                }
            }

            return detected;
        }

        // Detect all objects on the current panel -RAK-
        public static bool spellDetectObjectsWithinVicinity()
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            bool detected = false;

            Coord_t coord = new Coord_t(0, 0);

            for (coord.y = dg.panel.top; coord.y <= dg.panel.bottom; coord.y++)
            {
                for (coord.x = dg.panel.left; coord.x <= dg.panel.right; coord.x++)
                {
                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.treasure_id != 0 && game.treasure.list[tile.treasure_id].category_id < TV_MAX_OBJECT && !caveTileVisible(coord))
                    {
                        tile.field_mark = true;
                        dungeonLiteSpot(coord);
                        detected = true;
                    }
                }
            }

            return detected;
        }

        // Locates and displays traps on current panel -RAK-
        public static bool spellDetectTrapsWithinVicinity()
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            bool detected = false;

            Coord_t coord = new Coord_t(0, 0);

            for (coord.y = dg.panel.top; coord.y <= dg.panel.bottom; coord.y++)
            {
                for (coord.x = dg.panel.left; coord.x <= dg.panel.right; coord.x++)
                {
                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.treasure_id == 0)
                    {
                        continue;
                    }

                    if (game.treasure.list[tile.treasure_id].category_id == TV_INVIS_TRAP)
                    {
                        tile.field_mark = true;
                        trapChangeVisibility(coord);
                        detected = true;
                    }
                    else if (game.treasure.list[tile.treasure_id].category_id == TV_CHEST)
                    {
                        var item = game.treasure.list[tile.treasure_id];
                        spellItemIdentifyAndRemoveRandomInscription(item);
                    }
                }
            }

            return detected;
        }

        // Locates and displays all secret doors on current panel -RAK-
        public static bool spellDetectSecretDoorssWithinVicinity()
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            bool detected = false;

            Coord_t coord = new Coord_t(0, 0);

            for (coord.y = dg.panel.top; coord.y <= dg.panel.bottom; coord.y++)
            {
                for (coord.x = dg.panel.left; coord.x <= dg.panel.right; coord.x++)
                {
                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.treasure_id == 0)
                    {
                        continue;
                    }

                    if (game.treasure.list[tile.treasure_id].category_id == TV_SECRET_DOOR)
                    {
                        // Secret doors

                        tile.field_mark = true;
                        trapChangeVisibility(coord);
                        detected = true;
                    }
                    else if ((game.treasure.list[tile.treasure_id].category_id == TV_UP_STAIR || game.treasure.list[tile.treasure_id].category_id == TV_DOWN_STAIR) && !tile.field_mark)
                    {
                        // Staircases

                        tile.field_mark = true;
                        dungeonLiteSpot(coord);
                        detected = true;
                    }
                }
            }

            return detected;
        }

        // Locates and displays all invisible creatures on current panel -RAK-
        public static bool spellDetectInvisibleCreaturesWithinVicinity()
        {
            bool detected = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (coordInsidePanel(new Coord_t(monster.pos.y, monster.pos.x)) &&
                    ((Library.Instance.Creatures.creatures_list[(int)monster.creature_id].movement & Config.monsters_move.CM_INVISIBLE) != 0u))
                {
                    monster.lit = true;

                    // works correctly even if hallucinating
                    panelPutTile((char)Library.Instance.Creatures.creatures_list[(int)monster.creature_id].sprite, new Coord_t(monster.pos.y, monster.pos.x));

                    detected = true;
                }
            }

            if (detected)
            {
                printMessage("You sense the presence of invisible creatures!");
                printMessage(/*CNIL*/null);

                // must unlight every monster just lighted
                updateMonsters(false);
            }

            return detected;
        }

        // Light an area: -RAK-
        //     1.  If corridor  light immediate area
        //     2.  If room      light entire room plus immediate area.
        public static bool spellLightArea(Coord_t coord)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            if (py.flags.blind < 1)
            {
                printMessage("You are surrounded by a white light.");
            }

            // NOTE: this is not changed anywhere. A bug or correct? -MRC-
            bool lit = true;

            if (dg.floor[coord.y][coord.x].perma_lit_room && dg.current_level > 0)
            {
                dungeonLightRoom(coord);
            }

            // Must always light immediate area, because one might be standing on
            // the edge of a room, or next to a destroyed area, etc.
            Coord_t spot = new Coord_t(0, 0);
            for (spot.y = coord.y - 1; spot.y <= coord.y + 1; spot.y++)
            {
                for (spot.x = coord.x - 1; spot.x <= coord.x + 1; spot.x++)
                {
                    dg.floor[spot.y][spot.x].permanent_light = true;
                    dungeonLiteSpot(spot);
                }
            }

            return lit;
        }

        // Darken an area, opposite of light area -RAK-
        public static bool spellDarkenArea(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            bool darkened = false;

            Coord_t spot = new Coord_t(0, 0);

            if (dg.floor[coord.y][coord.x].perma_lit_room && dg.current_level > 0)
            {
                int half_height = (int)(SCREEN_HEIGHT / 2);
                int half_width = (int)(SCREEN_WIDTH / 2);
                int start_row = (coord.y / half_height) * half_height + 1;
                int start_col = (coord.x / half_width) * half_width + 1;
                int end_row = start_row + half_height - 1;
                int end_col = start_col + half_width - 1;

                for (spot.y = start_row; spot.y <= end_row; spot.y++)
                {
                    for (spot.x = start_col; spot.x <= end_col; spot.x++)
                    {
                        var tile = dg.floor[spot.y][spot.x];

                        if (tile.perma_lit_room && tile.feature_id <= MAX_CAVE_FLOOR)
                        {
                            tile.permanent_light = false;
                            tile.feature_id = TILE_DARK_FLOOR;

                            dungeonLiteSpot(spot);

                            if (!caveTileVisible(spot))
                            {
                                darkened = true;
                            }
                        }
                    }
                }
            }
            else
            {
                for (spot.y = coord.y - 1; spot.y <= coord.y + 1; spot.y++)
                {
                    for (spot.x = coord.x - 1; spot.x <= coord.x + 1; spot.x++)
                    {
                        var tile = dg.floor[spot.y][spot.x];

                        if (tile.feature_id == TILE_CORR_FLOOR && tile.permanent_light)
                        {
                            // permanent_light could have been set by star-lite wand, etc
                            tile.permanent_light = false;
                            darkened = true;
                        }
                    }
                }
            }

            if (darkened && py.flags.blind < 1)
            {
                printMessage("Darkness surrounds you.");
            }

            return darkened;
        }

        public static void dungeonLightAreaAroundFloorTile(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            Coord_t spot = new Coord_t(0, 0);

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

            int row_min = dg.panel.top - randomNumber(10);
            int row_max = dg.panel.bottom + randomNumber(10);
            int col_min = dg.panel.left - randomNumber(20);
            int col_max = dg.panel.right + randomNumber(20);

            Coord_t coord = new Coord_t(0, 0);

            for (coord.y = row_min; coord.y <= row_max; coord.y++)
            {
                for (coord.x = col_min; coord.x <= col_max; coord.x++)
                {
                    if (coordInBounds(coord) && dg.floor[coord.y][coord.x].feature_id <= MAX_CAVE_FLOOR)
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
            int item_id = 0;
            if (!inventoryGetInputForItemId(ref item_id, "Item you wish identified?", 0, (int)PLAYER_INVENTORY_SIZE, /*CNIL*/null, /*CNIL*/null))
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
                msg = $"{playerItemWearingDescription(item_id)}: {description}";
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
            bool aggravated = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
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

            Coord_t coord = new Coord_t(0, 0);

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
                            dungeonDeleteObject(coord);
                        }

                        dungeonSetTrap(coord, randomNumber(Config.dungeon_objects.MAX_TRAPS) - 1);

                        // don't let player gain exp from the newly created traps
                        game.treasure.list[tile.treasure_id].misc_use = 0;

                        // open pits are immediately visible, so call dungeonLiteSpot
                        dungeonLiteSpot(coord);
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

            bool created = false;

            Coord_t coord = new Coord_t(0, 0);

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
                            dungeonDeleteObject(coord);
                        }

                        int free_id = popt();
                        tile.feature_id = TILE_BLOCKED_FLOOR;
                        tile.treasure_id = (uint)free_id;

                        inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, game.treasure.list[free_id]);
                        dungeonLiteSpot(coord);

                        created = true;
                    }
                }
            }

            return created;
        }

        // Destroys any adjacent door(s)/trap(s) -RAK-
        public static bool spellDestroyAdjacentDoorsTraps()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            bool destroyed = false;

            Coord_t coord = new Coord_t(0, 0);

            for (coord.y = py.pos.y - 1; coord.y <= py.pos.y + 1; coord.y++)
            {
                for (coord.x = py.pos.x - 1; coord.x <= py.pos.x + 1; coord.x++)
                {
                    var tile = dg.floor[coord.y][coord.x];

                    if (tile.treasure_id == 0)
                    {
                        continue;
                    }

                    var item = game.treasure.list[tile.treasure_id];

                    if ((item.category_id >= TV_INVIS_TRAP && item.category_id <= TV_CLOSED_DOOR && item.category_id != TV_RUBBLE) || item.category_id == TV_SECRET_DOOR)
                    {
                        if (dungeonDeleteObject(coord))
                        {
                            destroyed = true;
                        }
                    }
                    else if (item.category_id == TV_CHEST && item.flags != 0)
                    {
                        // destroy traps on chest and unlock
                        item.flags &= ~(Config.treasure_chests.CH_TRAPPED | Config.treasure_chests.CH_LOCKED);
                        item.special_name_id = (int)SpecialNameIds.SN_UNLOCKED;

                        destroyed = true;

                        printMessage("You have disarmed the chest.");
                        spellItemIdentifyAndRemoveRandomInscription(item);
                    }
                }
            }

            return destroyed;
        }

        // Display all creatures on the current panel -RAK-
        public static bool spellDetectMonsters()
        {
            bool detected = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (coordInsidePanel(new Coord_t(monster.pos.y, monster.pos.x)) && (Library.Instance.Creatures.creatures_list[(int)monster.creature_id].movement & Config.monsters_move.CM_INVISIBLE) == 0)
                {
                    monster.lit = true;
                    detected = true;

                    // works correctly even if hallucinating
                    panelPutTile((char)Library.Instance.Creatures.creatures_list[(int)monster.creature_id].sprite, new Coord_t(monster.pos.y, monster.pos.x));
                }
            }

            if (detected)
            {
                printMessage("You sense the presence of monsters!");
                printMessage(/*CNIL*/null);

                // must unlight every monster just lighted
                updateMonsters(false);
            }

            return detected;
        }

        // Update monster when light line spell touches it.
        public static void spellLightLineTouchesMonster(int monster_id)
        {
            var monster = State.Instance.monsters[monster_id];
            var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

            // light up and draw monster
            monsterUpdateVisibility(monster_id);

            var name = monsterNameDescription(creature.name, monster.lit);

            if ((creature.defenses & Config.monsters_defense.CD_LIGHT) != 0)
            {
                if (monster.lit)
                {
                    State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_LIGHT;
                }

                if (monsterTakeHit(monster_id, diceRoll(new Dice_t(2, 8))) >= 0)
                {
                    printMonsterActionText(name, "shrivels away in the light!");
                    displayCharacterExperience();
                }
                else
                {
                    printMonsterActionText(name, "cringes from the light!");
                }
            }
        }

        // Leave a line of light in given dir, blue light can sometimes hurt creatures. -RAK-
        public static void spellLightLine(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            int distance = 0;
            bool finished = false;

            Coord_t tmp_coord = new Coord_t(0, 0);

            while (!finished)
            {
                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    playerMovePosition(direction, coord);
                    finished = true;
                    continue; // we're done here, break out of the loop
                }

                if (!tile.permanent_light && !tile.temporary_light)
                {
                    // set permanent_light so that dungeonLiteSpot will work
                    tile.permanent_light = true;

                    // coord y/x need to be maintained, so copy them
                    tmp_coord.y = coord.y;
                    tmp_coord.x = coord.x;

                    if (tile.feature_id == TILE_LIGHT_FLOOR)
                    {
                        if (coordInsidePanel(tmp_coord))
                        {
                            dungeonLightRoom(tmp_coord);
                        }
                    }
                    else
                    {
                        dungeonLiteSpot(tmp_coord);
                    }
                }

                // set permanent_light in case temporary_light was true above
                tile.permanent_light = true;

                if (tile.creature_id > 1)
                {
                    spellLightLineTouchesMonster((int)tile.creature_id);
                }

                // move must be at end because want to light up current tmp_coord
                playerMovePosition(direction, coord);
                distance++;
            }
        }

        // Light line in all directions -RAK-
        public static void spellStarlite(Coord_t coord)
        {
            var py = State.Instance.py;
            if (py.flags.blind < 1)
            {
                printMessage("The end of the staff bursts into a blue shimmering light.");
            }

            for (int dir = 1; dir <= 9; dir++)
            {
                if (dir != 5)
                {
                    spellLightLine(coord, dir);
                }
            }
        }

        // Disarms all traps/chests in a given direction -RAK-
        public static bool spellDisarmAllInDirection(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            int distance = 0;
            bool disarmed = false;

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
                        if (dungeonDeleteObject(coord))
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
                        trapChangeVisibility(coord);
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
                playerMovePosition(direction, coord);

                distance++;
            } while (distance <= Config.treasure.OBJECT_BOLTS_MAX_RANGE && tile.feature_id <= MAX_OPEN_SPACE);

            return disarmed;
        }

        // Return flags for given type area affect -RAK-
        public static void spellGetAreaAffectFlags(int spell_type, ref uint weapon_type, ref int harm_type, ref Func<Inventory_t, bool> destroy)
        {
            switch ((MagicSpellFlags)spell_type)
            {
                case MagicSpellFlags.MagicMissile:
                    weapon_type = 0;
                    harm_type = 0;
                    destroy = setNull;
                    break;
                case MagicSpellFlags.Lightning:
                    weapon_type = Config.monsters_spells.CS_BR_LIGHT;
                    harm_type = (int)Config.monsters_defense.CD_LIGHT;
                    destroy = setLightningDestroyableItems;
                    break;
                case MagicSpellFlags.PoisonGas:
                    weapon_type = Config.monsters_spells.CS_BR_GAS;
                    harm_type = (int)Config.monsters_defense.CD_POISON;
                    destroy = setNull;
                    break;
                case MagicSpellFlags.Acid:
                    weapon_type = Config.monsters_spells.CS_BR_ACID;
                    harm_type = (int)Config.monsters_defense.CD_ACID;
                    destroy = setAcidDestroyableItems;
                    break;
                case MagicSpellFlags.Frost:
                    weapon_type = Config.monsters_spells.CS_BR_FROST;
                    harm_type = (int)Config.monsters_defense.CD_FROST;
                    destroy = setFrostDestroyableItems;
                    break;
                case MagicSpellFlags.Fire:
                    weapon_type = Config.monsters_spells.CS_BR_FIRE;
                    harm_type = (int)Config.monsters_defense.CD_FIRE;
                    destroy = setFireDestroyableItems;
                    break;
                case MagicSpellFlags.HolyOrb:
                    weapon_type = 0;
                    harm_type = (int)Config.monsters_defense.CD_EVIL;
                    destroy = setNull;
                    break;
                default:
                    printMessage("ERROR in spellGetAreaAffectFlags()\n");
                    break;
            }
        }

        public static void printBoltStrikesMonsterMessage(Creature_t creature, string bolt_name, bool is_lit)
        {
            string monster_name;
            if (is_lit)
            {
                monster_name = "the " + creature.name;
            }
            else
            {
                monster_name = "it";
            }
            var msg = "The " + bolt_name + " strikes " + monster_name + ".";
            printMessage(msg);
        }

        // Light up, draw, and check for monster damage when Fire Bolt touches it.
        public static void spellFireBoltTouchesMonster(Tile_t tile, int damage, int harm_type, uint weapon_id, string bolt_name)
        {
            var monster = State.Instance.monsters[tile.creature_id];
            var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

            // light up monster and draw monster, temporarily set
            // permanent_light so that `monsterUpdateVisibility()` will work
            bool saved_lit_status = tile.permanent_light;
            tile.permanent_light = true;
            monsterUpdateVisibility((int)tile.creature_id);
            tile.permanent_light = saved_lit_status;

            // draw monster and clear previous bolt
            putQIO();

            printBoltStrikesMonsterMessage(creature, bolt_name, monster.lit);

            if ((harm_type & creature.defenses) != 0)
            {
                damage = damage * 2;
                if (monster.lit)
                {
                    State.Instance.creature_recall[monster.creature_id].defenses |= (uint)harm_type;
                }
            }
            else if ((weapon_id & creature.spells) != 0u)
            {
                damage = damage / 4;
                if (monster.lit)
                {
                    State.Instance.creature_recall[monster.creature_id].spells |= weapon_id;
                }
            }

            var name = monsterNameDescription(creature.name, monster.lit);

            if (monsterTakeHit((int)tile.creature_id, damage) >= 0)
            {
                printMonsterActionText(name, "dies in a fit of agony.");
                displayCharacterExperience();
            }
            else if (damage > 0)
            {
                printMonsterActionText(name, "screams in agony.");
            }
        }

        // Shoot a bolt in a given direction -RAK-
        public static void spellFireBolt(Coord_t coord, int direction, int damage_hp, int spell_type, string spell_name)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            Func<Inventory_t, bool> dummy = null;
            //bool(*dummy)(Inventory_t *);
            int harm_type = 0;
            uint weapon_type = 0;
            spellGetAreaAffectFlags(spell_type, ref weapon_type, ref harm_type, ref dummy);

            Coord_t old_coord = new Coord_t(0, 0);

            int distance = 0;
            bool finished = false;

            while (!finished)
            {
                old_coord.y = coord.y;
                old_coord.x = coord.x;
                playerMovePosition(direction, coord);

                distance++;

                var tile = dg.floor[coord.y][coord.x];

                dungeonLiteSpot(old_coord);

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue; // we're done here, break out of the loop
                }

                if (tile.creature_id > 1)
                {
                    finished = true;
                    spellFireBoltTouchesMonster(tile, damage_hp, harm_type, weapon_type, spell_name);
                }
                else if (coordInsidePanel(coord) && py.flags.blind < 1)
                {
                    panelPutTile('*', coord);

                    // show the bolt
                    putQIO();
                }
            }
        }

        // Shoot a ball in a given direction.  Note that balls have an area affect. -RAK-
        public static void spellFireBall(Coord_t coord, int direction, int damage_hp, int spell_type, string spell_name)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            int total_hits = 0;
            int total_kills = 0;
            int max_distance = 2;

            Func<Inventory_t, bool> destroy = null;
            int harm_type = 0;
            uint weapon_type = 0;
            spellGetAreaAffectFlags(spell_type, ref weapon_type, ref harm_type, ref destroy);

            Coord_t old_coord = new Coord_t(0, 0);
            Coord_t spot = new Coord_t(0, 0);

            int distance = 0;
            bool finished = false;

            while (!finished)
            {
                old_coord.y = coord.y;
                old_coord.x = coord.x;
                playerMovePosition(direction, coord);

                distance++;

                dungeonLiteSpot(old_coord);

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE)
                {
                    finished = true;
                    continue;
                }

                var tile = dg.floor[coord.y][coord.x];

                if (tile.feature_id >= MIN_CLOSED_SPACE || tile.creature_id > 1)
                {
                    finished = true;

                    if (tile.feature_id >= MIN_CLOSED_SPACE)
                    {
                        coord.y = old_coord.y;
                        coord.x = old_coord.x;
                    }

                    // The ball hits and explodes.

                    // The explosion.
                    for (int row = coord.y - max_distance; row <= coord.y + max_distance; row++)
                    {
                        for (int col = coord.x - max_distance; col <= coord.x + max_distance; col++)
                        {
                            spot.y = row;
                            spot.x = col;

                            if (coordInBounds(spot) && coordDistanceBetween(coord, spot) <= max_distance && los(coord, spot))
                            {
                                tile = dg.floor[spot.y][spot.x];

                                if (tile.treasure_id != 0 && destroy(game.treasure.list[tile.treasure_id])) // TOFIX: destroy can be null
                                {
                                    dungeonDeleteObject(spot);
                                }

                                if (tile.feature_id <= MAX_OPEN_SPACE)
                                {
                                    if (tile.creature_id > 1)
                                    {
                                        var monster = State.Instance.monsters[tile.creature_id];
                                        var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                                        // lite up creature if visible, temp set permanent_light so that monsterUpdateVisibility works
                                        bool saved_lit_status = tile.permanent_light;
                                        tile.permanent_light = true;
                                        monsterUpdateVisibility((int)tile.creature_id);

                                        total_hits++;
                                        int damage = damage_hp;

                                        if ((harm_type & creature.defenses) != 0)
                                        {
                                            damage = damage * 2;
                                            if (monster.lit)
                                            {
                                                State.Instance.creature_recall[monster.creature_id].defenses |= (uint)harm_type;
                                            }
                                        }
                                        else if ((weapon_type & creature.spells) != 0u)
                                        {
                                            damage = damage / 4;
                                            if (monster.lit)
                                            {
                                                State.Instance.creature_recall[monster.creature_id].spells |= weapon_type;
                                            }
                                        }

                                        damage = (damage / (coordDistanceBetween(spot, coord) + 1));

                                        if (monsterTakeHit((int)tile.creature_id, damage) >= 0)
                                        {
                                            total_kills++;
                                        }
                                        tile.permanent_light = saved_lit_status;
                                    }
                                    else if (coordInsidePanel(spot) && py.flags.blind < 1)
                                    {
                                        panelPutTile('*', spot);
                                    }
                                }
                            }
                        }
                    }

                    // show ball of whatever
                    putQIO();

                    for (int row = (coord.y - 2); row <= (coord.y + 2); row++)
                    {
                        for (int col = (coord.x - 2); col <= (coord.x + 2); col++)
                        {
                            spot.y = row;
                            spot.x = col;

                            if (coordInBounds(spot) && coordInsidePanel(spot) && coordDistanceBetween(coord, spot) <= max_distance)
                            {
                                dungeonLiteSpot(spot);
                            }
                        }
                    }
                    // End explosion.

                    if (total_hits == 1)
                    {
                        printMessage(("The " + spell_name + " envelops a creature!"));
                    }
                    else if (total_hits > 1)
                    {
                        printMessage(("The " + spell_name + " envelops several creatures!"));
                    }

                    if (total_kills == 1)
                    {
                        printMessage("There is a scream of agony!");
                    }
                    else if (total_kills > 1)
                    {
                        printMessage("There are several screams of agony!");
                    }

                    if (total_kills >= 0)
                    {
                        displayCharacterExperience();
                    }
                    // End ball hitting.
                }
                else if (coordInsidePanel(coord) && py.flags.blind < 1)
                {
                    panelPutTile('*', coord);

                    // show bolt
                    putQIO();
                }
            }
        }

        // Breath weapon works like a spellFireBall(), but affects the player.
        // Note the area affect. -RAK-
        public static void spellBreath(Coord_t coord, int monster_id, int damage_hp, int spell_type, string spell_name)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            int max_distance = 2;

            Func<Inventory_t, bool> destroy = null;
            //bool(*destroy)(Inventory_t *);
            int harm_type = 0;
            uint weapon_type = 0;
            spellGetAreaAffectFlags(spell_type, ref weapon_type, ref harm_type, ref destroy);

            Coord_t location = new Coord_t(0, 0);

            for (location.y = coord.y - 2; location.y <= coord.y + 2; location.y++)
            {
                for (location.x = coord.x - 2; location.x <= coord.x + 2; location.x++)
                {
                    if (coordInBounds(location) && coordDistanceBetween(coord, location) <= max_distance && los(coord, location))
                    {
                        var tile = dg.floor[location.y][location.x];

                        if (tile.treasure_id != 0 && destroy(game.treasure.list[tile.treasure_id])) // TOFIX: destroy can be null
                        {
                            dungeonDeleteObject(location);
                        }

                        if (tile.feature_id <= MAX_OPEN_SPACE)
                        {
                            // must test status bit, not py.flags.blind here, flag could have
                            // been set by a previous monster, but the breath should still
                            // be visible until the blindness takes effect
                            if (coordInsidePanel(location) && ((py.flags.status & Config.player_status.PY_BLIND) == 0u))
                            {
                                panelPutTile('*', location);
                            }

                            if (tile.creature_id > 1)
                            {
                                var monster = State.Instance.monsters[tile.creature_id];
                                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                                int damage = damage_hp;

                                if ((harm_type & creature.defenses) != 0)
                                {
                                    damage = damage * 2;
                                }
                                else if ((weapon_type & creature.spells) != 0u)
                                {
                                    damage = (damage / 4);
                                }

                                damage = (damage / (coordDistanceBetween(location, coord) + 1));

                                // can not call monsterTakeHit here, since player does not
                                // get experience for kill
                                monster.hp = (int)(monster.hp - damage);
                                monster.sleep_count = 0;

                                if (monster.hp < 0)
                                {
                                    uint treasure_id = monsterDeath(new Coord_t(monster.pos.y, monster.pos.x), creature.movement);

                                    if (monster.lit)
                                    {
                                        var tmp = (uint)((State.Instance.creature_recall[monster.creature_id].movement & Config.monsters_move.CM_TREASURE) >> (int)Config.monsters_move.CM_TR_SHIFT);
                                        if (tmp > ((treasure_id & Config.monsters_move.CM_TREASURE) >> (int)Config.monsters_move.CM_TR_SHIFT))
                                        {
                                            treasure_id = (uint)((treasure_id & ~Config.monsters_move.CM_TREASURE) | (tmp << (int)Config.monsters_move.CM_TR_SHIFT));
                                        }
                                        State.Instance.creature_recall[monster.creature_id].movement =
                                            (uint)(treasure_id | (State.Instance.creature_recall[monster.creature_id].movement & ~Config.monsters_move.CM_TREASURE));
                                    }

                                    // It ate an already processed monster. Handle normally.
                                    if (monster_id < tile.creature_id)
                                    {
                                        dungeonDeleteMonster((int)tile.creature_id);
                                    }
                                    else
                                    {
                                        // If it eats this monster, an already processed monster
                                        // will take its place, causing all kinds of havoc.
                                        // Delay the kill a bit.
                                        dungeonDeleteMonsterFix1((int)tile.creature_id);
                                    }
                                }
                            }
                            else if (tile.creature_id == 1)
                            {
                                int damage = (damage_hp / (coordDistanceBetween(location, coord) + 1));

                                // let's do at least one point of damage
                                // prevents randomNumber(0) problem with damagePoisonedGas, also
                                if (damage == 0)
                                {
                                    damage = 1;
                                }

                                switch ((MagicSpellFlags)spell_type)
                                {
                                    case MagicSpellFlags.Lightning:
                                        damageLightningBolt(damage, spell_name);
                                        break;
                                    case MagicSpellFlags.PoisonGas:
                                        damagePoisonedGas(damage, spell_name);
                                        break;
                                    case MagicSpellFlags.Acid:
                                        damageAcid(damage, spell_name);
                                        break;
                                    case MagicSpellFlags.Frost:
                                        damageCold(damage, spell_name);
                                        break;
                                    case MagicSpellFlags.Fire:
                                        damageFire(damage, spell_name);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            // show the ball of gas
            putQIO();

            Coord_t spot = new Coord_t(0, 0);
            for (spot.y = (coord.y - 2); spot.y <= (coord.y + 2); spot.y++)
            {
                for (spot.x = (coord.x - 2); spot.x <= (coord.x + 2); spot.x++)
                {
                    if (coordInBounds(spot) && coordInsidePanel(spot) && coordDistanceBetween(coord, spot) <= max_distance)
                    {
                        dungeonLiteSpot(spot);
                    }
                }
            }
        }

        // Recharge a wand, staff, or rod.  Sometimes the item breaks. -RAK-
        public static bool spellRechargeItem(int number_of_charges)
        {
            var py = State.Instance.py;
            int item_pos_start = 0, item_pos_end = 0;
            if (!inventoryFindRange((int)TV_STAFF, (int)TV_WAND, ref item_pos_start, ref item_pos_end))
            {
                printMessage("You have nothing to recharge.");
                return false;
            }

            int item_id = 0;
            if (!inventoryGetInputForItemId(ref item_id, "Recharge which item?", item_pos_start, item_pos_end, /*CNIL*/null, /*CNIL*/null))
            {
                return false;
            }

            var item = py.inventory[item_id];

            // recharge  I = recharge(20) = 1/6  failure for empty 10th level wand
            // recharge II = recharge(60) = 1/10 failure for empty 10th level wand
            //
            // make it harder to recharge high level, and highly charged wands,
            // note that `fail_chance` can be negative, so check its value before
            // trying to call randomNumber().
            int fail_chance = number_of_charges + 50 - (int)item.depth_first_found - item.misc_use;

            // Automatic failure.
            if (fail_chance < 19)
            {
                fail_chance = 1;
            }
            else
            {
                fail_chance = randomNumber(fail_chance / 10);
            }

            if (fail_chance == 1)
            {
                printMessage("There is a bright flash of light.");
                inventoryDestroyItem(item_id);
            }
            else
            {
                number_of_charges = (number_of_charges / ((int)item.depth_first_found + 2)) + 1;
                item.misc_use += 2 + randomNumber(number_of_charges);

                if (spellItemIdentified(item))
                {
                    spellItemRemoveIdentification(item);
                }

                itemIdentificationClearEmpty(item);
            }

            return true;
        }

        // Increase or decrease a creatures hit points -RAK-
        public static bool spellChangeMonsterHitPoints(Coord_t coord, int direction, int damage_hp)
        {
            var dg = State.Instance.dg;

            int distance = 0;
            bool changed = false;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue;
                }

                if (tile.creature_id > 1)
                {
                    finished = true;

                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    var name = monsterNameDescription(creature.name, monster.lit);

                    if (monsterTakeHit((int)tile.creature_id, damage_hp) >= 0)
                    {
                        printMonsterActionText(name, "dies in a fit of agony.");
                        displayCharacterExperience();
                    }
                    else if (damage_hp > 0)
                    {
                        printMonsterActionText(name, "screams in agony.");
                    }

                    changed = true;
                }
            }

            return changed;
        }

        // Drains life; note it must be living. -RAK-
        public static bool spellDrainLifeFromMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;

            int distance = 0;
            bool drained = false;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue;
                }

                if (tile.creature_id > 1)
                {
                    finished = true;

                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if ((creature.defenses & Config.monsters_defense.CD_UNDEAD) == 0)
                    {
                        var name = monsterNameDescription(creature.name, monster.lit);

                        if (monsterTakeHit((int)tile.creature_id, 75) >= 0)
                        {
                            printMonsterActionText(name, "dies in a fit of agony.");
                            displayCharacterExperience();
                        }
                        else
                        {
                            printMonsterActionText(name, "screams in agony.");
                        }

                        drained = true;
                    }
                    else
                    {
                        State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_UNDEAD;
                    }
                }
            }

            return drained;
        }

        // Increase or decrease a creatures speed -RAK-
        // NOTE: cannot slow a winning creature (BALROG)
        public static bool spellSpeedMonster(Coord_t coord, int direction, int speed)
        {
            var dg = State.Instance.dg;
            int distance = 0;
            bool changed = false;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue;
                }

                if (tile.creature_id > 1)
                {
                    finished = true;

                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    var name = monsterNameDescription(creature.name, monster.lit);

                    if (speed > 0)
                    {
                        monster.speed += speed;
                        monster.sleep_count = 0;

                        changed = true;

                        printMonsterActionText(name, "starts moving faster.");
                    }
                    else if (randomNumber(MON_MAX_LEVELS) > creature.level)
                    {
                        monster.speed += speed;
                        monster.sleep_count = 0;

                        changed = true;

                        printMonsterActionText(name, "starts moving slower.");
                    }
                    else
                    {
                        monster.sleep_count = 0;

                        printMonsterActionText(name, "is unaffected.");
                    }
                }
            }

            return changed;
        }

        // Confuse a creature -RAK-
        public static bool spellConfuseMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            int distance = 0;
            bool confused = false;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue;
                }

                if (tile.creature_id > 1)
                {
                    finished = true;

                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    var name = monsterNameDescription(creature.name, monster.lit);

                    if (randomNumber(MON_MAX_LEVELS) < creature.level || ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0))
                    {
                        if (monster.lit && ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0))
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_NO_SLEEP;
                        }

                        // Monsters which resisted the attack should wake up.
                        // Monsters with innate resistance ignore the attack.
                        if ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) == 0)
                        {
                            monster.sleep_count = 0;
                        }

                        printMonsterActionText(name, "is unaffected.");
                    }
                    else
                    {
                        if (monster.confused_amount != 0u)
                        {
                            monster.confused_amount += 3;
                        }
                        else
                        {
                            monster.confused_amount = (uint)(2 + randomNumber(16));
                        }
                        monster.sleep_count = 0;

                        confused = true;

                        printMonsterActionText(name, "appears confused.");
                    }
                }
            }

            return confused;
        }

        // Sleep a creature. -RAK-
        public static bool spellSleepMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            int distance = 0;
            bool asleep = false;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue;
                }

                if (tile.creature_id > 1)
                {
                    finished = true;

                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    var name = monsterNameDescription(creature.name, monster.lit);

                    if (randomNumber(MON_MAX_LEVELS) < creature.level || ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0))
                    {
                        if (monster.lit && ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0))
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_NO_SLEEP;
                        }

                        printMonsterActionText(name, "is unaffected.");
                    }
                    else
                    {
                        monster.sleep_count = 500;

                        asleep = true;

                        printMonsterActionText(name, "falls asleep.");
                    }
                }
            }

            return asleep;
        }

        // Turn stone to mud, delete wall. -RAK-
        public static bool spellWallToMud(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            int distance = 0;
            bool turned = false;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
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

                    if (caveTileVisible(coord))
                    {
                        turned = true;
                        printMessage("The wall turns into mud.");
                    }
                }
                else if (tile.treasure_id != 0 && tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;

                    if (coordInsidePanel(coord) && caveTileVisible(coord))
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
                        dungeonDeleteObject(coord);
                        if (randomNumber(10) == 1)
                        {
                            dungeonPlaceRandomObjectAt(coord, false);
                            if (caveTileVisible(coord))
                            {
                                printMessage("You have found something!");
                            }
                        }
                        dungeonLiteSpot(coord);
                    }
                    else
                    {
                        dungeonDeleteObject(coord);
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
                        int creature_id = monsterTakeHit((int)tile.creature_id, 100);
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

        // Destroy all traps and doors in a given direction -RAK-
        public static bool spellDestroyDoorsTrapsInDirection(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            bool destroyed = false;
            int distance = 0;

            Tile_t tile = null;

            do
            {
                playerMovePosition(direction, coord);
                distance++;

                tile = dg.floor[coord.y][coord.x];

                // must move into first closed spot, as it might be a secret door
                if (tile.treasure_id != 0)
                {
                    var item = game.treasure.list[tile.treasure_id];

                    if (item.category_id == TV_INVIS_TRAP || item.category_id == TV_CLOSED_DOOR || item.category_id == TV_VIS_TRAP || item.category_id == TV_OPEN_DOOR ||
                        item.category_id == TV_SECRET_DOOR)
                    {
                        if (dungeonDeleteObject(coord))
                        {
                            destroyed = true;
                            printMessage("There is a bright flash of light!");
                        }
                    }
                    else if (item.category_id == TV_CHEST && item.flags != 0)
                    {
                        destroyed = true;
                        printMessage("Click!");

                        item.flags &= ~(Config.treasure_chests.CH_TRAPPED | Config.treasure_chests.CH_LOCKED);
                        item.special_name_id = (int)SpecialNameIds.SN_UNLOCKED;

                        spellItemIdentifyAndRemoveRandomInscription(item);
                    }
                }
            } while ((distance <= Config.treasure.OBJECT_BOLTS_MAX_RANGE) || tile.feature_id <= MAX_OPEN_SPACE);

            return destroyed;
        }

        // Polymorph a monster -RAK-
        // NOTE: cannot polymorph a winning creature (BALROG)
        public static bool spellPolymorphMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            int distance = 0;
            bool morphed = false;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue;
                }

                if (tile.creature_id > 1)
                {
                    var monster = State.Instance.monsters[tile.creature_id];
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if (randomNumber(MON_MAX_LEVELS) > creature.level)
                    {
                        finished = true;

                        dungeonDeleteMonster((int)tile.creature_id);

                        // Place_monster() should always return true here.
                        morphed = monsterPlaceNew(coord, randomNumber(State.Instance.monster_levels[MON_MAX_LEVELS] - State.Instance.monster_levels[0]) - 1 + State.Instance.monster_levels[0], false);

                        // don't test tile.field_mark here, only permanent_light/temporary_light
                        if (morphed && coordInsidePanel(coord) && (tile.temporary_light || tile.permanent_light))
                        {
                            morphed = true;
                        }
                    }
                    else
                    {
                        var name = monsterNameDescription(creature.name, monster.lit);
                        printMonsterActionText(name, "is unaffected.");
                    }
                }
            }

            return morphed;
        }

        // Create a wall. -RAK-
        public static bool spellBuildWall(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            int distance = 0;
            bool built = false;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue; // we're done here, break out of the loop
                }

                if (tile.treasure_id != 0)
                {
                    dungeonDeleteObject(coord);
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
                            damage = diceRoll(new Dice_t(4, 8));
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
                        monster.hp += diceRoll(new Dice_t(4, 8));
                    }
                }

                tile.feature_id = TILE_MAGMA_WALL;
                tile.field_mark = false;

                // Permanently light this wall if it is lit by player's lamp.
                tile.permanent_light = (tile.temporary_light || tile.permanent_light);
                dungeonLiteSpot(coord);

                built = true;
            }

            return built;
        }

        // Replicate a creature -RAK-
        public static bool spellCloneMonster(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;
            int distance = 0;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
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

        // Move the creature record to a new location -RAK-
        public static void spellTeleportAwayMonster(int monster_id, int distance_from_player)
        {
            var dg = State.Instance.dg;
            int counter = 0;

            Coord_t coord = new Coord_t(0, 0);
            var monster = State.Instance.monsters[monster_id];

            do
            {
                do
                {
                    coord.y = monster.pos.y + (randomNumber(2 * distance_from_player + 1) - (distance_from_player + 1));
                    coord.x = monster.pos.x + (randomNumber(2 * distance_from_player + 1) - (distance_from_player + 1));
                } while (!coordInBounds(coord));

                counter++;
                if (counter > 9)
                {
                    counter = 0;
                    distance_from_player += 5;
                }
            } while ((dg.floor[coord.y][coord.x].feature_id >= MIN_CLOSED_SPACE) || (dg.floor[coord.y][coord.x].creature_id != 0));

            dungeonMoveCreatureRecord(new Coord_t(monster.pos.y, monster.pos.x), coord);
            dungeonLiteSpot(new Coord_t(monster.pos.y, monster.pos.x));

            monster.pos.y = coord.y;
            monster.pos.x = coord.x;

            // this is necessary, because the creature is
            // not currently visible in its new position.
            monster.lit = false;
            monster.distance_from_player = (uint)coordDistanceBetween(State.Instance.py.pos, coord);

            monsterUpdateVisibility(monster_id);
        }

        // Teleport player to spell casting creature -RAK-
        public static void spellTeleportPlayerTo(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            int distance = 1;
            int counter = 0;

            Coord_t rnd_coord = new Coord_t(0, 0);

            do
            {
                rnd_coord.y = coord.y + (randomNumber(2 * distance + 1) - (distance + 1));
                rnd_coord.x = coord.x + (randomNumber(2 * distance + 1) - (distance + 1));
                counter++;
                if (counter > 9)
                {
                    counter = 0;
                    distance++;
                }
            } while (!coordInBounds(rnd_coord) || (dg.floor[rnd_coord.y][rnd_coord.x].feature_id >= MIN_CLOSED_SPACE) || (dg.floor[rnd_coord.y][rnd_coord.x].creature_id >= 2));

            dungeonMoveCreatureRecord(py.pos, rnd_coord);

            Coord_t spot = new Coord_t(0, 0);
            for (spot.y = py.pos.y - 1; spot.y <= py.pos.y + 1; spot.y++)
            {
                for (spot.x = py.pos.x - 1; spot.x <= py.pos.x + 1; spot.x++)
                {
                    dg.floor[spot.y][spot.x].temporary_light = false;
                    dungeonLiteSpot(spot);
                }
            }

            dungeonLiteSpot(py.pos);

            py.pos.y = rnd_coord.y;
            py.pos.x = rnd_coord.x;

            dungeonResetView();

            // light creatures
            updateMonsters(false);
        }

        // Teleport all creatures in a given direction away -RAK-
        public static bool spellTeleportAwayMonsterInDirection(Coord_t coord, int direction)
        {
            var dg = State.Instance.dg;

            int distance = 0;
            bool teleported = false;
            bool finished = false;

            while (!finished)
            {
                playerMovePosition(direction, coord);
                distance++;

                var tile = dg.floor[coord.y][coord.x];

                if (distance > Config.treasure.OBJECT_BOLTS_MAX_RANGE || tile.feature_id >= MIN_CLOSED_SPACE)
                {
                    finished = true;
                    continue;
                }

                if (tile.creature_id > 1)
                {
                    // wake it up
                    State.Instance.monsters[tile.creature_id].sleep_count = 0;

                    spellTeleportAwayMonster((int)tile.creature_id, (int)Config.monsters.MON_MAX_SIGHT);

                    teleported = true;
                }
            }

            return teleported;
        }

        // Delete all creatures within max_sight distance -RAK-
        // NOTE : Winning creatures cannot be killed by genocide.
        public static bool spellMassGenocide()
        {
            bool killed = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                if (monster.distance_from_player <= Config.monsters.MON_MAX_SIGHT && (creature.movement & Config.monsters_move.CM_WIN) == 0)
                {
                    killed = true;
                    dungeonDeleteMonster(id);
                }
            }

            return killed;
        }

        // Delete all creatures of a given type from level. -RAK-
        // This does not keep creatures of type from appearing later.
        // NOTE : Winning creatures can not be killed by genocide.
        public static bool spellGenocide()
        {
            char creature_char = '\0';
            if (!getCommand("Which type of creature do you wish exterminated?", out creature_char))
            {
                return false;
            }

            bool killed = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                if (creature_char == Library.Instance.Creatures.creatures_list[(int)monster.creature_id].sprite)
                {
                    if ((creature.movement & Config.monsters_move.CM_WIN) == 0)
                    {
                        killed = true;
                        dungeonDeleteMonster(id);
                    }
                    else
                    {
                        // genocide is a powerful spell, so we will let the player
                        // know the names of the creatures they did not destroy,
                        // this message makes no sense otherwise
                        printMessage(("The " + creature.name) + " is unaffected.");
                    }
                }
            }

            return killed;
        }

        // Change speed of any creature . -RAK-
        // NOTE: cannot slow a winning creature (BALROG)
        public static bool spellSpeedAllMonsters(int speed)
        {
            var py = State.Instance.py;

            bool speedy = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                var name = monsterNameDescription(creature.name, monster.lit);

                if (monster.distance_from_player > Config.monsters.MON_MAX_SIGHT || !los(py.pos, monster.pos))
                {
                    continue; // do nothing
                }

                if (speed > 0)
                {
                    monster.speed += speed;
                    monster.sleep_count = 0;

                    if (monster.lit)
                    {
                        speedy = true;
                        printMonsterActionText(name, "starts moving faster.");
                    }
                }
                else if (randomNumber(MON_MAX_LEVELS) > creature.level)
                {
                    monster.speed += speed;
                    monster.sleep_count = 0;

                    if (monster.lit)
                    {
                        speedy = true;
                        printMonsterActionText(name, "starts moving slower.");
                    }
                }
                else if (monster.lit)
                {
                    monster.sleep_count = 0;
                    printMonsterActionText(name, "is unaffected.");
                }
            }

            return speedy;
        }

        // Sleep any creature . -RAK-
        public static bool spellSleepAllMonsters()
        {
            var py = State.Instance.py;

            bool asleep = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                var name = monsterNameDescription(creature.name, monster.lit);

                if (monster.distance_from_player > Config.monsters.MON_MAX_SIGHT || !los(py.pos, monster.pos))
                {
                    continue; // do nothing
                }

                if (randomNumber(MON_MAX_LEVELS) < creature.level || ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0))
                {
                    if (monster.lit)
                    {
                        if ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_NO_SLEEP;
                        }
                        printMonsterActionText(name, "is unaffected.");
                    }
                }
                else
                {
                    monster.sleep_count = 500;
                    if (monster.lit)
                    {
                        asleep = true;
                        printMonsterActionText(name, "falls asleep.");
                    }
                }
            }

            return asleep;
        }

        // Polymorph any creature that player can see. -RAK-
        // NOTE: cannot polymorph a winning creature (BALROG)
        public static bool spellMassPolymorph()
        {
            bool morphed = false;
            Coord_t coord = new Coord_t(0, 0);

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (monster.distance_from_player <= Config.monsters.MON_MAX_SIGHT)
                {
                    var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                    if ((creature.movement & Config.monsters_move.CM_WIN) == 0)
                    {
                        coord.y = monster.pos.y;
                        coord.x = monster.pos.x;
                        dungeonDeleteMonster(id);

                        // Place_monster() should always return true here.
                        morphed = monsterPlaceNew(coord, randomNumber(State.Instance.monster_levels[MON_MAX_LEVELS] - State.Instance.monster_levels[0]) - 1 + State.Instance.monster_levels[0], false);
                    }
                }
            }

            return morphed;
        }

        // Display evil creatures on current panel -RAK-
        public static bool spellDetectEvil()
        {
            bool detected = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (coordInsidePanel(new Coord_t(monster.pos.y, monster.pos.x)) && ((Library.Instance.Creatures.creatures_list[(int)monster.creature_id].defenses & Config.monsters_defense.CD_EVIL) != 0))
                {
                    monster.lit = true;

                    detected = true;

                    // works correctly even if hallucinating
                    panelPutTile((char)Library.Instance.Creatures.creatures_list[(int)monster.creature_id].sprite, new Coord_t(monster.pos.y, monster.pos.x));
                }
            }

            if (detected)
            {
                printMessage("You sense the presence of evil!");
                printMessage(/*CNIL*/null);

                // must unlight every monster just lighted
                updateMonsters(false);
            }

            return detected;
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

        public static void earthquakeHitsMonster(int monster_id)
        {
            var monster = State.Instance.monsters[monster_id];
            var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

            if ((creature.movement & Config.monsters_move.CM_PHASE) == 0u)
            {
                int damage;
                if ((creature.movement & Config.monsters_move.CM_ATTACK_ONLY) != 0u)
                {
                    // this will kill everything
                    damage = 3000;
                }
                else
                {
                    damage = diceRoll(new Dice_t(4, 8));
                }

                var name = monsterNameDescription(creature.name, monster.lit);

                printMonsterActionText(name, "wails out in pain!");

                if (monsterTakeHit(monster_id, damage) >= 0)
                {
                    printMonsterActionText(name, "is embedded in the rock.");
                    displayCharacterExperience();
                }
            }
            else if (creature.sprite == 'E' || creature.sprite == 'X')
            {
                // must be an earth elemental or an earth spirit, or a
                // Xorn increase its hit points
                monster.hp += diceRoll(new Dice_t(4, 8));
            }
        }

        // This is a fun one.  In a given block, pick some walls and
        // turn them into open spots.  Pick some open spots and dg.game_turn
        // them into walls.  An "Earthquake" effect. -RAK-
        public static void spellEarthquake()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            Coord_t coord = new Coord_t(0, 0);

            for (coord.y = py.pos.y - 8; coord.y <= py.pos.y + 8; coord.y++)
            {
                for (coord.x = py.pos.x - 8; coord.x <= py.pos.x + 8; coord.x++)
                {
                    if ((coord.y != py.pos.y || coord.x != py.pos.x) && coordInBounds(coord) && randomNumber(8) == 1)
                    {
                        var tile = dg.floor[coord.y][coord.x];

                        if (tile.treasure_id != 0)
                        {
                            dungeonDeleteObject(coord);
                        }

                        if (tile.creature_id > 1)
                        {
                            earthquakeHitsMonster((int)tile.creature_id);
                        }

                        if (tile.feature_id >= MIN_CAVE_WALL && tile.feature_id != TILE_BOUNDARY_WALL)
                        {
                            tile.feature_id = TILE_CORR_FLOOR;
                            tile.permanent_light = false;
                            tile.field_mark = false;
                        }
                        else if (tile.feature_id <= MAX_CAVE_FLOOR)
                        {
                            int tmp = randomNumber(10);

                            if (tmp < 6)
                            {
                                tile.feature_id = TILE_QUARTZ_WALL;
                            }
                            else if (tmp < 9)
                            {
                                tile.feature_id = TILE_MAGMA_WALL;
                            }
                            else
                            {
                                tile.feature_id = TILE_GRANITE_WALL;
                            }

                            tile.field_mark = false;
                        }
                        dungeonLiteSpot(coord);
                    }
                }
            }
        }

        // Create some high quality mush for the player. -RAK-
        public static void spellCreateFood()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            // Note: must take reference to this location as dungeonPlaceRandomObjectAt()
            // below, changes the tile values.
            var tile = dg.floor[py.pos.y][py.pos.x];

            // take no action here, don't want to destroy object under player
            if (tile.treasure_id != 0)
            {
                // set player_free_turn so that scroll/spell points won't be used
                game.player_free_turn = true;

                printMessage("There is already an object under you.");

                return;
            }

            dungeonPlaceRandomObjectAt(py.pos, false);
            inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_MUSH, game.treasure.list[tile.treasure_id]);
        }

        // Attempts to destroy a type of creature.  Success depends on
        // the creatures level VS. the player's level -RAK-
        public static bool spellDispelCreature(int creature_defense, int damage)
        {
            var py = State.Instance.py;
            var creatures_list = Library.Instance.Creatures.creatures_list;
            bool dispelled = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];

                if (monster.distance_from_player <= Config.monsters.MON_MAX_SIGHT && ((creature_defense & creatures_list[(int)monster.creature_id].defenses) != 0) &&
                    los(py.pos, monster.pos))
                {
                    var creature = creatures_list[(int)monster.creature_id];

                    State.Instance.creature_recall[monster.creature_id].defenses |= (uint)creature_defense;

                    dispelled = true;

                    var name = monsterNameDescription(creature.name, monster.lit);

                    int hit = monsterTakeHit(id, randomNumber(damage));

                    // Should get these messages even if the monster is not visible.
                    if (hit >= 0)
                    {
                        printMonsterActionText(name, "dissolves!");
                    }
                    else
                    {
                        printMonsterActionText(name, "shudders.");
                    }

                    if (hit >= 0)
                    {
                        displayCharacterExperience();
                    }
                }
            }

            return dispelled;
        }

        // Attempt to turn (confuse) undead creatures. -RAK-
        public static bool spellTurnUndead()
        {
            var py = State.Instance.py;
            bool turned = false;

            for (int id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID; id--)
            {
                var monster = State.Instance.monsters[id];
                var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

                if (monster.distance_from_player <= Config.monsters.MON_MAX_SIGHT && ((creature.defenses & Config.monsters_defense.CD_UNDEAD) != 0) && los(py.pos, monster.pos))
                {
                    var name = monsterNameDescription(creature.name, monster.lit);

                    if (py.misc.level + 1 > creature.level || randomNumber(5) == 1)
                    {
                        if (monster.lit)
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_UNDEAD;

                            turned = true;

                            printMonsterActionText(name, "runs frantically!");
                        }

                        monster.confused_amount = (uint)py.misc.level;
                    }
                    else if (monster.lit)
                    {
                        printMonsterActionText(name, "is unaffected.");
                    }
                }
            }

            return turned;
        }

        // Leave a glyph of warding. Creatures will not pass over! -RAK-
        public static void spellWardingGlyph()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            if (dg.floor[py.pos.y][py.pos.x].treasure_id == 0)
            {
                int free_id = popt();
                dg.floor[py.pos.y][py.pos.x].treasure_id = (uint)free_id;
                inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_SCARE_MON, game.treasure.list[free_id]);
            }
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

            int exp = 0;
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

                var character_class = State.Instance.classes[py.misc.class_id];

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

        // Slow Poison -RAK-
        public static bool spellSlowPoison()
        {
            var py = State.Instance.py;
            if (py.flags.poisoned > 0)
            {
                py.flags.poisoned = (int)(py.flags.poisoned / 2);
                if (py.flags.poisoned < 1)
                {
                    py.flags.poisoned = 1;
                }
                printMessage("The effect of the poison has been reduced.");
                return true;
            }

            return false;
        }

        public static void replaceSpot(Coord_t coord, int typ)
        {
            var dg = State.Instance.dg;

            var tile = dg.floor[coord.y][coord.x];

            switch (typ)
            {
                case 1:
                case 2:
                case 3:
                    tile.feature_id = TILE_CORR_FLOOR;
                    break;
                case 4:
                case 7:
                case 10:
                    tile.feature_id = TILE_GRANITE_WALL;
                    break;
                case 5:
                case 8:
                case 11:
                    tile.feature_id = TILE_MAGMA_WALL;
                    break;
                case 6:
                case 9:
                case 12:
                    tile.feature_id = TILE_QUARTZ_WALL;
                    break;
                default:
                    break;
            }

            tile.permanent_light = false;
            tile.field_mark = false;
            tile.perma_lit_room = false; // this is no longer part of a room

            if (tile.treasure_id != 0)
            {
                dungeonDeleteObject(coord);
            }

            if (tile.creature_id > 1)
            {
                dungeonDeleteMonster((int)tile.creature_id);
            }
        }

        // The spell of destruction. -RAK-
        // NOTE:
        //   Winning creatures that are deleted will be considered as teleporting to another level.
        //   This will NOT win the game.
        public static void spellDestroyArea(Coord_t coord)
        {
            var dg = State.Instance.dg;

            if (dg.current_level > 0)
            {
                Coord_t spot = new Coord_t(0, 0);

                for (spot.y = coord.y - 15; spot.y <= coord.y + 15; spot.y++)
                {
                    for (spot.x = coord.x - 15; spot.x <= coord.x + 15; spot.x++)
                    {
                        if (coordInBounds(spot) && dg.floor[spot.y][spot.x].feature_id != TILE_BOUNDARY_WALL)
                        {
                            int distance = coordDistanceBetween(spot, coord);

                            // clear player's spot, but don't put wall there
                            if (distance == 0)
                            {
                                replaceSpot(spot, 1);
                            }
                            else if (distance < 13)
                            {
                                replaceSpot(spot, randomNumber(6));
                            }
                            else if (distance < 16)
                            {
                                replaceSpot(spot, randomNumber(9));
                            }
                        }
                    }
                }
            }

            printMessage("There is a searing blast of light!");
            State.Instance.py.flags.blind += 10 + randomNumber(10);
        }

        // Enchants a plus onto an item. -RAK-
        // `limit` param is the maximum bonus allowed; usually 10,
        // but weapon's maximum damage when enchanting melee weapons to damage.
        public static bool spellEnchantItem(ref int plusses, int max_bonus_limit)
        {
            // avoid randomNumber(0) call
            if (max_bonus_limit <= 0)
            {
                return false;
            }

            int chance = 0;

            if (plusses > 0)
            {
                chance = plusses;

                // very rarely allow enchantment over limit
                if (randomNumber(100) == 1)
                {
                    chance = randomNumber(chance) - 1;
                }
            }

            if (randomNumber(max_bonus_limit) > chance)
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

            bool removed = false;

            for (int id = (int)PlayerEquipment.Wield; id <= (int)PlayerEquipment.Outer; id++)
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
