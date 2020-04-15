namespace Moria.Core.Configs
{
    public class ConfigMonstersDefense
    {
        public uint CD_DRAGON { get; set; } = 0x0001;
        public uint CD_ANIMAL { get; set; } = 0x0002;
        public uint CD_EVIL { get; set; } = 0x0004;
        public uint CD_UNDEAD { get; set; } = 0x0008;
        public uint CD_WEAKNESS { get; set; } = 0x03F0;
        public uint CD_FROST { get; set; } = 0x0010;
        public uint CD_FIRE { get; set; } = 0x0020;
        public uint CD_POISON { get; set; } = 0x0040;
        public uint CD_ACID { get; set; } = 0x0080;
        public uint CD_LIGHT { get; set; } = 0x0100;
        public uint CD_STONE { get; set; } = 0x0200;
        public uint CD_NO_SLEEP { get; set; } = 0x1000;
        public uint CD_INFRA { get; set; } = 0x2000;
        public uint CD_MAX_HP { get; set; } = 0x4000;
    }
}