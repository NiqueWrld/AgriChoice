using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AgriChoice.Data;

var builder = WebApplication.CreateBuilder(args);

// Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("AgriChoiceContextConnection")
    ?? throw new InvalidOperationException("Connection string 'AgriChoiceContextConnection' not found.");

// Configure Entity Framework and Identity
builder.Services.AddDbContext<AgriChoiceContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Require email confirmation for account sign-in
})
.AddRoles<IdentityRole>() // Enable role management
.AddEntityFrameworkStores<AgriChoiceContext>();

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Add Razor Pages support

var app = builder.Build();

// Seed roles and the admin user
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await DbSeeder.SeedRolesAndAdminAsync(serviceProvider); // Ensure this method is implemented
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Map Razor Pages

app.Run();