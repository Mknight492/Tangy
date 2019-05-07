using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tangy.Controllers.APIs
{
    [Route("api/[controller]")]
    public class IndexController : Controller 
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }


        // GET: api/<controller>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Get(int menuid, string type)
        {
            var shift  = (type == "flag") ? 1 :  -1;
            var user = await _userManager.GetUserAsync(User);
            var userCart = await _db.ShoppingCarts.FirstAsync(s => s.MenuItemId == menuid);
            ShoppingCart newCart;
            string returnCart = "";

            if (userCart != null)
            {
                userCart.Count += shift;
                userCart.MenuItem = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == menuid);
                returnCart = Newtonsoft.Json.JsonConvert.SerializeObject(userCart);
                if (userCart.Count == 0)
                {
                    
                    _db.Remove(userCart);
                }
                
            }
            else if(shift == 1)
            {
                newCart = new  ShoppingCart
                {
                    Id=menuid,
                    ApplicationUserId = user.Id,
                    Count = 1,
                    MenuItem = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == menuid)
                };
                _db.ShoppingCarts.Add(newCart);

                returnCart = Newtonsoft.Json.JsonConvert.SerializeObject(newCart);

            }

            await _db.SaveChangesAsync();



            return Ok(returnCart);
        }
    }
}
