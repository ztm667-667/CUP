using HearthCup.Domain;
using Microsoft.EntityFrameworkCore;
namespace HearthCup.Infrastructure;
public sealed class ShopDbContext(DbContextOptions<ShopDbContext> options):DbContext(options)
{
 public DbSet<Product> Products=>Set<Product>();
 protected override void OnModelCreating(ModelBuilder model)
 {
  var p=model.Entity<Product>();p.Property(x=>x.Name).HasMaxLength(80).IsRequired();
  p.Property(x=>x.Description).HasMaxLength(1200).IsRequired();
  foreach(var name in new[]{"Category","Material","Colour","Design"}) p.Property<string>(name).HasMaxLength(40).IsRequired();
  p.Property(x=>x.Price).HasPrecision(6,2);p.Property(x=>x.Version).IsRowVersion();
  p.HasIndex(x=>x.Category);
  p.ToTable("Products",t=>{t.HasCheckConstraint("CK_Products_Price","[Price] >= 0.50 AND [Price] <= 999.99");t.HasCheckConstraint("CK_Products_Stock","[Stock] >= 0 AND [Stock] <= 9999");t.HasCheckConstraint("CK_Products_Capacity","[CapacityMl] >= 50 AND [CapacityMl] <= 2000");});
 }
}
