using System;
using System.Collections.Generic;
using Moria.Core.Configs;
using Moria.Core.States;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Moria.Core.Constants;
using Moria.Core.Structures;
using Newtonsoft.Json;

namespace Moria.Core.Methods
{
    public interface IGameSave
    {
        bool loadGame(ref bool generate);

        bool saveGame();
    }

    public class Game_save_m : IGameSave
    {
        private readonly IFileSystem fileSystem;

        public Game_save_m(
            IFileSystem fileSystem
        )
        {
            this.fileSystem = fileSystem;
        }

        public bool loadGame(ref bool generate)
        {
            var saveGameContents = this.fileSystem.File.ReadAllText(Config.files.save_game);
            var instance = JsonConvert.DeserializeObject<State>(saveGameContents);
            State.Instance = instance;
            return true;
        }

        public bool saveGame()
        {
            var instance = State.Instance;
            var saveGameContents = JsonConvert.SerializeObject(instance, Formatting.Indented);
            File.WriteAllText(Config.files.save_game, saveGameContents);
            return true;
        }

        /*

        // This save package was brought to by                -JWT-
        // and                                                -RAK-
        // and has been completely rewritten for UNIX by      -JEW-
        // and has been completely rewritten again by         -CJS-
        // and completely rewritten again! for portability by -JEW-

        // Set up prior to actual save, do the save, then clean up
        public static bool saveGame()
        {
            var input = string.Empty;
            //vtype_t input = { '\0' };
            string output;

            while (!saveChar(Config.files.save_game))
            {
                output = "Save file '" + Config.files.save_game + "' fails.";
                printMessage(output);

                int i = 0;
                if (access(Config.files.save_game.c_str(), 0) < 0 || !getInputConfirmation("File exists. Delete old save file?") || (i = unlink(Config.files.save_game)) < 0)
                {
                    if (i < 0)
                    {
                        output = "Can't delete '" + Config.files.save_game + "'";
                        printMessage(output);
                    }
                    putStringClearToEOL("New Save file [ESC to give up]:", new Coord_t(0, 0));
                    if (!getStringInput(out input, new Coord_t(0, 31), 45))
                    {
                        return false;
                    }
                    if (input[0] != 0)
                    {
                        // (void) strcpy(Config.files.save_game, input);
                        Config.files.save_game = input;
                    }
                }
                output = "Saving with '" + Config.files.save_game + "'...";
                putStringClearToEOL(output, new Coord_t(0, 0));
            }

            return true;
        }

        static bool svWrite()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            // clear the game.character_is_dead flag when creating a HANGUP save file,
            // so that player can see tombstone when restart
            if (State.Instance.eof_flag != 0)
            {
                game.character_is_dead = false;
            }

            uint l = 0;

            if (Config.options.run_cut_corners)
            {
                l |= 0x1;
            }
            if (Config.options.run_examine_corners)
            {
                l |= 0x2;
            }
            if (Config.options.run_print_self)
            {
                l |= 0x4;
            }
            if (Config.options.find_bound)
            {
                l |= 0x8;
            }
            if (Config.options.prompt_to_pickup)
            {
                l |= 0x10;
            }
            if (Config.options.use_roguelike_keys)
            {
                l |= 0x20;
            }
            if (Config.options.show_inventory_weights)
            {
                l |= 0x40;
            }
            if (Config.options.highlight_seams)
            {
                l |= 0x80;
            }
            if (Config.options.run_ignore_doors)
            {
                l |= 0x100;
            }
            if (Config.options.error_beep_sound)
            {
                l |= 0x200;
            }
            if (Config.options.display_counts)
            {
                l |= 0x400;
            }
            if (game.character_is_dead)
            {
                // Sign bit
                l |= 0x80000000;
            }
            if (game.total_winner)
            {
                l |= 0x40000000;
            }

            for (int i = 0; i < MON_MAX_CREATURES; i++)
            {
                Recall_t r = State.Instance.creature_recall[i];
                if (r.movement != 0 ||
                    r.defenses != 0 ||
                    r.kills != 0 ||
                    r.spells != 0 ||
                    r.deaths != 0 ||
                    r.attacks[0] != 0 ||
                    r.attacks[1] != 0 ||
                    r.attacks[2] != 0 ||
                    r.attacks[3] != 0
                )
                {
                    wrShort((uint)i);
                    wrLong(r.movement);
                    wrLong(r.spells);
                    wrShort(r.kills);
                    wrShort(r.deaths);
                    wrShort(r.defenses);
                    wrByte(r.wake);
                    wrByte(r.ignore);
                    wrBytes(r.attacks, MON_MAX_ATTACKS);
                }
            }

            // sentinel to indicate no more monster info
            wrShort((uint)0xFFFF);

            wrLong(l);

            wrString(py.misc.name);
            wrBool(py.misc.gender);
            wrLong((uint)py.misc.au);
            wrLong((uint)py.misc.max_exp);
            wrLong((uint)py.misc.exp);
            wrShort(py.misc.exp_fraction);
            wrShort(py.misc.age);
            wrShort(py.misc.height);
            wrShort(py.misc.weight);
            wrShort(py.misc.level);
            wrShort(py.misc.max_dungeon_depth);
            wrShort((uint)py.misc.chance_in_search);
            wrShort((uint)py.misc.fos);
            wrShort((uint)py.misc.bth);
            wrShort((uint)py.misc.bth_with_bows);
            wrShort((uint)py.misc.mana);
            wrShort((uint)py.misc.max_hp);
            wrShort((uint)py.misc.plusses_to_hit);
            wrShort((uint)py.misc.plusses_to_damage);
            wrShort((uint)py.misc.ac);
            wrShort((uint)py.misc.magical_ac);
            wrShort((uint)py.misc.display_to_hit);
            wrShort((uint)py.misc.display_to_damage);
            wrShort((uint)py.misc.display_ac);
            wrShort((uint)py.misc.display_to_ac);
            wrShort((uint)py.misc.disarm);
            wrShort((uint)py.misc.saving_throw);
            wrShort((uint)py.misc.social_class);
            wrShort((uint)py.misc.stealth_factor);
            wrByte(py.misc.class_id);
            wrByte(py.misc.race_id);
            wrByte(py.misc.hit_die);
            wrByte(py.misc.experience_factor);
            wrShort((uint)py.misc.current_mana);
            wrShort(py.misc.current_mana_fraction);
            wrShort((uint)py.misc.current_hp);
            wrShort(py.misc.current_hp_fraction);
            foreach (var entry in py.misc.history)
            {
                wrString(entry);
            }

            wrBytes(py.stats.max, 6);
            wrBytes(py.stats.current, 6);
            wrShorts((uint[])py.stats.modified, 6);
            wrBytes(py.stats.used, 6);

            wrLong(py.flags.status);
            wrShort((uint)py.flags.rest);
            wrShort((uint)py.flags.blind);
            wrShort((uint)py.flags.paralysis);
            wrShort((uint)py.flags.confused);
            wrShort((uint)py.flags.food);
            wrShort((uint)py.flags.food_digested);
            wrShort((uint)py.flags.protection);
            wrShort((uint)py.flags.speed);
            wrShort((uint)py.flags.fast);
            wrShort((uint)py.flags.slow);
            wrShort((uint)py.flags.afraid);
            wrShort((uint)py.flags.poisoned);
            wrShort((uint)py.flags.image);
            wrShort((uint)py.flags.protect_evil);
            wrShort((uint)py.flags.invulnerability);
            wrShort((uint)py.flags.heroism);
            wrShort((uint)py.flags.super_heroism);
            wrShort((uint)py.flags.blessed);
            wrShort((uint)py.flags.heat_resistance);
            wrShort((uint)py.flags.cold_resistance);
            wrShort((uint)py.flags.detect_invisible);
            wrShort((uint)py.flags.word_of_recall);
            wrShort((uint)py.flags.see_infra);
            wrShort((uint)py.flags.timed_infra);
            wrBool(py.flags.see_invisible);
            wrBool(py.flags.teleport);
            wrBool(py.flags.free_action);
            wrBool(py.flags.slow_digest);
            wrBool(py.flags.aggravate);
            wrBool(py.flags.resistant_to_fire);
            wrBool(py.flags.resistant_to_cold);
            wrBool(py.flags.resistant_to_acid);
            wrBool(py.flags.regenerate_hp);
            wrBool(py.flags.resistant_to_light);
            wrBool(py.flags.free_fall);
            wrBool(py.flags.sustain_str);
            wrBool(py.flags.sustain_int);
            wrBool(py.flags.sustain_wis);
            wrBool(py.flags.sustain_con);
            wrBool(py.flags.sustain_dex);
            wrBool(py.flags.sustain_chr);
            wrBool(py.flags.confuse_monster);
            wrByte(py.flags.new_spells_to_learn);

            wrShort((uint)State.Instance.missiles_counter);
            wrLong((uint)State.Instance.dg.game_turn);
            wrShort((uint)py.pack.unique_items);
            for (int i = 0; i < py.pack.unique_items; i++)
            {
                wrItem(py.inventory[i]);
            }
            for (int i = PlayerEquipment.Wield; i < PLAYER_INVENTORY_SIZE; i++)
            {
                wrItem(py.inventory[i]);
            }
            wrShort((uint)py.pack.weight);
            wrShort((uint)py.equipment_count);
            wrLong(py.flags.spells_learnt);
            wrLong(py.flags.spells_worked);
            wrLong(py.flags.spells_forgotten);
            wrBytes(py.flags.spells_learned_order, 32);
            wrBytes(State.Instance.objects_identified, OBJECT_IDENT_SIZE);
            wrLong(game.magic_seed);
            wrLong(game.town_seed);
            wrShort((uint)State.Instance.last_message_id);
            foreach (var message in messages)
            {
                wrString(message);
            }

            // this indicates 'cheating' if it is a one
            wrShort((uint)(State.Instance.panic_save ? 1 : 0));
            wrShort((uint)(game.total_winner ? 1 : 0));
            wrShort((uint)game.noscore);
            wrShorts(py.base_hp_levels, PLAYER_MAX_LEVEL);

            foreach (var store in stores)
            {
                wrLong((uint)store.turns_left_before_closing);
                wrShort((uint)store.insults_counter);
                wrByte(store.owner_id);
                wrByte(store.unique_items_counter);
                wrShort(store.good_purchases);
                wrShort(store.bad_purchases);
                for (int j = 0; j < store.unique_items_counter; j++)
                {
                    wrLong((uint)store.inventory[j].cost);
                    wrItem(store.inventory[j].item);
                }
            }

            // save the current time in the save file
            l = DateTime.Now;

            if (l < start_time)
            {
                // someone is messing with the clock!,
                // assume that we have been playing for 1 day
                l = (uint)(start_time + 86400L);
            }
            wrLong(l);

            // put game.character_died_from string in save file
            wrString(game.character_died_from);

            // put the max_score in the save file
            l = (uint)(playerCalculateTotalPoints());
            wrLong(l);

            // put the date_of_birth in the save file
            wrLong((uint)py.misc.date_of_birth);

            // only level specific info follows, this allows characters to be
            // resurrected, the dungeon level info is not needed for a resurrection
            if (game.character_is_dead)
            {
                return !((ferror(fileptr) != 0) || fflush(fileptr) == EOF);
            }

            var dg = State.Instance.dg;
            wrShort((uint)dg.current_level);
            wrShort((uint)py.pos.y);
            wrShort((uint)py.pos.x);
            wrShort((uint)State.Instance.monster_multiply_total);
            wrShort((uint)dg.height);
            wrShort((uint)dg.width);
            wrShort((uint)dg.panel.max_rows);
            wrShort((uint)dg.panel.max_cols);

            for (int i = 0; i < MAX_HEIGHT; i++)
            {
                for (int j = 0; j < MAX_WIDTH; j++)
                {
                    if (dg.floor[i][j].creature_id != 0)
                    {
                        wrByte((uint)i);
                        wrByte((uint)j);
                        wrByte(dg.floor[i][j].creature_id);
                    }
                }
            }

            // marks end of creature_id info
            wrByte((uint)0xFF);

            for (int i = 0; i < MAX_HEIGHT; i++)
            {
                for (int j = 0; j < MAX_WIDTH; j++)
                {
                    if (dg.floor[i][j].treasure_id != 0)
                    {
                        wrByte((uint)i);
                        wrByte((uint)j);
                        wrByte(dg.floor[i][j].treasure_id);
                    }
                }
            }

            // marks end of treasure_id info
            wrByte((uint)0xFF);

            // must set counter to zero, note that code may write out two bytes unnecessarily
            int count = 0;
            uint prev_char = 0;

            foreach (var row in dg.floor)
            {
                foreach (var tile in row)
                {
                    var char_tmp = (uint)(tile.feature_id |
                                          ((tile.perma_lit_room ? 1u : 0) << 4) |
                                          ((tile.field_mark ? 1u : 0) << 5) |
                                          ((tile.permanent_light ? 1u : 0) << 6) |
                                          ((tile.temporary_light ? 1u : 0) << 7)
                                        );

                    if (char_tmp != prev_char || count == UCHAR_MAX)
                    {
                        wrByte((uint)count);
                        wrByte(prev_char);
                        prev_char = char_tmp;
                        count = 1;
                    }
                    else
                    {
                        count++;
                    }
                }
            }

            // save last entry
            wrByte((uint)count);
            wrByte(prev_char);

            wrShort((uint)game.treasure.current_id);
            for (int i = (int)Config.treasure.MIN_TREASURE_LIST_ID; i < game.treasure.current_id; i++)
            {
                wrItem(game.treasure.list[i]);
            }
            wrShort((uint)State.Instance.next_free_monster_id);
            for (int i = (int)Config.monsters.MON_MIN_INDEX_ID; i < State.Instance.next_free_monster_id; i++)
            {
                wrMonster(State.Instance.monsters[i]);
            }

            return !((ferror(fileptr) != 0) || fflush(fileptr) == EOF);
        }

        static bool saveChar(string filename)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            if (game.character_saved)
            {
                return true; // Nothing to save.
            }

            putQIO();
            playerDisturb(1, 0);                   // Turn off resting and searching.
            playerChangeSpeed(-py.pack.heaviness); // Fix the speed
            py.pack.heaviness = 0;
            bool ok = false;

            fileptr = nullptr; // Do not assume it has been init'ed

            int fd = open(filename, O_RDWR | O_CREAT | O_EXCL, 0600);

            if (fd < 0 && access(filename, 0) >= 0 && ((from_save_file != 0) || (game.wizard_mode && getInputConfirmation("Can't make new save file. Overwrite old?"))))
            {
                (void)chmod(filename.c_str(), 0600);
                fd = open(filename.c_str(), O_RDWR | O_TRUNC, 0600);
            }

            if (fd >= 0)
            {
                (void)close(fd);
                fileptr = fopen(Config.files.save_game, "wb");
            }

            DEBUG(logfile = fopen("IO_LOG", "a"));
            DEBUG(fprintf(logfile, "Saving data to %s\n", Config.files.save_game));

            if (fileptr != nullptr)
            {
                xor_byte = 0;
                wrByte(CURRENT_VERSION_MAJOR);
                xor_byte = 0;
                wrByte(CURRENT_VERSION_MINOR);
                xor_byte = 0;
                wrByte(CURRENT_VERSION_PATCH);
                xor_byte = 0;

                auto char_tmp = (uint8_t)(randomNumber(256) - 1);
                wrByte(char_tmp);
                // Note that xor_byte is now equal to char_tmp

                ok = svWrite();

                DEBUG(fclose(logfile));

                if (fclose(fileptr) == EOF)
                {
                    ok = false;
                }
            }

            if (!ok)
            {
                if (fd >= 0)
                {
                    (void)unlink(filename.c_str());
                }

                std::string output;
                if (fd >= 0)
                {
                    output = "Error writing to file '" + filename + "'";
                }
                else
                {
                    output = "Can't create new file '" + filename + "'";
                }
                printMessage(output.c_str());

                return false;
            }

            game.character_saved = true;
            dg.game_turn = -1;

            return true;
        }

        // Certain checks are omitted for the wizard. -CJS-
        public static bool loadGame(ref bool generate)
        {
            Tile_t* tile = nullptr;
            int c;
            uint32_t time_saved = 0;
            uint8_t version_maj = 0;
            uint8_t version_min = 0;
            uint8_t patch_level = 0;

            generate = true;
            int fd = -1;
            int total_count = 0;

            // Not required for Mac, because the file name is obtained through a dialog.
            // There is no way for a nonexistent file to be specified. -BS-
            if (access(Config.files.save_game.c_str(), 0) != 0)
            {
                printMessage("Save file does not exist.");
                return false; // Don't bother with messages here. File absent.
            }

            clearScreen();

            std::string filename = "Save file '" + Config.files.save_game + "' present. Attempting restore.";
            putString(filename.c_str(), new Coord_t(23, 0));

            // FIXME: check this if/else logic! -- MRC
            if (dg.game_turn >= 0)
            {
                printMessage("IMPOSSIBLE! Attempt to restore while still alive!");
            }
            else if ((fd = open(Config.files.save_game.c_str(), O_RDONLY, 0)) < 0 &&
                     (chmod(Config.files.save_game.c_str(), 0400) < 0 || (fd = open(Config.files.save_game.c_str(), O_RDONLY, 0)) < 0))
            {
                // Allow restoring a file belonging to someone else, if we can delete it.
                // Hence first try to read without doing a chmod.

                printMessage("Can't open file for reading.");
            }
            else
            {
                dg.game_turn = -1;
                bool ok = true;

                (void)close(fd);
                fd = -1; // Make sure it isn't closed again
                fileptr = fopen(Config.files.save_game.c_str(), "rb");

                if (fileptr == nullptr)
                {
                    goto error;
                }

                putStringClearToEOL("Restoring Memory...", new Coord_t(0, 0));
                putQIO();

                DEBUG(logfile = fopen("IO_LOG", "a"));
                DEBUG(fprintf(logfile, "Reading data from %s\n", Config.files.save_game));

                // Note: setting these xor_byte is correct!
                xor_byte = 0;
                version_maj = rdByte();
                xor_byte = 0;
                version_min = rdByte();
                xor_byte = 0;
                patch_level = rdByte();

                xor_byte = getByte();

                if (!validGameVersion(version_maj, version_min, patch_level))
                {
                    putStringClearToEOL("Sorry. This save file is from a different version of umoria.", new Coord_t(2, 0));
                    goto error;
                }

                uint16_t uint_16_t_tmp;
                uint32_t l;

                uint_16_t_tmp = rdShort();
                while (uint_16_t_tmp != 0xFFFF)
                {
                    if (uint_16_t_tmp >= MON_MAX_CREATURES)
                    {
                        goto error;
                    }
                    Recall_t & memory = creature_recall[uint_16_t_tmp];
                    memory.movement = rdLong();
                    memory.spells = rdLong();
                    memory.kills = rdShort();
                    memory.deaths = rdShort();
                    memory.defenses = rdShort();
                    memory.wake = rdByte();
                    memory.ignore = rdByte();
                    rdBytes(memory.attacks, MON_MAX_ATTACKS);
                    uint_16_t_tmp = rdShort();
                }

                l = rdLong();

                Config.options.run_cut_corners = (l & 0x1) != 0;
                Config.options.run_examine_corners = (l & 0x2) != 0;
                Config.options.run_print_self = (l & 0x4) != 0;
                Config.options.find_bound = (l & 0x8) != 0;
                Config.options.prompt_to_pickup = (l & 0x10) != 0;
                Config.options.use_roguelike_keys = (l & 0x20) != 0;
                Config.options.show_inventory_weights = (l & 0x40) != 0;
                Config.options.highlight_seams = (l & 0x80) != 0;
                Config.options.run_ignore_doors = (l & 0x100) != 0;
                Config.options.error_beep_sound = (l & 0x200) != 0;
                Config.options.display_counts = (l & 0x400) != 0;

                // Don't allow resurrection of game.total_winner characters.  It causes
                // problems because the character level is out of the allowed range.
                if (game.to_be_wizard && ((l & 0x40000000L) != 0))
                {
                    printMessage("Sorry, this character is retired from moria.");
                    printMessage("You can not resurrect a retired character.");
                }
                else if (game.to_be_wizard && ((l & 0x80000000L) != 0) && getInputConfirmation("Resurrect a dead character?"))
                {
                    l &= ~0x80000000L;
                }

                if ((l & 0x80000000L) == 0)
                {
                    rdString(py.misc.name);
                    py.misc.gender = rdBool();
                    py.misc.au = rdLong();
                    py.misc.max_exp = rdLong();
                    py.misc.exp = rdLong();
                    py.misc.exp_fraction = rdShort();
                    py.misc.age = rdShort();
                    py.misc.height = rdShort();
                    py.misc.weight = rdShort();
                    py.misc.level = rdShort();
                    py.misc.max_dungeon_depth = rdShort();
                    py.misc.chance_in_search = rdShort();
                    py.misc.fos = rdShort();
                    py.misc.bth = rdShort();
                    py.misc.bth_with_bows = rdShort();
                    py.misc.mana = rdShort();
                    py.misc.max_hp = rdShort();
                    py.misc.plusses_to_hit = rdShort();
                    py.misc.plusses_to_damage = rdShort();
                    py.misc.ac = rdShort();
                    py.misc.magical_ac = rdShort();
                    py.misc.display_to_hit = rdShort();
                    py.misc.display_to_damage = rdShort();
                    py.misc.display_ac = rdShort();
                    py.misc.display_to_ac = rdShort();
                    py.misc.disarm = rdShort();
                    py.misc.saving_throw = rdShort();
                    py.misc.social_class = rdShort();
                    py.misc.stealth_factor = rdShort();
                    py.misc.class_id = rdByte();
                    py.misc.race_id = rdByte();
                    py.misc.hit_die = rdByte();
                    py.misc.experience_factor = rdByte();
                    py.misc.current_mana = rdShort();
                    py.misc.current_mana_fraction = rdShort();
                    py.misc.current_hp = rdShort();
                    py.misc.current_hp_fraction = rdShort();
                    for (auto & entry : py.misc.history)
                    {
                        rdString(entry);
                    }

                    rdBytes(py.stats.max, 6);
                    rdBytes(py.stats.current, 6);
                    rdShorts((uint16_t*)py.stats.modified, 6);
                    rdBytes(py.stats.used, 6);

                    py.flags.status = rdLong();
                    py.flags.rest = rdShort();
                    py.flags.blind = rdShort();
                    py.flags.paralysis = rdShort();
                    py.flags.confused = rdShort();
                    py.flags.food = rdShort();
                    py.flags.food_digested = rdShort();
                    py.flags.protection = rdShort();
                    py.flags.speed = rdShort();
                    py.flags.fast = rdShort();
                    py.flags.slow = rdShort();
                    py.flags.afraid = rdShort();
                    py.flags.poisoned = rdShort();
                    py.flags.image = rdShort();
                    py.flags.protect_evil = rdShort();
                    py.flags.invulnerability = rdShort();
                    py.flags.heroism = rdShort();
                    py.flags.super_heroism = rdShort();
                    py.flags.blessed = rdShort();
                    py.flags.heat_resistance = rdShort();
                    py.flags.cold_resistance = rdShort();
                    py.flags.detect_invisible = rdShort();
                    py.flags.word_of_recall = rdShort();
                    py.flags.see_infra = rdShort();
                    py.flags.timed_infra = rdShort();
                    py.flags.see_invisible = rdBool();
                    py.flags.teleport = rdBool();
                    py.flags.free_action = rdBool();
                    py.flags.slow_digest = rdBool();
                    py.flags.aggravate = rdBool();
                    py.flags.resistant_to_fire = rdBool();
                    py.flags.resistant_to_cold = rdBool();
                    py.flags.resistant_to_acid = rdBool();
                    py.flags.regenerate_hp = rdBool();
                    py.flags.resistant_to_light = rdBool();
                    py.flags.free_fall = rdBool();
                    py.flags.sustain_str = rdBool();
                    py.flags.sustain_int = rdBool();
                    py.flags.sustain_wis = rdBool();
                    py.flags.sustain_con = rdBool();
                    py.flags.sustain_dex = rdBool();
                    py.flags.sustain_chr = rdBool();
                    py.flags.confuse_monster = rdBool();
                    py.flags.new_spells_to_learn = rdByte();

                    missiles_counter = rdShort();
                    dg.game_turn = rdLong();
                    py.pack.unique_items = rdShort();
                    if (py.pack.unique_items > PlayerEquipment::Wield)
                    {
                        goto error;
                    }
                    for (int i = 0; i < py.pack.unique_items; i++)
                    {
                        rdItem(py.inventory[i]);
                    }
                    for (int i = PlayerEquipment::Wield; i < PLAYER_INVENTORY_SIZE; i++)
                    {
                        rdItem(py.inventory[i]);
                    }
                    py.pack.weight = rdShort();
                    py.equipment_count = rdShort();
                    py.flags.spells_learnt = rdLong();
                    py.flags.spells_worked = rdLong();
                    py.flags.spells_forgotten = rdLong();
                    rdBytes(py.flags.spells_learned_order, 32);
                    rdBytes(objects_identified, OBJECT_IDENT_SIZE);
                    game.magic_seed = rdLong();
                    game.town_seed = rdLong();
                    last_message_id = rdShort();
                    for (auto & message : messages)
                    {
                        rdString(message);
                    }

                    uint16_t panic_save_short;
                    uint16_t total_winner_short;
                    panic_save_short = rdShort();
                    total_winner_short = rdShort();
                    panic_save = panic_save_short != 0;
                    game.total_winner = total_winner_short != 0;

                    game.noscore = rdShort();
                    rdShorts(py.base_hp_levels, PLAYER_MAX_LEVEL);

                    for (auto & store : stores)
                    {
                        store.turns_left_before_closing = rdLong();
                        store.insults_counter = rdShort();
                        store.owner_id = rdByte();
                        store.unique_items_counter = rdByte();
                        store.good_purchases = rdShort();
                        store.bad_purchases = rdShort();
                        if (store.unique_items_counter > STORE_MAX_DISCRETE_ITEMS)
                        {
                            goto error;
                        }
                        for (int j = 0; j < store.unique_items_counter; j++)
                        {
                            store.inventory[j].cost = rdLong();
                            rdItem(store.inventory[j].item);
                        }
                    }

                    time_saved = rdLong();
                    rdString(game.character_died_from);
                    py.max_score = rdLong();
                    py.misc.date_of_birth = rdLong();
                }

                c = getc(fileptr);
                if (c == EOF || ((l & 0x80000000L) != 0))
                {
                    if ((l & 0x80000000L) == 0)
                    {
                        if (!game.to_be_wizard || dg.game_turn < 0)
                        {
                            goto error;
                        }
                        putStringClearToEOL("Attempting a resurrection!", new Coord_t(0, 0));
                        if (py.misc.current_hp < 0)
                        {
                            py.misc.current_hp = 0;
                            py.misc.current_hp_fraction = 0;
                        }

                        // don't let them starve to death immediately
                        if (py.flags.food < 0)
                        {
                            py.flags.food = 0;
                        }

                        // don't let them immediately die of poison again
                        if (py.flags.poisoned > 1)
                        {
                            py.flags.poisoned = 1;
                        }

                        dg.current_level = 0; // Resurrect on the town level.
                        game.character_generated = true;

                        // set `noscore` to indicate a resurrection, and don't enter wizard mode
                        game.to_be_wizard = false;
                        game.noscore |= 0x1;
                    }
                    else
                    {
                        // Make sure that this message is seen, since it is a bit
                        // more interesting than the other messages.
                        printMessage("Restoring Memory of a departed spirit...");
                        dg.game_turn = -1;
                    }
                    putQIO();
                    goto closefiles;
                }
                if (ungetc(c, fileptr) == EOF)
                {
                    goto error;
                }

                putStringClearToEOL("Restoring Character...", new Coord_t(0, 0));
                putQIO();

                // only level specific info should follow,
                // not present for dead characters

                dg.current_level = rdShort();
                py.pos.y = rdShort();
                py.pos.x = rdShort();
                monster_multiply_total = rdShort();
                dg.height = rdShort();
                dg.width = rdShort();
                dg.panel.max_rows = rdShort();
                dg.panel.max_cols = rdShort();

                uint8_t char_tmp, ychar, xchar, count;

                // read in the creature ptr info
                char_tmp = rdByte();
                while (char_tmp != 0xFF)
                {
                    ychar = char_tmp;
                    xchar = rdByte();
                    char_tmp = rdByte();
                    if (xchar > MAX_WIDTH || ychar > MAX_HEIGHT)
                    {
                        goto error;
                    }
                    dg.floor[ychar][xchar].creature_id = char_tmp;
                    char_tmp = rdByte();
                }

                // read in the treasure ptr info
                char_tmp = rdByte();
                while (char_tmp != 0xFF)
                {
                    ychar = char_tmp;
                    xchar = rdByte();
                    char_tmp = rdByte();
                    if (xchar > MAX_WIDTH || ychar > MAX_HEIGHT)
                    {
                        goto error;
                    }
                    dg.floor[ychar][xchar].treasure_id = char_tmp;
                    char_tmp = rdByte();
                }

                // read in the rest of the cave info
                tile = &dg.floor[0][0];
                total_count = 0;
                while (total_count != MAX_HEIGHT * MAX_WIDTH)
                {
                    count = rdByte();
                    char_tmp = rdByte();
                    for (int i = count; i > 0; i--)
                    {
                        if (tile >= &dg.floor[MAX_HEIGHT][0])
                        {
                            goto error;
                        }
                        tile->feature_id = (uint8_t)(char_tmp & 0xF);
                        tile->perma_lit_room = (bool)((char_tmp >> 4) & 0x1);
                        tile->field_mark = (bool)((char_tmp >> 5) & 0x1);
                        tile->permanent_light = (bool)((char_tmp >> 6) & 0x1);
                        tile->temporary_light = (bool)((char_tmp >> 7) & 0x1);
                        tile++;
                    }
                    total_count += count;
                }

                game.treasure.current_id = rdShort();
                if (game.treasure.current_id > LEVEL_MAX_OBJECTS)
                {
                    goto error;
                }
                for (int i = config::treasure::MIN_TREASURE_LIST_ID; i < game.treasure.current_id; i++)
                {
                    rdItem(game.treasure.list[i]);
                }
                next_free_monster_id = rdShort();
                if (next_free_monster_id > MON_TOTAL_ALLOCATIONS)
                {
                    goto error;
                }
                for (int i = config::monsters::MON_MIN_INDEX_ID; i < next_free_monster_id; i++)
                {
                    rdMonster(monsters[i]);
                }

                generate = false; // We have restored a cave - no need to generate.

                if (ferror(fileptr) != 0)
                {
                    goto error;
                }

                if (dg.game_turn < 0)
                {
                    //error:
                    ok = false; // Assume bad data.
                }
                else
                {
                    // don't overwrite the killed by string if character is dead
                    if (py.misc.current_hp >= 0)
                    {
                        (void)strcpy(game.character_died_from, "(alive and well)");
                    }
                    game.character_generated = true;
                }

                //closefiles:

                DEBUG(fclose(logfile));

                if (fileptr != nullptr)
                {
                    if (fclose(fileptr) < 0)
                    {
                        ok = false;
                    }
                }
                if (fd >= 0)
                {
                    (void)close(fd);
                }

                if (!ok)
                {
                    printMessage("Error during reading of file.");
                }
                else
                {
                    // let the user overwrite the old save file when save/quit
                    from_save_file = 1;

                    if (panic_save)
                    {
                        printMessage("This game is from a panic save.  Score will not be added to scoreboard.");
                    }
                    else if ((!game.noscore) & 0x04)
                    {
                        printMessage("This character is already on the scoreboard; it will not be scored again.");
                        game.noscore |= 0x4;
                    }

                    if (dg.game_turn >= 0)
                    { // Only if a full restoration.
                        py.weapon_is_heavy = false;
                        py.pack.heaviness = 0;
                        playerStrength();

                        // rotate store inventory, depending on how old the save file
                        // is foreach day old (rounded up), call storeMaintenance
                        // calculate age in seconds
                        start_time = getCurrentUnixTime();

                        uint32_t age;

                        // check for reasonable values of time here ...
                        if (start_time < time_saved)
                        {
                            age = 0;
                        }
                        else
                        {
                            age = start_time - time_saved;
                        }

                        age = (uint32_t)((age + 43200L) / 86400L); // age in days
                        if (age > 10)
                        {
                            age = 10; // in case save file is very old
                        }

                        for (int i = 0; i < (int)age; i++)
                        {
                            storeMaintenance();
                        }
                    }

                    if (game.noscore != 0)
                    {
                        printMessage("This save file cannot be used to get on the score board.");
                    }
                    if (validGameVersion(version_maj, version_min, patch_level) && !isCurrentGameVersion(version_maj, version_min, patch_level))
                    {
                        std::string msg = "Save file version ";
                        msg += std::to_string(version_maj) + "." + std::to_string(version_min);
                        msg += " accepted on game version ";
                        msg += std::to_string(CURRENT_VERSION_MAJOR) + "." + std::to_string(CURRENT_VERSION_MINOR) + ".";
                        printMessage(msg.c_str());
                    }

                    // if false: only restored options and monster memory.
                    return dg.game_turn >= 0;
                }
            }
            dg.game_turn = -1;
            putStringClearToEOL("Please try again without that save file.", new Coord_t(1, 0));

            // We have messages for the player to read, this will ask for a keypress
            printMessage(CNIL);

            exitProgram();

            return false; // not reached
        }

            */

