using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Utility;

namespace Tangy.Controllers
{
    [Authorize(Roles = SD.AdminEndUser)]
    public class CouponsController : Controller
    {
        public ApplicationDbContext _db;

        public CouponsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async  Task<IActionResult> Index()
        {
            var coupons = await _db.Coupons.ToListAsync();
            return View(coupons);
        }

        //GET Create
        public IActionResult Create()
        {
            return View();
        }

        //POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null && files[0].Length != 0)
                {
                    byte[] p1;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }

                    coupon.Picture = p1;
                        _db.Coupons.Add(coupon);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(coupon);
        }

        //GET Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var couponinDB = await _db.Coupons.FirstOrDefaultAsync(x => x.Id == id);
            if (couponinDB == null) return NotFound();
            return View(couponinDB);
        }


        //POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            if (id != coupon.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                return View(coupon);
            }

            var couponInDb =await _db.Coupons.FirstOrDefaultAsync(x => x.Id == id);
            var files = HttpContext.Request.Form.Files;
            if (files[0] != null && files[0].Length > 0)
            {
                //user has added a new file.
                byte[] p1;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }

                    couponInDb.Picture = p1;
                }
            }

            couponInDb.Name = coupon.Name;
            couponInDb.CouponType = coupon.CouponType;
            couponInDb.Discount = coupon.Discount;
            couponInDb.MinimumAmount = coupon.MinimumAmount;
            couponInDb.IsActive = coupon.IsActive;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET Delete Coupon
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var couponinDB = await _db.Coupons.FirstOrDefaultAsync(x => x.Id == id);
            if (couponinDB == null) return NotFound();
            return View(couponinDB);
        }

        //POST Delete Coupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, Coupon coupon)
        {
            if (id != coupon.Id) return NotFound();
            var couponInDb = await _db.Coupons.SingleOrDefaultAsync(x => x.Id == id);
            if (couponInDb == null) return NotFound();

            _db.Coupons.Remove(couponInDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }

}