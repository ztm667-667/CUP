using System.ComponentModel.DataAnnotations;
using HearthCup.Domain;
namespace HearthCup.Application;
public sealed class ProductInput
{
 [Required, StringLength(80, MinimumLength=2)] public string Name { get; set; } = "";
 [Required] public string Category { get; set; } = "Mugs";
 [Required] public string Material { get; set; } = "Ceramic";
 [Required] public string Colour { get; set; } = "Oat";
 [Range(50,2000)] public int CapacityMl { get; set; } = 350;
 [Range(typeof(decimal),"0.50","999.99")] public decimal Price { get; set; } = 12;
 [Range(0,9999)] public int Stock { get; set; } = 10;
 [Required, StringLength(1200,MinimumLength=10)] public string Description { get; set; } = "";
 [Required] public string Design { get; set; } = "oat-mug";
 public string? Version { get; set; }
}
public sealed class CatalogueQuery
{
 [StringLength(80)] public string? Q { get; set; }
 public string? Category { get; set; }
 public string? Material { get; set; }
 public bool InStock { get; set; }
 public string Sort { get; set; } = "featured";
 public int Page { get; set; } = 1;
}
public sealed record ProductCard(int Id,string Name,string Category,string Material,string Colour,int CapacityMl,decimal Price,int Stock,string Description,string Design,string Version)
{
 public string ImageUrl => Design.StartsWith("amazon-",StringComparison.Ordinal) ? $"/images/products/{Design[7..]}.jpg" : $"/images/{Design}.svg";
 public static ProductCard From(Product p) => new(p.Id,p.Name,p.Category,p.Material,p.Colour,p.CapacityMl,p.Price,p.Stock,p.Description,p.Design,Convert.ToBase64String(p.Version));
}
public sealed record CataloguePage(IReadOnlyList<ProductCard> Items,int Total,int Page,int PageSize)
{
 public int Pages => Math.Max(1,(int)Math.Ceiling((double)Total/PageSize));
}
public sealed record CartEntry(int ProductId,int Quantity);
public sealed record CartLine(ProductCard Product,int Quantity) { public decimal Subtotal => Product.Price*Quantity; }
public sealed record CartView(IReadOnlyList<CartLine> Lines,bool Changed)
{
 public decimal Total => Lines.Sum(x=>x.Subtotal);
 public int Count => Lines.Sum(x=>x.Quantity);
}
public sealed class ShopRuleException(string message) : Exception(message);
public interface IProductRepository
{
 Task<CataloguePage> SearchAsync(CatalogueQuery query,CancellationToken ct=default);
 Task<Product?> FindAsync(int id,CancellationToken ct=default);
 Task<IReadOnlyList<Product>> FindManyAsync(IEnumerable<int> ids,CancellationToken ct=default);
 Task<int> SaveAsync(int? id,ProductInput input,byte[]? expectedVersion,CancellationToken ct=default);
 Task DeleteAsync(int id,byte[] expectedVersion,CancellationToken ct=default);
}
