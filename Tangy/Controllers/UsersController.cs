using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Utility;

namespace Tangy.Controllers
{
    [Authorize(Roles = SD.AdminEndUser)]
    public class UsersController : Controller
    {
        private ApplicationDbContext _db;
        private UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        //GET Index
        public async Task<IActionResult> Index()
        {
           
            var user = await _userManager.GetUserAsync(User);

            var users = _db.Users.Where(u => u != user);

            return View(users);
        }


        //GET Edit
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var userInDb = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            return View(userInDb);
        }


        //POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user)
        {
            var userInDb = await _db.Users.SingleOrDefaultAsync(u => u.Id == user.Id);
            if (!ModelState.IsValid) return View(user);
            if (userInDb == null ) return NotFound();

            userInDb.FirstName = user.FirstName;
            userInDb.LastName = user.LastName;
            userInDb.PhoneNumber = user.PhoneNumber;
            userInDb.LockoutReason = user.LockoutReason;
            userInDb.LockoutEnd = user.LockoutEnd;
            userInDb.AccessFailedCount = user.AccessFailedCount;
            


            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET Delete
        public async Task<IActionResult> LockUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var userInDb = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            return View(userInDb);
        }

        //POST LockUser

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(ApplicationUser user)
        {
            var userInDb = await _db.Users.SingleOrDefaultAsync(u => u.Id == user.Id);
            if (!ModelState.IsValid) return View(user);
            if (userInDb == null) return NotFound();


            userInDb.LockoutEnd = DateTime.Today.AddYears(100);
            userInDb.AccessFailedCount = int.MaxValue;


            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



    }
}