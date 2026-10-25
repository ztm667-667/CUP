using System.Text.Json;
using HearthCup.DataAccess.Data;
using HearthCup.Models;
using Microsoft.EntityFrameworkCore;

namespace HearthCup.DataAccess.DbInitializer;

/// <summary>
/// One-off, guarded replacement of the original fictional seed with the reviewed Amazon references.
/// It refuses to run unless the database still holds the untrimmed fictional catalogue.
/// </summary>
public static class CatalogueImporter
{
    private static readonly string[] OriginalNames =
    [
        "Sunday Morning Mug", "Sage Kitchen Mug", "Little Hearth Mug", "Rose Breakfast Cup",
        "Everyday Water Glass", "Garden Day Tumbler", "The Slow Commute Cup", "Slate Trail Flask",
        "Oat Milk Mug", "Terracotta Tea Cup", "Bedside Water Glass", "Picnic Tumbler",
        "Morning Walk Cup", "Soft Sage Tea Mug", "Family Table Glass", "Winter Cocoa Mug",
        "Rose Afternoon Mug", "Weekend Travel Cup"
    ];

    public static async Task ImportAsync(ApplicationDbContext db, string cataloguePath, string backupDirectory)
    {
        var incoming = JsonSerializer.Deserialize<List<Product>>(await File.ReadAllTextAsync(cataloguePath))
            ?? throw new InvalidOperationException("Invalid catalogue.");

        if (incoming.Count != 18 || incoming.Any(x => !x.Design.StartsWith("amazon-")))
        {
            throw new InvalidOperationException("Expected the reviewed 18-item Amazon catalogue.");
        }

        var existing = await db.Products.OrderBy(x => x.Id).ToListAsync();

        if (existing.Count != 18)
        {
            throw new InvalidOperationException("Catalogue size changed. Review it before importing; no data overwritten.");
        }

        if (!existing.Select(x => x.Name).SequenceEqual(OriginalNames))
        {
            throw new InvalidOperationException("Catalogue was edited or already imported. No data overwritten.");
        }

        Directory.CreateDirectory(backupDirectory);
        var backup = Path.Combine(backupDirectory, "before-amazon-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".json");
        await File.WriteAllTextAsync(backup, JsonSerializer.Serialize(existing, new JsonSerializerOptions { WriteIndented = true }));

        for (var i = 0; i < existing.Count; i++)
        {
            var current = existing[i];
            var replacement = incoming[i];
            current.Name = replacement.Name;
            current.Category = replacement.Category;
            current.Material = replacement.Material;
            current.Colour = replacement.Colour;
            current.CapacityMl = replacement.CapacityMl;
            current.Price = replacement.Price;
            current.Stock = replacement.Stock;
            current.Description = replacement.Description;
            current.Design = replacement.Design;
        }

        // SaveChanges commits the 18 replacements atomically; rowversion also detects concurrent edits.
        await db.SaveChangesAsync();
    }
}
