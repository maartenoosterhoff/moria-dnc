using System.Collections.Generic;
using Moria.Core.Structures;
using static Moria.Core.Constants.Treasure_c;

namespace Moria.Core.Data
{
    public class Treasure_d
    {
        public Treasure_d()
        {
            this.special_item_names = CreateSpecialItemNames().AsReadOnly();
            this.game_objects = CreateGameObjects().AsReadOnly();
        }

        public IReadOnlyList<DungeonObject_t> game_objects { get; }

        public IReadOnlyList<string> special_item_names { get; }

        private static List<DungeonObject_t> CreateGameObjects()
        {
            var list = new List<DungeonObject_t>
            {
new DungeonObject_t("Poison",                          0x00000001L, TV_FOOD,        ',', 500,  0,    64,  1, 1,    0,  0, 0,   0, 0, 0, 7), // 0
new DungeonObject_t("Blindness",                       0x00000002L, TV_FOOD,        ',', 500,  0,    65,  1, 1,    0,  0, 0,   0, 0, 0, 9), // 1
new DungeonObject_t("Paranoia",                        0x00000004L, TV_FOOD,        ',', 500,  0,    66,  1, 1,    0,  0, 0,   0, 0, 0, 9), // 2
new DungeonObject_t("Confusion",                       0x00000008L, TV_FOOD,        ',', 500,  0,    67,  1, 1,    0,  0, 0,   0, 0, 0, 7), // 3
new DungeonObject_t("Hallucination",                   0x00000010L, TV_FOOD,        ',', 500,  0,    68,  1, 1,    0,  0, 0,   0, 0, 0, 13), // 4
new DungeonObject_t("Cure Poison",                     0x00000020L, TV_FOOD,        ',', 500,  60,   69,  1, 1,    0,  0, 0,   0, 0, 0, 8), // 5
new DungeonObject_t("Cure Blindness",                  0x00000040L, TV_FOOD,        ',', 500,  50,   70,  1, 1,    0,  0, 0,   0, 0, 0, 10), // 6
new DungeonObject_t("Cure Paranoia",                   0x00000080L, TV_FOOD,        ',', 500,  25,   71,  1, 1,    0,  0, 0,   0, 0, 0, 12), // 7
new DungeonObject_t("Cure Confusion",                  0x00000100L, TV_FOOD,        ',', 500,  50,   72,  1, 1,    0,  0, 0,   0, 0, 0, 6), // 8
new DungeonObject_t("Weakness",                        0x04000200L, TV_FOOD,        ',', 500,  0,    73,  1, 1,    0,  0, 0,   0, 0, 0, 7), // 9
new DungeonObject_t("Unhealth",                        0x04000400L, TV_FOOD,        ',', 500,  50,   74,  1, 1,    0,  0, 0,   0, 10, 10, 15), // 10
new DungeonObject_t("Restore Constitution",            0x00010000L, TV_FOOD,        ',', 500,  350,  75,  1, 1,    0,  0, 0,   0, 0, 0, 20), // 11
new DungeonObject_t("First-Aid",                       0x00200000L, TV_FOOD,        ',', 500,  5,    76,  1, 1,    0,  0, 0,   0, 0, 0, 6), // 12
new DungeonObject_t("Minor Cures",                     0x00400000L, TV_FOOD,        ',', 500,  20,   77,  1, 1,    0,  0, 0,   0, 0, 0, 7), // 13
new DungeonObject_t("Light Cures",                     0x00800000L, TV_FOOD,        ',', 500,  30,   78,  1, 1,    0,  0, 0,   0, 0, 0, 10), // 14
new DungeonObject_t("Restoration",                     0x001F8000L, TV_FOOD,        ',', 500,  1000, 79,  1, 1,    0,  0, 0,   0, 0, 0, 30), // 15
new DungeonObject_t("Poison",                          0x00000001L, TV_FOOD,        ',', 1200, 0,    80,  1, 1,    0,  0, 0,   0, 0, 0, 15), // 16
new DungeonObject_t("Hallucination",                   0x00000010L, TV_FOOD,        ',', 1200, 0,    81,  1, 1,    0,  0, 0,   0, 0, 0, 18), // 17
new DungeonObject_t("Cure Poison",                     0x00000020L, TV_FOOD,        ',', 1200, 75,   82,  1, 1,    0,  0, 0,   0, 0, 0, 19), // 18
new DungeonObject_t("Unhealth",                        0x04000400L, TV_FOOD,        ',', 1200, 75,   83,  1, 1,    0,  0, 0,   0, 10, 12, 28), // 19
new DungeonObject_t("Major Cures",                     0x02000000L, TV_FOOD,        ',', 1200, 75,   84,  1, 2,    0,  0, 0,   0, 0, 0, 16), // 20
new DungeonObject_t("& Ration~ of Food",               0x00000000L, TV_FOOD,        ',', 5000, 3,    90,  1, 10,   0,  0, 0,   0, 0, 0, 0), // 21
new DungeonObject_t("& Ration~ of Food",               0x00000000L, TV_FOOD,        ',', 5000, 3,    90,  1, 10,   0,  0, 0,   0, 0, 0, 5), // 22
new DungeonObject_t("& Ration~ of Food",               0x00000000L, TV_FOOD,        ',', 5000, 3,    90,  1, 10,   0,  0, 0,   0, 0, 0, 10), // 23
new DungeonObject_t("& Slime Mold~",                   0x00000000L, TV_FOOD,        ',', 3000, 2,    91,  1, 5,    0,  0, 0,   0, 0, 0, 1), // 24
new DungeonObject_t("& Piece~ of Elvish Waybread",     0x02000020L, TV_FOOD,        ',', 7500, 25,   92,  1, 3,    0,  0, 0,   0, 0, 0, 6), // 25
new DungeonObject_t("& Piece~ of Elvish Waybread",     0x02000020L, TV_FOOD,        ',', 7500, 25,   92,  1, 3,    0,  0, 0,   0, 0, 0, 12), // 26
new DungeonObject_t("& Piece~ of Elvish Waybread",     0x02000020L, TV_FOOD,        ',', 7500, 25,   92,  1, 3,    0,  0, 0,   0, 0, 0, 20), // 27

new DungeonObject_t("& Dagger (Main Gauche)",          0x00000000L, TV_SWORD,       '|', 0,    25,   1,   1, 30,   0,  0, 0,   0, 1, 5, 2), // 28
new DungeonObject_t("& Dagger (Misericorde)",          0x00000000L, TV_SWORD,       '|', 0,    10,   2,   1, 15,   0,  0, 0,   0, 1, 4, 0), // 29
new DungeonObject_t("& Dagger (Stiletto)",             0x00000000L, TV_SWORD,       '|', 0,    10,   3,   1, 12,   0,  0, 0,   0, 1, 4, 0), // 30
new DungeonObject_t("& Dagger (Bodkin)",               0x00000000L, TV_SWORD,       '|', 0,    10,   4,   1, 20,   0,  0, 0,   0, 1, 4, 1), // 31
new DungeonObject_t("& Broken Dagger",                 0x00000000L, TV_SWORD,       '|', 0,    0,    5,   1, 15,  -2, -2, 0,   0, 1, 1, 0), // 32
new DungeonObject_t("& Backsword",                     0x00000000L, TV_SWORD,       '|', 0,    150,  6,   1, 95,   0,  0, 0,   0, 1, 9, 7), // 33
new DungeonObject_t("& Bastard Sword",                 0x00000000L, TV_SWORD,       '|', 0,    350,  7,   1, 140,  0,  0, 0,   0, 3, 4, 14), // 34
new DungeonObject_t("& Thrusting Sword (Bilbo)",       0x00000000L, TV_SWORD,       '|', 0,    60,   8,   1, 80,   0,  0, 0,   0, 1, 6, 4), // 35
new DungeonObject_t("& Thrusting Sword (Baselard)",    0x00000000L, TV_SWORD,       '|', 0,    80,   9,   1, 100,  0,  0, 0,   0, 1, 7, 5), // 36
new DungeonObject_t("& Broadsword",                    0x00000000L, TV_SWORD,       '|', 0,    255,  10,  1, 150,  0,  0, 0,   0, 2, 5, 9), // 37
new DungeonObject_t("& Two-Handed Sword (Claymore)",   0x00000000L, TV_SWORD,       '|', 0,    775,  11,  1, 200,  0,  0, 0,   0, 3, 6, 30), // 38
new DungeonObject_t("& Cutlass",                       0x00000000L, TV_SWORD,       '|', 0,    85,   12,  1, 110,  0,  0, 0,   0, 1, 7, 7), // 39
new DungeonObject_t("& Two-Handed Sword (Espadon)",    0x00000000L, TV_SWORD,       '|', 0,    655,  13,  1, 180,  0,  0, 0,   0, 3, 6, 35), // 40
new DungeonObject_t("& Executioner's Sword",           0x00000000L, TV_SWORD,       '|', 0,    850,  14,  1, 260,  0,  0, 0,   0, 4, 5, 40), // 41
new DungeonObject_t("& Two-Handed Sword (Flamberge)",  0x00000000L, TV_SWORD,       '|', 0,    1000, 15,  1, 240,  0,  0, 0,   0, 4, 5, 45), // 42
new DungeonObject_t("& Foil",                          0x00000000L, TV_SWORD,       '|', 0,    35,   16,  1, 30,   0,  0, 0,   0, 1, 5, 2), // 43
new DungeonObject_t("& Katana",                        0x00000000L, TV_SWORD,       '|', 0,    400,  17,  1, 120,  0,  0, 0,   0, 3, 4, 18), // 44
new DungeonObject_t("& Longsword",                     0x00000000L, TV_SWORD,       '|', 0,    200,  18,  1, 130,  0,  0, 0,   0, 1, 10, 12), // 45
new DungeonObject_t("& Two-Handed Sword (No-Dachi)",   0x00000000L, TV_SWORD,       '|', 0,    675,  19,  1, 200,  0,  0, 0,   0, 4, 4, 45), // 46
new DungeonObject_t("& Rapier",                        0x00000000L, TV_SWORD,       '|', 0,    42,   20,  1, 40,   0,  0, 0,   0, 1, 6, 4), // 47
new DungeonObject_t("& Sabre",                         0x00000000L, TV_SWORD,       '|', 0,    50,   21,  1, 50,   0,  0, 0,   0, 1, 7, 5), // 48
new DungeonObject_t("& Small Sword",                   0x00000000L, TV_SWORD,       '|', 0,    48,   22,  1, 75,   0,  0, 0,   0, 1, 6, 5), // 49
new DungeonObject_t("& Two-Handed Sword (Zweihander)", 0x00000000L, TV_SWORD,       '|', 0,    1500, 23,  1, 280,  0,  0, 0,   0, 4, 6, 50), // 50
new DungeonObject_t("& Broken Sword",                  0x00000000L, TV_SWORD,       '|', 0,    0,    24,  1, 75,  -2, -2, 0,   0, 1, 1, 0), // 51
new DungeonObject_t("& Ball and Chain",                0x00000000L, TV_HAFTED,     '\\', 0,    200,  1,   1, 150,  0,  0, 0,   0, 2, 4, 20), // 52
new DungeonObject_t("& Cat-o'-Nine-Tails",             0x00000000L, TV_HAFTED,     '\\', 0,    14,   2,   1, 40,   0,  0, 0,   0, 1, 4, 3), // 53
new DungeonObject_t("& Wooden Club",                   0x00000000L, TV_HAFTED,     '\\', 0,    10,   3,   1, 100,  0,  0, 0,   0, 1, 3, 0), // 54
new DungeonObject_t("& Flail",                         0x00000000L, TV_HAFTED,     '\\', 0,    353,  4,   1, 150,  0,  0, 0,   0, 2, 6, 12), // 55
new DungeonObject_t("& Two-Handed Great Flail",        0x00000000L, TV_HAFTED,     '\\', 0,    590,  5,   1, 280,  0,  0, 0,   0, 3, 6, 45), // 56
new DungeonObject_t("& Morningstar",                   0x00000000L, TV_HAFTED,     '\\', 0,    396,  6,   1, 150,  0,  0, 0,   0, 2, 6, 10), // 57
new DungeonObject_t("& Mace",                          0x00000000L, TV_HAFTED,     '\\', 0,    130,  7,   1, 120,  0,  0, 0,   0, 2, 4, 6), // 58
new DungeonObject_t("& War Hammer",                    0x00000000L, TV_HAFTED,     '\\', 0,    225,  8,   1, 120,  0,  0, 0,   0, 3, 3, 5), // 59
new DungeonObject_t("& Lead-Filled Mace",              0x00000000L, TV_HAFTED,     '\\', 0,    502,  9,   1, 180,  0,  0, 0,   0, 3, 4, 15), // 60
new DungeonObject_t("& Awl-Pike",                      0x00000000L, TV_POLEARM,     '/', 0,    200,  1,   1, 160,  0,  0, 0,   0, 1, 8, 8), // 61
new DungeonObject_t("& Beaked Axe",                    0x00000000L, TV_POLEARM,     '/', 0,    408,  2,   1, 180,  0,  0, 0,   0, 2, 6, 15), // 62
new DungeonObject_t("& Fauchard",                      0x00000000L, TV_POLEARM,     '/', 0,    326,  3,   1, 170,  0,  0, 0,   0, 1, 10, 17), // 63
new DungeonObject_t("& Glaive",                        0x00000000L, TV_POLEARM,     '/', 0,    363,  4,   1, 190,  0,  0, 0,   0, 2, 6, 20), // 64
new DungeonObject_t("& Halberd",                       0x00000000L, TV_POLEARM,     '/', 0,    430,  5,   1, 190,  0,  0, 0,   0, 3, 4, 22), // 65
new DungeonObject_t("& Lucerne Hammer",                0x00000000L, TV_POLEARM,     '/', 0,    376,  6,   1, 120,  0,  0, 0,   0, 2, 5, 11), // 66
new DungeonObject_t("& Pike",                          0x00000000L, TV_POLEARM,     '/', 0,    358,  7,   1, 160,  0,  0, 0,   0, 2, 5, 15), // 67
new DungeonObject_t("& Spear",                         0x00000000L, TV_POLEARM,     '/', 0,    36,   8,   1, 50,   0,  0, 0,   0, 1, 6, 5), // 68
new DungeonObject_t("& Lance",                         0x00000000L, TV_POLEARM,     '/', 0,    230,  9,   1, 300,  0,  0, 0,   0, 2, 8, 10), // 69
new DungeonObject_t("& Javelin",                       0x00000000L, TV_POLEARM,     '/', 0,    18,   10,  1, 30,   0,  0, 0,   0, 1, 4, 4), // 70
new DungeonObject_t("& Battle Axe (Balestarius)",      0x00000000L, TV_POLEARM,     '/', 0,    500,  11,  1, 180,  0,  0, 0,   0, 2, 8, 30), // 71
new DungeonObject_t("& Battle Axe (European)",         0x00000000L, TV_POLEARM,     '/', 0,    334,  12,  1, 170,  0,  0, 0,   0, 3, 4, 13), // 72
new DungeonObject_t("& Broad Axe",                     0x00000000L, TV_POLEARM,     '/', 0,    304,  13,  1, 160,  0,  0, 0,   0, 2, 6, 17), // 73
new DungeonObject_t("& Short Bow",                     0x00000000L, TV_BOW,         '}', 2,    50,   1,   1, 30,   0,  0, 0,   0, 0, 0, 3), // 74
new DungeonObject_t("& Long Bow",                      0x00000000L, TV_BOW,         '}', 3,    120,  2,   1, 40,   0,  0, 0,   0, 0, 0, 10), // 75
new DungeonObject_t("& Composite Bow",                 0x00000000L, TV_BOW,         '}', 4,    240,  3,   1, 40,   0,  0, 0,   0, 0, 0, 40), // 76
new DungeonObject_t("& Light Crossbow",                0x00000000L, TV_BOW,         '}', 5,    140,  10,  1, 110,  0,  0, 0,   0, 0, 0, 15), // 77
new DungeonObject_t("& Heavy Crossbow",                0x00000000L, TV_BOW,         '}', 6,    300,  11,  1, 200,  0,  0, 0,   0, 1, 1, 30), // 78
new DungeonObject_t("& Sling",                         0x00000000L, TV_BOW,         '}', 1,    5,    20,  1, 5,    0,  0, 0,   0, 0, 0, 1), // 79
new DungeonObject_t("& Arrow~",                        0x00000000L, TV_ARROW,       '{', 0,    1,    193, 1, 2,    0,  0, 0,   0, 1, 4, 2), // 80
new DungeonObject_t("& Bolt~",                         0x00000000L, TV_BOLT,        '{', 0,    2,    193, 1, 3,    0,  0, 0,   0, 1, 5, 2), // 81
new DungeonObject_t("& Rounded Pebble~",               0x00000000L, TV_SLING_AMMO,  '{', 0,    1,    193, 1, 4,    0,  0, 0,   0, 1, 2, 0), // 82
new DungeonObject_t("& Iron Shot~",                    0x00000000L, TV_SLING_AMMO,  '{', 0,    2,    194, 1, 5,    0,  0, 0,   0, 1, 3, 3), // 83
new DungeonObject_t("& Iron Spike~",                   0x00000000L, TV_SPIKE,       '~', 0,    1,    193, 1, 10,   0,  0, 0,   0, 1, 1, 1), // 84
new DungeonObject_t("& Brass Lantern~",                0x00000000L, TV_LIGHT,       '~', 7500, 35,   1,   1, 50,   0,  0, 0,   0, 1, 1, 1), // 85
new DungeonObject_t("& Wooden Torch~",                 0x00000000L, TV_LIGHT,       '~', 4000, 2,    193, 1, 30,   0,  0, 0,   0, 1, 1, 1), // 86
new DungeonObject_t("& Orcish Pick",                   0x20000000L, TV_DIGGING,    '\\', 2,    500,  2,   1, 180,  0,  0, 0,   0, 1, 3, 20), // 87
new DungeonObject_t("& Dwarven Pick",                  0x20000000L, TV_DIGGING,    '\\', 3,    1200, 3,   1, 200,  0,  0, 0,   0, 1, 4, 50), // 88
new DungeonObject_t("& Gnomish Shovel",                0x20000000L, TV_DIGGING,    '\\', 1,    100,  5,   1, 50,   0,  0, 0,   0, 1, 2, 20), // 89
new DungeonObject_t("& Dwarven Shovel",                0x20000000L, TV_DIGGING,    '\\', 2,    250,  6,   1, 120,  0,  0, 0,   0, 1, 3, 40), // 90
new DungeonObject_t("& Pair of Soft Leather Shoes",    0x00000000L, TV_BOOTS,       ']', 0,    4,    1,   1, 5,    0,  0, 1,   0, 0, 0, 1), // 91
new DungeonObject_t("& Pair of Soft Leather Boots",    0x00000000L, TV_BOOTS,       ']', 0,    7,    2,   1, 20,   0,  0, 2,   0, 1, 1, 4), // 92
new DungeonObject_t("& Pair of Hard Leather Boots",    0x00000000L, TV_BOOTS,       ']', 0,    12,   3,   1, 40,   0,  0, 3,   0, 1, 1, 6), // 93
new DungeonObject_t("& Soft Leather Cap",              0x00000000L, TV_HELM,        ']', 0,    4,    1,   1, 10,   0,  0, 1,   0, 0, 0, 2), // 94
new DungeonObject_t("& Hard Leather Cap",              0x00000000L, TV_HELM,        ']', 0,    12,   2,   1, 15,   0,  0, 2,   0, 0, 0, 4), // 95
new DungeonObject_t("& Metal Cap",                     0x00000000L, TV_HELM,        ']', 0,    30,   3,   1, 20,   0,  0, 3,   0, 1, 1, 7), // 96
new DungeonObject_t("& Iron Helm",                     0x00000000L, TV_HELM,        ']', 0,    75,   4,   1, 75,   0,  0, 5,   0, 1, 3, 20), // 97
new DungeonObject_t("& Steel Helm",                    0x00000000L, TV_HELM,        ']', 0,    200,  5,   1, 60,   0,  0, 6,   0, 1, 3, 40), // 98
new DungeonObject_t("& Silver Crown",                  0x00000000L, TV_HELM,        ']', 0,    500,  6,   1, 20,   0,  0, 0,   0, 1, 1, 44), // 99
new DungeonObject_t("& Golden Crown",                  0x00000000L, TV_HELM,        ']', 0,    1000, 7,   1, 30,   0,  0, 0,   0, 1, 2, 47), // 100
new DungeonObject_t("& Jewel-Encrusted Crown",         0x00000000L, TV_HELM,        ']', 0,    2000, 8,   1, 40,   0,  0, 0,   0, 1, 3, 50), // 101
new DungeonObject_t("& Robe",                          0x00000000L, TV_SOFT_ARMOR,  '(', 0,    4,    1,   1, 20,   0,  0, 2,   0, 0, 0, 1), // 102
new DungeonObject_t("Soft Leather Armor",              0x00000000L, TV_SOFT_ARMOR,  '(', 0,    18,   2,   1, 80,   0,  0, 4,   0, 0, 0, 2), // 103
new DungeonObject_t("Soft Studded Leather",            0x00000000L, TV_SOFT_ARMOR,  '(', 0,    35,   3,   1, 90,   0,  0, 5,   0, 1, 1, 3), // 104
new DungeonObject_t("Hard Leather Armor",              0x00000000L, TV_SOFT_ARMOR,  '(', 0,    55,   4,   1, 100, -1,  0, 6,   0, 1, 1, 5), // 105
new DungeonObject_t("Hard Studded Leather",            0x00000000L, TV_SOFT_ARMOR,  '(', 0,    100,  5,   1, 110, -1,  0, 7,   0, 1, 2, 7), // 106
new DungeonObject_t("Woven Cord Armor",                0x00000000L, TV_SOFT_ARMOR,  '(', 0,    45,   6,   1, 150, -1,  0, 6,   0, 0, 0, 7), // 107
new DungeonObject_t("Soft Leather Ring Mail",          0x00000000L, TV_SOFT_ARMOR,  '(', 0,    160,  7,   1, 130, -1,  0, 6,   0, 1, 2, 10), // 108
new DungeonObject_t("Hard Leather Ring Mail",          0x00000000L, TV_SOFT_ARMOR,  '(', 0,    230,  8,   1, 150, -2,  0, 8,   0, 1, 3, 12), // 109
new DungeonObject_t("Leather Scale Mail",              0x00000000L, TV_SOFT_ARMOR,  '(', 0,    330,  9,   1, 140, -1,  0, 11,  0, 1, 1, 14), // 110
new DungeonObject_t("Metal Scale Mail",                0x00000000L, TV_HARD_ARMOR,  '[', 0,    430,  1,   1, 250, -2,  0, 13,  0, 1, 4, 24), // 111
new DungeonObject_t("Chain Mail",                      0x00000000L, TV_HARD_ARMOR,  '[', 0,    530,  2,   1, 220, -2,  0, 14,  0, 1, 4, 26), // 112
new DungeonObject_t("Rusty Chain Mail",                0x00000000L, TV_HARD_ARMOR,  '[', 0,    0,    3,   1, 220, -5,  0, 14, -8, 1, 4, 26), // 113
new DungeonObject_t("Double Chain Mail",               0x00000000L, TV_HARD_ARMOR,  '[', 0,    630,  4,   1, 260, -2,  0, 15,  0, 1, 4, 28), // 114
new DungeonObject_t("Augmented Chain Mail",            0x00000000L, TV_HARD_ARMOR,  '[', 0,    675,  5,   1, 270, -2,  0, 16,  0, 1, 4, 30), // 115
new DungeonObject_t("Bar Chain Mail",                  0x00000000L, TV_HARD_ARMOR,  '[', 0,    720,  6,   1, 280, -2,  0, 18,  0, 1, 4, 34), // 116
new DungeonObject_t("Metal Brigandine Armor",          0x00000000L, TV_HARD_ARMOR,  '[', 0,    775,  7,   1, 290, -3,  0, 19,  0, 1, 4, 36), // 117
new DungeonObject_t("Laminated Armor",                 0x00000000L, TV_HARD_ARMOR,  '[', 0,    825,  8,   1, 300, -3,  0, 20,  0, 1, 4, 38), // 118
new DungeonObject_t("Partial Plate Armor",             0x00000000L, TV_HARD_ARMOR,  '[', 0,    900,  9,   1, 320, -3,  0, 22,  0, 1, 6, 42), // 119
new DungeonObject_t("Metal Lamellar Armor",            0x00000000L, TV_HARD_ARMOR,  '[', 0,    950,  10,  1, 340, -3,  0, 23,  0, 1, 6, 44), // 120
new DungeonObject_t("Full Plate Armor",                0x00000000L, TV_HARD_ARMOR,  '[', 0,    1050, 11,  1, 380, -3,  0, 25,  0, 2, 4, 48), // 121
new DungeonObject_t("Ribbed Plate Armor",              0x00000000L, TV_HARD_ARMOR,  '[', 0,    1200, 12,  1, 380, -3,  0, 28,  0, 2, 4, 50), // 122
new DungeonObject_t("& Cloak",                         0x00000000L, TV_CLOAK,       '(', 0,    3,    1,   1, 10,   0,  0, 1,   0, 0, 0, 1), // 123
new DungeonObject_t("& Set of Leather Gloves",         0x00000000L, TV_GLOVES,      ']', 0,    3,    1,   1, 5,    0,  0, 1,   0, 0, 0, 1), // 124
new DungeonObject_t("& Set of Gauntlets",              0x00000000L, TV_GLOVES,      ']', 0,    35,   2,   1, 25,   0,  0, 2,   0, 1, 1, 12), // 125
new DungeonObject_t("& Small Leather Shield",          0x00000000L, TV_SHIELD,      ')', 0,    30,   1,   1, 50,   0,  0, 2,   0, 1, 1, 3), // 126
new DungeonObject_t("& Medium Leather Shield",         0x00000000L, TV_SHIELD,      ')', 0,    60,   2,   1, 75,   0,  0, 3,   0, 1, 2, 8), // 127
new DungeonObject_t("& Large Leather Shield",          0x00000000L, TV_SHIELD,      ')', 0,    120,  3,   1, 100,  0,  0, 4,   0, 1, 2, 15), // 128
new DungeonObject_t("& Small Metal Shield",            0x00000000L, TV_SHIELD,      ')', 0,    50,   4,   1, 65,   0,  0, 3,   0, 1, 2, 10), // 129
new DungeonObject_t("& Medium Metal Shield",           0x00000000L, TV_SHIELD,      ')', 0,    125,  5,   1, 90,   0,  0, 4,   0, 1, 3, 20), // 130
new DungeonObject_t("& Large Metal Shield",            0x00000000L, TV_SHIELD,      ')', 0,    200,  6,   1, 120,  0,  0, 5,   0, 1, 3, 30), // 131
new DungeonObject_t("Strength",                        0x00000001L, TV_RING,        '=', 0,    400,  0,   1, 2,    0,  0, 0,   0, 0, 0, 30), // 132
new DungeonObject_t("Dexterity",                       0x00000008L, TV_RING,        '=', 0,    400,  1,   1, 2,    0,  0, 0,   0, 0, 0, 30), // 133
new DungeonObject_t("Constitution",                    0x00000010L, TV_RING,        '=', 0,    400,  2,   1, 2,    0,  0, 0,   0, 0, 0, 30), // 134
new DungeonObject_t("Intelligence",                    0x00000002L, TV_RING,        '=', 0,    400,  3,   1, 2,    0,  0, 0,   0, 0, 0, 30), // 135
new DungeonObject_t("Speed",                           0x00001000L, TV_RING,        '=', 0,    3000, 4,   1, 2,    0,  0, 0,   0, 0, 0, 50), // 136
new DungeonObject_t("Searching",                       0x00000040L, TV_RING,        '=', 0,    250,  5,   1, 2,    0,  0, 0,   0, 0, 0, 7), // 137
new DungeonObject_t("Teleportation",                   0x80000400L, TV_RING,        '=', 0,    0,    6,   1, 2,    0,  0, 0,   0, 0, 0, 7), // 138
new DungeonObject_t("Slow Digestion",                  0x00000080L, TV_RING,        '=', 0,    200,  7,   1, 2,    0,  0, 0,   0, 0, 0, 7), // 139
new DungeonObject_t("Resist Fire",                     0x00080000L, TV_RING,        '=', 0,    250,  8,   1, 2,    0,  0, 0,   0, 0, 0, 14), // 140
new DungeonObject_t("Resist Cold",                     0x00200000L, TV_RING,        '=', 0,    250,  9,   1, 2,    0,  0, 0,   0, 0, 0, 14), // 141
new DungeonObject_t("Feather Falling",                 0x04000000L, TV_RING,        '=', 0,    200,  10,  1, 2,    0,  0, 0,   0, 0, 0, 7), // 142
new DungeonObject_t("Adornment",                       0x00000000L, TV_RING,        '=', 0,    20,   11,  1, 2,    0,  0, 0,   0, 0, 0, 7), // 143
    // was a ring of adornment, sub_category_id = 12 here
new DungeonObject_t("& Arrow~",                        0x00000000L, TV_ARROW,        '{',  0, 1,    193, 1, 2, 0, 0, 0,  0, 1, 4, 15), // 144
new DungeonObject_t("Weakness",                        0x80000001L, TV_RING,         '=', -5, 0,    13,  1, 2, 0, 0, 0,  0, 0, 0, 7), // 145
new DungeonObject_t("Lordly Protection (FIRE)",        0x00080000L, TV_RING,         '=',  0, 1200, 14,  1, 2, 0, 0, 0,  5, 0, 0, 50), // 146
new DungeonObject_t("Lordly Protection (ACID)",        0x00100000L, TV_RING,         '=',  0, 1200, 15,  1, 2, 0, 0, 0,  5, 0, 0, 50), // 147
new DungeonObject_t("Lordly Protection (COLD)",        0x00200000L, TV_RING,         '=',  0, 1200, 16,  1, 2, 0, 0, 0,  5, 0, 0, 50), // 148
new DungeonObject_t("WOE",                             0x80000644L, TV_RING,         '=', -5, 0,    17,  1, 2, 0, 0, 0, -3, 0, 0, 50), // 149
new DungeonObject_t("Stupidity",                       0x80000002L, TV_RING,         '=', -5, 0,    18,  1, 2, 0, 0, 0,  0, 0, 0, 7), // 150
new DungeonObject_t("Increase Damage",                 0x00000000L, TV_RING,         '=',  0, 100,  19,  1, 2, 0, 0, 0,  0, 0, 0, 20), // 151
new DungeonObject_t("Increase To-Hit",                 0x00000000L, TV_RING,         '=',  0, 100,  20,  1, 2, 0, 0, 0,  0, 0, 0, 20), // 152
new DungeonObject_t("Protection",                      0x00000000L, TV_RING,         '=',  0, 100,  21,  1, 2, 0, 0, 0,  0, 0, 0, 7), // 153
new DungeonObject_t("Aggravate Monster",               0x80000200L, TV_RING,         '=',  0, 0,    22,  1, 2, 0, 0, 0,  0, 0, 0, 7), // 154
new DungeonObject_t("See Invisible",                   0x01000000L, TV_RING,         '=',  0, 500,  23,  1, 2, 0, 0, 0,  0, 0, 0, 40), // 155
new DungeonObject_t("Sustain Strength",                0x00400000L, TV_RING,         '=',  1, 750,  24,  1, 2, 0, 0, 0,  0, 0, 0, 44), // 156
new DungeonObject_t("Sustain Intelligence",            0x00400000L, TV_RING,         '=',  2, 600,  25,  1, 2, 0, 0, 0,  0, 0, 0, 44), // 157
new DungeonObject_t("Sustain Wisdom",                  0x00400000L, TV_RING,         '=',  3, 600,  26,  1, 2, 0, 0, 0,  0, 0, 0, 44), // 158
new DungeonObject_t("Sustain Constitution",            0x00400000L, TV_RING,         '=',  4, 750,  27,  1, 2, 0, 0, 0,  0, 0, 0, 44), // 159
new DungeonObject_t("Sustain Dexterity",               0x00400000L, TV_RING,         '=',  5, 750,  28,  1, 2, 0, 0, 0,  0, 0, 0, 44), // 160
new DungeonObject_t("Sustain Charisma",                0x00400000L, TV_RING,         '=',  6, 500,  29,  1, 2, 0, 0, 0,  0, 0, 0, 44), // 161
new DungeonObject_t("Slaying",                         0x00000000L, TV_RING,         '=',  0, 1000, 30,  1, 2, 0, 0, 0,  0, 0, 0, 50), // 162
new DungeonObject_t("Wisdom",                          0x00000004L, TV_AMULET,       '"',  0, 300,  0,   1, 3, 0, 0, 0,  0, 0, 0, 20), // 163
new DungeonObject_t("Charisma",                        0x00000020L, TV_AMULET,       '"',  0, 250,  1,   1, 3, 0, 0, 0,  0, 0, 0, 20), // 164
new DungeonObject_t("Searching",                       0x00000040L, TV_AMULET,       '"',  0, 250,  2,   1, 3, 0, 0, 0,  0, 0, 0, 14), // 165
new DungeonObject_t("Teleportation",                   0x80000400L, TV_AMULET,       '"',  0, 0,    3,   1, 3, 0, 0, 0,  0, 0, 0, 14), // 166
new DungeonObject_t("Slow Digestion",                  0x00000080L, TV_AMULET,       '"',  0, 200,  4,   1, 3, 0, 0, 0,  0, 0, 0, 14), // 167
new DungeonObject_t("Resist Acid",                     0x00100000L, TV_AMULET,       '"',  0, 250,  5,   1, 3, 0, 0, 0,  0, 0, 0, 24), // 168
new DungeonObject_t("Adornment",                       0x00000000L, TV_AMULET,       '"',  0, 20,   6,   1, 3, 0, 0, 0,  0, 0, 0, 16), // 169
    // was an amulet of adornment here, sub_category_id = 7
new DungeonObject_t("& Bolt~",                         0x00000000L, TV_BOLT,         '{',  0, 2,    193, 1, 3, 0, 0, 0, 0, 1, 5, 25), // 170
new DungeonObject_t("the Magi",                        0x01800040L, TV_AMULET,       '"',  0, 5000, 8,   1, 3, 0, 0, 0, 3, 0, 0, 50), // 171
new DungeonObject_t("DOOM",                            0x8000007FL, TV_AMULET,       '"', -5, 0,    9,   1, 3, 0, 0, 0, 0, 0, 0, 50), // 172
new DungeonObject_t("Enchant Weapon To-Hit",           0x00000001L, TV_SCROLL1,      '?',  0, 125,  64,  1, 5, 0, 0, 0, 0, 0, 0, 12), // 173
new DungeonObject_t("Enchant Weapon To-Dam",           0x00000002L, TV_SCROLL1,      '?',  0, 125,  65,  1, 5, 0, 0, 0, 0, 0, 0, 12), // 174
new DungeonObject_t("Enchant Armor",                   0x00000004L, TV_SCROLL1,      '?',  0, 125,  66,  1, 5, 0, 0, 0, 0, 0, 0, 12), // 175
new DungeonObject_t("Identify",                        0x00000008L, TV_SCROLL1,      '?',  0, 50,   67,  1, 5, 0, 0, 0, 0, 0, 0, 1), // 176
new DungeonObject_t("Identify",                        0x00000008L, TV_SCROLL1,      '?',  0, 50,   67,  1, 5, 0, 0, 0, 0, 0, 0, 5), // 177
new DungeonObject_t("Identify",                        0x00000008L, TV_SCROLL1,      '?',  0, 50,   67,  1, 5, 0, 0, 0, 0, 0, 0, 10), // 178
new DungeonObject_t("Identify",                        0x00000008L, TV_SCROLL1,      '?',  0, 50,   67,  1, 5, 0, 0, 0, 0, 0, 0, 30), // 179
new DungeonObject_t("Remove Curse",                    0x00000010L, TV_SCROLL1,      '?',  0, 100,  68,  1, 5, 0, 0, 0, 0, 0, 0, 7), // 180
new DungeonObject_t("Light",                           0x00000020L, TV_SCROLL1,      '?',  0, 15,   69,  1, 5, 0, 0, 0, 0, 0, 0, 0), // 181
new DungeonObject_t("Light",                           0x00000020L, TV_SCROLL1,      '?',  0, 15,   69,  1, 5, 0, 0, 0, 0, 0, 0, 3), // 182
new DungeonObject_t("Light",                           0x00000020L, TV_SCROLL1,      '?',  0, 15,   69,  1, 5, 0, 0, 0, 0, 0, 0, 7), // 183
new DungeonObject_t("Summon Monster",                  0x00000040L, TV_SCROLL1,      '?',  0, 0,    70,  1, 5, 0, 0, 0, 0, 0, 0, 1), // 184
new DungeonObject_t("Phase Door",                      0x00000080L, TV_SCROLL1,      '?',  0, 15,   71,  1, 5, 0, 0, 0, 0, 0, 0, 1), // 185
new DungeonObject_t("Teleport",                        0x00000100L, TV_SCROLL1,      '?',  0, 40,   72,  1, 5, 0, 0, 0, 0, 0, 0, 10), // 186
new DungeonObject_t("Teleport Level",                  0x00000200L, TV_SCROLL1,      '?',  0, 50,   73,  1, 5, 0, 0, 0, 0, 0, 0, 20), // 187
new DungeonObject_t("Monster Confusion",               0x00000400L, TV_SCROLL1,      '?',  0, 30,   74,  1, 5, 0, 0, 0, 0, 0, 0, 5), // 188
new DungeonObject_t("Magic Mapping",                   0x00000800L, TV_SCROLL1,      '?',  0, 40,   75,  1, 5, 0, 0, 0, 0, 0, 0, 5), // 189
new DungeonObject_t("Sleep Monster",                   0x00001000L, TV_SCROLL1,      '?',  0, 35,   76,  1, 5, 0, 0, 0, 0, 0, 0, 5), // 190
new DungeonObject_t("Rune of Protection",              0x00002000L, TV_SCROLL1,      '?',  0, 500,  77,  1, 5, 0, 0, 0, 0, 0, 0, 50), // 191
new DungeonObject_t("Treasure Detection",              0x00004000L, TV_SCROLL1,      '?',  0, 15,   78,  1, 5, 0, 0, 0, 0, 0, 0, 0), // 192
new DungeonObject_t("Object Detection",                0x00008000L, TV_SCROLL1,      '?',  0, 15,   79,  1, 5, 0, 0, 0, 0, 0, 0, 0), // 193
new DungeonObject_t("Trap Detection",                  0x00010000L, TV_SCROLL1,      '?',  0, 35,   80,  1, 5, 0, 0, 0, 0, 0, 0, 5), // 194
new DungeonObject_t("Trap Detection",                  0x00010000L, TV_SCROLL1,      '?',  0, 35,   80,  1, 5, 0, 0, 0, 0, 0, 0, 8), // 195
new DungeonObject_t("Trap Detection",                  0x00010000L, TV_SCROLL1,      '?',  0, 35,   80,  1, 5, 0, 0, 0, 0, 0, 0, 12), // 196
new DungeonObject_t("Door/Stair Location",             0x00020000L, TV_SCROLL1,      '?',  0, 35,   81,  1, 5, 0, 0, 0, 0, 0, 0, 5), // 197
new DungeonObject_t("Door/Stair Location",             0x00020000L, TV_SCROLL1,      '?',  0, 35,   81,  1, 5, 0, 0, 0, 0, 0, 0, 10), // 198
new DungeonObject_t("Door/Stair Location",             0x00020000L, TV_SCROLL1,      '?',  0, 35,   81,  1, 5, 0, 0, 0, 0, 0, 0, 15), // 199
new DungeonObject_t("Mass Genocide",                   0x00040000L, TV_SCROLL1,      '?',  0, 1000, 82,  1, 5, 0, 0, 0, 0, 0, 0, 50), // 200
new DungeonObject_t("Detect Invisible",                0x00080000L, TV_SCROLL1,      '?',  0, 15,   83,  1, 5, 0, 0, 0, 0, 0, 0, 1), // 201
new DungeonObject_t("Aggravate Monster",               0x00100000L, TV_SCROLL1,      '?',  0, 0,    84,  1, 5, 0, 0, 0, 0, 0, 0, 5), // 202
new DungeonObject_t("Trap Creation",                   0x00200000L, TV_SCROLL1,      '?',  0, 0,    85,  1, 5, 0, 0, 0, 0, 0, 0, 12), // 203
new DungeonObject_t("Trap/Door Destruction",           0x00400000L, TV_SCROLL1,      '?',  0, 50,   86,  1, 5, 0, 0, 0, 0, 0, 0, 12), // 204
new DungeonObject_t("Door Creation",                   0x00800000L, TV_SCROLL1,      '?',  0, 100,  87,  1, 5, 0, 0, 0, 0, 0, 0, 12), // 205
new DungeonObject_t("Recharging",                      0x01000000L, TV_SCROLL1,      '?',  0, 200,  88,  1, 5, 0, 0, 0, 0, 0, 0, 40), // 206
new DungeonObject_t("Genocide",                        0x02000000L, TV_SCROLL1,      '?',  0, 750,  89,  1, 5, 0, 0, 0, 0, 0, 0, 35), // 207
new DungeonObject_t("Darkness",                        0x04000000L, TV_SCROLL1,      '?',  0, 0,    90,  1, 5, 0, 0, 0, 0, 0, 0, 1), // 208
new DungeonObject_t("Protection from Evil",            0x08000000L, TV_SCROLL1,      '?',  0, 100,  91,  1, 5, 0, 0, 0, 0, 0, 0, 30), // 209
new DungeonObject_t("Create Food",                     0x10000000L, TV_SCROLL1,      '?',  0, 10,   92,  1, 5, 0, 0, 0, 0, 0, 0, 5), // 210
new DungeonObject_t("Dispel Undead",                   0x20000000L, TV_SCROLL1,      '?',  0, 200,  93,  1, 5, 0, 0, 0, 0, 0, 0, 40), // 211
new DungeonObject_t("*Enchant Weapon*",                0x00000001L, TV_SCROLL2,      '?',  0, 500,  94,  1, 5, 0, 0, 0, 0, 0, 0, 50), // 212
new DungeonObject_t("Curse Weapon",                    0x00000002L, TV_SCROLL2,      '?',  0, 0,    95,  1, 5, 0, 0, 0, 0, 0, 0, 50), // 213
new DungeonObject_t("*Enchant Armor*",                 0x00000004L, TV_SCROLL2,      '?',  0, 500,  96,  1, 5, 0, 0, 0, 0, 0, 0, 50), // 214
new DungeonObject_t("Curse Armor",                     0x00000008L, TV_SCROLL2,      '?',  0, 0,    97,  1, 5, 0, 0, 0, 0, 0, 0, 50), // 215
new DungeonObject_t("Summon Undead",                   0x00000010L, TV_SCROLL2,      '?',  0, 0,    98,  1, 5, 0, 0, 0, 0, 0, 0, 15), // 216
new DungeonObject_t("Blessing",                        0x00000020L, TV_SCROLL2,      '?',  0, 15,   99,  1, 5, 0, 0, 0, 0, 0, 0, 1), // 217
new DungeonObject_t("Holy Chant",                      0x00000040L, TV_SCROLL2,      '?',  0, 40,   100, 1, 5, 0, 0, 0, 0, 0, 0, 12), // 218
new DungeonObject_t("Holy Prayer",                     0x00000080L, TV_SCROLL2,      '?',  0, 80,   101, 1, 5, 0, 0, 0, 0, 0, 0, 24), // 219
new DungeonObject_t("Word-of-Recall",                  0x00000100L, TV_SCROLL2,      '?',  0, 150,  102, 1, 5, 0, 0, 0, 0, 0, 0, 5), // 220
new DungeonObject_t("*Destruction*",                   0x00000200L, TV_SCROLL2,      '?',  0, 750,  103, 1, 5, 0, 0, 0, 0, 0, 0, 40), // 221
    // SMJ, AJ, Water must be sub_category_id 64-66 resp. for itemDescription to work
new DungeonObject_t("Slime Mold Juice",                0x30000000L, TV_POTION1,      '!', 400,  2,    64,  1, 4, 0, 0, 0, 0, 1, 1, 0), // 222
new DungeonObject_t("Apple Juice",                     0x00000000L, TV_POTION1,      '!', 250,  1,    65,  1, 4, 0, 0, 0, 0, 1, 1, 0), // 223
new DungeonObject_t("Water",                           0x00000000L, TV_POTION1,      '!', 200,  0,    66,  1, 4, 0, 0, 0, 0, 1, 1, 0), // 224
new DungeonObject_t("Strength",                        0x00000001L, TV_POTION1,      '!', 50,   300,  67,  1, 4, 0, 0, 0, 0, 1, 1, 25), // 225
new DungeonObject_t("Weakness",                        0x00000002L, TV_POTION1,      '!', 0,    0,    68,  1, 4, 0, 0, 0, 0, 1, 1, 3), // 226
new DungeonObject_t("Restore Strength",                0x00000004L, TV_POTION1,      '!', 0,    300,  69,  1, 4, 0, 0, 0, 0, 1, 1, 40), // 227
new DungeonObject_t("Intelligence",                    0x00000008L, TV_POTION1,      '!', 0,    300,  70,  1, 4, 0, 0, 0, 0, 1, 1, 25), // 228
new DungeonObject_t("Lose Intelligence",               0x00000010L, TV_POTION1,      '!', 0,    0,    71,  1, 4, 0, 0, 0, 0, 1, 1, 25), // 229
new DungeonObject_t("Restore Intelligence",            0x00000020L, TV_POTION1,      '!', 0,    300,  72,  1, 4, 0, 0, 0, 0, 1, 1, 40), // 230
new DungeonObject_t("Wisdom",                          0x00000040L, TV_POTION1,      '!', 0,    300,  73,  1, 4, 0, 0, 0, 0, 1, 1, 25), // 231
new DungeonObject_t("Lose Wisdom",                     0x00000080L, TV_POTION1,      '!', 0,    0,    74,  1, 4, 0, 0, 0, 0, 1, 1, 25), // 232
new DungeonObject_t("Restore Wisdom",                  0x00000100L, TV_POTION1,      '!', 0,    300,  75,  1, 4, 0, 0, 0, 0, 1, 1, 40), // 233
new DungeonObject_t("Charisma",                        0x00000200L, TV_POTION1,      '!', 0,    300,  76,  1, 4, 0, 0, 0, 0, 1, 1, 25), // 234
new DungeonObject_t("Ugliness",                        0x00000400L, TV_POTION1,      '!', 0,    0,    77,  1, 4, 0, 0, 0, 0, 1, 1, 25), // 235
new DungeonObject_t("Restore Charisma",                0x00000800L, TV_POTION1,      '!', 0,    300,  78,  1, 4, 0, 0, 0, 0, 1, 1, 40), // 236
new DungeonObject_t("Cure Light Wounds",               0x10001000L, TV_POTION1,      '!', 50,   15,   79,  1, 4, 0, 0, 0, 0, 1, 1, 0), // 237
new DungeonObject_t("Cure Light Wounds",               0x10001000L, TV_POTION1,      '!', 50,   15,   79,  1, 4, 0, 0, 0, 0, 1, 1, 1), // 238
new DungeonObject_t("Cure Light Wounds",               0x10001000L, TV_POTION1,      '!', 50,   15,   79,  1, 4, 0, 0, 0, 0, 1, 1, 2), // 239
new DungeonObject_t("Cure Serious Wounds",             0x30002000L, TV_POTION1,      '!', 100,  40,   80,  1, 4, 0, 0, 0, 0, 1, 1, 3), // 240
new DungeonObject_t("Cure Critical Wounds",            0x70004000L, TV_POTION1,      '!', 100,  100,  81,  1, 4, 0, 0, 0, 0, 1, 1, 5), // 241
new DungeonObject_t("Healing",                         0x70008000L, TV_POTION1,      '!', 200,  200,  82,  1, 4, 0, 0, 0, 0, 1, 1, 12), // 242
new DungeonObject_t("Constitution",                    0x00010000L, TV_POTION1,      '!', 50,   300,  83,  1, 4, 0, 0, 0, 0, 1, 1, 25), // 243
new DungeonObject_t("Gain Experience",                 0x00020000L, TV_POTION1,      '!', 0,    2500, 84,  1, 4, 0, 0, 0, 0, 1, 1, 50), // 244
new DungeonObject_t("Sleep",                           0x00040000L, TV_POTION1,      '!', 100,  0,    85,  1, 4, 0, 0, 0, 0, 1, 1, 0), // 245
new DungeonObject_t("Blindness",                       0x00080000L, TV_POTION1,      '!', 0,    0,    86,  1, 4, 0, 0, 0, 0, 1, 1, 0), // 246
new DungeonObject_t("Confusion",                       0x00100000L, TV_POTION1,      '!', 50,   0,    87,  1, 4, 0, 0, 0, 0, 1, 1, 0), // 247
new DungeonObject_t("Poison",                          0x00200000L, TV_POTION1,      '!', 0,    0,    88,  1, 4, 0, 0, 0, 0, 1, 1, 3), // 248
new DungeonObject_t("Haste Self",                      0x00400000L, TV_POTION1,      '!', 0,    75,   89,  1, 4, 0, 0, 0, 0, 1, 1, 1), // 249
new DungeonObject_t("Slowness",                        0x00800000L, TV_POTION1,      '!', 50,   0,    90,  1, 4, 0, 0, 0, 0, 1, 1, 1), // 250
new DungeonObject_t("Dexterity",                       0x02000000L, TV_POTION1,      '!', 0,    300,  91,  1, 4, 0, 0, 0, 0, 1, 1, 25), // 251
new DungeonObject_t("Restore Dexterity",               0x04000000L, TV_POTION1,      '!', 0,    300,  92,  1, 4, 0, 0, 0, 0, 1, 1, 40), // 252
new DungeonObject_t("Restore Constitution",            0x68000000L, TV_POTION1,      '!', 0,    300,  93,  1, 4, 0, 0, 0, 0, 1, 1, 40), // 253
new DungeonObject_t("Lose Experience",                 0x00000002L, TV_POTION2,      '!', 0,    0,    95,  1, 4, 0, 0, 0, 0, 1, 1, 10), // 254
new DungeonObject_t("Salt Water",                      0x00000004L, TV_POTION2,      '!', 0,    0,    96,  1, 4, 0, 0, 0, 0, 1, 1, 0), // 255
new DungeonObject_t("Invulnerability",                 0x00000008L, TV_POTION2,      '!', 0,    1000, 97,  1, 4, 0, 0, 0, 0, 1, 1, 40), // 256
new DungeonObject_t("Heroism",                         0x00000010L, TV_POTION2,      '!', 0,    35,   98,  1, 4, 0, 0, 0, 0, 1, 1, 1), // 257
new DungeonObject_t("Super Heroism",                   0x00000020L, TV_POTION2,      '!', 0,    100,  99,  1, 4, 0, 0, 0, 0, 1, 1, 3), // 258
new DungeonObject_t("Boldness",                        0x00000040L, TV_POTION2,      '!', 0,    10,   100, 1, 4, 0, 0, 0, 0, 1, 1, 1), // 259
new DungeonObject_t("Restore Life Levels",             0x00000080L, TV_POTION2,      '!', 0,    400,  101, 1, 4, 0, 0, 0, 0, 1, 1, 40), // 260
new DungeonObject_t("Resist Heat",                     0x00000100L, TV_POTION2,      '!', 0,    30,   102, 1, 4, 0, 0, 0, 0, 1, 1, 1), // 261
new DungeonObject_t("Resist Cold",                     0x00000200L, TV_POTION2,      '!', 0,    30,   103, 1, 4, 0, 0, 0, 0, 1, 1, 1), // 262
new DungeonObject_t("Detect Invisible",                0x00000400L, TV_POTION2,      '!', 0,    50,   104, 1, 4, 0, 0, 0, 0, 1, 1, 3), // 263
new DungeonObject_t("Slow Poison",                     0x00000800L, TV_POTION2,      '!', 0,    25,   105, 1, 4, 0, 0, 0, 0, 1, 1, 1), // 264
new DungeonObject_t("Neutralize Poison",               0x00001000L, TV_POTION2,      '!', 0,    75,   106, 1, 4, 0, 0, 0, 0, 1, 1, 5), // 265
new DungeonObject_t("Restore Mana",                    0x00002000L, TV_POTION2,      '!', 0,    350,  107, 1, 4, 0, 0, 0, 0, 1, 1, 25), // 266
new DungeonObject_t("Infra-Vision",                    0x00004000L, TV_POTION2,      '!', 0,    20,   108, 1, 4, 0, 0, 0, 0, 1, 1, 3), // 267
new DungeonObject_t("& Flask~ of Oil",                 0x00040000L, TV_FLASK,        '!', 7500, 3,    64,  1, 10, 0, 0, 0, 0, 2, 6, 1), // 268
new DungeonObject_t("Light",                           0x00000001L, TV_WAND,         '-', 0,    200,  0,   1, 10, 0, 0, 0, 0, 1, 1, 2), // 269
new DungeonObject_t("Lightning Bolts",                 0x00000002L, TV_WAND,         '-', 0,    600,  1,   1, 10, 0, 0, 0, 0, 1, 1, 15), // 270
new DungeonObject_t("Frost Bolts",                     0x00000004L, TV_WAND,         '-', 0,    800,  2,   1, 10, 0, 0, 0, 0, 1, 1, 20), // 271
new DungeonObject_t("Fire Bolts",                      0x00000008L, TV_WAND,         '-', 0,    1000, 3,   1, 10, 0, 0, 0, 0, 1, 1, 30), // 272
new DungeonObject_t("Stone-to-Mud",                    0x00000010L, TV_WAND,         '-', 0,    300,  4,   1, 10, 0, 0, 0, 0, 1, 1, 12), // 273
new DungeonObject_t("Polymorph",                       0x00000020L, TV_WAND,         '-', 0,    400,  5,   1, 10, 0, 0, 0, 0, 1, 1, 20), // 274
new DungeonObject_t("Heal Monster",                    0x00000040L, TV_WAND,         '-', 0,    0,    6,   1, 10, 0, 0, 0, 0, 1, 1, 2), // 275
new DungeonObject_t("Haste Monster",                   0x00000080L, TV_WAND,         '-', 0,    0,    7,   1, 10, 0, 0, 0, 0, 1, 1, 2), // 276
new DungeonObject_t("Slow Monster",                    0x00000100L, TV_WAND,         '-', 0,    500,  8,   1, 10, 0, 0, 0, 0, 1, 1, 2), // 277
new DungeonObject_t("Confuse Monster",                 0x00000200L, TV_WAND,         '-', 0,    400,  9,   1, 10, 0, 0, 0, 0, 1, 1, 2), // 278
new DungeonObject_t("Sleep Monster",                   0x00000400L, TV_WAND,         '-', 0,    500,  10,  1, 10, 0, 0, 0, 0, 1, 1, 7), // 279
new DungeonObject_t("Drain Life",                      0x00000800L, TV_WAND,         '-', 0,    1200, 11,  1, 10, 0, 0, 0, 0, 1, 1, 50), // 280
new DungeonObject_t("Trap/Door Destruction",           0x00001000L, TV_WAND,         '-', 0,    500,  12,  1, 10, 0, 0, 0, 0, 1, 1, 12), // 281
new DungeonObject_t("Magic Missile",                   0x00002000L, TV_WAND,         '-', 0,    200,  13,  1, 10, 0, 0, 0, 0, 1, 1, 2), // 282
new DungeonObject_t("Wall Building",                   0x00004000L, TV_WAND,         '-', 0,    400,  14,  1, 10, 0, 0, 0, 0, 1, 1, 25), // 283
new DungeonObject_t("Clone Monster",                   0x00008000L, TV_WAND,         '-', 0,    0,    15,  1, 10, 0, 0, 0, 0, 1, 1, 15), // 284
new DungeonObject_t("Teleport Away",                   0x00010000L, TV_WAND,         '-', 0,    350,  16,  1, 10, 0, 0, 0, 0, 1, 1, 20), // 285
new DungeonObject_t("Disarming",                       0x00020000L, TV_WAND,         '-', 0,    500,  17,  1, 10, 0, 0, 0, 0, 1, 1, 20), // 286
new DungeonObject_t("Lightning Balls",                 0x00040000L, TV_WAND,         '-', 0,    1200, 18,  1, 10, 0, 0, 0, 0, 1, 1, 35), // 287
new DungeonObject_t("Cold Balls",                      0x00080000L, TV_WAND,         '-', 0,    1500, 19,  1, 10, 0, 0, 0, 0, 1, 1, 40), // 288
new DungeonObject_t("Fire Balls",                      0x00100000L, TV_WAND,         '-', 0,    1800, 20,  1, 10, 0, 0, 0, 0, 1, 1, 50), // 289
new DungeonObject_t("Stinking Cloud",                  0x00200000L, TV_WAND,         '-', 0,    400,  21,  1, 10, 0, 0, 0, 0, 1, 1, 5), // 290
new DungeonObject_t("Acid Balls",                      0x00400000L, TV_WAND,         '-', 0,    1650, 22,  1, 10, 0, 0, 0, 0, 1, 1, 48), // 291
new DungeonObject_t("Wonder",                          0x00800000L, TV_WAND,         '-', 0,    250,  23,  1, 10, 0, 0, 0, 0, 1, 1, 2), // 292
new DungeonObject_t("Light",                           0x00000001L, TV_STAFF,        '_', 0,    250,  0,   1, 50, 0, 0, 0, 0, 1, 2, 5), // 293
new DungeonObject_t("Door/Stair Location",             0x00000002L, TV_STAFF,        '_', 0,    350,  1,   1, 50, 0, 0, 0, 0, 1, 2, 10), // 294
new DungeonObject_t("Trap Location",                   0x00000004L, TV_STAFF,        '_', 0,    350,  2,   1, 50, 0, 0, 0, 0, 1, 2, 10), // 295
new DungeonObject_t("Treasure Location",               0x00000008L, TV_STAFF,        '_', 0,    200,  3,   1, 50, 0, 0, 0, 0, 1, 2, 5), // 296
new DungeonObject_t("Object Location",                 0x00000010L, TV_STAFF,        '_', 0,    200,  4,   1, 50, 0, 0, 0, 0, 1, 2, 5), // 297
new DungeonObject_t("Teleportation",                   0x00000020L, TV_STAFF,        '_', 0,    800,  5,   1, 50, 0, 0, 0, 0, 1, 2, 20), // 298
new DungeonObject_t("Earthquakes",                     0x00000040L, TV_STAFF,        '_', 0,    350,  6,   1, 50, 0, 0, 0, 0, 1, 2, 40), // 299
new DungeonObject_t("Summoning",                       0x00000080L, TV_STAFF,        '_', 0,    0,    7,   1, 50, 0, 0, 0, 0, 1, 2, 10), // 300
new DungeonObject_t("Summoning",                       0x00000080L, TV_STAFF,        '_', 0,    0,    7,   1, 50, 0, 0, 0, 0, 1, 2, 50), // 301
new DungeonObject_t("*Destruction*",                   0x00000200L, TV_STAFF,        '_', 0,    2500, 8,   1, 50, 0, 0, 0, 0, 1, 2, 50), // 302
new DungeonObject_t("Starlight",                       0x00000400L, TV_STAFF,        '_', 0,    400,  9,   1, 50, 0, 0, 0, 0, 1, 2, 20), // 303
new DungeonObject_t("Haste Monsters",                  0x00000800L, TV_STAFF,        '_', 0,    0,    10,  1, 50, 0, 0, 0, 0, 1, 2, 10), // 304
new DungeonObject_t("Slow Monsters",                   0x00001000L, TV_STAFF,        '_', 0,    800,  11,  1, 50, 0, 0, 0, 0, 1, 2, 10), // 305
new DungeonObject_t("Sleep Monsters",                  0x00002000L, TV_STAFF,        '_', 0,    700,  12,  1, 50, 0, 0, 0, 0, 1, 2, 10), // 306
new DungeonObject_t("Cure Light Wounds",               0x00004000L, TV_STAFF,        '_', 0,    200,  13,  1, 50, 0, 0, 0, 0, 1, 2, 5), // 307
new DungeonObject_t("Detect Invisible",                0x00008000L, TV_STAFF,        '_', 0,    200,  14,  1, 50, 0, 0, 0, 0, 1, 2, 5), // 308
new DungeonObject_t("Speed",                           0x00010000L, TV_STAFF,        '_', 0,    1000, 15,  1, 50, 0, 0, 0, 0, 1, 2, 40), // 309
new DungeonObject_t("Slowness",                        0x00020000L, TV_STAFF,        '_', 0,    0,    16,  1, 50, 0, 0, 0, 0, 1, 2, 40), // 310
new DungeonObject_t("Mass Polymorph",                  0x00040000L, TV_STAFF,        '_', 0,    750,  17,  1, 50, 0, 0, 0, 0, 1, 2, 46), // 311
new DungeonObject_t("Remove Curse",                    0x00080000L, TV_STAFF,        '_', 0,    500,  18,  1, 50, 0, 0, 0, 0, 1, 2, 47), // 312
new DungeonObject_t("Detect Evil",                     0x00100000L, TV_STAFF,        '_', 0,    350,  19,  1, 50, 0, 0, 0, 0, 1, 2, 20), // 313
new DungeonObject_t("Curing",                          0x00200000L, TV_STAFF,        '_', 0,    1000, 20,  1, 50, 0, 0, 0, 0, 1, 2, 25), // 314
new DungeonObject_t("Dispel Evil",                     0x00400000L, TV_STAFF,        '_', 0,    1200, 21,  1, 50, 0, 0, 0, 0, 1, 2, 49), // 315
new DungeonObject_t("Darkness",                        0x01000000L, TV_STAFF,        '_', 0,    0,    22,  1, 50, 0, 0, 0, 0, 1, 2, 50), // 316
new DungeonObject_t("Darkness",                        0x01000000L, TV_STAFF,        '_', 0,    0,    22,  1, 50, 0, 0, 0, 0, 1, 2, 5), // 317
new DungeonObject_t("[Beginners-Magick]",              0x0000007FL, TV_MAGIC_BOOK,   '?', 0,    25,   64,  1, 30, 0, 0, 0, 0, 1, 1, 40), // 318
new DungeonObject_t("[Magick I]",                      0x0000FF80L, TV_MAGIC_BOOK,   '?', 0,    100,  65,  1, 30, 0, 0, 0, 0, 1, 1, 40), // 319
new DungeonObject_t("[Magick II]",                     0x00FF0000L, TV_MAGIC_BOOK,   '?', 0,    400,  66,  1, 30, 0, 0, 0, 0, 1, 1, 40), // 320
new DungeonObject_t("[The Mages' Guide to Power]",     0x7F000000L, TV_MAGIC_BOOK,   '?', 0,    800,  67,  1, 30, 0, 0, 0, 0, 1, 1, 40), // 321
new DungeonObject_t("[Beginners Handbook]",            0x000000FFL, TV_PRAYER_BOOK,  '?', 0,    25,   64,  1, 30, 0, 0, 0, 0, 1, 1, 40), // 322
new DungeonObject_t("[Words of Wisdom]",               0x0000FF00L, TV_PRAYER_BOOK,  '?', 0,    100,  65,  1, 30, 0, 0, 0, 0, 1, 1, 40), // 323
new DungeonObject_t("[Chants and Blessings]",          0x01FF0000L, TV_PRAYER_BOOK,  '?', 0,    400,  66,  1, 30, 0, 0, 0, 0, 1, 1, 40), // 324
new DungeonObject_t("[Exorcisms and Dispellings]",     0x7E000000L, TV_PRAYER_BOOK,  '?', 0,    800,  67,  1, 30, 0, 0, 0, 0, 1, 1, 40), // 325
new DungeonObject_t("& Small Wooden Chest",            0x13800000L, TV_CHEST,        '&', 0,    20,   1,   1, 250, 0, 0, 0, 0, 2, 3, 7), // 326
new DungeonObject_t("& Large Wooden Chest",            0x17800000L, TV_CHEST,        '&', 0,    60,   4,   1, 500, 0, 0, 0, 0, 2, 5, 15), // 327
new DungeonObject_t("& Small Iron Chest",              0x17800000L, TV_CHEST,        '&', 0,    100,  7,   1, 500, 0, 0, 0, 0, 2, 4, 25), // 328
new DungeonObject_t("& Large Iron Chest",              0x23800000L, TV_CHEST,        '&', 0,    150,  10,  1, 1000, 0, 0, 0, 0, 2, 6, 35), // 329
new DungeonObject_t("& Small Steel Chest",             0x1B800000L, TV_CHEST,        '&', 0,    200,  13,  1, 500, 0, 0, 0, 0, 2, 4, 45), // 330
new DungeonObject_t("& Large Steel Chest",             0x33800000L, TV_CHEST,        '&', 0,    250,  16,  1, 1000, 0, 0, 0, 0, 2, 6, 50), // 331
new DungeonObject_t("& Rat Skeleton",                  0x00000000L, TV_MISC,         's', 0,    0,    1,   1, 10, 0, 0, 0, 0, 1, 1, 1), // 332
new DungeonObject_t("& Giant Centipede Skeleton",      0x00000000L, TV_MISC,         's', 0,    0,    2,   1, 25, 0, 0, 0, 0, 1, 1, 1), // 333
new DungeonObject_t("some Filthy Rags",                0x00000000L, TV_SOFT_ARMOR,   '~', 0,    0,    63,  1, 20, 0, 0, 1, 0, 0, 0, 0), // 334
new DungeonObject_t("& empty bottle",                  0x00000000L, TV_MISC,         '!', 0,    0,    4,   1, 2, 0, 0, 0, 0, 1, 1, 0), // 335
new DungeonObject_t("some shards of pottery",          0x00000000L, TV_MISC,         '~', 0,    0,    5,   1, 5, 0, 0, 0, 0, 1, 1, 0), // 336
new DungeonObject_t("& Human Skeleton",                0x00000000L, TV_MISC,         's', 0,    0,    7,   1, 60, 0, 0, 0, 0, 1, 1, 1), // 337
new DungeonObject_t("& Dwarf Skeleton",                0x00000000L, TV_MISC,         's', 0,    0,    8,   1, 50, 0, 0, 0, 0, 1, 1, 1), // 338
new DungeonObject_t("& Elf Skeleton",                  0x00000000L, TV_MISC,         's', 0,    0,    9,   1, 40, 0, 0, 0, 0, 1, 1, 1), // 339
new DungeonObject_t("& Gnome Skeleton",                0x00000000L, TV_MISC,         's', 0,    0,    10,  1, 25, 0, 0, 0, 0, 1, 1, 1), // 340
new DungeonObject_t("& broken set of teeth",           0x00000000L, TV_MISC,         's', 0,    0,    11,  1, 3, 0, 0, 0, 0, 1, 1, 0), // 341
new DungeonObject_t("& large broken bone",             0x00000000L, TV_MISC,         's', 0,    0,    12,  1, 2, 0, 0, 0, 0, 1, 1, 0), // 342
new DungeonObject_t("& broken stick",                  0x00000000L, TV_MISC,         '~', 0,    0,    13,  1, 3, 0, 0, 0, 0, 1, 1, 0), // 343
    // end of Dungeon items

    // Store items, which are not also dungeon items, some of these
    // can be found above, except that the number is >1 below.
new DungeonObject_t("& Ration~ of Food",               0x00000000L, TV_FOOD,         ',', 5000,   3,  90, 5, 10,  0, 0, 0, 0, 0, 0, 0), // 344
new DungeonObject_t("& Hard Biscuit~",                 0x00000000L, TV_FOOD,         ',', 500,    1,  93, 5,  2,  0, 0, 0, 0, 0, 0, 0), // 345
new DungeonObject_t("& Strip~ of Beef Jerky",          0x00000000L, TV_FOOD,         ',', 1750,   2,  94, 5,  4,  0, 0, 0, 0, 0, 0, 0), // 346
new DungeonObject_t("& Pint~ of Fine Ale",             0x00000000L, TV_FOOD,         ',', 500,    1,  95, 3, 10,  0, 0, 0, 0, 0, 0, 0), // 347
new DungeonObject_t("& Pint~ of Fine Wine",            0x00000000L, TV_FOOD,         ',', 400,    2,  96, 1, 10,  0, 0, 0, 0, 0, 0, 0), // 348
new DungeonObject_t("& Pick",                          0x20000000L, TV_DIGGING,     '\\', 1,     50,   1, 1, 150, 0, 0, 0, 0, 1, 3, 0), // 349
new DungeonObject_t("& Shovel",                        0x20000000L, TV_DIGGING,     '\\', 0,     15,   4, 1, 60,  0, 0, 0, 0, 1, 2, 0), // 350
new DungeonObject_t("Identify",                        0x00000008L, TV_SCROLL1,      '?', 0,     50,  67, 2,  5,  0, 0, 0, 0, 0, 0, 0), // 351
new DungeonObject_t("Light",                           0x00000020L, TV_SCROLL1,      '?', 0,     15,  69, 3,  5,  0, 0, 0, 0, 0, 0, 0), // 352
new DungeonObject_t("Phase Door",                      0x00000080L, TV_SCROLL1,      '?', 0,     15,  71, 2,  5,  0, 0, 0, 0, 0, 0, 0), // 353
new DungeonObject_t("Magic Mapping",                   0x00000800L, TV_SCROLL1,      '?', 0,     40,  75, 2,  5,  0, 0, 0, 0, 0, 0, 0), // 354
new DungeonObject_t("Treasure Detection",              0x00004000L, TV_SCROLL1,      '?', 0,     15,  78, 2,  5,  0, 0, 0, 0, 0, 0, 0), // 355
new DungeonObject_t("Object Detection",                0x00008000L, TV_SCROLL1,      '?', 0,     15,  79, 2,  5,  0, 0, 0, 0, 0, 0, 0), // 356
new DungeonObject_t("Detect Invisible",                0x00080000L, TV_SCROLL1,      '?', 0,     15,  83, 2,  5,  0, 0, 0, 0, 0, 0, 0), // 357
new DungeonObject_t("Blessing",                        0x00000020L, TV_SCROLL2,      '?', 0,     15,  99, 2,  5,  0, 0, 0, 0, 0, 0, 0), // 358
new DungeonObject_t("Word-of-Recall",                  0x00000100L, TV_SCROLL2,      '?', 0,    150, 102, 3,  5,  0, 0, 0, 0, 0, 0, 0), // 359
new DungeonObject_t("Cure Light Wounds",               0x10001000L, TV_POTION1,      '!', 50,    15,  79, 2,  4,  0, 0, 0, 0, 1, 1, 0), // 360
new DungeonObject_t("Heroism",                         0x00000010L, TV_POTION2,      '!', 0,     35,  98, 2,  4,  0, 0, 0, 0, 1, 1, 0), // 361
new DungeonObject_t("Boldness",                        0x00000040L, TV_POTION2,      '!', 0,     10, 100, 2,  4,  0, 0, 0, 0, 1, 1, 0), // 362
new DungeonObject_t("Slow Poison",                     0x00000800L, TV_POTION2,      '!', 0,     25, 105, 2,  4,  0, 0, 0, 0, 1, 1, 0), // 363
new DungeonObject_t("& Brass Lantern~",                0x00000000L, TV_LIGHT,        '~', 7500,  35,   0, 1, 50,  0, 0, 0, 0, 1, 1, 1), // 364
new DungeonObject_t("& Wooden Torch~",                 0x00000000L, TV_LIGHT,        '~', 4000,   2, 192, 5, 30,  0, 0, 0, 0, 1, 1, 1), // 365
new DungeonObject_t("& Flask~ of Oil",                 0x00040000L, TV_FLASK,        '!', 7500,   3,  64, 5, 10,  0, 0, 0, 0, 2, 6, 1), // 366
    // end store items

    // start doors
    // Secret door must have same sub_category_id as closed door in
    // TRAP_LISTB.  See CHANGE_TRAP. Must use & because of stone_to_mud.
new DungeonObject_t("& open door",                   0x00000000L, TV_OPEN_DOOR,   '\'', 0, 0,  1, 1, 0, 0, 0, 0, 0, 1, 1, 0), // 367
new DungeonObject_t("& closed door",                 0x00000000L, TV_CLOSED_DOOR,  '+', 0, 0, 19, 1, 0, 0, 0, 0, 0, 1, 1, 0), // 368
new DungeonObject_t("& secret door",                 0x00000000L, TV_SECRET_DOOR,  '#', 0, 0, 19, 1, 0, 0, 0, 0, 0, 1, 1, 0), // 369
    // end doors

    // stairs
new DungeonObject_t("an up staircase",               0x00000000L, TV_UP_STAIR,     '<', 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0), // 370
new DungeonObject_t("a down staircase",              0x00000000L, TV_DOWN_STAIR,   '>', 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0), // 371

    // store door
    // Stores are just special traps
new DungeonObject_t("General Store",                 0x00000000L, TV_STORE_DOOR,   '1', 0, 0, 101, 1, 0, 0, 0, 0, 0, 0, 0, 0), // 372
new DungeonObject_t("Armory",                        0x00000000L, TV_STORE_DOOR,   '2', 0, 0, 102, 1, 0, 0, 0, 0, 0, 0, 0, 0), // 373
new DungeonObject_t("Weapon Smiths",                 0x00000000L, TV_STORE_DOOR,   '3', 0, 0, 103, 1, 0, 0, 0, 0, 0, 0, 0, 0), // 374
new DungeonObject_t("Temple",                        0x00000000L, TV_STORE_DOOR,   '4', 0, 0, 104, 1, 0, 0, 0, 0, 0, 0, 0, 0), // 375
new DungeonObject_t("Alchemy Shop",                  0x00000000L, TV_STORE_DOOR,   '5', 0, 0, 105, 1, 0, 0, 0, 0, 0, 0, 0, 0), // 376
new DungeonObject_t("Magic Shop",                    0x00000000L, TV_STORE_DOOR,   '6', 0, 0, 106, 1, 0, 0, 0, 0, 0, 0, 0, 0), // 377
    // end store door

    // Traps are just Nasty treasures.
    // Traps: Level represents the relative difficulty of disarming;
    // and `misc_use` represents the experienced gained when disarmed
new DungeonObject_t("an open pit",                   0x00000000L, TV_VIS_TRAP,     ' ',  1, 0,  1, 1, 0, 0, 0, 0, 0, 2, 6,  50), // 378
new DungeonObject_t("an arrow trap",                 0x00000000L, TV_INVIS_TRAP,   '^',  3, 0,  2, 1, 0, 0, 0, 0, 0, 1, 8,  90), // 379
new DungeonObject_t("a covered pit",                 0x00000000L, TV_INVIS_TRAP,   '^',  2, 0,  3, 1, 0, 0, 0, 0, 0, 2, 6,  60), // 380
new DungeonObject_t("a trap door",                   0x00000000L, TV_INVIS_TRAP,   '^',  5, 0,  4, 1, 0, 0, 0, 0, 0, 2, 8,  75), // 381
new DungeonObject_t("a gas trap",                    0x00000000L, TV_INVIS_TRAP,   '^',  3, 0,  5, 1, 0, 0, 0, 0, 0, 1, 4,  95), // 382
new DungeonObject_t("a loose rock",                  0x00000000L, TV_INVIS_TRAP,   ';',  0, 0,  6, 1, 0, 0, 0, 0, 0, 0, 0,  10), // 383
new DungeonObject_t("a dart trap",                   0x00000000L, TV_INVIS_TRAP,   '^',  5, 0,  7, 1, 0, 0, 0, 0, 0, 1, 4, 110), // 384
new DungeonObject_t("a strange rune",                0x00000000L, TV_INVIS_TRAP,   '^',  5, 0,  8, 1, 0, 0, 0, 0, 0, 0, 0,  90), // 385
new DungeonObject_t("some loose rock",               0x00000000L, TV_INVIS_TRAP,   '^',  5, 0,  9, 1, 0, 0, 0, 0, 0, 2, 6,  90), // 386
new DungeonObject_t("a gas trap",                    0x00000000L, TV_INVIS_TRAP,   '^', 10, 0, 10, 1, 0, 0, 0, 0, 0, 1, 4, 105), // 387
new DungeonObject_t("a strange rune",                0x00000000L, TV_INVIS_TRAP,   '^',  5, 0, 11, 1, 0, 0, 0, 0, 0, 0, 0,  90), // 388
new DungeonObject_t("a blackened spot",              0x00000000L, TV_INVIS_TRAP,   '^', 10, 0, 12, 1, 0, 0, 0, 0, 0, 4, 6, 110), // 389
new DungeonObject_t("some corroded rock",            0x00000000L, TV_INVIS_TRAP,   '^', 10, 0, 13, 1, 0, 0, 0, 0, 0, 4, 6, 110), // 390
new DungeonObject_t("a gas trap",                    0x00000000L, TV_INVIS_TRAP,   '^',  5, 0, 14, 1, 0, 0, 0, 0, 0, 2, 6, 105), // 391
new DungeonObject_t("a gas trap",                    0x00000000L, TV_INVIS_TRAP,   '^',  5, 0, 15, 1, 0, 0, 0, 0, 0, 1, 4, 110), // 392
new DungeonObject_t("a gas trap",                    0x00000000L, TV_INVIS_TRAP,   '^',  5, 0, 16, 1, 0, 0, 0, 0, 0, 1, 8, 105), // 393
new DungeonObject_t("a dart trap",                   0x00000000L, TV_INVIS_TRAP,   '^',  5, 0, 17, 1, 0, 0, 0, 0, 0, 1, 8, 110), // 394
new DungeonObject_t("a dart trap",                   0x00000000L, TV_INVIS_TRAP,   '^',  5, 0, 18, 1, 0, 0, 0, 0, 0, 1, 8, 110), // 395

    // rubble
new DungeonObject_t("some rubble",                  0x00000000L, TV_RUBBLE,   ':', 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0), // 396

    // mush
new DungeonObject_t("& Pint~ of Fine Grade Mush",    0x00000000L, TV_FOOD,     ',', 1500, 1, 97, 1, 1, 0, 0, 0, 0, 1, 1, 1), // 397

    // Special trap
new DungeonObject_t("a strange rune",                0x00000000L, TV_VIS_TRAP, '^', 0, 0, 99, 1, 0, 0, 0, 0, 0, 0, 0, 10), // 398

    // Gold list (All types of gold and gems are defined here)
new DungeonObject_t("copper",                        0x00000000L, TV_GOLD,     '$', 0,  3,  1, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 399
new DungeonObject_t("copper",                        0x00000000L, TV_GOLD,     '$', 0,  4,  2, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 400
new DungeonObject_t("copper",                        0x00000000L, TV_GOLD,     '$', 0,  5,  3, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 401
new DungeonObject_t("silver",                        0x00000000L, TV_GOLD,     '$', 0,  6,  4, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 402
new DungeonObject_t("silver",                        0x00000000L, TV_GOLD,     '$', 0,  7,  5, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 403
new DungeonObject_t("silver",                        0x00000000L, TV_GOLD,     '$', 0,  8,  6, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 404
new DungeonObject_t("garnets",                       0x00000000L, TV_GOLD,     '*', 0,  9,  7, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 405
new DungeonObject_t("garnets",                       0x00000000L, TV_GOLD,     '*', 0, 10,  8, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 406
new DungeonObject_t("gold",                          0x00000000L, TV_GOLD,     '$', 0, 12,  9, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 407
new DungeonObject_t("gold",                          0x00000000L, TV_GOLD,     '$', 0, 14, 10, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 408
new DungeonObject_t("gold",                          0x00000000L, TV_GOLD,     '$', 0, 16, 11, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 409
new DungeonObject_t("opals",                         0x00000000L, TV_GOLD,     '*', 0, 18, 12, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 410
new DungeonObject_t("sapphires",                     0x00000000L, TV_GOLD,     '*', 0, 20, 13, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 411
new DungeonObject_t("gold",                          0x00000000L, TV_GOLD,     '$', 0, 24, 14, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 412
new DungeonObject_t("rubies",                        0x00000000L, TV_GOLD,     '*', 0, 28, 15, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 413
new DungeonObject_t("diamonds",                      0x00000000L, TV_GOLD,     '*', 0, 32, 16, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 414
new DungeonObject_t("emeralds",                      0x00000000L, TV_GOLD,     '*', 0, 40, 17, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 415
new DungeonObject_t("mithril",                       0x00000000L, TV_GOLD,     '$', 0, 80, 18, 1, 0, 0, 0, 0, 0, 0, 0, 1), // 416

    // nothing, used as inventory place holder
    // must be stackable, so that can be picked up by inventoryCarryItem
new DungeonObject_t("nothing",                       0x00000000L, TV_NOTHING,  ' ', 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0), // 417

    // these next two are needed only for the names
new DungeonObject_t("& ruined chest",                0x00000000L, TV_CHEST,    '&', 0, 0, 0, 1, 250, 0, 0, 0, 0, 0, 0, 0), // 418
new DungeonObject_t("",                              0x00000000L, TV_NOTHING,  ' ', 0, 0, 0, 0,   0, 0, 0, 0, 0, 0, 0, 0), // 419
            };

            return list;
        }

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
