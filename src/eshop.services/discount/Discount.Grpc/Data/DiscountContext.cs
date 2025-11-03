using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public sealed class DiscountContext(DbContextOptions<DiscountContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>().ToTable("Coupon");
        
        // Données de seed pour compatibilité ascendante
        var now = DateTime.UtcNow;
        modelBuilder.Entity<Coupon>().HasData([
            new Coupon 
            {
                Id = 1, 
                ProductName = "IPhone X", 
                Description = "IPhone X Discount", 
                Amount = 150.0,
                DiscountType = "Fixed",
                DiscountPercent = null,
                Code = "IPHONE150",
                IsActive = true,
                StartDate = null,
                EndDate = null,
                MinOrderAmount = null,
                MaxDiscountAmount = null,
                IsCumulative = false,
                Category = "Electronics"
            },
            new Coupon 
            {
                Id = 2, 
                ProductName = "Samsung 10", 
                Description = "Samsung 10 Discount", 
                Amount = 100.0,
                DiscountType = "Fixed",
                DiscountPercent = null,
                Code = "SAMSUNG100",
                IsActive = true,
                StartDate = null,
                EndDate = null,
                MinOrderAmount = null,
                MaxDiscountAmount = null,
                IsCumulative = false,
                Category = "Electronics"
            }   
        ]);
    }
}