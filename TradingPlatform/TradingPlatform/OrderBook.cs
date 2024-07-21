using System.Runtime.InteropServices;

namespace TradingPlatform
{
    public unsafe class OrderBook
    {
        int size = 10;
        Order* buyOrders;
        Order* sellOrders;

        public delegate void PriceNotificationEventHandler(object sender, PriceNotificationEventArgs e);

        public event PriceNotificationEventHandler PriceNotification;

        private double highestBuyPrice;
        private double lowestSellPrice;

        public OrderBook(int size)
        {
            this.size = size;
            buyOrders = (Order*)NativeMemory.Alloc((nuint)(size * sizeof(Order)));
            sellOrders = (Order*)NativeMemory.Alloc((nuint)(size * sizeof(Order)));

            for (int i = 0; i < size; i++)
            {
                buyOrders[i] = new Order() { Id = 0 };
                sellOrders[i] = new Order { Id = 0 };
            }

            highestBuyPrice = double.MinValue;
            lowestSellPrice = double.MaxValue;
        }

        protected virtual void OnPriceNotification(PriceNotificationEventArgs e)
        {
            PriceNotification?.Invoke(this, e);
        }

        ~OrderBook()
        {
            NativeMemory.Free(buyOrders);
            NativeMemory.Free(sellOrders);
        }

        public unsafe void AddOrder(Order newOrder)
        {
            Order* orders = newOrder.IsBuyOrder ? buyOrders : sellOrders;
            for (int i = 0; i < size; i++)
            {
                if (orders[i].Id == 0)
                {
                    orders[i] = newOrder;
                    UpdateAndNotify();
                    break;
                }
            }
        }

        public unsafe void RemoveOrder(int orderId)
        {
            Order* orders = null;
            int orderIndex = -1;
            for (int i = 0; i < size; i++)
            {
                if (buyOrders[i].Id == orderId)
                {
                    orders = buyOrders;
                    orderIndex = i;
                    break;
                }
                else if (sellOrders[i].Id == orderId)
                {
                    orders = sellOrders;
                    orderIndex = i;
                    break;
                }
            }

            if (orderIndex != -1)
            {
                orders[orderIndex] = new Order() { Id = 0 };
            }
        }

        public unsafe void ModifyOrder(int orderId, double newPrice, int newQuantity)
        {
            for (int i = 0; i < size; i++)
            {
                if (buyOrders[i].Id == orderId)
                {
                    buyOrders[i].Price = newPrice;
                    buyOrders[i].Quantity = newQuantity;
                    UpdateAndNotify();
                    break;
                }
                else if (sellOrders[i].Id == orderId)
                {
                    sellOrders[i].Price = newPrice;
                    sellOrders[i].Quantity = newQuantity;
                    UpdateAndNotify();
                    break;
                }
            }
        }

        private unsafe void UpdateAndNotify()
        {
            fixed (double* fixedHighestBuyPrice = &highestBuyPrice, fixedLowestSellPrice = &lowestSellPrice)
            {
                *fixedHighestBuyPrice = double.MinValue;
                *fixedLowestSellPrice = double.MaxValue;

                for (int i = 0; i < size; i++)
                {
                    if (buyOrders[i].Price > *fixedHighestBuyPrice)
                    {
                        *fixedHighestBuyPrice = buyOrders[i].Price;
                        OnPriceNotification(new PriceNotificationEventArgs(*fixedHighestBuyPrice, true));
                    }

                    if (sellOrders[i].Price < *fixedLowestSellPrice)
                    {
                        *fixedLowestSellPrice = sellOrders[i].Price;
                        OnPriceNotification(new PriceNotificationEventArgs(*fixedLowestSellPrice, false));
                    }
                }
            }
        }

        public unsafe double GetLowestSellPrice(out int lowestSellIndex)
        {
            lowestSellIndex = -1;
            double lowestSellPrice = double.MaxValue;

            for (int i = 0; i < size; i++)
            {
                if (sellOrders[i].Id != 0 && sellOrders[i].Price < lowestSellPrice)
                {
                    lowestSellPrice = sellOrders[i].Price;
                    lowestSellIndex = i;
                }
            }

            return lowestSellPrice == double.MaxValue ? -1 : lowestSellPrice;
        }

        public unsafe bool BuyAtLowestSellPrice(int buyQuantity, double maxPrice, User buyer)
        {
            int lowestSellIndex;
            double lowestSellPrice = GetLowestSellPrice(out lowestSellIndex);

            if (lowestSellPrice == -1 || lowestSellPrice > maxPrice)
            {
                Console.WriteLine($"No Suitable sell orders available");
                return false;
            }

            double totalCost = lowestSellPrice * buyQuantity;
            if (buyer.Balance < totalCost)
            {
                Console.WriteLine($"Insufficient funds to complete the purchase");
                return false;
            }

            byte* sellPtrBytte = (byte*)sellOrders;
            Order* matchedSellOrder = (Order*)(sellPtrBytte + (lowestSellIndex * sizeof(Order)));

            int matchedQuantity = Math.Min(buyQuantity, matchedSellOrder->Quantity);
            matchedSellOrder->Quantity -= matchedQuantity;

            buyer.Balance -= totalCost;

            Console.WriteLine($"Ordeer fulfilled: Buyer mmatch with Sell Order {matchedSellOrder->Id} at Price {matchedSellOrder->Price} with Quantity {matchedQuantity}");

            if(matchedSellOrder->Quantity == 0)
            {
                RemoveOrder(matchedSellOrder->Id);
            }

            return true;
        }

        public unsafe void PrintOrders()
        {
            Console.WriteLine($"Buy Orders:");
            for (int i = 0; i < size; i++)
            {
                if (buyOrders[i].Id != 0)
                    Console.WriteLine($"Id: {buyOrders[i].Id}, Price: {buyOrders[i].Price}, Quantity: {buyOrders[i].Quantity}");
            }
            Console.WriteLine($"Sell Orders:");
            for (int i = 0; i < size; i++)
            {
                if (sellOrders[i].Id != 0)
                    Console.WriteLine($"Id: {sellOrders[i].Id}, Price: {sellOrders[i].Price}, Quantity: {sellOrders[i].Quantity}");
            }
        }
    }
}