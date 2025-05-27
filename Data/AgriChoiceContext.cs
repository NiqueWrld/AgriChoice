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
    public DbSet<Review> Reviews { get; set; }
    public DbSet<RefundRequest> RefundRequests { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure the one-to-one relationship between Purchase and RefundRequest
        builder.Entity<RefundRequest>()
            .HasOne(r => r.Purchase)
            .WithOne(p => p.RefundRequest)
            .HasForeignKey<RefundRequest>(r => r.PurchaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure the many-to-many relationship between RefundRequest and Cow
        builder.Entity<RefundRequestCow>()
            .HasKey(rc => new { rc.RefundRequestId, rc.CowId });

        builder.Entity<RefundRequestCow>()
            .HasOne(rc => rc.RefundRequest)
            .WithMany(r => r.RefundRequestCows)
            .HasForeignKey(rc => rc.RefundRequestId);

        builder.Entity<RefundRequestCow>()
            .HasOne(rc => rc.Cow)
            .WithMany()
            .HasForeignKey(rc => rc.CowId);

    }
}
