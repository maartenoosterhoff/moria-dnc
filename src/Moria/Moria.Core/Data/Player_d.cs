using System.Collections.Generic;
using Moria.Core.Configs;
using Moria.Core.Structures;

namespace Moria.Core.Data
{
    public class Player_d
    {
        public Player_d()
        {
            this.class_rank_titles = CreateClassRankTitles().AsReadOnly();
            this.character_races = CreateCharacterRaces().AsReadOnly();
            this.character_backgrounds = CreateCharacterBackgrounds().AsReadOnly();
            this.classes = CreateClasses().AsReadOnly();
            this.class_level_adj = CreateClassLevelAdj().AsReadOnly();
            this.magic_spells = CreateMagicSpells().AsReadOnly();
            this.spell_names = CreateSpellNames().AsReadOnly();
            this.class_base_provisions = CreateClassBaseProvisions().AsReadOnly();
        }

        public IReadOnlyList<IReadOnlyList<string>> class_rank_titles { get; }
        //ArrayInitializer.InitializeWithDefault<string>(Player_c.PLAYER_MAX_CLASSES, Player_c.PLAYER_MAX_LEVEL);

        public IReadOnlyList<Race_t> character_races { get; }
        //ArrayInitializer.Initialize<Race_t>(Player_c.PLAYER_MAX_RACES);

        public IReadOnlyList<Background_t> character_backgrounds { get; }
        //ArrayInitializer.Initialize<Background_t>(Player_c.PLAYER_MAX_BACKGROUNDS);

        public IReadOnlyList<Class_t> classes { get; }
        //ArrayInitializer.Initialize<Class_t>(Player_c.PLAYER_MAX_CLASSES);

        public IReadOnlyList<IReadOnlyList<int>> class_level_adj { get; }
        //ArrayInitializer.InitializeWithDefault<int>(Player_c.PLAYER_MAX_CLASSES, Player_c.CLASS_MAX_LEVEL_ADJUST);

        public IReadOnlyList<IReadOnlyList<Spell_t>> magic_spells { get; }
        //ArrayInitializer.InitializeWithDefault<Spell_t>(Player_c.PLAYER_MAX_CLASSES - 1, 31);

        public IReadOnlyList<string> spell_names { get; }

        public IReadOnlyList<IReadOnlyList<uint>> class_base_provisions { get; }
        //ArrayInitializer.InitializeWithDefault<uint>(Player_c.PLAYER_MAX_CLASSES, 5);

