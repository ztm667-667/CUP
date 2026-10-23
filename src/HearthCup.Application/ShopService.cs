using System.ComponentModel.DataAnnotations;
using HearthCup.Domain;
namespace HearthCup.Application;
public sealed class ShopService(IProductRepository repository)
{
 public Task<CataloguePage> BrowseAsync(CatalogueQuery query,CancellationToken ct=default)
 {
  query.Q=query.Q?.Trim(); if(query.Q?.Length>80) query.Q=query.Q[..80];
  query.Page=Math.Clamp(query.Page,1,100000);
  if(!CupOptions.Categories.Contains(query.Category)) query.Category=null;
  if(!CupOptions.Materials.Contains(query.Material)) query.Material=null;
  if(!new[]{"featured","price-low","price-high","name"}.Contains(query.Sort)) query.Sort="featured";
  return repository.SearchAsync(query,ct);
 }
 public async Task<ProductCard?> FindAsync(int id,CancellationToken ct=default) => (await repository.FindAsync(id,ct)) is {} p?ProductCard.From(p):null;
 public Task<int> SaveAsync(int? id,ProductInput input,CancellationToken ct=default)
 {
  input.Name=input.Name.Trim();input.Description=input.Description.Trim();
  var errors=new List<ValidationResult>();
  if(!Validator.TryValidateObject(input,new ValidationContext(input),errors,true)) throw new ShopRuleException(errors[0].ErrorMessage!);
  if(!CupOptions.Categories.Contains(input.Category)||!CupOptions.Materials.Contains(input.Material)||!CupOptions.Colours.Contains(input.Colour)||!CupOptions.Designs.Contains(input.Design)) throw new ShopRuleException("Choose one of the available product options.");
  if(decimal.Round(input.Price,2)!=input.Price) throw new ShopRuleException("Use no more than two decimal places for the price.");
  return repository.SaveAsync(id,input,id.HasValue?ParseVersion(input.Version):null,ct);
 }
 public Task DeleteAsync(int id,string version,CancellationToken ct=default)=>repository.DeleteAsync(id,ParseVersion(version),ct);
 private static byte[] ParseVersion(string? version)
 {
  try { var b=Convert.FromBase64String(version??""); if(b.Length==8) return b; }catch(FormatException){}
  throw new ShopRuleException("This product has changed. Reload the page and try again.");
 }
 public async Task<IReadOnlyList<CartEntry>> AddAsync(IReadOnlyList<CartEntry> entries,int productId,int quantity,CancellationToken ct=default)
 {
  if(quantity<1||quantity>99) throw new ShopRuleException("Choose a quantity between 1 and 99.");
  var p=await repository.FindAsync(productId,ct)??throw new ShopRuleException("This cup is no longer available.");
  var current=entries.FirstOrDefault(x=>x.ProductId==productId)?.Quantity??0;
  if(current+quantity>Math.Min(p.Stock,99)) throw new ShopRuleException($"Only {Math.Min(p.Stock,99)} can be added for this cup.");
  if(current==0&&entries.Count>=50) throw new ShopRuleException("Your basket can hold up to 50 different cups.");
  return entries.Where(x=>x.ProductId!=productId).Append(new(productId,current+quantity)).ToArray();
 }
 public async Task<CartView> ReadCartAsync(IReadOnlyList<CartEntry> entries,CancellationToken ct=default)
 {
  var products=await repository.FindManyAsync(entries.Select(x=>x.ProductId),ct);
  var lines=new List<CartLine>();var changed=false;
  foreach(var entry in entries)
  {
   var p=products.FirstOrDefault(x=>x.Id==entry.ProductId);
   if(p is null||p.Stock==0){changed=true;continue;}
   var quantity=Math.Clamp(entry.Quantity,1,Math.Min(99,p.Stock));
   changed|=quantity!=entry.Quantity;
   lines.Add(new(ProductCard.From(p),quantity));
  }
  return new(lines,changed);
 }
}