        private void wrBool(BinaryWriter writer, ref uint xor_byte, bool value)
        {
            this.wrByte(writer, ref xor_byte, (uint)(value ? 1u : 0));
        }

        private void wrByte(BinaryWriter writer, ref uint xor_byte, uint value)
        {
            xor_byte ^= value;
            writer.Write((int)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, "BYTE:  %02X = %d\n", (int)xor_byte, (int)value));
        }

        private void wrShort(BinaryWriter writer, ref uint xor_byte, uint value)
        {
            xor_byte ^= (value & 0xFF);
            writer.Write((int)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, "SHORT: %02X", (int)xor_byte));
            xor_byte ^= ((value >> 8) & 0xFF);
            writer.Write((int)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X = %d\n", (int)xor_byte, (int)value));
        }

        private void wrLong(BinaryWriter writer, ref uint xor_byte, uint value)
        {
            xor_byte ^= (value & 0xFF);
            writer.Write((int)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, "LONG:  %02X", (int)xor_byte));
            xor_byte ^= ((value >> 8) & 0xFF);
            writer.Write((int)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X", (int)xor_byte));
            xor_byte ^= ((value >> 16) & 0xFF);
            writer.Write((int)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X", (int)xor_byte));
            xor_byte ^= ((value >> 24) & 0xFF);
            writer.Write((int)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X = %ld\n", (int)xor_byte, (int32_t)value));
        }

        private void wrBytes(BinaryWriter writer, ref uint xor_byte, uint[] values, int count)
        {
            //uint8_t* ptr;

            //DEBUG(fprintf(logfile, "%d BYTES:", count));
            //ptr = value;
            for (int i = 0; i < count; i++)
            {
                xor_byte ^= values[i];
                //xor_byte ^= *ptr++;
                //(void)putc((int)xor_byte, fileptr);
                writer.Write((int)xor_byte);
                //DEBUG(fprintf(logfile, "  %02X = %d", (int)xor_byte, (int)(ptr[-1])));
            }

            //DEBUG(fprintf(logfile, "\n"));
        }

        private void wrString(BinaryWriter writer, ref uint xor_byte, string str)
        {
            //DEBUG(char * s = str);
            //DEBUG(fprintf(logfile, "STRING:"));
            //while (*str != '\0')
            foreach (var c in str)
            {
                //xor_byte ^= *str++;
                xor_byte = c;
                writer.Write((int)xor_byte);
                //(void)putc((int)xor_byte, fileptr);
                //DEBUG(fprintf(logfile, " %02X", (int)xor_byte));
            }
            xor_byte ^= '\0';
            writer.Write((int)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X = \"%s\"\n", (int)xor_byte, s));
        }

        private void wrShorts(BinaryWriter writer, ref uint xor_byte, uint[] values, int count)
        {
            //DEBUG(fprintf(logfile, "%d SHORTS:", count));

            //uint16_t* sptr = value;

            for (int i = 0; i < count; i++)
            {
                //xor_byte ^= (*sptr & 0xFF);
                xor_byte ^= (values[i] & 0xFF);
                writer.Write((int)xor_byte);
                //(void)putc((int)xor_byte, fileptr);
                //DEBUG(fprintf(logfile, "  %02X", (int)xor_byte));
                xor_byte ^= ((values[i] >> 8) & 0xFF);
                //xor_byte ^= ((*sptr++ >> 8) & 0xFF);
                writer.Write((int)xor_byte);
                //(void)putc((int)xor_byte, fileptr);
                //DEBUG(fprintf(logfile, " %02X = %d", (int)xor_byte, (int)sptr[-1]));
            }

            //DEBUG(fprintf(logfile, "\n"));
        }

        private void wrItem(BinaryWriter writer, ref uint xor_byte, Inventory_t item)
        {
            //DEBUG(fprintf(logfile, "ITEM:\n"));
            this.wrShort(writer, ref xor_byte, item.id);
            this.wrByte(writer, ref xor_byte, item.special_name_id);
            this.wrString(writer, ref xor_byte, item.inscription);
            this.wrLong(writer, ref xor_byte, item.flags);
            this.wrByte(writer, ref xor_byte, item.category_id);
            this.wrByte(writer, ref xor_byte, item.sprite);
            this.wrShort(writer, ref xor_byte, (uint)item.misc_use);
            this.wrLong(writer, ref xor_byte, (uint)item.cost);
            this.wrByte(writer, ref xor_byte, item.sub_category_id);
            this.wrByte(writer, ref xor_byte, item.items_count);
            this.wrShort(writer, ref xor_byte, item.weight);
            this.wrShort(writer, ref xor_byte, (uint)item.to_hit);
            this.wrShort(writer, ref xor_byte, (uint)item.to_damage);
            this.wrShort(writer, ref xor_byte, (uint)item.ac);
            this.wrShort(writer, ref xor_byte, (uint)item.to_ac);
            this.wrByte(writer, ref xor_byte, item.damage.dice);
            this.wrByte(writer, ref xor_byte, item.damage.sides);
            this.wrByte(writer, ref xor_byte, item.depth_first_found);
            this.wrByte(writer, ref xor_byte, item.identification);
        }

        private void wrMonster(BinaryWriter writer, ref uint xor_byte, Monster_t monster)
        {
            //DEBUG(fprintf(logfile, "MONSTER:\n"));
            this.wrShort(writer, ref xor_byte, (uint)monster.hp);
            this.wrShort(writer, ref xor_byte, (uint)monster.sleep_count);
            this.wrShort(writer, ref xor_byte, (uint)monster.speed);
            this.wrShort(writer, ref xor_byte, monster.creature_id);
            this.wrByte(writer, ref xor_byte, (uint)monster.pos.y);
            this.wrByte(writer, ref xor_byte, (uint)monster.pos.x);
            this.wrByte(writer, ref xor_byte, monster.distance_from_player);
            this.wrBool(writer, ref xor_byte, monster.lit);
            this.wrByte(writer, ref xor_byte, monster.stunned_amount);
            this.wrByte(writer, ref xor_byte, monster.confused_amount);
        }

        // get_byte reads a single byte from a file, without any xor_byte encryption
        private uint getByte(BinaryReader reader)
        {
            return (uint)(reader.ReadChar() & 0xFF);
            //return (uint)(getc(fileptr) & 0xFF);
        }

        private bool rdBool(BinaryReader reader, ref uint xor_byte)
        {
            return this.rdByte(reader, ref xor_byte) != 0;
            //return (bool)(rdByte() != 0);
        }

        private uint rdByte(BinaryReader reader, ref uint xor_byte)
        {
            var c = this.getByte(reader);
            uint decoded_byte = c ^ xor_byte;
            xor_byte = c;

            //DEBUG(fprintf(logfile, "BYTE:  %02X = %d\n", (int)c, decoded_byte));

            return decoded_byte;
        }

        private uint rdShort(BinaryReader reader, ref uint xor_byte)
        {
            var c = this.getByte(reader);
            uint decoded_int = c ^ xor_byte;

            xor_byte = this.getByte(reader);
            decoded_int |= (uint)(c ^ xor_byte) << 8;

            //DEBUG(fprintf(logfile, "SHORT: %02X %02X = %d\n", (int)c, (int)xor_byte, decoded_int));

            return decoded_int;
        }

        private uint rdLong(BinaryReader reader, ref uint xor_byte)
        {
            var c = this.getByte(reader);
            uint decoded_long = c ^ xor_byte;

            xor_byte = this.getByte(reader);
            decoded_long |= (uint)(c ^ xor_byte) << 8;
            //DEBUG(fprintf(logfile, "LONG:  %02X %02X ", (int)c, (int)xor_byte));



            c = this.getByte(reader);
            decoded_long |= (uint)(c ^ xor_byte) << 16;

            xor_byte = this.getByte(reader);
            decoded_long |= (uint)(c ^ xor_byte) << 24;
            //DEBUG(fprintf(logfile, "%02X %02X = %ld\n", (int)c, (int)xor_byte, decoded_long));

            return decoded_long;
        }

        private void rdBytes(BinaryReader reader, ref uint xor_byte, uint[] value, int count)
        {
            //DEBUG(fprintf(logfile, "%d BYTES:", count));
            //uint8_t* ptr = value;
            for (int i = 0; i < count; i++)
            {
                var c = this.getByte(reader);
                value[i] = c ^ xor_byte;
                //*ptr++ = c ^ xor_byte;
                xor_byte = c;
                //DEBUG(fprintf(logfile, "  %02X = %d", (int)c, (int)ptr[-1]));
            }

            //DEBUG(fprintf(logfile, "\n"));
        }

        private void rdString(BinaryReader reader, ref uint xor_byte, out string str)
        {
            //DEBUG(char * s = str);
            //DEBUG(fprintf(logfile, "STRING: "));

            var chars = new List<char>();
            do
            {
                var c = this.getByte(reader);
                chars.Add((char)(c ^ xor_byte));
                //*str = c ^ xor_byte;
                xor_byte = c;
                //DEBUG(fprintf(logfile, "%02X ", (int)c));
                //} while (*str++ != '\0');
            } while (chars.Last() != '\0');

            // remove '\0'
            chars.RemoveAt(chars.Count - 1);

            str = new string(chars.ToArray());

            //DEBUG(fprintf(logfile, "= \"%s\"\n", s));
        }

        private void rdShorts(BinaryReader reader, ref uint xor_byte, uint[] values, int count)
        {
            //DEBUG(fprintf(logfile, "%d SHORTS:", count));
            //uint16_t* sptr = value;

            for (int i = 0; i < count; i++)
            {
                var c = this.getByte(reader);
                uint s = c ^ xor_byte;
                xor_byte = this.getByte(reader);
                s |= (uint)(c ^ xor_byte) << 8;
                values[i] = s;
                //*sptr++ = s;
                //DEBUG(fprintf(logfile, "  %02X %02X = %d", (int)c, (int)xor_byte, (int)s));
            }

            //DEBUG(fprintf(logfile, "\n"));
        }

        private void rdItem(BinaryReader reader, ref uint xor_byte, Inventory_t item)
        {
            //DEBUG(fprintf(logfile, "ITEM:\n"));
            item.id = this.rdShort(reader, ref xor_byte);
            item.special_name_id = this.rdByte(reader, ref xor_byte);
            this.rdString(reader, ref xor_byte, out var inscription);
            item.inscription = inscription;
            item.flags = this.rdLong(reader, ref xor_byte);
            item.category_id = this.rdByte(reader, ref xor_byte);
            item.sprite = this.rdByte(reader, ref xor_byte);
            item.misc_use = (int)this.rdShort(reader, ref xor_byte);
            item.cost = (int)this.rdLong(reader, ref xor_byte);
            item.sub_category_id = this.rdByte(reader, ref xor_byte);
            item.items_count = this.rdByte(reader, ref xor_byte);
            item.weight = this.rdShort(reader, ref xor_byte);
            item.to_hit = (int)this.rdShort(reader, ref xor_byte);
            item.to_damage = (int)this.rdShort(reader, ref xor_byte);
            item.ac = (int)this.rdShort(reader, ref xor_byte);
            item.to_ac = (int)this.rdShort(reader, ref xor_byte);
            item.damage = new Dice_t(
                this.rdByte(reader, ref xor_byte),
                this.rdByte(reader, ref xor_byte)
            );
            item.depth_first_found = this.rdByte(reader, ref xor_byte);
            item.identification = this.rdByte(reader, ref xor_byte);
        }

        private void rdMonster(BinaryReader reader, ref uint xor_byte, Monster_t monster)
        {
            //DEBUG(fprintf(logfile, "MONSTER:\n"));
            monster.hp = (int)this.rdShort(reader, ref xor_byte);
            monster.sleep_count = (int)this.rdShort(reader, ref xor_byte);
            monster.speed = (int)this.rdShort(reader, ref xor_byte);
            monster.creature_id = this.rdShort(reader, ref xor_byte);
            monster.pos.y = (int)this.rdByte(reader, ref xor_byte);
            monster.pos.x = (int)this.rdByte(reader, ref xor_byte);
            monster.distance_from_player = this.rdByte(reader, ref xor_byte);
            monster.lit = this.rdBool(reader, ref xor_byte);
            monster.stunned_amount = this.rdByte(reader, ref xor_byte);
            monster.confused_amount = this.rdByte(reader, ref xor_byte);
        }

        // functions called from death.c to implement the score file

        // set the local fileptr to the score file fileptr
        //void setFileptr(FILE* file)
        //{
        //    fileptr = file;
        //}

        private void saveHighScore(BinaryWriter writer, ref uint xor_byte, HighScore_t score)
        {
            //DEBUG(logfile = fopen("IO_LOG", "a"));
            //DEBUG(fprintf(logfile, "Saving score:\n"));

            // Save the encryption byte for robustness.
            this.wrByte(writer, ref xor_byte, xor_byte);

            this.wrLong(writer, ref xor_byte, (uint)score.points);
            this.wrLong(writer, ref xor_byte, (uint)score.birth_date.Ticks);
            this.wrShort(writer, ref xor_byte, (uint)score.uid);
            this.wrShort(writer, ref xor_byte, (uint)score.mhp);
            this.wrShort(writer, ref xor_byte, (uint)score.chp);
            this.wrByte(writer, ref xor_byte, score.dungeon_depth);
            this.wrByte(writer, ref xor_byte, score.level);
            this.wrByte(writer, ref xor_byte, score.deepest_dungeon_depth);
            this.wrByte(writer, ref xor_byte, score.gender);
            this.wrByte(writer, ref xor_byte, score.race);
            this.wrByte(writer, ref xor_byte, score.character_class);
            // TOFIX: 2 lines below
            this.wrBytes(writer, ref xor_byte, score.name.Select(x => (uint)x).ToArray(), (int)Player_c.PLAYER_NAME_SIZE);
            this.wrBytes(writer, ref xor_byte, score.died_from.Select(x => (uint)x).ToArray(), 25);
            //DEBUG(fclose(logfile))
        }

        private void readHighScore(BinaryReader reader, HighScore_t score)
        {
            //DEBUG(logfile = fopen("IO_LOG", "a"));
            //DEBUG(fprintf(logfile, "Reading score:\n"));

            // Read the encryption byte.
            var xor_byte = this.getByte(reader);
            //xor_byte = getByte();

            score.points = (int)this.rdLong(reader, ref xor_byte);
            var ticks = (long)this.rdLong(reader, ref xor_byte);
            score.birth_date = new DateTime(ticks);
            score.uid = (int)this.rdShort(reader, ref xor_byte);
            score.mhp = (int)this.rdShort(reader, ref xor_byte);
            score.chp = (int)this.rdShort(reader, ref xor_byte);
            score.dungeon_depth = this.rdByte(reader, ref xor_byte);
            score.level = this.rdByte(reader, ref xor_byte);
            score.deepest_dungeon_depth = this.rdByte(reader, ref xor_byte);
            score.gender = this.rdByte(reader, ref xor_byte);
            score.race = this.rdByte(reader, ref xor_byte);
            score.character_class = this.rdByte(reader, ref xor_byte);
            // TOFIX: 2 lines below
            this.rdBytes(reader, ref xor_byte, score.name.Select(x => (uint)x).ToArray(), (int)Player_c.PLAYER_NAME_SIZE);
            this.rdBytes(reader, ref xor_byte, score.died_from.Select(x => (uint)x).ToArray(), 25);
            //DEBUG(fclose(logfile));
        }
    }
}
