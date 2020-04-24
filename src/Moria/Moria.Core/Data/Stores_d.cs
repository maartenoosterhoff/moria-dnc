using System.Collections.Generic;

namespace Moria.Core.Data
{
    public class Stores_d
    {
        public Stores_d()
        {
            this.race_gold_adjustments = CreateRaceGoldAdjustments().AsReadOnly();
            this.store_choices = CreateStoreChoices().AsReadOnly();
        }

        public IReadOnlyList<IReadOnlyList<uint>> race_gold_adjustments { get; }
            //ArrayInitializer.InitializeWithDefault<uint>(Player_c.PLAYER_MAX_RACES, Player_c.PLAYER_MAX_RACES);

        public IReadOnlyList<IReadOnlyList<uint>> store_choices { get; }
            //ArrayInitializer.InitializeWithDefault<uint>(Store_c.MAX_STORES, Store_c.STORE_MAX_ITEM_TYPES);

        private static List<IReadOnlyList<uint>> CreateRaceGoldAdjustments()
        {
            return new List<IReadOnlyList<uint>>
            {
                //               Hum, HfE, Elf, Hal, Gno, Dwa, HfO, HfT
                new List<uint> { 100, 105, 105, 110, 113, 115, 120, 125 }.AsReadOnly(), // Human
                new List<uint> { 110, 100, 100, 105, 110, 120, 125, 130 }.AsReadOnly(), // Half-Elf
                new List<uint> { 110, 105, 100, 105, 110, 120, 125, 130 }.AsReadOnly(), // Elf
                new List<uint> { 115, 110, 105,  95, 105, 110, 115, 130 }.AsReadOnly(), // Halfling
                new List<uint> { 115, 115, 110, 105,  95, 110, 115, 130 }.AsReadOnly(), // Gnome
                new List<uint> { 115, 120, 120, 110, 110,  95, 125, 135 }.AsReadOnly(), // Dwarf
                new List<uint> { 115, 120, 125, 115, 115, 130, 110, 115 }.AsReadOnly(), // Half-Orc
                new List<uint> { 110, 115, 115, 110, 110, 130, 110, 110 }.AsReadOnly(), // Half-Troll
            };
        }

        private static List<IReadOnlyList<uint>> CreateStoreChoices()
        {
            return new List<IReadOnlyList<uint>>
            {
                // General Store
                new List<uint> {
                    366, 365, 364,  84,  84, 365, 123, 366, 365, 350, 349, 348, 347,
                    346, 346, 345, 345, 345, 344, 344, 344, 344, 344, 344, 344, 344,
                }.AsReadOnly(),
                // Armory
                new List<uint> {
                    94,  95,  96, 109, 103, 104, 105, 106, 110, 111, 112, 114, 116,
                    124, 125, 126, 127, 129, 103, 104, 124, 125, 91,  92,  95,  96,
                }.AsReadOnly(),
                // Weaponsmith
                new List<uint> {
                    29, 30, 34, 37, 45, 49, 57, 58, 59, 65, 67, 68, 73,
                    74, 75, 77, 79, 80, 81, 83, 29, 30, 80, 83, 80, 83,
                }.AsReadOnly(),
                // Temple
                new List<uint> {
                    322, 323, 324, 325, 180, 180, 233, 237, 240, 241, 361, 362, 57,
                    58,  59, 260, 358, 359, 265, 237, 237, 240, 240, 241, 323, 359,
                }.AsReadOnly(),
                // Alchemy shop
                new List<uint> {
                    173, 174, 175, 351, 351, 352, 353, 354, 355, 356, 357, 206, 227,
                    230, 236, 252, 253, 352, 353, 354, 355, 356, 359, 363, 359, 359,
                }.AsReadOnly(),
                // Magic-User store
                new List<uint> {
                    318, 141, 142, 153, 164, 167, 168, 140, 319, 320, 320, 321, 269,
                    270, 282, 286, 287, 292, 293, 294, 295, 308, 269, 290, 319, 282,
                }.AsReadOnly(),
            };
        }

    }
}
