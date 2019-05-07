using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tangy.Models.OrderViewModel
{
    public class PlacedOrderViewModel
    {
        public OrderHeader OrderHeader { get; set; }
        public List<OrderDetails> OrdersDetails { get; set; }
    }
}
