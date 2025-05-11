using Microsoft.AspNetCore.Mvc;
using AgriChoice.Data; // Replace with your actual namespace
using AgriChoice.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Azure.Core;
using System.Security.Claims;
using System;
using Braintree;
using AgriChoice.Services;
using Microsoft.AspNetCore.Authorization;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace AgriChoice.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly AgriChoiceContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private PaymentService _braintreeService;

        public CustomerController(AgriChoiceContext context , UserManager<IdentityUser> userManager , PaymentService braintreeService)
        {
            _context = context;
            _userManager = userManager;
            _braintreeService = braintreeService;
        }

        public async Task<IActionResult> GetClientToken()
        {
            var gateway = _braintreeService.GetGateway();
            var clientToken = await gateway.ClientToken.GenerateAsync();
            return Json(new { clientToken });
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

        public IActionResult MyOrders()
        {
            var currentUserName = User.Identity.Name;
            var orders = _context.Purchases
                .Include(o => o.PurchaseCows)
                .ThenInclude(o => o.Cow)
                .Where(o => o.User.UserName == currentUserName)
                .OrderByDescending(o => o.PurchaseDate)
                .ToList();

            return View(orders);
        }

        public IActionResult ViewOrderDetails(int id)
        {
            var currentUserName = User.Identity.Name;

            var order = _context.Purchases
               .Include(o => o.RefundRequest)
               .Include(o => o.Review)
               .Include(o => o.Delivery)
               .Include(o => o.PurchaseCows)
               .ThenInclude(o => o.Cow)
               .FirstOrDefault(o => o.User.UserName == currentUserName && o.PurchaseId == id);

            if (order == null)
            {
                return NotFound();
            }
   
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> RequestRefund(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }
            return View(purchase); 
        }

        [HttpPost]
        public async Task<IActionResult> RequestRefund(RefundRequest model, IFormFile uploadedFile)
        {
            var currentUserId = _userManager.GetUserId(User);

            var order = await _context.Purchases
                .FirstOrDefaultAsync(o => o.PurchaseId == model.PurchaseId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    uploadedFile.CopyTo(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    string base64Image = Convert.ToBase64String(imageBytes);

                    // Optionally prepend the data URI scheme if you need to render it directly in HTML
                    model.UploadedFileUrl = $"data:{uploadedFile.ContentType};base64,{base64Image}";
                }
            }

            Random random = new Random();
            model.UserId = currentUserId;
            model.RequestedAt = DateTime.UtcNow;
            model.DropOffPin = random.Next(1000, 10000);
            model.PickOffPin = random.Next(1000, 10000);
            model.Status = RefundRequest.Refundstatus.Pending;

            _context.RefundRequests.Add(model);
            await _context.SaveChangesAsync();

            order.RefundRequest = model;
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewOrderDetails", new { id = model.PurchaseId });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReview(int purchaseId, string reviewContent)
        {
            if (string.IsNullOrWhiteSpace(reviewContent))
            {
                return BadRequest("Review content cannot be empty.");
            }

            var order = await _context.Purchases.FindAsync(purchaseId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var currentUserId = _userManager.GetUserId(User);

            var review = new Review
            {
                UserId = currentUserId,
                Content = reviewContent,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync(); // Save to generate ReviewId

            // Now associate review with the order
            order.ReviewId = review.ReviewId;
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewOrderDetails", new { id = purchaseId });
        }
        public IActionResult Checkout()
        {
            var currentUserId = _userManager.GetUserId(User);

            var cart = _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Cow)
                    .FirstOrDefault(c => c.UserId == currentUserId);

            ViewBag.Adress = cart?.DeliveryAddress ?? "undefined";

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                // Find the cow by ID
                var cow = await _context.Cows.FirstOrDefaultAsync(c => c.CowId == id);

                if (cow == null || !cow.IsAvailable)
                {
                    return NotFound();
                }

                // Find or create the user's cart
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(c => c.Cow)
                    .FirstOrDefaultAsync(c => c.UserId == currentUserId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = currentUserId,
                        DeliveryAddress = "undefined",
                        DateCreated = DateTime.UtcNow,
                        Items = new List<CartItem>()
                    };

                    _context.Carts.Add(cart);
                     await _context.SaveChangesAsync();
                    return Json(new { success = false, triggerModal = true, message = "Please provide a delivery address to create your cart." });
                }

                // Check if the item is already in the cart
                if (cart.Items.Any(i => i.CowId == id))
                {
                    return Json(new { success = false, message = "This cow is already in your cart." });
                }

                // Check if cart already has 4 items
                if (cart.Items.Count >= 4)
                {
                    return Json(new { success = false, message = "You cannot add more than 4 items to your cart." });
                }

                // Add the cow to the cart as a CartItem
                var cartItem = new CartItem
                {
                    CowId = cow.CowId,
                    Cart = cart,
                    DateAdded = DateTime.UtcNow,
                    Cow = cow
                };

                cart.Items.Add(cartItem);

                cart.SubTotal = cart.Items.Sum(item => item.Cow.Price);
                cart.ShippingCost = await cart.CalculateShippingCostAsync();
                cart.TotalCost = cart.SubTotal + cart.ShippingCost;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();
            }

            return Json(new { success = true, message = "Cow added to cart successfully!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDeliveryAddress(string deliveryAddress)
        {
            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                return Json(new { success = false, message = "Delivery address cannot be empty." });
            }

            var currentUserId = _userManager.GetUserId(User);

            // Find or create the user's cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == currentUserId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = currentUserId,
                    DeliveryAddress = deliveryAddress,
                    DateCreated = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };

                _context.Carts.Add(cart);
            }
            else
            {
                cart.DeliveryAddress = deliveryAddress;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Delivery address set successfully!" });
        }

        public class RemoveCartItemRequest
        {
            public int Id { get; set; }
            public string PaymentMethodNonce { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveCartItemRequest request)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Find the user's cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Cow)
                .FirstOrDefaultAsync(c => c.UserId == currentUserId);

            if (cart == null)
            {
                return Json(new { success = false, message = "Cart not found." });
            }

            // Find the item to remove by CowId
            var itemToRemove = cart.Items.FirstOrDefault(i => i.CowId == request.Id);

            if (itemToRemove == null)
            {
                return Json(new { success = false, message = "Cow not found in cart." + request.Id });
            }

            _context.CartItems.Remove(itemToRemove);
            cart.Items.Remove(itemToRemove); // Also remove from in-memory collection

            cart.SubTotal = cart.Items.Sum(item => item.Cow.Price);
            cart.ShippingCost = cart.Items.Count * 500;
            cart.TotalCost = cart.SubTotal + cart.ShippingCost;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cow removed from cart successfully." });
        }

        public class CheckoutRequest
        {
            public string FullName { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public string Phone { get; set; }
            public string PaymentMethodNonce { get; set; }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.PaymentMethodNonce))
                {
                    return BadRequest(new { success = false, error = "Invalid request: PaymentMethodNonce is required." });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(ci => ci.Cow)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.Items.Count == 0)
                {
                    return RedirectToAction("Cart");
                }

                Random random = new Random();

                // Create delivery with no driver and scheduled for 3 days from now
                var delivery = new Delivery
                {
                    DriverId = null,
                    CurrentLocation = null, // or set an initial value if needed
                    ScheduledDate = DateTime.UtcNow.AddDays(3),
                    User = await _userManager.FindByIdAsync(userId),
                    PickedUp = false,
                    PickUpPin = random.Next(1000, 10000),
                    DropOffPin = random.Next(1000, 10000)
                };

                _context.Deliveries.Add(delivery);
                await _context.SaveChangesAsync(); // Save to get the DeliveryId

                var purchase = new Purchase
                {
                    UserId = userId,
                    ShippingCost = cart.ShippingCost,
                    TotalPrice = cart.TotalCost,
                    PurchaseDate = DateTime.UtcNow,
                    PaymentStatus = Purchase.Paymentstatus.Completed,
                    DeliveryStatus = Purchase.Deliverystatus.Scheduled,
                    DeliveryAddress = request.AddressLine1,
                    DeliveryId = delivery.DeliveryId,
                    PurchaseCows = new List<PurchaseCow>()
                };

                foreach (var item in cart.Items)
                {
                    item.Cow.IsAvailable = false;

                    var purchaseCow = new PurchaseCow
                    {
                        CowId = item.CowId,
                        Purchase = purchase
                    };

                    purchase.PurchaseCows.Add(purchaseCow);
                }

                var gateway = _braintreeService.GetGateway();

                var transactionRequest = new TransactionRequest
                {
                    Amount = purchase.TotalPrice, // Now using the calculated amount
                    PaymentMethodNonce = request.PaymentMethodNonce,
                    Options = new TransactionOptionsRequest { SubmitForSettlement = true }
                };

                var result = await gateway.Transaction.SaleAsync(transactionRequest);

                if (result.IsSuccess())
                {
                    _context.Purchases.Add(purchase);
                    cart.TotalCost = 0;
                    cart.SubTotal = 0;
                    cart.ShippingCost = 0;
                    _context.CartItems.RemoveRange(cart.Items);

                    await _context.SaveChangesAsync();

                    // Send email after successful checkout
                    var emailSender = new EmailSender();
                    var user = await _userManager.FindByIdAsync(userId);
                    var email = user.Email;

                    var emailSubject = "Order Confirmation";
                    var emailBody = $"Dear {user.UserName},\n\nYour order has been successfully placed. " +
                                    $"Your total is {cart.TotalCost:C}. Your delivery is scheduled for {delivery.ScheduledDate:dddd, MMMM dd, yyyy}.\n\nThank you for shopping with us!\n\nBest regards,\nAgriChoice Team";

                    await emailSender.SendEmailAsync(
                        to: email,
                        subject: emailSubject,
                        body: emailBody
                    );

                    return Json(new { success = true, message = "Your purchase was completed successfully!" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
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