using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Constants.Std_c;

namespace Moria.Core.Methods
{
    public interface ITreasure
    {
        void magicTreasureMagicalAbility(int item_id, int level);
    }

    public class Treasure_m : ITreasure
    {
        private Treasure_m(
            IDice dice,
            IRnd rnd,
            IStd std
        )
        {
            this.dice = dice;
            this.rnd = rnd;
            this.std = std;
        }

        private readonly IDice dice;
        private readonly IRnd rnd;
        private readonly IStd std;

        // Should the object be enchanted -RAK-
        private bool magicShouldBeEnchanted(int chance)
        {
            return rnd.randomNumber(100) <= chance;
        }

        // Enchant a bonus based on degree desired -RAK-
        private int magicEnchantmentBonus(int @base, int max_standard, int level)
        {
            var stand_deviation = ((int)Config.treasure.LEVEL_STD_OBJECT_ADJUST * level / 100) + (int)Config.treasure.LEVEL_MIN_OBJECT_STD;

            // Check for level > max_standard since that may have generated an overflow.
            if (stand_deviation > max_standard || level > max_standard)
            {
                stand_deviation = max_standard;
            }

            // abs may be a macro, don't call it with rnd.randomNumberNormalDistribution() as a parameter
            var abs_distribution = (int)std.std_abs(std.std_intmax_t(rnd.randomNumberNormalDistribution(0, stand_deviation)));
            var bonus = (abs_distribution / 10) + @base;

            if (bonus < @base)
            {
                return @base;
            }

            return bonus;
        }

        private void magicalArmor(Inventory_t item, int special, int level)
        {
            item.to_ac += magicEnchantmentBonus(1, 30, level);

            if (!magicShouldBeEnchanted(special))
            {
                return;
            }

            switch (rnd.randomNumber(9))
            {
                case 1:
                    item.flags |=
                        (Config.treasure_flags.TR_RES_LIGHT | Config.treasure_flags.TR_RES_COLD | Config.treasure_flags.TR_RES_ACID | Config.treasure_flags.TR_RES_FIRE);
                    item.special_name_id = (int)SpecialNameIds.SN_R;
                    item.to_ac += 5;
                    item.cost += 2500;
                    break;
                case 2: // Resist Acid
                    item.flags |= Config.treasure_flags.TR_RES_ACID;
                    item.special_name_id = (int)SpecialNameIds.SN_RA;
                    item.cost += 1000;
                    break;
                case 3:
                case 4: // Resist Fire
                    item.flags |= Config.treasure_flags.TR_RES_FIRE;
                    item.special_name_id = (int)SpecialNameIds.SN_RF;
                    item.cost += 600;
                    break;
                case 5:
                case 6: // Resist Cold
                    item.flags |= Config.treasure_flags.TR_RES_COLD;
                    item.special_name_id = (int)SpecialNameIds.SN_RC;
                    item.cost += 600;
                    break;
                case 7:
                case 8:
                case 9: // Resist Lightning
                    item.flags |= Config.treasure_flags.TR_RES_LIGHT;
                    item.special_name_id = (int)SpecialNameIds.SN_RL;
                    item.cost += 500;
                    break;
                default:
                    // Do not apply any special magic
                    break;
            }
        }

        private void cursedArmor(Inventory_t item, int level)
        {
            item.to_ac -= magicEnchantmentBonus(1, 40, level);
            item.cost = 0;
            item.flags |= Config.treasure_flags.TR_CURSED;
        }

