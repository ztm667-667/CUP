using System.Text.Json;
using HearthCup.DataAccess.Data;
using HearthCup.Models;
using Microsoft.EntityFrameworkCore;

namespace HearthCup.DataAccess.DbInitializer;

public sealed class DbInitializer(ApplicationDbContext db, DbInitializerOptions options) : IDbInitializer
{
    private string CataloguePath => Path.Combine(options.ContentRootPath, "Data", "catalogue.json");

    private string BackupDirectory => Path.Combine(options.ContentRootPath, ".local", "catalogue-backups");

    public async Task InitializeAsync()
    {
        // Does the database exist yet? Only a brand-new one gets the sample catalogue, so a manager who
        // deliberately empties the shop does not find the cups restored on the next restart.
        var existed = await db.Database.CanConnectAsync();

        await db.Database.MigrateAsync();

        if (existed) return;

        var products = JsonSerializer.Deserialize<List<Product>>(await File.ReadAllTextAsync(CataloguePath))
            ?? throw new InvalidOperationException("Catalogue is missing.");

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    public Task ImportCatalogueAsync() => CatalogueImporter.ImportAsync(db, CataloguePath, BackupDirectory);
}
