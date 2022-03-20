using System;
using System.Collections.Generic;
using Moria.Core.Configs;
using Moria.Core.States;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Moria.Core.Constants;
using Moria.Core.Methods.Commands.Player;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using Moria.Core.Utils;

namespace Moria.Core.Methods
{
    public interface IGameSave
    {
        bool loadGame(out bool generate);

        bool saveGame();
    }

    public class Game_save_m : IGameSave
    {
        private readonly IEventPublisher eventPublisher;
        private readonly IFileSystem fileSystem;
        private readonly IGame game;
        private readonly IRnd rnd;
        private readonly IStoreInventory storeInventory;
        private readonly ITerminal terminal;
        private readonly IBinaryReaderWriterFactory binaryReaderWriterFactory;

        public Game_save_m(
            IEventPublisher eventPublisher,
            IFileSystem fileSystem,
            IGame game,
            IRnd rnd,
            IStoreInventory storeInventory,
            ITerminal terminal,
            IBinaryReaderWriterFactory binaryReaderWriterFactory
        )
        {
            this.eventPublisher = eventPublisher;
            this.fileSystem = fileSystem;
            this.game = game;
            this.rnd = rnd;
            this.storeInventory = storeInventory;
            this.terminal = terminal;
            this.binaryReaderWriterFactory = binaryReaderWriterFactory;
        }

        public bool loadGame(out bool generate)
        {
            return this.loadGame_old(out generate);
            //var saveGameContents = this.fileSystem.File.ReadAllText(Config.files.save_game);
            //var instance = JsonConvert.DeserializeObject<State>(saveGameContents);
            //State.Instance = instance;
            //return true;
        }

        public bool saveGame()
        {
            return this.saveGame_old();
            //var instance = State.Instance;
            //var saveGameContents = JsonConvert.SerializeObject(instance, Formatting.Indented);
            //File.WriteAllText(Config.files.save_game, saveGameContents);
            //return true;
        }

        /*

        // This save package was brought to by                -JWT-
        // and                                                -RAK-
        // and has been completely rewritten for UNIX by      -JEW-
        // and has been completely rewritten again by         -CJS-
        // and completely rewritten again! for portability by -JEW-

        // Set up prior to actual save, do the save, then clean up

        */
        private bool saveGame_old()
        {
            var input = string.Empty;
            //vtype_t input = { '\0' };
            string output;

            while (!this.saveChar(Config.files.save_game))
            {
                output = "Save file '" + Config.files.save_game + "' fails.";
                this.terminal.printMessage(output);

                //if (access(Config.files.save_game.c_str(), 0) < 0 || !this.terminal.getInputConfirmation("File exists. Delete old save file?") || (i = unlink(Config.files.save_game)) < 0)
                //{
                //    if (i < 0)
                //    {
                //        output = "Can't delete '" + Config.files.save_game + "'";
                //        this.terminal.printMessage(output);
                //    }
                //    this.terminal.putStringClearToEOL("New Save file [ESC to give up]:", new Coord_t(0, 0));
                //    if (!this.terminal.getStringInput(out input, new Coord_t(0, 31), 45))
                //    {
                //        return false;
                //    }
                //    if (input[0] != 0)
                //    {
                //        // (void) strcpy(Config.files.save_game, input);
                //        Config.files.save_game = input;
                //    }
                //}
                output = "Saving with '" + Config.files.save_game + "'...";
                this.terminal.putStringClearToEOL(output, new Coord_t(0, 0));

                return false;
            }

            return true;
        }

        private bool svWrite(IBinaryWriter writer, ref uint xor_byte)
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

