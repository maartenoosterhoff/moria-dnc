namespace Moria.Core.Constants
{
    public static class Game_c
    {
        public const uint TREASURE_MAX_LEVELS = 50; // Maximum level of magic in dungeon

        // Note that the following constants are all related, if you change one, you
        // must also change all succeeding ones.
        // Also, player_base_provisions[] and store_choices[] may also have to be changed.
        public const uint MAX_OBJECTS_IN_GAME = 420; // Number of objects for universe
        public const uint MAX_DUNGEON_OBJECTS = 344; // Number of dungeon objects
        public const uint OBJECT_IDENT_SIZE = 448;   // 7*64, see object_offset() in desc.cpp, could be MAX_OBJECTS o_o() rewritten

        // With LEVEL_MAX_OBJECTS set to 150, it's possible to get compacting
        // objects during level generation, although it is extremely rare.
        public const uint LEVEL_MAX_OBJECTS = 175; // Max objects per level

        // definitions for the pseudo-normal distribution generation
        public const uint NORMAL_TABLE_SIZE = 256;
        public const uint NORMAL_TABLE_SD = 64; // the standard deviation for the table
    }
}
