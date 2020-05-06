namespace Moria.Core.Configs
{
    public class ConfigDungeonObjects
    {
        public uint OBJ_OPEN_DOOR { get; } = 367;
        public uint OBJ_CLOSED_DOOR { get; } = 368;
        public uint OBJ_SECRET_DOOR { get; } = 369;
        public uint OBJ_UP_STAIR { get; } = 370;
        public uint OBJ_DOWN_STAIR { get; } = 371;
        public uint OBJ_STORE_DOOR { get; } = 372;
        public uint OBJ_TRAP_LIST { get; } = 378;
        public uint OBJ_RUBBLE { get; } = 396;
        public uint OBJ_MUSH { get; } = 397;
        public uint OBJ_SCARE_MON { get; } = 398;
        public uint OBJ_GOLD_LIST { get; } = 399;
        public uint OBJ_NOTHING { get; } = 417;
        public uint OBJ_RUINED_CHEST { get; } = 418;
        public uint OBJ_WIZARD { get; } = 419;

        public uint MAX_GOLD_TYPES { get; } = 18; // Number of different types of gold
        public uint MAX_TRAPS { get; } = 18;      // Number of defined traps

        public uint LEVEL_OBJECTS_PER_ROOM { get; } = 7;     // Amount of objects for rooms
        public uint LEVEL_OBJECTS_PER_CORRIDOR { get; } = 2; // Amount of objects for corridors
        public uint LEVEL_TOTAL_GOLD_AND_GEMS { get; } = 2;  // Amount of gold (and gems)
    }
}