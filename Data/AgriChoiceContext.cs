using AgriChoice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgriChoice.Data;

public class AgriChoiceContext : IdentityDbContext<IdentityUser>
{
    public AgriChoiceContext(DbContextOptions<AgriChoiceContext> options)
        : base(options)
    {
    }

    public DbSet<Cow> Cows { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseCow> PurchaseCow { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
