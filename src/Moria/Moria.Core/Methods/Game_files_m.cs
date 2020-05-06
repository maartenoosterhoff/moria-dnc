using System;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.Resources;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods
{
    public interface IGameFiles
    {
        bool outputPlayerCharacterToFile(string filename);

        void displayTextHelpFile(string helpText);

        void displaySplashScreen();

        void displayDeathFile(string resourceName);
    }

    public class Game_files_m : IGameFiles
    {
        private readonly IFileSystem fileSystem;
        private readonly IHelpers helpers;
        private readonly IIdentification identification;
        private readonly ITerminal terminal;

        public Game_files_m(
            IFileSystem fileSystem,
            IHelpers helpers,
            IIdentification identification,
            ITerminal terminal
        )
        {
            this.fileSystem = fileSystem;
            this.helpers = helpers;
            this.identification = identification;
            this.terminal = terminal;
        }

        ////  initializeScoreFile
        ////  Open the score file while we still have the setuid privileges.  Later
        ////  when the score is being written out, you must be sure to flock the file
        ////  so we don't have multiple people trying to write to it at the same time.
        ////  Craig Norborg (doc)    Mon Aug 10 16:41:59 EST 1987
        //public bool initializeScoreFile()
        //{

        //    highscore_fp = fopen(Config.files.scores.c_str(), (char*)"rb+");

        //    return highscore_fp != nullptr;
        //}

        // Attempt to open and print the file containing the intro splash screen text -RAK-
        public void displaySplashScreen()
        {
            this.terminal.clearScreen();
            var lines = DataFilesResource.splash.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.None);
            var i = 0;
            foreach (var line in lines)
            {
                this.terminal.putString(line, new Coord_t(i, 0));
                i++;
            }

            this.terminal.waitForContinueKey(23);

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
        public void displayTextHelpFile(string helpText)
        {
            //FILE* file = fopen(filename.c_str(), "r");
            //if (file == nullptr)
            //{
            //    putStringClearToEOL("Can not find help file '" + filename + "'.", new Coord_t(0, 0));
            //    return;
            //}

            this.terminal.terminalSaveScreen();

            var lines = helpText.Split(new[] {Environment.NewLine, "\r", "\n"}, StringSplitOptions.None)
                .ToList();

            while (lines.Any())
            {
                this.terminal.clearScreen();
                for (var i = 0; i < 23 && lines.Count > 0; i++)
                {
                    var line = lines[0];
                    lines.RemoveAt(0);
                    this.terminal.putString(line, new Coord_t(i, 0));
                }


                this.terminal.putStringClearToEOL("[ press any key to continue ]", new Coord_t(23, 23));
                var input = this.terminal.getKeyInput();
                if (input == Ui_c.ESCAPE)
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

            this.terminal.terminalRestoreScreen();
        }

        // Open and display a "death" text file
        public void displayDeathFile(string resourceName)
        {
            this.terminal.clearScreen();

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

            for (var i = 0; i < 23 && i < lines.Length; i++)
            {
                this.terminal.putString(lines[i], new Coord_t(i, 0));
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

        // Write character sheet to the file
        private void writeCharacterSheetToFile(StringBuilder builder/*FILE* char_file*/)
        {
            var py = State.Instance.py;
            this.terminal.putStringClearToEOL("Writing character sheet...", new Coord_t(0, 0));
            this.terminal.putQIO();

            const char colon = ':';
            const char blank = ' ';

            //vtype_t stat_description = { '\0' };

            builder.AppendLine(Ui_c.CTRL_KEY_L.ToString());
            builder.AppendLine();
            //(void)fprintf(char_file, "%c\n\n", CTRL_KEY_L);

            builder.Append($" Name{colon,9} {py.misc.name,-23}");
            //(void)fprintf(char_file, " Name%9s %-23s", colon, py.misc.name);
            builder.Append($" Age{colon,11} {py.misc.age,6}");
            //(void)fprintf(char_file, " Age%11s %6d", colon, (int)py.misc.age);
            this.helpers.statsAsString(py.stats.used[(int)PlayerAttr.STR], out var stat_description);
            //statsAsString(py.stats.used[PlayerAttr.STR], stat_description);
            builder.AppendLine($"   STR : {stat_description}");
            //(void)fprintf(char_file, "   STR : %s\n", stat_description);
            
            builder.Append($" Race{colon,9} {Library.Instance.Player.character_races[(int) py.misc.race_id].name,-23}");
            //(void)fprintf(char_file, " Race%9s %-23s", colon, Library.Instance.Player.character_races[(int)py.misc.race_id].name);
            builder.Append($" Height{colon,8} {(int)py.misc.height,6:d}");
            //(void)fprintf(char_file, " Height%8s %6d", colon, (int)py.misc.height);
            this.helpers.statsAsString(py.stats.used[(int)PlayerAttr.INT], out stat_description);
            //statsAsString(py.stats.used[(int)PlayerAttr.INT], stat_description);
            builder.AppendLine($"   INT : {stat_description}");
            //(void)fprintf(char_file, "   INT : %s\n", stat_description);

            builder.Append($" Sex{colon,10} {Player_m.playerGetGenderLabel(),-23}");
            //(void)fprintf(char_file, " Sex%10s %-23s", colon, (Player_m.playerGetGenderLabel()));
            builder.Append($" Weight{colon,8} {(int)py.misc.weight,6:d}");
            //(void)fprintf(char_file, " Weight%8s %6d", colon, (int)py.misc.weight);
            this.helpers.statsAsString(py.stats.used[(int)PlayerAttr.WIS], out stat_description);
            //statsAsString(py.stats.used[(int)PlayerAttr.WIS], stat_description);
            builder.AppendLine($"   WIS : {stat_description}");
            //(void)fprintf(char_file, "   WIS : %s\n", stat_description);

            builder.Append($" Class{colon,8} {Library.Instance.Player.classes[(int)py.misc.class_id].title,-23}");
            //(void)fprintf(char_file, " Class%8s %-23s", colon, Library.Instance.Player.classes[(int)py.misc.class_id].title);
            builder.Append($" Social Class : {py.misc.social_class,6:d}");
            //(void)fprintf(char_file, " Social Class : %6d", py.misc.social_class);
            this.helpers.statsAsString(py.stats.used[(int)PlayerAttr.DEX], out stat_description);
            //statsAsString(py.stats.used[(int)PlayerAttr.DEX], stat_description);
            builder.AppendLine($"   DEX : {stat_description}");
            //(void)fprintf(char_file, "   DEX : %s\n", stat_description);

            builder.Append($" Title{colon,8} {Player_m.playerRankTitle(),-23}");
            //(void)fprintf(char_file, " Title%8s %-23s", colon, Player_m.playerRankTitle());
            builder.Append($"{blank,22}");
            //(void)fprintf(char_file, "%22s", blank);
            this.helpers.statsAsString(py.stats.used[(int)PlayerAttr.CON], out stat_description);
            //statsAsString(py.stats.used[(int)PlayerAttr.CON], stat_description);
            builder.AppendLine($"   CON : {stat_description}");
            //(void)fprintf(char_file, "   CON : %s\n", stat_description);

            builder.Append($"{blank,34}");
            //(void)fprintf(char_file, "%34s", blank);
            builder.Append($"{blank,26}");
            //(void)fprintf(char_file, "%26s", blank);
            this.helpers.statsAsString(py.stats.used[(int)PlayerAttr.CHR], out stat_description);
            //statsAsString(py.stats.used[(int)PlayerAttr.CHR], stat_description);
            builder.AppendLine($"   CHR : {stat_description}");
            //(void)fprintf(char_file, "   CHR : %s\n\n", stat_description);
            builder.AppendLine();

            builder.Append($" + To Hit    : {py.misc.display_to_hit,6:d}");
            //(void)fprintf(char_file, " + To Hit    : %6d", py.misc.display_to_hit);
            builder.Append($"{blank,7}Level      : {(int)py.misc.level,7:d}");
            //(void)fprintf(char_file, "%7sLevel      : %7d", blank, (int)py.misc.level);
            builder.AppendLine($"    Max Hit Points : {py.misc.max_hp,6:d}");
            //(void)fprintf(char_file, "    Max Hit Points : %6d\n", py.misc.max_hp);

            builder.Append($" + To Damage : {py.misc.display_to_damage,6:d}");
            //(void)fprintf(char_file, " + To Damage : %6d", py.misc.display_to_damage);
            builder.Append($"{blank,7}Experience : {py.misc.exp,7:d}");
            //(void)fprintf(char_file, "%7sExperience : %7d", blank, py.misc.exp);
            builder.AppendLine($"    Cur Hit Points : {py.misc.current_hp,6:d}");
            //(void)fprintf(char_file, "    Cur Hit Points : %6d\n", py.misc.current_hp);

            builder.Append($" + To AC     : {py.misc.display_to_ac,6}");
            //(void)fprintf(char_file, " + To AC     : %6d", py.misc.display_to_ac);
            builder.Append($"{blank,7}Max Exp    : {py.misc.max_exp,7:d}");
            //(void)fprintf(char_file, "%7sMax Exp    : %7d", blank, py.misc.max_exp);
            builder.AppendLine($"    Max Mana{colon,8} {py.misc.mana,6:d}");
            //(void)fprintf(char_file, "    Max Mana%8s %6d\n", colon, py.misc.mana);

            builder.Append($"   Total AC  : {py.misc.display_ac,6:d}");
            //(void)fprintf(char_file, "   Total AC  : %6d", py.misc.display_ac);
            if (py.misc.level >= Player_c.PLAYER_MAX_LEVEL)
            {
                builder.Append($"{blank,7}Exp to Adv : *******");
                //(void)fprintf(char_file, "%7sExp to Adv : *******", blank);
            }
            else
            {
                builder.Append($"{blank,7}Exp to Adv : {(int)(py.base_exp_levels[py.misc.level - 1] * py.misc.experience_factor / 100),7:d}");
                //(void)fprintf(char_file, "%7sExp to Adv : %7d", blank, (int)(py.base_exp_levels[py.misc.level - 1] * py.misc.experience_factor / 100));
            }
            builder.AppendLine($"    Cur Mana{colon,8} {py.misc.current_mana,6:d}");

            //(void)fprintf(char_file, "    Cur Mana%8s %6d\n", colon, py.misc.current_mana);
            builder.AppendLine($"{blank,28}Gold{colon,8} {py.misc.au,7:d}");
            builder.AppendLine();

            var xbth = py.misc.bth + py.misc.plusses_to_hit * (int)Player_c.BTH_PER_PLUS_TO_HIT_ADJUST + //
                       (Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.BTH] * (int)py.misc.level);
            var xbthb = py.misc.bth_with_bows + py.misc.plusses_to_hit * (int)Player_c.BTH_PER_PLUS_TO_HIT_ADJUST + //
                        (Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.BTHB] * (int)py.misc.level);

            // this results in a range from 0 to 29
            var xfos = 40 - py.misc.fos;
            if (xfos < 0)
            {
                xfos = 0;
            }
            var xsrh = py.misc.chance_in_search;

            // this results in a range from 0 to 9
            var xstl = py.misc.stealth_factor + 1;
            var xdis = py.misc.disarm + 2 * Player_stats_m.playerDisarmAdjustment() + Player_stats_m.playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT) + //
                       (Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DISARM] * (int)py.misc.level / 3);
            var xsave = py.misc.saving_throw + Player_stats_m.playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.WIS) + //
                        (Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.SAVE] * (int)py.misc.level / 3);
            var xdev = py.misc.saving_throw + Player_stats_m.playerStatAdjustmentWisdomIntelligence((int)PlayerAttr.INT) + //
                       (Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.DEVICE] * (int)py.misc.level / 3);

            //vtype_t xinfra = { '\0' };
            var xinfra = $"{py.flags.see_infra * 10:d} feet";
            //(void)sprintf(xinfra, "%d feet", py.flags.see_infra * 10);

            builder.AppendLine("(Miscellaneous Abilities)");
            builder.AppendLine();
            //(void)fprintf(char_file, "(Miscellaneous Abilities)\n\n");
            builder.Append($" Fighting    : {this.helpers.statRating(12, xbth),-10}");
            //(void)fprintf(char_file, " Fighting    : %-10s", this.terminalEx.statRating(12, xbth));
            builder.Append($"   Stealth     : {this.helpers.statRating(1, xstl),-10}");
            //(void)fprintf(char_file, "   Stealth     : %-10s", this.terminalEx.statRating(1, xstl));
            builder.AppendLine($"   Perception  : {this.helpers.statRating(3, xfos)}");
            //(void)fprintf(char_file, "   Perception  : %s\n", this.terminalEx.statRating(3, xfos));

            builder.Append($" Bows/Throw  : {this.helpers.statRating(12, xbthb),-10}");
            //(void)fprintf(char_file, " Bows/Throw  : %-10s", this.terminalEx.statRating(12, xbthb));
            builder.Append($"   Disarming   : {this.helpers.statRating(8, xdis),-10}");
            //(void)fprintf(char_file, "   Disarming   : %-10s", this.terminalEx.statRating(8, xdis));
            builder.AppendLine($"   Searching   : {this.helpers.statRating(6, xsrh)}");
            //(void)fprintf(char_file, "   Searching   : %s\n", this.terminalEx.statRating(6, xsrh));

            builder.Append($" Saving Throw: {this.helpers.statRating(6, xsave),-10}");
            //(void)fprintf(char_file, " Saving Throw: %-10s", this.terminalEx.statRating(6, xsave));
            builder.Append($"   Magic Device: {this.helpers.statRating(6, xdev),-10}");
            //(void)fprintf(char_file, "   Magic Device: %-10s", this.terminalEx.statRating(6, xdev));
            builder.AppendLine($"   Infra-Vision: {xinfra}");
            builder.AppendLine();
            //(void)fprintf(char_file, "   Infra-Vision: %s\n\n", xinfra);

            // Write out the character's history
            builder.AppendLine("Character Background");
            foreach (var entry in py.misc.history)
            {
                builder.AppendLine(entry);
            }
            //(void)fprintf(char_file, "Character Background\n");
            //for (auto & entry : py.misc.history)
            //{
            //    (void)fprintf(char_file, " %s\n", entry);
            //}
        }

        private string equipmentPlacementDescription(int item_id)
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

        // Write out the equipment list.
        private void writeEquipmentListToFile(/*FILE**//*string equip_file*/StringBuilder builder)
        {
            var py = State.Instance.py;

            builder.AppendLine();
            builder.AppendLine("  [Character's Equipment List]");
            builder.AppendLine();
            //(void)fprintf(equip_file, "\n  [Character's Equipment List]\n\n");

            if (py.equipment_count == 0)
            {
                builder.AppendLine("  Character has no equipment in use.");
                //(void)fprintf(equip_file, "  Character has no equipment in use.\n");
                return;
            }

            //obj_desc_t description = { '\0' };
            var item_slot_id = 0;

            for (var i = (int)PlayerEquipment.Wield; i < Inventory_c.PLAYER_INVENTORY_SIZE; i++)
            {
                if (py.inventory[i].category_id == Treasure_c.TV_NOTHING)
                {
                    continue;
                }

                this.identification.itemDescription(out var description, py.inventory[i], true);
                builder.AppendLine($"  {(char) (item_slot_id + 'a')}) {equipmentPlacementDescription(i),-19}: {description}");
                //(void)fprintf(equip_file, "  %c) %-19s: %s\n", item_slot_id + 'a', equipmentPlacementDescription(i), description);

                item_slot_id++;
            }

            builder.AppendLine(Ui_c.CTRL_KEY_L.ToString());
            builder.AppendLine();
            //(void)fprintf(equip_file, "%c\n\n", CTRL_KEY_L);
        }

        // Write out the character's inventory.
        private void writeInventoryToFile(/*FILE* *//*string inv_file*/StringBuilder builder)
        {
            var py = State.Instance.py;

            builder.AppendLine("  [General Inventory List]");
            builder.AppendLine();
            //(void)fprintf(inv_file, "  [General Inventory List]\n\n");

            if (py.pack.unique_items == 0)
            {
                builder.AppendLine("  Character has no objects in inventory.");
                //(void)fprintf(inv_file, "  Character has no objects in inventory.\n");
                return;
            }

            //obj_desc_t description = { '\0' };

            for (var i = 0; i < py.pack.unique_items; i++)
            {
                this.identification.itemDescription(out var description, py.inventory[i], true);
                builder.AppendLine($"{(char) (i + 'a')}) {description}");
                //(void)fprintf(inv_file, "%c) %s\n", i + 'a', description);
            }

            builder.Append(Ui_c.CTRL_KEY_L.ToString());
            //(void)fprintf(inv_file, "%c", CTRL_KEY_L);
        }

        //// Print the character to a file or device -RAK-
        public bool outputPlayerCharacterToFile(string filename)
        {
            if (this.fileSystem.File.Exists(filename))
            {
                if (!this.terminal.getInputConfirmation($"Replace existing file {filename}?"))
                {
                    return false;
                }
            }

            //int fd = open(filename, O_WRONLY | O_CREAT | O_EXCL, 0644);
            //if (fd < 0 && errno == EEXIST)
            //{
            //    if (getInputConfirmation("Replace existing file " + std::string(filename) + "?"))
            //    {
            //        fd = open(filename, O_WRONLY, 0644);
            //    }
            //}

            //FILE* file;
            //if (fd >= 0)
            //{
            //    // on some non-unix machines, fdopen() is not reliable,
            //    // hence must call close() and then fopen().
            //    (void)close(fd);
            //    file = fopen(filename, "w");
            //}
            //else
            //{
            //    file = nullptr;
            //}

            //if (file == nullptr)
            //{
            //    if (fd >= 0)
            //    {
            //        (void)close(fd);
            //    }
            //    vtype_t msg = { '\0' };
            //    (void)sprintf(msg, "Can't open file %s:", filename);
            //    printMessage(msg);
            //    return false;
            //}

            var builder = new StringBuilder();

            this.writeCharacterSheetToFile(builder);
            this.writeEquipmentListToFile(builder);
            this.writeInventoryToFile(builder);

            this.fileSystem.File.WriteAllText(filename, builder.ToString());

            //(void)fclose(file);

            //putStringClearToEOL("Completed.", new Coord_t(0, 0));
            this.terminal.putStringClearToEOL("Completed.", new Coord_t(0, 0));

            return true;
        }
    }
}