        private void magicalSword(Inventory_t item, int special, int level)
        {
            item.to_hit += magicEnchantmentBonus(0, 40, level);

            // Magical damage bonus now proportional to weapon base damage
            var damage_bonus = dice.maxDiceRoll(item.damage);

            item.to_damage += magicEnchantmentBonus(0, 4 * damage_bonus, damage_bonus * level / 10);

            // the 3*special/2 is needed because weapons are not as common as
            // before change to treasure distribution, this helps keep same
            // number of ego weapons same as before, see also missiles
            if (magicShouldBeEnchanted(3 * special / 2))
            {
                switch (rnd.randomNumber(16))
                {
                    case 1: // Holy Avenger
                        item.flags |= (Config.treasure_flags.TR_SEE_INVIS | Config.treasure_flags.TR_SUST_STAT | Config.treasure_flags.TR_SLAY_UNDEAD |
                                       Config.treasure_flags.TR_SLAY_EVIL | Config.treasure_flags.TR_STR);
                        item.to_hit += 5;
                        item.to_damage += 5;
                        item.to_ac += rnd.randomNumber(4);

                        // the value in `misc_use` is used for strength increase
                        // `misc_use` is also used for sustain stat
                        item.misc_use = rnd.randomNumber(4);
                        item.special_name_id = (int)SpecialNameIds.SN_HA;
                        item.cost += item.misc_use * 500;
                        item.cost += 10000;
                        break;
                    case 2: // Defender
                        item.flags |= (Config.treasure_flags.TR_FFALL | Config.treasure_flags.TR_RES_LIGHT | Config.treasure_flags.TR_SEE_INVIS |
                                       Config.treasure_flags.TR_FREE_ACT | Config.treasure_flags.TR_RES_COLD | Config.treasure_flags.TR_RES_ACID |
                                       Config.treasure_flags.TR_RES_FIRE | Config.treasure_flags.TR_REGEN | Config.treasure_flags.TR_STEALTH);
                        item.to_hit += 3;
                        item.to_damage += 3;
                        item.to_ac += 5 + rnd.randomNumber(5);
                        item.special_name_id = (int)SpecialNameIds.SN_DF;

                        // the value in `misc_use` is used for stealth
                        item.misc_use = rnd.randomNumber(3);
                        item.cost += item.misc_use * 500;
                        item.cost += 7500;
                        break;
                    case 3:
                    case 4: // Slay Animal
                        item.flags |= Config.treasure_flags.TR_SLAY_ANIMAL;
                        item.to_hit += 2;
                        item.to_damage += 2;
                        item.special_name_id = (int)SpecialNameIds.SN_SA;
                        item.cost += 3000;
                        break;
                    case 5:
                    case 6: // Slay Dragon
                        item.flags |= Config.treasure_flags.TR_SLAY_DRAGON;
                        item.to_hit += 3;
                        item.to_damage += 3;
                        item.special_name_id = (int)SpecialNameIds.SN_SD;
                        item.cost += 4000;
                        break;
                    case 7:
                    case 8: // Slay Evil
                        item.flags |= Config.treasure_flags.TR_SLAY_EVIL;
                        item.to_hit += 3;
                        item.to_damage += 3;
                        item.special_name_id = (int)SpecialNameIds.SN_SE;
                        item.cost += 4000;
                        break;
                    case 9:
                    case 10: // Slay Undead
                        item.flags |= (Config.treasure_flags.TR_SEE_INVIS | Config.treasure_flags.TR_SLAY_UNDEAD);
                        item.to_hit += 3;
                        item.to_damage += 3;
                        item.special_name_id = (int)SpecialNameIds.SN_SU;
                        item.cost += 5000;
                        break;
                    case 11:
                    case 12:
                    case 13: // Flame Tongue
                        item.flags |= Config.treasure_flags.TR_FLAME_TONGUE;
                        item.to_hit++;
                        item.to_damage += 3;
                        item.special_name_id = (int)SpecialNameIds.SN_FT;
                        item.cost += 2000;
                        break;
                    case 14:
                    case 15:
                    case 16: // Frost Brand
                        item.flags |= Config.treasure_flags.TR_FROST_BRAND;
                        item.to_hit++;
                        item.to_damage++;
                        item.special_name_id = (int)SpecialNameIds.SN_FB;
                        item.cost += 1200;
                        break;
                    default:
                        break;
                }
            }
        }

        private void cursedSword(Inventory_t item, int level)
        {
            item.to_hit -= magicEnchantmentBonus(1, 55, level);

            // Magical damage bonus now proportional to weapon base damage
            var damage_bonus = dice.maxDiceRoll(item.damage);

            item.to_damage -= magicEnchantmentBonus(1, 11 * damage_bonus / 2, damage_bonus * level / 10);
            item.flags |= Config.treasure_flags.TR_CURSED;
            item.cost = 0;
        }

        private void magicalBow(Inventory_t item, int level)
        {
            item.to_hit += magicEnchantmentBonus(1, 30, level);

            // add damage. -CJS-
            item.to_damage += magicEnchantmentBonus(1, 20, level);
        }

        private void cursedBow(Inventory_t item, int level)
        {
            item.to_hit -= magicEnchantmentBonus(1, 50, level);

            // add damage. -CJS-
            item.to_damage -= magicEnchantmentBonus(1, 30, level);

            item.flags |= Config.treasure_flags.TR_CURSED;
            item.cost = 0;
        }

        private void magicalDiggingTool(Inventory_t item, int level)
        {
            item.misc_use += magicEnchantmentBonus(0, 25, level);
        }

        private void cursedDiggingTool(Inventory_t item, int level)
        {
            item.misc_use = -magicEnchantmentBonus(1, 30, level);
            item.cost = 0;
            item.flags |= Config.treasure_flags.TR_CURSED;
        }

        private void magicalGloves(Inventory_t item, int special, int level)
        {
            item.to_ac += magicEnchantmentBonus(1, 20, level);

            if (!magicShouldBeEnchanted(special))
            {
                return;
            }

            if (rnd.randomNumber(2) == 1)
            {
                item.flags |= Config.treasure_flags.TR_FREE_ACT;
                item.special_name_id = (int)SpecialNameIds.SN_FREE_ACTION;
                item.cost += 1000;
            }
            else
            {
                item.identification |= Config.identification.ID_SHOW_HIT_DAM;
                item.to_hit += 1 + rnd.randomNumber(3);
                item.to_damage += 1 + rnd.randomNumber(3);
                item.special_name_id = (int)SpecialNameIds.SN_SLAYING;
                item.cost += (item.to_hit + item.to_damage) * 250;
            }
        }

