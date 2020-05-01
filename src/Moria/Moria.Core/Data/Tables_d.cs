using System.Collections.Generic;
using System.Linq;
using Moria.Core.Methods;
using static Moria.Core.Constants.Identification_c;
using static Moria.Core.Methods.Game_m;

namespace Moria.Core.Data
{
    public class Tables_d
    {
        public Tables_d()
        {
            this.colors = CreateColors().AsReadOnly();
            this.mushrooms = CreateMushrooms().AsReadOnly();
            this.woods = CreateWoods().AsReadOnly();
            this.metals = CreateMetals().AsReadOnly();
            this.rocks = CreateRocks().AsReadOnly();
            this.amulets = CreateAmulets().AsReadOnly();
            this.syllables = CreateSyllables().AsReadOnly();
            this.blows_table = CreateBlowsTable().AsReadOnly();
            this.normal_table = CreateNormalTable().AsReadOnly();
        }

        public void initializeItemNames(IRnd rnd)
        {
            int id;

            // The first 3 entries for colors are fixed, (slime & apple juice, water)
            var colorsArray = Library.Instance.Tables.colors.ToArray();
            for (var i = 3; i < MAX_COLORS; i++)
            {
                id = rnd.randomNumber(MAX_COLORS - 3) + 2;
                var color = colorsArray[i];
                colorsArray[i] = colorsArray[id];
                colorsArray[id] = color;
            }

            this.colors = colorsArray.ToList().AsReadOnly();

            var woodsArray = Library.Instance.Tables.woods.ToArray();
            for (var i = 0; i < woodsArray.Length; i++)
            {
                id = rnd.randomNumber(MAX_WOODS) - 1;
                var wood = woodsArray[i];
                woodsArray[i] = woodsArray[id];
                woodsArray[id] = wood;
            }

            this.woods = woodsArray.ToList().AsReadOnly();

            var metalsArray = Library.Instance.Tables.metals.ToArray();
            for (var i = 0; i < metalsArray.Length; i++)
            {
                id = rnd.randomNumber(MAX_METALS) - 1;
                var metal = metalsArray[i];
                metalsArray[i] = metalsArray[id];
                metalsArray[id] = metal;
            }

            this.metals = metalsArray.ToList().AsReadOnly();

            var rocksArray = Library.Instance.Tables.rocks.ToArray();
            for (var i = 0; i < rocksArray.Length; i++)
            {
                id = rnd.randomNumber(MAX_ROCKS) - 1;
                var rock = rocksArray[i];
                rocksArray[i] = rocksArray[id];
                rocksArray[id] = rock;
            }

            this.rocks = rocksArray.ToList().AsReadOnly();

            var amuletsToArray = Library.Instance.Tables.amulets.ToArray();
            for (var i = 0; i < amuletsToArray.Length; i++)
            {
                id = rnd.randomNumber(MAX_AMULETS) - 1;
                var amulet = amuletsToArray[i];
                amuletsToArray[i] = amuletsToArray[id];
                amuletsToArray[id] = amulet;
            }

            this.amulets = amuletsToArray.ToList().AsReadOnly();

            var mushroomsArray = Library.Instance.Tables.mushrooms.ToArray();
            for (var i = 0; i < mushroomsArray.Length; i++)
            {
                id = rnd.randomNumber(MAX_MUSHROOMS) - 1;
                var mushroom = mushroomsArray[i];
                mushroomsArray[i] = mushroomsArray[id];
                mushroomsArray[id] = mushroom;
            }

            this.mushrooms = mushroomsArray.ToList().AsReadOnly();

            var magicItemTitlesArray = new string[MAX_TITLES];
            for (var mit = 0; mit < magicItemTitlesArray.Length; mit++)
            {
                var title = string.Empty;
                var k = rnd.randomNumber(2) + 1;


                for (var i = 0; i < k; i++)
                {
                    for (var s = rnd.randomNumber(2); s > 0; s--)
                    {
                        title += Library.Instance.Tables.syllables[rnd.randomNumber(MAX_SYLLABLES) - 1];
                    }
                    if (i < k - 1)
                    {
                        title += " ";
                    }
                }

                magicItemTitlesArray[mit] = title;
            }

            this.magic_item_titles = magicItemTitlesArray.ToList().AsReadOnly();
        }

        public IReadOnlyList<string> colors { get; private set; }
        public IReadOnlyList<string> mushrooms { get; private set; }
        public IReadOnlyList<string> woods { get; private set; }
        public IReadOnlyList<string> metals { get; private set; }
        public IReadOnlyList<string> rocks { get; private set; }

