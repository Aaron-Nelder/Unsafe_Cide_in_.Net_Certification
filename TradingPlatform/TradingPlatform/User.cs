namespace TradingPlatform
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public double Balance { get; set; }
        public List<double> BuyTargetPrices { get; set; }
        public List<double> SellTargetPrices { get; set; }

        public User(int userId, string name)
        {
            UserId = userId;
            Name = name;
            BuyTargetPrices = new();
            SellTargetPrices = new();
        }

        public void SubscribeToBuyPrice(double price)
        {
            BuyTargetPrices.Add(price);
        }

        public void SubscribeToSellPrice(double price)
        {
            SellTargetPrices.Add(price);
        }

        public void OnPriceNotification(object sender,PriceNotificationEventArgs e)
        {
            if (e.IsBuyOrder)
            {
                for(int i = BuyTargetPrices.Count - 1; i >= 0; i--)
                {
                    if(e.Price >= BuyTargetPrices[i])
                    {
                        Console.WriteLine($"Notification to user {UserId} ({Name}: current highest buy price has risen to {e.Price})");
                        BuyTargetPrices.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                for (int i = SellTargetPrices.Count - 1; i >= 0; i--)
                {
                    if (e.Price >= SellTargetPrices[i] && e.Price != 0)
                    {
                        Console.WriteLine($"Notification to user {UserId} ({Name}: current lowest sell price has fallen to {e.Price})");
                        SellTargetPrices.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
