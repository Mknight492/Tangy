using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.SubCategoriesViewModels;
using Tangy.Utility;

namespace Tangy.Controllers
{


    [Authorize(Roles = SD.AdminEndUser)]
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index()
        {

            var subcategories = _db.SubCategories.Include(x => x.Category).OrderBy(x => x.Category);
            return View(await subcategories.ToListAsync());
        }

        //GET Action Create
        public IActionResult Create()
        {
            var model = new SubCategoryandCategoryViewModel
            {
                CategoryList = _db.Categories,
                SubCategory = new SubCategory(),
                SubCategoryList = _db.SubCategories.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToList()

            };
            return View(model);

        }


        //POST Action Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryandCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {

                var doesSubCategroyExist = _db.SubCategories.Where(x => x.Name == model.SubCategory.Name).Count();
                var doesSubCatandCatExist = _db.SubCategories.Where(x =>
                    x.CategoryId == model.SubCategory.CategoryId && x.Name == model.SubCategory.Name).Count();

                if (doesSubCategroyExist > 0 && model.isNew)
                {
                    //error message
                    StatusMessage = "Error : Sub Catergory Name Already Exists";

                }
                else
                {
                    if (doesSubCategroyExist == 0 && !model.isNew)
                    {
                        //error message
                        StatusMessage = "Error : Sub Catergory Does Not Exist";

                    }
                    else
                    {
                        if (doesSubCatandCatExist > 0)
                        {
                            //error message
                            StatusMessage = "Error : Sub Catergory And Catergory Already Exist";

                        }
                        else
                        {                            
                            _db.Add(model.SubCategory);
                            await _db.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            var model2 = new SubCategoryandCategoryViewModel
            {
                CategoryList = _db.Categories,
                SubCategory = model.SubCategory,
                SubCategoryList = _db.SubCategories.OrderBy(x => x.Name).Select(x => x.Name).ToList(),
                StatusMessage = StatusMessage

            };
            return View(model2);
   
        }


        //GET Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCat = _db.SubCategories.SingleOrDefault(x => x.Id == id);
            if (subCat == null)
            {
                return NotFound();
            }

            var model = new SubCategoryandCategoryViewModel
            {
                SubCategory = subCat,
                CategoryList = _db.Categories.ToList(),
                SubCategoryList = _db.SubCategories.Select(x=>x.Name).Distinct().ToList()

            };
            return View(model);
        }



        //POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryandCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategroyExist = _db.SubCategories.Where(x => x.Name == model.SubCategory.Name).Count();
                var doesSubCatandCatExist = _db.SubCategories.Where(x =>
                    x.CategoryId == model.SubCategory.CategoryId && x.Name == model.SubCategory.Name).Count();

                if (doesSubCategroyExist == 0)
                {
                    StatusMessage = "Error : Subcatergory does not exist";
                }
                else
                {
                    if (doesSubCatandCatExist > 0)
                    {
                        StatusMessage = "Error : Cat and SubCat combination already Exists";
                    }
                    else
                    {
                        var subcatfromdb = _db.SubCategories.SingleOrDefault(x => x.Id == id);
                        subcatfromdb.Name = model.SubCategory.Name;
                        subcatfromdb.CategoryId = model.SubCategory.Id;
                        await _db.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }

            }
            var model2 = new SubCategoryandCategoryViewModel
            {
                CategoryList = _db.Categories,
                SubCategory = model.SubCategory,
                SubCategoryList = _db.SubCategories.OrderBy(x => x.Name).Select(x => x.Name).ToList(),
                StatusMessage = StatusMessage

            };
            return View(model2);
        }

        //GET Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCat = _db.SubCategories.Include(x=>x.Category).SingleOrDefault(x => x.Id == id);
            if (subCat == null)
            {
                return NotFound();
            }
            return View(subCat);
        }


        //GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCat = _db.SubCategories.Include(x => x.Category).SingleOrDefault(x => x.Id == id);
            if (subCat == null)
            {
                return NotFound();
            }
            return View(subCat);
        }

        //POST Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCat = await _db.SubCategories.SingleOrDefaultAsync(x => x.Id == id);
            if (subCat == null)
            {
                return NotFound();
            }

            _db.SubCategories.Remove(subCat);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}