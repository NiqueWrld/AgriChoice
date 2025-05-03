using AgriChoice.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriChoice.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly AgriChoiceContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(AgriChoiceContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult GetCart()
        {
            var currentUserId = _userManager.GetUserId(User);

            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Cow)
                .FirstOrDefault(c => c.UserId == currentUserId);

            if (cart == null || cart.Items == null)
            {
                return Ok(new { Items = new List<object>(), TotalItems = 0 });
            }

            var cartData = new
            {
                Items = cart.Items.Select(item => new
                {
                    item.CartItemId,
                    CowName = item.Cow.Name,
                    CowBreed = item.Cow.Breed,
                    CowPrice = item.Cow.Price
                }),
                SubTotal = cart.SubTotal,
                ShippingFee = cart.ShippingCost,
                TotalItems = cart.TotalCost
            };

            return Ok(cartData);
        }
    }
}
