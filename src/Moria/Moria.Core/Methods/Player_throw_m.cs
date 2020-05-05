using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.Monster;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Player_m;

namespace Moria.Core.Methods
{
    public static class Player_throw_m
    {
        public static void SetDependencies(
            IDice dice,
            IDungeon dungeon,
            IGame game,
            IGameObjects gameObjects,
            IHelpers helpers,
            IIdentification identification,
            IInventoryManager inventoryManager,
            IPlayerMagic playerMagic,
            IRnd rnd,
            ITerminal terminal,
            ITerminalEx terminalEx,
            IUiInventory uiInventory,

            IEventPublisher eventPublisher
        )
        {
            Player_throw_m.dice = dice;
            Player_throw_m.dungeon = dungeon;
            Player_throw_m.game = game;
            Player_throw_m.gameObjects = gameObjects;
            Player_throw_m.helpers = helpers;
            Player_throw_m.identification = identification;
            Player_throw_m.inventoryManager = inventoryManager;
            Player_throw_m.playerMagic = playerMagic;
            Player_throw_m.rnd = rnd;
            Player_throw_m.terminal = terminal;
            Player_throw_m.terminalEx = terminalEx;
            Player_throw_m.uiInventory = uiInventory;

            Player_throw_m.eventPublisher = eventPublisher;
        }

        private static IDice dice;
        private static IDungeon dungeon;
        private static IGame game;
        private static IGameObjects gameObjects;
        private static IHelpers helpers;
        private static IIdentification identification;
        private static IInventoryManager inventoryManager;
        private static IPlayerMagic playerMagic;
        private static IRnd rnd;
        private static ITerminal terminal;
        private static ITerminalEx terminalEx;
        private static IUiInventory uiInventory;

        private static IEventPublisher eventPublisher;

        private static void inventoryThrow(int item_id, out Inventory_t treasure)
        {
            var py = State.Instance.py;

            var item = py.inventory[item_id];

            //*treasure = *item;
            treasure = item;

            if (item.items_count > 1)
            {
                treasure.items_count = 1;
                item.items_count--;
                py.pack.weight -= (int)item.weight;
                py.flags.status |= Config.player_status.PY_STR_WGT;
            }
            else
            {
                inventoryManager.inventoryDestroyItem(item_id);
            }
        }

