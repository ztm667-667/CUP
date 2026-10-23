using System.Text.Json;
using HearthCup.Domain;
using Microsoft.EntityFrameworkCore;
namespace HearthCup.Infrastructure;
public static class CatalogueImporter
{
 public static async Task ImportAsync(ShopDbContext db,string cataloguePath,string backupDirectory)
 {
  var incoming=JsonSerializer.Deserialize<List<Product>>(await File.ReadAllTextAsync(cataloguePath))??throw new InvalidOperationException("Invalid catalogue.");
  if(incoming.Count!=18||incoming.Any(x=>!x.Design.StartsWith("amazon-")))throw new InvalidOperationException("Expected the reviewed 18-item Amazon catalogue.");
  var existing=await db.Products.OrderBy(x=>x.Id).ToListAsync();
  var originalNames=new[]{"Sunday Morning Mug","Sage Kitchen Mug","Little Hearth Mug","Rose Breakfast Cup","Everyday Water Glass","Garden Day Tumbler","The Slow Commute Cup","Slate Trail Flask","Oat Milk Mug","Terracotta Tea Cup","Bedside Water Glass","Picnic Tumbler","Morning Walk Cup","Soft Sage Tea Mug","Family Table Glass","Winter Cocoa Mug","Rose Afternoon Mug","Weekend Travel Cup"};
  if(existing.Count!=18)throw new InvalidOperationException("Catalogue size changed. Review it before importing; no data overwritten.");
  var fictional=existing.Select(x=>x.Name).SequenceEqual(originalNames);
  if(!fictional)throw new InvalidOperationException("Catalogue was edited or already imported. No data overwritten.");
  Directory.CreateDirectory(backupDirectory);
  var backup=Path.Combine(backupDirectory,"before-amazon-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".json");
  await File.WriteAllTextAsync(backup,JsonSerializer.Serialize(existing,new JsonSerializerOptions{WriteIndented=true}));
  for(var i=0;i<existing.Count;i++)
  {
   var p=existing[i];var n=incoming[i];p.Name=n.Name;p.Category=n.Category;p.Material=n.Material;p.Colour=n.Colour;p.CapacityMl=n.CapacityMl;p.Price=n.Price;p.Stock=n.Stock;p.Description=n.Description;p.Design=n.Design;
  }
  // SaveChanges commits the 18 replacements atomically; rowversion also detects concurrent edits.
  await db.SaveChangesAsync();
 }
}
