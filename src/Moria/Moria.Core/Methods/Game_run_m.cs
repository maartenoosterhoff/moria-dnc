﻿using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using System.Resources;
using Moria.Core.Utils;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Game_c;
using static Moria.Core.Constants.Store_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dice_m;
using static Moria.Core.Methods.Dungeon_los_m;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Dungeon_generate_m;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Game_run_m;
using static Moria.Core.Methods.Game_files_m;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Monster_manager_m;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Game_save_m;
using static Moria.Core.Methods.Character_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Mage_spells_m;
using static Moria.Core.Methods.Player_magic_m;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Store_inventory_m;
using static Moria.Core.Methods.Game_death_m;
using static Moria.Core.Methods.Scores_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_move_m;
using static Moria.Core.Methods.Store_m;
using static Moria.Core.Methods.Scrolls_m;
using static Moria.Core.Methods.Std_m;
using static Moria.Core.Methods.Player_run_m;
using static Moria.Core.Methods.Player_pray_m;
using static Moria.Core.Methods.Player_tunnel_m;
using static Moria.Core.Methods.Player_throw_m;
using static Moria.Core.Methods.Player_eat_m;
using static Moria.Core.Methods.Player_traps_m;
using static Moria.Core.Methods.Player_bash_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Staves_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Ui_inventory_m;
using static Moria.Core.Methods.Wizard_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Player_quaff_m;

namespace Moria.Core.Methods
{
    public static class Game_run_m
    {
        public static void startMoria(int seed, bool start_new_game, bool use_roguelike_keys)
        {
            var py = State.Instance.py;
            var game = State.Instance.game;

            priceAdjust();

            // Show the game splash screen
            displaySplashScreen();

            // Grab a random seed from the clock
            seedsInitialize((uint)seed);

            // Init monster and treasure levels for allocate
            initializeMonsterLevels();
            initializeTreasureLevels();

            // Init the store inventories
            storeInitializeOwners();

            // NOTE: base exp levels need initializing before loading a game
            playerInitializeBaseExperienceLevels();

            // initialize some player fields - may or may not be needed -MRC-
            py.flags.spells_learnt = 0;
            py.flags.spells_worked = 0;
            py.flags.spells_forgotten = 0;

            // If -n is not passed, the calling routine will know
            // save file name, hence, this code is not necessary.

            // This restoration of a saved character may get ONLY the monster memory. In
            // this case, `loadGame()` returns false. It may also resurrect a dead character
            // (if you are the wizard). In this case, it returns true, but also sets the
            // parameter "generate" to true, as it does not recover any cave details.

            bool result = false;
            bool generate = false;

            if (!start_new_game && (access(Config.files.save_game, 0) == 0) && loadGame(generate))
            {
                result = true;
            }

            // Executing after game load to override saved game settings
            if (use_roguelike_keys)
            {
                Config.options.use_roguelike_keys = true;
            }

            // enter wizard mode before showing the character display, but must wait
            // until after loadGame() in case it was just a resurrection
            if (game.to_be_wizard)
            {
                if (!enterWizardMode())
                {
                    endGame();
                }
            }

            if (result)
            {
                changeCharacterName();

                // could be restoring a dead character after a signal or HANGUP
                if (py.misc.current_hp < 0)
                {
                    game.character_is_dead = true;
                }
            }
            else
            {
                // Create character
                characterCreate();

                py.misc.date_of_birth = DateTime.Now;

                initializeCharacterInventory();
                py.flags.food = 7500;
                py.flags.food_digested = 2;

                // Spell and Mana based on class: Mage or Clerical realm.
                if (State.Instance.classes[py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
                {
                    clearScreen(); // makes spell list easier to read
                    playerCalculateAllowedSpellsCount((int)PlayerAttr.INT);
                    playerGainMana((int)PlayerAttr.INT);
                }
                else if (State.Instance.classes[py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_PRIEST)
                {
                    playerCalculateAllowedSpellsCount((int)PlayerAttr.WIS);
                    clearScreen(); // force out the 'learn prayer' message
                    playerGainMana((int)PlayerAttr.WIS);
                }

                // Set some default values -MRC-
                py.temporary_light_only = false;
                py.weapon_is_heavy = false;
                py.pack.heaviness = 0;

                // prevent ^c quit from entering score into scoreboard,
                // and prevent signal from creating panic save until this
                // point, all info needed for save file is now valid.
                game.character_generated = true;
                generate = true;
            }

            magicInitializeItemNames();

            //
            // Begin the game
            //
            clearScreen();
            printCharacterStatsBlock();

            if (generate)
            {
                generateCave();
            }

            // Loop till dead, or exit
            while (!game.character_is_dead)
            {
                // Dungeon logic
                playDungeon();

                // check for eof here, see getKeyInput() in io.c
                // eof can occur if the process gets a HANGUP signal
                if (eof_flag != 0)
                {
                    game.character_died_from = "(end of input: saved)";
                    //(void)strcpy(game.character_died_from, "(end of input: saved)");
                    if (!saveGame())
                    {
                        game.character_died_from = "unexpected eof";
                        //(void)strcpy(game.character_died_from, "unexpected eof");
                    }

                    // should not reach here, but if we do, this guarantees exit
                    game.character_is_dead = true;
                }

                // New level if not dead
                if (!game.character_is_dead)
                {
                    generateCave();
                }
            }

            // Character gets buried.
            endGame();
        }

        // Init players with some belongings -RAK-
        public static void initializeCharacterInventory()
        {
            var py = State.Instance.py;

            Inventory_t item = new Inventory_t();

            // this is needed for bash to work right, it can't hurt anyway
            foreach (var entry in py.inventory)
            {
                inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_NOTHING, entry);
            }

            foreach (var item_id in State.Instance.class_base_provisions[py.misc.class_id])
            {
                inventoryItemCopyTo((int)item_id, item);

                // this makes it spellItemIdentifyAndRemoveRandomInscription and itemSetAsIdentified
                itemIdentifyAsStoreBought(item);

                // must set this bit to display to_hit/to_damage for stiletto
                if (item.category_id == TV_SWORD)
                {
                    item.identification |= Config.identification.ID_SHOW_HIT_DAM;
                }

                inventoryCarryItem(item);
            }

            // weird place for it, but why not?
            for (int i = 0; i < py.flags.spells_learned_order.Length; i++)//.spell uint8_t & id : py.flags.spells_learned_order)
            {
                py.flags.spells_learned_order[i] = 99;
                //id = 99;
            }
        }

        // Initializes M_LEVEL array for use with PLACE_MONSTER -RAK-
        static void initializeMonsterLevels()
        {
            var monster_levels = State.Instance.monster_levels;
            for (int i = 0; i < monster_levels.Length; i++)
            {
                monster_levels[i] = 0;
            }
            //for (auto & level : monster_levels)
            //{
            //    level = 0;
            //}

            for (int i = 0; i < MON_MAX_CREATURES - Config.monsters.MON_ENDGAME_MONSTERS; i++)
            {
                monster_levels[State.Instance.creatures_list[i].level]++;
            }

            for (int i = 1; i <= MON_MAX_LEVELS; i++)
            {
                monster_levels[i] += monster_levels[i - 1];
            }
        }

        // Initializes T_LEVEL array for use with PLACE_OBJECT -RAK-
        static void initializeTreasureLevels()
        {
            var treasure_levels = State.Instance.treasure_levels;
            for (int i = 0; i < treasure_levels.Length; i++)
            {
                treasure_levels[i] = 0;
            }
            //for (auto & level : treasure_levels)
            //{
            //    level = 0;
            //}

            for (int i = 0; i < MAX_DUNGEON_OBJECTS; i++)
            {
                treasure_levels[State.Instance.game_objects[i].depth_first_found]++;
            }

            for (int i = 1; i <= TREASURE_MAX_LEVELS; i++)
            {
                treasure_levels[i] += treasure_levels[i - 1];
            }

            // now produce an array with object indexes sorted by level,
            // by using the info in treasure_levels, this is an O(n) sort!
            // this is not a stable sort, but that does not matter
            var indexes = ArrayInitializer.InitializeWithDefault(TREASURE_MAX_LEVELS + 1, 1);
            //int indexes[TREASURE_MAX_LEVELS + 1] = { };
            //for (auto & i : indexes)
            //{
            //    i = 1;
            //}

            for (int i = 0; i < MAX_DUNGEON_OBJECTS; i++)
            {
                int level = (int)State.Instance.game_objects[i].depth_first_found;
                int object_id = treasure_levels[level] - indexes[level];

                State.Instance.sorted_objects[object_id] = (int)i;

                indexes[level]++;
            }
        }

        // Adjust prices of objects -RAK-
        static void priceAdjust()
        {
            if (COST_ADJUSTMENT != 100)
            {
                // round half-way cases up
                foreach (var item in State.Instance.game_objects)
                {
                    item.cost = ((item.cost * (int)COST_ADJUSTMENT) + 50) / 100;
                }
            }
        }

        // Moria game module -RAK-
        // The code in this section has gone through many revisions, and
        // some of it could stand some more hard work. -RAK-

        // It has had a bit more hard work. -CJS-

        // Reset flags and initialize variables
        public static void resetDungeonFlags()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            game.command_count = 0;
            dg.generate_new_level = false;
            py.running_tracker = 0;
            game.teleport_player = false;
            State.Instance.monster_multiply_total = 0;
            dg.floor[py.pos.y][py.pos.x].creature_id = 1;
        }

        // Check light status for dungeon setup
        public static void playerInitializePlayerLight()
        {
            var py = State.Instance.py;
            py.carrying_light = (py.inventory[(int)PlayerEquipment.Light].misc_use > 0);
        }

        // Check for a maximum level
        public static void playerUpdateMaxDungeonDepth()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            if (dg.current_level > py.misc.max_dungeon_depth)
            {
                py.misc.max_dungeon_depth = (uint)dg.current_level;
            }
        }

