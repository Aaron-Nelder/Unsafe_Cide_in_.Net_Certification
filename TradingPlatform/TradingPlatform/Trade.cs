
namespace TradingPlatform
{
    public struct Trade
    {
        public int TradeId { get; set; }
        public int OrderId { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
