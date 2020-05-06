namespace Moria.Core.Configs
{
    public class ConfigPlayerStatus
    {
        public uint PY_HUNGRY { get; } = 0x00000001;
        public uint PY_WEAK { get; } = 0x00000002;
        public uint PY_BLIND { get; } = 0x00000004;
        public uint PY_CONFUSED { get; } = 0x00000008;
        public uint PY_FEAR { get; } = 0x00000010;
        public uint PY_POISONED { get; } = 0x00000020;
        public uint PY_FAST { get; } = 0x00000040;
        public uint PY_SLOW { get; } = 0x00000080;
        public uint PY_SEARCH { get; } = 0x00000100;
        public uint PY_REST { get; } = 0x00000200;
        public uint PY_STUDY { get; } = 0x00000400;

        public uint PY_INVULN { get; } = 0x00001000;
        public uint PY_HERO { get; } = 0x00002000;
        public uint PY_SHERO { get; } = 0x00004000;
        public uint PY_BLESSED { get; } = 0x00008000;
        public uint PY_DET_INV { get; } = 0x00010000;
        public uint PY_TIM_INFRA { get; } = 0x00020000;
        public uint PY_SPEED { get; } = 0x00040000;
        public uint PY_STR_WGT { get; } = 0x00080000;
        public uint PY_PARALYSED { get; } = 0x00100000;
        public uint PY_REPEAT { get; } = 0x00200000;
        public uint PY_ARMOR { get; } = 0x00400000;

        public uint PY_STATS { get; } = 0x3F000000;
        public uint PY_STR { get; } = 0x01000000; // these 6 stat flags must be adjacent
        public uint PY_INT { get; } = 0x02000000;
        public uint PY_WIS { get; } = 0x04000000;
        public uint PY_DEX { get; } = 0x08000000;
        public uint PY_CON { get; } = 0x10000000;
        public uint PY_CHR { get; } = 0x20000000;

        public uint PY_HP { get; } = 0x40000000;
        public uint PY_MANA { get; } = 0x80000000;
    }
}