        private void cursedGloves(Inventory_t item, int special, int level)
        {
            if (magicShouldBeEnchanted(special))
            {
                if (rnd.randomNumber(2) == 1)
                {
                    item.flags |= Config.treasure_flags.TR_DEX;
                    item.special_name_id = (int)SpecialNameIds.SN_CLUMSINESS;
                }
                else
                {
                    item.flags |= Config.treasure_flags.TR_STR;
                    item.special_name_id = (int)SpecialNameIds.SN_WEAKNESS;
                }
                item.identification |= Config.identification.ID_SHOW_P1;
                item.misc_use = -magicEnchantmentBonus(1, 10, level);
            }

            item.to_ac -= magicEnchantmentBonus(1, 40, level);
            item.flags |= Config.treasure_flags.TR_CURSED;
            item.cost = 0;
        }

        private void magicalBoots(Inventory_t item, int special, int level)
        {
            item.to_ac += magicEnchantmentBonus(1, 20, level);

            if (!magicShouldBeEnchanted(special))
            {
                return;
            }

            var magic_type = rnd.randomNumber(12);

            if (magic_type > 5)
            {
                item.flags |= Config.treasure_flags.TR_FFALL;
                item.special_name_id = (int)SpecialNameIds.SN_SLOW_DESCENT;
                item.cost += 250;
            }
            else if (magic_type == 1)
            {
                item.flags |= Config.treasure_flags.TR_SPEED;
                item.special_name_id = (int)SpecialNameIds.SN_SPEED;
                item.identification |= Config.identification.ID_SHOW_P1;
                item.misc_use = 1;
                item.cost += 5000;
            }
            else
            {
                // 2 - 5
                item.flags |= Config.treasure_flags.TR_STEALTH;
                item.identification |= Config.identification.ID_SHOW_P1;
                item.misc_use = rnd.randomNumber(3);
                item.special_name_id = (int)SpecialNameIds.SN_STEALTH;
                item.cost += 500;
            }
        }

        private void cursedBoots(Inventory_t item, int level)
        {
            var magic_type = rnd.randomNumber(3);

            switch (magic_type)
            {
                case 1:
                    item.flags |= Config.treasure_flags.TR_SPEED;
                    item.special_name_id = (int)SpecialNameIds.SN_SLOWNESS;
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = -1;
                    break;
                case 2:
                    item.flags |= Config.treasure_flags.TR_AGGRAVATE;
                    item.special_name_id = (int)SpecialNameIds.SN_NOISE;
                    break;
                default:
                    item.special_name_id = (int)SpecialNameIds.SN_GREAT_MASS;
                    item.weight = (uint)(item.weight * 5);
                    break;
            }

            item.cost = 0;
            item.to_ac -= magicEnchantmentBonus(2, 45, level);
            item.flags |= Config.treasure_flags.TR_CURSED;
        }

        private void magicalHelms(Inventory_t item, int special, int level)
        {
            item.to_ac += magicEnchantmentBonus(1, 20, level);

            if (!magicShouldBeEnchanted(special))
            {
                return;
            }

            if (item.sub_category_id < 6)
            {
                item.identification |= Config.identification.ID_SHOW_P1;

                var magic_type = rnd.randomNumber(3);

                switch (magic_type)
                {
                    case 1:
                        item.misc_use = rnd.randomNumber(2);
                        item.flags |= Config.treasure_flags.TR_INT;
                        item.special_name_id = (int)SpecialNameIds.SN_INTELLIGENCE;
                        item.cost += item.misc_use * 500;
                        break;
                    case 2:
                        item.misc_use = rnd.randomNumber(2);
                        item.flags |= Config.treasure_flags.TR_WIS;
                        item.special_name_id = (int)SpecialNameIds.SN_WISDOM;
                        item.cost += item.misc_use * 500;
                        break;
                    default:
                        item.misc_use = (1 + rnd.randomNumber(4));
                        item.flags |= Config.treasure_flags.TR_INFRA;
                        item.special_name_id = (int)SpecialNameIds.SN_INFRAVISION;
                        item.cost += item.misc_use * 250;
                        break;
                }
                return;
            }

            switch (rnd.randomNumber(6))
            {
                case 1:
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = rnd.randomNumber(3);
                    item.flags |= (Config.treasure_flags.TR_FREE_ACT | Config.treasure_flags.TR_CON | Config.treasure_flags.TR_DEX | Config.treasure_flags.TR_STR);
                    item.special_name_id = (int)SpecialNameIds.SN_MIGHT;
                    item.cost += 1000 + item.misc_use * 500;
                    break;
                case 2:
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = rnd.randomNumber(3);
                    item.flags |= (Config.treasure_flags.TR_CHR | Config.treasure_flags.TR_WIS);
                    item.special_name_id = (int)SpecialNameIds.SN_LORDLINESS;
                    item.cost += 1000 + item.misc_use * 500;
                    break;
                case 3:
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = rnd.randomNumber(3);
                    item.flags |= (Config.treasure_flags.TR_RES_LIGHT | Config.treasure_flags.TR_RES_COLD | Config.treasure_flags.TR_RES_ACID |
                                   Config.treasure_flags.TR_RES_FIRE | Config.treasure_flags.TR_INT);
                    item.special_name_id = (int)SpecialNameIds.SN_MAGI;
                    item.cost += 3000 + item.misc_use * 500;
                    break;
                case 4:
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = rnd.randomNumber(3);
                    item.flags |= Config.treasure_flags.TR_CHR;
                    item.special_name_id = (int)SpecialNameIds.SN_BEAUTY;
                    item.cost += 750;
                    break;
                case 5:
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = (5 * (1 + rnd.randomNumber(4)));
                    item.flags |= (Config.treasure_flags.TR_SEE_INVIS | Config.treasure_flags.TR_SEARCH);
                    item.special_name_id = (int)SpecialNameIds.SN_SEEING;
                    item.cost += 1000 + item.misc_use * 100;
                    break;
                case 6:
                    item.flags |= Config.treasure_flags.TR_REGEN;
                    item.special_name_id = (int)SpecialNameIds.SN_REGENERATION;
                    item.cost += 1500;
                    break;
                default:
                    break;
            }
        }

