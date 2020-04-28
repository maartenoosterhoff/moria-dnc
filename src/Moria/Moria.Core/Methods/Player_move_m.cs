using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Player_run_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Store_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Player_move_m
    {
        public static void SetDependencies(
            IDice dice,
            IDungeon dungeon,
            IInventory inventory,
            IMonsterManager monsterManager,
            IRnd rnd
        )
        {
            Player_move_m.dice = dice;
            Player_move_m.dungeon = dungeon;
            Player_move_m.inventory = inventory;
            Player_move_m.monsterManager = monsterManager;
            Player_move_m.rnd = rnd;
        }

        private static IDice dice;
        private static IDungeon dungeon;
        private static IInventory inventory;
        private static IMonsterManager monsterManager;
        private static IRnd rnd;

        public static void trapOpenPit(Inventory_t item, int dam)
        {
            var py = State.Instance.py;
            printMessage("You fell into a pit!");

            if (py.flags.free_fall)
            {
                printMessage("You gently float down.");
                return;
            }

            var description = string.Empty;
            //obj_desc_t description = { '\0' };
            itemDescription(ref description, item, true);
            playerTakesHit(dam, description);
        }

        public static void trapArrow(Inventory_t item, int dam)
        {
            var py = State.Instance.py;

            if (playerTestBeingHit(125, 0, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
            {
                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                itemDescription(ref description, item, true);
                playerTakesHit(dam, description);

                printMessage("An arrow hits you.");
                return;
            }

            printMessage("An arrow barely misses you.");
        }

        public static void trapCoveredPit(Inventory_t item, int dam, Coord_t coord)
        {
            var py = State.Instance.py;

            printMessage("You fell into a covered pit.");

            if (py.flags.free_fall)
            {
                printMessage("You gently float down.");
            }
            else
            {
                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                itemDescription(ref description, item, true);
                playerTakesHit(dam, description);
            }

            dungeon.dungeonSetTrap(coord, 0);
        }

        public static void trapDoor(Inventory_t item, int dam)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            dg.generate_new_level = true;
            dg.current_level++;

            printMessage("You fell through a trap door!");

            if (py.flags.free_fall)
            {
                printMessage("You gently float down.");
            }
            else
            {
                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                itemDescription(ref description, item, true);
                playerTakesHit(dam, description);
            }

            // Force the messages to display before starting to generate the next level.
            printMessage(/*CNIL*/null);
        }

        public static void trapSleepingGas()
        {
            var py = State.Instance.py;

            if (py.flags.paralysis != 0)
            {
                return;
            }

            printMessage("A strange white mist surrounds you!");

            if (py.flags.free_action)
            {
                printMessage("You are unaffected.");
                return;
            }

            py.flags.paralysis += rnd.randomNumber(10) + 4;
            printMessage("You fall asleep.");
        }

        public static void trapHiddenObject(Coord_t coord)
        {
            dungeon.dungeonDeleteObject(coord);

            dungeon.dungeonPlaceRandomObjectAt(coord, false);

            printMessage("Hmmm, there was something under this rock.");
        }

        public static void trapStrengthDart(Inventory_t item, int dam)
        {
            var py = State.Instance.py;
            if (playerTestBeingHit(125, 0, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
            {
                if (!py.flags.sustain_str)
                {
                    playerStatRandomDecrease((int)PlayerAttr.STR);

                    var description = string.Empty;
                    //obj_desc_t description = { '\0' };
                    itemDescription(ref description, item, true);
                    playerTakesHit(dam, description);

                    printMessage("A small dart weakens you!");
                }
                else
                {
                    printMessage("A small dart hits you.");
                }
            }
            else
            {
                printMessage("A small dart barely misses you.");
            }
        }

        public static void trapTeleport(Coord_t coord)
        {
            var game = State.Instance.game;

            game.teleport_player = true;

            printMessage("You hit a teleport trap!");

            // Light up the teleport trap, before we teleport away.
            dungeon.dungeonMoveCharacterLight(coord, coord);
        }

        public static void trapRockfall(Coord_t coord, int dam)
        {
            playerTakesHit(dam, "a falling rock");

            dungeon.dungeonDeleteObject(coord);
            dungeon.dungeonPlaceRubble(coord);

            printMessage("You are hit by falling rock.");
        }

        public static void trapCorrodeGas()
        {
            printMessage("A strange red gas surrounds you.");

            inventory.damageCorrodingGas("corrosion gas");
        }

        public static void trapSummonMonster(Coord_t coord)
        {
            // Rune disappears.
            dungeon.dungeonDeleteObject(coord);

            var num = 2 + rnd.randomNumber(3);

            var location = new Coord_t(0, 0);

            for (var i = 0; i < num; i++)
            {
                location.y = coord.y;
                location.x = coord.x;
                monsterManager.monsterSummon(location, false);
            }
        }

        static void trapFire(int dam)
        {
            printMessage("You are enveloped in flames!");

            inventory.damageFire(dam, "a fire trap");
        }

        static void trapAcid(int dam)
        {
            printMessage("You are splashed with acid!");

            inventory.damageAcid(dam, "an acid trap");
        }

        static void trapPoisonGas(int dam)
        {
            printMessage("A pungent green gas surrounds you!");

            inventory.damagePoisonedGas(dam, "a poison gas trap");
        }

        static void trapBlindGas()
        {
            var py = State.Instance.py;
            printMessage("A black gas surrounds you!");

            py.flags.blind += rnd.randomNumber(50) + 50;
        }

        static void trapConfuseGas()
        {
            var py = State.Instance.py;
            printMessage("A gas of scintillating colors surrounds you!");

            py.flags.confused += rnd.randomNumber(15) + 15;
        }

        static void trapSlowDart(Inventory_t item, int dam)
        {
            var py = State.Instance.py;

            if (playerTestBeingHit(125, 0, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
            {
                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                itemDescription(ref description, item, true);
                playerTakesHit(dam, description);

                printMessage("A small dart hits you!");

                if (py.flags.free_action)
                {
                    printMessage("You are unaffected.");
                }
                else
                {
                    py.flags.slow += rnd.randomNumber(20) + 10;
                }
            }
            else
            {
                printMessage("A small dart barely misses you.");
            }
        }

        static void trapConstitutionDart(Inventory_t item, int dam)
        {
            var py = State.Instance.py;

            if (playerTestBeingHit(125, 0, 0, py.misc.ac + py.misc.magical_ac, (int)CLASS_MISC_HIT))
            {
                if (!py.flags.sustain_con)
                {
                    playerStatRandomDecrease((int)PlayerAttr.CON);

                    var description = string.Empty;
                    //obj_desc_t description = { '\0' };
                    itemDescription(ref description, item, true);
                    playerTakesHit(dam, description);

                    printMessage("A small dart saps your health!");
                }
                else
                {
                    printMessage("A small dart hits you.");
                }
            }
            else
            {
                printMessage("A small dart barely misses you.");
            }
        }



        // Player hit a trap.  (Chuckle) -RAK-
        static void playerStepsOnTrap(Coord_t coord)
        {
            var game = State.Instance.game;
            var dg = State.Instance.dg;

            playerEndRunning();
            dungeon.trapChangeVisibility(coord);

            var item = game.treasure.list[dg.floor[coord.y][coord.x].treasure_id];

            var damage = dice.diceRoll(item.damage);

            switch ((TrapTypes)item.sub_category_id)
            {
                case TrapTypes.OpenPit:
                    trapOpenPit(item, damage);
                    break;
                case TrapTypes.ArrowPit:
                    trapArrow(item, damage);
                    break;
                case TrapTypes.CoveredPit:
                    trapCoveredPit(item, damage, coord);
                    break;
                case TrapTypes.TrapDoor:
                    trapDoor(item, damage);
                    break;
                case TrapTypes.SleepingGas:
                    trapSleepingGas();
                    break;
                case TrapTypes.HiddenObject:
                    trapHiddenObject(coord);
                    break;
                case TrapTypes.DartOfStr:
                    trapStrengthDart(item, damage);
                    break;
                case TrapTypes.Teleport:
                    trapTeleport(coord);
                    break;
                case TrapTypes.Rockfall:
                    trapRockfall(coord, damage);
                    break;
                case TrapTypes.CorrodingGas:
                    trapCorrodeGas();
                    break;
                case TrapTypes.SummonMonster:
                    trapSummonMonster(coord);
                    break;
                case TrapTypes.FireTrap:
                    trapFire(damage);
                    break;
                case TrapTypes.AcidTrap:
                    trapAcid(damage);
                    break;
                case TrapTypes.PoisonGasTrap:
                    trapPoisonGas(damage);
                    break;
                case TrapTypes.BlindingGas:
                    trapBlindGas();
                    break;
                case TrapTypes.ConfuseGas:
                    trapConfuseGas();
                    break;
                case TrapTypes.SlowDart:
                    trapSlowDart(item, damage);
                    break;
                case TrapTypes.DartOfCon:
                    trapConstitutionDart(item, damage);
                    break;
                case TrapTypes.SecretDoor:
                case TrapTypes.ScareMonster:
                    break;

                // Town level traps are special, the stores.
                case TrapTypes.GeneralStore:
                    storeEnter(0);
                    break;
                case TrapTypes.Armory:
                    storeEnter(1);
                    break;
                case TrapTypes.Weaponsmith:
                    storeEnter(2);
                    break;
                case TrapTypes.Temple:
                    storeEnter(3);
                    break;
                case TrapTypes.Alchemist:
                    storeEnter(4);
                    break;
                case TrapTypes.MagicShop:
                    storeEnter(5);
                    break;

                default:
                    // All cases are handled, so this should never be reached!
                    printMessage("Unknown trap value.");
                    break;
            }
        }

        static bool playerRandomMovement(int dir)
        {
            var py = State.Instance.py;
            // Never random if sitting
            if (dir == 5)
            {
                return false;
            }

            // 75% random movement
            var player_random_move = rnd.randomNumber(4) > 1;

            var player_is_confused = py.flags.confused > 0;

            return player_is_confused && player_random_move;
        }

        // Player is on an object. Many things can happen based -RAK-
        // on the TVAL of the object. Traps are set off, money and most objects
        // are picked up. Some objects, such as open doors, just sit there.
        static void carry(Coord_t coord, bool pickup)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var py = State.Instance.py;

            var item = game.treasure.list[dg.floor[coord.y][coord.x].treasure_id];

            var tile_flags = (int)game.treasure.list[dg.floor[coord.y][coord.x].treasure_id].category_id;

            if (tile_flags > TV_MAX_PICK_UP)
            {
                if (tile_flags == TV_INVIS_TRAP || tile_flags == TV_VIS_TRAP || tile_flags == TV_STORE_DOOR)
                {
                    // OOPS!
                    playerStepsOnTrap(coord);
                }
                return;
            }

            var description = string.Empty;
            var msg = string.Empty;
            //obj_desc_t description = { '\0' };
            //obj_desc_t msg = { '\0' };

            playerEndRunning();

            // There's GOLD in them thar hills!
            if (tile_flags == TV_GOLD)
            {
                py.misc.au += item.cost;

                itemDescription(ref description, item, true);
                msg = $"You have found {item.cost} gold pieces worth of {description}";
                //(void)sprintf(msg, "You have found %d gold pieces worth of %s", item.cost, description);

                printCharacterGoldValue();
                dungeon.dungeonDeleteObject(coord);

                printMessage(msg);

                return;
            }

            // Too many objects?
            if (inventory.inventoryCanCarryItemCount(item))
            {
                // Okay,  pick it up
                if (pickup && Config.options.prompt_to_pickup)
                {
                    itemDescription(ref description, item, true);

                    // change the period to a question mark
                    description = description.Substring(0, description.Length - 1) + "?";
                    //description[strlen(description) - 1] = '?';
                    pickup = getInputConfirmation("Pick up " + description);
                }

                // Check to see if it will change the players speed.
                if (pickup && !inventory.inventoryCanCarryItem(item))
                {
                    itemDescription(ref description, item, true);

                    // change the period to a question mark
                    //description[strlen(description) - 1] = '?';
                    description = description.Substring(0, description.Length - 1) + "?";
                    pickup = getInputConfirmation("Exceed your weight limit to pick up " + description);
                }

                // Attempt to pick up an object.
                if (pickup)
                {
                    var locn = inventory.inventoryCarryItem(item);

                    itemDescription(ref description, py.inventory[locn], true);
                    msg = $"You have {description} ({(char)(locn + 'a')})";
                    //(void)sprintf(msg, "You have %s (%c)", description, locn + 'a');
                    printMessage(msg);
                    dungeon.dungeonDeleteObject(coord);
                }
            }
            else
            {
                itemDescription(ref description, item, true);
                msg = $"You can't carry {description}";
                //(void)sprintf(msg, "You can't carry %s", description);
                printMessage(msg);
            }
        }

        // Moves player from one space to another. -RAK-
        public static void playerMove(int direction, bool do_pickup)
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;
            if (playerRandomMovement(direction))
            {
                direction = rnd.randomNumber(9);
                playerEndRunning();
            }

            var coord = py.pos.Clone();

            // Legal move?
            if (!playerMovePosition(direction, ref coord))
            {
                return;
            }

            var tile = dg.floor[coord.y][coord.x];
            var monster = State.Instance.monsters[tile.creature_id];

            // if there is no creature, or an unlit creature in the walls then...
            // disallow attacks against unlit creatures in walls because moving into
            // a wall is a free turn normally, hence don't give player free turns
            // attacking each wall in an attempt to locate the invisible creature,
            // instead force player to tunnel into walls which always takes a turn
            if (tile.creature_id < 2 || (!monster.lit && tile.feature_id >= MIN_CLOSED_SPACE))
            {
                // Open floor spot
                if (tile.feature_id <= MAX_OPEN_SPACE)
                {
                    // Make final assignments of char coords
                    var old_coord = py.pos;

                    //py.pos.y = coord.y;
                    //py.pos.x = coord.x;
                    py.pos = coord;

                    // Move character record (-1)
                    dungeon.dungeonMoveCreatureRecord(old_coord, py.pos);

                    // Check for new panel
                    if (coordOutsidePanel(py.pos, false))
                    {
                        drawDungeonPanel();
                    }

                    // Check to see if they should stop
                    if (py.running_tracker != 0)
                    {
                        playerAreaAffect(direction, py.pos);
                    }

                    // Check to see if they've noticed something
                    // fos may be negative if have good rings of searching
                    if (py.misc.fos <= 1 || rnd.randomNumber(py.misc.fos) == 1 || ((py.flags.status & Config.player_status.PY_SEARCH) != 0u))
                    {
                        playerSearch(py.pos, py.misc.chance_in_search);
                    }

                    if (tile.feature_id == TILE_LIGHT_FLOOR)
                    {
                        // A room of light should be lit.

                        if (!tile.permanent_light && (py.flags.blind == 0))
                        {
                            dungeon.dungeonLightRoom(py.pos);
                        }
                    }
                    else if (tile.perma_lit_room && py.flags.blind < 1)
                    {
                        // In doorway of light-room?

                        for (var row = (py.pos.y - 1); row <= (py.pos.y + 1); row++)
                        {
                            for (var col = (py.pos.x - 1); col <= (py.pos.x + 1); col++)
                            {
                                if (dg.floor[row][col].feature_id == TILE_LIGHT_FLOOR && !dg.floor[row][col].permanent_light)
                                {
                                    dungeon.dungeonLightRoom(new Coord_t(row, col));
                                }
                            }
                        }
                    }

                    // Move the light source
                    dungeon.dungeonMoveCharacterLight(old_coord, py.pos);

                    // An object is beneath them.
                    if (tile.treasure_id != 0)
                    {
                        carry(py.pos, do_pickup);

                        // if stepped on falling rock trap, and space contains
                        // rubble, then step back into a clear area
                        if (game.treasure.list[tile.treasure_id].category_id == TV_RUBBLE)
                        {
                            dungeon.dungeonMoveCreatureRecord(py.pos, old_coord);
                            dungeon.dungeonMoveCharacterLight(py.pos, old_coord);

                            //py.pos.y = old_coord.y;
                            //py.pos.x = old_coord.x;
                            py.pos = old_coord;

                            // check to see if we have stepped back onto another trap, if so, set it off
                            var id = dg.floor[py.pos.y][py.pos.x].treasure_id;
                            if (id != 0)
                            {
                                var val = (int)game.treasure.list[id].category_id;
                                if (val == TV_INVIS_TRAP || val == TV_VIS_TRAP || val == TV_STORE_DOOR)
                                {
                                    playerStepsOnTrap(py.pos);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Can't move onto floor space

                    if ((py.running_tracker == 0) && tile.treasure_id != 0)
                    {
                        if (game.treasure.list[tile.treasure_id].category_id == TV_RUBBLE)
                        {
                            printMessage("There is rubble blocking your way.");
                        }
                        else if (game.treasure.list[tile.treasure_id].category_id == TV_CLOSED_DOOR)
                        {
                            printMessage("There is a closed door blocking your way.");
                        }
                    }
                    else
                    {
                        playerEndRunning();
                    }
                    game.player_free_turn = true;
                }
            }
            else
            {
                // Attacking a creature!

                var old_find_flag = (int)py.running_tracker;

                playerEndRunning();

                // if player can see monster, and was in find mode, then nothing
                if (monster.lit && (old_find_flag != 0))
                {
                    // did not do anything this turn
                    game.player_free_turn = true;
                }
                else
                {
                    playerAttackPosition(coord);
                }
            }
        }
    }
}
