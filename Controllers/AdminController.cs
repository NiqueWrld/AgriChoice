using AgriChoice.Data;
using AgriChoice.Models;
using Braintree;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace AgriChoice.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AgriChoiceContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(AgriChoiceContext context , UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard
        public IActionResult Dashboard()
        {
            ViewData["TotalCows"] = _context.Cows.Count();
            ViewData["AvailableCows"] = _context.Cows.Count(c => c.IsAvailable);
            ViewData["TotalPurchases"] = _context.Purchases.Count();

            return View();
        }

        // Manage Cows - List all cows
        public async Task<IActionResult> ManageCows()
        {
            var cows = await _context.Cows.ToListAsync();
            return View(cows); // Return the list of cows to the Manage Cows view
        }

        // Add a Cow (GET)
        public IActionResult AddCow()
        {
            return View();
        }

        public async Task<IActionResult> Reports()
        {
            // Total Sales
            var totalSales = await _context.Purchases
                .Where(p => p.PaymentStatus == Purchase.Paymentstatus.Completed)
                .SumAsync(p => p.TotalPrice);

            // Total Refunds
            var totalRefunds = await _context.Transactions
                .Where(t => t.Type == Models.TransactionType.Credit && t.Description.Contains("Refund"))
                .SumAsync(t => t.Amount);

            // Driver Compensation
            var totalDriverCompensation = await _context.Transactions
                .Where(t => t.Type == Models.TransactionType.Credit && t.Description.Contains("Compensation"))
                .SumAsync(t => t.Amount);

            // Total Transactions
            var totalTransactions = await _context.Transactions.CountAsync();

            // Pass data to the view
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalRefunds = totalRefunds;
            ViewBag.TotalDriverCompensation = totalDriverCompensation;
            ViewBag.TotalTransactions = totalTransactions;

            return View();
        }


        // Add a Cow (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCow(Cow model, IFormFile ImageUrl)
        {
            // Handle image upload (optional)
            if (ImageUrl != null && ImageUrl.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    ImageUrl.CopyTo(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    string base64Image = Convert.ToBase64String(imageBytes);

                    // Optionally prepend the data URI scheme if you need to render it directly in HTML
                    model.ImageUrl = $"data:{ImageUrl.ContentType};base64,{base64Image}";
                }
            }

            model.IsAvailable = true;

            // Save the Cow to the database
            _context.Cows.Add(model);
            _context.SaveChanges();

            return RedirectToAction("ManageCows");
        }

        // Edit a Cow (GET)
        public async Task<IActionResult> EditCow(int id)
        {
            var cow = await _context.Cows.FindAsync(id);
            if (cow == null)
            {
                return NotFound();
            }
            return View(cow); // Return the Edit Cow form with the cow's data
        }

        public IActionResult ViewOrderDetails(int id)
        {
            var order = _context.Purchases
                .Include(o => o.RefundRequest)
                    .ThenInclude(r => r.RefundRequestCows)
                        .ThenInclude(rc => rc.Cow)
                .Include(o => o.Review)
                .Include(o => o.Delivery)
                .Include(o => o.PurchaseCows)
                    .ThenInclude(o => o.Cow)
                .FirstOrDefault(o => o.PurchaseId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRefund(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.RefundRequest)
                .Include(p => p.User) // Assuming there's a navigation property to IdentityUser
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null || purchase.RefundRequest == null)
            {
                return NotFound();
            }

            purchase.RefundRequest.Status = RefundRequest.Refundstatus.Approved;
            purchase.RefundRequest.PickedUp = false;

            await _context.SaveChangesAsync();

            var emailSender = new EmailSender();

            var email = purchase.User?.Email;
            var username = purchase.User?.UserName ?? "Customer";

            var emailSubject = "Refund Approved";
            var emailBody = $"Dear {username},\n\n" +
                            $"Your refund request for order ID {purchase.PurchaseId} has been approved. " +
                            $"The driver will come to pickup the cows.\n\n" +
                            $"Best regards,\nAgriChoice Team";

            if (!string.IsNullOrEmpty(email))
            {
                await emailSender.SendEmailAsync(
                    to: email,
                    subject: emailSubject,
                    body: emailBody
                );
            }

            return RedirectToAction(nameof(Purchases));
        }



        [HttpPost]
        public async Task<IActionResult> RejectRefund(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.RefundRequest)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null || purchase.RefundRequest == null)
            {
                return NotFound();
            }

            purchase.RefundRequest.Status = RefundRequest.Refundstatus.Rejected;
            await _context.SaveChangesAsync();

            var emailSender = new EmailSender();

            var email = purchase.User?.Email;
            var username = purchase.User?.UserName ?? "Customer";

            var emailSubject = "Refund Request Rejected";
            var emailBody = $"Dear {username},\n\n" +
                            $"We regret to inform you that your refund request for order ID {purchase.PurchaseId} has been rejected. " +
                            $"If you have any questions, please contact our support team.\n\nBest regards,\nAgriChoice Team";

            if (!string.IsNullOrEmpty(email))
            {
                await emailSender.SendEmailAsync(
                    to: email,
                    subject: emailSubject,
                    body: emailBody
                );
            }

            return RedirectToAction("ViewOrderDetails", new { id = purchase.PurchaseId });
        }


        // Edit a Cow (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCow(Cow model, IFormFile ImageUrl)
        {
            var existingCow = _context.Cows.FirstOrDefault(c => c.CowId == model.CowId);
            if (existingCow == null)
            {
                return NotFound();
            }

            // Update fields
            existingCow.Name = model.Name;
            existingCow.Gender = model.Gender;
            existingCow.Breed = model.Breed;
            existingCow.Age = model.Age;
            existingCow.Weight = model.Weight;
            existingCow.Price = model.Price;
            existingCow.Description = model.Description;
            existingCow.IsAvailable = model.IsAvailable;

            // Handle image upload (optional)
            if (ImageUrl != null && ImageUrl.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    ImageUrl.CopyTo(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    string base64Image = Convert.ToBase64String(imageBytes);

                    // Optionally prepend the data URI scheme if you need to render it directly in HTML
                    existingCow.ImageUrl = $"data:{ImageUrl.ContentType};base64,{base64Image}";
                }
            }

            _context.SaveChanges();
            return RedirectToAction("ManageCows");
        }

        // Delete a Cow (POST)
        [HttpPost, ActionName("DeleteCow")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCowConfirmed(int id)
        {
            var cow = await _context.Cows.FindAsync(id);
            if (cow != null)
            {
                _context.Cows.Remove(cow);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageCows));
        }

        // View Orders
        public async Task<IActionResult> Purchases()
        {
            // Retrieve all purchases with related cow and user information
            var purchases = await _context.Purchases
                 .Include(p => p.RefundRequest)
                .Include(p => p.Delivery)
                .Include(p => p.PurchaseCows)
                .ThenInclude(p => p.Cow)
                .Include(p => p.User)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            var freeDrivers = from user in _context.Users
                              join userRole in _context.UserRoles on user.Id equals userRole.UserId
                              join role in _context.Roles on userRole.RoleId equals role.Id
                              where role.Name == "Driver" &&
                                    !_context.Purchases.Any(purchase =>
                                        purchase.Delivery.DriverId == user.Id &&
                                        (purchase.DeliveryStatus != Purchase.Deliverystatus.Delivered || purchase.RefundRequest.DriverId == user.Id &&
                                         purchase.RefundRequest.Status != RefundRequest.Refundstatus.Returned))
                              select user;


            var freeDriversList = freeDrivers.ToList();

            // In your controller action
            var driverIds = purchases.Where(p => p.Delivery != null && !string.IsNullOrEmpty(p.Delivery.DriverId))
                                     .Select(p => p.Delivery.DriverId)
                                     .Distinct()
                                     .ToList();

            var driverUsernames = new Dictionary<string, string>();
            foreach (var driverId in driverIds)
            {
                var driver = await _userManager.FindByIdAsync(driverId);
                if (driver != null)
                {
                    driverUsernames[driverId] = driver.UserName;
                }
            }

            ViewBag.DriverUsernames = driverUsernames;

            ViewBag.Drivers = freeDriversList;

            return View(purchases);
        }

        public class AssignDriverRequest
        {
            public int PurchaseId { get; set; }
            public string DriverId { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> AssignDriver([FromBody] AssignDriverRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid data.");
            }

            var purchase = _context.Purchases
                .Include(p => p.Delivery)
                .FirstOrDefault(p => p.PurchaseId == request.PurchaseId);

            if (purchase == null || purchase.Delivery == null)
            {
                return NotFound("Purchase or delivery not found.");
            }

            var emailSender = new EmailSender();
            var user = await _context.Users.FindAsync(purchase.UserId);

            if (purchase.Delivery.DriverId != null && purchase.DeliveryStatus.ToString() == "Delivered")
            {
                var requestRefund = _context.RefundRequests
                    .FirstOrDefault(r => r.PurchaseId == purchase.PurchaseId);

                requestRefund.DriverId = request.DriverId;
                _context.SaveChanges();

                if (user != null)
                {
                    var email = user.Email;
                    var emailSubject = "Driver Assigned to Your Order";
                    var emailBody = $"Dear {user.UserName},\n\nA driver has been assigned to deliver your order with ID {purchase.PurchaseId}. You will be notified upon further updates.\n\nBest regards,\nAgriChoice Team";

                    await emailSender.SendEmailAsync(
                        to: email,
                        subject: emailSubject,
                        body: emailBody
                    );
                }

                return Ok("Driver assigned successfully.");
            }

            purchase.Delivery.DriverId = request.DriverId;
            _context.SaveChanges();
          
            if (user != null)
            {
                var email = user.Email;
                var emailSubject = "Driver Assigned to Your Order";
                var emailBody = $"Dear {user.UserName},\n\nA driver has been assigned to deliver your order with ID {purchase.PurchaseId}. You will be notified upon further updates.\n\nBest regards,\nAgriChoice Team";

                await emailSender.SendEmailAsync(
                    to: email,
                    subject: emailSubject,
                    body: emailBody
                );
            }

            return Ok("Driver assigned successfully.");
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

            if (request.Pin == purchase.Delivery.PickUpPin)
            {
                purchase.DeliveryStatus = Purchase.Deliverystatus.InTransit;
                purchase.Delivery.PickedUp = true;
                purchase.Delivery.PickupDate = DateTime.UtcNow;


                _context.SaveChanges();

                // Send email notification to the customer
                var emailSender = new EmailSender();
                var user = await _context.Users.FindAsync(purchase.UserId);

                if (user != null)
                {
                    var email = user.Email; // Assuming the email is stored in the IdentityUser object
                    var emailSubject = "Order In Transit";
                    var emailBody = $"Dear {user.UserName},\n\nYour order with ID {purchase.PurchaseId} is now in transit. Thank you for your patience.\n\nBest regards,\nAgriChoice Team";

                    await emailSender.SendEmailAsync(
                        to: email,
                        subject: emailSubject,
                        body: emailBody
                    );
                }

                return Json(new { success = true });
            
            
            
            }
            
            else if (purchase.RefundRequest != null && request.Pin == purchase.RefundRequest.DropOffPin)
            {
                purchase.RefundRequest.Status = RefundRequest.Refundstatus.Returned;

                var transaction = new Models.Transaction
                {
                    UserId = purchase.UserId,
                    Amount = purchase.TotalPrice - purchase.ShippingCost, // Refund amount
                    Type = Models.TransactionType.Credit,
                    Date = DateTime.UtcNow,
                    Description = $"Refund approved for Purchase ID: {purchase.PurchaseId}"
                };
                _context.Transactions.Add(transaction);

                if (!string.IsNullOrEmpty(purchase.RefundRequest.DriverId))
                {
                    var driverTransaction = new Models.Transaction
                    {
                        UserId = purchase.RefundRequest.DriverId,
                        Amount = purchase.ShippingCost * 0.8m,
                        Type = Models.TransactionType.Credit,
                        Date = DateTime.UtcNow,
                        Description = $"Compensation for refund pickup for Purchase ID: {purchase.PurchaseId}"
                    };
                    _context.Transactions.Add(driverTransaction);
                }

                _context.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        public class PinRequest
        {
            public int Pin { get; set; }
            public int PurchaseId { get; set; }
        }

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
