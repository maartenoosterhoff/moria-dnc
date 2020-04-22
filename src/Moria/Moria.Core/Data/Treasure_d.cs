using System.Collections.Generic;
using Moria.Core.Constants;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using Moria.Core.Utils;

namespace Moria.Core.Data
{
    public static class Treasure_d
    {
        static Treasure_d()
        {
            special_item_names = CreateSpecialItemNames().AsReadOnly();
        }

        public static DungeonObject_t[] game_objects =
            ArrayInitializer.Initialize<DungeonObject_t>(Game_c.MAX_OBJECTS_IN_GAME);

        public static IReadOnlyList<string> special_item_names = new string[(int)SpecialNameIds.SN_ARRAY_SIZE];

        private static List<string> CreateSpecialItemNames()
        {
            var list = new List<string>
            {
                null, "(R)", "(RA)",
                "(RF)", "(RC)", "(RL)",
                "(HA)", "(DF)", "(SA)",
                "(SD)", "(SE)", "(SU)",
                "(FT)", "(FB)", "of Free Action",
                "of Slaying", "of Clumsiness", "of Weakness",
                "of Slow Descent", "of Speed", "of Stealth",
                "of Slowness", "of Noise", "of Great Mass",
                "of Intelligence", "of Wisdom", "of Infra-Vision",
                "of Might", "of Lordliness", "of the Magi",
                "of Beauty", "of Seeing", "of Regeneration",
                "of Stupidity", "of Dullness", "of Blindness",
                "of Timidness", "of Teleportation", "of Ugliness",
                "of Protection", "of Irritation", "of Vulnerability",
                "of Enveloping", "of Fire", "of Slay Evil",
                "of Dragon Slaying", "(Empty)", "(Locked)",
                "(Poison Needle)", "(Gas Trap)", "(Explosion Device)",
                "(Summoning Runes)", "(Multiple Traps)", "(Disarmed)",
                "(Unlocked)", "of Slay Animal",
            };

            return list;
        }
    }
}
