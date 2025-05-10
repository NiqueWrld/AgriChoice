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

            string[] roles = { "Admin", "Customer" , "Driver" };

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

            // Seed customer user
            string driverEmail = "driver@agri-choice.com";
            string driverPassword = "Driver@123";

            var driverUser = await userManager.FindByEmailAsync(driverEmail);
            if (driverUser == null)
            {
                var newDriver = new IdentityUser
                {
                    UserName = driverEmail,
                    Email = driverEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newDriver, driverPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newDriver, "Driver");
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
                        Price = 11800.00M,
                        Gender = "Male",
                        Description = "High milk yield",
                        ImageUrl = "https://cdn.britannica.com/11/136111-050-174C2796/Holstein-cow.jpg",
                        IsAvailable = true
                    },
                    new Cow
                    {
                        Name = "Luna",
                        Breed = "Jersey",
                        Age = 3,
                        Weight = 550.0,
                        Price = 12150.00M,
                        Gender = "Female",
                        Description = "Calm temperament",
                        ImageUrl = "https://tse1.mm.bing.net/th/id/OIP.4plNfNTdMGql1R2JijVkaAHaE8?cb=iwc1&rs=1&pid=ImgDetMain",
                        IsAvailable = true
                    },
                    new Cow
                    {
                        Name = "Daisy",
                        Breed = "Guernsey",
                        Age = 5,
                        Weight = 620.3,
                        Price = 11950.00M,
                        Gender = "Male",
                        Description = "Good butterfat content",
                        ImageUrl = "https://tse1.mm.bing.net/th/id/OIP.83NsYycYaT_E14TFxuVjgQHaGC?cb=iwc1&rs=1&pid=ImgDetMain",
                        IsAvailable = true
                    },
                    new Cow
                    {
                        Name = "Rosie",
                        Breed = "Ayrshire",
                        Age = 2,
                        Weight = 500.2,
                        Price = 12200.00M,
                        Gender = "Female",
                        Description = "Young and healthy",
                        ImageUrl = "https://th.bing.com/th/id/R.4f9405bf46d9476e1e743c319b6457f0?rik=8uXCXAkDm7iNxQ&pid=ImgRaw&r=0",
                        IsAvailable = true
                    },
                    new Cow
                    {
                        Name = "Molly",
                        Breed = "Brown Swiss",
                        Age = 6,
                        Weight = 700.1,
                        Price = 12050.00M,
                        Gender = "Male",
                        Description = "Experienced milker",
                        ImageUrl = "https://tse3.mm.bing.net/th/id/OIP.GxXf2EKvsKGCBEYiFqWQhAHaFA?cb=iwc1&rs=1&pid=ImgDetMain",
                        IsAvailable = true
                    }
                );
                await context.SaveChangesAsync();
            }

        }
    }
}
