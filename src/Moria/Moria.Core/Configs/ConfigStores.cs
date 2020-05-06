namespace Moria.Core.Configs
{
    public class ConfigStores
    {
        public uint STORE_MAX_AUTO_BUY_ITEMS { get; } = 18;  // Max diff objects in stock for auto buy
        public uint STORE_MIN_AUTO_SELL_ITEMS { get; } = 10; // Min diff objects in stock for auto sell
        public uint STORE_STOCK_TURN_AROUND { get; } = 9; // Amount of buying and selling normally
    }
}