using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tangy.Models.HomeViewModel
{
    public class HomeViewModel
    {
        public IEnumerable<MenuItem> MenuItems { get; set; }
        
        public IEnumerable<Category> Categories { get; set; }
        
        public IEnumerable<Coupon> Coupon { get; set; }

        public string StatusMessage { get; set; }

    }
}
