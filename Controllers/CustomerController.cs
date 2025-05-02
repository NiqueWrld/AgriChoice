using Microsoft.AspNetCore.Mvc;
using AgriChoice.Data; // Replace with your actual namespace
using AgriChoice.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AgriChoice.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AgriChoiceContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CustomerController(AgriChoiceContext context , UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Customer/BrowseCows
        public IActionResult BrowseCows(string search, string availability)
        {
            var cows = _context.Cows.AsQueryable();

            // Filter by search term
            if (!string.IsNullOrEmpty(search))
            {
                cows = cows.Where(c => c.Breed.Contains(search) || c.Description.Contains(search));
            }

            // Filter by availability
            if (!string.IsNullOrEmpty(availability))
            {
                bool isAvailable = availability == "true";
                cows = cows.Where(c => c.IsAvailable == isAvailable);
            }

            return View(cows.ToList());
        }

        public IActionResult MyOrders(int id)
        {
            var currentUserName = User.Identity.Name;
            var orders = _context.Purchases
                .Include(o => o.Cow)
                .Where(o => o.User.UserName == currentUserName)
                .ToList();

            return View(orders);
        }

        public IActionResult MyOrders2(int id)
        {
            var order = _context.Purchases.FirstOrDefault(o => o.PurchaseId == id);
            if (order == null || order.User.UserName != User.Identity.Name)
            {
                return Unauthorized();
            }

            return View(order);
        }

        public IActionResult PurchaseCow(int id)
        {
            var cow = _context.Cows.FirstOrDefault(c => c.CowId == id);
            if (cow == null || !cow.IsAvailable)
            {
                return NotFound();
            }

            var model = new PurchaseViewModel
            {
                Cow = cow
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPurchase(int id)
        {
         

            var currentUserId = _userManager.GetUserId(User);
            var cow = _context.Cows.FirstOrDefault(c => c.CowId == id);

            if (cow == null || !cow.IsAvailable)
            {
                return NotFound();
            }

            var purchase = new Purchase
            {
                CowId = cow.CowId,
                UserId = currentUserId,
                PurchaseDate = DateTime.UtcNow,
                PaymentStatus = Purchase.Paymentstatus.Pending,
                DeliveryStatus = Purchase.Deliverystatus.Scheduled
            };

            _context.Purchases.Add(purchase);
            cow.IsAvailable = false; // Optional: Mark cow as sold
            await _context.SaveChangesAsync();

            return RedirectToAction("MyOrders");
        }



        // GET: Customer/ViewCow/{id}
        public IActionResult ViewCow(int id)
        {
            var cow = _context.Cows.FirstOrDefault(c => c.CowId == id);
            if (cow == null)
            {
                return NotFound();
            }

            return View(cow);
        }
    }
}