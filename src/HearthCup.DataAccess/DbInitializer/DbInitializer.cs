using System.Text.Json;
using HearthCup.DataAccess.Data;
using HearthCup.Models;

namespace HearthCup.DataAccess.DbInitializer;

public sealed class DbInitializer(ApplicationDbContext db, DbInitializerOptions options) : IDbInitializer
{
    private string CataloguePath => Path.Combine(options.ContentRootPath, "Data", "catalogue.json");

    private string BackupDirectory => Path.Combine(options.ContentRootPath, ".local", "catalogue-backups");

    public async Task InitializeAsync()
    {
        var created = await db.Database.EnsureCreatedAsync();

        // Never recreate products when a manager has deliberately emptied the shop.
        if (!created) return;

        var products = JsonSerializer.Deserialize<List<Product>>(await File.ReadAllTextAsync(CataloguePath))
            ?? throw new InvalidOperationException("Catalogue is missing.");

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    public Task ImportCatalogueAsync() => CatalogueImporter.ImportAsync(db, CataloguePath, BackupDirectory);
}
