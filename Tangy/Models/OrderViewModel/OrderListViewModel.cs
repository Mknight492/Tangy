using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tangy.Models.OrderViewModel
{
    public class OrderListViewModel
    {
        public IList<PlacedOrderViewModel> Orders { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
