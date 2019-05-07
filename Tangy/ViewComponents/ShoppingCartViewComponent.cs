using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tangy.Data;
using Tangy.Models;

namespace Tangy.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public ShoppingCartViewComponent(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int totalCount = 0;

            var user = _userManager.GetUserId(UserClaimsPrincipal);
            var list = _db.ShoppingCarts.Where(c => c.ApplicationUserId == user).ToList();
            foreach (var shoppingCart in list)
            {
                totalCount += shoppingCart.Count;
            }   
            return View(totalCount);
        }
    }
}