        private void cursedHelms(Inventory_t item, int special, int level)
        {
            item.to_ac -= magicEnchantmentBonus(1, 45, level);
            item.flags |= Config.treasure_flags.TR_CURSED;
            item.cost = 0;

            if (!magicShouldBeEnchanted(special))
            {
                return;
            }

            switch (rnd.randomNumber(7))
            {
                case 1:
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = -rnd.randomNumber(5);
                    item.flags |= Config.treasure_flags.TR_INT;
                    item.special_name_id = (int)SpecialNameIds.SN_STUPIDITY;
                    break;
                case 2:
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = -rnd.randomNumber(5);
                    item.flags |= Config.treasure_flags.TR_WIS;
                    item.special_name_id = (int)SpecialNameIds.SN_DULLNESS;
                    break;
                case 3:
                    item.flags |= Config.treasure_flags.TR_BLIND;
                    item.special_name_id = (int)SpecialNameIds.SN_BLINDNESS;
                    break;
                case 4:
                    item.flags |= Config.treasure_flags.TR_TIMID;
                    item.special_name_id = (int)SpecialNameIds.SN_TIMIDNESS;
                    break;
                case 5:
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = -rnd.randomNumber(5);
                    item.flags |= Config.treasure_flags.TR_STR;
                    item.special_name_id = (int)SpecialNameIds.SN_WEAKNESS;
                    break;
                case 6:
                    item.flags |= Config.treasure_flags.TR_TELEPORT;
                    item.special_name_id = (int)SpecialNameIds.SN_TELEPORTATION;
                    break;
                case 7:
                    item.identification |= Config.identification.ID_SHOW_P1;
                    item.misc_use = -rnd.randomNumber(5);
                    item.flags |= Config.treasure_flags.TR_CHR;
                    item.special_name_id = (int)SpecialNameIds.SN_UGLINESS;
                    break;
                default:
                    return;
            }
        }

        private void processRings(Inventory_t item, int level, int cursed)
        {
            switch (item.sub_category_id)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    if (magicShouldBeEnchanted(cursed))
                    {
                        item.misc_use = -magicEnchantmentBonus(1, 20, level);
                        item.flags |= Config.treasure_flags.TR_CURSED;
                        item.cost = -item.cost;
                    }
                    else
                    {
                        item.misc_use = magicEnchantmentBonus(1, 10, level);
                        item.cost += item.misc_use * 100;
                    }
                    break;
                case 4:
                    if (magicShouldBeEnchanted(cursed))
                    {
                        item.misc_use = -rnd.randomNumber(3);
                        item.flags |= Config.treasure_flags.TR_CURSED;
                        item.cost = -item.cost;
                    }
                    else
                    {
                        item.misc_use = 1;
                    }
                    break;
                case 5:
                    item.misc_use = (5 * magicEnchantmentBonus(1, 20, level));
                    item.cost += item.misc_use * 50;
                    if (magicShouldBeEnchanted(cursed))
                    {
                        item.misc_use = -item.misc_use;
                        item.flags |= Config.treasure_flags.TR_CURSED;
                        item.cost = -item.cost;
                    }
                    break;
                case 19: // Increase damage
                    item.to_damage += magicEnchantmentBonus(1, 20, level);
                    item.cost += item.to_damage * 100;
                    if (magicShouldBeEnchanted(cursed))
                    {
                        item.to_damage = -item.to_damage;
                        item.flags |= Config.treasure_flags.TR_CURSED;
                        item.cost = -item.cost;
                    }
                    break;
                case 20: // Increase To-Hit
                    item.to_hit += magicEnchantmentBonus(1, 20, level);
                    item.cost += item.to_hit * 100;
                    if (magicShouldBeEnchanted(cursed))
                    {
                        item.to_hit = -item.to_hit;
                        item.flags |= Config.treasure_flags.TR_CURSED;
                        item.cost = -item.cost;
                    }
                    break;
                case 21: // Protection
                    item.to_ac += magicEnchantmentBonus(1, 20, level);
                    item.cost += item.to_ac * 100;
                    if (magicShouldBeEnchanted(cursed))
                    {
                        item.to_ac = -item.to_ac;
                        item.flags |= Config.treasure_flags.TR_CURSED;
                        item.cost = -item.cost;
                    }
                    break;
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                    item.identification |= Config.identification.ID_NO_SHOW_P1;
                    break;
                case 30: // Slaying
                    item.identification |= Config.identification.ID_SHOW_HIT_DAM;
                    item.to_damage += magicEnchantmentBonus(1, 25, level);
                    item.to_hit += magicEnchantmentBonus(1, 25, level);
                    item.cost += (item.to_hit + item.to_damage) * 100;
                    if (magicShouldBeEnchanted(cursed))
                    {
                        item.to_hit = -item.to_hit;
                        item.to_damage = -item.to_damage;
                        item.flags |= Config.treasure_flags.TR_CURSED;
                        item.cost = -item.cost;
                    }
                    break;
                default:
                    break;
            }
        }

