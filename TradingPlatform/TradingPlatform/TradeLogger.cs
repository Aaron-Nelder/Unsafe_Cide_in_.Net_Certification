using System.Runtime.InteropServices;

namespace TradingPlatform
{
    public unsafe class TradeLogger
    {
        const int bufferSize = 100;
        Trade* tradeBuffer;
        int tradeCount;

        public TradeLogger()
        {
            unsafe
            {
                tradeBuffer = (Trade*)NativeMemory.Alloc((nuint)(bufferSize * sizeof(Trade)));
            }
        }

        ~TradeLogger()
        {
            unsafe
            {
                NativeMemory.Free(tradeBuffer);
            }
        }

        public unsafe void LogTrade(Trade trade)
        {
            if (tradeCount < bufferSize)
            {
                tradeBuffer[tradeCount] = trade;
                tradeCount++;
            }
            else
            {
                ProcessAndClearBuffer();
                tradeBuffer[0] = trade;

                tradeCount = 1;
            }
        }

        unsafe void ProcessAndClearBuffer()
        {
            using (StreamWriter writer = new("TradeReport.txt"))
            {
                for (int i = 0; i < tradeCount; i++)
                {
                    Trade trade = tradeBuffer[i];
                    writer.WriteLine($"Trade ID: {trade.TradeId},Order ID: {trade.OrderId},Price: {trade.Price},Quantity: {trade.Quantity},Timestamp: {trade.Timestamp}");
                }
            }
            tradeCount = 0;
        }

        public unsafe void FinalizeLogging()
        {
            if (tradeCount > 0)
                ProcessAndClearBuffer();
        }
    }
}