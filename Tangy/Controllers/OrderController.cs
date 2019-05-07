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
using Tangy.Models.OrderViewModel;
using Tangy.Services;
using Tangy.Utility;

namespace Tangy.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int PageSize = 2;
        private readonly IEmailSender _emailSender;

        public OrderController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        //GET Index
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var  user = await _userManager.GetUserAsync(User);

            var model = new PlacedOrderViewModel
            {
                OrderHeader = _db.OrderHeaders.FirstOrDefault(o => o.UserId == user.Id),                
            };
            model.OrdersDetails = _db.OrderDetails.Where(o => o.OrderId == model.OrderHeader.Id).ToList();

            return View(model);
        }
        
        //GET Confirm
        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new PlacedOrderViewModel
            {
                OrderHeader = _db.OrderHeaders.FirstOrDefault(o => o.Id == id && o.UserId==user.Id)
            };
            model.OrdersDetails = _db.OrderDetails.Where(o => o.OrderId == model.OrderHeader.Id).ToList();

            await _emailSender.SendOrderStatusAsync(user.Email, model.OrderHeader.Id.ToString(), SD.StatusSubmitted);

            return View(model);
        }

        //GET Order Histort
        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage=1)
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new OrderListViewModel()
            {
                Orders = new List<PlacedOrderViewModel>()
            };
            
            var OrderHeaders = _db.OrderHeaders.Where(o => o.UserId == user.Id).OrderByDescending(o=>o.PickupTime).ToList();

            foreach (var orderHeader in OrderHeaders)
            {
                var order = new PlacedOrderViewModel();
                order.OrderHeader = orderHeader;                
                order.OrdersDetails= _db.OrderDetails.Where(o => o.OrderId == order.OrderHeader.Id).ToList();
                model.Orders.Add(order);
            }

            var numberOfItem = model.Orders.Count;

            model.Orders = model.Orders
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            model.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = numberOfItem
            };

            return View(model);
        }

        //GET ManageOrders
        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> ManageOrders()
        {           

            var model = new List<PlacedOrderViewModel>();

            var OrderHeaders = _db.OrderHeaders.Where(o => o.Status == SD.StatusSubmitted || o.Status == SD.StatusInProcess).OrderByDescending(o => o.PickupTime).ToList();

            foreach (var orderHeader in OrderHeaders)
            {
                var order = new PlacedOrderViewModel();
                order.OrderHeader = orderHeader;
                order.OrdersDetails = _db.OrderDetails.Where(o => o.OrderId == order.OrderHeader.Id).ToList();
                model.Add(order);
            }

            return View(model);
        }

        //POST Cooking
        [Authorize(Roles = SD.AdminEndUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cooking(int id)
        {
            var order = await _db.OrderHeaders.FirstOrDefaultAsync(o => o.Id == id && o.Status==SD.StatusSubmitted);
            if (order == null) return NotFound();
            order.Status = SD.StatusInProcess;
            await _db.SaveChangesAsync();



            return RedirectToAction(nameof(ManageOrders));

        }

        //POST Prepared
        [Authorize(Roles = SD.AdminEndUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Prepared(int id)
        {
            var order = await _db.OrderHeaders.FirstOrDefaultAsync(o => o.Id == id && o.Status == SD.StatusInProcess);
            if (order == null) return NotFound();
            order.Status = SD.StatusReady;

            var user = await _userManager.GetUserAsync(User);
            await _emailSender.SendOrderStatusAsync(user.Email, order.Id.ToString(), SD.StatusReady);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(ManageOrders));

        }

        //POST Prepared
        [Authorize(Roles = SD.AdminEndUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _db.OrderHeaders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            order.Status = SD.StatusCancelled;
            await _db.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            await _emailSender.SendOrderStatusAsync(user.Email, order.Id.ToString(), SD.StatusCancelled);

            return RedirectToAction(nameof(ManageOrders));

        }

        //GET Pickup
        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> Pickup(string searchEmail =null, string searchPhone = null, string searchOrder = null, string searchName = null,int productPage = 1)
        {
            var model = new OrderListViewModel();
            model.Orders = new List<PlacedOrderViewModel>();

            var OrderHeaders = _db.OrderHeaders.Where(o => o.Status == SD.StatusReady).OrderByDescending(o => o.PickupTime).ToList();


            if ( searchOrder != null)
            {
                //filter by criteria
                //var unmatching = OrderHeaders.Where(o => o.Id != Convert.ToInt32(searchOrder)).ToList();
                //foreach (var orderHeader in unmatching)
                //{
                //    OrderHeaders.Remove(orderHeader);
                //}

             OrderHeaders = OrderHeaders
                 .Where(o => o.Id.ToString().Contains(searchOrder))
                 .OrderBy(o => o.Id.ToString().Length -searchOrder.Length)
                 .ToList();
     
                
                

            }

            if (searchEmail != null)
            {

                var users = _db.Users.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).ToList();
                var templist = new List<OrderHeader>();
                foreach (var orderHeader in OrderHeaders)
                {
                    foreach (var applicationUser in users)
                    {
                        if (applicationUser.Id == orderHeader.UserId) templist.Add(orderHeader);

                    }
                }
                OrderHeaders = templist;
            }

            if (searchPhone != null)
            {
                var users = _db.Users.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToList();
                var templist = new List<OrderHeader>();
                foreach (var orderHeader in OrderHeaders)
                {
                    foreach (var applicationUser in users)
                    {
                        if (applicationUser.Id == orderHeader.UserId) templist.Add(orderHeader);

                    }
                }
                OrderHeaders = templist;
            }

            if (searchName != null)
            {

                var users = _db.Users
                    .Where(u => u.FirstName.ToLower().Contains(searchName.ToLower()) 
                                || u.LastName.ToLower().Contains(searchName.ToLower()))
                    .ToList();
                var templist = new List<OrderHeader>();
                foreach (var orderHeader in OrderHeaders)
                {
                    foreach (var applicationUser in users)
                    {
                        if (applicationUser.Id == orderHeader.UserId) templist.Add(orderHeader);

                    }
                }
                OrderHeaders = templist;
            }


            foreach (var orderHeader in OrderHeaders)
            {
                var order = new PlacedOrderViewModel();
                order.OrderHeader = orderHeader;
                order.OrdersDetails = _db.OrderDetails.Where(o => o.OrderId == order.OrderHeader.Id).ToList();
                model.Orders.Add(order);
            }

            var numberOfItem = model.Orders.Count;

            model.Orders = model.Orders
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();


            model.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = numberOfItem
            };
            

            return View(model);
        }

        //GET OrderPickupDetails
        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> OrderPickupDetails(int Id)
        {
            var model = new PlacedOrderViewModel()
            {
                OrderHeader = _db.OrderHeaders.FirstOrDefault(o => o.Id == Id),
                OrdersDetails = _db.OrderDetails.Where(o => o.OrderId == Id).ToList(),              
            };
            model.OrderHeader.ApplicationUser = _db.Users.FirstOrDefault(u => u.Id == model.OrderHeader.UserId);

            return View(model);
        }

        //POST OrderPickupDetails
        [Authorize(Roles = SD.AdminEndUser)]
        [HttpPost, ActionName("OrderPickupDetails")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> OrderPickupDetailsPost(int Id)
        {
            var order = await _db.OrderHeaders.FirstOrDefaultAsync(o => o.Id == Id);
            if (order == null) return NotFound();
            order.Status = SD.StatusCompleted;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Pickup));
           

        }
    }
}