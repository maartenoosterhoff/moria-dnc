﻿using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using System.Linq;
using Moria.Core.Resources;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Treasure_m;
using static Moria.Core.Methods.Ui_io_m;

namespace Moria.Core.Methods
{
    public static class Game_files_m
    {
        ////  initializeScoreFile
        ////  Open the score file while we still have the setuid privileges.  Later
        ////  when the score is being written out, you must be sure to flock the file
        ////  so we don't have multiple people trying to write to it at the same time.
        ////  Craig Norborg (doc)    Mon Aug 10 16:41:59 EST 1987
        //public static bool initializeScoreFile()
        //{

        //    highscore_fp = fopen(Config.files.scores.c_str(), (char*)"rb+");

        //    return highscore_fp != nullptr;
        //}

        // Attempt to open and print the file containing the intro splash screen text -RAK-
        public static void displaySplashScreen()
        {
            clearScreen();
            var lines = DataFilesResource.splash.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.None);
            int i = 0;
            foreach (var line in lines)
            {
                putString(line, new Coord_t(i, 0));
                i++;
            }

            waitForContinueKey(23);

            /*
            vtype_t in_line = { '\0' };

            FILE* screen_file = fopen(Config.files.splash_screen.c_str(), "r");
            if (screen_file != nullptr)
            {
                clearScreen();
                for (int i = 0; fgets(in_line, 80, screen_file) != CNIL; i++)
                {
                    putString(in_line, new Coord_t(i, 0));
                }
                waitForContinueKey(23);

                (void)fclose(screen_file);
            }
            */
        }

        // Open and display a text help file
        // File perusal, primitive, but portable -CJS-
        public static void displayTextHelpFile(string helpText)
        {
            //FILE* file = fopen(filename.c_str(), "r");
            //if (file == nullptr)
            //{
            //    putStringClearToEOL("Can not find help file '" + filename + "'.", new Coord_t(0, 0));
            //    return;
            //}

            terminalSaveScreen();

            var lines = helpText.Split(new[] {Environment.NewLine, "\r", "\n"}, StringSplitOptions.None)
                .ToList();

            while (lines.Any())
            {
                clearScreen();
                for (var i = 0; i < 23 && lines.Count > 0; i++)
                {
                    var line = lines[0];
                    lines.RemoveAt(0);
                    putString(line, new Coord_t(i, 0));
                }


                putStringClearToEOL("[ press any key to continue ]", new Coord_t(23, 23));
                var input = getKeyInput();
                if (input == ESCAPE)
                {
                    break;
                }
            }

            /*
                        constexpr uint8_t max_line_length = 80;
                        char line_buffer[max_line_length];
                        char input;

                        while (feof(file) == 0)
                        {
                            clearScreen();

                            for (int i = 0; i < 23; i++)
                            {
                                if (fgets(line_buffer, max_line_length - 1, file) != CNIL)
                                {
                                    putString(line_buffer, new Coord_t(i, 0));
                                }
                            }

                            putStringClearToEOL("[ press any key to continue ]", new Coord_t(23, 23));
                            input = getKeyInput();
                            if (input == ESCAPE)
                            {
                                break;
                            }
                        }

                        (void)fclose(file);

                        */

            terminalRestoreScreen();
        }

        // Open and display a "death" text file
        public static void displayDeathFile(string resourceName)
        {
            var dataFile = string.Empty;
            if (resourceName == nameof(DataFilesResource.death_tomb))
            {
                dataFile = DataFilesResource.death_tomb;
            }
            else if (resourceName == nameof(DataFilesResource.death_royal))
            {
                dataFile = DataFilesResource.death_royal;
            }

            var lines = dataFile
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .ToArray();

            for (int i = 0; i < 23 && i < lines.Length; i++)
            {
                putString(lines[i], new Coord_t(i, 0));
            }



            /*

            FILE* file = fopen(filename.c_str(), "r");
            if (file == nullptr)
            {
                putStringClearToEOL("Can not find help file '" + filename + "'.", new Coord_t(0, 0));
                return;
            }

            clearScreen();

            constexpr uint8_t max_line_length = 80;
            char line_buffer[max_line_length];

            for (int i = 0; i < 23 && feof(file) == 0; i++)
            {
                if (fgets(line_buffer, max_line_length - 1, file) != CNIL)
                {
                    putString(line_buffer, new Coord_t(i, 0));
                }
            }
            (void)fclose(file);

            */
        }

