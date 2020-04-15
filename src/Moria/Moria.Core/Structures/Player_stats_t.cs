namespace Moria.Core.Structures
{
    public class Player_stats_t
    {
        public uint[] max { get; set; } = new uint[6];
        public uint[] current { get; set; } = new uint[6];
        public int[] modified { get; set; } = new int[6];
        public uint[] used { get; set; } = new uint[6];
    }
}