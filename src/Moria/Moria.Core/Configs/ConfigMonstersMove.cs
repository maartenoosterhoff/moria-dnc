namespace Moria.Core.Configs
{
    public class ConfigMonstersMove
    {
        public uint CM_ALL_MV_FLAGS { get; set; } = 0x0000003F;
        public uint CM_ATTACK_ONLY { get; set; } = 0x00000001;
        public uint CM_MOVE_NORMAL { get; set; } = 0x00000002;
        public uint CM_ONLY_MAGIC { get; set; } = 0x00000004; // For Quylthulgs, which have no physical movement.

        public uint CM_RANDOM_MOVE { get; set; } = 0x00000038;
        public uint CM_20_RANDOM { get; set; } = 0x00000008;
        public uint CM_40_RANDOM { get; set; } = 0x00000010;
        public uint CM_75_RANDOM { get; set; } = 0x00000020;

        public uint CM_SPECIAL { get; set; } = 0x003F0000;
        public uint CM_INVISIBLE { get; set; } = 0x00010000;
        public uint CM_OPEN_DOOR { get; set; } = 0x00020000;
        public uint CM_PHASE { get; set; } = 0x00040000;
        public uint CM_EATS_OTHER { get; set; } = 0x00080000;
        public uint CM_PICKS_UP { get; set; } = 0x00100000;
        public uint CM_MULTIPLY { get; set; } = 0x00200000;

        public uint CM_SMALL_OBJ { get; set; } = 0x00800000;
        public uint CM_CARRY_OBJ { get; set; } = 0x01000000;
        public uint CM_CARRY_GOLD { get; set; } = 0x02000000;
        public uint CM_TREASURE { get; set; } = 0x7C000000;
        public uint CM_TR_SHIFT { get; set; } = 26; // used for recall of treasure
        public uint CM_60_RANDOM { get; set; } = 0x04000000;
        public uint CM_90_RANDOM { get; set; } = 0x08000000;
        public uint CM_1D2_OBJ { get; set; } = 0x10000000;
        public uint CM_2D2_OBJ { get; set; } = 0x20000000;
        public uint CM_4D2_OBJ { get; set; } = 0x40000000;
        public uint CM_WIN { get; set; } = 0x80000000;
    }
}