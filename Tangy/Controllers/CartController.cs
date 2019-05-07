using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Extensions;
using Tangy.Models;
using Tangy.Models.OrderViewModel;
using Tangy.Utility;

namespace Tangy.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public OrderViewModel CartModel { get; set;}
        
        public CartController(ApplicationDbContext db,UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userid = _userManager.GetUserId(User);
            CartModel = new OrderViewModel()
            {
                OrderHeader = new OrderHeader(),
                ShoppingCarts = await _db.ShoppingCarts.Where(s => s.ApplicationUserId == userid).ToListAsync()

            };


            foreach (var shoppingCart in CartModel.ShoppingCarts)
            {
                shoppingCart.MenuItem = _db.MenuItems.FirstOrDefault(m => m.Id == shoppingCart.MenuItemId);
                if (shoppingCart.MenuItem != null)
                { 
                CartModel.OrderHeader.Total += shoppingCart.MenuItem.Price * shoppingCart.Count;
                    if (shoppingCart.MenuItem.Description.Length > 100)
                    {
                        shoppingCart.MenuItem.Description =
                            shoppingCart.MenuItem.Description.Substring(0, 100) + "...";
                    }
                }
                
            }

            return View(CartModel);

        }

        //POST Index
        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IndexPost()
        {
            var userid = _userManager.GetUserId(User);

           CartModel.ShoppingCarts =
                    await _db.ShoppingCarts.Where(s => s.ApplicationUserId == userid).ToListAsync();


            //Orderheader
            var orderHeader = new OrderHeader
            {
                UserId = userid,
                OrderTime = DateTime.Now,
                PickupTime = CartModel.OrderHeader.PickupTime,
                CouponCode = CartModel.OrderHeader.CouponCode,
                Comments = CartModel.OrderHeader.Comments,
                Status = SD.StatusSubmitted,
                Total = CartModel.OrderHeader.Total
            };
            await _db.OrderHeaders.AddAsync(orderHeader);

            //Orderdetails
            foreach (var shoppingcart in CartModel.ShoppingCarts)
            {
                shoppingcart.MenuItem = _db.MenuItems.FirstOrDefault(m => m.Id == shoppingcart.MenuItemId);
                if (shoppingcart.MenuItem.Description.Length > 100)
                {
                    shoppingcart.MenuItem.Description =
                        shoppingcart.MenuItem.Description.Substring(0, 100) + "...";
                }

                var order = new OrderDetails
                {
                    OrderId = orderHeader.Id,                   
                    MenuItemId = shoppingcart.MenuItemId,
                    Count = shoppingcart.Count,
                    Name = shoppingcart.MenuItem.Name,
                    Description = shoppingcart.MenuItem.Description,
                    Price = shoppingcart.MenuItem.Price
                };
                if(order.Count>0) _db.OrderDetails.Add(order);
                _db.ShoppingCarts.Remove(shoppingcart);
            }

           await _db.SaveChangesAsync();

           return RedirectToAction( "Confirm", "Order", new {id=orderHeader.Id});
        }




        //POST Plus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Plus(int cartId)
        {

            var cartInDb = await _db.ShoppingCarts.SingleOrDefaultAsync(c => c.Id == cartId);

            cartInDb.Count++;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cartInDb = await _db.ShoppingCarts.SingleOrDefaultAsync(c => c.Id == cartId);
            if(cartInDb.Count !=0) cartInDb.Count--;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }





    }
}