        public IReadOnlyList<string> amulets { get; private set; }
        public IReadOnlyList<string> syllables { get; }
        public IReadOnlyList<IReadOnlyList<uint>> blows_table { get; }
        public IReadOnlyList<uint> normal_table { get; }

        public IReadOnlyList<string> magic_item_titles { get; private set; }


        private static List<string> CreateColors()
        {
            return new List<string>
            {
                // Do not move the first three
                "Icky Green",  "Light Brown",  "Clear",
                "Azure", "Blue", "Blue Speckled", "Black", "Brown", "Brown Speckled", "Bubbling",
                "Chartreuse", "Cloudy", "Copper Speckled", "Crimson", "Cyan", "Dark Blue",
                "Dark Green", "Dark Red", "Gold Speckled", "Green", "Green Speckled", "Grey",
                "Grey Speckled", "Hazy", "Indigo", "Light Blue", "Light Green", "Magenta",
                "Metallic Blue", "Metallic Red", "Metallic Green", "Metallic Purple", "Misty",
                "Orange", "Orange Speckled", "Pink", "Pink Speckled", "Puce", "Purple",
                "Purple Speckled", "Red", "Red Speckled", "Silver Speckled", "Smoky",
                "Tangerine", "Violet", "Vermilion", "White", "Yellow",
            };
        }
        private static List<string> CreateMushrooms()
        {
            return new List<string>
            {
                "Blue", "Black", "Black Spotted", "Brown", "Dark Blue", "Dark Green", "Dark Red",
                "Ecru", "Furry", "Green", "Grey", "Light Blue", "Light Green", "Plaid", "Red",
                "Slimy", "Tan", "White", "White Spotted", "Wooden", "Wrinkled", "Yellow",
            };
        }

        private static List<string> CreateWoods()
        {
            return new List<string>
            {
                "Aspen", "Balsa", "Banyan", "Birch", "Cedar", "Cottonwood", "Cypress", "Dogwood",
                "Elm", "Eucalyptus", "Hemlock", "Hickory", "Ironwood", "Locust", "Mahogany",
                "Maple", "Mulberry", "Oak", "Pine", "Redwood", "Rosewood", "Spruce", "Sycamore",
                "Teak", "Walnut",
            };
        }

        private static List<string> CreateMetals()
        {
            return new List<string>
            {
                "Aluminum", "Cast Iron", "Chromium", "Copper", "Gold", "Iron", "Magnesium",
                "Molybdenum", "Nickel", "Rusty", "Silver", "Steel", "Tin", "Titanium", "Tungsten",
                "Zirconium", "Zinc", "Aluminum-Plated", "Copper-Plated", "Gold-Plated",
                "Nickel-Plated", "Silver-Plated", "Steel-Plated", "Tin-Plated", "Zinc-Plated",
            };
        }

        private static List<string> CreateRocks()
        {
            return new List<string>
            {
                "Alexandrite", "Amethyst", "Aquamarine", "Azurite", "Beryl", "Bloodstone",
                "Calcite", "Carnelian", "Corundum", "Diamond", "Emerald", "Fluorite", "Garnet",
                "Granite", "Jade", "Jasper", "Lapis Lazuli", "Malachite", "Marble", "Moonstone",
                "Onyx", "Opal", "Pearl", "Quartz", "Quartzite", "Rhodonite", "Ruby", "Sapphire",
                "Tiger Eye", "Topaz", "Turquoise", "Zircon"
            };
        }

        private static List<string> CreateAmulets()
        {
            return new List<string>
            {
                "Amber", "Driftwood", "Coral", "Agate", "Ivory", "Obsidian",
                "Bone", "Brass", "Bronze", "Pewter", "Tortoise Shell",
            };
        }

        private static List<string> CreateSyllables()
        {
            return new List<string>
            {
                "a",    "ab",   "ag",   "aks",  "ala",  "an",  "ankh", "app", "arg",
                "arze", "ash",  "aus",  "ban",  "bar",  "bat", "bek",  "bie", "bin",
                "bit",  "bjor", "blu",  "bot",  "bu",   "byt", "comp", "con", "cos",
                "cre",  "dalf", "dan",  "den",  "doe",  "dok", "eep",  "el",  "eng",
                "er",   "ere",  "erk",  "esh",  "evs",  "fa",  "fid",  "for", "fri",
                "fu",   "gan",  "gar",  "glen", "gop",  "gre", "ha",   "he",  "hyd",
                "i",    "ing",  "ion",  "ip",   "ish",  "it",  "ite",  "iv",  "jo",
                "kho",  "kli",  "klis", "la",   "lech", "man", "mar",  "me",  "mi",
                "mic",  "mik",  "mon",  "mung", "mur",  "nej", "nelg", "nep", "ner",
                "nes",  "nis",  "nih",  "nin",  "o",    "od",  "ood",  "org", "orn",
                "ox",   "oxy",  "pay",  "pet",  "ple",  "plu", "po",   "pot", "prok",
                "re",   "rea",  "rhov", "ri",   "ro",   "rog", "rok",  "rol", "sa",
                "san",  "sat",  "see",  "sef",  "seh",  "shu", "ski",  "sna", "sne",
                "snik", "sno",  "so",   "sol",  "sri",  "sta", "sun",  "ta",  "tab",
                "tem",  "ther", "ti",   "tox",  "trol", "tue", "turs", "u",   "ulk",
                "um",   "un",   "uni",  "ur",   "val",  "viv", "vly",  "vom", "wah",
                "wed",  "werg", "wex",  "whon", "wun",  "x",   "yerg", "yp",  "zun",
            };
        }

