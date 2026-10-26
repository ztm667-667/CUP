using HearthCup.Models;
using Microsoft.EntityFrameworkCore;

namespace HearthCup.DataAccess.Data;

/// <summary>Entity Framework Core context for the shop. The only place that knows about SQL Server.</summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        var product = model.Entity<Product>();

        product.Property(x => x.Name).HasMaxLength(80).IsRequired();
        product.Property(x => x.Description).HasMaxLength(1200).IsRequired();

        // Column lengths keep the option lists bounded at the storage level too.
        foreach (var name in new[] { "Category", "Material", "Colour", "Design" })
        {
            product.Property<string>(name).HasMaxLength(40).IsRequired();
        }

        product.Property(x => x.Price).HasPrecision(6, 2);
        product.Property(x => x.Version).IsRowVersion();
        product.HasIndex(x => x.Category);
        product.HasIndex(x => x.IsListed);

        // The check constraints repeat the rules in the business tier as a last line of defence.
        product.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_Price", "[Price] >= 0.50 AND [Price] <= 999.99");
            table.HasCheckConstraint("CK_Products_Stock", "[Stock] >= 0 AND [Stock] <= 9999");
            table.HasCheckConstraint("CK_Products_Capacity", "[CapacityMl] >= 50 AND [CapacityMl] <= 2000");
        });

        var user = model.Entity<User>();
        user.Property(x => x.Email).HasMaxLength(254).IsRequired();
        user.Property(x => x.PasswordHash).HasMaxLength(400).IsRequired();
        user.Property(x => x.Role).HasMaxLength(20).IsRequired();
        user.HasIndex(x => x.Email).IsUnique();

        var order = model.Entity<Order>();
        order.Property(x => x.Total).HasPrecision(10, 2);
        order.Property(x => x.Status).HasMaxLength(20).IsRequired();
        order.HasIndex(x => x.UserId);
        order.HasIndex(x => x.Status);
        order.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        order.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);

        var item = model.Entity<OrderItem>();
        item.Property(x => x.Name).HasMaxLength(100).IsRequired();
        item.Property(x => x.UnitPrice).HasPrecision(10, 2);
        item.ToTable("OrderItems", table => table.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] >= 1"));

        var cart = model.Entity<CartItem>();
        cart.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
        cart.ToTable("CartItems", table => table.HasCheckConstraint("CK_CartItems_Quantity", "[Quantity] >= 1"));
    }
}
