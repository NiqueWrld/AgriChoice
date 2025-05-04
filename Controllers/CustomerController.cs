using Microsoft.AspNetCore.Mvc;
using AgriChoice.Data; // Replace with your actual namespace
using AgriChoice.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Azure.Core;
using System.Security.Claims;
using System;

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
            if(id == null)
            {
                return NotFound();
            }

            var currentUserName = User.Identity.Name;

            var order = _context.Purchases
                .Include(o => o.PurchaseCows)
                .ThenInclude(o => o.Cow)
                .FirstOrDefault(o => o.User.UserName == currentUserName && o.PurchaseId == id);
   

            return View(order);
        }

        


        public IActionResult Checkout()
        {
            var currentUserId = _userManager.GetUserId(User);

            var cart = _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Cow)
                    .FirstOrDefault(c => c.UserId == currentUserId);

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
                cart.ShippingCost = cart.Items.Count * 500;
                cart.TotalCost = cart.SubTotal + cart.ShippingCost;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();
            }

            return Json(new { success = true, message = "Cow added to cart successfully!" });
        }

        public class RemoveCartItemRequest
        {
            public int Id { get; set; }
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

            _context.Purchases.Add(purchase);
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Your purchase was completed successfully!" });
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