        private static List<IReadOnlyList<string>> CreateClassRankTitles()
        {
            return new List<IReadOnlyList<string>>
            {
    // Warrior
    new List<string>
    {"Rookie",       "Private",      "Soldier",      "Mercenary",
     "Veteran(1st)", "Veteran(2nd)", "Veteran(3rd)", "Warrior(1st)",
     "Warrior(2nd)", "Warrior(3rd)", "Warrior(4th)", "Swordsman-1",
     "Swordsman-2",  "Swordsman-3",  "Hero",         "Swashbuckler",
     "Myrmidon",     "Champion-1",   "Champion-2",   "Champion-3",
     "Superhero",    "Knight",       "Superior Knt", "Gallant Knt",
     "Knt Errant",   "Guardian Knt", "Baron",        "Duke",
     "Lord (1st)",   "Lord (2nd)",   "Lord (3rd)",   "Lord (4th)",
     "Lord (5th)",   "Lord (6th)",   "Lord (7th)",   "Lord (8th)",
     "Lord (9th)",   "Lord Gallant", "Lord Keeper",  "Lord Noble"}.AsReadOnly(),
    // Mage
    new List<string>
    {"Novice",       "Apprentice",   "Trickster-1",  "Trickster-2",
     "Trickster-3",  "Cabalist-1",   "Cabalist-2",   "Cabalist-3",
     "Visionist",    "Phantasmist",  "Shadowist",    "Spellbinder",
     "Illusionist",  "Evoker (1st)", "Evoker (2nd)", "Evoker (3rd)",
     "Evoker (4th)", "Conjurer",     "Theurgist",    "Thaumaturge",
     "Magician",     "Enchanter",    "Warlock",      "Sorcerer",
     "Necromancer",  "Mage (1st)",   "Mage (2nd)",   "Mage (3rd)",
     "Mage (4th)",   "Mage (5th)",   "Wizard (1st)", "Wizard (2nd)",
     "Wizard (3rd)", "Wizard (4th)", "Wizard (5th)", "Wizard (6th)",
     "Wizard (7th)", "Wizard (8th)", "Wizard (9th)", "Wizard Lord"}.AsReadOnly(),
    // Priests
    new List<string>
    {"Believer",     "Acolyte(1st)", "Acolyte(2nd)", "Acolyte(3rd)",
     "Adept (1st)",  "Adept (2nd)",  "Adept (3rd)",  "Priest (1st)",
     "Priest (2nd)", "Priest (3rd)", "Priest (4th)", "Priest (5th)",
     "Priest (6th)", "Priest (7th)", "Priest (8th)", "Priest (9th)",
     "Curate (1st)", "Curate (2nd)", "Curate (3rd)", "Curate (4th)",
     "Curate (5th)", "Curate (6th)", "Curate (7th)", "Curate (8th)",
     "Curate (9th)", "Canon (1st)",  "Canon (2nd)",  "Canon (3rd)",
     "Canon (4th)",  "Canon (5th)",  "Low Lama",     "Lama-1",
     "Lama-2",       "Lama-3",       "High Lama",    "Great Lama",
     "Patriarch",    "High Priest",  "Great Priest", "Noble Priest"}.AsReadOnly(),
    // Rogues
    new List<string>
    {"Vagabond",     "Footpad",     "Cutpurse",      "Robber",
     "Burglar",      "Filcher",     "Sharper",       "Magsman",
     "Common Rogue", "Rogue (1st)", "Rogue (2nd)",   "Rogue (3rd)",
     "Rogue (4th)",  "Rogue (5th)", "Rogue (6th)",   "Rogue (7th)",
     "Rogue (8th)",  "Rogue (9th)", "Master Rogue",  "Expert Rogue",
     "Senior Rogue", "Chief Rogue", "Prime Rogue",   "Low Thief",
     "Thief (1st)",  "Thief (2nd)", "Thief (3rd)",   "Thief (4th)",
     "Thief (5th)",  "Thief (6th)", "Thief (7th)",   "Thief (8th)",
     "Thief (9th)",  "High Thief",  "Master Thief",  "Executioner",
     "Low Assassin", "Assassin",    "High Assassin", "Guildsmaster"}.AsReadOnly(),
    // Rangers
    new List<string>
    {"Runner (1st)",  "Runner (2nd)",  "Runner (3rd)",  "Strider (1st)",
     "Strider (2nd)", "Strider (3rd)", "Scout (1st)",   "Scout (2nd)",
     "Scout (3rd)",   "Scout (4th)",   "Scout (5th)",   "Courser (1st)",
     "Courser (2nd)", "Courser (3rd)", "Courser (4th)", "Courser (5th)",
     "Tracker (1st)", "Tracker (2nd)", "Tracker (3rd)", "Tracker (4th)",
     "Tracker (5th)", "Tracker (6th)", "Tracker (7th)", "Tracker (8th)",
     "Tracker (9th)", "Guide (1st)",   "Guide (2nd)",   "Guide (3rd)",
     "Guide (4th)",   "Guide (5th)",   "Guide (6th)",   "Guide (7th)",
     "Guide (8th)",   "Guide (9th)",   "Pathfinder-1",  "Pathfinder-2",
     "Pathfinder-3",  "Ranger",        "High Ranger",   "Ranger Lord"}.AsReadOnly(),
    // Paladins
    new List<string>
    {"Gallant",      "Keeper (1st)", "Keeper (2nd)", "Keeper (3rd)",
     "Keeper (4th)", "Keeper (5th)", "Keeper (6th)", "Keeper (7th)",
     "Keeper (8th)", "Keeper (9th)", "Protector-1",  "Protector-2",
     "Protector-3",  "Protector-4",  "Protector-5",  "Protector-6",
     "Protector-7",  "Protector-8",  "Defender-1",   "Defender-2",
     "Defender-3",   "Defender-4",   "Defender-5",   "Defender-6",
     "Defender-7",   "Defender-8",   "Warder (1st)", "Warder (2nd)",
     "Warder (3rd)", "Warder (4th)", "Warder (5th)", "Warder (6th)",
     "Warder (7th)", "Warder (8th)", "Warder (9th)", "Guardian",
     "Chevalier",    "Justiciar",    "Paladin",      "High Lord"}.AsReadOnly(),
            };
        }

