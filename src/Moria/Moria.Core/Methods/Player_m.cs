using System.Collections.Generic;
using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Player;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Std_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Player_run_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Player_traps_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_stats_m;

namespace Moria.Core.Methods
{
    public static class Player_m
    {
        public static void SetDependencies(
            IDice dice,
            IDungeon dungeon,
            IGame game,
            IHelpers helpers,
            IInventoryManager inventoryManager,
            IPlayerMagic playerMagic,
            IRnd rnd,

            IEventPublisher eventPublisher
        )
        {
            Player_m.dice = dice;
            Player_m.dungeon = dungeon;
            Player_m.game = game;
            Player_m.helpers = helpers;
            Player_m.inventoryManager = inventoryManager;
            Player_m.playerMagic = playerMagic;
            Player_m.rnd = rnd;

            Player_m.eventPublisher = eventPublisher;
        }

        private static IDice dice;
        private static IDungeon dungeon;
        private static IGame game;
        private static IHelpers helpers;
        private static IInventoryManager inventoryManager;
        private static IPlayerMagic playerMagic;
        private static IRnd rnd;

        private static IEventPublisher eventPublisher;

        static void playerResetFlags()
        {
            var py = State.Instance.py;

            py.flags.see_invisible = false;
            py.flags.teleport = false;
            py.flags.free_action = false;
            py.flags.slow_digest = false;
            py.flags.aggravate = false;
            py.flags.sustain_str = false;
            py.flags.sustain_int = false;
            py.flags.sustain_wis = false;
            py.flags.sustain_con = false;
            py.flags.sustain_dex = false;
            py.flags.sustain_chr = false;
            py.flags.resistant_to_fire = false;
            py.flags.resistant_to_acid = false;
            py.flags.resistant_to_cold = false;
            py.flags.regenerate_hp = false;
            py.flags.resistant_to_light = false;
            py.flags.free_fall = false;
        }

        public static bool playerIsMale()
        {
            return State.Instance.py.misc.gender;
        }

        public static void playerSetGender(bool is_male)
        {
            State.Instance.py.misc.gender = is_male;
        }

        public static string playerGetGenderLabel()
        {
            if (playerIsMale())
            {
                return "Male";
            }
            return "Female";
        }

        

        



        // Something happens to disturb the player. -CJS-
        // The first arg indicates a major disturbance, which affects search.
        // The second arg indicates a light change.
        public static void playerDisturb(int major_disturbance, int light_disturbance)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.command_count = 0;

            if (major_disturbance != 0 && (py.flags.status & Config.player_status.PY_SEARCH) != 0u)
            {
                playerSearchOff();
            }

            if (py.flags.rest != 0)
            {
                playerRestOff();
            }

            if (light_disturbance != 0 || py.running_tracker != 0)
            {
                py.running_tracker = 0;
                dungeonResetView();
            }

            flushInputBuffer();
        }

        // Search Mode enhancement -RAK-
        public static void playerSearchOn()
        {
            var py = State.Instance.py;

            playerChangeSpeed(1);

            py.flags.status |= Config.player_status.PY_SEARCH;

            printCharacterMovementState();
            printCharacterSpeed();

            py.flags.food_digested++;
        }

        public static void playerSearchOff()
        {
            var py = State.Instance.py;

            dungeonResetView();
            playerChangeSpeed(-1);

            py.flags.status &= ~Config.player_status.PY_SEARCH;

            printCharacterMovementState();
            printCharacterSpeed();
            py.flags.food_digested--;
        }

        // Resting allows a player to safely restore his hp -RAK-
        public static void playerRestOn()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            int rest_num;

            if (game.command_count > 0)
            {
                rest_num = game.command_count;
                game.command_count = 0;
            }
            else
            {
                rest_num = 0;
                var rest_str = string.Empty;
                //vtype_t rest_str = { '\0' };

                putStringClearToEOL("Rest for how long? ", new Coord_t(0, 0));

                if (getStringInput(out rest_str, new Coord_t(0, 19), 5))
                {
                    if (rest_str[0] == '*')
                    {
                        rest_num = -SHRT_MAX;
                    }
                    else
                    {
                        helpers.stringToNumber(rest_str, out rest_num);
                        //(void)stringToNumber(rest_str, rest_num);
                    }
                }
            }

            // check for reasonable value, must be positive number
            // in range of a short, or must be -MAX_SHORT
            if (rest_num == -SHRT_MAX || rest_num > 0 && rest_num <= SHRT_MAX)
            {
                if ((py.flags.status & Config.player_status.PY_SEARCH) != 0u)
                {
                    playerSearchOff();
                }

                py.flags.rest = (int)rest_num;
                py.flags.status |= Config.player_status.PY_REST;
                printCharacterMovementState();
                py.flags.food_digested--;

                putStringClearToEOL("Press any key to stop resting...", new Coord_t(0, 0));
                putQIO();

                return;
            }

            // Something went wrong
            if (rest_num != 0)
            {
                printMessage("Invalid rest count.");
            }
            messageLineClear();

