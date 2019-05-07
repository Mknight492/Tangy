using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.HomeViewModel;

namespace Tangy.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext db,UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        //GET Index Home
        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                MenuItems = await _db.MenuItems.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync(),
                Coupon = await _db.Coupons.Where(c=>c.IsActive).ToListAsync(),
                Categories = await _db.Categories.OrderBy(c=>c.DisplayOrder).ToListAsync()
            };
            return View(model);
        }

        //GET Details Home
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var menuItemInDb = await _db.MenuItems.Include(m=>m.Category).Include(m=>m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            if (menuItemInDb == null) return NotFound();
            var model = new ShoppingCart
            {
                MenuItem = menuItemInDb,
                ApplicationUserId = _userManager.GetUserId(User),
                MenuItemId = menuItemInDb.Id
            };
            return View(model);
        }

        //POST Details Home
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart model)
        {
            model.Id = 0;
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Details), new {id=model.MenuItemId});
            }

            var cartInDb = await _db.ShoppingCarts.Where(c =>
                c.MenuItemId == model.MenuItemId && c.ApplicationUserId == _userManager.GetUserId(User)).SingleOrDefaultAsync();
            if (cartInDb == null)
            {
                _db.ShoppingCarts.Add(model);
            }
            else
            {
                cartInDb.Count += model.Count;
            } 
            await _db.SaveChangesAsync();

            var list = _db.ShoppingCarts.Where(c => c.ApplicationUserId == model.ApplicationUserId).ToList();
            int totalCount = 0;
            foreach (var shoppingCart in list)
            {
                totalCount += shoppingCart.Count;
            }
            HttpContext.Session.SetInt32("CartCount",totalCount);
            return RedirectToAction(nameof(Index));

        }



        public async Task<IActionResult> Plus(int? cartId = null, int? itemId = null)
        {
            if (cartId != null)
            {
                var cartInDb = await _db.ShoppingCarts.SingleOrDefaultAsync(c => c.Id == cartId);

                cartInDb.Count++;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            if (itemId != null)
            {
                var user = await _userManager.GetUserAsync(User);
                var menuItem = _db.MenuItems.SingleOrDefaultAsync(m => m.Id == itemId);
                var usersCarts = await _db.ShoppingCarts.Where(s => s.ApplicationUserId == user.Id).ToListAsync();
                foreach (var shoppingCart in usersCarts)
                {
                    shoppingCart.MenuItem =
                        await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == shoppingCart.MenuItemId);
                    if (shoppingCart.MenuItemId == itemId)
                    {
                        shoppingCart.Count++;
                        await _db.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }

                var newCart = new ShoppingCart
                {

                    ApplicationUserId = user.Id,
                    Count = 1,
                    MenuItemId = (int) itemId,

                };
                await _db.ShoppingCarts.AddAsync(newCart);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }



        

        public async Task<IActionResult> Minus(int? cartId = null, int? itemId = null)
        {
            if (cartId != null)
            {
                var cartInDb = await _db.ShoppingCarts.SingleOrDefaultAsync(c => c.Id == cartId);
                if (cartInDb.Count != 1) cartInDb.Count--;
                else
                {
                    _db.ShoppingCarts.Remove(cartInDb);
                }
            }
            else if (itemId != null)
            {
                var user = await _userManager.GetUserAsync(User);
                var usersCarts = await _db.ShoppingCarts.Where(s => s.ApplicationUserId == user.Id).ToListAsync();
                foreach (var shoppingCart in usersCarts)
                {
                    if (shoppingCart.MenuItemId == itemId)
                    {
                        shoppingCart.Count--;
                        if (shoppingCart.Count == 0)
                        {
                            _db.ShoppingCarts.Remove(shoppingCart);
                        }
                        await _db.SaveChangesAsync();                       
                    }
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
