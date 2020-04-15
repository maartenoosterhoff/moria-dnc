namespace Moria.Core.Configs
{
    public class ConfigDungeonObjects
    {
        public uint OBJ_OPEN_DOOR { get; set; } = 367;
        public uint OBJ_CLOSED_DOOR { get; set; } = 368;
        public uint OBJ_SECRET_DOOR { get; set; } = 369;
        public uint OBJ_UP_STAIR { get; set; } = 370;
        public uint OBJ_DOWN_STAIR { get; set; } = 371;
        public uint OBJ_STORE_DOOR { get; set; } = 372;
        public uint OBJ_TRAP_LIST { get; set; } = 378;
        public uint OBJ_RUBBLE { get; set; } = 396;
        public uint OBJ_MUSH { get; set; } = 397;
        public uint OBJ_SCARE_MON { get; set; } = 398;
        public uint OBJ_GOLD_LIST { get; set; } = 399;
        public uint OBJ_NOTHING { get; set; } = 417;
        public uint OBJ_RUINED_CHEST { get; set; } = 418;
        public uint OBJ_WIZARD { get; set; } = 419;

        public uint MAX_GOLD_TYPES { get; set; } = 18; // Number of different types of gold
        public uint MAX_TRAPS { get; set; } = 18;      // Number of defined traps

        public uint LEVEL_OBJECTS_PER_ROOM { get; set; } = 7;     // Amount of objects for rooms
        public uint LEVEL_OBJECTS_PER_CORRIDOR { get; set; } = 2; // Amount of objects for corridors
        public uint LEVEL_TOTAL_GOLD_AND_GEMS { get; set; } = 2;  // Amount of gold (and gems)
    }
}