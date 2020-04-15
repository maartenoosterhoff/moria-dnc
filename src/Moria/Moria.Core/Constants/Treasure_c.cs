namespace Moria.Core.Constants
{
    public static class Treasure_c
    {
        // defines for treasure type values (tval)
        public const int TV_NEVER = -1; // used by find_range() for non-search
        public const uint TV_NOTHING = 0;
        public const uint TV_MISC = 1;
        public const uint TV_CHEST = 2;

        // min tval for wearable items, all items between TV_MIN_WEAR and
        // TV_MAX_WEAR use the same flag bits, see the TR_* defines.
        public const uint TV_MIN_WEAR = 10;
        // items tested for enchantments, i.e. the MAGIK inscription, see the enchanted() procedure.
        public const uint TV_MIN_ENCHANT = 10;
        public const uint TV_SLING_AMMO = 10;
        public const uint TV_BOLT = 11;
        public const uint TV_ARROW = 12;
        public const uint TV_SPIKE = 13;
        public const uint TV_LIGHT = 15;
        public const uint TV_BOW = 20;
        public const uint TV_HAFTED = 21;
        public const uint TV_POLEARM = 22;
        public const uint TV_SWORD = 23;
        public const uint TV_DIGGING = 25;
        public const uint TV_BOOTS = 30;
        public const uint TV_GLOVES = 31;
        public const uint TV_CLOAK = 32;
        public const uint TV_HELM = 33;
        public const uint TV_SHIELD = 34;
        public const uint TV_HARD_ARMOR = 35;
        public const uint TV_SOFT_ARMOR = 36;
        // max tval that uses the TR_* flags
        public const uint TV_MAX_ENCHANT = 39;
        public const uint TV_AMULET = 40;
        public const uint TV_RING = 45;
        public const uint TV_MAX_WEAR = 50; // max tval for wearable items

        public const uint TV_STAFF = 55;
        public const uint TV_WAND = 65;
        public const uint TV_SCROLL1 = 70;
        public const uint TV_SCROLL2 = 71;
        public const uint TV_POTION1 = 75;
        public const uint TV_POTION2 = 76;
        public const uint TV_FLASK = 77;
        public const uint TV_FOOD = 80;
        public const uint TV_MAGIC_BOOK = 90;
        public const uint TV_PRAYER_BOOK = 91;
        public const uint TV_MAX_OBJECT = 99; // objects with tval above this are never picked up by monsters
        public const uint TV_GOLD = 100;
        public const uint TV_MAX_PICK_UP = 100; // objects with higher tvals can not be picked up
        public const uint TV_INVIS_TRAP = 101;

        // objects between TV_MIN_VISIBLE and TV_MAX_VISIBLE are always visible,
        // i.e. the cave fm flag is set when they are present
        public const uint TV_MIN_VISIBLE = 102;
        public const uint TV_VIS_TRAP = 102;
        public const uint TV_RUBBLE = 103;
        // following objects are never deleted when trying to create another one during level generation
        public const uint TV_MIN_DOORS = 104;
        public const uint TV_OPEN_DOOR = 104;
        public const uint TV_CLOSED_DOOR = 105;
        public const uint TV_UP_STAIR = 107;
        public const uint TV_DOWN_STAIR = 108;
        public const uint TV_SECRET_DOOR = 109;
        public const uint TV_STORE_DOOR = 110;
        public const uint TV_MAX_VISIBLE = 110;
    }
}
