using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using System.Resources;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Store_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dice_m;
using static Moria.Core.Methods.Dungeon_los_m;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Game_files_m;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Mage_spells_m;
using static Moria.Core.Methods.Player_magic_m;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Store_inventory_m;
using static Moria.Core.Methods.Scores_m;
using static Moria.Core.Methods.Std_m;
using static Moria.Core.Methods.Player_run_m;
using static Moria.Core.Methods.Player_eat_m;
using static Moria.Core.Methods.Player_traps_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_inventory_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_stats_m;

namespace Moria.Core.Methods
{
    public static class Game_death_m
    {
        // Prints the gravestone of the character -RAK-
        static void printTomb()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;

            displayDeathFile(Config.files.death_tomb);

            string text;

            text = py.misc.name;
            putString(text, new Coord_t(6, (int)(26 - text.Length / 2)));

            if (!game.total_winner)
            {
                text = playerRankTitle();
            }
            else
            {
                text = "Magnificent";
            }
            putString(text, new Coord_t(8, (int)(26 - text.Length / 2)));

            if (!game.total_winner)
            {
                text = State.Instance.classes[py.misc.class_id].title;
            }
            else if (playerIsMale())
            {
                text = "*King*";
            }
            else
            {
                text = "*Queen*";
            }
            putString(text, new Coord_t(10, (int)(26 - text.Length / 2)));

            text = py.misc.level.ToString();
            putString(text, new Coord_t(11, 30));

            text = py.misc.exp.ToString() + " Exp";
            putString(text, new Coord_t(12, (int)(26 - text.Length / 2)));

            text = py.misc.au.ToString() + " Au";
            putString(text, new Coord_t(13, (int)(26 - text.Length / 2)));

            text = State.Instance.dg.current_level.ToString();
            putString(text, new Coord_t(14, 34));

            text = game.character_died_from;
            putString(text, new Coord_t(16, (int)(26 - text.Length / 2)));

            string day = string.Empty;
            humanDateString(ref day);
            text = day;
            putString(text, new Coord_t(17, (int)(26 - text.Length / 2)));

        retry:
            flushInputBuffer();

            putString("(ESC to abort, return to print on screen, or file name)", new Coord_t(23, 0));
            putString("Character record?", new Coord_t(22, 0));

            var str = string.Empty;
            //vtype_t str = { '\0' };
            if (getStringInput(ref str, new Coord_t(22, 18), 60))
            {
                foreach (var item in State.Instance.py.inventory)
                {
                    itemSetAsIdentified((int)item.category_id, (int)item.sub_category_id);
                    spellItemIdentifyAndRemoveRandomInscription(item);
                }

                playerRecalculateBonuses();

                if (str[0] != 0)
                {
                    if (!outputPlayerCharacterToFile(str))
                    {
                        goto retry;
                    }
                }
                else
                {
                    clearScreen();
                    printCharacter();
                    putString("Type ESC to skip the inventory:", new Coord_t(23, 0));
                    if (getKeyInput() != ESCAPE)
                    {
                        clearScreen();
                        printMessage("You are using:");
                        displayEquipment(true, 0);
                        printMessage(/*CNIL*/null);
                        printMessage("You are carrying:");
                        clearToBottom(1);
                        displayInventory(0, py.pack.unique_items - 1, true, 0, /*CNIL*/null);
                        printMessage(/*CNIL*/null);
                    }
                }
            }
        }

        // Let the player know they did good.
        public static void printCrown()
        {
            displayDeathFile(Config.files.death_royal);
            if (playerIsMale())
            {
                putString("King!", new Coord_t(17, 45));
            }
            else
            {
                putString("Queen!", new Coord_t(17, 45));
            }
            flushInputBuffer();
            waitForContinueKey(23);
        }

        // Change the player into a King! -RAK-
        static void kingly()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            // Change the character attributes.
            dg.current_level = 0;
            State.Instance.game.character_died_from = "Ripe Old Age";

            spellRestorePlayerLevels();

            py.misc.level += PLAYER_MAX_LEVEL;
            py.misc.au += 250000;
            py.misc.max_exp += 5000000;
            py.misc.exp = py.misc.max_exp;

            printCrown();
        }

        // What happens upon dying -RAK-
        // Handles the gravestone and top-twenty routines -RAK-
        public static void endGame()
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            printMessage(/*CNIL*/null);

            // flush all input
            flushInputBuffer();

            // If the game has been saved, then save sets turn back to -1,
            // which inhibits the printing of the tomb.
            if (dg.game_turn >= 0)
            {
                if (game.total_winner)
                {
                    kingly();
                }
                printTomb();
            }

            // Save the memory at least.
            if (game.character_generated && !game.character_saved)
            {
                saveGame();
            }

            // add score to score file if applicable
            if (game.character_generated)
            {
                // Clear `game.character_saved`, strange thing to do, but it prevents
                // getKeyInput() from recursively calling endGame() when there has
                // been an eof on stdin detected.
                game.character_saved = false;
                recordNewHighScore();
                showScoresScreen();
            }
            eraseLine(new Coord_t(23, 0));

            exitProgram();
        }

    }
}
