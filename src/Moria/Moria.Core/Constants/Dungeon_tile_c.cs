namespace Moria.Core.Constants
{
    public static class Dungeon_tile_c
    {
        public const uint TILE_NULL_WALL = 0;
        public const uint TILE_DARK_FLOOR = 1;
        public const uint TILE_LIGHT_FLOOR = 2;
        public const uint MAX_CAVE_ROOM = 2;
        public const uint TILE_CORR_FLOOR = 3;
        public const uint TILE_BLOCKED_FLOOR = 4; // a corridor space with cl/st/se door or rubble
        public const uint MAX_CAVE_FLOOR = 4;

        public const uint MAX_OPEN_SPACE = 3;
        public const uint MIN_CLOSED_SPACE = 4;

        public const uint TMP1_WALL = 8;
        public const uint TMP2_WALL = 9;

        public const uint MIN_CAVE_WALL = 12;
        public const uint TILE_GRANITE_WALL = 12;
        public const uint TILE_MAGMA_WALL = 13;
        public const uint TILE_QUARTZ_WALL = 14;
        public const uint TILE_BOUNDARY_WALL = 15;
    }
}
