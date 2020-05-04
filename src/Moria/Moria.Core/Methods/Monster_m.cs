using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.SpellCasting;
using Moria.Core.Methods.Commands.SpellCasting.Attacking;
using Moria.Core.Methods.Commands.SpellCasting.Defending;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Constants.Std_c;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Monster_m
    {
        public static void SetDependencies(
            IDice dice,
            IDungeon dungeon,
            IDungeonLos dungeonLos,
            IDungeonPlacer dungeonPlacer,
            IHelpers helpers,
            IInventory inventory,
            IInventoryManager inventoryManager,
            IMonsterManager monsterManager,
            IRnd rnd,
            IStd std,

            IEventPublisher eventPublisher
        )
        {
            Monster_m.dice = dice;
            Monster_m.dungeon = dungeon;
            Monster_m.dungeonLos = dungeonLos;
            Monster_m.dungeonPlacer = dungeonPlacer;
            Monster_m.helpers = helpers;
            Monster_m.inventory = inventory;
            Monster_m.inventoryManager = inventoryManager;
            Monster_m.monsterManager = monsterManager;
            Monster_m.rnd = rnd;
            Monster_m.std = std;

            Monster_m.eventPublisher = eventPublisher;
        }

        private static IDice dice;
        private static IDungeon dungeon;
        private static IDungeonLos dungeonLos;
        private static IDungeonPlacer dungeonPlacer;
        private static IInventory inventory;
        private static IHelpers helpers;
        private static IInventoryManager inventoryManager;
        private static IMonsterManager monsterManager;
        private static IRnd rnd;
        private static IStd std;

        private static IEventPublisher eventPublisher;

        public static bool monsterIsVisible(Monster_t monster)
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;
            var creatures_list = Library.Instance.Creatures.creatures_list;

            var visible = false;

            var tile = dg.floor[monster.pos.y][monster.pos.x];
            var creature = creatures_list[(int)monster.creature_id];

            if (tile.permanent_light || tile.temporary_light || py.running_tracker != 0 && monster.distance_from_player < 2 && py.carrying_light)
            {
                // Normal sight.
                if ((creature.movement & Config.monsters_move.CM_INVISIBLE) == 0)
                {
                    visible = true;
                }
                else if (py.flags.see_invisible)
                {
                    visible = true;
                    State.Instance.creature_recall[monster.creature_id].movement |= Config.monsters_move.CM_INVISIBLE;
                }
            }
            else if (py.flags.see_infra > 0 && monster.distance_from_player <= py.flags.see_infra && (creature.defenses & Config.monsters_defense.CD_INFRA) != 0)
            {
                // Infra vision.
                visible = true;
                State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_INFRA;
            }

            return visible;
        }

        // Updates screen when monsters move about -RAK-
        public static void monsterUpdateVisibility(int monster_id)
        {
            var py = State.Instance.py;
            var game = State.Instance.game;

            var visible = false;
            var monster = State.Instance.monsters[monster_id];

            if (monster.distance_from_player <= Config.monsters.MON_MAX_SIGHT &&
                (py.flags.status & Config.player_status.PY_BLIND) == 0u &&
                coordInsidePanel(new Coord_t(monster.pos.y, monster.pos.x)))
            {
                if (game.wizard_mode)
                {
                    // Wizard sight.
                    visible = true;
                }
                else if (dungeonLos.los(py.pos, monster.pos))
                {
                    visible = monsterIsVisible(monster);
                }
            }

            if (visible)
            {
                // Light it up.
                if (!monster.lit)
                {
                    playerDisturb(1, 0);
                    monster.lit = true;
                    dungeon.dungeonLiteSpot(new Coord_t(monster.pos.y, monster.pos.x));

                    // notify inventoryExecuteCommand()
                    State.Instance.screen_has_changed = true;
                }
            }
            else if (monster.lit)
            {
                // Turn it off.
                monster.lit = false;
                dungeon.dungeonLiteSpot(new Coord_t(monster.pos.y, monster.pos.x));

                // notify inventoryExecuteCommand()
                State.Instance.screen_has_changed = true;
            }
        }

        // Given speed, returns number of moves this turn. -RAK-
        // NOTE: Player must always move at least once per iteration,
        // a slowed player is handled by moving monsters faster
        public static int monsterMovementRate(int speed)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            if (speed > 0)
            {
                if (py.flags.rest != 0)
                {
                    return 1;
                }
                return speed;
            }

            // speed must be negative here
            var rate = 0;
            if (dg.game_turn % (2 - speed) == 0)
            {
                rate = 1;
            }

            return rate;
        }

        // Makes sure a new creature gets lit up. -CJS-
        public static bool monsterMakeVisible(Coord_t coord)
        {
            var dg = State.Instance.dg;

            var monster_id = (int)dg.floor[coord.y][coord.x].creature_id;
            if (monster_id <= 1)
            {
                return false;
            }

            monsterUpdateVisibility(monster_id);
            return State.Instance.monsters[monster_id].lit;
        }

        // Choose correct directions for monster movement -RAK-
        public static void monsterGetMoveDirection(int monster_id, int[] directions)
        {
            var monsters = State.Instance.monsters;
            var py = State.Instance.py;

            int ay, ax, movement;

            var y = monsters[monster_id].pos.y - py.pos.y;
            var x = monsters[monster_id].pos.x - py.pos.x;

            if (y < 0)
            {
                movement = 8;
                ay = -y;
            }
            else
            {
                movement = 0;
                ay = y;
            }
            if (x > 0)
            {
                movement += 4;
                ax = x;
            }
            else
            {
                ax = -x;
            }

            // this has the advantage of preventing the diamond maneuver, also faster
            if (ay > ax << 1)
            {
                movement += 2;
            }
            else if (ax > ay << 1)
            {
                movement++;
            }

            switch (movement)
            {
                case 0:
                    directions[0] = 9;
                    if (ay > ax)
                    {
                        directions[1] = 8;
                        directions[2] = 6;
                        directions[3] = 7;
                        directions[4] = 3;
                    }
                    else
                    {
                        directions[1] = 6;
                        directions[2] = 8;
                        directions[3] = 3;
                        directions[4] = 7;
                    }
                    break;
                case 1:
                case 9:
                    directions[0] = 6;
                    if (y < 0)
                    {
                        directions[1] = 3;

                        directions[2] = 9;
                        directions[3] = 2;
                        directions[4] = 8;
                    }
                    else
                    {
                        directions[1] = 9;
                        directions[2] = 3;
                        directions[3] = 8;
                        directions[4] = 2;
                    }
                    break;
                case 2:
                case 6:
                    directions[0] = 8;
                    if (x < 0)
                    {
                        directions[1] = 9;
                        directions[2] = 7;
                        directions[3] = 6;
                        directions[4] = 4;
                    }
                    else
                    {
                        directions[1] = 7;
                        directions[2] = 9;
                        directions[3] = 4;
                        directions[4] = 6;
                    }
                    break;
                case 4:
                    directions[0] = 7;
                    if (ay > ax)
                    {
                        directions[1] = 8;
                        directions[2] = 4;
                        directions[3] = 9;
                        directions[4] = 1;
                    }
                    else
                    {
                        directions[1] = 4;
                        directions[2] = 8;
                        directions[3] = 1;
                        directions[4] = 9;
                    }
                    break;
                case 5:
                case 13:
                    directions[0] = 4;
                    if (y < 0)
                    {
                        directions[1] = 1;
                        directions[2] = 7;
                        directions[3] = 2;
                        directions[4] = 8;
                    }
                    else
                    {
                        directions[1] = 7;
                        directions[2] = 1;
                        directions[3] = 8;
                        directions[4] = 2;
                    }
                    break;
                case 8:
                    directions[0] = 3;
                    if (ay > ax)
                    {
                        directions[1] = 2;
                        directions[2] = 6;
                        directions[3] = 1;
                        directions[4] = 9;
                    }
                    else
                    {
                        directions[1] = 6;
                        directions[2] = 2;
                        directions[3] = 9;
                        directions[4] = 1;
                    }
                    break;
                case 10:
                case 14:
                    directions[0] = 2;
                    if (x < 0)
                    {
                        directions[1] = 3;
                        directions[2] = 1;
                        directions[3] = 6;
                        directions[4] = 4;
                    }
                    else
                    {
                        directions[1] = 1;
                        directions[2] = 3;
                        directions[3] = 4;
                        directions[4] = 6;
                    }
                    break;
                case 12:
                    directions[0] = 1;
                    if (ay > ax)
                    {
                        directions[1] = 2;
                        directions[2] = 4;
                        directions[3] = 3;
                        directions[4] = 7;
                    }
                    else
                    {
                        directions[1] = 4;
                        directions[2] = 2;
                        directions[3] = 7;
                        directions[4] = 3;
                    }
                    break;
                default:
                    break;
            }
        }

        public static void monsterPrintAttackDescription(ref string msg, int attack_id)
        {
            switch (attack_id)
            {
                case 1:
                    printMessage(msg + "hits you.");
                    break;
                case 2:
                    printMessage(msg + "bites you.");
                    break;
                case 3:
                    printMessage(msg + "claws you.");
                    break;
                case 4:
                    printMessage(msg + "stings you.");
                    break;
                case 5:
                    printMessage(msg + "touches you.");
                    break;
                case 6:
                    printMessage(msg + "kicks you.");
                    break;
                case 7:
                    printMessage(msg + "gazes at you.");
                    break;
                case 8:
                    printMessage(msg + "breathes on you.");
                    break;
                case 9:
                    printMessage(msg + "spits on you.");
                    break;
                case 10:
                    printMessage(msg + "makes a horrible wail.");
                    break;
                case 11:
                    printMessage(msg + "embraces you.");
                    break;
                case 12:
                    printMessage(msg + "crawls on you.");
                    break;
                case 13:
                    printMessage(msg + "releases a cloud of spores.");
                    break;
                case 14:
                    printMessage(msg + "begs you for money.");
                    break;
                case 15:
                    printMessage("You've been slimed!");
                    break;
                case 16:
                    printMessage(msg + "crushes you.");
                    break;
                case 17:
                    printMessage(msg + "tramples you.");
                    break;
                case 18:
                    printMessage(msg + "drools on you.");
                    break;
                case 19:
                    switch (rnd.randomNumber(9))
                    {
                        case 1:
                            printMessage(msg + "insults you!");
                            break;
                        case 2:
                            printMessage(msg + "insults your mother!");
                            break;
                        case 3:
                            printMessage(msg + "gives you the finger!");
                            break;
                        case 4:
                            printMessage(msg + "humiliates you!");
                            break;
                        case 5:
                            printMessage(msg + "wets on your leg!");
                            break;
                        case 6:
                            printMessage(msg + "defiles you!");
                            break;
                        case 7:
                            printMessage(msg + "dances around you!");
                            break;
                        case 8:
                            printMessage(msg + "makes obscene gestures!");
                            break;
                        case 9:
                            printMessage(msg + "moons you!!!");
                            break;
                        default:
                            break;
                    }
                    break;
                case 99:
                    printMessage(msg + "is repelled.");
                    break;
                default:
                    break;
            }
        }

        public static void monsterConfuseOnAttack(Creature_t creature, Monster_t monster, int attack_type, string monster_name, bool visible)
        {
            var py = State.Instance.py;

            if (py.flags.confuse_monster && attack_type != 99)
            {
                printMessage("Your hands stop glowing.");
                py.flags.confuse_monster = false;

                var msg = string.Empty;

                if (rnd.randomNumber(MON_MAX_LEVELS) < creature.level || (creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                {
                    msg = $"{monster_name}is unaffected.";
                    //(void)sprintf(msg, "%sis unaffected.", monster_name);
                }
                else
                {
                    msg = $"{monster_name}appears confused.";
                    //(void)sprintf(msg, "%sappears confused.", monster_name);
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

                if (visible && !State.Instance.game.character_is_dead && rnd.randomNumber(4) == 1)
                {
                    State.Instance.creature_recall[monster.creature_id].defenses |= creature.defenses & Config.monsters_defense.CD_NO_SLEEP;
                }
            }
        }

        public const int UCHAR_MAX = 255;

        // Make an attack on the player (chuckle.) -RAK-
        public static void monsterAttackPlayer(int monster_id)
        {
            // don't beat a dead body!
            if (State.Instance.game.character_is_dead)
            {
                return;
            }

            var monster = State.Instance.monsters[monster_id];
            var creature = Library.Instance.Creatures.creatures_list[(int)monster.creature_id];

            var name = string.Empty;
            if (!monster.lit)
            {
                name = "It ";
                //(void)strcpy(name, "It ");
            }
            else
            {
                name = $"The {creature.name} ";
                //(void)sprintf(name, "The %s ", creature.name);
            }

            var death_description = string.Empty;
            //vtype_t death_description = { '\0' };
            playerDiedFromString(ref death_description, creature.name, creature.movement);

            var attack_counter = 0;

            var py = State.Instance.py;
            var monster_attacks = Library.Instance.Creatures.monster_attacks;

            foreach (var damage_type_id in creature.damage)
            {
                if (damage_type_id == 0 || State.Instance.game.character_is_dead)
                {
                    break;
                }

                var attack_type = monster_attacks[(int)damage_type_id].type_id;
                var attack_desc = monster_attacks[(int)damage_type_id].description_id;
                var dice = monster_attacks[(int)damage_type_id].dice;

                if (py.flags.protect_evil > 0 &&
                    (creature.defenses & Config.monsters_defense.CD_EVIL) != 0 && py.misc.level + 1 > creature.level)
                {
                    if (monster.lit)
                    {
                        State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_EVIL;
                    }
                    attack_type = 99;
                    attack_desc = 99;
                }

                if (playerTestAttackHits((int)attack_type, creature.level))
                {
                    playerDisturb(1, 0);

                    // can not strcat to name because the creature may have multiple attacks.
                    var description = name;
                    //(void)strcpy(description, name);
                    monsterPrintAttackDescription(ref description, (int)attack_desc);

                    // always fail to notice attack if creature invisible, set notice
                    // and visible here since creature may be visible when attacking
                    // and then teleport afterwards (becoming effectively invisible)
                    var notice = true;
                    var visible = true;
                    if (!monster.lit)
                    {
                        visible = false;
                        notice = false;
                    }

                    var damage = Monster_m.dice.diceRoll(dice);
                    var monsterhp = monster.hp;
                    notice = executeAttackOnPlayer(creature.level, ref monsterhp, monster_id, (int)attack_type, damage, death_description, notice);
                    monster.hp = monsterhp;

                    // Moved here from monsterMove, so that monster only confused if it
                    // actually hits. A monster that has been repelled has not hit
                    // the player, so it should not be confused.
                    monsterConfuseOnAttack(creature, monster, (int)attack_desc, name, visible);

                    // increase number of attacks if notice true, or if visible and
                    // had previously noticed the attack (in which case all this does
                    // is help player learn damage), note that in the second case do
                    // not increase attacks if creature repelled (no damage done)
                    if ((notice || visible && State.Instance.creature_recall[monster.creature_id].attacks[attack_counter] != 0 && attack_type != 99) &&
                        State.Instance.creature_recall[monster.creature_id].attacks[attack_counter] < UCHAR_MAX)
                    {
                        State.Instance.creature_recall[monster.creature_id].attacks[attack_counter]++;
                    }

                    if (State.Instance.game.character_is_dead && State.Instance.creature_recall[monster.creature_id].deaths < SHRT_MAX)
                    {
                        State.Instance.creature_recall[monster.creature_id].deaths++;
                    }
                }
                else
                {
                    if (attack_desc >= 1 && attack_desc <= 3 || attack_desc == 6)
                    {
                        playerDisturb(1, 0);

                        var description = name;
                        //(void)strcpy(description, name);
                        printMessage(description + "misses you.");
                    }
                }

                if (attack_counter < MON_MAX_ATTACKS - 1)
                {
                    attack_counter++;
                }
                else
                {
                    break;
                }
            }
        }

        public static void monsterOpenDoor(Tile_t tile, int monster_hp, uint move_bits, ref bool do_turn, ref bool do_move, ref uint rcmove, Coord_t coord)
        {
            var game = State.Instance.game;

            var item = game.treasure.list[tile.treasure_id];

            // Creature can open doors.
            if ((move_bits & Config.monsters_move.CM_OPEN_DOOR) != 0u)
            {
                var door_is_stuck = false;

                if (item.category_id == TV_CLOSED_DOOR)
                {
                    do_turn = true;

                    if (item.misc_use == 0)
                    {
                        // Closed doors

                        do_move = true;
                    }
                    else if (item.misc_use > 0)
                    {
                        // Locked doors

                        if (rnd.randomNumber((monster_hp + 1) * (50 + item.misc_use)) < 40 * (monster_hp - 10 - item.misc_use))
                        {
                            item.misc_use = 0;
                        }
                    }
                    else if (item.misc_use < 0)
                    {
                        // Stuck doors

                        if (rnd.randomNumber((monster_hp + 1) * (50 - item.misc_use)) < 40 * (monster_hp - 10 + item.misc_use))
                        {
                            printMessage("You hear a door burst open!");
                            playerDisturb(1, 0);
                            door_is_stuck = true;
                            do_move = true;
                        }
                    }
                }
                else if (item.category_id == TV_SECRET_DOOR)
                {
                    do_turn = true;
                    do_move = true;
                }

                if (do_move)
                {
                    inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_OPEN_DOOR, item);

                    // 50% chance of breaking door
                    if (door_is_stuck)
                    {
                        item.misc_use = 1 - rnd.randomNumber(2);
                    }
                    tile.feature_id = TILE_CORR_FLOOR;
                    dungeon.dungeonLiteSpot(coord);
                    rcmove |= Config.monsters_move.CM_OPEN_DOOR;
                    do_move = false;
                }
            }
            else if (item.category_id == TV_CLOSED_DOOR)
            {
                // Creature can not open doors, must bash them
                do_turn = true;

                var abs_misc_use = (int)std.std_abs(std.std_intmax_t(item.misc_use));
                if (rnd.randomNumber((monster_hp + 1) * (80 + abs_misc_use)) < 40 * (monster_hp - 20 - abs_misc_use))
                {
                    inventoryManager.inventoryItemCopyTo((int)Config.dungeon_objects.OBJ_OPEN_DOOR, item);

                    // 50% chance of breaking door
                    item.misc_use = 1 - rnd.randomNumber(2);
                    tile.feature_id = TILE_CORR_FLOOR;
                    dungeon.dungeonLiteSpot(coord);
                    printMessage("You hear a door burst open!");
                    playerDisturb(1, 0);
                }
            }
        }

        public static void glyphOfWardingProtection(uint creature_id, uint move_bits, ref bool do_move, ref bool do_turn, Coord_t coord)
        {
            var py = State.Instance.py;

            if (rnd.randomNumber(Config.treasure.OBJECTS_RUNE_PROTECTION) < Library.Instance.Creatures.creatures_list[(int)creature_id].level)
            {
                if (coord.y == py.pos.y && coord.x == py.pos.x)
                {
                    printMessage("The rune of protection is broken!");
                }
                dungeon.dungeonDeleteObject(coord);
                return;
            }

            do_move = false;

            // If the creature moves only to attack, don't let it
            // move if the glyph prevents it from attacking
            if ((move_bits & Config.monsters_move.CM_ATTACK_ONLY) != 0u)
            {
                do_turn = true;
            }
        }

        public static void monsterMovesOnPlayer(Monster_t monster, uint creature_id, int monster_id, uint move_bits, ref bool do_move, ref bool do_turn, ref uint rcmove, Coord_t coord)
        {
            var creatures_list = Library.Instance.Creatures.creatures_list;
            var monsters = State.Instance.monsters;

            if (creature_id == 1)
            {
                // if the monster is not lit, must call monsterUpdateVisibility, it
                // may be faster than character, and hence could have
                // just moved next to character this same turn.
                if (!monster.lit)
                {
                    monsterUpdateVisibility(monster_id);
                }
                monsterAttackPlayer(monster_id);
                do_move = false;
                do_turn = true;
            }
            else if (creature_id > 1 && (coord.y != monster.pos.y || coord.x != monster.pos.x))
            {
                // Creature is attempting to move on other creature?

                // Creature eats other creatures?
                if ((move_bits & Config.monsters_move.CM_EATS_OTHER) != 0u &&
                    creatures_list[(int)monster.creature_id].kill_exp_value >= creatures_list[(int)monsters[creature_id].creature_id].kill_exp_value)
                {
                    if (monsters[creature_id].lit)
                    {
                        rcmove |= Config.monsters_move.CM_EATS_OTHER;
                    }

                    // It ate an already processed monster. Handle normally.
                    if (monster_id < creature_id)
                    {
                        dungeon.dungeonDeleteMonster((int)creature_id);
                    }
                    else
                    {
                        // If it eats this monster, an already processed
                        // monster will take its place, causing all kinds
                        // of havoc. Delay the kill a bit.
                        dungeon.dungeonDeleteMonsterFix1((int)creature_id);
                    }
                }
                else
                {
                    do_move = false;
                }
            }
        }

        public static void monsterAllowedToMove(Monster_t monster, uint move_bits, ref bool do_turn, ref uint rcmove, Coord_t coord)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var py = State.Instance.py;

            // Pick up or eat an object
            if ((move_bits & Config.monsters_move.CM_PICKS_UP) != 0u)
            {
                var treasure_id = dg.floor[coord.y][coord.x].treasure_id;

                if (treasure_id != 0 && game.treasure.list[treasure_id].category_id <= TV_MAX_OBJECT)
                {
                    rcmove |= Config.monsters_move.CM_PICKS_UP;
                    dungeon.dungeonDeleteObject(coord);
                }
            }

            // Move creature record
            dungeon.dungeonMoveCreatureRecord(new Coord_t(monster.pos.y, monster.pos.x), coord);

            if (monster.lit)
            {
                monster.lit = false;
                dungeon.dungeonLiteSpot(new Coord_t(monster.pos.y, monster.pos.x));
            }

            monster.pos.y = coord.y;
            monster.pos.x = coord.x;
            monster.distance_from_player = (uint)dungeon.coordDistanceBetween(py.pos, coord);

            do_turn = true;
        }

        // Make the move if possible, five choices -RAK-
        public static void makeMove(int monster_id, int[] directions, ref uint rcmove)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var monsters = State.Instance.monsters;
            var creatures_list = Library.Instance.Creatures.creatures_list;

            var do_turn = false;
            var do_move = false;

            var monster = monsters[monster_id];
            var move_bits = creatures_list[(int)monster.creature_id].movement;

            // Up to 5 attempts at moving, give up.
            var coord = new Coord_t(0, 0);
            for (var i = 0; !do_turn && i < 5; i++)
            {
                // Get new position
                coord.y = monster.pos.y;
                coord.x = monster.pos.x;

                helpers.movePosition(directions[i], ref coord);

                var tile = dg.floor[coord.y][coord.x];

                if (tile.feature_id == TILE_BOUNDARY_WALL)
                {
                    continue;
                }

                // Floor is open?
                if (tile.feature_id <= MAX_OPEN_SPACE)
                {
                    do_move = true;
                }
                else if ((move_bits & Config.monsters_move.CM_PHASE) != 0u)
                {
                    // Creature moves through walls?
                    do_move = true;
                    rcmove |= Config.monsters_move.CM_PHASE;
                }
                else if (tile.treasure_id != 0)
                {
                    // Creature can open doors?
                    monsterOpenDoor(tile, monster.hp, move_bits, ref do_turn, ref do_move, ref rcmove, coord);
                }

                // Glyph of warding present?
                if (do_move && tile.treasure_id != 0 && game.treasure.list[tile.treasure_id].category_id == TV_VIS_TRAP && game.treasure.list[tile.treasure_id].sub_category_id == 99)
                {
                    glyphOfWardingProtection(monster.creature_id, move_bits, ref do_move, ref do_turn, coord);
                }

                // Creature has attempted to move on player?
                if (do_move)
                {
                    monsterMovesOnPlayer(monster, tile.creature_id, monster_id, move_bits, ref do_move, ref do_turn, ref rcmove, coord);
                }

                // Creature has been allowed move.
                if (do_move)
                {
                    monsterAllowedToMove(monster, move_bits, ref do_turn, ref rcmove, coord);
                }
            }
        }

        public static bool monsterCanCastSpells(Monster_t monster, uint spells)
        {
            var py = State.Instance.py;
            // 1 in x chance of casting spell
            if (rnd.randomNumber((int)(spells & Config.monsters_spells.CS_FREQ)) != 1)
            {
                return false;
            }

            // Must be within certain range
            var within_range = monster.distance_from_player <= Config.monsters.MON_MAX_SPELL_CAST_DISTANCE;

            // Must have unobstructed Line-Of-Sight
            var unobstructed = dungeonLos.los(py.pos, monster.pos);

            return within_range && unobstructed;
        }

        public static void monsterExecuteCastingOfSpell(Monster_t monster, int monster_id, int spell_id, uint level, string monster_name, string death_description)
        {
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            var coord = py.pos; //  only used for cases 14 and 15.

            // Cast the spell.
            switch (spell_id)
            {
                case 5: // Teleport Short
                    eventPublisher.Publish(new TeleportAwayMonsterCommand(monster_id, 5));
                    //spellTeleportAwayMonster(monster_id, 5);
                    break;
                case 6: // Teleport Long
                    eventPublisher.Publish(new TeleportAwayMonsterCommand(monster_id, (int)Config.monsters.MON_MAX_SIGHT));
                    //spellTeleportAwayMonster(monster_id, (int)Config.monsters.MON_MAX_SIGHT);
                    break;
                case 7: // Teleport To
                    eventPublisher.Publish(new TeleportPlayerToCommand(
                        new Coord_t(monster.pos.y, monster.pos.x)
                    ));
                    //spellTeleportPlayerTo(new Coord_t(monster.pos.y, monster.pos.x));
                    break;
                case 8: // Light Wound
                    if (playerSavingThrow())
                    {
                        printMessage("You resist the effects of the spell.");
                    }
                    else
                    {
                        playerTakesHit(dice.diceRoll(new Dice_t(3, 8)), death_description);
                    }
                    break;
                case 9: // Serious Wound
                    if (playerSavingThrow())
                    {
                        printMessage("You resist the effects of the spell.");
                    }
                    else
                    {
                        playerTakesHit(dice.diceRoll(new Dice_t(8, 8)), death_description);
                    }
                    break;
                case 10: // Hold Person
                    if (py.flags.free_action)
                    {
                        printMessage("You are unaffected.");
                    }
                    else if (playerSavingThrow())
                    {
                        printMessage("You resist the effects of the spell.");
                    }
                    else if (py.flags.paralysis > 0)
                    {
                        py.flags.paralysis += 2;
                    }
                    else
                    {
                        py.flags.paralysis = rnd.randomNumber(5) + 4;
                    }
                    break;
                case 11: // Cause Blindness
                    if (playerSavingThrow())
                    {
                        printMessage("You resist the effects of the spell.");
                    }
                    else if (py.flags.blind > 0)
                    {
                        py.flags.blind += 6;
                    }
                    else
                    {
                        py.flags.blind += 12 + rnd.randomNumber(3);
                    }
                    break;
                case 12: // Cause Confuse
                    if (playerSavingThrow())
                    {
                        printMessage("You resist the effects of the spell.");
                    }
                    else if (py.flags.confused > 0)
                    {
                        py.flags.confused += 2;
                    }
                    else
                    {
                        py.flags.confused = rnd.randomNumber(5) + 3;
                    }
                    break;
                case 13: // Cause Fear
                    if (playerSavingThrow())
                    {
                        printMessage("You resist the effects of the spell.");
                    }
                    else if (py.flags.afraid > 0)
                    {
                        py.flags.afraid += 2;
                    }
                    else
                    {
                        py.flags.afraid = rnd.randomNumber(5) + 3;
                    }
                    break;
                case 14: // Summon Monster
                    monster_name += "magically summons a monsters!";
                    //(void)strcat(monster_name, "magically summons a monster!");
                    printMessage(monster_name);
                    coord.y = py.pos.y;
                    coord.x = py.pos.x;

                    // in case compact_monster() is called,it needs monster_id
                    State.Instance.hack_monptr = monster_id;
                    monsterManager.monsterSummon(coord, false);
                    State.Instance.hack_monptr = -1;
                    monsterUpdateVisibility((int)dg.floor[coord.y][coord.x].creature_id);
                    break;
                case 15: // Summon Undead
                    monster_name += "magically summons an undead!";
                    //(void)strcat(monster_name, "magically summons an undead!");
                    printMessage(monster_name);
                    coord.y = py.pos.y;
                    coord.x = py.pos.x;

                    // in case compact_monster() is called,it needs monster_id
                    State.Instance.hack_monptr = monster_id;
                    monsterManager.monsterSummonUndead(coord);
                    State.Instance.hack_monptr = -1;
                    monsterUpdateVisibility((int)dg.floor[coord.y][coord.x].creature_id);
                    break;
                case 16: // Slow Person
                    if (py.flags.free_action)
                    {
                        printMessage("You are unaffected.");
                    }
                    else if (playerSavingThrow())
                    {
                        printMessage("You resist the effects of the spell.");
                    }
                    else if (py.flags.slow > 0)
                    {
                        py.flags.slow += 2;
                    }
                    else
                    {
                        py.flags.slow = rnd.randomNumber(5) + 3;
                    }
                    break;
                case 17: // Drain Mana
                    if (py.misc.current_mana > 0)
                    {
                        playerDisturb(1, 0);

                        var msg = $"{monster_name}draws psychic energy from you!";
                        //vtype_t msg = { '\0' };
                        //(void)sprintf(msg, "%sdraws psychic energy from you!", monster_name);
                        printMessage(msg);

                        if (monster.lit)
                        {
                            msg = $"{monster_name}appears healthier.";
                            //(void)sprintf(msg, "%sappears healthier.", monster_name);
                            printMessage(msg);
                        }

                        var num = (rnd.randomNumber((int)level) >> 1) + 1;
                        if (num > py.misc.current_mana)
                        {
                            num = py.misc.current_mana;
                            py.misc.current_mana = 0;
                            py.misc.current_mana_fraction = 0;
                        }
                        else
                        {
                            py.misc.current_mana -= num;
                        }
                        printCharacterCurrentMana();
                        monster.hp += 6 * num;
                    }
                    break;
                case 20: // Breath Light
                    monster_name += "breathes lightning.";
                    //(void)strcat(monster_name, "breathes lightning.");
                    printMessage(monster_name);
                    eventPublisher.Publish(
                        new BreathCommand(
                            py.pos, monster_id, monster.hp / 4, (int)MagicSpellFlags.Lightning, death_description
                        )
                    );
                    //spellBreath(py.pos, monster_id, monster.hp / 4, (int)MagicSpellFlags.Lightning, death_description);
                    break;
                case 21: // Breath Gas
                    monster_name += "breathes gas.";
                    //(void)strcat(monster_name, "breathes gas.");
                    printMessage(monster_name);
                    eventPublisher.Publish(
                        new BreathCommand(
                            py.pos, monster_id, monster.hp / 3, (int)MagicSpellFlags.PoisonGas, death_description
                        )
                    );
                    //spellBreath(py.pos, monster_id, monster.hp / 3, (int)MagicSpellFlags.PoisonGas, death_description);
                    break;
                case 22: // Breath Acid
                    monster_name += "breathes acid.";
                    //(void)strcat(monster_name, "breathes acid.");
                    printMessage(monster_name);
                    eventPublisher.Publish(
                        new BreathCommand(
                            py.pos, monster_id, monster.hp / 3, (int)MagicSpellFlags.Acid, death_description
                        )
                    );
                    //spellBreath(py.pos, monster_id, monster.hp / 3, (int)MagicSpellFlags.Acid, death_description);
                    break;
                case 23: // Breath Frost
                    monster_name += "breathes frost.";
                    //(void)strcat(monster_name, "breathes frost.");
                    printMessage(monster_name);
                    eventPublisher.Publish(
                        new BreathCommand(
                            py.pos, monster_id, monster.hp / 3, (int)MagicSpellFlags.Frost, death_description
                        )
                    );
                    //spellBreath(py.pos, monster_id, monster.hp / 3, (int)MagicSpellFlags.Frost, death_description);
                    break;
                case 24: // Breath Fire
                    monster_name += "breathes fire.";
                    //(void)strcat(monster_name, "breathes fire.");
                    printMessage(monster_name);
                    eventPublisher.Publish(
                        new BreathCommand(
                            py.pos, monster_id, monster.hp / 3, (int)MagicSpellFlags.Fire, death_description
                        )
                    );
                    //spellBreath(py.pos, monster_id, monster.hp / 3, (int)MagicSpellFlags.Fire, death_description);
                    break;
                default:
                    monster_name += "cast unknown spell.";
                    //(void)strcat(monster_name, "cast unknown spell.");
                    printMessage(monster_name);
                    break;
            }
        }

        // Creatures can cast spells too.  (Dragon Breath) -RAK-
        //   castSpellGetId = true if creature changes position
        //   return true (took_turn) if creature casts a spell
        public static bool monsterCastSpell(int monster_id)
        {
            var game = State.Instance.game;
            var monsters = State.Instance.monsters;
            var creatures_list = Library.Instance.Creatures.creatures_list;
            var creature_recall = State.Instance.creature_recall;

            if (game.character_is_dead)
            {
                return false;
            }

            var monster = monsters[monster_id];
            var creature = creatures_list[(int)monster.creature_id];

            if (!monsterCanCastSpells(monster, creature.spells))
            {
                return false;
            }

            // Creature is going to cast a spell

            // Check to see if monster should be lit.
            monsterUpdateVisibility(monster_id);

            // Describe the attack
            string name;
            if (monster.lit)
            {
                name = $"The {creature.name} ";
                //(void)sprintf(name, "The %s ", creature.name);
            }
            else
            {
                name = "It ";
                //(void)strcpy(name, "It ");
            }

            var death_description = string.Empty;
            //vtype_t death_description = { '\0' };
            playerDiedFromString(ref death_description, creature.name, creature.movement);

            // Extract all possible spells into spell_choice
            var spell_choice = new int[30];
            //int spell_choice[30];
            var spell_flags = (uint)(creature.spells & ~Config.monsters_spells.CS_FREQ);

            var id = 0;
            while (spell_flags != 0)
            {
                spell_choice[id] = helpers.getAndClearFirstBit(ref spell_flags);
                id++;
            }

            // Choose a spell to cast
            var thrown_spell = spell_choice[rnd.randomNumber(id) - 1];
            thrown_spell++;

            // all except spellTeleportAwayMonster() and drain mana spells always disturb
            if (thrown_spell > 6 && thrown_spell != 17)
            {
                playerDisturb(1, 0);
            }

            // save some code/data space here, with a small time penalty
            if (thrown_spell < 14 && thrown_spell > 6 || thrown_spell == 16)
            {
                name += "casts a spell.";
                //(void)strcat(name, "casts a spell.");
                printMessage(name);
            }

            monsterExecuteCastingOfSpell(monster, monster_id, thrown_spell, creature.level, name, death_description);

            if (monster.lit)
            {
                creature_recall[monster.creature_id].spells |= 1u << (thrown_spell - 1);
                if ((creature_recall[monster.creature_id].spells & Config.monsters_spells.CS_FREQ) != Config.monsters_spells.CS_FREQ)
                {
                    creature_recall[monster.creature_id].spells++;
                }
                if (game.character_is_dead && creature_recall[monster.creature_id].deaths < SHRT_MAX)
                {
                    creature_recall[monster.creature_id].deaths++;
                }
            }

            return true;
        }

        // Places creature adjacent to given location -RAK-
        // Rats and Flys are fun!
        public static bool monsterMultiply(Coord_t coord, int creature_id, int monster_id)
        {
            var dg = State.Instance.dg;
            var creatures_list = Library.Instance.Creatures.creatures_list;
            var monsters = State.Instance.monsters;

            var position = new Coord_t(0, 0);

            for (var i = 0; i <= 18; i++)
            {
                position.y = coord.y - 2 + rnd.randomNumber(3);
                position.x = coord.x - 2 + rnd.randomNumber(3);

                // don't create a new creature on top of the old one, that
                // causes invincible/invisible creatures to appear.
                if (dungeon.coordInBounds(position) && (position.y != coord.y || position.x != coord.x))
                {
                    var tile = dg.floor[position.y][position.x];

                    if (tile.feature_id <= MAX_OPEN_SPACE && tile.treasure_id == 0 && tile.creature_id != 1)
                    {
                        // Creature there already?
                        if (tile.creature_id > 1)
                        {
                            // Some critters are cannibalistic!
                            var cannibalistic = (creatures_list[creature_id].movement & Config.monsters_move.CM_EATS_OTHER) != 0;

                            // Check the experience level -CJS-
                            var experienced = creatures_list[creature_id].kill_exp_value >= creatures_list[(int)monsters[tile.creature_id].creature_id].kill_exp_value;

                            if (cannibalistic && experienced)
                            {
                                // It ate an already processed monster. Handle * normally.
                                if (monster_id < tile.creature_id)
                                {
                                    dungeon.dungeonDeleteMonster((int)tile.creature_id);
                                }
                                else
                                {
                                    // If it eats this monster, an already processed
                                    // monster will take its place, causing all kinds
                                    // of havoc. Delay the kill a bit.
                                    dungeon.dungeonDeleteMonsterFix1((int)tile.creature_id);
                                }

                                // in case compact_monster() is called, it needs monster_id.
                                State.Instance.hack_monptr = monster_id;
                                // Place_monster() may fail if monster list full.
                                var result = monsterManager.monsterPlaceNew(position, creature_id, false);
                                State.Instance.hack_monptr = -1;
                                if (!result)
                                {
                                    return false;
                                }

                                State.Instance.monster_multiply_total++;
                                return monsterMakeVisible(position);
                            }
                        }
                        else
                        {
                            // All clear,  place a monster

                            // in case compact_monster() is called,it needs monster_id
                            State.Instance.hack_monptr = monster_id;
                            // Place_monster() may fail if monster list full.
                            var result = monsterManager.monsterPlaceNew(position, creature_id, false);
                            State.Instance.hack_monptr = -1;
                            if (!result)
                            {
                                return false;
                            }

                            State.Instance.monster_multiply_total++;
                            return monsterMakeVisible(position);
                        }
                    }
                }
            }

            return false;
        }

        public static void monsterMultiplyCritter(Monster_t monster, int monster_id, ref uint rcmove)
        {
            var dg = State.Instance.dg;
            var counter = 0;

            var coord = new Coord_t(0, 0);

            for (coord.y = monster.pos.y - 1; coord.y <= monster.pos.y + 1; coord.y++)
            {
                for (coord.x = monster.pos.x - 1; coord.x <= monster.pos.x + 1; coord.x++)
                {
                    if (dungeon.coordInBounds(coord) && dg.floor[coord.y][coord.x].creature_id > 1)
                    {
                        counter++;
                    }
                }
            }

            // can't call rnd.randomNumber with a value of zero, increment
            // counter to allow creature multiplication.
            if (counter == 0)
            {
                counter++;
            }

            if (counter < 4 && rnd.randomNumber(counter * (int)Config.monsters.MON_MULTIPLY_ADJUST) == 1)
            {
                if (monsterMultiply(new Coord_t(monster.pos.y, monster.pos.x), (int)monster.creature_id, monster_id))
                {
                    rcmove |= Config.monsters_move.CM_MULTIPLY;
                }
            }
        }

        public static void monsterMoveOutOfWall(Monster_t monster, int monster_id, ref uint rcmove)
        {
            var dg = State.Instance.dg;

            // If the monster is already dead, don't kill it again!
            // This can happen for monsters moving faster than the player. They
            // will get multiple moves, but should not if they die on the first
            // move.  This is only a problem for monsters stuck in rock.
            if (monster.hp < 0)
            {
                return;
            }

            var id = 0;
            var dir = 1;
            var directions = new int[9];

            // Note direction of for loops matches direction of keypad from 1 to 9
            // Do not allow attack against the player.
            // Must cast y-1 to signed int, so that a negative value
            // of i will fail the comparison.
            for (var y = monster.pos.y + 1; y >= monster.pos.y - 1; y--)
            {
                for (var x = monster.pos.x - 1; x <= monster.pos.x + 1; x++)
                {
                    if (y < 0 || x < 0)
                    {
                        continue;
                    }

                    if (dir != 5 && dg.floor[y][x].feature_id <= MAX_OPEN_SPACE && dg.floor[y][x].creature_id != 1)
                    {
                        directions[id] = dir;
                        id++;
                    }
                    dir++;
                }
            }

            if (id != 0)
            {
                // put a random direction first
                dir = rnd.randomNumber(id) - 1;

                var saved_id = directions[0];

                directions[0] = directions[dir];
                directions[dir] = saved_id;

                // this can only fail if directions[0] has a rune of protection
                makeMove(monster_id, directions, ref rcmove);
            }

            // if still in a wall, let it dig itself out, but also apply some more damage
            if (dg.floor[monster.pos.y][monster.pos.x].feature_id >= MIN_CAVE_WALL)
            {
                // in case the monster dies, may need to callfix1_delete_monster()
                // instead of delete_monsters()
                State.Instance.hack_monptr = monster_id;
                var i = monsterTakeHit(monster_id, dice.diceRoll(new Dice_t(8, 8)));
                State.Instance.hack_monptr = -1;

                if (i >= 0)
                {
                    printMessage("You hear a scream muffled by rock!");
                    displayCharacterExperience();
                }
                else
                {
                    printMessage("A creature digs itself out from the rock!");
                    playerTunnelWall(new Coord_t(monster.pos.y, monster.pos.x), 1, 0);
                }
            }
        }

        // Undead only get confused from turn undead, so they should flee
        public static void monsterMoveUndead(Creature_t creature, int monster_id, ref uint rcmove)
        {
            var directions = new int[9];
            monsterGetMoveDirection(monster_id, directions);

            directions[0] = 10 - directions[0];
            directions[1] = 10 - directions[1];
            directions[2] = 10 - directions[2];
            directions[3] = rnd.randomNumber(9); // May attack only if cornered
            directions[4] = rnd.randomNumber(9);

            // don't move if it's is not supposed to move!
            if ((creature.movement & Config.monsters_move.CM_ATTACK_ONLY) == 0u)
            {
                makeMove(monster_id, directions, ref rcmove);
            }
        }

        public static void monsterMoveConfused(Creature_t creature, int monster_id, ref uint rcmove)
        {
            var directions = new int[9];

            directions[0] = rnd.randomNumber(9);
            directions[1] = rnd.randomNumber(9);
            directions[2] = rnd.randomNumber(9);
            directions[3] = rnd.randomNumber(9);
            directions[4] = rnd.randomNumber(9);

            // don't move if it's is not supposed to move!
            if ((creature.movement & Config.monsters_move.CM_ATTACK_ONLY) == 0u)
            {
                makeMove(monster_id, directions, ref rcmove);
            }
        }

        public static bool monsterDoMove(int monster_id, ref uint rcmove, Monster_t monster, Creature_t creature)
        {
            // Creature is confused or undead turned?
            if (monster.confused_amount != 0u)
            {
                if ((creature.defenses & Config.monsters_defense.CD_UNDEAD) != 0)
                {
                    monsterMoveUndead(creature, monster_id, ref rcmove);
                }
                else
                {
                    monsterMoveConfused(creature, monster_id, ref rcmove);
                }
                monster.confused_amount--;
                return true;
            }

            // Creature may cast a spell
            if ((creature.spells & Config.monsters_spells.CS_FREQ) != 0u)
            {
                return monsterCastSpell(monster_id);
            }

            return false;
        }

        public static void monsterMoveRandomly(int monster_id, ref uint rcmove, int randomness)
        {
            var directions = new int[9];

            directions[0] = rnd.randomNumber(9);
            directions[1] = rnd.randomNumber(9);
            directions[2] = rnd.randomNumber(9);
            directions[3] = rnd.randomNumber(9);
            directions[4] = rnd.randomNumber(9);

            rcmove |= (uint)randomness;

            makeMove(monster_id, directions, ref rcmove);
        }

        public static void monsterMoveNormally(int monster_id, ref uint rcmove)
        {
            var directions = new int[9];

            if (rnd.randomNumber(200) == 1)
            {
                directions[0] = rnd.randomNumber(9);
                directions[1] = rnd.randomNumber(9);
                directions[2] = rnd.randomNumber(9);
                directions[3] = rnd.randomNumber(9);
                directions[4] = rnd.randomNumber(9);
            }
            else
            {
                monsterGetMoveDirection(monster_id, directions);
            }

            rcmove |= Config.monsters_move.CM_MOVE_NORMAL;

            makeMove(monster_id, directions, ref rcmove);
        }

        public static void monsterAttackWithoutMoving(int monster_id, ref uint rcmove, uint distance_from_player)
        {
            var directions = new int[9];

            if (distance_from_player < 2)
            {
                monsterGetMoveDirection(monster_id, directions);
                makeMove(monster_id, directions, ref rcmove);
            }
            else
            {
                // Learn that the monster does does not move when
                // it should have moved, but didn't.
                rcmove |= Config.monsters_move.CM_ATTACK_ONLY;
            }
        }

        // Move the critters about the dungeon -RAK-
        public static void monsterMove(int monster_id, ref uint rcmove)
        {
            var monsters = State.Instance.monsters;
            var creatures_list = Library.Instance.Creatures.creatures_list;
            var py = State.Instance.py;
            var dg = State.Instance.dg;
            var creature_recall = State.Instance.creature_recall;

            var monster = monsters[monster_id];
            var creature = creatures_list[(int)monster.creature_id];

            // Does the critter multiply?
            // rest could be negative, to be safe, only use mod with positive values.
            var abs_rest_period = (int)std.std_abs(std.std_intmax_t(py.flags.rest));
            if ((creature.movement & Config.monsters_move.CM_MULTIPLY) != 0u && Config.monsters.MON_MAX_MULTIPLY_PER_LEVEL >= State.Instance.monster_multiply_total &&
                abs_rest_period % Config.monsters.MON_MULTIPLY_ADJUST == 0)
            {
                monsterMultiplyCritter(monster, monster_id, ref rcmove);
            }

            // if in wall, must immediately escape to a clear area
            // then monster movement finished
            if ((creature.movement & Config.monsters_move.CM_PHASE) == 0u && dg.floor[monster.pos.y][monster.pos.x].feature_id >= MIN_CAVE_WALL)
            {
                monsterMoveOutOfWall(monster, monster_id, ref rcmove);
                return;
            }

            if (monsterDoMove(monster_id, ref rcmove, monster, creature))
            {
                return;
            }

            // 75% random movement
            if ((creature.movement & Config.monsters_move.CM_75_RANDOM) != 0u && rnd.randomNumber(100) < 75)
            {
                monsterMoveRandomly(monster_id, ref rcmove, (int)Config.monsters_move.CM_75_RANDOM);
                return;
            }

            // 40% random movement
            if ((creature.movement & Config.monsters_move.CM_40_RANDOM) != 0u && rnd.randomNumber(100) < 40)
            {
                monsterMoveRandomly(monster_id, ref rcmove, (int)Config.monsters_move.CM_40_RANDOM);
                return;
            }

            // 20% random movement
            if ((creature.movement & Config.monsters_move.CM_20_RANDOM) != 0u && rnd.randomNumber(100) < 20)
            {
                monsterMoveRandomly(monster_id, ref rcmove, (int)Config.monsters_move.CM_20_RANDOM);
                return;
            }

            // Normal movement
            if ((creature.movement & Config.monsters_move.CM_MOVE_NORMAL) != 0u)
            {
                monsterMoveNormally(monster_id, ref rcmove);
                return;
            }

            // Attack, but don't move
            if ((creature.movement & Config.monsters_move.CM_ATTACK_ONLY) != 0u)
            {
                monsterAttackWithoutMoving(monster_id, ref rcmove, monster.distance_from_player);
                return;
            }

            if ((creature.movement & Config.monsters_move.CM_ONLY_MAGIC) != 0u && monster.distance_from_player < 2)
            {
                // A little hack for Quylthulgs, so that one will eventually
                // notice that they have no physical attacks.
                if (creature_recall[monster.creature_id].attacks[0] < UCHAR_MAX)
                {
                    creature_recall[monster.creature_id].attacks[0]++;
                }

                // Another little hack for Quylthulgs, so that one can
                // eventually learn their speed.
                if (creature_recall[monster.creature_id].attacks[0] > 20)
                {
                    creature_recall[monster.creature_id].movement |= Config.monsters_move.CM_ONLY_MAGIC;
                }
            }
        }

        public static void memoryUpdateRecall(Monster_t monster, bool wake, bool ignore, uint rcmove)
        {
            var creature_recall = State.Instance.creature_recall;

            if (!monster.lit)
            {
                return;
            }

            var memory = creature_recall[monster.creature_id];

            if (wake)
            {
                if (memory.wake < UCHAR_MAX)
                {
                    memory.wake++;
                }
            }
            else if (ignore)
            {
                if (memory.ignore < UCHAR_MAX)
                {
                    memory.ignore++;
                }
            }

            memory.movement |= rcmove;
        }

        public static void monsterAttackingUpdate(Monster_t monster, int monster_id, int moves)
        {
            var creatures_list = Library.Instance.Creatures.creatures_list;
            var py = State.Instance.py;
            var dg = State.Instance.dg;

            for (var i = moves; i > 0; i--)
            {
                var wake = false;
                var ignore = false;

                uint rcmove = 0;

                // Monsters trapped in rock must be given a turn also,
                // so that they will die/dig out immediately.
                if (monster.lit || monster.distance_from_player <= creatures_list[(int)monster.creature_id].area_affect_radius ||
                    (creatures_list[(int)monster.creature_id].movement & Config.monsters_move.CM_PHASE) == 0u && dg.floor[monster.pos.y][monster.pos.x].feature_id >= MIN_CAVE_WALL)
                {
                    if (monster.sleep_count > 0)
                    {
                        if (py.flags.aggravate)
                        {
                            monster.sleep_count = 0;
                        }
                        else if (py.flags.rest == 0 && py.flags.paralysis < 1 || rnd.randomNumber(50) == 1)
                        {
                            var notice = rnd.randomNumber(1024);

                            if (notice * notice * notice <= 1L << (29 - py.misc.stealth_factor))
                            {
                                monster.sleep_count -= 100 / (int)monster.distance_from_player;
                                if (monster.sleep_count > 0)
                                {
                                    ignore = true;
                                }
                                else
                                {
                                    wake = true;

                                    // force it to be exactly zero
                                    monster.sleep_count = 0;
                                }
                            }
                        }
                    }

                    if (monster.stunned_amount != 0)
                    {
                        // NOTE: Balrog = 100*100 = 10000, it always recovers instantly
                        if (rnd.randomNumber(5000) < creatures_list[(int)monster.creature_id].level * creatures_list[(int)monster.creature_id].level)
                        {
                            monster.stunned_amount = 0;
                        }
                        else
                        {
                            monster.stunned_amount--;
                        }

                        if (monster.stunned_amount == 0)
                        {
                            if (monster.lit)
                            {
                                var msg = $"The {creatures_list[(int)monster.creature_id].name} ";
                                //vtype_t msg = { '\0' };
                                //(void)sprintf(msg, "The %s ", creatures_list[monster.creature_id].name);
                                printMessage(msg + "recovers and glares at you.");
                            }
                        }
                    }
                    if (monster.sleep_count == 0 && monster.stunned_amount == 0)
                    {
                        monsterMove(monster_id, ref rcmove);
                    }
                }

                monsterUpdateVisibility(monster_id);
                memoryUpdateRecall(monster, wake, ignore, rcmove);
            }
        }

        // Creatures movement and attacking are done from here -RAK-
        public static void updateMonsters(bool attack)
        {
            var game = State.Instance.game;
            var py = State.Instance.py;
            var monsters = State.Instance.monsters;

            // Process the monsters
            for (var id = State.Instance.next_free_monster_id - 1; id >= Config.monsters.MON_MIN_INDEX_ID && !game.character_is_dead; id--)
            {
                var monster = monsters[id];

                // Get rid of an eaten/breathed on monster.  Note: Be sure not to
                // process this monster. This is necessary because we can't delete
                // monsters while scanning the monsters here.
                if (monster.hp < 0)
                {
                    dungeon.dungeonDeleteMonsterFix2(id);
                    continue;
                }

                monster.distance_from_player = (uint)dungeon.coordDistanceBetween(py.pos, new Coord_t(monster.pos.y, monster.pos.x));

                // Attack is argument passed to CREATURE
                if (attack)
                {
                    var moves = monsterMovementRate(monster.speed);

                    if (moves <= 0)
                    {
                        monsterUpdateVisibility(id);
                    }
                    else
                    {
                        monsterAttackingUpdate(monster, id, moves);
                    }
                }
                else
                {
                    monsterUpdateVisibility(id);
                }

                // Get rid of an eaten/breathed on monster. This is necessary because
                // we can't delete monsters while scanning the monsters here.
                // This monster may have been killed during monsterMove().
                if (monster.hp < 0)
                {
                    dungeon.dungeonDeleteMonsterFix2(id);
                    continue;
                }
            }
        }

        // Decreases monsters hit points and deletes monster if needed.
        // (Picking on my babies.) -RAK-
        public static int monsterTakeHit(int monster_id, int damage)
        {
            var monsters = State.Instance.monsters;
            var py = State.Instance.py;
            var creatures_list = Library.Instance.Creatures.creatures_list;

            var monster = monsters[monster_id];
            var creature = creatures_list[(int)monster.creature_id];

            monster.sleep_count = 0;
            monster.hp -= damage;

            if (monster.hp >= 0)
            {
                return -1;
            }

            var treasure_flags = monsterDeath(new Coord_t(monster.pos.y, monster.pos.x), creature.movement);

            var memory = State.Instance.creature_recall[monster.creature_id];

            if (py.flags.blind < 1 && monster.lit || (creature.movement & Config.monsters_move.CM_WIN) != 0u)
            {
                var tmp = (uint)((memory.movement & Config.monsters_move.CM_TREASURE) >> (int)Config.monsters_move.CM_TR_SHIFT);

                if (tmp > (treasure_flags & Config.monsters_move.CM_TREASURE) >> (int)Config.monsters_move.CM_TR_SHIFT)
                {
                    treasure_flags = (uint)((treasure_flags & ~Config.monsters_move.CM_TREASURE) | (tmp << (int)Config.monsters_move.CM_TR_SHIFT));
                }

                memory.movement = (uint)((memory.movement & ~Config.monsters_move.CM_TREASURE) | treasure_flags);

                if (memory.kills < SHRT_MAX)
                {
                    memory.kills++;
                }
            }

            playerGainKillExperience(creature);

            // can't call displayCharacterExperience() here, as that would result in "new level"
            // message appearing before "monster dies" message.
            var m_take_hit = (int)monster.creature_id;

            // in case this is called from within updateMonsters(), this is a horrible
            // hack, the monsters/updateMonsters() code needs to be rewritten.
            if (State.Instance.hack_monptr < monster_id)
            {
                dungeon.dungeonDeleteMonster(monster_id);
            }
            else
            {
                dungeon.dungeonDeleteMonsterFix1(monster_id);
            }

            return m_take_hit;
        }

        public static int monsterDeathItemDropType(uint flags)
        {
            int obj;

            if ((flags & Config.monsters_move.CM_CARRY_OBJ) != 0u)
            {
                obj = 1;
            }
            else
            {
                obj = 0;
            }

            if ((flags & Config.monsters_move.CM_CARRY_GOLD) != 0u)
            {
                obj += 2;
            }

            if ((flags & Config.monsters_move.CM_SMALL_OBJ) != 0u)
            {
                obj += 4;
            }

            return obj;
        }

        public static int monsterDeathItemDropCount(uint flags)
        {
            var count = 0;

            if ((flags & Config.monsters_move.CM_60_RANDOM) != 0u && rnd.randomNumber(100) < 60)
            {
                count++;
            }

            if ((flags & Config.monsters_move.CM_90_RANDOM) != 0u && rnd.randomNumber(100) < 90)
            {
                count++;
            }

            if ((flags & Config.monsters_move.CM_1D2_OBJ) != 0u)
            {
                count += rnd.randomNumber(2);
            }

            if ((flags & Config.monsters_move.CM_2D2_OBJ) != 0u)
            {
                count += dice.diceRoll(new Dice_t(2, 2));
            }

            if ((flags & Config.monsters_move.CM_4D2_OBJ) != 0u)
            {
                count += dice.diceRoll(new Dice_t(4, 2));
            }

            return count;
        }

        // Allocates objects upon a creatures death -RAK-
        // Oh well,  another creature bites the dust. Reward the
        // victor based on flags set in the main creature record.
        //
        // Returns a mask of bits from the given flags which indicates what the
        // monster is seen to have dropped.  This may be added to monster memory.
        public static uint monsterDeath(Coord_t coord, uint flags)
        {
            var item_type = monsterDeathItemDropType(flags);
            var item_count = monsterDeathItemDropCount(flags);

            uint dropped_item_id = 0;

            var game = State.Instance.game;

            if (item_count > 0)
            {
                dropped_item_id = (uint)dungeonPlacer.dungeonSummonObject(coord, item_count, item_type);
            }

            // maybe the player died in mid-turn
            if ((flags & Config.monsters_move.CM_WIN) != 0u && !game.character_is_dead)
            {
                game.total_winner = true;

                printCharacterWinner();

                printMessage("*** CONGRATULATIONS *** You have won the game.");
                printMessage("You cannot save this game, but you may retire when ready.");
            }

            if (dropped_item_id == 0)
            {
                return 0;
            }

            uint return_flags = 0;

            if ((dropped_item_id & 255) != 0u)
            {
                return_flags |= Config.monsters_move.CM_CARRY_OBJ;

                if ((item_type & 0x04) != 0)
                {
                    return_flags |= Config.monsters_move.CM_SMALL_OBJ;
                }
            }

            if (dropped_item_id >= 256)
            {
                return_flags |= Config.monsters_move.CM_CARRY_GOLD;
            }

            var number_of_items = (int)(dropped_item_id % 256 + dropped_item_id / 256);
            number_of_items = number_of_items << (int)Config.monsters_move.CM_TR_SHIFT;

            return return_flags | (uint)number_of_items;
        }

        public static void printMonsterActionText(string name, string action)
        {
            printMessage(name + " " + action);
        }

        public static string monsterNameDescription(string real_name, bool is_lit)
        {
            if (is_lit)
            {
                return "The " + real_name;
            }
            return "It";
        }

        // Sleep creatures adjacent to player -RAK-
        public static bool monsterSleep(Coord_t coord)
        { 
            var dg = State.Instance.dg;
            var monsters = State.Instance.monsters;
            var creatures_list = Library.Instance.Creatures.creatures_list;

            var asleep = false;

            for (var y = coord.y - 1; y <= coord.y + 1 && y < MAX_HEIGHT; y++)
            {
                for (var x = coord.x - 1; x <= coord.x + 1 && x < MAX_WIDTH; x++)
                {
                    var monster_id = dg.floor[y][x].creature_id;

                    if (monster_id <= 1)
                    {
                        continue;
                    }

                    var monster = monsters[monster_id];
                    var creature = creatures_list[(int)monster.creature_id];

                    var name = monsterNameDescription(creature.name, monster.lit);

                    if (rnd.randomNumber(MON_MAX_LEVELS) < creature.level || (creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                    {
                        if (monster.lit && (creature.defenses & Config.monsters_defense.CD_NO_SLEEP) != 0)
                        {
                            State.Instance.creature_recall[monster.creature_id].defenses |= Config.monsters_defense.CD_NO_SLEEP;
                        }

                        printMonsterActionText(name, "is unaffected.");
                    }
                    else
                    {
                        monster.sleep_count = 500;
                        asleep = true;

                        printMonsterActionText(name, "falls asleep.");
                    }
                }
            }

            return asleep;
        }

        public static bool executeAttackOnPlayer(uint creature_level, ref int monster_hp, int monster_id, int attack_type, int damage, string death_description, bool noticed)
        {
            var py = State.Instance.py;
            var item_pos_start = 0;
            var item_pos_end = 0;
            int gold;

            switch (attack_type)
            {
                case 1: // Normal attack
                        // round half-way case down
                    damage -= (py.misc.ac + py.misc.magical_ac) * damage / 200;
                    playerTakesHit(damage, death_description);
                    break;
                case 2: // Lose Strength
                    playerTakesHit(damage, death_description);
                    if (py.flags.sustain_str)
                    {
                        printMessage("You feel weaker for a moment, but it passes.");
                    }
                    else if (rnd.randomNumber(2) == 1)
                    {
                        printMessage("You feel weaker.");
                        playerStatRandomDecrease((int)PlayerAttr.STR);
                    }
                    else
                    {
                        noticed = false;
                    }
                    break;
                case 3: // Confusion attack
                    playerTakesHit(damage, death_description);
                    if (rnd.randomNumber(2) == 1)
                    {
                        if (py.flags.confused < 1)
                        {
                            printMessage("You feel confused.");
                            py.flags.confused += rnd.randomNumber((int)creature_level);
                        }
                        else
                        {
                            noticed = false;
                        }
                        py.flags.confused += 3;
                    }
                    else
                    {
                        noticed = false;
                    }
                    break;
                case 4: // Fear attack
                    playerTakesHit(damage, death_description);
                    if (playerSavingThrow())
                    {
                        printMessage("You resist the effects!");
                    }
                    else if (py.flags.afraid < 1)
                    {
                        printMessage("You are suddenly afraid!");
                        py.flags.afraid += 3 + rnd.randomNumber((int)creature_level);
                    }
                    else
                    {
                        py.flags.afraid += 3;
                        noticed = false;
                    }
                    break;
                case 5: // Fire attack
                    printMessage("You are enveloped in flames!");
                    inventory.damageFire(damage, death_description);
                    break;
                case 6: // Acid attack
                    printMessage("You are covered in acid!");
                    inventory.damageAcid(damage, death_description);
                    break;
                case 7: // Cold attack
                    printMessage("You are covered with frost!");
                    inventory.damageCold(damage, death_description);
                    break;
                case 8: // Lightning attack
                    printMessage("Lightning strikes you!");
                    inventory.damageLightningBolt(damage, death_description);
                    break;
                case 9: // Corrosion attack
                    printMessage("A stinging red gas swirls about you.");
                    inventory.damageCorrodingGas(death_description);
                    playerTakesHit(damage, death_description);
                    break;
                case 10: // Blindness attack
                    playerTakesHit(damage, death_description);
                    if (py.flags.blind < 1)
                    {
                        py.flags.blind += 10 + rnd.randomNumber((int)creature_level);
                        printMessage("Your eyes begin to sting.");
                    }
                    else
                    {
                        py.flags.blind += 5;
                        noticed = false;
                    }
                    break;
                case 11: // Paralysis attack
                    playerTakesHit(damage, death_description);
                    if (playerSavingThrow())
                    {
                        printMessage("You resist the effects!");
                    }
                    else if (py.flags.paralysis < 1)
                    {
                        if (py.flags.free_action)
                        {
                            printMessage("You are unaffected.");
                        }
                        else
                        {
                            py.flags.paralysis = (int)(rnd.randomNumber((int)creature_level) + 3);
                            printMessage("You are paralyzed.");
                        }
                    }
                    else
                    {
                        noticed = false;
                    }
                    break;
                case 12: // Steal Money
                    if (py.flags.paralysis < 1 && rnd.randomNumber(124) < py.stats.used[(int)PlayerAttr.DEX])
                    {
                        printMessage("You quickly protect your money pouch!");
                    }
                    else
                    {
                        gold = py.misc.au / 10 + rnd.randomNumber(25);
                        if (gold > py.misc.au)
                        {
                            py.misc.au = 0;
                        }
                        else
                        {
                            py.misc.au -= gold;
                        }
                        printMessage("Your purse feels lighter.");
                        printCharacterGoldValue();
                    }
                    if (rnd.randomNumber(2) == 1)
                    {
                        printMessage("There is a puff of smoke!");
                        eventPublisher.Publish(new TeleportAwayMonsterCommand(monster_id, (int)Config.monsters.MON_MAX_SIGHT));
                        //spellTeleportAwayMonster(monster_id, (int)Config.monsters.MON_MAX_SIGHT);
                    }
                    break;
                case 13: // Steal Object
                    if (py.flags.paralysis < 1 && rnd.randomNumber(124) < py.stats.used[(int)PlayerAttr.DEX])
                    {
                        printMessage("You grab hold of your backpack!");
                    }
                    else
                    {
                        inventoryManager.inventoryDestroyItem(rnd.randomNumber(py.pack.unique_items) - 1);
                        printMessage("Your backpack feels lighter.");
                    }
                    if (rnd.randomNumber(2) == 1)
                    {
                        printMessage("There is a puff of smoke!");
                        eventPublisher.Publish(new TeleportAwayMonsterCommand(monster_id, (int)Config.monsters.MON_MAX_SIGHT));
                        //spellTeleportAwayMonster(monster_id, (int)Config.monsters.MON_MAX_SIGHT);
                    }
                    break;
                case 14: // Poison
                    playerTakesHit(damage, death_description);
                    printMessage("You feel very sick.");
                    py.flags.poisoned += rnd.randomNumber((int)creature_level) + 5;
                    break;
                case 15: // Lose dexterity
                    playerTakesHit(damage, death_description);
                    if (py.flags.sustain_dex)
                    {
                        printMessage("You feel clumsy for a moment, but it passes.");
                    }
                    else
                    {
                        printMessage("You feel more clumsy.");
                        playerStatRandomDecrease((int)PlayerAttr.DEX);
                    }
                    break;
                case 16: // Lose constitution
                    playerTakesHit(damage, death_description);
                    if (py.flags.sustain_con)
                    {
                        printMessage("Your body resists the effects of the disease.");
                    }
                    else
                    {
                        printMessage("Your health is damaged!");
                        playerStatRandomDecrease((int)PlayerAttr.CON);
                    }
                    break;
                case 17: // Lose intelligence
                    playerTakesHit(damage, death_description);
                    printMessage("You have trouble thinking clearly.");
                    if (py.flags.sustain_int)
                    {
                        printMessage("But your mind quickly clears.");
                    }
                    else
                    {
                        playerStatRandomDecrease((int)PlayerAttr.INT);
                    }
                    break;
                case 18: // Lose wisdom
                    playerTakesHit(damage, death_description);
                    if (py.flags.sustain_wis)
                    {
                        printMessage("Your wisdom is sustained.");
                    }
                    else
                    {
                        printMessage("Your wisdom is drained.");
                        playerStatRandomDecrease((int)PlayerAttr.WIS);
                    }
                    break;
                case 19: // Lose experience
                    printMessage("You feel your life draining away!");
                    eventPublisher.Publish(new LoseExpCommand(
                        damage + py.misc.exp / 100 * (int)Config.monsters.MON_PLAYER_EXP_DRAINED_PER_HIT
                    ));
                    //spellLoseEXP(damage + py.misc.exp / 100 * (int)Config.monsters.MON_PLAYER_EXP_DRAINED_PER_HIT);
                    break;
                case 20: // Aggravate monster
                    eventPublisher.Publish(new AggravateMonstersCommand(20));
                    //spellAggravateMonsters(20);
                    break;
                case 21: // Disenchant
                    if (inventoryManager.executeDisenchantAttack())
                    {
                        printMessage("There is a static feeling in the air.");
                        playerRecalculateBonuses();
                    }
                    else
                    {
                        noticed = false;
                    }
                    break;
                case 22: // Eat food
                    if (inventoryManager.inventoryFindRange((int)TV_FOOD, TV_NEVER, out item_pos_start, out item_pos_end))
                    {
                        inventoryManager.inventoryDestroyItem(item_pos_start);
                        printMessage("It got at your rations!");
                    }
                    else
                    {
                        noticed = false;
                    }
                    break;
                case 23: // Eat light
                    noticed = inventory.inventoryDiminishLightAttack(noticed);
                    break;
                case 24: // Eat charges
                    noticed = inventory.inventoryDiminishChargesAttack(creature_level, ref monster_hp, noticed);
                    break;
                // NOTE: default handles this case
                // case 99:
                //     noticed = false;
                //     break;
                default:
                    noticed = false;
                    break;
            }

            return noticed;
        }

    }
}
