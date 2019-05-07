using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.MenuItemViewModels;
using Tangy.Utility;

namespace Tangy.Controllers
{
    [Authorize(Roles = SD.AdminEndUser)]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemViewModel { get; set; }

        public MenuItemController(ApplicationDbContext db, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemViewModel = new MenuItemViewModel
            {
                Categories = _db.Categories.ToList(),
                MenuItem = new MenuItem()
            };
        }


        //Get Index
        public async Task<IActionResult> Index()
        {
            var menuitems = _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory);
            return View(await menuitems.ToListAsync());
        }
        //GET Create
        public IActionResult Create()
        {
            
            return View(MenuItemViewModel);
        }

        //POST Creat
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(MenuItemViewModel);
            }              
            _db.MenuItems.Add(MenuItemViewModel.MenuItem);
            await _db.SaveChangesAsync();

            //Image Being Saved
            var webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = _db.MenuItems.Find(MenuItemViewModel.MenuItem.Id);
            if (files[0] != null && files[0].Length > 0)
            {
                //when user uploads an images
                var uploads = Path.Combine(webRootPath, "images");
                var extension = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."),
                    files[0].FileName.Length - files[0].FileName.LastIndexOf("."));
                using (var filestream = new FileStream(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension),
                    FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                menuItemFromDb.Image = @"\images\" +MenuItemViewModel.MenuItem.Id + extension;
            }
            else
            {
                //when user doesn't upload an image
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemViewModel.MenuItem.Id + @".png");
                menuItemFromDb.Image = @"\images\" + MenuItemViewModel.MenuItem.Id + @".png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<JsonResult> GetSubCategory(int CategoryId)
        {
            List<SubCategory> SubCategories =
                await _db.SubCategories.Where(x => x.CategoryId == CategoryId).ToListAsync();
            return Json(new SelectList(SubCategories, "Id", "Name"));
        }

        //GEt Edit Menu Item
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();


            MenuItemViewModel.MenuItem =await _db.MenuItems
                .Include(m=>m.Category)
                .Include(m=>m.SubCategory)
                .FirstOrDefaultAsync(x => x.Id == id);

            MenuItemViewModel.SubCategories = _db.SubCategories
                .Where(m => m.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToList();

            if (MenuItemViewModel.MenuItem == null) return NotFound();

            return View(MenuItemViewModel);

        }

        //POST Edit Menu Item
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (id != MenuItemViewModel.MenuItem.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {


                    var webRootPath = _hostingEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    var menuItemInDb = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
                    if (menuItemInDb == null) return NotFound();
                    if (files[0].Length > 0 && files[0] != null)
                    {
                        //if use uploads a new image
                        var uploads = Path.Combine(webRootPath, "images");
                        var extension_new = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."),
                            files[0].FileName.Length - files[0].FileName.LastIndexOf("."));
                        var extension_old = menuItemInDb.Image.Substring(menuItemInDb.Image.LastIndexOf("."),
                            menuItemInDb.Image.Length - menuItemInDb.Image.LastIndexOf("."));
                        if (System.IO.File.Exists(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension_old)))
                        {
                            System.IO.File.Delete(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension_old));
                        }

                        using (var filestream = new FileStream(
                            Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension_new),
                            FileMode.Create))
                        {
                            files[0].CopyTo(filestream);
                        }

                        menuItemInDb.Image = @"\images\" + MenuItemViewModel.MenuItem.Id + extension_new;
                    }

                    menuItemInDb.Name = MenuItemViewModel.MenuItem.Name;
                    menuItemInDb.Description = MenuItemViewModel.MenuItem.Description;
                    menuItemInDb.Price = MenuItemViewModel.MenuItem.Price;
                    menuItemInDb.Spicyness = MenuItemViewModel.MenuItem.Spicyness;
                    menuItemInDb.CategoryId = MenuItemViewModel.MenuItem.CategoryId;
                    menuItemInDb.SubCategoryId = MenuItemViewModel.MenuItem.SubCategoryId;
                    await _db.SaveChangesAsync();
                }
                catch(Exception Ex)
                {
                    
                }
                return RedirectToAction(nameof(Index));
            }
            MenuItemViewModel.SubCategories = _db.SubCategories
                .Where(m => m.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToList();
            return View(MenuItemViewModel);
            
        }

        //Get Details MenuItem
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            MenuItemViewModel.MenuItem = await _db.MenuItems.Include(x=>x.Category).Include(x=>x.SubCategory).FirstOrDefaultAsync(x => x.Id == id);
            if (MenuItemViewModel.MenuItem == null) return NotFound();
            return View(MenuItemViewModel);
        }



        //GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            MenuItemViewModel.MenuItem = await _db.MenuItems.Include(x => x.Category).Include(x => x.SubCategory).FirstOrDefaultAsync(x => x.Id == id);
            if (MenuItemViewModel.MenuItem == null) return NotFound();
            return View(MenuItemViewModel);
        }

        //POST Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            
            var menuItemInDb = await _db.MenuItems.FirstOrDefaultAsync(x=>x.Id==id);

            if (menuItemInDb == null) return NotFound();

            
            var extension= menuItemInDb.Image.Substring(menuItemInDb.Image.LastIndexOf("."),
                menuItemInDb.Image.Length - menuItemInDb.Image.LastIndexOf("."));
            var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images",(menuItemInDb.Id + extension) );
            

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            var orders = _db.ShoppingCarts.Where(s => s.MenuItemId == id).ToList();
            foreach (var shoppingCart in orders)
            {
                _db.ShoppingCarts.Remove(shoppingCart);
            }

            

            _db.MenuItems.Remove(menuItemInDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}