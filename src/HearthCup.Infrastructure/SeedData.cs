using System.Text.Json;
using HearthCup.Domain;
using Microsoft.EntityFrameworkCore;
namespace HearthCup.Infrastructure;
public static class SeedData
{
 public static async Task InitializeAsync(ShopDbContext db,string cataloguePath)
 {
  var created=await db.Database.EnsureCreatedAsync();
  if(!created)return; // Never recreate products when a manager deliberately empties the shop.
  var products=JsonSerializer.Deserialize<List<Product>>(await File.ReadAllTextAsync(cataloguePath))??throw new InvalidOperationException("Catalogue is missing.");
  db.Products.AddRange(products);await db.SaveChangesAsync();
 }
}
