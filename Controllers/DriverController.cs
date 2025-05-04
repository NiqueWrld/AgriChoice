using Microsoft.AspNetCore.Mvc;
using AgriChoice.Data;
using AgriChoice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static AgriChoice.Controllers.AdminController;

namespace AgriChoice.Controllers
{
    public class DriverController : Controller
    {
        private readonly AgriChoiceContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DriverController(AgriChoiceContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Action to display assigned jobs
        public async Task<IActionResult> AssignedJobs()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var deliveries = await _context.Purchases
    .Where(p => p.Delivery != null && p.Delivery.DriverId == user.Id  && p.DeliveryStatus != Purchase.Deliverystatus.Delivered)
    .Include(p => p.Delivery)
    .Include(p => p.PurchaseCows)
    .ThenInclude(pc => pc.Cow)
    .ToListAsync();

            return View(deliveries);
        }

        public async Task<IActionResult> PastJobs()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var deliveries = await _context.Purchases
    .Where(p => p.Delivery != null && p.Delivery.DriverId == user.Id && p.DeliveryStatus == Purchase.Deliverystatus.Delivered)
    .Include(p => p.Delivery)
    .Include(p => p.PurchaseCows)
    .ThenInclude(pc => pc.Cow)
    .ToListAsync();

            return View(deliveries);
        }
        


        public async Task<IActionResult> Wallet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var walletBalance = await _context.Purchases
                .Where(p => p.Delivery != null && p.Delivery.DriverId == user.Id)
                .Include(p => p.PurchaseCows)
                .ThenInclude(pc => pc.Cow)
                .SelectMany(p => p.PurchaseCows)
                .SumAsync(pc => pc.Cow.Price * 0.1m);

            ViewBag.WalletBalance = walletBalance;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ValidatePin([FromBody] PinRequest request)
        {

            var purchase = _context.Purchases
                .Include(p => p.Delivery)
                .FirstOrDefault(p => p.PurchaseId == request.PurchaseId);

            if (purchase == null)
            {
                return NotFound();
            }

            if (request.Pin == purchase.Delivery.DropOffPin)
            {
                purchase.DeliveryStatus = Purchase.Deliverystatus.Delivered;
                purchase.Delivery.DeliveryCompletedDate = DateTime.UtcNow;
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

    }
}