        // Obtain the hit and damage bonuses and the maximum distance for a thrown missile.
        private static void weaponMissileFacts(Inventory_t item, out int base_to_hit, out int plus_to_hit, out int damage, out int distance)
        {
            var py = State.Instance.py;

            var weight = (int)item.weight;
            if (weight < 1)
            {
                weight = 1;
            }

            // Throwing objects
            damage = dice.diceRoll(item.damage) + item.to_damage;
            base_to_hit = py.misc.bth_with_bows * 75 / 100;
            plus_to_hit = py.misc.plusses_to_hit + item.to_hit;

            // Add this back later if the correct throwing device. -CJS-
            if (py.inventory[(int)PlayerEquipment.Wield].category_id != TV_NOTHING)
            {
                plus_to_hit -= py.inventory[(int)PlayerEquipment.Wield].to_hit;
            }

            distance = ((int)py.stats.used[(int)PlayerAttr.STR] + 20) * 10 / weight;
            if (distance > 10)
            {
                distance = 10;
            }

            // multiply damage bonuses instead of adding, when have proper
            // missile/weapon combo, this makes them much more useful

            // Using Bows, slings, or crossbows?
            if (py.inventory[(int)PlayerEquipment.Wield].category_id != TV_BOW)
            {
                return;
            }

            switch (py.inventory[(int)PlayerEquipment.Wield].misc_use)
            {
                case 1:
                    if (item.category_id == TV_SLING_AMMO)
                    { // Sling and ammo
                        base_to_hit = py.misc.bth_with_bows;
                        plus_to_hit += 2 * py.inventory[(int)PlayerEquipment.Wield].to_hit;
                        damage += py.inventory[(int)PlayerEquipment.Wield].to_damage;
                        damage = damage * 2;
                        distance = 20;
                    }
                    break;
                case 2:
                    if (item.category_id == TV_ARROW)
                    { // Short Bow and Arrow
                        base_to_hit = py.misc.bth_with_bows;
                        plus_to_hit += 2 * py.inventory[(int)PlayerEquipment.Wield].to_hit;
                        damage += py.inventory[(int)PlayerEquipment.Wield].to_damage;
                        damage = damage * 2;
                        distance = 25;
                    }
                    break;
                case 3:
                    if (item.category_id == TV_ARROW)
                    { // Long Bow and Arrow
                        base_to_hit = py.misc.bth_with_bows;
                        plus_to_hit += 2 * py.inventory[(int)PlayerEquipment.Wield].to_hit;
                        damage += py.inventory[(int)PlayerEquipment.Wield].to_damage;
                        damage = damage * 3;
                        distance = 30;
                    }
                    break;
                case 4:
                    if (item.category_id == TV_ARROW)
                    { // Composite Bow and Arrow
                        base_to_hit = py.misc.bth_with_bows;
                        plus_to_hit += 2 * py.inventory[(int)PlayerEquipment.Wield].to_hit;
                        damage += py.inventory[(int)PlayerEquipment.Wield].to_damage;
                        damage = damage * 4;
                        distance = 35;
                    }
                    break;
                case 5:
                    if (item.category_id == TV_BOLT)
                    { // Light Crossbow and Bolt
                        base_to_hit = py.misc.bth_with_bows;
                        plus_to_hit += 2 * py.inventory[(int)PlayerEquipment.Wield].to_hit;
                        damage += py.inventory[(int)PlayerEquipment.Wield].to_damage;
                        damage = damage * 3;
                        distance = 25;
                    }
                    break;
                case 6:
                    if (item.category_id == TV_BOLT)
                    { // Heavy Crossbow and Bolt
                        base_to_hit = py.misc.bth_with_bows;
                        plus_to_hit += 2 * py.inventory[(int)PlayerEquipment.Wield].to_hit;
                        damage += py.inventory[(int)PlayerEquipment.Wield].to_damage;
                        damage = damage * 4;
                        distance = 35;
                    }
                    break;
                default:
                    // NOOP
                    break;
            }
        }

        private static void inventoryDropOrThrowItem(Coord_t coord, Inventory_t item)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var position = new Coord_t(coord.y, coord.x);

            var flag = false;

            if (rnd.randomNumber(10) > 1)
            {
                for (var k = 0; !flag && k <= 9;)
                {
                    if (dungeon.coordInBounds(position))
                    {
                        if (dg.floor[position.y][position.x].feature_id <= MAX_OPEN_SPACE && dg.floor[position.y][position.x].treasure_id == 0)
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        position.y = coord.y + rnd.randomNumber(3) - 2;
                        position.x = coord.x + rnd.randomNumber(3) - 2;
                        k++;
                    }
                }
            }

            if (flag)
            {
                var cur_pos = gameObjects.popt();
                dg.floor[position.y][position.x].treasure_id = (uint)cur_pos;
                game.treasure.list[cur_pos] = item;
                dungeon.dungeonLiteSpot(position);
            }
            else
            {
                //obj_desc_t description = { '\0' };
                //obj_desc_t msg = { '\0' };
                identification.itemDescription(out var description, item, false);

                var msg = $"The {description} disappears.";
                //(void)sprintf(msg, "The %s disappears.", description);
                terminal.printMessage(msg);
            }
        }

        // Throw an object across the dungeon. -RAK-
        // Note: Flasks of oil do fire damage
        // Note: Extra damage and chance of hitting when missiles are used
        // with correct weapon. i.e. wield bow and throw arrow.
        public static void playerThrowItem()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;
            if (py.pack.unique_items == 0)
            {
                terminal.printMessage("But you are not carrying anything.");
                game.player_free_turn = true;
                return;
            }

            if (!uiInventory.inventoryGetInputForItemId(out var item_id, "Fire/Throw which one?", 0, py.pack.unique_items - 1, /*CNIL*/null, /*CNIL*/null))
            {
                return;
            }