        // Prints a list of random objects to a file. -RAK-
        // Note that the objects produced is a sampling of objects
        // which be expected to appear on that level.
        public static void outputRandomLevelObjectsToFile()
        {
            var input = string.Empty;
            //obj_desc_t input = { 0 };

            putStringClearToEOL("Produce objects on what level?: ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 32), 10))
            {
                return;
            }

            int level;
            if (!stringToNumber(input, out level))
            {
                return;
            }

            putStringClearToEOL("Produce how many objects?: ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 27), 10))
            {
                return;
            }

            int count;
            if (!stringToNumber(input, out count))
            {
                return;
            }

            if (count < 1 || level < 0 || level > 1200)
            {
                putStringClearToEOL("Parameters no good.", new Coord_t(0, 0));
                return;
            }

            if (count > 10000)
            {
                count = 10000;
            }

            bool small_objects = getInputConfirmation("Small objects only?");

            putStringClearToEOL("File name: ", new Coord_t(0, 0));

            var filename = string.Empty;
            //vtype_t filename = { 0 };

            if (!getStringInput(out filename, new Coord_t(0, 11), 64))
            {
                return;
            }
            //if (strlen(filename) == 0)
            //{
            //    return;
            //}

            //FILE* file_ptr = fopen(filename, "w");
            //if (file_ptr == nullptr)
            //{
            //    putStringClearToEOL("File could not be opened.", new Coord_t(0, 0));
            //    return;
            //}

            input = $"{count:d}";
            //(void)sprintf(input, "%d", count);
            putStringClearToEOL(input +  " random objects being produced...", new Coord_t(0, 0));

            putQIO();

            //(void)fprintf(file_ptr, "*** Random Object Sampling:\n");
            //(void)fprintf(file_ptr, "*** %d objects\n", count);
            //(void)fprintf(file_ptr, "*** For Level %d\n", level);
            //(void)fprintf(file_ptr, "\n");
            //(void)fprintf(file_ptr, "\n");

            int treasure_id = popt();
            var game = State.Instance.game;

            for (int i = 0; i < count; i++)
            {
                int object_id = itemGetRandomObjectId(level, small_objects);
                inventoryItemCopyTo(State.Instance.sorted_objects[object_id], game.treasure.list[treasure_id]);

                magicTreasureMagicalAbility(treasure_id, level);

                Inventory_t item = game.treasure.list[treasure_id];
                itemIdentifyAsStoreBought(item);

                if ((item.flags & Config.treasure_flags.TR_CURSED) != 0u)
                {
                    itemAppendToInscription(item, Config.identification.ID_DAMD);
                }

                itemDescription(ref input, item, true);
                //(void)fprintf(file_ptr, "%d %s\n", item.depth_first_found, input);
            }

            pusht((uint)treasure_id);

            //(void)fclose(file_ptr);

            putStringClearToEOL("Completed.", new Coord_t(0, 0));
        }

        //// Write character sheet to the file
        //public static void writeCharacterSheetToFile(FILE* char_file)
        //{
        //    putStringClearToEOL("Writing character sheet...", new Coord_t(0, 0));
        //    putQIO();

        //    const char* colon = ":";
        //    const char* blank = " ";

        //    vtype_t stat_description = { '\0' };

        //    (void)fprintf(char_file, "%c\n\n", CTRL_KEY_L);

        //    (void)fprintf(char_file, " Name%9s %-23s", colon, py.misc.name);
        //    (void)fprintf(char_file, " Age%11s %6d", colon, (int)py.misc.age);
        //    statsAsString(py.stats.used[PlayerAttr::STR], stat_description);
        //    (void)fprintf(char_file, "   STR : %s\n", stat_description);
        //    (void)fprintf(char_file, " Race%9s %-23s", colon, character_races[py.misc.race_id].name);
        //    (void)fprintf(char_file, " Height%8s %6d", colon, (int)py.misc.height);
        //    statsAsString(py.stats.used[PlayerAttr::INT], stat_description);
        //    (void)fprintf(char_file, "   INT : %s\n", stat_description);
        //    (void)fprintf(char_file, " Sex%10s %-23s", colon, (playerGetGenderLabel()));
        //    (void)fprintf(char_file, " Weight%8s %6d", colon, (int)py.misc.weight);
        //    statsAsString(py.stats.used[PlayerAttr::WIS], stat_description);
        //    (void)fprintf(char_file, "   WIS : %s\n", stat_description);
        //    (void)fprintf(char_file, " Class%8s %-23s", colon, classes[py.misc.class_id].title);
        //    (void)fprintf(char_file, " Social Class : %6d", py.misc.social_class);
        //    statsAsString(py.stats.used[PlayerAttr::DEX], stat_description);
        //    (void)fprintf(char_file, "   DEX : %s\n", stat_description);
        //    (void)fprintf(char_file, " Title%8s %-23s", colon, playerRankTitle());
        //    (void)fprintf(char_file, "%22s", blank);
        //    statsAsString(py.stats.used[PlayerAttr::CON], stat_description);
        //    (void)fprintf(char_file, "   CON : %s\n", stat_description);
        //    (void)fprintf(char_file, "%34s", blank);
        //    (void)fprintf(char_file, "%26s", blank);
        //    statsAsString(py.stats.used[PlayerAttr::CHR], stat_description);
        //    (void)fprintf(char_file, "   CHR : %s\n\n", stat_description);

        //    (void)fprintf(char_file, " + To Hit    : %6d", py.misc.display_to_hit);
        //    (void)fprintf(char_file, "%7sLevel      : %7d", blank, (int)py.misc.level);
        //    (void)fprintf(char_file, "    Max Hit Points : %6d\n", py.misc.max_hp);
        //    (void)fprintf(char_file, " + To Damage : %6d", py.misc.display_to_damage);
        //    (void)fprintf(char_file, "%7sExperience : %7d", blank, py.misc.exp);
        //    (void)fprintf(char_file, "    Cur Hit Points : %6d\n", py.misc.current_hp);
        //    (void)fprintf(char_file, " + To AC     : %6d", py.misc.display_to_ac);
        //    (void)fprintf(char_file, "%7sMax Exp    : %7d", blank, py.misc.max_exp);
        //    (void)fprintf(char_file, "    Max Mana%8s %6d\n", colon, py.misc.mana);
        //    (void)fprintf(char_file, "   Total AC  : %6d", py.misc.display_ac);
        //    if (py.misc.level >= PLAYER_MAX_LEVEL)
        //    {
        //        (void)fprintf(char_file, "%7sExp to Adv : *******", blank);
        //    }
        //    else
        //    {
        //        (void)fprintf(char_file, "%7sExp to Adv : %7d", blank, (int32_t)(py.base_exp_levels[py.misc.level - 1] * py.misc.experience_factor / 100));
        //    }
        //    (void)fprintf(char_file, "    Cur Mana%8s %6d\n", colon, py.misc.current_mana);
        //    (void)fprintf(char_file, "%28sGold%8s %7d\n\n", blank, colon, py.misc.au);

        //    int xbth = py.misc.bth + py.misc.plusses_to_hit * BTH_PER_PLUS_TO_HIT_ADJUST + //
        //               (class_level_adj[py.misc.class_id][PlayerClassLevelAdj::BTH] * py.misc.level);
        //    int xbthb = py.misc.bth_with_bows + py.misc.plusses_to_hit * BTH_PER_PLUS_TO_HIT_ADJUST + //
        //                (class_level_adj[py.misc.class_id][PlayerClassLevelAdj::BTHB] * py.misc.level);

        //    // this results in a range from 0 to 29
        //    int xfos = 40 - py.misc.fos;
        //    if (xfos < 0)
        //    {
        //        xfos = 0;
        //    }
        //    int xsrh = py.misc.chance_in_search;

        //    // this results in a range from 0 to 9
        //    int xstl = py.misc.stealth_factor + 1;
        //    int xdis = py.misc.disarm + 2 * playerDisarmAdjustment() + playerStatAdjustmentWisdomIntelligence(PlayerAttr::INT) + //
        //               (class_level_adj[py.misc.class_id][PlayerClassLevelAdj::DISARM] * py.misc.level / 3);
        //    int xsave = py.misc.saving_throw + playerStatAdjustmentWisdomIntelligence(PlayerAttr::WIS) + //
        //                (class_level_adj[py.misc.class_id][PlayerClassLevelAdj::SAVE] * py.misc.level / 3);
        //    int xdev = py.misc.saving_throw + playerStatAdjustmentWisdomIntelligence(PlayerAttr::INT) + //
        //               (class_level_adj[py.misc.class_id][PlayerClassLevelAdj::DEVICE] * py.misc.level / 3);

        //    vtype_t xinfra = { '\0' };
        //    (void)sprintf(xinfra, "%d feet", py.flags.see_infra * 10);

        //    (void)fprintf(char_file, "(Miscellaneous Abilities)\n\n");
        //    (void)fprintf(char_file, " Fighting    : %-10s", statRating(new Coord_t(12, xbth)));
        //    (void)fprintf(char_file, "   Stealth     : %-10s", statRating(new Coord_t(1, xstl)));
        //    (void)fprintf(char_file, "   Perception  : %s\n", statRating(new Coord_t(3, xfos)));
        //    (void)fprintf(char_file, " Bows/Throw  : %-10s", statRating(new Coord_t(12, xbthb)));
        //    (void)fprintf(char_file, "   Disarming   : %-10s", statRating(new Coord_t(8, xdis)));
        //    (void)fprintf(char_file, "   Searching   : %s\n", statRating(new Coord_t(6, xsrh)));
        //    (void)fprintf(char_file, " Saving Throw: %-10s", statRating(new Coord_t(6, xsave)));
        //    (void)fprintf(char_file, "   Magic Device: %-10s", statRating(new Coord_t(6, xdev)));
        //    (void)fprintf(char_file, "   Infra-Vision: %s\n\n", xinfra);

        //    // Write out the character's history
        //    (void)fprintf(char_file, "Character Background\n");
        //    for (auto & entry : py.misc.history)
        //    {
        //        (void)fprintf(char_file, " %s\n", entry);
        //    }
        //}

        public static string equipmentPlacementDescription(int item_id)
        {
            switch ((PlayerEquipment)item_id)
            {
                case PlayerEquipment.Wield:
                    return "You are wielding";
                case PlayerEquipment.Head:
                    return "Worn on head";
                case PlayerEquipment.Neck:
                    return "Worn around neck";
                case PlayerEquipment.Body:
                    return "Worn on body";
                case PlayerEquipment.Arm:
                    return "Worn on shield arm";
                case PlayerEquipment.Hands:
                    return "Worn on hands";
                case PlayerEquipment.Right:
                    return "Right ring finger";
                case PlayerEquipment.Left:
                    return "Left  ring finger";
                case PlayerEquipment.Feet:
                    return "Worn on feet";
                case PlayerEquipment.Outer:
                    return "Worn about body";
                case PlayerEquipment.Light:
                    return "Light source is";
                case PlayerEquipment.Auxiliary:
                    return "Secondary weapon";
                default:
                    return "*Unknown value*";
            }
        }

        //// Write out the equipment list.
        //public static void writeEquipmentListToFile(/*FILE**/string equip_file)
        //{
        //    (void)fprintf(equip_file, "\n  [Character's Equipment List]\n\n");

        //    if (py.equipment_count == 0)
        //    {
        //        (void)fprintf(equip_file, "  Character has no equipment in use.\n");
        //        return;
        //    }

        //    obj_desc_t description = { '\0' };
        //    int item_slot_id = 0;

        //    for (int i = PlayerEquipment.Wield; i < PLAYER_INVENTORY_SIZE; i++)
        //    {
        //        if (py.inventory[i].category_id == TV_NOTHING)
        //        {
        //            continue;
        //        }

        //        itemDescription(description, py.inventory[i], true);
        //        (void)fprintf(equip_file, "  %c) %-19s: %s\n", item_slot_id + 'a', equipmentPlacementDescription(i), description);

        //        item_slot_id++;
        //    }

        //    (void)fprintf(equip_file, "%c\n\n", CTRL_KEY_L);
        //}

        //// Write out the character's inventory.
        //public static void writeInventoryToFile(/*FILE* */string inv_file)
        //{
        //    (void)fprintf(inv_file, "  [General Inventory List]\n\n");

        //    if (py.pack.unique_items == 0)
        //    {
        //        (void)fprintf(inv_file, "  Character has no objects in inventory.\n");
        //        return;
        //    }

        //    obj_desc_t description = { '\0' };

        //    for (int i = 0; i < py.pack.unique_items; i++)
        //    {
        //        itemDescription(description, py.inventory[i], true);
        //        (void)fprintf(inv_file, "%c) %s\n", i + 'a', description);
        //    }

        //    (void)fprintf(inv_file, "%c", CTRL_KEY_L);
        //}

        //// Print the character to a file or device -RAK-
        public static bool outputPlayerCharacterToFile(string filename)
        {
        //    int fd = open(filename, O_WRONLY | O_CREAT | O_EXCL, 0644);
        //    if (fd < 0 && errno == EEXIST)
        //    {
        //        if (getInputConfirmation("Replace existing file " + std::string(filename) + "?"))
        //        {
        //            fd = open(filename, O_WRONLY, 0644);
        //        }
        //    }

        //    FILE* file;
        //    if (fd >= 0)
        //    {
        //        // on some non-unix machines, fdopen() is not reliable,
        //        // hence must call close() and then fopen().
        //        (void)close(fd);
        //        file = fopen(filename, "w");
        //    }
        //    else
        //    {
        //        file = nullptr;
        //    }

        //    if (file == nullptr)
        //    {
        //        if (fd >= 0)
        //        {
        //            (void)close(fd);
        //        }
        //        vtype_t msg = { '\0' };
        //        (void)sprintf(msg, "Can't open file %s:", filename);
        //        printMessage(msg);
        //        return false;
        //    }

        //    writeCharacterSheetToFile(file);
        //    writeEquipmentListToFile(file);
        //    writeInventoryToFile(file);

        //    (void)fclose(file);

        //    putStringClearToEOL("Completed.", new Coord_t(0, 0));

            return true;
        }
    }
}