            for (int i = 0; i < Monster_c.MON_MAX_CREATURES; i++)
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
                    this.wrShort(writer, ref xor_byte, (uint)i);
                    this.wrLong(writer, ref xor_byte, r.movement);
                    this.wrLong(writer, ref xor_byte, r.spells);
                    this.wrShort(writer, ref xor_byte, r.kills);
                    this.wrShort(writer, ref xor_byte, r.deaths);
                    this.wrShort(writer, ref xor_byte, r.defenses);
                    this.wrByte(writer, ref xor_byte, r.wake);
                    this.wrByte(writer, ref xor_byte, r.ignore);
                    this.wrBytes(writer, ref xor_byte, r.attacks, (int)Monster_c.MON_MAX_ATTACKS);
                }
            }

            // sentinel to indicate no more monster info
            this.wrShort(writer, ref xor_byte, (uint)0xFFFF);

            this.wrLong(writer, ref xor_byte, l);

            this.wrString(writer, ref xor_byte, py.misc.name);
            this.wrBool(writer, ref xor_byte, py.misc.gender);
            this.wrLong(writer, ref xor_byte, (uint)py.misc.au);
            this.wrLong(writer, ref xor_byte, (uint)py.misc.max_exp);
            this.wrLong(writer, ref xor_byte, (uint)py.misc.exp);
            this.wrShort(writer, ref xor_byte, py.misc.exp_fraction);
            this.wrShort(writer, ref xor_byte, py.misc.age);
            this.wrShort(writer, ref xor_byte, py.misc.height);
            this.wrShort(writer, ref xor_byte, py.misc.weight);
            this.wrShort(writer, ref xor_byte, py.misc.level);
            this.wrShort(writer, ref xor_byte, py.misc.max_dungeon_depth);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.chance_in_search);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.fos);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.bth);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.bth_with_bows);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.mana);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.max_hp);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.plusses_to_hit);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.plusses_to_damage);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.ac);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.magical_ac);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.display_to_hit);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.display_to_damage);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.display_ac);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.display_to_ac);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.disarm);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.saving_throw);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.social_class);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.stealth_factor);
            this.wrByte(writer, ref xor_byte, py.misc.class_id);
            this.wrByte(writer, ref xor_byte, py.misc.race_id);
            this.wrByte(writer, ref xor_byte, py.misc.hit_die);
            this.wrByte(writer, ref xor_byte, py.misc.experience_factor);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.current_mana);
            this.wrShort(writer, ref xor_byte, py.misc.current_mana_fraction);
            this.wrShort(writer, ref xor_byte, (uint)py.misc.current_hp);
            this.wrShort(writer, ref xor_byte, py.misc.current_hp_fraction);
            foreach (var entry in py.misc.history)
            {
                this.wrString(writer, ref xor_byte, entry);
            }

            this.wrBytes(writer, ref xor_byte, py.stats.max, 6);
            this.wrBytes(writer, ref xor_byte, py.stats.current, 6);
            this.wrShorts(writer, ref xor_byte, py.stats.modified, 6);
            this.wrBytes(writer, ref xor_byte, py.stats.used, 6);

            this.wrLong(writer, ref xor_byte, py.flags.status);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.rest);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.blind);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.paralysis);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.confused);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.food);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.food_digested);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.protection);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.speed);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.fast);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.slow);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.afraid);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.poisoned);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.image);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.protect_evil);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.invulnerability);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.heroism);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.super_heroism);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.blessed);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.heat_resistance);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.cold_resistance);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.detect_invisible);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.word_of_recall);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.see_infra);
            this.wrShort(writer, ref xor_byte, (uint)py.flags.timed_infra);
            this.wrBool(writer, ref xor_byte, py.flags.see_invisible);
            this.wrBool(writer, ref xor_byte, py.flags.teleport);
            this.wrBool(writer, ref xor_byte, py.flags.free_action);
            this.wrBool(writer, ref xor_byte, py.flags.slow_digest);
            this.wrBool(writer, ref xor_byte, py.flags.aggravate);
            this.wrBool(writer, ref xor_byte, py.flags.resistant_to_fire);
            this.wrBool(writer, ref xor_byte, py.flags.resistant_to_cold);
            this.wrBool(writer, ref xor_byte, py.flags.resistant_to_acid);
            this.wrBool(writer, ref xor_byte, py.flags.regenerate_hp);
            this.wrBool(writer, ref xor_byte, py.flags.resistant_to_light);
            this.wrBool(writer, ref xor_byte, py.flags.free_fall);
            this.wrBool(writer, ref xor_byte, py.flags.sustain_str);
            this.wrBool(writer, ref xor_byte, py.flags.sustain_int);
            this.wrBool(writer, ref xor_byte, py.flags.sustain_wis);
            this.wrBool(writer, ref xor_byte, py.flags.sustain_con);
            this.wrBool(writer, ref xor_byte, py.flags.sustain_dex);
            this.wrBool(writer, ref xor_byte, py.flags.sustain_chr);
            this.wrBool(writer, ref xor_byte, py.flags.confuse_monster);
            this.wrByte(writer, ref xor_byte, py.flags.new_spells_to_learn);

            this.wrShort(writer, ref xor_byte, (uint)State.Instance.missiles_counter);
            this.wrLong(writer, ref xor_byte, (uint)State.Instance.dg.game_turn);
            this.wrShort(writer, ref xor_byte, (uint)py.pack.unique_items);
            for (int i = 0; i < py.pack.unique_items; i++)
            {
                this.wrItem(writer, ref xor_byte, py.inventory[i]);
            }
            for (int i = (int)PlayerEquipment.Wield; i < Inventory_c.PLAYER_INVENTORY_SIZE; i++)
            {
                this.wrItem(writer, ref xor_byte, py.inventory[i]);
            }

            this.wrShort(writer, ref xor_byte, (uint)py.pack.weight);
            this.wrShort(writer, ref xor_byte, (uint)py.equipment_count);
            this.wrLong(writer, ref xor_byte, py.flags.spells_learnt);
            this.wrLong(writer, ref xor_byte, py.flags.spells_worked);
            this.wrLong(writer, ref xor_byte, py.flags.spells_forgotten);
            this.wrBytes(writer, ref xor_byte, py.flags.spells_learned_order, 32);
            this.wrBytes(writer, ref xor_byte, State.Instance.objects_identified, (int)Game_c.OBJECT_IDENT_SIZE);
            this.wrLong(writer, ref xor_byte, game.magic_seed);
            this.wrLong(writer, ref xor_byte, game.town_seed);
            this.wrShort(writer, ref xor_byte, (uint)State.Instance.last_message_id);
            foreach (var message in State.Instance.messages)
            {
                this.wrString(writer, ref xor_byte, message);
            }

            // this indicates 'cheating' if it is a one
            this.wrShort(writer, ref xor_byte, (uint)(State.Instance.panic_save ? 1 : 0));
            this.wrShort(writer, ref xor_byte, (uint)(game.total_winner ? 1 : 0));
            this.wrShort(writer, ref xor_byte, (uint)game.noscore);
            this.wrShorts(writer, ref xor_byte, py.base_hp_levels, (int)Player_c.PLAYER_MAX_LEVEL);

            foreach (var store in State.Instance.stores)
            {
                this.wrLong(writer, ref xor_byte, (uint)store.turns_left_before_closing);
                this.wrShort(writer, ref xor_byte, (uint)store.insults_counter);
                this.wrByte(writer, ref xor_byte, store.owner_id);
                this.wrByte(writer, ref xor_byte, store.unique_items_counter);
                this.wrShort(writer, ref xor_byte, store.good_purchases);
                this.wrShort(writer, ref xor_byte, store.bad_purchases);
                for (int j = 0; j < store.unique_items_counter; j++)
                {
                    this.wrLong(writer, ref xor_byte, (uint)store.inventory[j].cost);
                    this.wrItem(writer, ref xor_byte, store.inventory[j].item);
                }
            }

            // save the current time in the save file
            l = (uint)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            //l = (uint)DateTime.Now.Ticks;


            if (l < (uint)(State.Instance.start_time - new DateTime(1970, 1, 1)).TotalSeconds)
            {
                // someone is messing with the clock!,
                // assume that we have been playing for 1 day
                l = (uint) (State.Instance.start_time.AddDays(1) - new DateTime(1970, 1, 1)).TotalSeconds;
                //l = (uint)State.Instance.start_time.AddDays(1).Ticks;
                //l = (uint)(start_time + 86400L);
            }

            this.wrLong(writer, ref xor_byte, l);

            // put game.character_died_from string in save file
            this.wrString(writer, ref xor_byte, game.character_died_from);

            // put the max_score in the save file
            l = (uint)(Scores_m.playerCalculateTotalPoints());
            this.wrLong(writer, ref xor_byte, l);

            // put the date_of_birth in the save file
            this.wrLong(writer, ref xor_byte, (uint)py.misc.date_of_birth.Ticks);

            // only level specific info follows, this allows characters to be
            // resurrected, the dungeon level info is not needed for a resurrection
            if (game.character_is_dead)
            {
                return true;
                //return !((ferror(fileptr) != 0) || fflush(fileptr) == EOF);
            }

            var dg = State.Instance.dg;
            this.wrShort(writer, ref xor_byte, (uint)dg.current_level);
            this.wrShort(writer, ref xor_byte, (uint)py.pos.y);
            this.wrShort(writer, ref xor_byte, (uint)py.pos.x);
            this.wrShort(writer, ref xor_byte, (uint)State.Instance.monster_multiply_total);
            this.wrShort(writer, ref xor_byte, (uint)dg.height);
            this.wrShort(writer, ref xor_byte, (uint)dg.width);
            this.wrShort(writer, ref xor_byte, (uint)dg.panel.max_rows);
            this.wrShort(writer, ref xor_byte, (uint)dg.panel.max_cols);

            for (int i = 0; i < Dungeon_c.MAX_HEIGHT; i++)
            {
                for (int j = 0; j < Dungeon_c.MAX_WIDTH; j++)
                {
                    if (dg.floor[i][j].creature_id != 0)
                    {
                        this.wrByte(writer, ref xor_byte, (uint)i);
                        this.wrByte(writer, ref xor_byte, (uint)j);
                        this.wrByte(writer, ref xor_byte, dg.floor[i][j].creature_id);
                    }
                }
            }

            // marks end of creature_id info
            this.wrByte(writer, ref xor_byte, (uint)0xFF);

            for (int i = 0; i < Dungeon_c.MAX_HEIGHT; i++)
            {
                for (int j = 0; j < Dungeon_c.MAX_WIDTH; j++)
                {
                    if (dg.floor[i][j].treasure_id != 0)
                    {
                        this.wrByte(writer, ref xor_byte, (uint)i);
                        this.wrByte(writer, ref xor_byte, (uint)j);
                        this.wrByte(writer, ref xor_byte, dg.floor[i][j].treasure_id);
                    }
                }
            }

            // marks end of treasure_id info
            this.wrByte(writer, ref xor_byte, (uint)0xFF);

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

                    if (char_tmp != prev_char || count == Monster_m.UCHAR_MAX)
                    {
                        this.wrByte(writer, ref xor_byte, (uint)count);
                        this.wrByte(writer, ref xor_byte, prev_char);
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
            this.wrByte(writer, ref xor_byte, (uint)count);
            this.wrByte(writer, ref xor_byte, prev_char);

            this.wrShort(writer, ref xor_byte, (uint)game.treasure.current_id);
            for (int i = (int)Config.treasure.MIN_TREASURE_LIST_ID; i < game.treasure.current_id; i++)
            {
                this.wrItem(writer, ref xor_byte, game.treasure.list[i]);
            }

            this.wrShort(writer, ref xor_byte, (uint)State.Instance.next_free_monster_id);
            for (int i = (int)Config.monsters.MON_MIN_INDEX_ID; i < State.Instance.next_free_monster_id; i++)
            {
                this.wrMonster(writer, ref xor_byte, State.Instance.monsters[i]);
            }

            return true;
            //return !((ferror(fileptr) != 0) || fflush(fileptr) == EOF);
        }

        private bool saveChar(string filename)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            if (game.character_saved)
            {
                return true; // Nothing to save.
            }

            this.terminal.putQIO();
            this.eventPublisher.Publish(new DisturbCommand(true, false));
            //playerDisturb(1, 0);                   // Turn off resting and searching.
            Player_m.playerChangeSpeed(-py.pack.heaviness); // Fix the speed
            py.pack.heaviness = 0;
            bool ok;
            try
            {
                var writeStream = new FileStream(filename, FileMode.Create);
                var writer = this.binaryReaderWriterFactory.CreateBinaryWriter(writeStream);// new BinaryWriter(writeStream);
                //fileptr = nullptr; // Do not assume it has been init'ed

                //int fd = open(filename, O_RDWR | O_CREAT | O_EXCL, 0600);

                //if (fd < 0 && access(filename, 0) >= 0 && ((from_save_file != 0) || (game.wizard_mode && getInputConfirmation("Can't make new save file. Overwrite old?"))))
                //{
                //    (void)chmod(filename.c_str(), 0600);
                //    fd = open(filename.c_str(), O_RDWR | O_TRUNC, 0600);
                //}

                //if (fd >= 0)
                //{
                //    (void)close(fd);
                //    fileptr = fopen(Config.files.save_game, "wb");
                //}

                //DEBUG(logfile = fopen("IO_LOG", "a"));
                //DEBUG(fprintf(logfile, "Saving data to %s\n", Config.files.save_game));

                //if (fileptr != nullptr)
                //{
                var xor_byte = 0u;
                this.wrByte(writer, ref xor_byte, Version_c.CURRENT_VERSION_MAJOR);
                xor_byte = 0;
                this.wrByte(writer, ref xor_byte, Version_c.CURRENT_VERSION_MINOR);
                xor_byte = 0;
                this.wrByte(writer, ref xor_byte, Version_c.CURRENT_VERSION_PATCH);
                xor_byte = 0;

                var char_tmp = (uint)(this.rnd.randomNumber(256) - 1);
                this.wrByte(writer, ref xor_byte, char_tmp);
                // Note that xor_byte is now equal to char_tmp

                this.svWrite(writer, ref xor_byte);

                //DEBUG(fclose(logfile));

                //if (fclose(fileptr) == EOF)
                //{
                //    ok = false;
                //}
                //}

                //if (!ok)
                //{
                //    if (fd >= 0)
                //    {
                //        (void)unlink(filename.c_str());
                //    }
                //
                //    std::string output;
                //    if (fd >= 0)
                //    {
                //        output = "Error writing to file '" + filename + "'";
                //    }
                //    else
                //    {
                //        output = "Can't create new file '" + filename + "'";
                //    }
                //    printMessage(output.c_str());
                //
                //    return false;
                //}

            }
            catch
            {
                return false;
            }

            game.character_saved = true;
            State.Instance.dg.game_turn = -1;

            return true;
        }

        // Certain checks are omitted for the wizard. -CJS-
        private bool loadGame_old(out bool generate)
        {
            Tile_t tile;
            //Tile_t* tile = nullptr;
            uint time_saved = 0;
            uint version_maj;
            uint version_min;
            uint patch_level;

            generate = true;
            int total_count;

            if (!this.fileSystem.File.Exists(Config.files.save_game))
            {
                this.terminal.printMessage("Save file does not exist.");
                return false;
            }
            //// Not required for Mac, because the file name is obtained through a dialog.
            //// There is no way for a nonexistent file to be specified. -BS-
            //if (access(Config.files.save_game.c_str(), 0) != 0)
            //{
            //    printMessage("Save file does not exist.");
            //    return false; // Don't bother with messages here. File absent.
            //}

            this.terminal.clearScreen();
            //clearScreen();

            var filename = $"Save file '{Config.files.save_game}' present. Attempting restore.";
            //std::string filename = "Save file '" + Config.files.save_game + "' present. Attempting restore.";
            this.terminal.putString(filename, new Coord_t(23, 0));
            //putString(filename.c_str(), new Coord_t(23, 0));

            var dg = State.Instance.dg;
            var py = State.Instance.py;
            // FIXME: check this if/else logic! -- MRC
                dg.game_turn = -1;
            if (dg.game_turn >= 0)
            {
                this.terminal.printMessage("IMPOSSIBLE! Attempt to restore while still alive!");
                //printMessage("IMPOSSIBLE! Attempt to restore while still alive!");
            }
            //else if ((fd = open(Config.files.save_game.c_str(), O_RDONLY, 0)) < 0 &&
            //         (chmod(Config.files.save_game.c_str(), 0400) < 0 || (fd = open(Config.files.save_game.c_str(), O_RDONLY, 0)) < 0))
            //{
            //    // Allow restoring a file belonging to someone else, if we can delete it.
            //    // Hence first try to read without doing a chmod.

            //    printMessage("Can't open file for reading.");
            //}
            else
            {
                dg.game_turn = -1;
                bool ok = true;

                //(void)close(fd);
                //fd = -1; // Make sure it isn't closed again
                var readerStream = this.fileSystem.File.Open(Config.files.save_game, FileMode.Open);
                var reader = this.binaryReaderWriterFactory.CreateBinaryReader(readerStream);// new BinaryReader(readerStream);
                //fileptr = fopen(Config.files.save_game.c_str(), "rb");
                //
                //if (fileptr == nullptr)
                //{
                //    goto error;
                //}

                this.terminal.putStringClearToEOL("Restoring Memory...", new Coord_t(0, 0));
                //putStringClearToEOL("Restoring Memory...", new Coord_t(0, 0));
                this.terminal.putQIO();

                //DEBUG(logfile = fopen("IO_LOG", "a"));
                //DEBUG(fprintf(logfile, "Reading data from %s\n", Config.files.save_game));
                uint xor_byte;

                // Note: setting these xor_byte is correct!
                xor_byte = 0;
                version_maj = this.rdByte(reader, ref xor_byte);
                xor_byte = 0;
                version_min = this.rdByte(reader, ref xor_byte);
                xor_byte = 0;
                patch_level = this.rdByte(reader, ref xor_byte);

                xor_byte = this.getByte(reader);

                if (!this.validGameVersion(version_maj, version_min, patch_level))
                {
                    this.terminal.putStringClearToEOL("Sorry. This save file is from a different version of umoria.", new Coord_t(2, 0));
                    goto error;
                }

                uint uint_16_t_tmp;
                uint l;
                string str;

                uint_16_t_tmp = this.rdShort(reader, ref xor_byte);
                while (uint_16_t_tmp != 0xFFFF)
                {
                    if (uint_16_t_tmp >= Monster_c.MON_MAX_CREATURES)
                    {
                        goto error;
                    }
                    Recall_t memory = State.Instance.creature_recall[uint_16_t_tmp];
                    memory.movement = this.rdLong(reader, ref xor_byte);
                    memory.spells = this.rdLong(reader, ref xor_byte);
                    memory.kills = this.rdShort(reader, ref xor_byte);
                    memory.deaths = this.rdShort(reader, ref xor_byte);
                    memory.defenses = this.rdShort(reader, ref xor_byte);
                    memory.wake = this.rdByte(reader, ref xor_byte);
                    memory.ignore = this.rdByte(reader, ref xor_byte);
                    this.rdBytes(reader, ref xor_byte, memory.attacks, (int)Monster_c.MON_MAX_ATTACKS);
                    uint_16_t_tmp = this.rdShort(reader, ref xor_byte);
                }

                l = this.rdLong(reader, ref xor_byte);

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
                if (State.Instance.game.to_be_wizard && ((l & 0x40000000L) != 0))
                {
                    this.terminal.printMessage("Sorry, this character is retired from moria.");
                    this.terminal.printMessage("You can not resurrect a retired character.");
                }
                else if (State.Instance.game.to_be_wizard && ((l & 0x80000000L) != 0) && this.terminal.getInputConfirmation("Resurrect a dead character?"))
                {
                    l &= ~0x80000000;
                }

                if ((l & 0x80000000u) == 0)
                {
                    this.rdString(reader, ref xor_byte, out str);
                    py.misc.name = str;
                    py.misc.gender = this.rdBool(reader, ref xor_byte);
                    py.misc.au = (int)this.rdLong(reader, ref xor_byte);
                    py.misc.max_exp = (int)this.rdLong(reader, ref xor_byte);
                    py.misc.exp = (int)this.rdLong(reader, ref xor_byte);
                    py.misc.exp_fraction = this.rdShort(reader, ref xor_byte);
                    py.misc.age = this.rdShort(reader, ref xor_byte);
                    py.misc.height = this.rdShort(reader, ref xor_byte);
                    py.misc.weight = this.rdShort(reader, ref xor_byte);
                    py.misc.level = this.rdShort(reader, ref xor_byte);
                    py.misc.max_dungeon_depth = this.rdShort(reader, ref xor_byte);
                    py.misc.chance_in_search = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.fos = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.bth = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.bth_with_bows = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.mana = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.max_hp = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.plusses_to_hit = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.plusses_to_damage = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.ac = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.magical_ac = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.display_to_hit = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.display_to_damage = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.display_ac = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.display_to_ac = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.disarm = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.saving_throw = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.social_class = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.stealth_factor = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.class_id = this.rdByte(reader, ref xor_byte);
                    py.misc.race_id = this.rdByte(reader, ref xor_byte);
                    py.misc.hit_die = this.rdByte(reader, ref xor_byte);
                    py.misc.experience_factor = this.rdByte(reader, ref xor_byte);
                    py.misc.current_mana = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.current_mana_fraction = this.rdShort(reader, ref xor_byte);
                    py.misc.current_hp = (int)this.rdShort(reader, ref xor_byte);
                    py.misc.current_hp_fraction = this.rdShort(reader, ref xor_byte);
                    for (int i = 0; i < py.misc.history.Length; i++)
                    {
                        this.rdString(reader, ref xor_byte, out str);
                        py.misc.history[i] = str;
                    }
                    //for (auto & entry : py.misc.history)
                    //{
                    //    rdString(entry);
                    //}

                    this.rdBytes(reader, ref xor_byte, py.stats.max, 6);
                    this.rdBytes(reader, ref xor_byte, py.stats.current, 6);
                    this.rdShorts(reader, ref xor_byte, py.stats.modified, 6);
                    this.rdBytes(reader, ref xor_byte, py.stats.used, 6);

                    py.flags.status = this.rdLong(reader, ref xor_byte);
                    py.flags.rest = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.blind = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.paralysis = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.confused = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.food = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.food_digested = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.protection = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.speed = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.fast = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.slow = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.afraid = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.poisoned = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.image = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.protect_evil = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.invulnerability = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.heroism = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.super_heroism = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.blessed = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.heat_resistance = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.cold_resistance = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.detect_invisible = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.word_of_recall = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.see_infra = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.timed_infra = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.see_invisible = this.rdBool(reader, ref xor_byte);
                    py.flags.teleport = this.rdBool(reader, ref xor_byte);
                    py.flags.free_action = this.rdBool(reader, ref xor_byte);
                    py.flags.slow_digest = this.rdBool(reader, ref xor_byte);
                    py.flags.aggravate = this.rdBool(reader, ref xor_byte);
                    py.flags.resistant_to_fire = this.rdBool(reader, ref xor_byte);
                    py.flags.resistant_to_cold = this.rdBool(reader, ref xor_byte);
                    py.flags.resistant_to_acid = this.rdBool(reader, ref xor_byte);
                    py.flags.regenerate_hp = this.rdBool(reader, ref xor_byte);
                    py.flags.resistant_to_light = this.rdBool(reader, ref xor_byte);
                    py.flags.free_fall = this.rdBool(reader, ref xor_byte);
                    py.flags.sustain_str = this.rdBool(reader, ref xor_byte);
                    py.flags.sustain_int = this.rdBool(reader, ref xor_byte);
                    py.flags.sustain_wis = this.rdBool(reader, ref xor_byte);
                    py.flags.sustain_con = this.rdBool(reader, ref xor_byte);
                    py.flags.sustain_dex = this.rdBool(reader, ref xor_byte);
                    py.flags.sustain_chr = this.rdBool(reader, ref xor_byte);
                    py.flags.confuse_monster = this.rdBool(reader, ref xor_byte);
                    py.flags.new_spells_to_learn = this.rdByte(reader, ref xor_byte);

                    State.Instance.missiles_counter = (int)this.rdShort(reader, ref xor_byte);
                    dg.game_turn = (int)this.rdLong(reader, ref xor_byte);
                    py.pack.unique_items = (int)this.rdShort(reader, ref xor_byte);
                    if (py.pack.unique_items > (int)PlayerEquipment.Wield)
                    {
                        goto error;
                    }
                    for (int i = 0; i < py.pack.unique_items; i++)
                    {
                        this.rdItem(reader, ref xor_byte, py.inventory[i]);
                    }
                    for (int i = (int)PlayerEquipment.Wield; i < Inventory_c.PLAYER_INVENTORY_SIZE; i++)
                    {
                        this.rdItem(reader, ref xor_byte, py.inventory[i]);
                    }
                    py.pack.weight = (int)this.rdShort(reader, ref xor_byte);
                    py.equipment_count = (int)this.rdShort(reader, ref xor_byte);
                    py.flags.spells_learnt = this.rdLong(reader, ref xor_byte);
                    py.flags.spells_worked = this.rdLong(reader, ref xor_byte);
                    py.flags.spells_forgotten = this.rdLong(reader, ref xor_byte);
                    this.rdBytes(reader, ref xor_byte, py.flags.spells_learned_order, 32);
                    this.rdBytes(reader, ref xor_byte, State.Instance.objects_identified, (int)Game_c.OBJECT_IDENT_SIZE);
                    State.Instance.game.magic_seed = this.rdLong(reader, ref xor_byte);
                    State.Instance.game.town_seed = this.rdLong(reader, ref xor_byte);
                    State.Instance.last_message_id = (int)this.rdShort(reader, ref xor_byte);
                    for (int i = 0; i < State.Instance.messages.Length; i++)
                    {
                        this.rdString(reader, ref xor_byte, out str);
                        State.Instance.messages[i] = str;
                    }

                    uint panic_save_short;
                    uint total_winner_short;
                    panic_save_short = this.rdShort(reader, ref xor_byte);
                    total_winner_short = this.rdShort(reader, ref xor_byte);
                    State.Instance.panic_save = panic_save_short != 0;
                    State.Instance.game.total_winner = total_winner_short != 0;

                    State.Instance.game.noscore = (int)this.rdShort(reader, ref xor_byte);
                    this.rdShorts(reader, ref xor_byte, py.base_hp_levels, (int)Player_c.PLAYER_MAX_LEVEL);

                    foreach (var store in State.Instance.stores)
                    //for (auto & store : stores)
                    {
                        store.turns_left_before_closing = (int)this.rdLong(reader, ref xor_byte);
                        store.insults_counter = (int)this.rdShort(reader, ref xor_byte);
                        store.owner_id = this.rdByte(reader, ref xor_byte);
                        store.unique_items_counter = this.rdByte(reader, ref xor_byte);
                        store.good_purchases = this.rdShort(reader, ref xor_byte);
                        store.bad_purchases = this.rdShort(reader, ref xor_byte);
                        if (store.unique_items_counter > Store_c.STORE_MAX_DISCRETE_ITEMS)
                        {
                            goto error;
                        }
                        for (int j = 0; j < store.unique_items_counter; j++)
                        {
                            store.inventory[j].cost = (int)this.rdLong(reader, ref xor_byte);
                            this.rdItem(reader, ref xor_byte, store.inventory[j].item);
                        }
                    }

                    time_saved = this.rdLong(reader, ref xor_byte);
                    this.rdString(reader, ref xor_byte, out str);
                    State.Instance.game.character_died_from = str;
                    py.max_score = (int)this.rdLong(reader, ref xor_byte);
                    py.misc.date_of_birth = new DateTime(this.rdLong(reader, ref xor_byte));
                }

                var isEof = reader.IsEof();
                //var isEof = reader.BaseStream.Position == reader.BaseStream.Length;
                //c = reader.ReadChar();
                //c = getc(fileptr);
                if (isEof || ((l & 0x80000000L) != 0))
                //if (c == EOF || ((l & 0x80000000L) != 0))
                {
                    if ((l & 0x80000000L) == 0)
                    {
                        if (!State.Instance.game.to_be_wizard || dg.game_turn < 0)
                        {
                            goto error;
                        }
                        this.terminal.putStringClearToEOL("Attempting a resurrection!", new Coord_t(0, 0));
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
                        State.Instance.game.character_generated = true;

                        // set `noscore` to indicate a resurrection, and don't enter wizard mode
                        State.Instance.game.to_be_wizard = false;
                        State.Instance.game.noscore |= 0x1;
                    }
                    else
                    {
                        // Make sure that this message is seen, since it is a bit
                        // more interesting than the other messages.
                        this.terminal.printMessage("Restoring Memory of a departed spirit...");
                        dg.game_turn = -1;
                    }
                    this.terminal.putQIO();
                    goto closefiles;
                }
                //if (ungetc(c, fileptr) == EOF)
                //{
                //    goto error;
                //}

                this.terminal.putStringClearToEOL("Restoring Character...", new Coord_t(0, 0));
                this.terminal.putQIO();

                // only level specific info should follow,
                // not present for dead characters

                dg.current_level = (int)this.rdShort(reader, ref xor_byte);
                py.pos.y = (int)this.rdShort(reader, ref xor_byte);
                py.pos.x = (int)this.rdShort(reader, ref xor_byte);
                State.Instance.monster_multiply_total = (int)this.rdShort(reader, ref xor_byte);
                dg.height = (int)this.rdShort(reader, ref xor_byte);
                dg.width = (int)this.rdShort(reader, ref xor_byte);
                dg.panel.max_rows = (int)this.rdShort(reader, ref xor_byte);
                dg.panel.max_cols = (int)this.rdShort(reader, ref xor_byte);

                uint char_tmp, ychar, xchar, count;

                // read in the creature ptr info
                char_tmp = this.rdByte(reader, ref xor_byte);
                while (char_tmp != 0xFF)
                {
                    ychar = char_tmp;
                    xchar = this.rdByte(reader, ref xor_byte);
                    char_tmp = this.rdByte(reader, ref xor_byte);
                    if (xchar > Dungeon_c.MAX_WIDTH || ychar > Dungeon_c.MAX_HEIGHT)
                    {
                        goto error;
                    }
                    dg.floor[ychar][xchar].creature_id = char_tmp;
                    char_tmp = this.rdByte(reader, ref xor_byte);
                }

                // read in the treasure ptr info
                char_tmp = this.rdByte(reader, ref xor_byte);
                while (char_tmp != 0xFF)
                {
                    ychar = char_tmp;
                    xchar = this.rdByte(reader, ref xor_byte);
                    char_tmp = this.rdByte(reader, ref xor_byte);
                    if (xchar > Dungeon_c.MAX_WIDTH || ychar > Dungeon_c.MAX_HEIGHT)
                    {
                        goto error;
                    }
                    dg.floor[ychar][xchar].treasure_id = char_tmp;
                    char_tmp = this.rdByte(reader, ref xor_byte);
                }

                // read in the rest of the cave info
                var col = 0;
                var row = 0;
                //tile = &dg.floor[0][0];
                total_count = 0;
                while (total_count != Dungeon_c.MAX_HEIGHT * Dungeon_c.MAX_WIDTH)
                {
                    count = this.rdByte(reader, ref xor_byte);
                    char_tmp = this.rdByte(reader, ref xor_byte);
                    for (int i = (int)count; i > 0; i--)
                    {
                        //if (tile >= &dg.floor[Dungeon_c.MAX_HEIGHT][0])
                        if (row >= Dungeon_c.MAX_HEIGHT)
                        {
                            goto error;
                        }

                        tile = State.Instance.dg.floor[row][col];
                        tile.feature_id = (uint)(char_tmp & 0xF);
                        tile.perma_lit_room = ((char_tmp >> 4) & 0x1) != 0;
                        tile.field_mark = ((char_tmp >> 5) & 0x1) != 0;
                        tile.permanent_light = ((char_tmp >> 6) & 0x1) != 0;
                        tile.temporary_light = ((char_tmp >> 7) & 0x1) != 0;
                        
                        col++;
                        if (col >= Dungeon_c.MAX_WIDTH)
                        {
                            col = 0;
                            row++;
                        }
                    }
                    total_count += (int)count;
                }

                State.Instance.game.treasure.current_id = (int) this.rdShort(reader, ref xor_byte);
                if (State.Instance.game.treasure.current_id > Game_c.LEVEL_MAX_OBJECTS)
                {
                    goto error;
                }
                for (int i = (int)Config.treasure.MIN_TREASURE_LIST_ID; i < State.Instance.game.treasure.current_id; i++)
                {
                    this.rdItem(reader, ref xor_byte, State.Instance.game.treasure.list[i]);
                }
                State.Instance.next_free_monster_id = (int) this.rdShort(reader, ref xor_byte);
                if (State.Instance.next_free_monster_id > Monster_c.MON_TOTAL_ALLOCATIONS)
                {
                    goto error;
                }
                for (int i = (int)Config.monsters.MON_MIN_INDEX_ID; i < State.Instance.next_free_monster_id; i++)
                {
                    this.rdMonster(reader, ref xor_byte, State.Instance.monsters[i]);
                }

                generate = false; // We have restored a cave - no need to generate.

                //if (ferror(fileptr) != 0)
                //{
                //    goto error;
                //}

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
                        State.Instance.game.character_died_from = "(alive and well)";
                        //(void)strcpy(this.game.character_died_from, "(alive and well)");
                    }

                    State.Instance.game.character_generated = true;
                }

                goto closefiles;

            error:
                ok = false; // Assume bad data.

            closefiles:

            //DEBUG(fclose(logfile));

            //if (fileptr != nullptr)
            //{
            //    if (fclose(fileptr) < 0)
            //    {
            //        ok = false;
            //    }
            //}
                reader.Close();
                //if (fd >= 0)
                //{
                //    (void)close(fd);
                //}

                if (!ok)
                {
                    this.terminal.printMessage("Error during reading of file.");
                }
                else
                {
                    // let the user overwrite the old save file when save/quit
                    State.Instance.from_save_file = 1;

                    if (State.Instance.panic_save)
                    {
                        this.terminal.printMessage("This game is from a panic save.  Score will not be added to scoreboard.");
                    }
                    //else if ((!State.Instance.game.noscore) & 0x04)
                    //{
                    //    this.terminal.printMessage("This character is already on the scoreboard; it will not be scored again.");
                    //    State.Instance.game.noscore |= 0x4;
                    //}

                    if (dg.game_turn >= 0)
                    { // Only if a full restoration.
                        py.weapon_is_heavy = false;
                        py.pack.heaviness = 0;
                        Player_m.playerStrength();

                        // rotate store inventory, depending on how old the save file
                        // is foreach day old (rounded up), call storeMaintenance
                        // calculate age in seconds
                        State.Instance.start_time = DateTime.Now;
                        //start_time = getCurrentUnixTime();

                        uint age;

                        var timeSavedDateTime = new DateTime(1970, 1, 1).AddSeconds(time_saved);

                        // check for reasonable values of time here ...
                        if (State.Instance.start_time < timeSavedDateTime)
                        //if (State.Instance.start_time.Ticks < time_saved)
                        {
                            age = 0;
                        }
                        else
                        {
                            age = (uint)(State.Instance.start_time - timeSavedDateTime).Days;
                        }

                        //age = (uint32_t)((age + 43200L) / 86400L); // age in days
                        if (age > 10)
                        {
                            age = 10; // in case save file is very old
                        }

                        for (int i = 0; i < (int)age; i++)
                        {
                            this.storeInventory.storeMaintenance();
                        }
                    }

                    if (State.Instance.game.noscore != 0)
                    {
                        this.terminal.printMessage("This save file cannot be used to get on the score board.");
                    }
                    if (this.validGameVersion(version_maj, version_min, patch_level) && !this.isCurrentGameVersion(version_maj, version_min, patch_level))
                    {
                        var msg =
                            $"Save file version {version_maj}.{version_min} accepted on game version {Version_c.CURRENT_VERSION_MAJOR}.{Version_c.CURRENT_VERSION_MINOR}.";
                        //var msg = "Save file version ";
                        //msg += version_maj + "." + version_min;
                        //msg += " accepted on game version ";
                        //msg += std::to_string(Version_c.CURRENT_VERSION_MAJOR) + "." + std::to_string(Version_c.CURRENT_VERSION_MINOR) + ".";
                        this.terminal.printMessage(msg);
                    }

                    // if false: only restored options and monster memory.
                    return dg.game_turn >= 0;
                }
            }
            dg.game_turn = -1;
            this.terminal.putStringClearToEOL("Please try again without that save file.", new Coord_t(1, 0));

            // We have messages for the player to read, this will ask for a keypress
            this.terminal.printMessage(/*CNIL*/null);

            this.game.exitProgram();

            return false; // not reached
        }

        private void putByte(IBinaryWriter writer, uint value)
        {
            writer.Write((byte) value);
        }

        private void wrBool(IBinaryWriter writer, ref uint xor_byte, bool value)
        {
            this.wrByte(writer, ref xor_byte, (uint)(value ? 1u : 0));
        }

        private void wrByte(IBinaryWriter writer, ref uint xor_byte, uint value)
        {
            xor_byte ^= value;
            this.putByte(writer, xor_byte);
            //writer.Write((byte)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, "BYTE:  %02X = %d\n", (int)xor_byte, (int)value));
        }

        private void wrShort(IBinaryWriter writer, ref uint xor_byte, uint value)
        {
            xor_byte ^= (value & 0xFF);
            this.putByte(writer, xor_byte);
            //writer.Write((byte)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, "SHORT: %02X", (int)xor_byte));
            xor_byte ^= (value >> 8) & 0xFF;
            this.putByte(writer, xor_byte);
            //writer.Write((byte)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X = %d\n", (int)xor_byte, (int)value));
        }

        private void wrLong(IBinaryWriter writer, ref uint xor_byte, uint value)
        {
            xor_byte ^= (value & 0xFF);
            this.putByte(writer, xor_byte);
            //writer.Write((byte)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, "LONG:  %02X", (int)xor_byte));
            xor_byte ^= ((value >> 8) & 0xFF);
            this.putByte(writer, xor_byte);
            //writer.Write((byte)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X", (int)xor_byte));
            xor_byte ^= ((value >> 16) & 0xFF);
            this.putByte(writer, xor_byte);
            //writer.Write((byte)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X", (int)xor_byte));
            xor_byte ^= ((value >> 24) & 0xFF);
            this.putByte(writer, xor_byte);
            //writer.Write((byte)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X = %ld\n", (int)xor_byte, (int32_t)value));
        }

        private void wrBytes(IBinaryWriter writer, ref uint xor_byte, uint[] values, int count)
        {
            //uint8_t* ptr;

            //DEBUG(fprintf(logfile, "%d BYTES:", count));
            //ptr = value;
            for (int i = 0; i < count; i++)
            {
                xor_byte ^= values[i];
                //xor_byte ^= *ptr++;
                //(void)putc((int)xor_byte, fileptr);
                this.putByte(writer, xor_byte);
                //writer.Write((byte)xor_byte);
                //DEBUG(fprintf(logfile, "  %02X = %d", (int)xor_byte, (int)(ptr[-1])));
            }

            //DEBUG(fprintf(logfile, "\n"));
        }

        private void wrString(IBinaryWriter writer, ref uint xor_byte, string str)
        {
            str = str ?? string.Empty;

            //DEBUG(char * s = str);
            //DEBUG(fprintf(logfile, "STRING:"));
            //while (*str != '\0')
            foreach (var c in str)
            {
                //xor_byte ^= *str++;
                xor_byte ^= c;
                this.putByte(writer, xor_byte);
                //writer.Write((byte)xor_byte);
                //(void)putc((int)xor_byte, fileptr);
                //DEBUG(fprintf(logfile, " %02X", (int)xor_byte));
            }
            xor_byte ^= '\0';
            this.putByte(writer, xor_byte);
            //writer.Write((byte)xor_byte);
            //(void)putc((int)xor_byte, fileptr);
            //DEBUG(fprintf(logfile, " %02X = \"%s\"\n", (int)xor_byte, s));
        }

        private void wrShorts(IBinaryWriter writer, ref uint xor_byte, int[] values, int count)
        {
            for (int i = 0; i < count; i++)
            {
                //xor_byte ^= (*sptr & 0xFF);
                xor_byte ^= (uint)values[i] & 0xFF;
                this.putByte(writer, xor_byte);
                //writer.Write((byte)xor_byte);
                //(void)putc((int)xor_byte, fileptr);
                //DEBUG(fprintf(logfile, "  %02X", (int)xor_byte));
                xor_byte ^= ((uint)values[i] >> 8) & 0xFF;
                //xor_byte ^= ((*sptr++ >> 8) & 0xFF);
                this.putByte(writer, xor_byte);
                //writer.Write((byte)xor_byte);
                //(void)putc((int)xor_byte, fileptr);
                //DEBUG(fprintf(logfile, " %02X = %d", (int)xor_byte, (int)sptr[-1]));
            }
        }

        private void wrShorts(IBinaryWriter writer, ref uint xor_byte, uint[] values, int count)
        {
            //DEBUG(fprintf(logfile, "%d SHORTS:", count));

            //uint16_t* sptr = value;

            for (int i = 0; i < count; i++)
            {
                //xor_byte ^= (*sptr & 0xFF);
                xor_byte ^= (values[i] & 0xFF);
                this.putByte(writer, xor_byte);
                //writer.Write((byte)xor_byte);
                //(void)putc((int)xor_byte, fileptr);
                //DEBUG(fprintf(logfile, "  %02X", (int)xor_byte));
                xor_byte ^= ((values[i] >> 8) & 0xFF);
                //xor_byte ^= ((*sptr++ >> 8) & 0xFF);
                this.putByte(writer, xor_byte);
                //writer.Write((byte)xor_byte);
                //(void)putc((int)xor_byte, fileptr);
                //DEBUG(fprintf(logfile, " %02X = %d", (int)xor_byte, (int)sptr[-1]));
            }

            //DEBUG(fprintf(logfile, "\n"));
        }

        private void wrItem(IBinaryWriter writer, ref uint xor_byte, Inventory_t item)
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

        private void wrMonster(IBinaryWriter writer, ref uint xor_byte, Monster_t monster)
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
        private uint getByte(IBinaryReader reader)
        {
            //return (uint)(reader.ReadChar() & 0xFF);
            return (uint)(reader.ReadByte() & 0xFF);
            //return (uint)(getc(fileptr) & 0xFF);
        }

        private bool rdBool(IBinaryReader reader, ref uint xor_byte)
        {
            return this.rdByte(reader, ref xor_byte) != 0;
            //return (bool)(rdByte() != 0);
        }

        private uint rdByte(IBinaryReader reader, ref uint xor_byte)
        {
            var c = this.getByte(reader);
            uint decoded_byte = c ^ xor_byte;
            xor_byte = c;

            //DEBUG(fprintf(logfile, "BYTE:  %02X = %d\n", (int)c, decoded_byte));

            return decoded_byte;
        }

        private uint rdShort(IBinaryReader reader, ref uint xor_byte)
        {
            var c = this.getByte(reader);
            uint decoded_int = c ^ xor_byte;

            xor_byte = this.getByte(reader);
            decoded_int |= (uint)(c ^ xor_byte) << 8;

            //DEBUG(fprintf(logfile, "SHORT: %02X %02X = %d\n", (int)c, (int)xor_byte, decoded_int));

            return decoded_int;
        }

        private uint rdLong(IBinaryReader reader, ref uint xor_byte)
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

        private void rdBytes(IBinaryReader reader, ref uint xor_byte, uint[] value, int count)
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

        private void rdString(IBinaryReader reader, ref uint xor_byte, out string str)
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

        private void rdShorts(IBinaryReader reader, ref uint xor_byte, int[] values, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var c = this.getByte(reader);
                uint s = c ^ xor_byte;
                xor_byte = this.getByte(reader);
                s |= (uint)(c ^ xor_byte) << 8;
                values[i] = (int)s;
                //*sptr++ = s;
                //DEBUG(fprintf(logfile, "  %02X %02X = %d", (int)c, (int)xor_byte, (int)s));
            }
        }

        private void rdShorts(IBinaryReader reader, ref uint xor_byte, uint[] values, int count)
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

        private void rdItem(IBinaryReader reader, ref uint xor_byte, Inventory_t item)
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

        private void rdMonster(IBinaryReader reader, ref uint xor_byte, Monster_t monster)
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

        private void saveHighScore(IBinaryWriter writer, ref uint xor_byte, HighScore_t score)
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

        private void readHighScore(IBinaryReader reader, HighScore_t score)
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


        // Support for Umoria 5.2.2 up to 5.7.x.
        // The save file format was frozen as of version 5.2.2.
        bool validGameVersion(uint major, uint minor, uint patch)
        {
            if (major != 5)
            {
                return false;
            }

            if (minor < 2)
            {
                return false;
            }

            if (minor == 2 && patch < 2)
            {
                return false;
            }

            return minor <= 7;
        }

        bool isCurrentGameVersion(uint major, uint minor, uint patch)
        {
            return major == Version_c.CURRENT_VERSION_MAJOR && minor == Version_c.CURRENT_VERSION_MINOR && patch == Version_c.CURRENT_VERSION_PATCH;
        }
    }
}
