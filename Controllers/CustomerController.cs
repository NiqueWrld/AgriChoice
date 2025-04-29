using Microsoft.AspNetCore.Mvc;
using AgriChoice.Data; // Replace with your actual namespace
using AgriChoice.Models;
using System.Linq;

namespace AgriChoice.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AgriChoiceContext _context;

        public CustomerController(AgriChoiceContext context)
        {
            _context = context;
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