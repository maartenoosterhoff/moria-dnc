using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Player_magic_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;

namespace Moria.Core.Methods
{
    public static class Player_throw_m
    {
        public static void SetDependencies(
            IDice dice,
            IGame game,
            IRnd rnd,
            IUiInventory uiInventory
        )
        {
            Player_throw_m.dice = dice;
            Player_throw_m.game = game;
            Player_throw_m.rnd = rnd;
            Player_throw_m.uiInventory = uiInventory;
        }

        private static IDice dice;
        private static IGame game;
        private static IRnd rnd;
        private static IUiInventory uiInventory;

        public static void inventoryThrow(int item_id, out Inventory_t treasure)
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
                inventoryDestroyItem(item_id);
            }
        }

        // Obtain the hit and damage bonuses and the maximum distance for a thrown missile.
        public static void weaponMissileFacts(Inventory_t item, ref int base_to_hit, ref int plus_to_hit, ref int damage, ref int distance)
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

            distance = ((((int)py.stats.used[(int)PlayerAttr.STR] + 20) * 10) / weight);
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

        static void inventoryDropOrThrowItem(Coord_t coord, Inventory_t item)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var position = new Coord_t(coord.y, coord.x);

            var flag = false;

            if (rnd.randomNumber(10) > 1)
            {
                for (var k = 0; !flag && k <= 9;)
                {
                    if (coordInBounds(position))
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
                var cur_pos = popt();
                dg.floor[position.y][position.x].treasure_id = (uint)cur_pos;
                game.treasure.list[cur_pos] = item;
                dungeonLiteSpot(position);
            }
            else
            {
                var description = string.Empty;
                //obj_desc_t description = { '\0' };
                //obj_desc_t msg = { '\0' };
                itemDescription(ref description, item, false);

                var msg = $"The {description} disappears.";
                //(void)sprintf(msg, "The %s disappears.", description);
                printMessage(msg);
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
                printMessage("But you are not carrying anything.");
                game.player_free_turn = true;
                return;
            }

            var item_id = 0;
            if (!uiInventory.inventoryGetInputForItemId(ref item_id, "Fire/Throw which one?", 0, py.pack.unique_items - 1, /*CNIL*/null, /*CNIL*/null))
            {
                return;
            }

            var dir = 0;
            if (!Player_throw_m.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
            {
                return;
            }

            itemTypeRemainingCountDescription(item_id);

            if (py.flags.confused > 0)
            {
                printMessage("You are confused.");
                dir = rnd.getRandomDirection();
            }

            Inventory_t thrown_item;
            inventoryThrow(item_id, out thrown_item);

            int tbth = 0, tpth = 0, tdam = 0, tdis = 0;
            weaponMissileFacts(thrown_item, ref tbth, ref tpth, ref tdam, ref tdis);

            var tile_char = (char)thrown_item.sprite;
            bool visible;
            var current_distance = 0;

            var coord = py.pos.Clone();
            var old_coord = py.pos.Clone();

            var flag = false;

            while (!flag)
            {
                playerMovePosition(dir, ref coord);

                if (current_distance + 1 > tdis)
                {
                    break;
                }

                current_distance++;
                dungeonLiteSpot(old_coord);

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

                            var description = string.Empty;
                            string msg;
                            //obj_desc_t description = { '\0' };
                            //obj_desc_t msg = { '\0' };
                            itemDescription(ref description, thrown_item, false);

                            // Does the player know what they're fighting?
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
                            printMessage(msg);

                            tdam = itemMagicAbilityDamage(thrown_item, tdam, damage);
                            tdam = playerWeaponCriticalBlow((int)thrown_item.weight, tpth, tdam, (int)PlayerClassLevelAdj.BTHB);

                            if (tdam < 0)
                            {
                                tdam = 0;
                            }

                            damage = monsterTakeHit((int)tile.creature_id, tdam);

                            if (damage >= 0)
                            {
                                if (!visible)
                                {
                                    printMessage("You have killed something!");
                                }
                                else
                                {
                                    msg = $"You have killed the {Library.Instance.Creatures.creatures_list[damage].name}.";
                                    //(void)sprintf(msg, "You have killed the %s.", creatures_list[damage].name);
                                    printMessage(msg);
                                }
                                displayCharacterExperience();
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

                        if (coordInsidePanel(coord) && py.flags.blind < 1 && (tile.temporary_light || tile.permanent_light))
                        {
                            panelPutTile(tile_char, coord);
                            putQIO(); // show object moving
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