        private void processAmulets(Inventory_t item, int level, int cursed)
        {
            if (item.sub_category_id < 2)
            {
                if (magicShouldBeEnchanted(cursed))
                {
                    item.misc_use = -magicEnchantmentBonus(1, 20, level);
                    item.flags |= Config.treasure_flags.TR_CURSED;
                    item.cost = -item.cost;
                }
                else
                {
                    item.misc_use = magicEnchantmentBonus(1, 10, level);
                    item.cost += item.misc_use * 100;
                }
            }
            else if (item.sub_category_id == 2)
            {
                item.misc_use = (5 * magicEnchantmentBonus(1, 25, level));
                if (magicShouldBeEnchanted(cursed))
                {
                    item.misc_use = -item.misc_use;
                    item.cost = -item.cost;
                    item.flags |= Config.treasure_flags.TR_CURSED;
                }
                else
                {
                    item.cost += 50 * item.misc_use;
                }
            }
            else if (item.sub_category_id == 8)
            {
                // amulet of the magi is never cursed
                item.misc_use = (5 * magicEnchantmentBonus(1, 25, level));
                item.cost += 20 * item.misc_use;
            }
        }

        private int wandMagic(uint id)
        {
            switch (id)
            {
                case 0:
                    return rnd.randomNumber(10) + 6;
                case 1:
                    return rnd.randomNumber(8) + 6;
                case 2:
                    return rnd.randomNumber(5) + 6;
                case 3:
                    return rnd.randomNumber(8) + 6;
                case 4:
                    return rnd.randomNumber(4) + 3;
                case 5:
                    return rnd.randomNumber(8) + 6;
                case 6:
                case 7:
                    return rnd.randomNumber(20) + 12;
                case 8:
                    return rnd.randomNumber(10) + 6;
                case 9:
                    return rnd.randomNumber(12) + 6;
                case 10:
                    return rnd.randomNumber(10) + 12;
                case 11:
                    return rnd.randomNumber(3) + 3;
                case 12:
                    return rnd.randomNumber(8) + 6;
                case 13:
                    return rnd.randomNumber(10) + 6;
                case 14:
                case 15:
                    return rnd.randomNumber(5) + 3;
                case 16:
                    return rnd.randomNumber(5) + 6;
                case 17:
                    return rnd.randomNumber(5) + 4;
                case 18:
                    return rnd.randomNumber(8) + 4;
                case 19:
                    return rnd.randomNumber(6) + 2;
                case 20:
                    return rnd.randomNumber(4) + 2;
                case 21:
                    return rnd.randomNumber(8) + 6;
                case 22:
                    return rnd.randomNumber(5) + 2;
                case 23:
                    return rnd.randomNumber(12) + 12;
                default:
                    return -1;
            }
        }

        private int staffMagic(uint id)
        {
            switch (id)
            {
                case 0:
                    return rnd.randomNumber(20) + 12;
                case 1:
                    return rnd.randomNumber(8) + 6;
                case 2:
                    return rnd.randomNumber(5) + 6;
                case 3:
                    return rnd.randomNumber(20) + 12;
                case 4:
                    return rnd.randomNumber(15) + 6;
                case 5:
                    return rnd.randomNumber(4) + 5;
                case 6:
                    return rnd.randomNumber(5) + 3;
                case 7:
                case 8:
                    return rnd.randomNumber(3) + 1;
                case 9:
                    return rnd.randomNumber(5) + 6;
                case 10:
                    return rnd.randomNumber(10) + 12;
                case 11:
                case 12:
                case 13:
                    return rnd.randomNumber(5) + 6;
                case 14:
                    return rnd.randomNumber(10) + 12;
                case 15:
                    return rnd.randomNumber(3) + 4;
                case 16:
                case 17:
                    return rnd.randomNumber(5) + 6;
                case 18:
                    return rnd.randomNumber(3) + 4;
                case 19:
                    return rnd.randomNumber(10) + 12;
                case 20:
                case 21:
                    return rnd.randomNumber(3) + 4;
                case 22:
                    return rnd.randomNumber(10) + 6;
                default:
                    return -1;
            }
        }

