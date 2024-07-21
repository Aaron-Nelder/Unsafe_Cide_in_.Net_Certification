using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingPlatform
{
    public struct Order
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public bool IsBuyOrder { get; set; }
    }
}
