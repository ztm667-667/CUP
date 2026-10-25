using HearthCup.Models;
using Microsoft.EntityFrameworkCore;

namespace HearthCup.DataAccess.Data;

/// <summary>Entity Framework Core context for the shop. The only place that knows about SQL Server.</summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

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

        // The check constraints repeat the rules in the business tier as a last line of defence.
        product.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_Price", "[Price] >= 0.50 AND [Price] <= 999.99");
            table.HasCheckConstraint("CK_Products_Stock", "[Stock] >= 0 AND [Stock] <= 9999");
            table.HasCheckConstraint("CK_Products_Capacity", "[CapacityMl] >= 50 AND [CapacityMl] <= 2000");
        });
    }
}