            game.player_free_turn = true;
        }

        public static void playerRestOff()
        {
            var py = State.Instance.py;

            py.flags.rest = 0;
            py.flags.status &= ~Config.player_status.PY_REST;

            printCharacterMovementState();

            // flush last message, or delete "press any key" message
            printMessage(/*CNIL*/null);

            py.flags.food_digested++;
        }

        // For "DIED_FROM" string
        public static void playerDiedFromString(ref string description, string monster_name, uint move)
        {
            if ((move & Config.monsters_move.CM_WIN) != 0u)
            {
                description = $"The {monster_name}";
                //(void) sprintf(*description, "The %s", monster_name);
            }
            else if (helpers.isVowel(monster_name[0]))
            {
                description = $"an {monster_name}";
                //(void) sprintf(*description, "an %s", monster_name);
            }
            else
            {
                description = $"a {monster_name}";
                //(void) sprintf(*description, "a %s", monster_name);
            }
        }

        public static bool playerTestAttackHits(int attack_id, uint level)
        {
            var py = State.Instance.py;
            var success = false;

            switch (attack_id)
            {
                case 1: // Normal attack
                    if (playerTestBeingHit(60, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 2: // Lose Strength
                    if (playerTestBeingHit(-3, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 3: // Confusion attack
                case 4: // Fear attack
                case 5: // Fire attack
                    if (playerTestBeingHit(10, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 6: // Acid attack
                    if (playerTestBeingHit(0, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 7: // Cold attack
                case 8: // Lightning attack
                    if (playerTestBeingHit(10, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 9: // Corrosion attack
                    if (playerTestBeingHit(0, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 10: // Blindness attack
                case 11: // Paralysis attack
                    if (playerTestBeingHit(2, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 12: // Steal Money
                    if (playerTestBeingHit(5, (int)level, 0, (int)py.misc.level, (int)CLASS_MISC_HIT) && py.misc.au > 0)
                    {
                        success = true;
                    }
                    break;
                case 13: // Steal Object
                    if (playerTestBeingHit(2, (int)level, 0, (int)py.misc.level, (int)CLASS_MISC_HIT) && py.pack.unique_items > 0)
                    {
                        success = true;
                    }
                    break;
                case 14: // Poison
                    if (playerTestBeingHit(5, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 15: // Lose dexterity
                case 16: // Lose constitution
                    if (playerTestBeingHit(0, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 17: // Lose intelligence
                case 18: // Lose wisdom
                    if (playerTestBeingHit(2, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 19: // Lose experience
                    if (playerTestBeingHit(5, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 20: // Aggravate monsters
                    success = true;
                    break;
                case 21: // Disenchant
                    if (playerTestBeingHit(20, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 22: // Eat food
                case 23: // Eat light
                    if (playerTestBeingHit(5, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
                    {
                        success = true;
                    }
                    break;
                case 24: // Eat charges
                         // check to make sure an object exists
                    if (playerTestBeingHit(15, (int)level, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT) && py.pack.unique_items > 0)
                    {
                        success = true;
                    }
                    break;
                case 99:
                    success = true;
                    break;
                default:
                    break;
            }

            return success;
        }

        // Changes speed of monsters relative to player -RAK-
        // Note: When the player is sped up or slowed down, I simply change
        // the speed of all the monsters. This greatly simplified the logic.
        public static void playerChangeSpeed(int speed)
        {
            var py = State.Instance.py;
            py.flags.speed += speed;
            py.flags.status |= Config.player_status.PY_SPEED;

            for (var i = State.Instance.next_free_monster_id - 1; i >= Config.monsters.MON_MIN_INDEX_ID; i--)
            {
                State.Instance.monsters[i].speed += speed;
            }
        }

        // Player bonuses -RAK-
        //
        // When an item is worn or taken off, this re-adjusts the player bonuses.
        //     Factor =  1 : wear
        //     Factor = -1 : removed
        //
        // Only calculates properties with cumulative effect.  Properties that
        // depend on everything being worn are recalculated by playerRecalculateBonuses() -CJS-
        public static void playerAdjustBonusesForItem(Inventory_t item, int factor)
        {
            var py = State.Instance.py;

            var amount = item.misc_use * factor;

            if ((item.flags & Config.treasure_flags.TR_STATS) != 0u)
            {
                for (var i = 0; i < 6; i++)
                {
                    if (((1 << i) & item.flags) != 0u)
                    {
                        playerStatBoost(i, amount);
                    }
                }
            }

            if ((item.flags & Config.treasure_flags.TR_SEARCH) != 0u)
            {
                py.misc.chance_in_search += amount;
                py.misc.fos -= amount;
            }

            if ((item.flags & Config.treasure_flags.TR_STEALTH) != 0u)
            {
                py.misc.stealth_factor += amount;
            }

            if ((item.flags & Config.treasure_flags.TR_SPEED) != 0u)
            {
                playerChangeSpeed(-amount);
            }

            if ((item.flags & Config.treasure_flags.TR_BLIND) != 0u && factor > 0)
            {
                py.flags.blind += 1000;
            }

            if ((item.flags & Config.treasure_flags.TR_TIMID) != 0u && factor > 0)
            {
                py.flags.afraid += 50;
            }

            if ((item.flags & Config.treasure_flags.TR_INFRA) != 0u)
            {
                py.flags.see_infra += amount;
            }
        }

        public static void playerRecalculateBonusesFromInventory()
        {
            var py = State.Instance.py;
            for (var i = (int)PlayerEquipment.Wield; i < (int)PlayerEquipment.Light; i++)
            {
                var item = py.inventory[i];

                if (item.category_id != TV_NOTHING)
                {
                    py.misc.plusses_to_hit += item.to_hit;

                    // Bows can't damage. -CJS-
                    if (item.category_id != TV_BOW)
                    {
                        py.misc.plusses_to_damage += item.to_damage;
                    }

                    py.misc.magical_ac += item.to_ac;
                    py.misc.ac += item.ac;

                    if (spellItemIdentified(item))
                    {
                        py.misc.display_to_hit += item.to_hit;

                        // Bows can't damage. -CJS-
                        if (item.category_id != TV_BOW)
                        {
                            py.misc.display_to_damage += item.to_damage;
                        }

                        py.misc.display_to_ac += item.to_ac;
                        py.misc.display_ac += item.ac;
                    }
                    else if ((item.flags & Config.treasure_flags.TR_CURSED) == 0u)
                    {
                        // Base AC values should always be visible,
                        // as long as the item is not cursed.
                        py.misc.display_ac += item.ac;
                    }
                }
            }
        }

        public static void playerRecalculateSustainStatsFromInventory()
        {
            var py = State.Instance.py;

            for (var i = (int)PlayerEquipment.Wield; i < (int)PlayerEquipment.Light; i++)
            {
                if ((py.inventory[i].flags & Config.treasure_flags.TR_SUST_STAT) == 0u)
                {
                    continue;
                }

                switch (py.inventory[i].misc_use)
                {
                    case 1:
                        py.flags.sustain_str = true;
                        break;
                    case 2:
                        py.flags.sustain_int = true;
                        break;
                    case 3:
                        py.flags.sustain_wis = true;
                        break;
                    case 4:
                        py.flags.sustain_con = true;
                        break;
                    case 5:
                        py.flags.sustain_dex = true;
                        break;
                    case 6:
                        py.flags.sustain_chr = true;
                        break;
                    default:
                        break;
                }
            }
        }

        // Recalculate the effect of all the stuff we use. -CJS-
        public static void playerRecalculateBonuses()
        {
            var py = State.Instance.py;

            // Temporarily adjust food_digested
            if (py.flags.slow_digest)
            {
                py.flags.food_digested++;
            }
            if (py.flags.regenerate_hp)
            {
                py.flags.food_digested -= 3;
            }

            var saved_display_ac = py.misc.display_ac;

            playerResetFlags();

            // Real values
            py.misc.plusses_to_hit = playerToHitAdjustment();
            py.misc.plusses_to_damage = playerDamageAdjustment();
            py.misc.magical_ac = playerArmorClassAdjustment();
            py.misc.ac = 0;

            // Display values
            py.misc.display_to_hit = py.misc.plusses_to_hit;
            py.misc.display_to_damage = py.misc.plusses_to_damage;
            py.misc.display_ac = 0;
            py.misc.display_to_ac = py.misc.magical_ac;

            playerRecalculateBonusesFromInventory();

            py.misc.display_ac += py.misc.display_to_ac;

            if (py.weapon_is_heavy)
            {
                py.misc.display_to_hit += (int)py.stats.used[(int)PlayerAttr.STR] * 15 - (int)py.inventory[(int)PlayerEquipment.Wield].weight;
            }

            // Add in temporary spell increases
            if (py.flags.invulnerability > 0)
            {
                py.misc.ac += 100;
                py.misc.display_ac += 100;
            }

            if (py.flags.blessed > 0)
            {
                py.misc.ac += 2;
                py.misc.display_ac += 2;
            }

            if (py.flags.detect_invisible > 0)
            {
                py.flags.see_invisible = true;
            }

            // can't print AC here because might be in a store
            if (saved_display_ac != py.misc.display_ac)
            {
                py.flags.status |= Config.player_status.PY_ARMOR;
            }

            var item_flags = inventoryManager.inventoryCollectAllItemFlags();

            if ((item_flags & Config.treasure_flags.TR_SLOW_DIGEST) != 0u)
            {
                py.flags.slow_digest = true;
            }
            if ((item_flags & Config.treasure_flags.TR_AGGRAVATE) != 0u)
            {
                py.flags.aggravate = true;
            }
            if ((item_flags & Config.treasure_flags.TR_TELEPORT) != 0u)
            {
                py.flags.teleport = true;
            }
            if ((item_flags & Config.treasure_flags.TR_REGEN) != 0u)
            {
                py.flags.regenerate_hp = true;
            }
            if ((item_flags & Config.treasure_flags.TR_RES_FIRE) != 0u)
            {
                py.flags.resistant_to_fire = true;
            }
            if ((item_flags & Config.treasure_flags.TR_RES_ACID) != 0u)
            {
                py.flags.resistant_to_acid = true;
            }
            if ((item_flags & Config.treasure_flags.TR_RES_COLD) != 0u)
            {
                py.flags.resistant_to_cold = true;
            }
            if ((item_flags & Config.treasure_flags.TR_FREE_ACT) != 0u)
            {
                py.flags.free_action = true;
            }
            if ((item_flags & Config.treasure_flags.TR_SEE_INVIS) != 0u)
            {
                py.flags.see_invisible = true;
            }
            if ((item_flags & Config.treasure_flags.TR_RES_LIGHT) != 0u)
            {
                py.flags.resistant_to_light = true;
            }
            if ((item_flags & Config.treasure_flags.TR_FFALL) != 0u)
            {
                py.flags.free_fall = true;
            }

            playerRecalculateSustainStatsFromInventory();

            // Reset food_digested values
            if (py.flags.slow_digest)
            {
                py.flags.food_digested--;
            }
            if (py.flags.regenerate_hp)
            {
                py.flags.food_digested += 3;
            }
        }

        // Remove item from equipment list -RAK-
        public static void playerTakeOff(int item_id, int pack_position_id)
        {
            var py = State.Instance.py;
            py.flags.status |= Config.player_status.PY_STR_WGT;

            var item = py.inventory[item_id];

            py.pack.weight -= (int)(item.weight * item.items_count);
            py.equipment_count--;

            string p;
            if (item_id == (int)PlayerEquipment.Wield || item_id == (int)PlayerEquipment.Auxiliary)
            {
                p = "Was wielding ";
            }
            else if (item_id == (int)PlayerEquipment.Light)
            {
                p = "Light source was ";
            }
            else
            {
                p = "Was wearing ";
            }

            var description = string.Empty;
            //obj_desc_t description = { '\0' };
            itemDescription(ref description, item, true);

            string msg;
            //obj_desc_t msg = { '\0' };
            if (pack_position_id >= 0)
            {
                msg = $"{p}{description} ({(char)('a' + pack_position_id)})";
                //(void)sprintf(msg, "%s%s (%c)", p, description, 'a' + pack_position_id);
            }
            else
            {
                msg = $"{p}{description}";
                //(void)sprintf(msg, "%s%s", p, description);
            }
            printMessage(msg);

            // For secondary weapon
            if (item_id != (int)PlayerEquipment.Auxiliary)
            {
                playerAdjustBonusesForItem(item, -1);
            }

            inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, item);
        }

        // Attacker's level and plusses,  defender's AC -RAK-
        public static bool playerTestBeingHit(int base_to_hit, int level, int plus_to_hit, int armor_class, int attack_type_id)
        {
            var py = State.Instance.py;
            playerDisturb(1, 0);

            // `plus_to_hit` could be less than 0 if player wielding weapon too heavy for them
            var hit_chance = base_to_hit + plus_to_hit * (int)BTH_PER_PLUS_TO_HIT_ADJUST + level * Library.Instance.Player.class_level_adj[(int)py.misc.class_id][attack_type_id];

            // always miss 1 out of 20, always hit 1 out of 20
            var die = rnd.randomNumber(20);

            // normal hit
            return die != 1 && (die == 20 || hit_chance > 0 && rnd.randomNumber(hit_chance) > armor_class);
        }

        // Decreases players hit points and sets game.character_is_dead flag if necessary -RAK-
        public static void playerTakesHit(int damage, string creature_name_label)
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            if (py.flags.invulnerability > 0)
            {
                damage = 0;
            }
            py.misc.current_hp -= damage;

            if (py.misc.current_hp >= 0)
            {
                printCharacterCurrentHitPoints();
                return;
            }

            if (!game.character_is_dead)
            {
                game.character_is_dead = true;

                game.character_died_from = creature_name_label;
                //(void)strcpy(game.character_died_from, creature_name_label);

                game.total_winner = false;
            }

            State.Instance.dg.generate_new_level = true;
        }

        // Searches for hidden things. -RAK-
        public static void playerSearch(Coord_t coord, int chance)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            if (py.flags.confused > 0)
            {
                chance = chance / 10;
            }

            if (py.flags.blind > 0 || helpers.playerNoLight())
            {
                chance = chance / 10;
            }

            if (py.flags.image > 0)
            {
                chance = chance / 10;
            }

            var spot = new Coord_t(0, 0);
            for (spot.y = coord.y - 1; spot.y <= coord.y + 1; spot.y++)
            {
                for (spot.x = coord.x - 1; spot.x <= coord.x + 1; spot.x++)
                {
                    // always coordInBounds() here
                    if (rnd.randomNumber(100) >= chance)
                    {
                        continue;
                    }

                    if (dg.floor[spot.y][spot.x].treasure_id == 0)
                    {
                        continue;
                    }

                    // Search for hidden objects

                    var item = game.treasure.list[dg.floor[spot.y][spot.x].treasure_id];

                    if (item.category_id == TV_INVIS_TRAP)
                    {
                        // Trap on floor?

                        var description = string.Empty;
                        //obj_desc_t description = { '\0' };
                        itemDescription(ref description, item, true);

                        var msg = $"You have found {description}";
                        //obj_desc_t msg = { '\0' };
                        //(void)sprintf(msg, "You have found %s", description);
                        printMessage(msg);

                        dungeon.trapChangeVisibility(spot);
                        eventPublisher.Publish(new EndRunningCommand());
                        //playerEndRunning();
                    }
                    else if (item.category_id == TV_SECRET_DOOR)
                    {
                        // Secret door?

                        printMessage("You have found a secret door.");

                        dungeon.trapChangeVisibility(spot);
                        eventPublisher.Publish(new EndRunningCommand());
                        //playerEndRunning();
                    }
                    else if (item.category_id == TV_CHEST)
                    {
                        // Chest is trapped?

                        // mask out the treasure bits
                        if ((item.flags & Config.treasure_chests.CH_TRAPPED) > 1)
                        {
                            if (!spellItemIdentified(item))
                            {
                                spellItemIdentifyAndRemoveRandomInscription(item);
                                printMessage("You have discovered a trap on the chest!");
                            }
                            else
                            {
                                printMessage("The chest is trapped!");
                            }
                        }
                    }
                }
            }
        }

        // Computes current weight limit -RAK-
        public static int playerCarryingLoadLimit()
        {
            var py = State.Instance.py;
            var weight_cap = (int)(py.stats.used[(int)PlayerAttr.STR] * Config.player.PLAYER_WEIGHT_CAP + py.misc.weight);

            if (weight_cap > 3000)
            {
                weight_cap = 3000;
            }

            return weight_cap;
        }

        // Are we strong enough for the current pack and weapon? -CJS-
        public static void playerStrength()
        {
            var py = State.Instance.py;
            var item = py.inventory[(int)PlayerEquipment.Wield];

            if (item.category_id != TV_NOTHING && py.stats.used[(int)PlayerAttr.STR] * 15 < item.weight)
            {
                if (!py.weapon_is_heavy)
                {
                    printMessage("You have trouble wielding such a heavy weapon.");
                    py.weapon_is_heavy = true;
                    playerRecalculateBonuses();
                }
            }
            else if (py.weapon_is_heavy)
            {
                py.weapon_is_heavy = false;
                if (item.category_id != TV_NOTHING)
                {
                    printMessage("You are strong enough to wield your weapon.");
                }
                playerRecalculateBonuses();
            }

            var limit = playerCarryingLoadLimit();

            if (limit < py.pack.weight)
            {
                limit = py.pack.weight / (limit + 1);
            }
            else
            {
                limit = 0;
            }

            if (py.pack.heaviness != limit)
            {
                if (py.pack.heaviness < limit)
                {
                    printMessage("Your pack is so heavy that it slows you down.");
                }
                else
                {
                    printMessage("You move more easily under the weight of your pack.");
                }
                playerChangeSpeed(limit - py.pack.heaviness);
                py.pack.heaviness = (int)limit;
            }

            py.flags.status &= ~Config.player_status.PY_STR_WGT;
        }

        public static int newMana(int stat)
        {
            var py = State.Instance.py;
            var levels = (int)(py.misc.level - Library.Instance.Player.classes[(int)py.misc.class_id].min_level_for_spell_casting + 1);

            switch (playerStatAdjustmentWisdomIntelligence(stat))
            {
                case 1:
                case 2:
                    return 1 * levels;
                case 3:
                    return 3 * levels / 2;
                case 4:
                    return 2 * levels;
                case 5:
                    return 5 * levels / 2;
                case 6:
                    return 3 * levels;
                case 7:
                    return 4 * levels;
                default:
                    return 0;
            }
        }

        // Gain some mana if you know at least one spell -RAK-
        public static void playerGainMana(int stat)
        {
            var py = State.Instance.py;
            if (py.flags.spells_learnt != 0)
            {
                var new_mana = newMana(stat);

                // increment mana by one, so that first level chars have 2 mana
                if (new_mana > 0)
                {
                    new_mana++;
                }

                // mana can be zero when creating character
                if (py.misc.mana != new_mana)
                {
                    if (py.misc.mana != 0)
                    {
                        // change current mana proportionately to change of max mana,
                        // divide first to avoid overflow, little loss of accuracy
                        var value = (((int)py.misc.current_mana << 16) + (int)py.misc.current_mana_fraction) / py.misc.mana * new_mana;
                        py.misc.current_mana = (int)(value >> 16);
                        py.misc.current_mana_fraction = (uint)(value & 0xFFFF);
                    }
                    else
                    {
                        py.misc.current_mana = (int)new_mana;
                        py.misc.current_mana_fraction = 0;
                    }

                    py.misc.mana = (int)new_mana;

                    // can't print mana here, may be in store or inventory mode
                    py.flags.status |= Config.player_status.PY_MANA;
                }
            }
            else if (py.misc.mana != 0)
            {
                py.misc.mana = 0;
                py.misc.current_mana = 0;

                // can't print mana here, may be in store or inventory mode
                py.flags.status |= Config.player_status.PY_MANA;
            }
        }

        // Critical hits, Nasty way to die. -RAK-
        public static int playerWeaponCriticalBlow(int weapon_weight, int plus_to_hit, int damage, int attack_type_id)
        {
            var py = State.Instance.py;
            var critical = damage;

            // Weight of weapon, plusses to hit, and character level all
            // contribute to the chance of a critical
            if (rnd.randomNumber(5000) <= weapon_weight + 5 * plus_to_hit + Library.Instance.Player.class_level_adj[(int)py.misc.class_id][attack_type_id] * py.misc.level)
            {
                weapon_weight += rnd.randomNumber(650);

                if (weapon_weight < 400)
                {
                    critical = 2 * damage + 5;
                    printMessage("It was a good hit! (x2 damage)");
                }
                else if (weapon_weight < 700)
                {
                    critical = 3 * damage + 10;
                    printMessage("It was an excellent hit! (x3 damage)");
                }
                else if (weapon_weight < 900)
                {
                    critical = 4 * damage + 15;
                    printMessage("It was a superb hit! (x4 damage)");
                }
                else
                {
                    critical = 5 * damage + 20;
                    printMessage("It was a *GREAT* hit! (x5 damage)");
                }
            }

            return critical;
        }

        // Saving throws for player character. -RAK-
        public static bool playerSavingThrow()
        {
            var py = State.Instance.py;
            var class_level_adjustment = Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.SAVE] * (int)py.misc.level / 3;

            var saving = py.misc.saving_throw + playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.WIS) + class_level_adjustment;

            return rnd.randomNumber(100) <= saving;
        }

        public static void playerGainKillExperience(Creature_t creature)
        {
            var py = State.Instance.py;
            var exp = (int)(creature.kill_exp_value * creature.level);

            var quotient = (int)(exp / py.misc.level);
            var remainder = (int)(exp % py.misc.level);

            remainder *= 0x10000;
            remainder /= (int)py.misc.level;
            remainder += (int)py.misc.exp_fraction;

            if (remainder >= 0x10000L)
            {
                quotient++;
                py.misc.exp_fraction = (uint)(remainder - 0x10000);
            }
            else
            {
                py.misc.exp_fraction = (uint)remainder;
            }

            py.misc.exp += quotient;
        }

        public static void playerCalculateToHitBlows(int weapon_id, int weapon_weight, ref int blows, ref int total_to_hit)
        {
            var py = State.Instance.py;
            if (weapon_id != TV_NOTHING)
            {
                // Proper weapon
                blows = playerAttackBlows(weapon_weight, ref total_to_hit);
            }
            else
            {
                // Bare hands?
                blows = 2;
                total_to_hit = -3;
            }

            // Fix for arrows
            if (weapon_id >= TV_SLING_AMMO && weapon_id <= TV_SPIKE)
            {
                blows = 1;
            }

            total_to_hit += py.misc.plusses_to_hit;
        }

        public static int playerCalculateBaseToHit(bool creature_lit, int tot_tohit)
        {
            var py = State.Instance.py;
            if (creature_lit)
            {
                return py.misc.bth;
            }

            // creature not lit, make it more difficult to hit
            int bth;

            bth = py.misc.bth / 2;
            bth -= tot_tohit * ((int)BTH_PER_PLUS_TO_HIT_ADJUST - 1);
            bth -= (int)py.misc.level * Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.BTH] / 2;

            return bth;
        }

        // Player attacks a (poor, defenseless) creature -RAK-
        public static void playerAttackMonster(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var creature_id = (int)dg.floor[coord.y][coord.x].creature_id;

            var monster = State.Instance.monsters[creature_id];
            var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];
            var item = py.inventory[(int)PlayerEquipment.Wield];

            monster.sleep_count = 0;

            // Does the player know what they're fighting?
            string name;
            //vtype_t name = { '\0' };
            if (!monster.lit)
            {
                name = "it";
                //(void)strcpy(name, "it");
            }
            else
            {
                name = $"the {creature.name}";
                //(void)sprintf(name, "the %s", creature.name);
            }

            int blows = 0, total_to_hit = 0;
            playerCalculateToHitBlows((int)item.category_id, (int)item.weight, ref blows, ref total_to_hit);

            var base_to_hit = playerCalculateBaseToHit(monster.lit, total_to_hit);

            int damage;
            var msg = string.Empty;
            //vtype_t msg = { '\0' };

            // Loop for number of blows, trying to hit the critter.
            // Note: blows will always be greater than 0 at the start of the loop -MRC-
            for (var i = blows; i > 0; i--)
            {
                if (!playerTestBeingHit(base_to_hit, (int)py.misc.level, total_to_hit, (int)creature.ac, (int)PlayerClassLevelAdj.BTH))
                {
                    msg = $"You miss {name}.";
                    //(void)sprintf(msg, "You miss %s.", name);
                    printMessage(msg);
                    continue;
                }

                msg = $"You hit {name}.";
                //(void)sprintf(msg, "You hit %s.", name);
                printMessage(msg);

                if (item.category_id != TV_NOTHING)
                {
                    damage = dice.diceRoll(item.damage);
                    damage = playerMagic.itemMagicAbilityDamage(item, damage, (int)monster.creature_id);
                    damage = playerWeaponCriticalBlow((int)item.weight, total_to_hit, damage, (int)PlayerClassLevelAdj.BTH);
                }
                else
                {
                    // Bare hands!?
                    damage = dice.diceRoll(new Dice_t(1, 1));
                    damage = playerWeaponCriticalBlow(1, 0, damage, (int)PlayerClassLevelAdj.BTH);
                }

                damage += py.misc.plusses_to_damage;
                if (damage < 0)
                {
                    damage = 0;
                }

                if (py.flags.confuse_monster)
                {
                    py.flags.confuse_monster = false;

                    printMessage("Your hands stop glowing.");

                    if ((creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0 || rnd.randomNumber(MON_MAX_LEVELS) < creature.level)
                    {
                        msg = $"{name} is unaffected.";
                        //(void)sprintf(msg, "%s is unaffected.", name);
                    }
                    else
                    {
                        msg = $"{name} appears confused.";
                        //(void)sprintf(msg, "%s appears confused.", name);
                        if (monster.confused_amount != 0u)
                        {
                            monster.confused_amount += 3;
                        }
                        else
                        {
                            monster.confused_amount = (uint)(2 + rnd.randomNumber(16));
                        }
                    }
                    printMessage(msg);

                    if (monster.lit && rnd.randomNumber(4) == 1)
                    {
                        State.Instance.creature_recall[monster.creature_id].defenses |= creature.defenses & Config.monsters_defense.CD_NO_SLEEP;
                    }
                }

                // See if we done it in.
                if (monsterTakeHit(creature_id, damage) >= 0)
                {
                    msg = $"You have slain {name}.";
                    //(void)sprintf(msg, "You have slain %s.", name);
                    printMessage(msg);
                    displayCharacterExperience();

                    return;
                }

                // Use missiles up
                if (item.category_id >= TV_SLING_AMMO && item.category_id <= TV_SPIKE)
                {
                    item.items_count--;
                    py.pack.weight -= (int)item.weight;
                    py.flags.status |= Config.player_status.PY_STR_WGT;

                    if (item.items_count == 0)
                    {
                        py.equipment_count--;
                        playerAdjustBonusesForItem(item, -1);
                        inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, item);
                        playerRecalculateBonuses();
                    }
                }
            }
        }

        public static int playerLockPickingSkill()
        {
            var py = State.Instance.py;

            var skill = py.misc.disarm;

            skill += 2;
            skill *= playerDisarmAdjustment();
            skill += playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT);
            skill += Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DISARM] * (int)py.misc.level / 3;

            return skill;
        }

        public static void openClosedDoor(Coord_t coord)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var tile = dg.floor[coord.y][coord.x];
            var item = game.treasure.list[tile.treasure_id];

            if (item.misc_use > 0)
            {
                // It's locked.

                if (py.flags.confused > 0)
                {
                    printMessage("You are too confused to pick the lock.");
                }
                else if (playerLockPickingSkill() - item.misc_use > rnd.randomNumber(100))
                {
                    printMessage("You have picked the lock.");
                    py.misc.exp++;
                    displayCharacterExperience();
                    item.misc_use = 0;
                }
                else
                {
                    printMessageNoCommandInterrupt("You failed to pick the lock.");
                }
            }
            else if (item.misc_use < 0)
            {
                // It's stuck

                printMessage("It appears to be stuck.");
            }

            if (item.misc_use == 0)
            {
                inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_OPEN_DOOR, game.treasure.list[tile.treasure_id]);
                tile.feature_id = TILE_CORR_FLOOR;
                dungeon.dungeonLiteSpot(coord);
                game.command_count = 0;
            }
        }

        public static void openClosedChest(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var py = State.Instance.py;

            var tile = dg.floor[coord.y][coord.x];
            var item = game.treasure.list[tile.treasure_id];

            var success = false;

            if ((item.flags & Config.treasure_chests.CH_LOCKED) != 0u)
            {
                if (py.flags.confused > 0)
                {
                    printMessage("You are too confused to pick the lock.");
                }
                else if (playerLockPickingSkill() - item.depth_first_found > rnd.randomNumber(100))
                {
                    printMessage("You have picked the lock.");

                    py.misc.exp += (int)item.depth_first_found;
                    displayCharacterExperience();

                    success = true;
                }
                else
                {
                    printMessageNoCommandInterrupt("You failed to pick the lock.");
                }
            }
            else
            {
                success = true;
            }

            if (success)
            {
                item.flags &= ~Config.treasure_chests.CH_LOCKED;
                item.special_name_id = (int)SpecialNameIds.SN_EMPTY;
                spellItemIdentifyAndRemoveRandomInscription(item);
                item.cost = 0;
            }

            // Was chest still trapped?
            if ((item.flags & Config.treasure_chests.CH_LOCKED) != 0)
            {
                return;
            }

            // Oh, yes it was...   (Snicker)
            chestTrap(coord);

            if (tile.treasure_id != 0)
            {
                // Chest treasure is allocated as if a creature had been killed.
                // clear the cursed chest/monster win flag, so that people
                // can not win by opening a cursed chest
                game.treasure.list[tile.treasure_id].flags &= ~Config.treasure_flags.TR_CURSED;

                monsterDeath(coord, game.treasure.list[tile.treasure_id].flags);

                game.treasure.list[tile.treasure_id].flags = 0;
            }
        }

        // Opens a closed door or closed chest. -RAK-
        public static void playerOpenClosedObject()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var dir = 0;
            if (!Player_m.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
            {
                return;
            }

            var coord = py.pos.Clone();
            helpers.movePosition(dir, ref coord);

            var no_object = false;

            var tile = dg.floor[coord.y][coord.x];
            var item = game.treasure.list[tile.treasure_id];

            if (tile.creature_id > 1 && tile.treasure_id != 0 && (item.category_id == TV_CLOSED_DOOR || item.category_id == TV_CHEST))
            {
                objectBlockedByMonster((int)tile.creature_id);
            }
            else if (tile.treasure_id != 0)
            {
                if (item.category_id == TV_CLOSED_DOOR)
                {
                    openClosedDoor(coord);
                }
                else if (item.category_id == TV_CHEST)
                {
                    openClosedChest(coord);
                }
                else
                {
                    no_object = true;
                }
            }
            else
            {
                no_object = true;
            }

            if (no_object)
            {
                game.player_free_turn = true;
                printMessage("I do not see anything you can open there.");
            }
        }

        // Closes an open door. -RAK-
        public static void playerCloseDoor()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            var dir = 0;

            if (!Player_m.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
            {
                return;
            }

            var coord = py.pos.Clone();
            helpers.movePosition(dir, ref coord);

            var tile = dg.floor[coord.y][coord.x];
            var item = game.treasure.list[tile.treasure_id];

            var no_object = false;

            if (tile.treasure_id != 0)
            {
                if (item.category_id == TV_OPEN_DOOR)
                {
                    if (tile.creature_id == 0)
                    {
                        if (item.misc_use == 0)
                        {
                            inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_CLOSED_DOOR, item);
                            tile.feature_id = TILE_BLOCKED_FLOOR;
                            dungeon.dungeonLiteSpot(coord);
                        }
                        else
                        {
                            printMessage("The door appears to be broken.");
                        }
                    }
                    else
                    {
                        objectBlockedByMonster((int)tile.creature_id);
                    }
                }
                else
                {
                    no_object = true;
                }
            }
            else
            {
                no_object = true;
            }

            if (no_object)
            {
                game.player_free_turn = true;
                printMessage("I do not see anything you can close there.");
            }
        }

        // Tunneling through real wall: 10, 11, 12 -RAK-
        // Used by TUNNEL and WALL_TO_MUD
        public static bool playerTunnelWall(Coord_t coord, int digging_ability, int digging_chance)
        {
            var dg = State.Instance.dg;

            if (digging_ability <= digging_chance)
            {
                return false;
            }

            var tile = dg.floor[coord.y][coord.x];

            if (tile.perma_lit_room)
            {
                // Should become a room space, check to see whether
                // it should be TILE_LIGHT_FLOOR or TILE_DARK_FLOOR.
                var found = false;

                for (var y = coord.y - 1; y <= coord.y + 1 && y < MAX_HEIGHT; y++)
                {
                    for (var x = coord.x - 1; x <= coord.x + 1 && x < MAX_WIDTH; x++)
                    {
                        if (dg.floor[y][x].feature_id <= MAX_CAVE_ROOM)
                        {
                            tile.feature_id = dg.floor[y][x].feature_id;
                            tile.permanent_light = dg.floor[y][x].permanent_light;
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    tile.feature_id = TILE_CORR_FLOOR;
                    tile.permanent_light = false;
                }
            }
            else
            {
                // should become a corridor space
                tile.feature_id = TILE_CORR_FLOOR;
                tile.permanent_light = false;
            }

            tile.field_mark = false;

            if (coordInsidePanel(coord) && (tile.temporary_light || tile.permanent_light) && tile.treasure_id != 0)
            {
                printMessage("You have found something!");
            }

            dungeon.dungeonLiteSpot(coord);

            return true;
        }

        // let the player attack the creature
        public static void playerAttackPosition(Coord_t coord)
        {
            var py = State.Instance.py;

            // Is a Coward?
            if (py.flags.afraid > 0)
            {
                printMessage("You are too afraid!");
                return;
            }

            playerAttackMonster(coord);
        }

        // check to see if know any spells greater than level, eliminate them
        public static void eliminateKnownSpellsGreaterThanLevel(IReadOnlyList<Spell_t> msp_ptr, string p, int offset)
        {
            var py = State.Instance.py;
            var mask = 0x80000000;

            for (var i = 31; mask != 0u; mask >>= 1, i--)
            {
                if ((mask & py.flags.spells_learnt) != 0u)
                {
                    if (msp_ptr[i].level_required > py.misc.level)
                    {
                        py.flags.spells_learnt &= ~mask;
                        py.flags.spells_forgotten |= mask;

                        var msg = $"You have forgotten the {p} of {Library.Instance.Player.spell_names[i + offset]}.";
                        //vtype_t msg = { '\0' };
                        //(void)sprintf(msg, "You have forgotten the %s of %s.", p, spell_names[i + offset]);
                        printMessage(msg);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public static int numberOfSpellsAllowed(int stat)
        {
            var py = State.Instance.py;
            var levels = (int)(py.misc.level - Library.Instance.Player.classes[(int)py.misc.class_id].min_level_for_spell_casting + 1);

            int allowed;

            switch (playerStatAdjustmentWisdomIntelligence(stat))
            {
                case 1:
                case 2:
                case 3:
                    allowed = 1 * levels;
                    break;
                case 4:
                case 5:
                    allowed = 3 * levels / 2;
                    break;
                case 6:
                    allowed = 2 * levels;
                    break;
                case 7:
                    allowed = 5 * levels / 2;
                    break;
                default:
                    allowed = 0;
                    break;
            }

            return allowed;
        }

        public static int numberOfSpellsKnown()
        {
            var py = State.Instance.py;
            var known = 0;

            for (uint mask = 0x1; mask != 0u; mask <<= 1)
            {
                if ((mask & py.flags.spells_learnt) != 0u)
                {
                    known++;
                }
            }

            return known;
        }

        // remember forgotten spells while forgotten spells exist of new_spells_to_learn positive,
        // remember the spells in the order that they were learned
        public static int rememberForgottenSpells(IReadOnlyList<Spell_t> msp_ptr, int allowed_spells, int new_spells, string p, int offset)
        {
            var py = State.Instance.py;
            uint mask;

            for (var n = 0; py.flags.spells_forgotten != 0u && new_spells != 0 && n < allowed_spells && n < 32; n++)
            {
                // order ID is (i+1)th spell learned
                var order_id = (int)py.flags.spells_learned_order[n];

                // shifting by amounts greater than number of bits in long gives
                // an undefined result, so don't shift for unknown spells
                if (order_id == 99)
                {
                    mask = 0x0;
                }
                else
                {
                    mask = (uint)(1L << order_id);
                }

                if ((mask & py.flags.spells_forgotten) != 0u)
                {
                    if (msp_ptr[order_id].level_required <= py.misc.level)
                    {
                        new_spells--;
                        py.flags.spells_forgotten &= ~mask;
                        py.flags.spells_learnt |= mask;

                        var msg = $"You have remembered the {p} of {Library.Instance.Player.spell_names[order_id + offset]}.";
                        //vtype_t msg = { '\0' };
                        //(void)sprintf(msg, "You have remembered the %s of %s.", p, spell_names[order_id + offset]);
                        printMessage(msg);
                    }
                    else
                    {
                        allowed_spells++;
                    }
                }
            }

            return new_spells;
        }

        // determine which spells player can learn must check all spells here,
        // in gain_spell() we actually check if the books are present
        public static int learnableSpells(IReadOnlyList<Spell_t> msp_ptr, int new_spells)
        {
            var py = State.Instance.py;
            var spell_flag = (uint)(0x7FFFFFFFL & ~py.flags.spells_learnt);

            var id = 0;
            uint mask = 0x1;

            for (var i = 0; spell_flag != 0u; mask <<= 1, i++)
            {
                if ((spell_flag & mask) != 0u)
                {
                    spell_flag &= ~mask;
                    if (msp_ptr[i].level_required <= py.misc.level)
                    {
                        id++;
                    }
                }
            }

            if (new_spells > id)
            {
                new_spells = id;
            }

            return new_spells;
        }

        // forget spells until new_spells_to_learn zero or no more spells know,
        // spells are forgotten in the opposite order that they were learned
        // NOTE: newSpells is always a negative value
        public static void forgetSpells(int new_spells, string p, int offset)
        {
            var py = State.Instance.py;
            uint mask;

            for (var i = 31; new_spells != 0 && py.flags.spells_learnt != 0u; i--)
            {
                // orderID is the (i+1)th spell learned
                var order_id = (int)py.flags.spells_learned_order[i];

                // shifting by amounts greater than number of bits in long gives
                // an undefined result, so don't shift for unknown spells
                if (order_id == 99)
                {
                    mask = 0x0;
                }
                else
                {
                    mask = (uint)(1L << order_id);
                }

                if ((mask & py.flags.spells_learnt) != 0u)
                {
                    py.flags.spells_learnt &= ~mask;
                    py.flags.spells_forgotten |= mask;
                    new_spells++;

                    var msg = $"You have forgotten the {p} of {Library.Instance.Player.spell_names[order_id + offset]}.";
                    //vtype_t msg = { '\0' };
                    //(void)sprintf(msg, "You have forgotten the %s of %s.", p, spell_names[order_id + offset]);
                    printMessage(msg);
                }
            }
        }

        // calculate number of spells player should have, and
        // learn forget spells until that number is met -JEW-
        public static void playerCalculateAllowedSpellsCount(int stat)
        {
            var py = State.Instance.py;
            var spell = Library.Instance.Player.magic_spells[(int)py.misc.class_id - 1];

            //const char* magic_type_str = nullptr;
            var magic_type_str = string.Empty;
            int offset;

            if (stat == (int)PlayerAttr.INT)
            {
                magic_type_str = "spell";
                offset = (int)Config.spells.NAME_OFFSET_SPELLS;
            }
            else
            {
                magic_type_str = "prayer";
                offset = (int)Config.spells.NAME_OFFSET_PRAYERS;
            }

            // check to see if know any spells greater than level, eliminate them
            eliminateKnownSpellsGreaterThanLevel(spell, magic_type_str, offset);

            // calc number of spells allowed
            var num_allowed = numberOfSpellsAllowed(stat);
            var num_known = numberOfSpellsKnown();
            var new_spells = num_allowed - num_known;

            if (new_spells > 0)
            {
                new_spells = rememberForgottenSpells(spell, num_allowed, new_spells, magic_type_str, offset);

                // If `new_spells_to_learn` is still greater than zero
                if (new_spells > 0)
                {
                    new_spells = learnableSpells(spell, new_spells);
                }
            }
            else if (new_spells < 0)
            {
                forgetSpells(new_spells, magic_type_str, offset);
                new_spells = 0;
            }

            if (new_spells != py.flags.new_spells_to_learn)
            {
                if (new_spells > 0 && py.flags.new_spells_to_learn == 0)
                {
                    var msg = $"You can learn some new {magic_type_str}s now.";
                    //vtype_t msg = { '\0' };
                    //(void)sprintf(msg, "You can learn some new %ss now.", magic_type_str);
                    printMessage(msg);
                }

                py.flags.new_spells_to_learn = (uint)new_spells;
                py.flags.status |= Config.player_status.PY_STUDY;
            }
        }

        public static string playerRankTitle()
        {
            var py = State.Instance.py;
            //const char* p = nullptr;
            var p = string.Empty;

            if (py.misc.level < 1)
            {
                p = "Babe in arms";
            }
            else if (py.misc.level <= PLAYER_MAX_LEVEL)
            {
                p = Library.Instance.Player.class_rank_titles[(int)py.misc.class_id][(int)py.misc.level - 1];
            }
            else if (playerIsMale())
            {
                p = "**KING**";
            }
            else
            {
                p = "**QUEEN**";
            }

            return p;
        }
    }
}
