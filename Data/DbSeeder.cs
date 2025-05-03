using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using AgriChoice.Models;
using System.Linq;

namespace AgriChoice.Data
{
    public class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<AgriChoiceContext>();

            string[] roles = { "Admin", "Customer" };

            // Seed roles
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed admin user
            string adminEmail = "admin@agri-choice.com";
            string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // Seed customer user
            string customerEmail = "customer@agri-choice.com";
            string customerPassword = "Customer@123";

            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                var newCustomer = new IdentityUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newCustomer, customerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newCustomer, "Customer");
                }
            }

            // Seed Cow data
            if (!context.Cows.Any())
            {
                context.Cows.AddRange(
                    new Cow
                    {
                        Name = "Bella",
                        Breed = "Holstein",
                        Age = 4,
                        Weight = 600.5,
                        Price = 1500.00M,
                        Description = "High milk yield",
                        ImageUrl = "/images/cows/bella.jpg",
                        IsAvailable = true
                    },
                    new Cow
                    {
                        Name = "Luna",
                        Breed = "Jersey",
                        Age = 3,
                        Weight = 550.0,
                        Price = 1300.00M,
                        Description = "Calm temperament",
                        ImageUrl = "/images/cows/luna.jpg",
                        IsAvailable = true
                    },
                    new Cow
                    {
                        Name = "Daisy",
                        Breed = "Guernsey",
                        Age = 5,
                        Weight = 620.3,
                        Price = 1400.00M,
                        Description = "Good butterfat content",
                        ImageUrl = "/images/cows/daisy.jpg",
                        IsAvailable = true
                    },
                    new Cow
                    {
                        Name = "Rosie",
                        Breed = "Ayrshire",
                        Age = 2,
                        Weight = 500.2,
                        Price = 1200.00M,
                        Description = "Young and healthy",
                        ImageUrl = "/images/cows/rosie.jpg",
                        IsAvailable = true
                    },
                    new Cow
                    {
                        Name = "Molly",
                        Breed = "Brown Swiss",
                        Age = 6,
                        Weight = 700.1,
                        Price = 1600.00M,
                        Description = "Experienced milker",
                        ImageUrl = "/images/cows/molly.jpg",
                        IsAvailable = true 
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
