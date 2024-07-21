using System.Dynamic;
using TradingPlatform;
TradeLogger logger = new();

Random random = new();
for (int i = 0; i < 10; i++)
{
    Trade trade = new()
    {
        TradeId = i + 1,
        OrderId = (i % 50) + 1,
        Price = 100 + (i % 20),
        Quantity = 10 + (i % 5),
        Timestamp = DateTime.Now.AddDays(-random.Next(0, 30))
    };
    logger.LogTrade(trade);
}

logger.FinalizeLogging();