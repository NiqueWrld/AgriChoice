using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgriChoice.Data;
using AgriChoice.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AgriChoice.Controllers
{
    [Authorize(Roles = "Admin")] // Restrict access to Admin role only
    public class AdminController : Controller
    {
        private readonly AgriChoiceContext _context;

        public AdminController(AgriChoiceContext context)
        {
            _context = context;
        }

        // Dashboard
        public IActionResult Dashboard()
        {
            ViewData["TotalCows"] = _context.Cows.Count();
            ViewData["AvailableCows"] = _context.Cows.Count(c => c.IsAvailable);
            ViewData["TotalOrders"] = _context.Purchases.Count();

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
            return View(); // Return the Add Cow form
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


            // Save the Cow to the database (pseudo-code)
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
                .Include(p => p.Cow)
                .Include(p => p.User)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return View(purchases);
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