        private static List<Race_t> CreateCharacterRaces()
        {
            return new List<Race_t>
            {
                new Race_t(
                    "Human", 0,  0,  0,  0,  0,  0,
                    14,  6, 72,  6,180, 25, 66,  4,150, 20,
                    0,  0,  0,  0,  0,  0,  0, 10,  0, 100, 0x3F
                ),
                new Race_t(
                    "Half-Elf", -1,  1,  0,  1, -1,  1,
                    24, 16, 66,  6,130, 15, 62,  6,100, 10,
                    2,  6,  1, -1, -1,  5,  3,  9,  2, 110, 0x3F
                ),
                new Race_t(
                    "Elf", -1,  2,  1,  1, -2,  1,
                    75, 75, 60,  4,100,  6, 54,  4, 80,  6,
                    5,  8,  1, -2, -5, 15,  6,  8,  3, 120, 0x1F
                ),
                new Race_t(
                    "Halfling", -2,  2,  1,  3,  1,  1,
                    21, 12, 36,  3, 60,  3, 33,  3, 50,  3,
                    15, 12,  4, -5,-10, 20, 18,  6,  4, 110, 0x0B
                ),
                new Race_t(
                    "Gnome", -1,  2,  0,  2,  1, -2,
                    50, 40, 42,  3, 90,  6, 39,  3, 75,  3,
                    10,  6,  3, -3, -8, 12, 12,  7,  4, 125, 0x0F
                ),
                new Race_t(
                    "Dwarf", 2, -3,  1, -2,  2, -3,
                    35, 15, 48,  3,150, 10, 46,  3,120, 10,
                    2,  7,  -1,  0, 15,  0,  9,  9,  5, 120, 0x05
                ),
                new Race_t(
                    "Half-Orc", 2, -1,  0,  0,  1, -4,
                    11,  4, 66,  1,150,  5, 62,  1,120,  5,
                    -3,  0, -1,  3, 12, -5, -3, 10,  3, 110, 0x0D
                ),
                new Race_t(
                    "Half-Troll", 4, -4, -2, -4,  3, -6,
                    20, 10, 96, 10,255, 50, 84,  8,225, 40,
                    -5, -1, -2,  5, 20,-10, -8, 12,  3, 120, 0x05
                )
            };
        }

