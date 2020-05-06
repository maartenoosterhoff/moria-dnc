namespace Moria.Core.Configs
{
    public class ConfigDungeon
    {
        public uint DUN_RANDOM_DIR { get; } = 9;
        public uint DUN_DIR_CHANGE { get; } = 70;
        public uint DUN_TUNNELING { get; } = 15;
        public uint DUN_ROOMS_MEAN { get; } = 32;
        public uint DUN_ROOM_DOORS { get; } = 25;
        public uint DUN_TUNNEL_DOORS { get; } = 15;
        public uint DUN_STREAMER_DENSITY { get; } = 5;
        public uint DUN_STREAMER_WIDTH { get; } = 2;
        public uint DUN_MAGMA_STREAMER { get; } = 3;
        public uint DUN_MAGMA_TREASURE { get; } = 90;
        public uint DUN_QUARTZ_STREAMER { get; } = 2;
        public uint DUN_QUARTZ_TREASURE { get; } = 40;
        public uint DUN_UNUSUAL_ROOMS { get; } = 300;
    }
}