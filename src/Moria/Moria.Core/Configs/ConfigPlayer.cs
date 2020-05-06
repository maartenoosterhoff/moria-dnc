namespace Moria.Core.Configs
{
    public class ConfigPlayer
    {
        public int PLAYER_MAX_EXP { get; } = 9999999;        // Maximum amount of experience -CJS-
        public uint PLAYER_USE_DEVICE_DIFFICULTY { get; } = 3; // x> Harder devices x< Easier devices
        public uint PLAYER_FOOD_FULL { get; } = 10000;        // Getting full
        public uint PLAYER_FOOD_MAX { get; } = 15000;         // Maximum food value, beyond is wasted
        public uint PLAYER_FOOD_FAINT { get; } = 300;         // Character begins fainting
        public uint PLAYER_FOOD_WEAK { get; } = 1000;         // Warn player that they're getting weak
        public uint PLAYER_FOOD_ALERT { get; } = 2000;        // Alert player that they're getting low on food
        public uint PLAYER_REGEN_FAINT { get; } = 33;          // Regen factor*2^16 when fainting
        public uint PLAYER_REGEN_WEAK { get; } = 98;           // Regen factor*2^16 when weak
        public uint PLAYER_REGEN_NORMAL { get; } = 197;        // Regen factor*2^16 when full
        public uint PLAYER_REGEN_HPBASE { get; } = 1442;      // Min amount hp regen*2^16
        public uint PLAYER_REGEN_MNBASE { get; } = 524;       // Min amount mana regen*2^16
        public uint PLAYER_WEIGHT_CAP { get; } = 130;          // "#"*(1/10 pounds) per strength point
    }
}