            var dir = 0;
            if (!Player_throw_m.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
            {
                return;
            }

            identification.itemTypeRemainingCountDescription(item_id);

            if (py.flags.confused > 0)
            {
                terminal.printMessage("You are confused.");
                dir = rnd.getRandomDirection();
            }

            inventoryThrow(item_id, out var thrown_item);

            weaponMissileFacts(thrown_item, out var tbth, out var tpth, out var tdam, out var tdis);

            var tile_char = (char)thrown_item.sprite;
            var current_distance = 0;

            var coord = py.pos.Clone();
            var old_coord = py.pos.Clone();

            var flag = false;

            while (!flag)
            {
                helpers.movePosition(dir, ref coord);

                if (current_distance + 1 > tdis)
                {
                    break;
                }

                current_distance++;
                dungeon.dungeonLiteSpot(old_coord);

                var tile = dg.floor[coord.y][coord.x];

                if (tile.feature_id <= MAX_OPEN_SPACE && !flag)
                {
                    if (tile.creature_id > 1)
                    {
                        flag = true;

                        var m_ptr = State.Instance.monsters[tile.creature_id];

                        tbth -= current_distance;

                        // if monster not lit, make it much more difficult to hit, subtract
                        // off most bonuses, and reduce bth_with_bows depending on distance.
                        if (!m_ptr.lit)
                        {
                            tbth /= current_distance + 2;
                            tbth -= (int)py.misc.level * Library.Instance.Player.class_level_adj[(int)py.misc.class_id][(int)PlayerClassLevelAdj.BTHB] / 2;
                            tbth -= tpth * ((int)BTH_PER_PLUS_TO_HIT_ADJUST - 1);
                        }

                        if (playerTestBeingHit(tbth, (int)py.misc.level, tpth, (int)Library.Instance.Creatures.creatures_list[(int)m_ptr.creature_id].ac, (int)PlayerClassLevelAdj.BTHB))
                        {
                            var damage = (int)m_ptr.creature_id;

                            string msg;
                            //obj_desc_t description = { '\0' };
                            //obj_desc_t msg = { '\0' };
                            identification.itemDescription(out var description, thrown_item, false);

                            // Does the player know what they're fighting?
                            bool visible;
                            if (!m_ptr.lit)
                            {
                                msg = $"You hear a cry as the {description} finds a mark.";
                                //(void)sprintf(msg, "You hear a cry as the %s finds a mark.", description);
                                visible = false;
                            }
                            else
                            {
                                msg = $"The {description} hits the {Library.Instance.Creatures.creatures_list[damage].name}.";
                                //(void)sprintf(msg, "The %s hits the %s.", description, creatures_list[damage].name);
                                visible = true;
                            }
                            terminal.printMessage(msg);

                            tdam = playerMagic.itemMagicAbilityDamage(thrown_item, tdam, damage);
                            tdam = playerWeaponCriticalBlow((int)thrown_item.weight, tpth, tdam, (int)PlayerClassLevelAdj.BTHB);

                            if (tdam < 0)
                            {
                                tdam = 0;
                            }

                            damage = eventPublisher.PublishWithOutputInt(
                                new TakeHitCommand((int)tile.creature_id, tdam)
                            );
                            //damage = monsterTakeHit((int)tile.creature_id, tdam);

                            if (damage >= 0)
                            {
                                if (!visible)
                                {
                                    terminal.printMessage("You have killed something!");
                                }
                                else
                                {
                                    msg = $"You have killed the {Library.Instance.Creatures.creatures_list[damage].name}.";
                                    //(void)sprintf(msg, "You have killed the %s.", creatures_list[damage].name);
                                    terminal.printMessage(msg);
                                }
                                terminalEx.displayCharacterExperience();
                            }
                        }
                        else
                        {
                            inventoryDropOrThrowItem(old_coord, thrown_item);
                        }
                    }
                    else
                    {
                        // do not test tile.field_mark here

                        if (helpers.coordInsidePanel(coord) && py.flags.blind < 1 && (tile.temporary_light || tile.permanent_light))
                        {
                            terminal.panelPutTile(tile_char, coord);
                            terminal.putQIO(); // show object moving
                        }
                    }
                }
                else
                {
                    flag = true;
                    inventoryDropOrThrowItem(old_coord, thrown_item);
                }

                old_coord.y = coord.y;
                old_coord.x = coord.x;
            }
        }
    }
}