        private static List<Background_t> CreateCharacterBackgrounds()
        {
            return new List<Background_t>
            {
    new Background_t("You are the illegitimate and unacknowledged child ",   10,  1,  2,  25),
    new Background_t("You are the illegitimate but acknowledged child ",     20,  1,  2,  35),
    new Background_t("You are one of several children ",                     95,  1,  2,  45),
    new Background_t("You are the first child ",                            100,  1,  2,  50),
    new Background_t("of a Serf.  ",                                         40,  2,  3,  65),
    new Background_t("of a Yeoman.  ",                                       65,  2,  3,  80),
    new Background_t("of a Townsman.  ",                                     80,  2,  3,  90),
    new Background_t("of a Guildsman.  ",                                    90,  2,  3, 105),
    new Background_t("of a Landed Knight.  ",                                96,  2,  3, 120),
    new Background_t("of a Titled Noble.  ",                                 99,  2,  3, 130),
    new Background_t("You are the black sheep of the family.  ",             20,  3, 50,  20),
    new Background_t("You are a credit to the family.  ",                    80,  3, 50,  55),
    new Background_t("You are a well liked child.  ",                       100,  3, 50,  60),
    new Background_t("Your mother was a Green-Elf.  ",                       40,  4,  1,  50),
    new Background_t("Your father was a Green-Elf.  ",                       75,  4,  1,  55),
    new Background_t("Your mother was a Grey-Elf.  ",                        90,  4,  1,  55),
    new Background_t("Your father was a Grey-Elf.  ",                        95,  4,  1,  60),
    new Background_t("Your mother was a High-Elf.  ",                        98,  4,  1,  65),
    new Background_t("Your father was a High-Elf.  ",                       100,  4,  1,  70),
    new Background_t("You are one of several children ",                     60,  7,  8,  50),
    new Background_t("You are the only child ",                             100,  7,  8,  55),
    new Background_t("of a Green-Elf ",                                      75,  8,  9,  50),
    new Background_t("of a Grey-Elf ",                                       95,  8,  9,  55),
    new Background_t("of a High-Elf ",                                      100,  8,  9,  60),
    new Background_t("Ranger.  ",                                            40,  9, 54,  80),
    new Background_t("Archer.  ",                                            70,  9, 54,  90),
    new Background_t("Warrior.  ",                                           87,  9, 54, 110),
    new Background_t("Mage.  ",                                              95,  9, 54, 125),
    new Background_t("Prince.  ",                                            99,  9, 54, 140),
    new Background_t("King.  ",                                             100,  9, 54, 145),
    new Background_t("You are one of several children of a Halfling ",       85, 10, 11,  45),
    new Background_t("You are the only child of a Halfling ",               100, 10, 11,  55),
    new Background_t("Bum.  ",                                               20, 11,  3,  55),
    new Background_t("Tavern Owner.  ",                                      30, 11,  3,  80),
    new Background_t("Miller.  ",                                            40, 11,  3,  90),
    new Background_t("Home Owner.  ",                                        50, 11,  3, 100),
    new Background_t("Burglar.  ",                                           80, 11,  3, 110),
    new Background_t("Warrior.  ",                                           95, 11,  3, 115),
    new Background_t("Mage.  ",                                              99, 11,  3, 125),
    new Background_t("Clan Elder.  ",                                       100, 11,  3, 140),
    new Background_t("You are one of several children of a Gnome ",          85, 13, 14,  45),
    new Background_t("You are the only child of a Gnome ",                  100, 13, 14,  55),
    new Background_t("Beggar.  ",                                            20, 14,  3,  55),
    new Background_t("Braggart.  ",                                          50, 14,  3,  70),
    new Background_t("Prankster.  ",                                         75, 14,  3,  85),
    new Background_t("Warrior.  ",                                           95, 14,  3, 100),
    new Background_t("Mage.  ",                                             100, 14,  3, 125),
    new Background_t("You are one of two children of a Dwarven ",            25, 16, 17,  40),
    new Background_t("You are the only child of a Dwarven ",                100, 16, 17,  50),
    new Background_t("Thief.  ",                                             10, 17, 18,  60),
    new Background_t("Prison Guard.  ",                                      25, 17, 18,  75),
    new Background_t("Miner.  ",                                             75, 17, 18,  90),
    new Background_t("Warrior.  ",                                           90, 17, 18, 110),
    new Background_t("Priest.  ",                                            99, 17, 18, 130),
    new Background_t("King.  ",                                             100, 17, 18, 150),
    new Background_t("You are the black sheep of the family.  ",             15, 18, 57,  10),
    new Background_t("You are a credit to the family.  ",                    85, 18, 57,  50),
    new Background_t("You are a well liked child.  ",                       100, 18, 57,  55),
    new Background_t("Your mother was an Orc, but it is unacknowledged.  ",  25, 19, 20,  25),
    new Background_t("Your father was an Orc, but it is unacknowledged.  ", 100, 19, 20,  25),
    new Background_t("You are the adopted child ",                          100, 20,  2,  50),
    new Background_t("Your mother was a Cave-Troll ",                        30, 22, 23,  20),
    new Background_t("Your father was a Cave-Troll ",                        60, 22, 23,  25),
    new Background_t("Your mother was a Hill-Troll ",                        75, 22, 23,  30),
    new Background_t("Your father was a Hill-Troll ",                        90, 22, 23,  35),
    new Background_t("Your mother was a Water-Troll ",                       95, 22, 23,  40),
    new Background_t("Your father was a Water-Troll ",                      100, 22, 23,  45),
    new Background_t("Cook.  ",                                               5, 23, 62,  60),
    new Background_t("Warrior.  ",                                           95, 23, 62,  55),
    new Background_t("Shaman.  ",                                            99, 23, 62,  65),
    new Background_t("Clan Chief.  ",                                       100, 23, 62,  80),
    new Background_t("You have dark brown eyes, ",                           20, 50, 51,  50),
    new Background_t("You have brown eyes, ",                                60, 50, 51,  50),
    new Background_t("You have hazel eyes, ",                                70, 50, 51,  50),
    new Background_t("You have green eyes, ",                                80, 50, 51,  50),
    new Background_t("You have blue eyes, ",                                 90, 50, 51,  50),
    new Background_t("You have blue-gray eyes, ",                           100, 50, 51,  50),
    new Background_t("straight ",                                            70, 51, 52,  50),
    new Background_t("wavy ",                                                90, 51, 52,  50),
    new Background_t("curly ",                                              100, 51, 52,  50),
    new Background_t("black hair, ",                                         30, 52, 53,  50),
    new Background_t("brown hair, ",                                         70, 52, 53,  50),
    new Background_t("auburn hair, ",                                        80, 52, 53,  50),
    new Background_t("red hair, ",                                           90, 52, 53,  50),
    new Background_t("blond hair, ",                                        100, 52, 53,  50),
    new Background_t("and a very dark complexion.",                          10, 53,  0,  50),
    new Background_t("and a dark complexion.",                               30, 53,  0,  50),
    new Background_t("and an average complexion.",                           80, 53,  0,  50),
    new Background_t("and a fair complexion.",                               90, 53,  0,  50),
    new Background_t("and a very fair complexion.",                         100, 53,  0,  50),
    new Background_t("You have light grey eyes, ",                           85, 54, 55,  50),
    new Background_t("You have light blue eyes, ",                           95, 54, 55,  50),
    new Background_t("You have light green eyes, ",                         100, 54, 55,  50),
    new Background_t("straight ",                                            75, 55, 56,  50),
    new Background_t("wavy ",                                               100, 55, 56,  50),
    new Background_t("black hair, and a fair complexion.",                   75, 56,  0,  50),
    new Background_t("brown hair, and a fair complexion.",                   85, 56,  0,  50),
    new Background_t("blond hair, and a fair complexion.",                   95, 56,  0,  50),
    new Background_t("silver hair, and a fair complexion.",                 100, 56,  0,  50),
    new Background_t("You have dark brown eyes, ",                           99, 57, 58,  50),
    new Background_t("You have glowing red eyes, ",                         100, 57, 58,  60),
    new Background_t("straight ",                                            90, 58, 59,  50),
    new Background_t("wavy ",                                               100, 58, 59,  50),
    new Background_t("black hair, ",                                         75, 59, 60,  50),
    new Background_t("brown hair, ",                                        100, 59, 60,  50),
    new Background_t("a one foot beard, ",                                   25, 60, 61,  50),
    new Background_t("a two foot beard, ",                                   60, 60, 61,  51),
    new Background_t("a three foot beard, ",                                 90, 60, 61,  53),
    new Background_t("a four foot beard, ",                                 100, 60, 61,  55),
    new Background_t("and a dark complexion.",                              100, 61,  0,  50),
    new Background_t("You have slime green eyes, ",                          60, 62, 63,  50),
    new Background_t("You have puke yellow eyes, ",                          85, 62, 63,  50),
    new Background_t("You have blue-bloodshot eyes, ",                       99, 62, 63,  50),
    new Background_t("You have glowing red eyes, ",                         100, 62, 63,  55),
    new Background_t("dirty ",                                               33, 63, 64,  50),
    new Background_t("mangy ",                                               66, 63, 64,  50),
    new Background_t("oily ",                                               100, 63, 64,  50),
    new Background_t("sea-weed green hair, ",                                33, 64, 65,  50),
    new Background_t("bright red hair, ",                                    66, 64, 65,  50),
    new Background_t("dark purple hair, ",                                  100, 64, 65,  50),
    new Background_t("and green ",                                           25, 65, 66,  50),
    new Background_t("and blue ",                                            50, 65, 66,  50),
    new Background_t("and white ",                                           75, 65, 66,  50),
    new Background_t("and black ",                                          100, 65, 66,  50),
    new Background_t("ulcerous skin.",                                       33, 66,  0,  50),
    new Background_t("scabby skin.",                                         66, 66,  0,  50),
    new Background_t("leprous skin.",                                       100, 66,  0,  50),
    new Background_t("of a Royal Blood Line.  ",                            100,  2,  3, 140),
            };
        }

