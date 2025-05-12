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
                .Where(p => p.Delivery != null && p.Delivery.DriverId == user.Id && p.DeliveryStatus != Purchase.Deliverystatus.Delivered  || p.RefundRequest.DriverId == user.Id && p.RefundRequest.Status != RefundRequest.Refundstatus.Returned)
                .Include(p => p.User)
                .Include(p => p.Delivery)
                .Include(p => p.RefundRequest)
                .Include(p => p.PurchaseCows)
                .ThenInclude(pc => pc.Cow)
                .ToListAsync();

            return View(deliveries);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartJob(int purchaseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Delivery)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.Delivery.DriverId == user.Id);

            if (purchase == null || purchase.DeliveryStatus != Purchase.Deliverystatus.Scheduled)
            {
                return NotFound(new { success = false, message = "Job not found or already started." });
            }

            purchase.DeliveryStatus = Purchase.Deliverystatus.InTransit;
            purchase.Delivery.PickupDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("AssignedJobs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineJob(int purchaseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Delivery)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId && p.Delivery.DriverId == user.Id);

            if (purchase == null || purchase.DeliveryStatus != Purchase.Deliverystatus.Scheduled)
            {
                return NotFound(new { success = false, message = "Job not found or cannot be declined." });
            }

            purchase.Delivery.DriverId = null; 
            purchase.DeliveryStatus = Purchase.Deliverystatus.Scheduled;

            await _context.SaveChangesAsync();

            return RedirectToAction("AssignedJobs");
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
                .Include(p => p.RefundRequest)
                .Include(p => p.PurchaseCows)
                .ThenInclude(pc => pc.Cow)
                .ToListAsync();

            return View(deliveries);
        }

        public async Task<IActionResult> Wallet()
        {
            var userId = _userManager.GetUserId(User);

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            ViewBag.Balance = transactions.Sum(t => t.Amount);

            return View(transactions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidatePin([FromBody] PinRequest request)
        {
            var purchase = _context.Purchases
                .Include(p => p.RefundRequest)
                .Include(p => p.Delivery)
                .FirstOrDefault(p => p.PurchaseId == request.PurchaseId);

            if (purchase == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(purchase.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var emailSender = new EmailSender();
            var email = user.Email;

            if (request.Pin == purchase.Delivery?.DropOffPin)
            {
                purchase.DeliveryStatus = Purchase.Deliverystatus.Delivered;
                purchase.Delivery.DeliveryCompletedDate = DateTime.UtcNow;
                _context.SaveChanges();

                var emailSubject = "Order Delivered Successfully";
                var emailBody = $"Dear {user.UserName},\n\nYour order (ID: {purchase.PurchaseId}) has been delivered successfully.\nThank you for shopping with AgriChoice!\n\nBest regards,\nAgriChoice Team";

                await emailSender.SendEmailAsync(email, emailSubject, emailBody);
                return Json(new { success = true });
            }

            if (request.Pin == purchase.RefundRequest?.PickOffPin)
            {
                purchase.RefundRequest.PickedUp = true;
                purchase.RefundRequest.CollectionCompletedDate = DateTime.UtcNow;
                _context.SaveChanges();

                var emailSubject = "Refund Item Picked Up";
                var emailBody = $"Dear {user.UserName},\n\nYour refund item for order ID {purchase.PurchaseId} has been picked up successfully. We will notify you once it's processed.\n\nBest regards,\nAgriChoice Team";

                await emailSender.SendEmailAsync(email, emailSubject, emailBody);
                return Json(new { success = true });
            }

            if (request.Pin == purchase.RefundRequest?.DropOffPin)
            {
                purchase.RefundRequest.CollectionCompletedDate = DateTime.UtcNow;
                _context.SaveChanges();

                var emailSubject = "Refund Item Delivered";
                var emailBody = $"Dear {user.UserName},\n\nYour refund item for order ID {purchase.PurchaseId} has been successfully delivered to us. We will begin processing your refund shortly.\n\nBest regards,\nAgriChoice Team";

                await emailSender.SendEmailAsync(email, emailSubject, emailBody);
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

    }
}