        private void magicalCloak(Inventory_t item, int special, int level)
        {
            if (!magicShouldBeEnchanted(special))
            {
                item.to_ac += magicEnchantmentBonus(1, 20, level);
                return;
            }

            if (rnd.randomNumber(2) == 1)
            {
                item.special_name_id = (int)SpecialNameIds.SN_PROTECTION;
                item.to_ac += magicEnchantmentBonus(2, 40, level);
                item.cost += 250;
                return;
            }

            item.to_ac += magicEnchantmentBonus(1, 20, level);
            item.identification |= Config.identification.ID_SHOW_P1;
            item.misc_use = rnd.randomNumber(3);
            item.flags |= Config.treasure_flags.TR_STEALTH;
            item.special_name_id = (int)SpecialNameIds.SN_STEALTH;
            item.cost += 500;
        }

        private void cursedCloak(Inventory_t item, int level)
        {
            var magic_type = rnd.randomNumber(3);

            switch (magic_type)
            {
                case 1:
                    item.flags |= Config.treasure_flags.TR_AGGRAVATE;
                    item.special_name_id = (int)SpecialNameIds.SN_IRRITATION;
                    item.to_ac -= magicEnchantmentBonus(1, 10, level);
                    item.identification |= Config.identification.ID_SHOW_HIT_DAM;
                    item.to_hit -= magicEnchantmentBonus(1, 10, level);
                    item.to_damage -= magicEnchantmentBonus(1, 10, level);
                    item.cost = 0;
                    break;
                case 2:
                    item.special_name_id = (int)SpecialNameIds.SN_VULNERABILITY;
                    item.to_ac -= magicEnchantmentBonus(10, 100, level + 50);
                    item.cost = 0;
                    break;
                default:
                    item.special_name_id = (int)SpecialNameIds.SN_ENVELOPING;
                    item.to_ac -= magicEnchantmentBonus(1, 10, level);
                    item.identification |= Config.identification.ID_SHOW_HIT_DAM;
                    item.to_hit -= magicEnchantmentBonus(2, 40, level + 10);
                    item.to_damage -= magicEnchantmentBonus(2, 40, level + 10);
                    item.cost = 0;
                    break;
            }

            item.flags |= Config.treasure_flags.TR_CURSED;
        }

        private void magicalChests(Inventory_t item, int level)
        {
            var magic_type = rnd.randomNumber(level + 4);

            switch (magic_type)
            {
                case 1:
                    item.flags = 0;
                    item.special_name_id = (int)SpecialNameIds.SN_EMPTY;
                    break;
                case 2:
                    item.flags |= Config.treasure_chests.CH_LOCKED;
                    item.special_name_id = (int)SpecialNameIds.SN_LOCKED;
                    break;
                case 3:
                case 4:
                    item.flags |= (Config.treasure_chests.CH_LOSE_STR | Config.treasure_chests.CH_LOCKED);
                    item.special_name_id = (int)SpecialNameIds.SN_POISON_NEEDLE;
                    break;
                case 5:
                case 6:
                    item.flags |= (Config.treasure_chests.CH_POISON | Config.treasure_chests.CH_LOCKED);
                    item.special_name_id = (int)SpecialNameIds.SN_POISON_NEEDLE;
                    break;
                case 7:
                case 8:
                case 9:
                    item.flags |= (Config.treasure_chests.CH_PARALYSED | Config.treasure_chests.CH_LOCKED);
                    item.special_name_id = (int)SpecialNameIds.SN_GAS_TRAP;
                    break;
                case 10:
                case 11:
                    item.flags |= (Config.treasure_chests.CH_EXPLODE | Config.treasure_chests.CH_LOCKED);
                    item.special_name_id = (int)SpecialNameIds.SN_EXPLOSION_DEVICE;
                    break;
                case 12:
                case 13:
                case 14:
                    item.flags |= (Config.treasure_chests.CH_SUMMON | Config.treasure_chests.CH_LOCKED);
                    item.special_name_id = (int)SpecialNameIds.SN_SUMMONING_RUNES;
                    break;
                case 15:
                case 16:
                case 17:
                    item.flags |=
                        (Config.treasure_chests.CH_PARALYSED | Config.treasure_chests.CH_POISON | Config.treasure_chests.CH_LOSE_STR | Config.treasure_chests.CH_LOCKED);
                    item.special_name_id = (int)SpecialNameIds.SN_MULTIPLE_TRAPS;
                    break;
                default:
                    item.flags |= (Config.treasure_chests.CH_SUMMON | Config.treasure_chests.CH_EXPLODE | Config.treasure_chests.CH_LOCKED);
                    item.special_name_id = (int)SpecialNameIds.SN_MULTIPLE_TRAPS;
                    break;
            }
        }