        // Check light status
        static void playerUpdateLightStatus()
        {
            var py = State.Instance.py;
            Inventory_t item = py.inventory[(int)PlayerEquipment.Light];

            if (py.carrying_light)
            {
                if (item.misc_use > 0)
                {
                    item.misc_use--;

                    if (item.misc_use == 0)
                    {
                        py.carrying_light = false;
                        printMessage("Your light has gone out!");
                        playerDisturb(0, 1);

                        // unlight creatures
                        updateMonsters(false);
                    }
                    else if (item.misc_use < 40 && randomNumber(5) == 1 && py.flags.blind < 1)
                    {
                        playerDisturb(0, 0);
                        printMessage("Your light is growing faint.");
                    }
                }
                else
                {
                    py.carrying_light = false;
                    playerDisturb(0, 1);

                    // unlight creatures
                    updateMonsters(false);
                }
            }
            else if (item.misc_use > 0)
            {
                item.misc_use--;
                py.carrying_light = true;
                playerDisturb(0, 1);

                // light creatures
                updateMonsters(false);
            }
        }

        public static void playerActivateHeroism()
        {
            var py = State.Instance.py;

            py.flags.status |= Config.player_status.PY_HERO;
            playerDisturb(0, 0);

            py.misc.max_hp += 10;
            py.misc.current_hp += 10;
            py.misc.bth += 12;
            py.misc.bth_with_bows += 12;

            printMessage("You feel like a HERO!");
            printCharacterMaxHitPoints();
            printCharacterCurrentHitPoints();
        }

        public static void playerDisableHeroism()
        {
            var py = State.Instance.py;

            py.flags.status &= ~Config.player_status.PY_HERO;
            playerDisturb(0, 0);

            py.misc.max_hp -= 10;
            if (py.misc.current_hp > py.misc.max_hp)
            {
                py.misc.current_hp = py.misc.max_hp;
                py.misc.current_hp_fraction = 0;
                printCharacterCurrentHitPoints();
            }
            py.misc.bth -= 12;
            py.misc.bth_with_bows -= 12;

            printMessage("The heroism wears off.");
            printCharacterMaxHitPoints();
        }

        public static void playerActivateSuperHeroism()
        {
            var py = State.Instance.py;

            py.flags.status |= Config.player_status.PY_SHERO;
            playerDisturb(0, 0);

            py.misc.max_hp += 20;
            py.misc.current_hp += 20;
            py.misc.bth += 24;
            py.misc.bth_with_bows += 24;

            printMessage("You feel like a SUPER HERO!");
            printCharacterMaxHitPoints();
            printCharacterCurrentHitPoints();
        }

        public static void playerDisableSuperHeroism()
        {
            var py = State.Instance.py;

            py.flags.status &= ~Config.player_status.PY_SHERO;
            playerDisturb(0, 0);

            py.misc.max_hp -= 20;
            if (py.misc.current_hp > py.misc.max_hp)
            {
                py.misc.current_hp = py.misc.max_hp;
                py.misc.current_hp_fraction = 0;
                printCharacterCurrentHitPoints();
            }
            py.misc.bth -= 24;
            py.misc.bth_with_bows -= 24;

            printMessage("The super heroism wears off.");
            printCharacterMaxHitPoints();
        }

        public static void playerUpdateHeroStatus()
        {
            var py = State.Instance.py;

            // Heroism
            if (py.flags.heroism > 0)
            {
                if ((py.flags.status & Config.player_status.PY_HERO) == 0)
                {
                    playerActivateHeroism();
                }

                py.flags.heroism--;

                if (py.flags.heroism == 0)
                {
                    playerDisableHeroism();
                }
            }

            // Super Heroism
            if (py.flags.super_heroism > 0)
            {
                if ((py.flags.status & Config.player_status.PY_SHERO) == 0)
                {
                    playerActivateSuperHeroism();
                }

                py.flags.super_heroism--;

                if (py.flags.super_heroism == 0)
                {
                    playerDisableSuperHeroism();
                }
            }
        }

        public static int playerFoodConsumption()
        {
            var py = State.Instance.py;

            // Regenerate hp and mana
            int regen_amount = (int)Config.player.PLAYER_REGEN_NORMAL;

            if (py.flags.food < Config.player.PLAYER_FOOD_ALERT)
            {
                if (py.flags.food < Config.player.PLAYER_FOOD_WEAK)
                {
                    if (py.flags.food < 0)
                    {
                        regen_amount = 0;
                    }
                    else if (py.flags.food < Config.player.PLAYER_FOOD_FAINT)
                    {
                        regen_amount = (int)Config.player.PLAYER_REGEN_FAINT;
                    }
                    else if (py.flags.food < Config.player.PLAYER_FOOD_WEAK)
                    {
                        regen_amount = (int)Config.player.PLAYER_REGEN_WEAK;
                    }

                    if ((py.flags.status & Config.player_status.PY_WEAK) == 0)
                    {
                        py.flags.status |= Config.player_status.PY_WEAK;
                        printMessage("You are getting weak from hunger.");
                        playerDisturb(0, 0);
                        printCharacterHungerStatus();
                    }

                    if (py.flags.food < Config.player.PLAYER_FOOD_FAINT && randomNumber(8) == 1)
                    {
                        py.flags.paralysis += randomNumber(5);
                        printMessage("You faint from the lack of food.");
                        playerDisturb(1, 0);
                    }
                }
                else if ((py.flags.status & Config.player_status.PY_HUNGRY) == 0)
                {
                    py.flags.status |= Config.player_status.PY_HUNGRY;
                    printMessage("You are getting hungry.");
                    playerDisturb(0, 0);
                    printCharacterHungerStatus();
                }
            }

            // Food consumption
            // Note: Sped up characters really burn up the food!
            if (py.flags.speed < 0)
            {
                py.flags.food -= py.flags.speed * py.flags.speed;
            }

            py.flags.food -= py.flags.food_digested;

            if (py.flags.food < 0)
            {
                playerTakesHit(-py.flags.food / 16, "starvation"); // -CJS-
                playerDisturb(1, 0);
            }

            return regen_amount;
        }

        public static void playerUpdateRegeneration(int amount)
        {
            var py = State.Instance.py;

            if (py.flags.regenerate_hp)
            {
                amount = amount * 3 / 2;
            }

            if (((py.flags.status & Config.player_status.PY_SEARCH) != 0u) || py.flags.rest != 0)
            {
                amount = amount * 2;
            }

            if (py.flags.poisoned < 1 && py.misc.current_hp < py.misc.max_hp)
            {
                playerRegenerateHitPoints(amount);
            }

            if (py.misc.current_mana < py.misc.mana)
            {
                playerRegenerateMana(amount);
            }
        }

        public static void playerUpdateBlindness()
        {
            var py = State.Instance.py;

            if (py.flags.blind <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_BLIND) == 0)
            {
                py.flags.status |= Config.player_status.PY_BLIND;

                drawDungeonPanel();
                printCharacterBlindStatus();
                playerDisturb(0, 1);

                // unlight creatures
                updateMonsters(false);
            }

            py.flags.blind--;

            if (py.flags.blind == 0)
            {
                py.flags.status &= ~Config.player_status.PY_BLIND;

                printCharacterBlindStatus();
                drawDungeonPanel();
                playerDisturb(0, 1);

                // light creatures
                updateMonsters(false);

                printMessage("The veil of darkness lifts.");
            }
        }

        public static void playerUpdateConfusion()
        {
            var py = State.Instance.py;

            if (py.flags.confused <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_CONFUSED) == 0)
            {
                py.flags.status |= Config.player_status.PY_CONFUSED;
                printCharacterConfusedState();
            }

            py.flags.confused--;

