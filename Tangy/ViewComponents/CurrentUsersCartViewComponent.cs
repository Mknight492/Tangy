using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;

namespace Tangy.ViewComponents
{
    public class CurrentUsersCartViewComponent:ViewComponent
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public CurrentUsersCartViewComponent(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);

            var shoppingCarts = await _db.ShoppingCarts.Where(s => s.ApplicationUserId == user.Id).ToListAsync();

            foreach (var shoppingCart in shoppingCarts)
            {
                shoppingCart.MenuItem = await _db.MenuItems.SingleOrDefaultAsync(m => m.Id == shoppingCart.MenuItemId);
            }

            return View(shoppingCarts);
        }

    }
}