        private void magicalProjectileAdjustment(Inventory_t item, int special, int level)
        {
            item.to_hit += magicEnchantmentBonus(1, 35, level);
            item.to_damage += magicEnchantmentBonus(1, 35, level);

            // see comment for weapons
            if (magicShouldBeEnchanted(3 * special / 2))
            {
                switch (rnd.randomNumber(10))
                {
                    case 1:
                    case 2:
                    case 3:
                        item.special_name_id = (int)SpecialNameIds.SN_SLAYING;
                        item.to_hit += 5;
                        item.to_damage += 5;
                        item.cost += 20;
                        break;
                    case 4:
                    case 5:
                        item.flags |= Config.treasure_flags.TR_FLAME_TONGUE;
                        item.to_hit += 2;
                        item.to_damage += 4;
                        item.special_name_id = (int)SpecialNameIds.SN_FIRE;
                        item.cost += 25;
                        break;
                    case 6:
                    case 7:
                        item.flags |= Config.treasure_flags.TR_SLAY_EVIL;
                        item.to_hit += 3;
                        item.to_damage += 3;
                        item.special_name_id = (int)SpecialNameIds.SN_SLAY_EVIL;
                        item.cost += 25;
                        break;
                    case 8:
                    case 9:
                        item.flags |= Config.treasure_flags.TR_SLAY_ANIMAL;
                        item.to_hit += 2;
                        item.to_damage += 2;
                        item.special_name_id = (int)SpecialNameIds.SN_SLAY_ANIMAL;
                        item.cost += 30;
                        break;
                    case 10:
                        item.flags |= Config.treasure_flags.TR_SLAY_DRAGON;
                        item.to_hit += 3;
                        item.to_damage += 3;
                        item.special_name_id = (int)SpecialNameIds.SN_DRAGON_SLAYING;
                        item.cost += 35;
                        break;
                    default:
                        break;
                }
            }
        }

        private void cursedProjectileAdjustment(Inventory_t item, int level)
        {
            item.to_hit -= magicEnchantmentBonus(5, 55, level);
            item.to_damage -= magicEnchantmentBonus(5, 55, level);
            item.flags |= Config.treasure_flags.TR_CURSED;
            item.cost = 0;
        }

        private void magicalProjectile(Inventory_t item, int special, int level, int chance, int cursed)
        {
            if (item.category_id == TV_SLING_AMMO || item.category_id == TV_BOLT || item.category_id == TV_ARROW)
            {
                // always show to_hit/to_damage values if identified
                item.identification |= Config.identification.ID_SHOW_HIT_DAM;

                if (magicShouldBeEnchanted(chance))
                {
                    magicalProjectileAdjustment(item, special, level);
                }
                else if (magicShouldBeEnchanted(cursed))
                {
                    cursedProjectileAdjustment(item, level);
                }
            }

            item.items_count = 0;

            for (var i = 0; i < 7; i++)
            {
                item.items_count += (uint) rnd.randomNumber(6);
            }

            if (State.Instance.missiles_counter == SHRT_MAX)
            {
                State.Instance.missiles_counter = -SHRT_MAX - 1;
            }
            else
            {
                State.Instance.missiles_counter++;
            }

            item.misc_use = State.Instance.missiles_counter;
        }

