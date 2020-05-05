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
    public interface IPlayerThrow
    {
        void playerThrowItem();
    }

    public class Player_throw_m : IPlayerThrow
    {
        private readonly IDice dice;
        private readonly IDungeon dungeon;
        private readonly IEventPublisher eventPublisher;
        private readonly IGame game;
        private readonly IGameObjects gameObjects;
        private readonly IHelpers helpers;
        private readonly IIdentification identification;
        private readonly IInventoryManager inventoryManager;
        private readonly IPlayerMagic playerMagic;
        private readonly IRnd rnd;
        private readonly ITerminal terminal;
        private readonly ITerminalEx terminalEx;
        private readonly IUiInventory uiInventory;

        public Player_throw_m(
            IDice dice,
            IDungeon dungeon,
            IEventPublisher eventPublisher,
            IGame game,
            IGameObjects gameObjects,
            IHelpers helpers,
            IIdentification identification,
            IInventoryManager inventoryManager,
            IPlayerMagic playerMagic,
            IRnd rnd,
            ITerminal terminal,
            ITerminalEx terminalEx,
            IUiInventory uiInventory
        )
        {
            this.dice = dice;
            this.dungeon = dungeon;
            this.eventPublisher = eventPublisher;
            this.game = game;
            this.gameObjects = gameObjects;
            this.helpers = helpers;
            this.identification = identification;
            this.inventoryManager = inventoryManager;
            this.playerMagic = playerMagic;
            this.rnd = rnd;
            this.terminal = terminal;
            this.terminalEx = terminalEx;
            this.uiInventory = uiInventory;
        }

        private void inventoryThrow(int item_id, out Inventory_t treasure)
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
                this.inventoryManager.inventoryDestroyItem(item_id);
            }
        }

        // Obtain the hit and damage bonuses and the maximum distance for a thrown missile.
        private void weaponMissileFacts(Inventory_t item, out int base_to_hit, out int plus_to_hit, out int damage, out int distance)
        {
            var py = State.Instance.py;

            var weight = (int)item.weight;
            if (weight < 1)
            {
                weight = 1;
            }

            // Throwing objects
            damage = this.dice.diceRoll(item.damage) + item.to_damage;
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

        private void inventoryDropOrThrowItem(Coord_t coord, Inventory_t item)
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;
            var position = new Coord_t(coord.y, coord.x);

            var flag = false;

            if (this.rnd.randomNumber(10) > 1)
            {
                for (var k = 0; !flag && k <= 9;)
                {
                    if (this.dungeon.coordInBounds(position))
                    {
                        if (dg.floor[position.y][position.x].feature_id <= MAX_OPEN_SPACE && dg.floor[position.y][position.x].treasure_id == 0)
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        position.y = coord.y + this.rnd.randomNumber(3) - 2;
                        position.x = coord.x + this.rnd.randomNumber(3) - 2;
                        k++;
                    }
                }
            }

            if (flag)
            {
                var cur_pos = this.gameObjects.popt();
                dg.floor[position.y][position.x].treasure_id = (uint)cur_pos;
                game.treasure.list[cur_pos] = item;
                this.dungeon.dungeonLiteSpot(position);
            }
            else
            {
                //obj_desc_t description = { '\0' };
                //obj_desc_t msg = { '\0' };
                this.identification.itemDescription(out var description, item, false);

                var msg = $"The {description} disappears.";
                //(void)sprintf(msg, "The %s disappears.", description);
                this.terminal.printMessage(msg);
            }
        }

        // Throw an object across the dungeon. -RAK-
        // Note: Flasks of oil do fire damage
        // Note: Extra damage and chance of hitting when missiles are used
        // with correct weapon. i.e. wield bow and throw arrow.
        public void playerThrowItem()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;
            var dg = State.Instance.dg;
            if (py.pack.unique_items == 0)
            {
                this.terminal.printMessage("But you are not carrying anything.");
                game.player_free_turn = true;
                return;
            }

            if (!this.uiInventory.inventoryGetInputForItemId(out var item_id, "Fire/Throw which one?", 0, py.pack.unique_items - 1, /*CNIL*/null, /*CNIL*/null))
            {
                return;
            }

            var dir = 0;
            if (!this.game.getDirectionWithMemory(/*CNIL*/null, ref dir))
            {
                return;
            }

            this.identification.itemTypeRemainingCountDescription(item_id);

            if (py.flags.confused > 0)
            {
                this.terminal.printMessage("You are confused.");
                dir = this.rnd.getRandomDirection();
            }

            this.inventoryThrow(item_id, out var thrown_item);

            this.weaponMissileFacts(thrown_item, out var tbth, out var tpth, out var tdam, out var tdis);

            var tile_char = (char)thrown_item.sprite;
            var current_distance = 0;

            var coord = py.pos.Clone();
            var old_coord = py.pos.Clone();

            var flag = false;

            while (!flag)
            {
                this.helpers.movePosition(dir, ref coord);

                if (current_distance + 1 > tdis)
                {
                    break;
                }

                current_distance++;
                this.dungeon.dungeonLiteSpot(old_coord);

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
                            this.identification.itemDescription(out var description, thrown_item, false);

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

                            this.terminal.printMessage(msg);

                            tdam = this.playerMagic.itemMagicAbilityDamage(thrown_item, tdam, damage);
                            tdam = playerWeaponCriticalBlow((int)thrown_item.weight, tpth, tdam, (int)PlayerClassLevelAdj.BTHB);

                            if (tdam < 0)
                            {
                                tdam = 0;
                            }

                            damage = this.eventPublisher.PublishWithOutputInt(
                                new TakeHitCommand((int)tile.creature_id, tdam)
                            );
                            //damage = monsterTakeHit((int)tile.creature_id, tdam);

                            if (damage >= 0)
                            {
                                if (!visible)
                                {
                                    this.terminal.printMessage("You have killed something!");
                                }
                                else
                                {
                                    msg = $"You have killed the {Library.Instance.Creatures.creatures_list[damage].name}.";
                                    //(void)sprintf(msg, "You have killed the %s.", creatures_list[damage].name);
                                    this.terminal.printMessage(msg);
                                }

                                this.terminalEx.displayCharacterExperience();
                            }
                        }
                        else
                        {
                            this.inventoryDropOrThrowItem(old_coord, thrown_item);
                        }
                    }
                    else
                    {
                        // do not test tile.field_mark here

                        if (this.helpers.coordInsidePanel(coord) && py.flags.blind < 1 && (tile.temporary_light || tile.permanent_light))
                        {
                            this.terminal.panelPutTile(tile_char, coord);
                            this.terminal.putQIO(); // show object moving
                        }
                    }
                }
                else
                {
                    flag = true;
                    this.inventoryDropOrThrowItem(old_coord, thrown_item);
                }

                old_coord.y = coord.y;
                old_coord.x = coord.x;
            }
        }
    }
}
