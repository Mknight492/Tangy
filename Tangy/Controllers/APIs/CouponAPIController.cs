using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tangy.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tangy.Controllers.APIs
{
    [Route("api/[controller]")]
    public class CouponAPIController : Controller
    {

        private readonly ApplicationDbContext _db;

        public CouponAPIController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(double orderTotal, string couponCode=null)
        {
            var rtn = "";
            if (couponCode==null) return Ok( orderTotal + ":E");
            var couponFromDb = _db.Coupons.SingleOrDefault(c => c.Name == couponCode);
            if (couponFromDb == null) return Ok(orderTotal + ":E");
            if (couponFromDb.MinimumAmount > orderTotal)
            {
                return Ok(orderTotal + ":E");

            }

            if (couponFromDb.CouponType == "0")
            {
                return Ok ((orderTotal*(1-couponFromDb.Discount/100)) +":S");
            }
            else
            {
                return Ok ((orderTotal - couponFromDb.Discount) + ":S");
            }
            
        }


    }
}
