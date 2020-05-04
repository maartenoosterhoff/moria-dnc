﻿using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_move_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Player_bash_m
    {
        public static void SetDependencies(
            IDice dice,
            IDungeon dungeon,
            IGame game,
            IHelpers helpers,
            IInventoryManager inventoryManager,
            IRnd rnd,
            IStd std,
            ITerminal terminal
        )
        {
            Player_bash_m.dice = dice;
            Player_bash_m.dungeon = dungeon;
            Player_bash_m.game = game;
            Player_bash_m.helpers = helpers;
            Player_bash_m.inventoryManager = inventoryManager;
            Player_bash_m.rnd = rnd;
            Player_bash_m.std = std;
            Player_bash_m.terminal = terminal;
        }

        private static IDice dice;
        private static IDungeon dungeon;
        private static IGame game;
        private static IHelpers helpers;
        private static IInventoryManager inventoryManager;
        private static IRnd rnd;
        private static IStd std;
        private static ITerminal terminal;

        // Bash open a door or chest -RAK-
        // Note: Affected by strength and weight of character
        //
        // For a closed door, `misc_use` is positive if locked; negative if stuck. A disarm spell
        // unlocks and unjams doors!
        //
        // For an open door, `misc_use` is positive for a broken door.
        //
        // A closed door can be opened - harder if locked. Any door might be bashed open
        // (and thereby broken). Bashing a door is (potentially) faster! You move into the
        // door way. To open a stuck door, it must be bashed. A closed door can be jammed
        // (which makes it stuck if previously locked).
        //
        // Creatures can also open doors. A creature with open door ability will (if not
        // in the line of sight) move though a closed or secret door with no changes. If
        // in the line of sight, closed door are opened, & secret door revealed. Whether
        // in the line of sight or not, such a creature may unlock or unstick a door.
        //
        // A creature with no such ability will attempt to bash a non-secret door.
        public static void playerBash()
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            var dir = 0;
            if (!Player_bash_m.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
            {
                return;
            }

            if (py.flags.confused > 0)
            {
                terminal.printMessage("You are confused.");
                dir = rnd.getRandomDirection();
            }

            var coord = py.pos.Clone();
            helpers.movePosition(dir, ref coord);

            var tile = dg.floor[coord.y][coord.x];

            if (tile.creature_id > 1)
            {
                playerBashPosition(coord);
                return;
            }

            if (tile.treasure_id != 0)
            {
                var item = game.treasure.list[tile.treasure_id];

                if (item.category_id == TV_CLOSED_DOOR)
                {
                    playerBashClosedDoor(coord, dir, tile, item);
                }
                else if (item.category_id == TV_CHEST)
                {
                    playerBashClosedChest(item);
                }
                else
                {
                    // Can't give free turn, or else player could try
                    // directions until they find the invisible creature
                    terminal.printMessage("You bash it, but nothing interesting happens.");
                }
                return;
            }

            if (tile.feature_id < MIN_CAVE_WALL)
            {
                terminal.printMessage("You bash at empty space.");
                return;
            }

            // same message for wall as for secret door
            terminal.printMessage("You bash it, but nothing interesting happens.");
        }

        // Make a bash attack on someone. -CJS-
        // Used to be part of bash above.
        public static void playerBashAttack(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            var monster_id = (int)dg.floor[coord.y][coord.x].creature_id;

            var monster = State.Instance.monsters[monster_id];
            var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

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

            var base_to_hit = (int)py.stats.used[(int)PlayerAttr.STR];
            base_to_hit += (int)py.inventory[(int)PlayerEquipment.Arm].weight / 2;
            base_to_hit += (int)py.misc.weight / 10;

            if (!monster.lit)
            {
                base_to_hit /= 2;
                base_to_hit -= (int)py.stats.used[(int)PlayerAttr.DEX] * ((int)BTH_PER_PLUS_TO_HIT_ADJUST - 1);
                base_to_hit -= (int)py.misc.level * Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.BTH] / 2;
            }

            if (playerTestBeingHit(base_to_hit, (int)py.misc.level, (int)py.stats.used[(int)PlayerAttr.DEX], (int)creature.ac, (int)PlayerClassLevelAdj.BTH))
            {
                var msg = $"You hit {name}.";
                //vtype_t msg = { '\0' };
                //(void)sprintf(msg, "You hit %s.", name);
                terminal.printMessage(msg);

                var damage = dice.diceRoll(py.inventory[(int)PlayerEquipment.Arm].damage);
                damage = playerWeaponCriticalBlow((int)(py.inventory[(int)PlayerEquipment.Arm].weight / 4 + py.stats.used[(int)PlayerAttr.STR]), 0, damage, (int)PlayerClassLevelAdj.BTH);
                damage += (int)py.misc.weight / 60;
                damage += 3;

                if (damage < 0)
                {
                    damage = 0;
                }

                // See if we done it in.
                if (monsterTakeHit(monster_id, damage) >= 0)
                {
                    msg = $"You have slain {name}.";
                    //(void)sprintf(msg, "You have slain %s.", name);
                    terminal.printMessage(msg);
                    displayCharacterExperience();
                }
                else
                {
                    name = name.Substring(0, 1).ToUpper() + name.Substring(1);
                    //name[0] = (char)toupper((int)name[0]); // Capitalize

                    // Can not stun Balrog
                    int avg_max_hp;
                    if ((creature.defenses & Config.monsters_defense.CD_MAX_HP) != 0)
                    {
                        avg_max_hp = dice.maxDiceRoll(creature.hit_die);
                    }
                    else
                    {
                        // TODO: use maxDiceRoll(), just be careful about the bit shift
                        avg_max_hp = (int)((creature.hit_die.dice * (creature.hit_die.sides + 1)) >> 1);
                    }

                    if (100 + rnd.randomNumber(400) + rnd.randomNumber(400) > monster.hp + avg_max_hp)
                    {
                        monster.stunned_amount += (uint)rnd.randomNumber(3) + 1;
                        if (monster.stunned_amount > 24)
                        {
                            monster.stunned_amount = 24;
                        }

                        msg = $"{name} appears stunned!";
                        //(void)sprintf(msg, "%s appears stunned!", name);
                    }
                    else
                    {
                        msg = $"{name} ignores your bash!";
                        //(void)sprintf(msg, "%s ignores your bash!", name);
                    }
                    terminal.printMessage(msg);
                }
            }
            else
            {
                var msg = $"You miss {name}.";
                //vtype_t msg = { '\0' };
                //(void)sprintf(msg, "You miss %s.", name);
                terminal.printMessage(msg);
            }

            if (rnd.randomNumber(150) > py.stats.used[(int)PlayerAttr.DEX])
            {
                terminal.printMessage("You are off balance.");
                py.flags.paralysis = (int)(1 + rnd.randomNumber(2));
            }
        }

        public static void playerBashPosition(Coord_t coord)
        {
            var py = State.Instance.py;
            // Is a Coward?
            if (py.flags.afraid > 0)
            {
                terminal.printMessage("You are afraid!");
                return;
            }

            playerBashAttack(coord);
        }

        public static void playerBashClosedDoor(Coord_t coord, int dir, Tile_t tile, Inventory_t item)
        {
            var py = State.Instance.py;
            var game = State.Instance.game;

            terminal.printMessageNoCommandInterrupt("You smash into the door!");

            var chance = (int)(py.stats.used[(int)PlayerAttr.STR] + py.misc.weight / 2);

            // Use (roughly) similar method as for monsters.
            var abs_misc_use = (int)std.std_abs(std.std_intmax_t(item.misc_use));
            if (rnd.randomNumber(chance * (20 + abs_misc_use)) < 10 * (chance - abs_misc_use))
            {
                terminal.printMessage("The door crashes open!");

                inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_OPEN_DOOR, game.treasure.list[tile.treasure_id]);

                // 50% chance of breaking door
                item.misc_use = (int)(uint)(1 - rnd.randomNumber(2));

                tile.feature_id = TILE_CORR_FLOOR;

                if (py.flags.confused == 0)
                {
                    playerMove(dir, false);
                }
                else
                {
                    dungeon.dungeonLiteSpot(coord);
                }

                return;
            }

            if (rnd.randomNumber(150) > py.stats.used[(int)PlayerAttr.DEX])
            {
                terminal.printMessage("You are off-balance.");
                py.flags.paralysis = (int)(1 + rnd.randomNumber(2));
                return;
            }

            if (game.command_count == 0)
            {
                terminal.printMessage("The door holds firm.");
            }
        }

        public static void playerBashClosedChest(Inventory_t item)
        {
            if (rnd.randomNumber(10) == 1)
            {
                terminal.printMessage("You have destroyed the chest.");
                terminal.printMessage("and its contents!");

                item.id = Config.dungeon_objects.OBJ_RUINED_CHEST;
                item.flags = 0;

                return;
            }

            if ((item.flags & Config.treasure_chests.CH_LOCKED) != 0u && rnd.randomNumber(10) == 1)
            {
                terminal.printMessage("The lock breaks open!");

                item.flags &= ~Config.treasure_chests.CH_LOCKED;

                return;
            }

            terminal.printMessageNoCommandInterrupt("The chest holds firm.");
        }

    }
}