namespace Moria.Core.Configs
{
    public class ConfigTreasureChests
    {
        public uint CH_LOCKED { get; set; } = 0x00000001;
        public uint CH_TRAPPED { get; set; } = 0x000001F0;
        public uint CH_LOSE_STR { get; set; } = 0x00000010;
        public uint CH_POISON { get; set; } = 0x00000020;
        public uint CH_PARALYSED { get; set; } = 0x00000040;
        public uint CH_EXPLODE { get; set; } = 0x00000080;
        public uint CH_SUMMON { get; set; } = 0x00000100;
    }
}