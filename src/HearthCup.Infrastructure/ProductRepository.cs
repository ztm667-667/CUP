using HearthCup.Application;
using HearthCup.Domain;
using Microsoft.EntityFrameworkCore;
namespace HearthCup.Infrastructure;
public sealed class ProductRepository(ShopDbContext db):IProductRepository
{
 public async Task<CataloguePage> SearchAsync(CatalogueQuery q,CancellationToken ct=default)
 {
  var source=db.Products.AsNoTracking().AsQueryable();
  if(!string.IsNullOrWhiteSpace(q.Q)) source=source.Where(x=>x.Name.Contains(q.Q)||x.Description.Contains(q.Q)||x.Colour.Contains(q.Q)||x.Material.Contains(q.Q));
  if(q.Category is not null) source=source.Where(x=>x.Category==q.Category);
  if(q.Material is not null) source=source.Where(x=>x.Material==q.Material);
  if(q.InStock) source=source.Where(x=>x.Stock>0);
  var total=await source.CountAsync(ct);const int size=8;var page=Math.Clamp(q.Page,1,Math.Max(1,(total+size-1)/size));
  source=q.Sort switch {"price-low"=>source.OrderBy(x=>x.Price).ThenBy(x=>x.Id),"price-high"=>source.OrderByDescending(x=>x.Price).ThenBy(x=>x.Id),"name"=>source.OrderBy(x=>x.Name).ThenBy(x=>x.Id),_=>source.OrderBy(x=>x.Id)};
  var items=await source.Skip((page-1)*size).Take(size).ToListAsync(ct);
  return new(items.Select(ProductCard.From).ToArray(),total,page,size);
 }
 public Task<Product?> FindAsync(int id,CancellationToken ct=default)=>db.Products.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id,ct);
 public async Task<IReadOnlyList<Product>> FindManyAsync(IEnumerable<int> ids,CancellationToken ct=default)=>await db.Products.AsNoTracking().Where(x=>ids.Contains(x.Id)).ToListAsync(ct);
 public async Task<int> SaveAsync(int? id,ProductInput input,byte[]? expectedVersion,CancellationToken ct=default)
 {
  Product p;
  if(id.HasValue){p=await db.Products.FirstOrDefaultAsync(x=>x.Id==id,ct)??throw new ShopRuleException("This cup has been deleted.");db.Entry(p).Property(x=>x.Version).OriginalValue=expectedVersion!;}
  else{p=new Product();db.Products.Add(p);}
  p.Name=input.Name;p.Category=input.Category;p.Material=input.Material;p.Colour=input.Colour;p.CapacityMl=input.CapacityMl;p.Price=input.Price;p.Stock=input.Stock;p.Description=input.Description;p.Design=input.Design;
  await SaveChanges(ct);return p.Id;
 }
 public async Task DeleteAsync(int id,byte[] expectedVersion,CancellationToken ct=default)
 {
  var p=await db.Products.FirstOrDefaultAsync(x=>x.Id==id,ct)??throw new ShopRuleException("This cup has already been deleted.");
  db.Entry(p).Property(x=>x.Version).OriginalValue=expectedVersion;db.Products.Remove(p);await SaveChanges(ct);
 }
 private async Task SaveChanges(CancellationToken ct)
 {
  try{await db.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){throw new ShopRuleException("Someone changed this cup while you were editing. Reload before trying again.");}
 }
}