        private static List<Class_t> CreateClasses()
        {
            return new List<Class_t>
            {
                // class       hp dis src stl fos bth btb sve  s   i   w   d  co  ch  spell             exp  spl
                new Class_t("Warrior", 9, 25, 14, 1, 38, 70, 55, 18,  5, -2, -2,  2,  2, -1, Config.spells.SPELL_TYPE_NONE,    0, 0),
                new Class_t("Mage",    0, 30, 16, 2, 20, 34, 20, 36, -5,  3,  0,  1, -2,  1, Config.spells.SPELL_TYPE_MAGE,   30, 1),
                new Class_t("Priest",  2, 25, 16, 2, 32, 48, 35, 30, -3, -3,  3, -1,  0,  2, Config.spells.SPELL_TYPE_PRIEST, 20, 1),
                new Class_t("Rogue",   6, 45, 32, 5, 16, 60, 66, 30,  2,  1, -2,  3,  1, -1, Config.spells.SPELL_TYPE_MAGE,    0, 5),
                new Class_t("Ranger",  4, 30, 24, 3, 24, 56, 72, 30,  2,  2,  0,  1,  1,  1, Config.spells.SPELL_TYPE_MAGE,   40, 3),
                new Class_t("Paladin", 6, 20, 12, 1, 38, 68, 40, 24,  3, -3,  1,  0,  2,  2, Config.spells.SPELL_TYPE_PRIEST, 35, 1),
            };
        }

