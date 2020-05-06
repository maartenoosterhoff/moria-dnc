namespace Moria.Core.Configs
{
    public class ConfigMonstersDefense
    {
        public uint CD_DRAGON { get; } = 0x0001;
        public uint CD_ANIMAL { get; } = 0x0002;
        public uint CD_EVIL { get; } = 0x0004;
        public uint CD_UNDEAD { get; } = 0x0008;
        public uint CD_WEAKNESS { get; } = 0x03F0;
        public uint CD_FROST { get; } = 0x0010;
        public uint CD_FIRE { get; } = 0x0020;
        public uint CD_POISON { get; } = 0x0040;
        public uint CD_ACID { get; } = 0x0080;
        public uint CD_LIGHT { get; } = 0x0100;
        public uint CD_STONE { get; } = 0x0200;
        public uint CD_NO_SLEEP { get; } = 0x1000;
        public uint CD_INFRA { get; } = 0x2000;
        public uint CD_MAX_HP { get; } = 0x4000;
    }
}