using Microsoft.EntityFrameworkCore; // For DbContext and related EF Core functionality
using Domain.Entities; // For CategoryEntity and ProductEntity
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // For IdentityDbContext
using Microsoft.AspNetCore.Identity; // For IdentityUser

namespace Domain
{
    // Database context for EF Core
    public class AppDbContext : IdentityDbContext<UserEntity> // Inherit from IdentityDbContext with IdentityUser
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } // Pass options to base constructor

        // Database tables
        public DbSet<CategoryEntity> Categories { get; set; } // Represents the Categories table
        public DbSet<ProductEntity> Products { get; set; } // Represents the Products table.
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Call base to configure Identity tables (e.g., AspNetUsers, AspNetRoles)

            // Configure Category-Product relationship
            modelBuilder.Entity<CategoryEntity>()
                .HasMany(c => c.Products) // One category has many products
                .WithOne(p => p.Category) // One product belongs to one category
                .HasForeignKey(p => p.CategoryId); // Use CategoryId as the foreign key

            // Configure Order-OrderItem relationship
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);

            // Configure OrderItem-Product relationship
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany() // No navigation in Product
                .HasForeignKey(oi => oi.ProductId);
        }
    }
}