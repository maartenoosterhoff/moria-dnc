namespace Moria.Core.Configs
{
    public class ConfigDungeon
    {
        public uint DUN_RANDOM_DIR { get; set; } = 9;
        public uint DUN_DIR_CHANGE { get; set; } = 70;
        public uint DUN_TUNNELING { get; set; } = 15;
        public uint DUN_ROOMS_MEAN { get; set; } = 32;
        public uint DUN_ROOM_DOORS { get; set; } = 25;
        public uint DUN_TUNNEL_DOORS { get; set; } = 15;
        public uint DUN_STREAMER_DENSITY { get; set; } = 5;
        public uint DUN_STREAMER_WIDTH { get; set; } = 2;
        public uint DUN_MAGMA_STREAMER { get; set; } = 3;
        public uint DUN_MAGMA_TREASURE { get; set; } = 90;
        public uint DUN_QUARTZ_STREAMER { get; set; } = 2;
        public uint DUN_QUARTZ_TREASURE { get; set; } = 40;
        public uint DUN_UNUSUAL_ROOMS { get; set; } = 300;
    }
}