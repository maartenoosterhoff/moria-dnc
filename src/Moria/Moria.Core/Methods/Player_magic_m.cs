using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using static Moria.Core.Constants.Treasure_c;

namespace Moria.Core.Methods
{
    public static class Player_magic_m
    {
        public static void SetDependencies(
            IRnd rnd
        )
        {
            Player_magic_m.rnd = rnd;
        }

        private static IRnd rnd;

        // Cure players confusion -RAK-
        public static bool playerCureConfusion()
        {
            var py = State.Instance.py;
            if (py.flags.confused > 1)
            {
                py.flags.confused = 1;
                return true;
            }
            return false;
        }

        // Cure players blindness -RAK-
        public static bool playerCureBlindness()
        {
            var py = State.Instance.py;
            if (py.flags.blind > 1)
            {
                py.flags.blind = 1;
                return true;
            }
            return false;
        }

        // Cure poisoning -RAK-
        public static bool playerCurePoison()
        {
            var py = State.Instance.py;
            if (py.flags.poisoned > 1)
            {
                py.flags.poisoned = 1;
                return true;
            }
            return false;
        }

        // Cure the players fear -RAK-
        public static bool playerRemoveFear()
        {
            var py = State.Instance.py;
            if (py.flags.afraid > 1)
            {
                py.flags.afraid = 1;
                return true;
            }
            return false;
        }

        // Evil creatures don't like this. -RAK-
        public static bool playerProtectEvil()
        {
            var py = State.Instance.py;
            var is_protected = py.flags.protect_evil == 0;

            py.flags.protect_evil += rnd.randomNumber(25) + 3 * (int)py.misc.level;

            return is_protected;
        }

        // Bless -RAK-
        public static void playerBless(int adjustment)
        {
            var py = State.Instance.py;
            py.flags.blessed += adjustment;
        }

        // Detect Invisible for period of time -RAK-
        public static void playerDetectInvisible(int adjustment)
        {
            var py = State.Instance.py;
            py.flags.detect_invisible += adjustment;
        }

        // Special damage due to magical abilities of object -RAK-
        public static int itemMagicAbilityDamage(Inventory_t item, int total_damage, int monster_id)
        {
            var is_ego_weapon = (item.flags & Config.treasure_flags.TR_EGO_WEAPON) != 0;
            var is_projectile = item.category_id >= TV_SLING_AMMO && item.category_id <= TV_ARROW;
            var is_hafted_sword = item.category_id >= TV_HAFTED && item.category_id <= TV_SWORD;
            var is_flask = item.category_id == TV_FLASK;

            if (is_ego_weapon && (is_projectile || is_hafted_sword || is_flask))
            {
                var creature = Library.Instance.Creatures.creatures_list[monster_id];
                var memory = State.Instance.creature_recall[monster_id];

                // Slay Dragon
                if (((creature.defenses & Config.monsters_defense.CD_DRAGON) != 0) && ((item.flags & Config.treasure_flags.TR_SLAY_DRAGON) != 0u))
                {
                    memory.defenses |= Config.monsters_defense.CD_DRAGON;
                    return total_damage * 4;
                }

                // Slay Undead
                if (((creature.defenses & Config.monsters_defense.CD_UNDEAD) != 0) && ((item.flags & Config.treasure_flags.TR_SLAY_UNDEAD) != 0u))
                {
                    memory.defenses |= Config.monsters_defense.CD_UNDEAD;
                    return total_damage * 3;
                }

                // Slay Animal
                if (((creature.defenses & Config.monsters_defense.CD_ANIMAL) != 0) && ((item.flags & Config.treasure_flags.TR_SLAY_ANIMAL) != 0u))
                {
                    memory.defenses |= Config.monsters_defense.CD_ANIMAL;
                    return total_damage * 2;
                }

                // Slay Evil
                if (((creature.defenses & Config.monsters_defense.CD_EVIL) != 0) && ((item.flags & Config.treasure_flags.TR_SLAY_EVIL) != 0u))
                {
                    memory.defenses |= Config.monsters_defense.CD_EVIL;
                    return total_damage * 2;
                }

                // Frost
                if (((creature.defenses & Config.monsters_defense.CD_FROST) != 0) && ((item.flags & Config.treasure_flags.TR_FROST_BRAND) != 0u))
                {
                    memory.defenses |= Config.monsters_defense.CD_FROST;
                    return total_damage * 3 / 2;
                }

                // Fire
                if (((creature.defenses & Config.monsters_defense.CD_FIRE) != 0) && ((item.flags & Config.treasure_flags.TR_FLAME_TONGUE) != 0u))
                {
                    memory.defenses |= Config.monsters_defense.CD_FIRE;
                    return total_damage * 3 / 2;
                }
            }

            return total_damage;
        }

    }
}
