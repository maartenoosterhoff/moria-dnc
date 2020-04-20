using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dice_m;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Player_magic_m;
using static Moria.Core.Methods.Player_stats_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Ui_io_m;

namespace Moria.Core.Methods
{
    public static class Mage_spells_m
    {
        public static bool canReadSpells()
        {
            var py = State.Instance.py;
            if (py.flags.blind > 0)
            {
                printMessage("You can't see to read your spell book!");
                return false;
            }

            if (playerNoLight())
            {
                printMessage("You have no light to read by.");
                return false;
            }

            if (py.flags.confused > 0)
            {
                printMessage("You are too confused.");
                return false;
            }

            if (State.Instance.classes[py.misc.class_id].class_to_use_mage_spells != Config.spells.SPELL_TYPE_MAGE)
            {
                printMessage("You can't cast spells!");
                return false;
            }

            return true;
        }

        static void castSpell(int spell_id)
        {
            var py = State.Instance.py;

            int dir = 0;

            switch ((MageSpellId)spell_id)
            {
                case MageSpellId.MagicMissile:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellFireBolt(py.pos, dir, diceRoll(new Dice_t(2, 6)), (int)MagicSpellFlags.MagicMissile, State.Instance.spell_names[0]);
                    }
                    break;
                case MageSpellId.DetectMonsters:
                    spellDetectMonsters();
                    break;
                case MageSpellId.PhaseDoor:
                    playerTeleport(10);
                    break;
                case MageSpellId.LightArea:
                    spellLightArea(py.pos);
                    break;
                case MageSpellId.CureLightWounds:
                    spellChangePlayerHitPoints(diceRoll(new Dice_t(4, 4)));
                    break;
                case MageSpellId.FindHiddenTrapsDoors:
                    spellDetectSecretDoorssWithinVicinity();
                    spellDetectTrapsWithinVicinity();
                    break;
                case MageSpellId.StinkingCloud:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellFireBall(py.pos, dir, 12, (int)MagicSpellFlags.PoisonGas, State.Instance.spell_names[6]);
                    }
                    break;
                case MageSpellId.Confusion:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellConfuseMonster(py.pos, dir);
                    }
                    break;
                case MageSpellId.LightningBolt:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellFireBolt(py.pos, dir, diceRoll(new Dice_t(4, 8)), (int)MagicSpellFlags.Lightning, State.Instance.spell_names[8]);
                    }
                    break;
                case MageSpellId.TrapDoorDestruction:
                    spellDestroyAdjacentDoorsTraps();
                    break;
                case MageSpellId.Sleep1:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellSleepMonster(py.pos, dir);
                    }
                    break;
                case MageSpellId.CurePoison:
                    playerCurePoison();
                    break;
                case MageSpellId.TeleportSelf:
                    playerTeleport(((int)py.misc.level * 5));
                    break;
                case MageSpellId.RemoveCurse:
                    for (int id = 22; id < PLAYER_INVENTORY_SIZE; id++)
                    {
                        py.inventory[id].flags = py.inventory[id].flags & ~Config.treasure_flags.TR_CURSED;
                    }
                    break;
                case MageSpellId.FrostBolt:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellFireBolt(py.pos, dir, diceRoll(new Dice_t(6, 8)), (int)MagicSpellFlags.Frost, State.Instance.spell_names[14]);
                    }
                    break;
                case MageSpellId.WallToMud:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellWallToMud(py.pos, dir);
                    }
                    break;
                case MageSpellId.CreateFood:
                    spellCreateFood();
                    break;
                case MageSpellId.RechargeItem1:
                    spellRechargeItem(20);
                    break;
                case MageSpellId.Sleep2:
                    monsterSleep(py.pos);
                    break;
                case MageSpellId.PolymorphOther:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellPolymorphMonster(py.pos, dir);
                    }
                    break;
                case MageSpellId.IdentifyItem:
                    spellIdentifyItem();
                    break;
                case MageSpellId.Sleep3:
                    spellSleepAllMonsters();
                    break;
                case MageSpellId.FireBolt:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellFireBolt(py.pos, dir, diceRoll(new Dice_t(9, 8)), (int)MagicSpellFlags.Fire, State.Instance.spell_names[22]);
                    }
                    break;
                case MageSpellId.SpeedMonster:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellSpeedMonster(py.pos, dir, -1);
                    }
                    break;
                case MageSpellId.FrostBall:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellFireBall(py.pos, dir, 48, (int)MagicSpellFlags.Frost, State.Instance.spell_names[24]);
                    }
                    break;
                case MageSpellId.RechargeItem2:
                    spellRechargeItem(60);
                    break;
                case MageSpellId.TeleportOther:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellTeleportAwayMonsterInDirection(py.pos, dir);
                    }
                    break;
                case MageSpellId.HasteSelf:
                    py.flags.fast += randomNumber(20) + (int)py.misc.level;
                    break;
                case MageSpellId.FireBall:
                    if (getDirectionWithMemory(CNIL, dir))
                    {
                        spellFireBall(py.pos, dir, 72, (int)MagicSpellFlags.Fire, State.Instance.spell_names[28]);
                    }
                    break;
                case MageSpellId.WordOfDestruction:
                    spellDestroyArea(py.pos);
                    break;
                case MageSpellId.Genocide:
                    spellGenocide();
                    break;
                default:
                    // All cases are handled, so this should never be reached!
                    break;
            }
        }

        // Throw a magic spell -RAK-
        public static void getAndCastMagicSpell()
        {
            var game = State.Instance.game;
            var py = State.Instance.py;

            game.player_free_turn = true;

            if (!canReadSpells())
            {
                return;
            }

            int i = 0, j = 0;
            if (!inventoryFindRange((int)TV_MAGIC_BOOK, TV_NEVER, ref i, ref j))
            {
                printMessage("But you are not carrying any spell-books!");
                return;
            }

            int item_val = 0;
            if (!inventoryGetInputForItemId(item_val, "Use which spell-book?", i, j, CNIL, CNIL))
            {
                return;
            }

            int choice = 0, chance = 0;
            int result = castSpellGetId("Cast which spell?", item_val, ref choice, ref chance);
            if (result < 0)
            {
                printMessage("You don't know any spells in that book.");
                return;
            }
            if (result == 0)
            {
                return;
            }

            game.player_free_turn = false;
            
            Spell_t magic_spell = State.Instance.magic_spells[py.misc.class_id - 1][choice];

            if (randomNumber(100) < chance)
            {
                printMessage("You failed to get the spell off!");
            }
            else
            {
                castSpell(choice + 1);

                if ((py.flags.spells_worked & (1L << choice)) == 0)
                {
                    py.misc.exp += (int)(magic_spell.exp_gain_for_learning << 2);
                    py.flags.spells_worked = py.flags.spells_worked | (1u << choice);

                    displayCharacterExperience();
                }
            }

            if (magic_spell.mana_required > py.misc.current_mana)
            {
                printMessage("You faint from the effort!");

                py.flags.paralysis = randomNumber((5 * ((int)magic_spell.mana_required - py.misc.current_mana)));
                py.misc.current_mana = 0;
                py.misc.current_mana_fraction = 0;

                if (randomNumber(3) == 1)
                {
                    printMessage("You have damaged your health!");
                    playerStatRandomDecrease((int)PlayerAttr.CON);
                }
            }
            else
            {
                py.misc.current_mana -= (int)magic_spell.mana_required;
            }

            printCharacterCurrentMana();
        }

        // Returns spell chance of failure for class_to_use_mage_spells -RAK-
        public static int spellChanceOfSuccess(int spell_id)
        {
            var py = State.Instance.py;

            Spell_t spell = State.Instance.magic_spells[py.misc.class_id - 1][spell_id];

            int chance = (int)(spell.failure_chance - 3 * (py.misc.level - spell.level_required));

            int stat;
            if (State.Instance.classes[py.misc.class_id].class_to_use_mage_spells == Config.spells.SPELL_TYPE_MAGE)
            {
                stat = (int)PlayerAttr.INT;
            }
            else
            {
                stat = (int)PlayerAttr.WIS;
            }

            chance -= 3 * (playerStatAdjustmentWisdomIntelligence(stat) - 1);

            if (spell.mana_required > py.misc.current_mana)
            {
                chance += 5 * ((int)spell.mana_required - py.misc.current_mana);
            }

            if (chance > 95)
            {
                chance = 95;
            }
            else if (chance < 5)
            {
                chance = 5;
            }

            return chance;
        }
    }
}