        private static List<IReadOnlyList<uint>> CreateBlowsTable()
        {
            return new List<IReadOnlyList<uint>>
            {
                // STR/W:   9  18  67  107 117 118  : DEX
                new List<uint>{ 1,  1,  1,  1,  1,  1 }.AsReadOnly(), // <2
                new List<uint>{ 1,  1,  1,  1,  2,  2 }.AsReadOnly(), // <3
                new List<uint>{ 1,  1,  1,  2,  2,  3 }.AsReadOnly(), // <4
                new List<uint>{ 1,  1,  2,  2,  3,  3 }.AsReadOnly(), // <5
                new List<uint>{ 1,  2,  2,  3,  3,  4 }.AsReadOnly(), // <7
                new List<uint>{ 1,  2,  2,  3,  4,  4 }.AsReadOnly(), // <9
                new List<uint>{ 2,  2,  3,  3,  4,  4 }.AsReadOnly(), // >9
            };
        }

        private static List<uint> CreateNormalTable()
        {
            return new List<uint>
            {
                206, 613, 1022, 1430, 1838, 2245, 2652, 3058,
                3463, 3867, 4271, 4673, 5075, 5475, 5874, 6271,
                6667, 7061, 7454, 7845, 8234, 8621, 9006, 9389,
                9770, 10148, 10524, 10898, 11269, 11638, 12004, 12367,
                12727, 13085, 13440, 13792, 14140, 14486, 14828, 15168,
                15504, 15836, 16166, 16492, 16814, 17133, 17449, 17761,
                18069, 18374, 18675, 18972, 19266, 19556, 19842, 20124,
                20403, 20678, 20949, 21216, 21479, 21738, 21994, 22245,
                22493, 22737, 22977, 23213, 23446, 23674, 23899, 24120,
                24336, 24550, 24759, 24965, 25166, 25365, 25559, 25750,
                25937, 26120, 26300, 26476, 26649, 26818, 26983, 27146,
                27304, 27460, 27612, 27760, 27906, 28048, 28187, 28323,
                28455, 28585, 28711, 28835, 28955, 29073, 29188, 29299,
                29409, 29515, 29619, 29720, 29818, 29914, 30007, 30098,
                30186, 30272, 30356, 30437, 30516, 30593, 30668, 30740,
                30810, 30879, 30945, 31010, 31072, 31133, 31192, 31249,
                31304, 31358, 31410, 31460, 31509, 31556, 31601, 31646,
                31688, 31730, 31770, 31808, 31846, 31882, 31917, 31950,
                31983, 32014, 32044, 32074, 32102, 32129, 32155, 32180,
                32205, 32228, 32251, 32273, 32294, 32314, 32333, 32352,
                32370, 32387, 32404, 32420, 32435, 32450, 32464, 32477,
                32490, 32503, 32515, 32526, 32537, 32548, 32558, 32568,
                32577, 32586, 32595, 32603, 32611, 32618, 32625, 32632,
                32639, 32645, 32651, 32657, 32662, 32667, 32672, 32677,
                32682, 32686, 32690, 32694, 32698, 32702, 32705, 32708,
                32711, 32714, 32717, 32720, 32722, 32725, 32727, 32729,
                32731, 32733, 32735, 32737, 32739, 32740, 32742, 32743,
                32745, 32746, 32747, 32748, 32749, 32750, 32751, 32752,
                32753, 32754, 32755, 32756, 32757, 32757, 32758, 32758,
                32759, 32760, 32760, 32761, 32761, 32761, 32762, 32762,
                32763, 32763, 32763, 32764, 32764, 32764, 32764, 32765,
                32765, 32765, 32765, 32766, 32766, 32766, 32766, 32766,
            };
        }
    }
}