            if (py.flags.confused == 0)
            {
                py.flags.status &= ~Config.player_status.PY_CONFUSED;

                printCharacterConfusedState();
                printMessage("You feel less confused now.");

                if (py.flags.rest != 0)
                {
                    playerRestOff();
                }
            }
        }

        public static void playerUpdateFearState()
        {
            var py = State.Instance.py;

            if (py.flags.afraid <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_FEAR) == 0)
            {
                if (py.flags.super_heroism + py.flags.heroism > 0)
                {
                    py.flags.afraid = 0;
                }
                else
                {
                    py.flags.status |= Config.player_status.PY_FEAR;
                    printCharacterFearState();
                }
            }
            else if (py.flags.super_heroism + py.flags.heroism > 0)
            {
                py.flags.afraid = 1;
            }

            py.flags.afraid--;

            if (py.flags.afraid == 0)
            {
                py.flags.status &= ~Config.player_status.PY_FEAR;

                printCharacterFearState();
                printMessage("You feel bolder now.");
                playerDisturb(0, 0);
            }
        }

        public static void playerUpdatePoisonedState()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            if (py.flags.poisoned <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_POISONED) == 0)
            {
                py.flags.status |= Config.player_status.PY_POISONED;
                printCharacterPoisonedState();
            }

            py.flags.poisoned--;

            if (py.flags.poisoned == 0)
            {
                py.flags.status &= ~Config.player_status.PY_POISONED;

                printCharacterPoisonedState();
                printMessage("You feel better.");
                playerDisturb(0, 0);

                return;
            }

            int damage;

            switch (playerStatAdjustmentConstitution())
            {
                case -4:
                    damage = 4;
                    break;
                case -3:
                case -2:
                    damage = 3;
                    break;
                case -1:
                    damage = 2;
                    break;
                case 0:
                    damage = 1;
                    break;
                case 1:
                case 2:
                case 3:
                    damage = ((dg.game_turn % 2) == 0 ? 1 : 0);
                    break;
                case 4:
                case 5:
                    damage = ((dg.game_turn % 3) == 0 ? 1 : 0);
                    break;
                case 6:
                    damage = ((dg.game_turn % 4) == 0 ? 1 : 0);
                    break;
                default:
                    damage = 0;
                    break;
            }

            playerTakesHit(damage, "poison");
            playerDisturb(1, 0);
        }

        public static void playerUpdateFastness()
        {
            var py = State.Instance.py;

            if (py.flags.fast <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_FAST) == 0)
            {
                py.flags.status |= Config.player_status.PY_FAST;
                playerChangeSpeed(-1);

                printMessage("You feel yourself moving faster.");
                playerDisturb(0, 0);
            }

            py.flags.fast--;

            if (py.flags.fast == 0)
            {
                py.flags.status &= ~Config.player_status.PY_FAST;
                playerChangeSpeed(1);

                printMessage("You feel yourself slow down.");
                playerDisturb(0, 0);
            }
        }

        public static void playerUpdateSlowness()
        {
            var py = State.Instance.py;

            if (py.flags.slow <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_SLOW) == 0)
            {
                py.flags.status |= Config.player_status.PY_SLOW;
                playerChangeSpeed(1);

                printMessage("You feel yourself moving slower.");
                playerDisturb(0, 0);
            }

            py.flags.slow--;

            if (py.flags.slow == 0)
            {
                py.flags.status &= ~Config.player_status.PY_SLOW;
                playerChangeSpeed(-1);

                printMessage("You feel yourself speed up.");
                playerDisturb(0, 0);
            }
        }

        public static void playerUpdateSpeed()
        {
            playerUpdateFastness();
            playerUpdateSlowness();
        }

        // Resting is over?
        public static void playerUpdateRestingState()
        {
            var py = State.Instance.py;

            if (py.flags.rest > 0)
            {
                py.flags.rest--;

                // Resting over
                if (py.flags.rest == 0)
                {
                    playerRestOff();
                }
            }
            else if (py.flags.rest < 0)
            {
                // Rest until reach max mana and max hit points.
                py.flags.rest++;

                if ((py.misc.current_hp == py.misc.max_hp && py.misc.current_mana == py.misc.mana) || py.flags.rest == 0)
                {
                    playerRestOff();
                }
            }
        }

        // Hallucinating?   (Random characters appear!)
        public static void playerUpdateHallucination()
        {
            var py = State.Instance.py;

            if (py.flags.image <= 0)
            {
                return;
            }

            playerEndRunning();

            py.flags.image--;

            if (py.flags.image == 0)
            {
                // Used to draw entire screen! -CJS-
                drawDungeonPanel();
            }
        }

        public static void playerUpdateParalysis()
        {
            var py = State.Instance.py;

            if (py.flags.paralysis <= 0)
            {
                return;
            }

            // when paralysis true, you can not see any movement that occurs
            py.flags.paralysis--;

            playerDisturb(1, 0);
        }

        // Protection from evil counter
        public static void playerUpdateEvilProtection()
        {
            var py = State.Instance.py;

            if (py.flags.protect_evil <= 0)
            {
                return;
            }

            py.flags.protect_evil--;

            if (py.flags.protect_evil == 0)
            {
                printMessage("You no longer feel safe from evil.");
            }
        }

        public static void playerUpdateInvulnerability()
        {
            var py = State.Instance.py;

            if (py.flags.invulnerability <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_INVULN) == 0)
            {
                py.flags.status |= Config.player_status.PY_INVULN;
                playerDisturb(0, 0);

                py.misc.ac += 100;
                py.misc.display_ac += 100;

                printCharacterCurrentArmorClass();
                printMessage("Your skin turns into steel!");
            }

            py.flags.invulnerability--;

            if (py.flags.invulnerability == 0)
            {
                py.flags.status &= ~Config.player_status.PY_INVULN;
                playerDisturb(0, 0);

                py.misc.ac -= 100;
                py.misc.display_ac -= 100;

                printCharacterCurrentArmorClass();
                printMessage("Your skin returns to normal.");
            }
        }

        public static void playerUpdateBlessedness()
        {
            var py = State.Instance.py;

            if (py.flags.blessed <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_BLESSED) == 0)
            {
                py.flags.status |= Config.player_status.PY_BLESSED;
                playerDisturb(0, 0);

                py.misc.bth += 5;
                py.misc.bth_with_bows += 5;
                py.misc.ac += 2;
                py.misc.display_ac += 2;

                printMessage("You feel righteous!");
                printCharacterCurrentArmorClass();
            }

            py.flags.blessed--;

            if (py.flags.blessed == 0)
            {
                py.flags.status &= ~Config.player_status.PY_BLESSED;
                playerDisturb(0, 0);

                py.misc.bth -= 5;
                py.misc.bth_with_bows -= 5;
                py.misc.ac -= 2;
                py.misc.display_ac -= 2;

                printMessage("The prayer has expired.");
                printCharacterCurrentArmorClass();
            }
        }

        // Resist Heat
        public static void playerUpdateHeatResistance()
        {
            var py = State.Instance.py;

            if (py.flags.heat_resistance <= 0)
            {
                return;
            }

            py.flags.heat_resistance--;

            if (py.flags.heat_resistance == 0)
            {
                printMessage("You no longer feel safe from flame.");
            }
        }

        public static void playerUpdateColdResistance()
        {
            var py = State.Instance.py;

            if (py.flags.cold_resistance <= 0)
            {
                return;
            }

            py.flags.cold_resistance--;

            if (py.flags.cold_resistance == 0)
            {
                printMessage("You no longer feel safe from cold.");
            }
        }

        public static void playerUpdateDetectInvisible()
        {
            var py = State.Instance.py;

            if (py.flags.detect_invisible <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_DET_INV) == 0)
            {
                py.flags.status |= Config.player_status.PY_DET_INV;
                py.flags.see_invisible = true;

                // light but don't move creatures
                updateMonsters(false);
            }

            py.flags.detect_invisible--;

            if (py.flags.detect_invisible == 0)
            {
                py.flags.status &= ~Config.player_status.PY_DET_INV;

                // may still be able to see_invisible if wearing magic item
                playerRecalculateBonuses();

                // unlight but don't move creatures
                updateMonsters(false);
            }
        }

        // Timed infra-vision
        public static void playerUpdateInfraVision()
        {
            var py = State.Instance.py;

            if (py.flags.timed_infra <= 0)
            {
                return;
            }

            if ((py.flags.status & Config.player_status.PY_TIM_INFRA) == 0)
            {
                py.flags.status |= Config.player_status.PY_TIM_INFRA;
                py.flags.see_infra++;

                // light but don't move creatures
                updateMonsters(false);
            }

            py.flags.timed_infra--;

            if (py.flags.timed_infra == 0)
            {
                py.flags.status &= ~Config.player_status.PY_TIM_INFRA;
                py.flags.see_infra--;

                // unlight but don't move creatures
                updateMonsters(false);
            }
        }

        // Word-of-Recall  Note: Word-of-Recall is a delayed action
        public static void playerUpdateWordOfRecall()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            if (py.flags.word_of_recall <= 0)
            {
                return;
            }

            if (py.flags.word_of_recall == 1)
            {
                dg.generate_new_level = true;

                py.flags.paralysis++;
                py.flags.word_of_recall = 0;

                if (dg.current_level > 0)
                {
                    dg.current_level = 0;
                    printMessage("You feel yourself yanked upwards!");
                }
                else if (py.misc.max_dungeon_depth != 0)
                {
                    dg.current_level = (int)py.misc.max_dungeon_depth;
                    printMessage("You feel yourself yanked downwards!");
                }
            }
            else
            {
                py.flags.word_of_recall--;
            }
        }

        public static void playerUpdateStatusFlags()
        {
            var py = State.Instance.py;

            if ((py.flags.status & Config.player_status.PY_SPEED) != 0u)
            {
                py.flags.status &= ~Config.player_status.PY_SPEED;
                printCharacterSpeed();
            }

            if (((py.flags.status & Config.player_status.PY_PARALYSED) != 0u) && py.flags.paralysis < 1)
            {
                printCharacterMovementState();
                py.flags.status &= ~Config.player_status.PY_PARALYSED;
            }
            else if (py.flags.paralysis > 0)
            {
                printCharacterMovementState();
                py.flags.status |= Config.player_status.PY_PARALYSED;
            }
            else if (py.flags.rest != 0)
            {
                printCharacterMovementState();
            }

            if ((py.flags.status & Config.player_status.PY_ARMOR) != 0)
            {
                printCharacterCurrentArmorClass();
                py.flags.status &= ~Config.player_status.PY_ARMOR;
            }

            if ((py.flags.status & Config.player_status.PY_STATS) != 0)
            {
                for (int n = 0; n < 6; n++)
                {
                    if (((Config.player_status.PY_STR << n) & py.flags.status) != 0u)
                    {
                        displayCharacterStats(n);
                    }
                }

                py.flags.status &= ~Config.player_status.PY_STATS;
            }

            if ((py.flags.status & Config.player_status.PY_HP) != 0u)
            {
                printCharacterMaxHitPoints();
                printCharacterCurrentHitPoints();
                py.flags.status &= ~Config.player_status.PY_HP;
            }

            if ((py.flags.status & Config.player_status.PY_MANA) != 0u)
            {
                printCharacterCurrentMana();
                py.flags.status &= ~Config.player_status.PY_MANA;
            }
        }

        // Allow for a slim chance of detect enchantment -CJS-
        public static void playerDetectEnchantment()
        {
            var py = State.Instance.py;

            for (int i = 0; i < PLAYER_INVENTORY_SIZE; i++)
            {
                if (i == py.pack.unique_items)
                {
                    i = 22;
                }

                Inventory_t item = py.inventory[i];

                // if in inventory, succeed 1 out of 50 times,
                // if in equipment list, success 1 out of 10 times
                int chance = (i < 22 ? 50 : 10);

                if (item.category_id != TV_NOTHING && itemEnchanted(item) && randomNumber(chance) == 1)
                {
                    var tmp_str = $"There's something about what you {playerItemWearingDescription(i)}...";
                    //vtype_t tmp_str = { '\0' };
                    //(void)sprintf(tmp_str, "There's something about what you are %s...", playerItemWearingDescription(i));
                    playerDisturb(0, 0);
                    printMessage(tmp_str);
                    itemAppendToInscription(item, Config.identification.ID_MAGIK);
                }
            }
        }

        public static int getCommandRepeatCount(ref char last_input_command)
        {
            putStringClearToEOL("Repeat count:", new Coord_t(0, 0));

            if (last_input_command == '#')
            {
                last_input_command = '0';
            }

            string text_buffer;
            int repeat_count = 0;

            while (true)
            {
                if (last_input_command == DELETE || last_input_command == CTRL_KEY_H)
                {
                    repeat_count /= 10;
                    text_buffer = $"{repeat_count:d}";
                    //(void)sprintf(text_buffer, "%d", (int16_t)repeat_count);
                    putStringClearToEOL(text_buffer, new Coord_t(0, 14));
                }
                else if (last_input_command >= '0' && last_input_command <= '9')
                {
                    if (repeat_count > 99)
                    {
                        terminalBellSound();
                    }
                    else
                    {
                        repeat_count = repeat_count * 10 + last_input_command - '0';
                        text_buffer = $"{repeat_count:d}";
                        //(void)sprintf(text_buffer, "%d", repeat_count);
                        putStringClearToEOL(text_buffer, new Coord_t(0, 14));
                    }
                }
                else
                {
                    break;
                }
                last_input_command = getKeyInput();
            }

            if (repeat_count == 0)
            {
                repeat_count = 99;
                text_buffer = $"{repeat_count:d}";
                //(void)sprintf(text_buffer, "%d", repeat_count);
                putStringClearToEOL(text_buffer, new Coord_t(0, 14));
            }

            // a special hack to allow numbers as commands
            if (last_input_command == ' ')
            {
                putStringClearToEOL("Command:", new Coord_t(0, 20));
                last_input_command = getKeyInput();
            }

            return repeat_count;
        }

        public static char parseAlternateCtrlInput(out char last_input_command)
        {
            var game = State.Instance.game;
            if (game.command_count > 0)
            {
                printCharacterMovementState();
            }

            if (getCommand("Control-", out last_input_command))
            {
                if (last_input_command >= 'A' && last_input_command <= 'Z')
                {
                    last_input_command = (char)(last_input_command - ('A' - 1));
                }
                else if (last_input_command >= 'a' && last_input_command <= 'z')
                {
                    last_input_command = (char)(last_input_command - ('a' - 1));
                }
                else
                {
                    last_input_command = ' ';
                    printMessage("Type ^ <letter> for a control char");
                }
            }
            else
            {
                last_input_command = ' ';
            }

            return last_input_command;
        }

        // Accept a command and execute it
        public static void executeInputCommands(ref char command, ref int find_count)
        {
            var py = State.Instance.py;
            var game = State.Instance.game;

            char last_input_command = command;

            // Accept a command and execute it
            do
            {
                if ((py.flags.status & Config.player_status.PY_REPEAT) != 0u)
                {
                    printCharacterMovementState();
                }

                game.use_last_direction = false;
                game.player_free_turn = false;

                if (py.running_tracker != 0)
                {
                    playerRunAndFind();
                    find_count -= 1;

                    if (find_count == 0)
                    {
                        playerEndRunning();
                    }

                    putQIO();
                    continue;
                }

                if (game.doing_inventory_command != 0)
                {
                    inventoryExecuteCommand((char)game.doing_inventory_command);
                    continue;
                }

                // move the cursor to the players character
                panelMoveCursor(py.pos);

                State.Instance.message_ready_to_print = false;

                if (game.command_count > 0)
                {
                    game.use_last_direction = true;
                }
                else
                {
                    last_input_command = getKeyInput();

                    // Get a count for a command.
                    int repeat_count = 0;
                    if ((Config.options.use_roguelike_keys && last_input_command >= '0' && last_input_command <= '9') || (!Config.options.use_roguelike_keys && last_input_command == '#'))
                    {
                        repeat_count = getCommandRepeatCount(ref last_input_command);
                    }

                    // Another way of typing control codes -CJS-
                    if (last_input_command == '^')
                    {
                        last_input_command = parseAlternateCtrlInput(out last_input_command);
                    }

                    // move cursor to player char again, in case it moved
                    panelMoveCursor(py.pos);

                    // Commands are always converted to rogue form. -CJS-
                    if (!Config.options.use_roguelike_keys)
                    {
                        last_input_command = originalCommands(last_input_command);
                    }

                    if (repeat_count > 0)
                    {
                        if (!validCountCommand(last_input_command))
                        {
                            game.player_free_turn = true;
                            last_input_command = ' ';
                            printMessage("Invalid command with a count.");
                        }
                        else
                        {
                            game.command_count = repeat_count;
                            printCharacterMovementState();
                        }
                    }
                }

                // Flash the message line.
                messageLineClear();
                panelMoveCursor(py.pos);
                putQIO();

                doCommand(last_input_command);

                // Find is counted differently, as the command changes.
                if (py.running_tracker != 0)
                {
                    find_count = game.command_count - 1;
                    game.command_count = 0;
                }
                else if (game.player_free_turn)
                {
                    game.command_count = 0;
                }
                else if (game.command_count != 0)
                {
                    game.command_count--;
                }
            } while (game.player_free_turn && !State.Instance.dg.generate_new_level && (eof_flag == 0));

            command = last_input_command;
        }

        static char originalCommands(char command)
        {
            int direction = 0;

            switch (command)
            {
                case CTRL_KEY_K: // ^K = exit
                    command = 'Q';
                    break;
                case CTRL_KEY_J:
                case CTRL_KEY_M:
                    command = '+';
                    break;
                case CTRL_KEY_P: // ^P = repeat
                case CTRL_KEY_W: // ^W = password
                case CTRL_KEY_X: // ^X = save
                case CTRL_KEY_V: // ^V = view license
                case ' ':
                case '!':
                case '$':
                    break;
                case '.':
                    if (getDirectionWithMemory(/*CNIL*/ null, ref direction))
                    {
                        switch (direction)
                        {
                            case 1:
                                command = 'B';
                                break;
                            case 2:
                                command = 'J';
                                break;
                            case 3:
                                command = 'N';
                                break;
                            case 4:
                                command = 'H';
                                break;
                            case 6:
                                command = 'L';
                                break;
                            case 7:
                                command = 'Y';
                                break;
                            case 8:
                                command = 'K';
                                break;
                            case 9:
                                command = 'U';
                                break;
                            default:
                                command = ' ';
                                break;
                        }
                    }
                    else
                    {
                        command = ' ';
                    }
                    break;
                case '/':
                case '<':
                case '>':
                case '-':
                case '=':
                case '{':
                case '?':
                case 'A':
                    break;
                case '1':
                    command = 'b';
                    break;
                case '2':
                    command = 'j';
                    break;
                case '3':
                    command = 'n';
                    break;
                case '4':
                    command = 'h';
                    break;
                case '5': // Rest one turn
                    command = '.';
                    break;
                case '6':
                    command = 'l';
                    break;
                case '7':
                    command = 'y';
                    break;
                case '8':
                    command = 'k';
                    break;
                case '9':
                    command = 'u';
                    break;
                case 'B':
                    command = 'f';
                    break;
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                    break;
                case 'L':
                    command = 'W';
                    break;
                case 'M':
                case 'R':
                    break;
                case 'S':
                    command = '#';
                    break;
                case 'T':
                    if (getDirectionWithMemory(/*CNIL*/ null, ref direction))
                    {
                        switch (direction)
                        {
                            case 1:
                                command = CTRL_KEY_B;
                                break;
                            case 2:
                                command = CTRL_KEY_J;
                                break;
                            case 3:
                                command = CTRL_KEY_N;
                                break;
                            case 4:
                                command = CTRL_KEY_H;
                                break;
                            case 6:
                                command = CTRL_KEY_L;
                                break;
                            case 7:
                                command = CTRL_KEY_Y;
                                break;
                            case 8:
                                command = CTRL_KEY_K;
                                break;
                            case 9:
                                command = CTRL_KEY_U;
                                break;
                            default:
                                command = ' ';
                                break;
                        }
                    }
                    else
                    {
                        command = ' ';
                    }
                    break;
                case 'V':
                    break;
                case 'a':
                    command = 'z';
                    break;
                case 'b':
                    command = 'P';
                    break;
                case 'c':
                case 'd':
                case 'e':
                    break;
                case 'f':
                    command = 't';
                    break;
                case 'h':
                    command = '?';
                    break;
                case 'i':
                    break;
                case 'j':
                    command = 'S';
                    break;
                case 'l':
                    command = 'x';
                    break;
                case 'm':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                    break;
                case 't':
                    command = 'T';
                    break;
                case 'u':
                    command = 'Z';
                    break;
                case 'v':
                case 'w':
                    break;
                case 'x':
                    command = 'X';
                    break;

                // wizard mode commands follow
                case CTRL_KEY_A: // ^A = cure all
                    break;
                case CTRL_KEY_B: // ^B = objects
                    command = CTRL_KEY_O;
                    break;
                case CTRL_KEY_D: // ^D = up/down
                    break;
                case CTRL_KEY_H: // ^H = wizhelp
                    command = '\\';
                    break;
                case CTRL_KEY_I: // ^I = identify
                    break;
                case CTRL_KEY_L: // ^L = wizlight
                    command = '*';
                    break;
                case ':':
                case CTRL_KEY_T: // ^T = teleport
                case CTRL_KEY_E: // ^E = wizchar
                case CTRL_KEY_F: // ^F = genocide
                case CTRL_KEY_G: // ^G = treasure
                case '@':
                case '+':
                    break;
                case CTRL_KEY_U: // ^U = summon
                    command = '&';
                    break;
                default:
                    command = '~'; // Anything illegal.
                    break;
            }

            return command;
        }

        public static bool moveWithoutPickup(ref char command)
        {
            var game = State.Instance.game;

            char cmd = command;

            // hack for move without pickup.  Map '-' to a movement command.
            if (cmd != '-')
            {
                return true;
            }

            int direction = 0;

            // Save current game.command_count as getDirectionWithMemory() may change it
            int count_save = game.command_count;

            if (getDirectionWithMemory(/*CNIL*/null, ref direction))
            {
                // Restore game.command_count
                game.command_count = count_save;

                switch (direction)
                {
                    case 1:
                        cmd = 'b';
                        break;
                    case 2:
                        cmd = 'j';
                        break;
                    case 3:
                        cmd = 'n';
                        break;
                    case 4:
                        cmd = 'h';
                        break;
                    case 6:
                        cmd = 'l';
                        break;
                    case 7:
                        cmd = 'y';
                        break;
                    case 8:
                        cmd = 'k';
                        break;
                    case 9:
                        cmd = 'u';
                        break;
                    default:
                        cmd = '~';
                        break;
                }
            }
            else
            {
                cmd = ' ';
            }

            //*command = cmd;

            return false;
        }

        public static void commandQuit()
        {
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            flushInputBuffer();

            if (getInputConfirmation("Do you really want to quit?"))
            {
                game.character_is_dead = true;
                dg.generate_new_level = true;

                game.character_died_from = "Quitting";
                //(void)strcpy(game.character_died_from, "Quitting");
            }
        }

        public static uint calculateMaxMessageCount()
        {
            var game = State.Instance.game;

            uint max_messages = MESSAGE_HISTORY_SIZE;

            if (game.command_count > 0)
            {
                if (game.command_count < MESSAGE_HISTORY_SIZE)
                {
                    max_messages = (uint)game.command_count;
                }
                game.command_count = 0;
            }
            else if (game.last_command != CTRL_KEY_P)
            {
                max_messages = 1;
            }

            return max_messages;
        }

        public static void commandPreviousMessage()
        {
            uint max_messages = calculateMaxMessageCount();

            if (max_messages <= 1)
            {
                // Distinguish real and recovered messages with a '>'. -CJS-
                putString(">", new Coord_t(0, 0));
                putStringClearToEOL(State.Instance.messages[State.Instance.last_message_id], new Coord_t(0, 1));
                return;
            }

            terminalSaveScreen();

            uint line_number = max_messages;
            int msg_id = State.Instance.last_message_id;

            while (max_messages > 0)
            {
                max_messages--;

                putStringClearToEOL(State.Instance.messages[msg_id], new Coord_t((int)max_messages, 0));

                if (msg_id == 0)
                {
                    msg_id = (int)MESSAGE_HISTORY_SIZE - 1;
                }
                else
                {
                    msg_id--;
                }
            }

            eraseLine(new Coord_t((int)line_number, 0));
            waitForContinueKey((int)line_number);
            terminalRestoreScreen();
        }

        public static void commandFlipWizardMode()
        {
            var game = State.Instance.game;
            if (game.wizard_mode)
            {
                game.wizard_mode = false;
                printMessage("Wizard mode off.");
            }
            else if (enterWizardMode())
            {
                printMessage("Wizard mode on.");
            }

            printCharacterWinner();
        }

        public static void commandSaveAndExit()
        {
            var game = State.Instance.game;
            if (game.total_winner)
            {
                printMessage("You are a Total Winner,  your character must be retired.");

                if (Config.options.use_roguelike_keys)
                {
                    printMessage("Use 'Q' to when you are ready to quit.");
                }
                else
                {
                    printMessage("Use <Control>-K when you are ready to quit.");
                }
            }
            else
            {
                game.character_died_from = "(saved)";
                //(void)strcpy(game.character_died_from, "(saved)");
                printMessage("Saving game...");

                if (saveGame())
                {
                    endGame();
                }

                game.character_died_from = "(alive and well)";
                //(void)strcpy(game.character_died_from, "(alive and well)");
            }
        }

        public static void commandLocateOnMap()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            if (py.flags.blind > 0 || playerNoLight())
            {
                printMessage("You can't see your map.");
                return;
            }

            Coord_t player_coord = py.pos;
            if (coordOutsidePanel(player_coord, true))
            {
                drawDungeonPanel();
            }

            int dir_val = 0;
            var out_val = string.Empty;
            var tmp_str = string.Empty;
            //vtype_t out_val = { '\0' };
            //vtype_t tmp_str = { '\0' };

            Coord_t old_panel = new Coord_t(dg.panel.row, dg.panel.col);
            Coord_t panel = new Coord_t(0, 0);

            while (true)
            {
                panel.y = dg.panel.row;
                panel.x = dg.panel.col;

                if (panel.y == old_panel.y && panel.x == old_panel.x)
                {
                    tmp_str = string.Empty;
                    //tmp_str[0] = '\0';
                }
                else
                {
                    var p1 = panel.y < old_panel.y ? " North" : panel.y > old_panel.y ? " South" : "";
                    var p2 = panel.x < old_panel.x ? " West" : panel.x > old_panel.x ? " East" : "";
                    tmp_str = $"{p1}{p2} of";
                    //(void)sprintf(tmp_str,                                                                  //
                    //               "%s%s of",                                                                //
                    //               panel.y < old_panel.y ? " North" : panel.y > old_panel.y ? " South" : "", //
                    //               panel.x < old_panel.x ? " West" : panel.x > old_panel.x ? " East" : ""    //
                    //);
                }

                out_val = $"Map sector [{panel.y:d},{panel.x:d}], which is{tmp_str} your sector. Look which direction?";
                //(void)sprintf(out_val, "Map sector [%d,%d], which is%s your sector. Look which direction?", panel.y, panel.x, tmp_str);

                if (!getDirectionWithMemory(out_val, ref dir_val))
                {
                    break;
                }

                // -CJS-
                // Should really use the move function, but what the hell. This
                // is nicer, as it moves exactly to the same place in another
                // section. The direction calculation is not intuitive. Sorry.
                while (true)
                {
                    player_coord.x += ((dir_val - 1) % 3 - 1) * (int)SCREEN_WIDTH / 2;
                    player_coord.y -= ((dir_val - 1) / 3 - 1) * (int)SCREEN_HEIGHT / 2;

                    if (player_coord.x < 0 || player_coord.y < 0 || player_coord.x >= dg.width || player_coord.y >= dg.width)
                    {
                        printMessage("You've gone past the end of your map.");

                        player_coord.x -= ((dir_val - 1) % 3 - 1) * (int)SCREEN_WIDTH / 2;
                        player_coord.y += ((dir_val - 1) / 3 - 1) * (int)SCREEN_HEIGHT / 2;

                        break;
                    }

                    if (coordOutsidePanel(player_coord, true))
                    {
                        drawDungeonPanel();
                        break;
                    }
                }
            }

            // Move to a new panel - but only if really necessary.
            if (coordOutsidePanel(py.pos, false))
            {
                drawDungeonPanel();
            }
        }

        public static void commandToggleSearch()
        {
            var py = State.Instance.py;

            if ((py.flags.status & Config.player_status.PY_SEARCH) != 0u)
            {
                playerSearchOff();
            }
            else
            {
                playerSearchOn();
            }
        }

        public static void doWizardCommands(char command)
        {
            switch (command)
            {
                case CTRL_KEY_A:
                    // Cure all!
                    wizardCureAll();
                    break;
                case CTRL_KEY_E:
                    // Edit Character
                    wizardCharacterAdjustment();
                    messageLineClear();
                    break;
                case CTRL_KEY_F:
                    // Mass Genocide, vanquish all monsters
                    spellMassGenocide();
                    break;
                case CTRL_KEY_G:
                    // Generate random items
                    wizardDropRandomItems();
                    break;
                case CTRL_KEY_D:
                    // Go up/down to specified depth
                    wizardJumpLevel();
                    break;
                case CTRL_KEY_O:
                    // Print random level object to a file
                    outputRandomLevelObjectsToFile();
                    break;
                case '\\':
                    // Display wizard help
                    if (Config.options.use_roguelike_keys)
                    {
                        displayTextHelpFile(Config.files.help_roguelike_wizard);
                    }
                    else
                    {
                        displayTextHelpFile(Config.files.help_wizard);
                    }
                    break;
                case CTRL_KEY_I:
                    // Identify an item
                    spellIdentifyItem();
                    break;
                case '*':
                    // Light up entire dungeon
                    wizardLightUpDungeon();
                    break;
                case ':':
                    // Light up current panel
                    spellMapCurrentArea();
                    break;
                case CTRL_KEY_T:
                    // Random player teleportation
                    playerTeleport(100);
                    break;
                case '%':
                    // Generate a dungeon item!
                    wizardGenerateObject();
                    drawDungeonPanel();
                    break;
                case '+':
                    // Increase Experience
                    wizardGainExperience();
                    break;
                case '&':
                    // Summon a random monster
                    wizardSummonMonster();
                    break;
                case '@':
                    // Generate an object
                    // NOTE: every field from the struct needs to be filled correctly
                    wizardCreateObjects();
                    break;
                default:
                    if (Config.options.use_roguelike_keys)
                    {
                        putStringClearToEOL("Type '?' or '\\' for help.", new Coord_t(0, 0));
                    }
                    else
                    {
                        putStringClearToEOL("Type '?' or ^H for help.", new Coord_t(0, 0));
                    }
                    break;
            }
        }

        // TODO: use only commands here - don't just call the external functions.
        // TODO: E.g. split playerEat() into command/action functions: commandEat(), playerEat().
        // Possibly the "setup" happens in the command, such as the food check/selection of playerEat().
        // The command then calls playerEat() in player_eat.cpp - passing the selected food `item_id`.
        public static void doCommand(char command)
        {
            var game = State.Instance.game;

            bool do_pickup = moveWithoutPickup(ref command);

            switch (command)
            {
                case 'Q': // (Q)uit    (^K)ill
                    commandQuit();
                    game.player_free_turn = true;
                    break;
                case CTRL_KEY_P: // (^P)revious message.
                    commandPreviousMessage();
                    game.player_free_turn = true;
                    break;
                case CTRL_KEY_V: // (^V)iew license
                    displayTextHelpFile(Config.files.license);
                    game.player_free_turn = true;
                    break;
                case CTRL_KEY_W: // (^W)izard mode
                    commandFlipWizardMode();
                    game.player_free_turn = true;
                    break;
                case CTRL_KEY_X: // e(^X)it and save
                    commandSaveAndExit();
                    game.player_free_turn = true;
                    break;
                case '=': // (=) set options
                    terminalSaveScreen();
                    setGameOptions();
                    terminalRestoreScreen();
                    game.player_free_turn = true;
                    break;
                case '{': // ({) inscribe an object
                    itemInscribe();
                    game.player_free_turn = true;
                    break;
                case '!':    // (!) escape to the shell
                case '$':    // escaping to shell disabled -MRC-
                case ESCAPE: // (ESC)   do nothing.
                case ' ':    // (space) do nothing.
                    game.player_free_turn = true;
                    break;
                case 'b': // (b) down, left  (1)
                    playerMove(1, do_pickup);
                    break;
                case 'j': // (j) down    (2)
                    playerMove(2, do_pickup);
                    break;
                case 'n': // (n) down, right  (3)
                    playerMove(3, do_pickup);
                    break;
                case 'h': // (h) left    (4)
                    playerMove(4, do_pickup);
                    break;
                case 'l': // (l) right    (6)
                    playerMove(6, do_pickup);
                    break;
                case 'y': // (y) up, left    (7)
                    playerMove(7, do_pickup);
                    break;
                case 'k': // (k) up    (8)
                    playerMove(8, do_pickup);
                    break;
                case 'u': // (u) up, right  (9)
                    playerMove(9, do_pickup);
                    break;
                case 'B': // (B) run down, left  (. 1)
                    playerFindInitialize(1);
                    break;
                case 'J': // (J) run down    (. 2)
                    playerFindInitialize(2);
                    break;
                case 'N': // (N) run down, right  (. 3)
                    playerFindInitialize(3);
                    break;
                case 'H': // (H) run left    (. 4)
                    playerFindInitialize(4);
                    break;
                case 'L': // (L) run right  (. 6)
                    playerFindInitialize(6);
                    break;
                case 'Y': // (Y) run up, left  (. 7)
                    playerFindInitialize(7);
                    break;
                case 'K': // (K) run up    (. 8)
                    playerFindInitialize(8);
                    break;
                case 'U': // (U) run up, right  (. 9)
                    playerFindInitialize(9);
                    break;
                case '/': // (/) identify a symbol
                    identifyGameObject();
                    game.player_free_turn = true;
                    break;
                case '.': // (.) stay in one place (5)
                    playerMove(5, do_pickup);

                    if (game.command_count > 1)
                    {
                        game.command_count--;
                        playerRestOn();
                    }
                    break;
                case '<': // (<) go down a staircase
                    dungeonGoUpLevel();
                    break;
                case '>': // (>) go up a staircase
                    dungeonGoDownLevel();
                    break;
                case '?': // (?) help with commands
                    if (Config.options.use_roguelike_keys)
                    {
                        displayTextHelpFile(Config.files.help_roguelike);
                    }
                    else
                    {
                        displayTextHelpFile(Config.files.help);
                    }
                    game.player_free_turn = true;
                    break;
                case 'f': // (f)orce    (B)ash
                    playerBash();
                    break;
                case 'C': // (C)haracter description
                    terminalSaveScreen();
                    changeCharacterName();
                    terminalRestoreScreen();
                    game.player_free_turn = true;
                    break;
                case 'D': // (D)isarm trap
                    playerDisarmTrap();
                    break;
                case 'E': // (E)at food
                    playerEat();
                    break;
                case 'F': // (F)ill lamp
                    inventoryRefillLamp();
                    break;
                case 'G': // (G)ain magic spells
                    playerGainSpells();
                    break;
                case 'V': // (V)iew scores
                    terminalSaveScreen();
                    showScoresScreen();
                    terminalRestoreScreen();
                    game.player_free_turn = true;
                    break;
                case 'W': // (W)here are we on the map  (L)ocate on map
                    commandLocateOnMap();
                    game.player_free_turn = true;
                    break;
                case 'R': // (R)est a while
                    playerRestOn();
                    break;
                case '#': // (#) search toggle  (S)earch toggle
                    commandToggleSearch();
                    game.player_free_turn = true;
                    break;
                case CTRL_KEY_B: // (^B) tunnel down left  (T 1)
                    playerTunnel(1);
                    break;
                case CTRL_KEY_M: // cr must be treated same as lf.
                case CTRL_KEY_J: // (^J) tunnel down    (T 2)
                    playerTunnel(2);
                    break;
                case CTRL_KEY_N: // (^N) tunnel down right  (T 3)
                    playerTunnel(3);
                    break;
                case CTRL_KEY_H: // (^H) tunnel left    (T 4)
                    playerTunnel(4);
                    break;
                case CTRL_KEY_L: // (^L) tunnel right    (T 6)
                    playerTunnel(6);
                    break;
                case CTRL_KEY_Y: // (^Y) tunnel up left    (T 7)
                    playerTunnel(7);
                    break;
                case CTRL_KEY_K: // (^K) tunnel up    (T 8)
                    playerTunnel(8);
                    break;
                case CTRL_KEY_U: // (^U) tunnel up right    (T 9)
                    playerTunnel(9);
                    break;
                case 'z': // (z)ap a wand    (a)im a wand
                    wandAim();
                    break;
                case 'M':
                    dungeonDisplayMap();
                    game.player_free_turn = true;
                    break;
                case 'P': // (P)eruse a book  (B)rowse in a book
                    examineBook();
                    game.player_free_turn = true;
                    break;
                case 'c': // (c)lose an object
                    playerCloseDoor();
                    break;
                case 'd': // (d)rop something
                    inventoryExecuteCommand('d');
                    break;
                case 'e': // (e)quipment list
                    inventoryExecuteCommand('e');
                    break;
                case 't': // (t)hrow something  (f)ire something
                    playerThrowItem();
                    break;
                case 'i': // (i)nventory list
                    inventoryExecuteCommand('i');
                    break;
                case 'S': // (S)pike a door  (j)am a door
                    dungeonJamDoor();
                    break;
                case 'x': // e(x)amine surrounds  (l)ook about
                    look();
                    game.player_free_turn = true;
                    break;
                case 'm': // (m)agic spells
                    getAndCastMagicSpell();
                    break;
                case 'o': // (o)pen something
                    playerOpenClosedObject();
                    break;
                case 'p': // (p)ray
                    pray();
                    break;
                case 'q': // (q)uaff
                    quaff();
                    break;
                case 'r': // (r)ead
                    scrollRead();
                    break;
                case 's': // (s)earch for a turn
                    playerSearch(State.Instance.py.pos, State.Instance.py.misc.chance_in_search);
                    break;
                case 'T': // (T)ake off something  (t)ake off
                    inventoryExecuteCommand('t');
                    break;
                case 'Z': // (Z)ap a staff  (u)se a staff
                    staffUse();
                    break;
                case 'v': // (v)ersion of game
                    displayTextHelpFile(Config.files.versions_history);
                    game.player_free_turn = true;
                    break;
                case 'w': // (w)ear or wield
                    inventoryExecuteCommand('w');
                    break;
                case 'X': // e(X)change weapons  e(x)change
                    inventoryExecuteCommand('x');
                    break;
                default:
                    // Wizard commands are free moves
                    game.player_free_turn = true;

                    if (game.wizard_mode)
                    {
                        doWizardCommands(command);
                    }
                    else
                    {
                        putStringClearToEOL("Type '?' for help.", new Coord_t(0, 0));
                    }
                    break;
            }
            game.last_command = command;
        }

        // Check whether this command will accept a count. -CJS-
        public static bool validCountCommand(char command)
        {
            switch (command)
            {
                case 'Q':
                case CTRL_KEY_W:
                case CTRL_KEY_X:
                case '=':
                case '{':
                case '/':
                case '<':
                case '>':
                case '?':
                case 'C':
                case 'E':
                case 'F':
                case 'G':
                case 'V':
                case '#':
                case 'z':
                case 'P':
                case 'c':
                case 'd':
                case 'e':
                case 't':
                case 'i':
                case 'x':
                case 'm':
                case 'p':
                case 'q':
                case 'r':
                case 'T':
                case 'Z':
                case 'v':
                case 'w':
                case 'W':
                case 'X':
                case CTRL_KEY_A:
                case '\\':
                case CTRL_KEY_I:
                case '*':
                case ':':
                case CTRL_KEY_T:
                case CTRL_KEY_E:
                case CTRL_KEY_F:
                case CTRL_KEY_S:
                case CTRL_KEY_Q:
                    return false;
                case CTRL_KEY_P:
                case ESCAPE:
                case ' ':
                case '-':
                case 'b':
                case 'f':
                case 'j':
                case 'n':
                case 'h':
                case 'l':
                case 'y':
                case 'k':
                case 'u':
                case '.':
                case 'B':
                case 'J':
                case 'N':
                case 'H':
                case 'L':
                case 'Y':
                case 'K':
                case 'U':
                case 'D':
                case 'R':
                case CTRL_KEY_Y:
                case CTRL_KEY_K:
                case CTRL_KEY_U:
                case CTRL_KEY_L:
                case CTRL_KEY_N:
                case CTRL_KEY_J:
                case CTRL_KEY_B:
                case CTRL_KEY_H:
                case 'S':
                case 'o':
                case 's':
                case CTRL_KEY_D:
                case CTRL_KEY_G:
                case '+':
                    return true;
                default:
                    return false;
            }
        }

        // Regenerate hit points -RAK-
        public static void playerRegenerateHitPoints(int percent)
        {
            var py = State.Instance.py;

            int old_chp = py.misc.current_hp;
            int new_chp = (int)py.misc.max_hp * percent + (int)Config.player.PLAYER_REGEN_HPBASE;

            // div 65536
            py.misc.current_hp += new_chp >> 16;

            // check for overflow
            if (py.misc.current_hp < 0 && old_chp > 0)
            {
                py.misc.current_hp = SHRT_MAX;
            }

            // mod 65536
            int new_chp_fraction = (new_chp & 0xFFFF) + (int)py.misc.current_hp_fraction;

            if (new_chp_fraction >= 0x10000L)
            {
                py.misc.current_hp_fraction = (uint)(new_chp_fraction - 0x10000L);
                py.misc.current_hp++;
            }
            else
            {
                py.misc.current_hp_fraction = (uint)new_chp_fraction;
            }

            // must set frac to zero even if equal
            if (py.misc.current_hp >= py.misc.max_hp)
            {
                py.misc.current_hp = py.misc.max_hp;
                py.misc.current_hp_fraction = 0;
            }

            if (old_chp != py.misc.current_hp)
            {
                printCharacterCurrentHitPoints();
            }
        }

        // Regenerate mana points -RAK-
        public static void playerRegenerateMana(int percent)
        {
            var py = State.Instance.py;

            int old_cmana = py.misc.current_mana;
            int new_mana = (int)py.misc.mana * percent + (int)Config.player.PLAYER_REGEN_MNBASE;

            // div 65536
            py.misc.current_mana += new_mana >> 16;

            // check for overflow
            if (py.misc.current_mana < 0 && old_cmana > 0)
            {
                py.misc.current_mana = SHRT_MAX;
            }

            // mod 65536
            int new_mana_fraction = (new_mana & 0xFFFF) + (int)py.misc.current_mana_fraction;

            if (new_mana_fraction >= 0x10000L)
            {
                py.misc.current_mana_fraction = (uint)(new_mana_fraction - 0x10000L);
                py.misc.current_mana++;
            }
            else
            {
                py.misc.current_mana_fraction = (uint)new_mana_fraction;
            }

            // must set frac to zero even if equal
            if (py.misc.current_mana >= py.misc.mana)
            {
                py.misc.current_mana = py.misc.mana;
                py.misc.current_mana_fraction = 0;
            }

            if (old_cmana != py.misc.current_mana)
            {
                printCharacterCurrentMana();
            }
        }

        // Is an item an enchanted weapon or armor and we don't know? -CJS-
        // only returns true if it is a good enchantment
        public static bool itemEnchanted(Inventory_t item)
        {
            if (item.category_id < TV_MIN_ENCHANT || item.category_id > TV_MAX_ENCHANT || (item.flags & Config.treasure_flags.TR_CURSED) != 0u)
            {
                return false;
            }

            if (spellItemIdentified(item))
            {
                return false;
            }

            if ((item.identification & Config.identification.ID_MAGIK) != 0)
            {
                return false;
            }

            if (item.to_hit > 0 || item.to_damage > 0 || item.to_ac > 0)
            {
                return true;
            }

            if ((0x4000107fL & item.flags) != 0 && item.misc_use > 0)
            {
                return true;
            }

            return (0x07ffe980L & item.flags) != 0;
        }

        // Examine a Book -RAK-
        public static void examineBook()
        {
            var py = State.Instance.py;

            int item_pos_start = 0, item_pos_end = 0;
            if (!inventoryFindRange((int)TV_MAGIC_BOOK, (int)TV_PRAYER_BOOK, ref item_pos_start, ref item_pos_end))
            {
                printMessage("You are not carrying any books.");
                return;
            }

            if (py.flags.blind > 0)
            {
                printMessage("You can't see to read your spell book!");
                return;
            }

            if (playerNoLight())
            {
                printMessage("You have no light to read by.");
                return;
            }

            if (py.flags.confused > 0)
            {
                printMessage("You are too confused.");
                return;
            }

            int item_id = 0;
            if (inventoryGetInputForItemId(ref item_id, "Which Book?", item_pos_start, item_pos_end, /*CNIL*/ null, /*CNIL*/ null))
            {
                var spell_index = new int[31];
                bool can_read = true;

                uint treasure_type = py.inventory[item_id].category_id;

                if (State.Instance.classes[py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
                {
                    if (treasure_type != TV_MAGIC_BOOK)
                    {
                        can_read = false;
                    }
                }
                else if (State.Instance.classes[py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_PRIEST)
                {
                    if (treasure_type != TV_PRAYER_BOOK)
                    {
                        can_read = false;
                    }
                }
                else
                {
                    can_read = false;
                }

                if (!can_read)
                {
                    printMessage("You do not understand the language.");
                    return;
                }

                uint item_flags = py.inventory[item_id].flags;

                int spell_id = 0;
                while (item_flags != 0u)
                {
                    item_pos_end = getAndClearFirstBit(ref item_flags);

                    if (State.Instance.magic_spells[py.misc.class_id - 1][item_pos_end].level_required < 99)
                    {
                        spell_index[spell_id] = item_pos_end;
                        spell_id++;
                    }
                }

                terminalSaveScreen();
                displaySpellsList(spell_index, spell_id, true, -1);
                waitForContinueKey(0);
                terminalRestoreScreen();
            }
        }

        // Go up one level -RAK-
        public static void dungeonGoUpLevel()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            uint tile_id = dg.floor[py.pos.y][py.pos.x].treasure_id;

            if (tile_id != 0 && game.treasure.list[tile_id].category_id == TV_UP_STAIR)
            {
                dg.current_level--;

                printMessage("You enter a maze of up staircases.");
                printMessage("You pass through a one-way door.");

                dg.generate_new_level = true;
            }
            else
            {
                printMessage("I see no up staircase here.");
                game.player_free_turn = true;
            }
        }

        // Go down one level -RAK-
        public static void dungeonGoDownLevel()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            uint tile_id = dg.floor[py.pos.y][py.pos.x].treasure_id;

            if (tile_id != 0 && game.treasure.list[tile_id].category_id == TV_DOWN_STAIR)
            {
                dg.current_level++;

                printMessage("You enter a maze of down staircases.");
                printMessage("You pass through a one-way door.");

                dg.generate_new_level = true;
            }
            else
            {
                printMessage("I see no down staircase here.");
                game.player_free_turn = true;
            }
        }

        // Jam a closed door -RAK-
        public static void dungeonJamDoor()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            game.player_free_turn = true;

            Coord_t coord = py.pos;

            int direction = 0;
            if (!getDirectionWithMemory(/*CNIL*/ null, ref direction))
            {
                return;
            }
            playerMovePosition(direction, coord);

            Tile_t tile = dg.floor[coord.y][coord.x];

            if (tile.treasure_id == 0)
            {
                printMessage("That isn't a door!");
                return;
            }

            Inventory_t item = game.treasure.list[tile.treasure_id];

            uint item_id = item.category_id;
            if (item_id != TV_CLOSED_DOOR && item_id != TV_OPEN_DOOR)
            {
                printMessage("That isn't a door!");
                return;
            }

            if (item_id == TV_OPEN_DOOR)
            {
                printMessage("The door must be closed first.");
                return;
            }

            // If we reach here, the door is closed and we can try to jam it -MRC-

            if (tile.creature_id == 0)
            {
                int item_pos_start = 0, item_pos_end = 0;
                if (inventoryFindRange((int)TV_SPIKE, TV_NEVER, ref item_pos_start, ref item_pos_end))
                {
                    game.player_free_turn = false;

                    printMessageNoCommandInterrupt("You jam the door with a spike.");

                    if (item.misc_use > 0)
                    {
                        // Make locked to stuck.
                        item.misc_use = -item.misc_use;
                    }

                    // Successive spikes have a progressively smaller effect.
                    // Series is: 0 20 30 37 43 48 52 56 60 64 67 70 ...
                    item.misc_use -= 1 + 190 / (10 - item.misc_use);

                    if (py.inventory[item_pos_start].items_count > 1)
                    {
                        py.inventory[item_pos_start].items_count--;
                        py.pack.weight -= (int)py.inventory[item_pos_start].weight;
                    }
                    else
                    {
                        inventoryDestroyItem(item_pos_start);
                    }
                }
                else
                {
                    printMessage("But you have no spikes.");
                }
            }
            else
            {
                game.player_free_turn = false;

                var msg = $"The {State.Instance.creatures_list[State.Instance.monsters[tile.creature_id].creature_id].name} is in your way!";
                //vtype_t msg = { '\0' };
                //(void)sprintf(msg, "The %s is in your way!", State.Instance.creatures_list[State.Instance.monsters[tile.creature_id].creature_id].name);
                printMessage(msg);
            }
        }

        // Refill the players lamp -RAK-
        public static void inventoryRefillLamp()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            game.player_free_turn = true;

            if (py.inventory[(int)PlayerEquipment.Light].sub_category_id != 0)
            {
                printMessage("But you are not using a lamp.");
                return;
            }

            int item_pos_start = 0, item_pos_end = 0;
            if (!inventoryFindRange((int)TV_FLASK, TV_NEVER, ref item_pos_start, ref item_pos_end))
            {
                printMessage("You have no oil.");
                return;
            }

            game.player_free_turn = false;

            Inventory_t item = py.inventory[(int)PlayerEquipment.Light];
            item.misc_use += py.inventory[item_pos_start].misc_use;

            if (item.misc_use > Config.treasure.OBJECT_LAMP_MAX_CAPACITY)
            {
                item.misc_use = (int)Config.treasure.OBJECT_LAMP_MAX_CAPACITY;
                printMessage("Your lamp overflows, spilling oil on the ground.");
                printMessage("Your lamp is full.");
            }
            else if (item.misc_use > Config.treasure.OBJECT_LAMP_MAX_CAPACITY / 2)
            {
                printMessage("Your lamp is more than half full.");
            }
            else if (item.misc_use == Config.treasure.OBJECT_LAMP_MAX_CAPACITY / 2)
            {
                printMessage("Your lamp is half full.");
            }
            else
            {
                printMessage("Your lamp is less than half full.");
            }

            itemTypeRemainingCountDescription(item_pos_start);
            inventoryDestroyItem(item_pos_start);
        }

        // Main procedure for dungeon. -RAK-
        public static void playDungeon()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var game = State.Instance.game;

            // Note: There is a lot of preliminary magic going on here at first
            playerInitializePlayerLight();
            playerUpdateMaxDungeonDepth();
            resetDungeonFlags();

            // Initialize find counter to `0`
            int find_count = 0;

            // Ensure we display the panel. Used to do this with a global var. -CJS-
            dg.panel.row = dg.panel.col = -1;

            // Light up the area around character
            dungeonResetView();

            // must do this after `dg.panel.row` / `dg.panel.col` set to -1, because playerSearchOff() will
            // call dungeonResetView(), and so the panel_* variables must be valid before
            // playerSearchOff() is called
            if ((py.flags.status & Config.player_status.PY_SEARCH) != 0u)
            {
                playerSearchOff();
            }

            // Light,  but do not move critters
            updateMonsters(false);

            // Print the depth
            printCharacterCurrentDepth();

            // Note: yes, this last input command needs to be persisted
            // over different iterations of the main loop below -MRC-
            char last_input_command = '\0';

            // Loop until dead,  or new level
            // Exit when `dg.generate_new_level` and `eof_flag` are both set
            do
            {
                // Increment turn counter
                dg.game_turn++;

                // turn over the store contents every, say, 1000 turns
                if (dg.current_level != 0 && dg.game_turn % 1000 == 0)
                {
                    storeMaintenance();
                }

                // Check for creature generation
                if (randomNumber(Config.monsters.MON_CHANCE_OF_NEW) == 1)
                {
                    monsterPlaceNewWithinDistance(1, (int)Config.monsters.MON_MAX_SIGHT, false);
                }

                playerUpdateLightStatus();

                //
                // Update counters and messages
                //

                // Heroism and Super Heroism must precede anything that can damage player
                playerUpdateHeroStatus();

                int regen_amount = playerFoodConsumption();
                playerUpdateRegeneration(regen_amount);

                playerUpdateBlindness();
                playerUpdateConfusion();
                playerUpdateFearState();
                playerUpdatePoisonedState();
                playerUpdateSpeed();
                playerUpdateRestingState();

                // Check for interrupts to find or rest.
                int microseconds = (py.running_tracker != 0 ? 0 : 10000);
                if ((game.command_count > 0 || (py.running_tracker != 0) || py.flags.rest != 0) && checkForNonBlockingKeyPress(microseconds))
                {
                    playerDisturb(0, 0);
                }

                playerUpdateHallucination();
                playerUpdateParalysis();
                playerUpdateEvilProtection();
                playerUpdateInvulnerability();
                playerUpdateBlessedness();
                playerUpdateHeatResistance();
                playerUpdateColdResistance();
                playerUpdateDetectInvisible();
                playerUpdateInfraVision();
                playerUpdateWordOfRecall();

                // Random teleportation
                if (py.flags.teleport && randomNumber(100) == 1)
                {
                    playerDisturb(0, 0);
                    playerTeleport(40);
                }

                // See if we are too weak to handle the weapon or pack. -CJS-
                if ((py.flags.status & Config.player_status.PY_STR_WGT) != 0u)
                {
                    playerStrength();
                }

                if ((py.flags.status & Config.player_status.PY_STUDY) != 0u)
                {
                    printCharacterStudyInstruction();
                }

                playerUpdateStatusFlags();

                // Allow for a slim chance of detect enchantment -CJS-
                // for 1st level char, check once every 2160 turns
                // for 40th level char, check once every 416 turns
                int chance = 10 + 750 / (5 + (int)py.misc.level);
                if ((dg.game_turn & 0xF) == 0 && py.flags.confused == 0 && randomNumber(chance) == 1)
                {
                    playerDetectEnchantment();
                }

                // Check the state of the monster list, and delete some monsters if
                // the monster list is nearly full.  This helps to avoid problems in
                // creature.c when monsters try to multiply.  Compact_monsters() is
                // much more likely to succeed if called from here, than if called
                // from within updateMonsters().
                if (MON_TOTAL_ALLOCATIONS - State.Instance.next_free_monster_id < 10)
                {
                    compactMonsters();
                }

                // Accept a command?
                if (py.flags.paralysis < 1 && py.flags.rest == 0 && !game.character_is_dead)
                {
                    executeInputCommands(ref last_input_command, ref find_count);
                }
                else
                {
                    // if paralyzed, resting, or dead, flush output
                    // but first move the cursor onto the player, for aesthetics
                    panelMoveCursor(py.pos);
                    putQIO();
                }

                // Teleport?
                if (game.teleport_player)
                {
                    playerTeleport(100);
                }

                // Move the creatures
                if (!dg.generate_new_level)
                {
                    updateMonsters(true);
                }
            } while (!dg.generate_new_level && (eof_flag == 0));
        }

    }
}
