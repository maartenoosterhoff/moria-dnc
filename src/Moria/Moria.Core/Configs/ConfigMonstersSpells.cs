namespace Moria.Core.Configs
{
    public class ConfigMonstersSpells
    {
        public uint CS_FREQ { get; set; } = 0x0000000F;
        public uint CS_SPELLS { get; set; } = 0x0001FFF0;
        public uint CS_TEL_SHORT { get; set; } = 0x00000010;
        public uint CS_TEL_LONG { get; set; } = 0x00000020;
        public uint CS_TEL_TO { get; set; } = 0x00000040;
        public uint CS_LGHT_WND { get; set; } = 0x00000080;
        public uint CS_SER_WND { get; set; } = 0x00000100;
        public uint CS_HOLD_PER { get; set; } = 0x00000200;
        public uint CS_BLIND { get; set; } = 0x00000400;
        public uint CS_CONFUSE { get; set; } = 0x00000800;
        public uint CS_FEAR { get; set; } = 0x00001000;
        public uint CS_SUMMON_MON { get; set; } = 0x00002000;
        public uint CS_SUMMON_UND { get; set; } = 0x00004000;
        public uint CS_SLOW_PER { get; set; } = 0x00008000;
        public uint CS_DRAIN_MANA { get; set; } = 0x00010000;

        public uint CS_BREATHE { get; set; } = 0x00F80000;  // may also just indicate resistance
        public uint CS_BR_LIGHT { get; set; } = 0x00080000; // if no spell frequency set
        public uint CS_BR_GAS { get; set; } = 0x00100000;
        public uint CS_BR_ACID { get; set; } = 0x00200000;
        public uint CS_BR_FROST { get; set; } = 0x00400000;
        public uint CS_BR_FIRE { get; set; } = 0x00800000;
    }
}