        // Chance of treasure having magic abilities -RAK-
        // Chance increases with each dungeon level
        public void magicTreasureMagicalAbility(int item_id, int level)
        {
            var game = State.Instance.game;

            var chance = (int)Config.treasure.OBJECT_BASE_MAGIC + level;
            if (chance > Config.treasure.OBJECT_MAX_BASE_MAGIC)
            {
                chance = (int)Config.treasure.OBJECT_MAX_BASE_MAGIC;
            }

            var special = chance / (int)Config.treasure.OBJECT_CHANCE_SPECIAL;
            var cursed = (10 * chance) / (int)Config.treasure.OBJECT_CHANCE_CURSED;

            int magic_amount;

            var item = game.treasure.list[item_id];

            // some objects appear multiple times in the game_objects with different
            // levels, this is to make the object occur more often, however, for
            // consistency, must set the level of these duplicates to be the same
            // as the object with the lowest level

            // Depending on treasure type, it can have certain magical properties
            switch (item.category_id)
            {
                case TV_SHIELD:
                case TV_HARD_ARMOR:
                case TV_SOFT_ARMOR:
                    if (magicShouldBeEnchanted(chance))
                    {
                        magicalArmor(item, special, level);
                    }
                    else if (magicShouldBeEnchanted(cursed))
                    {
                        cursedArmor(item, level);
                    }
                    break;
                case TV_HAFTED:
                case TV_POLEARM:
                case TV_SWORD:
                    // always show to_hit/to_damage values if identified
                    item.identification |= Config.identification.ID_SHOW_HIT_DAM;

                    if (magicShouldBeEnchanted(chance))
                    {
                        magicalSword(item, special, level);
                    }
                    else if (magicShouldBeEnchanted(cursed))
                    {
                        cursedSword(item, level);
                    }
                    break;
                case TV_BOW:
                    // always show to_hit/to_damage values if identified
                    item.identification |= Config.identification.ID_SHOW_HIT_DAM;

                    if (magicShouldBeEnchanted(chance))
                    {
                        magicalBow(item, level);
                    }
                    else if (magicShouldBeEnchanted(cursed))
                    {
                        cursedBow(item, level);
                    }
                    break;
                case TV_DIGGING:
                    // always show to_hit/to_damage values if identified
                    item.identification |= Config.identification.ID_SHOW_HIT_DAM;

                    if (magicShouldBeEnchanted(chance))
                    {
                        if (rnd.randomNumber(3) < 3)
                        {
                            magicalDiggingTool(item, level);
                        }
                        else
                        {
                            cursedDiggingTool(item, level);
                        }
                    }
                    break;
                case TV_GLOVES:
                    if (magicShouldBeEnchanted(chance))
                    {
                        magicalGloves(item, special, level);
                    }
                    else if (magicShouldBeEnchanted(cursed))
                    {
                        cursedGloves(item, special, level);
                    }
                    break;
                case TV_BOOTS:
                    if (magicShouldBeEnchanted(chance))
                    {
                        magicalBoots(item, special, level);
                    }
                    else if (magicShouldBeEnchanted(cursed))
                    {
                        cursedBoots(item, level);
                    }
                    break;
                case TV_HELM:
                    // give crowns a higher chance for magic
                    if (item.sub_category_id >= 6 && item.sub_category_id <= 8)
                    {
                        chance += (int)(item.cost / 100);
                        special += special;
                    }

                    if (magicShouldBeEnchanted(chance))
                    {
                        magicalHelms(item, special, level);
                    }
                    else if (magicShouldBeEnchanted(cursed))
                    {
                        cursedHelms(item, special, level);
                    }
                    break;
                case TV_RING:
                    processRings(item, level, cursed);
                    break;
                case TV_AMULET:
                    processAmulets(item, level, cursed);
                    break;
                case TV_LIGHT:
                    // `sub_category_id` should be even for store, odd for dungeon
                    // Dungeon found ones will be partially charged
                    if ((item.sub_category_id % 2) == 1)
                    {
                        item.misc_use = rnd.randomNumber(item.misc_use);
                        item.sub_category_id -= 1;
                    }
                    break;
                case TV_WAND:
                    magic_amount = wandMagic(item.sub_category_id);
                    if (magic_amount != -1)
                    {
                        item.misc_use = magic_amount;
                    }
                    break;
                case TV_STAFF:
                    magic_amount = staffMagic(item.sub_category_id);
                    if (magic_amount != -1)
                    {
                        item.misc_use = magic_amount;
                    }

                    // Change the level the items was first found on value
                    if (item.sub_category_id == 7)
                    {
                        item.depth_first_found = 10;
                    }
                    else if (item.sub_category_id == 22)
                    {
                        item.depth_first_found = 5;
                    }
                    break;
                case TV_CLOAK:
                    if (magicShouldBeEnchanted(chance))
                    {
                        magicalCloak(item, special, level);
                    }
                    else if (magicShouldBeEnchanted(cursed))
                    {
                        cursedCloak(item, level);
                    }
                    break;
                case TV_CHEST:
                    magicalChests(item, level);
                    break;
                case TV_SLING_AMMO:
                case TV_SPIKE:
                case TV_BOLT:
                case TV_ARROW:
                    magicalProjectile(item, special, level, chance, cursed);
                    break;
                case TV_FOOD:
                    // make sure all food rations have the same level
                    if (item.sub_category_id == 90)
                    {
                        item.depth_first_found = 0;
                    }

                    // give all Elvish waybread the same level
                    if (item.sub_category_id == 92)
                    {
                        item.depth_first_found = 6;
                    }
                    break;
                case TV_SCROLL1:
                    if (item.sub_category_id == 67)
                    {
                        // give all identify scrolls the same level
                        item.depth_first_found = 1;
                    }
                    else if (item.sub_category_id == 69)
                    {
                        // scroll of light
                        item.depth_first_found = 0;
                    }
                    else if (item.sub_category_id == 80)
                    {
                        // scroll of trap detection
                        item.depth_first_found = 5;
                    }
                    else if (item.sub_category_id == 81)
                    {
                        // scroll of door/stair location
                        item.depth_first_found = 5;
                    }
                    break;
                case TV_POTION1:
                    // cure light
                    if (item.sub_category_id == 76)
                    {
                        item.depth_first_found = 0;
                    }
                    break;
                default:
                    break;
            }
        }

    }
}