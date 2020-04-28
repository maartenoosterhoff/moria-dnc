using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using static Moria.Core.Constants.Std_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Monster_manager_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public interface IWizard
    {
        bool enterWizardMode();
        void wizardCureAll();
        void wizardDropRandomItems();
        void wizardJumpLevel();
        void wizardSummonMonster();
        void wizardCreateObjects();
        void wizardGainExperience();
        void wizardGenerateObject();
        void wizardLightUpDungeon();
        void wizardCharacterAdjustment();
    }

    public class Wizard_m : IWizard
    {
        private readonly IPlayerMagic playerMagic;
        private readonly IRnd rnd;
        private readonly ITreasure treasure;

        public Wizard_m(
            IPlayerMagic playerMagic,
            IRnd rnd,
            ITreasure treasure
        )
        {
            this.playerMagic = playerMagic;
            this.rnd = rnd;
            this.treasure = treasure;
        }

        // lets anyone enter wizard mode after a disclaimer... -JEW-
        public bool enterWizardMode()
        {
            var game = State.Instance.game;

            var answer = false;

            if (game.noscore == 0)
            {
                printMessage("Wizard mode is for debugging and experimenting.");
                answer = getInputConfirmation("The game will not be scored if you enter wizard mode. Are you sure?");
            }

            if ((game.noscore != 0) || answer)
            {
                game.noscore |= 0x2;
                game.wizard_mode = true;
                return true;
            }

            return false;
        }

        public void wizardCureAll()
        {
            var py = State.Instance.py;

            spellRemoveCurseFromAllItems();
            this.playerMagic.playerCureBlindness();
            this.playerMagic.playerCureConfusion();
            this.playerMagic.playerCurePoison();
            this.playerMagic.playerRemoveFear();
            playerStatRestore((int)PlayerAttr.STR);
            playerStatRestore((int)PlayerAttr.INT);
            playerStatRestore((int)PlayerAttr.WIS);
            playerStatRestore((int)PlayerAttr.CON);
            playerStatRestore((int)PlayerAttr.DEX);
            playerStatRestore((int)PlayerAttr.CHR);

            if (py.flags.slow > 1)
            {
                py.flags.slow = 1;
            }
            if (py.flags.image > 1)
            {
                py.flags.image = 1;
            }
        }

        // Generate random items
        public void wizardDropRandomItems()
        {
            var game = State.Instance.game;
            int i;

            if (game.command_count > 0)
            {
                i = game.command_count;
                game.command_count = 0;
            }
            else
            {
                i = 1;
            }
            dungeonPlaceRandomObjectNear(State.Instance.py.pos, i);

            drawDungeonPanel();
        }

        // Go up/down to specified depth
        public void wizardJumpLevel()
        {
            var game = State.Instance.game;
            var dg = State.Instance.dg;
            int i;

            if (game.command_count > 0)
            {
                if (game.command_count > 99)
                {
                    i = 0;
                }
                else
                {
                    i = game.command_count;
                }
                game.command_count = 0;
            }
            else
            {
                i = -1;
                var input = string.Empty;
                //vtype_t input = { 0 };

                putStringClearToEOL("Go to which level (0-99) ? ", new Coord_t(0, 0));

                if (getStringInput(out input, new Coord_t(0, 27), 10))
                {
                    stringToNumber(input, out i);
                }
            }

            if (i >= 0)
            {
                dg.current_level = (int)i;
                if (dg.current_level > 99)
                {
                    dg.current_level = 99;
                }
                dg.generate_new_level = true;
            }
            else
            {
                messageLineClear();
            }
        }

        // Increase Experience
        public void wizardGainExperience()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            if (game.command_count > 0)
            {
                py.misc.exp = game.command_count;
                game.command_count = 0;
            }
            else if (py.misc.exp == 0)
            {
                py.misc.exp = 1;
            }
            else
            {
                py.misc.exp = py.misc.exp * 2;
            }
            displayCharacterExperience();
        }

        // Summon a random monster
        public void wizardSummonMonster()
        {
            var py = State.Instance.py;
            var coord = new Coord_t(py.pos.y, py.pos.x);

            monsterSummon(coord, true);

            updateMonsters(false);
        }

        // Light up the dungeon -RAK-
        public void wizardLightUpDungeon()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            bool flag;

            flag = !dg.floor[py.pos.y][py.pos.x].permanent_light;

            for (var y = 0; y < dg.height; y++)
            {
                for (var x = 0; x < dg.width; x++)
                {
                    if (dg.floor[y][x].feature_id <= MAX_CAVE_FLOOR)
                    {
                        for (var yy = y - 1; yy <= y + 1; yy++)
                        {
                            for (var xx = x - 1; xx <= x + 1; xx++)
                            {
                                dg.floor[yy][xx].permanent_light = flag;
                                if (!flag)
                                {
                                    dg.floor[yy][xx].field_mark = false;
                                }
                            }
                        }
                    }
                }
            }

            drawDungeonPanel();
        }

        // Wizard routine for gaining on stats -RAK-
        public void wizardCharacterAdjustment()
        {
            var py = State.Instance.py;
            int number;
            string input;
            //vtype_t input = { '\0' };

            putStringClearToEOL("(3 - 118) Strength     = ", new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, 25), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > 2 && number < 119)
                {
                    py.stats.max[(int)PlayerAttr.STR] = (uint)number;
                    playerStatRestore((int)PlayerAttr.STR);
                }
            }
            else
            {
                return;
            }

            putStringClearToEOL("(3 - 118) Intelligence = ", new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, 25), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > 2 && number < 119)
                {
                    py.stats.max[(int)PlayerAttr.INT] = (uint)number;
                    playerStatRestore((int)PlayerAttr.INT);
                }
            }
            else
            {
                return;
            }

            putStringClearToEOL("(3 - 118) Wisdom       = ", new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, 25), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > 2 && number < 119)
                {
                    py.stats.max[(int)PlayerAttr.WIS] = (uint)number;
                    playerStatRestore((int)PlayerAttr.WIS);
                }
            }
            else
            {
                return;
            }

            putStringClearToEOL("(3 - 118) Dexterity    = ", new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, 25), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > 2 && number < 119)
                {
                    py.stats.max[(int)PlayerAttr.DEX] = (uint)number;
                    playerStatRestore((int)PlayerAttr.DEX);
                }
            }
            else
            {
                return;
            }

            putStringClearToEOL("(3 - 118) Constitution = ", new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, 25), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > 2 && number < 119)
                {
                    py.stats.max[(int)PlayerAttr.CON] = (uint)number;
                    playerStatRestore((int)PlayerAttr.CON);
                }
            }
            else
            {
                return;
            }

            putStringClearToEOL("(3 - 118) Charisma     = ", new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, 25), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > 2 && number < 119)
                {
                    py.stats.max[(int)PlayerAttr.CHR] = (uint)number;
                    playerStatRestore((int)PlayerAttr.CHR);
                }
            }
            else
            {
                return;
            }

            putStringClearToEOL("(1 - 32767) Hit points = ", new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, 25), 5))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > 0 && number <= SHRT_MAX)
                {
                    py.misc.max_hp = (int)number;
                    py.misc.current_hp = (int)number;
                    py.misc.current_hp_fraction = 0;
                    printCharacterMaxHitPoints();
                    printCharacterCurrentHitPoints();
                }
            }
            else
            {
                return;
            }

            putStringClearToEOL("(0 - 32767) Mana       = ", new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, 25), 5))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > -1 && number <= SHRT_MAX)
                {
                    py.misc.mana = (int)number;
                    py.misc.current_mana = (int)number;
                    py.misc.current_mana_fraction = 0;
                    printCharacterCurrentMana();
                }
            }
            else
            {
                return;
            }

            input = $"Current={py.misc.au:d}  Gold = ";
            //(void)sprintf(input, "Current=%d  Gold = ", py.misc.au);
            number = input.Length;
            //number = (int)strlen(input);
            putStringClearToEOL(input, new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, number), 7))
            {
                int new_gold;
                var valid_number = stringToNumber(input, out new_gold);
                if (valid_number && new_gold > -1)
                {
                    py.misc.au = new_gold;
                    printCharacterGoldValue();
                }
            }
            else
            {
                return;
            }

            input = $"Current={py.misc.chance_in_search:d}  (0-200) Searching = ";
            //(void)sprintf(input, "Current=%d  (0-200) Searching = ", py.misc.chance_in_search);
            number = input.Length;
            //number = (int)strlen(input);
            putStringClearToEOL(input, new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, number), 3))
            {
                int new_gold;
                var valid_number = stringToNumber(input, out new_gold);
                if (valid_number && number > -1 && number < 201)
                {
                    py.misc.chance_in_search = (int)number;
                }
            }
            else
            {
                return;
            }

            input = $"Current={py.misc.stealth_factor:d}  (-1-18) Stealth = ";
            //(void)sprintf(input, "Current=%d  (-1-18) Stealth = ", py.misc.stealth_factor);
            number = input.Length;
            //number = (int)strlen(input);
            putStringClearToEOL(input, new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, number), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > -2 && number < 19)
                {
                    py.misc.stealth_factor = (int)number;
                }
            }
            else
            {
                return;
            }

            input = $"Current={py.misc.disarm:d}  (0-200) Disarming = ";
            //(void)sprintf(input, "Current=%d  (0-200) Disarming = ", py.misc.disarm);
            number = input.Length;
            //number = (int)strlen(input);
            putStringClearToEOL(input, new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, number), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > -1 && number < 201)
                {
                    py.misc.disarm = (int)number;
                }
            }
            else
            {
                return;
            }

            input = $"Current={py.misc.saving_throw:d}  (0-100) Save = ";
            //(void)sprintf(input, "Current=%d  (0-100) Save = ", py.misc.saving_throw);
            number = input.Length;
            //number = (int)strlen(input);
            putStringClearToEOL(input, new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, number), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > -1 && number < 201)
                {
                    py.misc.saving_throw = (int)number;
                }
            }
            else
            {
                return;
            }

            input = $"Current={py.misc.bth:d}  (0-200) Base to hit = ";
            //(void)sprintf(input, "Current=%d  (0-200) Base to hit = ", py.misc.bth);
            number = input.Length;
            //number = (int)strlen(input);
            putStringClearToEOL(input, new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, number), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > -1 && number < 201)
                {
                    py.misc.bth = (int)number;
                }
            }
            else
            {
                return;
            }

            input = $"Current={py.misc.bth_with_bows:d}  (0-200) Bows/Throwing = ";
            //(void)sprintf(input, "Current=%d  (0-200) Bows/Throwing = ", py.misc.bth_with_bows);
            number = input.Length;
            //number = (int)strlen(input);
            putStringClearToEOL(input, new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, number), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > -1 && number < 201)
                {
                    py.misc.bth_with_bows = (int)number;
                }
            }
            else
            {
                return;
            }

            input = $"Current={py.misc.weight:d}  Weight = ";
            //(void)sprintf(input, "Current=%d  Weight = ", py.misc.weight);
            number = input.Length;
            //number = (int)strlen(input);
            putStringClearToEOL(input, new Coord_t(0, 0));
            if (getStringInput(out input, new Coord_t(0, number), 3))
            {
                var valid_number = stringToNumber(input, out number);
                if (valid_number && number > -1)
                {
                    py.misc.weight = (uint)number;
                }
            }
            else
            {
                return;
            }

            char inputChar;
            while (getCommand("Alter speed? (+/-)", out inputChar))
            {
                if (inputChar == '+')
                {
                    playerChangeSpeed(-1);
                }
                else if (inputChar == '-')
                {
                    playerChangeSpeed(1);
                }
                else
                {
                    break;
                }
                printCharacterSpeed();
            }
        }

        // Request user input to get the array index of the `game_objects[]`
        public bool wizardRequestObjectId(out int id, string label, int start_id, int end_id)
        {
            id = 0;

            var id_str = $"{start_id}-{end_id}";
            //std::ostringstream id_str;
            //id_str << start_id << "-" << end_id;

            var msg = $"{label} ID ({id_str}): ";
            //std::string msg = label + " ID (" + id_str.str() + "): ";
            putStringClearToEOL(msg, new Coord_t(0, 0));

            string input;
            //vtype_t input = { 0 };
            if (!getStringInput(out input, new Coord_t(0, (int)msg.Length), 3))
            {
                return false;
            }

            int given_id;
            if (!stringToNumber(input, out given_id))
            {
                return false;
            }

            if (given_id < start_id || given_id > end_id)
            {
                putStringClearToEOL("Invalid ID. Must be " + id_str, new Coord_t(0, 0));
                return false;
            }
            id = given_id;

            return true;
        }

        // Simplified wizard routine for creating an object
        public void wizardGenerateObject()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            int id;
            if (!wizardRequestObjectId(out id, "Dungeon/Store object", 0, 366))
            {
                return;
            }

            var coord = new Coord_t(0, 0);

            for (var i = 0; i < 10; i++)
            {
                coord.y = py.pos.y - 3 + this.rnd.randomNumber(5);
                coord.x = py.pos.x - 4 + this.rnd.randomNumber(7);

                if (coordInBounds(coord) && dg.floor[coord.y][coord.x].feature_id <= MAX_CAVE_FLOOR && dg.floor[coord.y][coord.x].treasure_id == 0)
                {
                    // delete any object at location, before call popt()
                    if (dg.floor[coord.y][coord.x].treasure_id != 0)
                    {
                        dungeonDeleteObject(coord);
                    }

                    // place the object
                    var free_treasure_id = popt();
                    dg.floor[coord.y][coord.x].treasure_id = (uint)free_treasure_id;
                    inventoryItemCopyTo(id, game.treasure.list[free_treasure_id]);
                    treasure.magicTreasureMagicalAbility(free_treasure_id, dg.current_level);

                    // auto identify the item
                    itemIdentify(game.treasure.list[free_treasure_id], ref free_treasure_id);

                    i = 9;
                }
            }
        }

        // Wizard routine for creating objects -RAK-
        public void wizardCreateObjects()
        {
            int number;
            string input;
            //vtype_t input = { 0 };

            printMessage("Warning: This routine can cause a fatal error.");

            var forge = new Inventory_t();
            var item = forge;

            item.id = Config.dungeon_objects.OBJ_WIZARD;
            item.special_name_id = 0;
            itemReplaceInscription(item, "wizard item");
            item.identification = Config.identification.ID_KNOWN2 | Config.identification.ID_STORE_BOUGHT;

            putStringClearToEOL("Tval   : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 3))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.category_id = (uint)number;
            }

            putStringClearToEOL("Tchar  : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 1))
            {
                return;
            }
            item.sprite = (uint)input[0];

            putStringClearToEOL("Subval : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 5))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.sub_category_id = (uint)number;
            }

            putStringClearToEOL("Weight : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 5))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.weight = (uint)number;
            }

            putStringClearToEOL("Number : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 5))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.items_count = (uint)number;
            }

            putStringClearToEOL("Damage (dice): ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 15), 3))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.damage.dice = (uint)number;
            }

            putStringClearToEOL("Damage (sides): ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 16), 3))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.damage.sides = (uint)number;
            }

            putStringClearToEOL("+To hit: ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 3))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.to_hit = (int)number;
            }

            putStringClearToEOL("+To dam: ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 3))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.to_damage = (int)number;
            }

            putStringClearToEOL("AC     : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 3))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.ac = (int)number;
            }

            putStringClearToEOL("+To AC : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 3))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.to_ac = (int)number;
            }

            putStringClearToEOL("P1     : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 5))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.misc_use = (int)number;
            }

            putStringClearToEOL("Flags (In HEX, eg 0x01020304): ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 16), 8))
            {
                return;
            }

            int input_number;

            //// can't be constant string, this causes problems with
            //// the GCC compiler and some scanf routines.
            //
            //char pattern[4];
            //
            //(void)strcpy(pattern, "%lx");
            //
            //(void)sscanf(input, pattern, &input_number);

            input_number = (int)Convert.ToInt64(input, 16);

            item.flags = (uint)input_number;

            putStringClearToEOL("Cost : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 9), 8))
            {
                return;
            }
            if (stringToNumber(input, out input_number))
            {
                item.cost = input_number;
            }

            putStringClearToEOL("Level : ", new Coord_t(0, 0));
            if (!getStringInput(out input, new Coord_t(0, 10), 3))
            {
                return;
            }
            if (stringToNumber(input, out number))
            {
                item.depth_first_found = (uint)number;
            }

            if (getInputConfirmation("Allocate?"))
            {
                var dg = State.Instance.dg;
                var game = State.Instance.game;
                var py = State.Instance.py;

                // delete object first if any, before call popt()
                var tile = dg.floor[py.pos.y][py.pos.x];

                if (tile.treasure_id != 0)
                {
                    dungeonDeleteObject(py.pos);
                }

                number = popt();

                game.treasure.list[number] = forge;
                tile.treasure_id = (uint)number;

                printMessage("Allocated.");
            }
            else
            {
                printMessage("Aborted.");
            }
        }
    }
}