        private static List<IReadOnlyList<int>> CreateClassLevelAdj()
        {
            return new List<IReadOnlyList<int>>
            {
                new List<int> { 4, 4, 2, 2, 3 }.AsReadOnly(), // Warrior
                new List<int> { 2, 2, 4, 3, 3 }.AsReadOnly(), // Mage
                new List<int> { 2, 2, 4, 3, 3 }.AsReadOnly(), // Priest
                new List<int> { 3, 4, 3, 4, 3 }.AsReadOnly(), // Rogue
                new List<int> { 3, 4, 3, 3, 3 }.AsReadOnly(), // Ranger
                new List<int> { 3, 3, 3, 2, 3 }.AsReadOnly(), // Paladin
            };
        }

        private static List<IReadOnlyList<Spell_t>> CreateMagicSpells()
        {
            return new List<IReadOnlyList<Spell_t>>
                {
    new List<Spell_t> {
        // Mage
        new Spell_t(  1,  1, 22,   1),
        new Spell_t(  1,  1, 23,   1),
        new Spell_t(  1,  2, 24,   1),
        new Spell_t(  1,  2, 26,   1),
        new Spell_t(  3,  3, 25,   2),
        new Spell_t(  3,  3, 25,   1),
        new Spell_t(  3,  3, 27,   2),
        new Spell_t(  3,  4, 30,   1),
        new Spell_t(  5,  4, 30,   6),
        new Spell_t(  5,  5, 30,   8),
        new Spell_t(  5,  5, 30,   5),
        new Spell_t(  5,  5, 35,   6),
        new Spell_t(  7,  6, 35,   9),
        new Spell_t(  7,  6, 50,  10),
        new Spell_t(  7,  6, 40,  12),
        new Spell_t(  9,  7, 44,  19),
        new Spell_t(  9,  7, 45,  19),
        new Spell_t(  9,  7, 75,  22),
        new Spell_t(  9,  7, 45,  19),
        new Spell_t( 11,  7, 45,  25),
        new Spell_t( 11,  7, 99,  19),
        new Spell_t( 13,  7, 50,  22),
        new Spell_t( 15,  9, 50,  25),
        new Spell_t( 17,  9, 50,  31),
        new Spell_t( 19, 12, 55,  38),
        new Spell_t( 21, 12, 90,  44),
        new Spell_t( 23, 12, 60,  50),
        new Spell_t( 25, 12, 65,  63),
        new Spell_t( 29, 18, 65,  88),
        new Spell_t( 33, 21, 80, 125),
        new Spell_t( 37, 25, 95, 200),
    }.AsReadOnly(),
    new List<Spell_t> {
        // Priest
        new Spell_t(  1,  1, 10,   1),
        new Spell_t(  1,  2, 15,   1),
        new Spell_t(  1,  2, 20,   1),
        new Spell_t(  1,  2, 25,   1),
        new Spell_t(  3,  2, 25,   1),
        new Spell_t(  3,  3, 27,   2),
        new Spell_t(  3,  3, 27,   2),
        new Spell_t(  3,  3, 28,   3),
        new Spell_t(  5,  4, 29,   4),
        new Spell_t(  5,  4, 30,   5),
        new Spell_t(  5,  4, 32,   5),
        new Spell_t(  5,  5, 34,   5),
        new Spell_t(  7,  5, 36,   6),
        new Spell_t(  7,  5, 38,   7),
        new Spell_t(  7,  6, 38,   9),
        new Spell_t(  7,  7, 38,   9),
        new Spell_t(  9,  6, 38,  10),
        new Spell_t(  9,  7, 38,  10),
        new Spell_t(  9,  7, 40,  10),
        new Spell_t( 11,  8, 42,  10),
        new Spell_t( 11,  8, 42,  12),
        new Spell_t( 11,  9, 55,  15),
        new Spell_t( 13, 10, 45,  15),
        new Spell_t( 13, 11, 45,  16),
        new Spell_t( 15, 12, 50,  20),
        new Spell_t( 15, 14, 50,  22),
        new Spell_t( 17, 14, 55,  32),
        new Spell_t( 21, 16, 60,  38),
        new Spell_t( 25, 20, 70,  75),
        new Spell_t( 33, 24, 90, 125),
        new Spell_t( 39, 32, 80, 200),
    }.AsReadOnly(),
    new List<Spell_t> {
        // Rogue
        new Spell_t( 99, 99,  0,   0),
        new Spell_t(  5,  1, 50,   1),
        new Spell_t(  7,  2, 55,   1),
        new Spell_t(  9,  3, 60,   2),
        new Spell_t( 11,  4, 65,   2),
        new Spell_t( 13,  5, 70,   3),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 15,  6, 75,   3),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 17,  7, 80,   4),
        new Spell_t( 19,  8, 85,   5),
        new Spell_t( 21,  9, 90,   6),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 23, 10, 95,   7),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 25, 12, 95,   9),
        new Spell_t( 27, 15, 99,  11),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 29, 18, 99,  19),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
        new Spell_t( 99, 99,  0,   0),
    }.AsReadOnly(),
    new List<Spell_t> {
        // Ranger
        new Spell_t(  3,  1, 30,   1),
        new Spell_t(  3,  2, 35,   2),
        new Spell_t(  3,  2, 35,   2),
        new Spell_t(  5,  3, 35,   2),
        new Spell_t(  5,  3, 40,   2),
        new Spell_t(  5,  4, 45,   3),
        new Spell_t(  7,  5, 40,   6),
        new Spell_t(  7,  6, 40,   5),
        new Spell_t(  9,  7, 40,   7),
        new Spell_t(  9,  8, 45,   8),
        new Spell_t( 11,  8, 40,  10),
        new Spell_t( 11,  9, 45,  10),
        new Spell_t( 13, 10, 45,  12),
        new Spell_t( 13, 11, 55,  13),
        new Spell_t( 15, 12, 50,  15),
        new Spell_t( 15, 13, 50,  15),
        new Spell_t( 17, 17, 55,  15),
        new Spell_t( 17, 17, 90,  17),
        new Spell_t( 21, 17, 55,  17),
        new Spell_t( 21, 19, 60,  18),
        new Spell_t( 23, 25, 95,  20),
        new Spell_t( 23, 20, 60,  20),
        new Spell_t( 25, 20, 60,  20),
        new Spell_t( 25, 21, 65,  20),
        new Spell_t( 27, 21, 65,  22),
        new Spell_t( 29, 23, 95,  23),
        new Spell_t( 31, 25, 70,  25),
        new Spell_t( 33, 25, 75,  38),
        new Spell_t( 35, 25, 80,  50),
        new Spell_t( 37, 30, 95, 100),
        new Spell_t( 99, 99,  0,   0),
    }.AsReadOnly(),
    new List<Spell_t> {
        // Paladin
        new Spell_t(  1,  1, 30,   1),
        new Spell_t(  2,  2, 35,   2),
        new Spell_t(  3,  3, 35,   3),
        new Spell_t(  5,  3, 35,   5),
        new Spell_t(  5,  4, 35,   5),
        new Spell_t(  7,  5, 40,   6),
        new Spell_t(  7,  5, 40,   6),
        new Spell_t(  9,  7, 40,   7),
        new Spell_t(  9,  7, 40,   8),
        new Spell_t(  9,  8, 40,   8),
        new Spell_t( 11,  9, 40,  10),
        new Spell_t( 11, 10, 45,  10),
        new Spell_t( 11, 10, 45,  10),
        new Spell_t( 13, 10, 45,  12),
        new Spell_t( 13, 11, 45,  13),
        new Spell_t( 15, 13, 45,  15),
        new Spell_t( 15, 15, 50,  15),
        new Spell_t( 17, 15, 50,  17),
        new Spell_t( 17, 15, 50,  18),
        new Spell_t( 19, 15, 50,  19),
        new Spell_t( 19, 15, 50,  19),
        new Spell_t( 21, 17, 50,  20),
        new Spell_t( 23, 17, 50,  20),
        new Spell_t( 25, 20, 50,  20),
        new Spell_t( 27, 21, 50,  22),
        new Spell_t( 29, 22, 50,  24),
        new Spell_t( 31, 24, 60,  25),
        new Spell_t( 33, 28, 60,  31),
        new Spell_t( 35, 32, 70,  38),
        new Spell_t( 37, 36, 90,  50),
        new Spell_t( 39, 38, 90, 100),
    }.AsReadOnly(),
                };
        }

        private static List<string> CreateSpellNames()
        {
            return new List<string>
            {
                // Mage Spells
                "Magic Missile", "Detect Monsters", "Phase Door", "Light Area",
                "Cure Light Wounds", "Find Hidden Traps/Doors", "Stinking Cloud",
                "Confusion", "Lightning Bolt", "Trap/Door Destruction", "Sleep I",
                "Cure Poison", "Teleport Self", "Remove Curse", "Frost Bolt",
                "Turn Stone to Mud", "Create Food", "Recharge Item I", "Sleep II",
                "Polymorph Other", "Identify", "Sleep III", "Fire Bolt", "Slow Monster",
                "Frost Ball", "Recharge Item II", "Teleport Other", "Haste Self",
                "Fire Ball", "Word of Destruction", "Genocide",

                // Priest Spells, start at index 31
                "Detect Evil", "Cure Light Wounds", "Bless", "Remove Fear", "Call Light",
                "Find Traps", "Detect Doors/Stairs", "Slow Poison", "Blind Creature",
                "Portal", "Cure Medium Wounds", "Chant", "Sanctuary", "Create Food",
                "Remove Curse", "Resist Heat and Cold", "Neutralize Poison",
                "Orb of Draining", "Cure Serious Wounds", "Sense Invisible",
                "Protection from Evil", "Earthquake", "Sense Surroundings",
                "Cure Critical Wounds", "Turn Undead", "Prayer", "Dispel Undead", "Heal",
                "Dispel Evil", "Glyph of Warding", "Holy Word",
            };
        }

        private static List<IReadOnlyList<uint>> CreateClassBaseProvisions()
        {
            // Each type of character starts out with a few provisions.
            //
            // Note that the entries refer to elements of the game_objects[] array.
            //      344 = Food Ration
            //      365 = Wooden Torch
            //      123 = Cloak
            //      318 = Beginners-Magick
            //      103 = Soft Leather Armor
            //       30 = Stiletto
            //      322 = Beginners Handbook
            return new List<IReadOnlyList<uint>>
            {
                new List<uint>{344, 365, 123, 30, 103}.AsReadOnly(), // Warrior
                new List<uint>{344, 365, 123, 30, 318}.AsReadOnly(), // Mage
                new List<uint>{344, 365, 123, 30, 322}.AsReadOnly(), // Priest
                new List<uint>{344, 365, 123, 30, 318}.AsReadOnly(), // Rogue
                new List<uint>{344, 365, 123, 30, 318}.AsReadOnly(), // Ranger
                new List<uint>{344, 365, 123, 30, 322}.AsReadOnly(),  // Paladin
            };
        }
    }
}
