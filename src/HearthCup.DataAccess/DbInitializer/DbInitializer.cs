using System.Text.Json;
using HearthCup.DataAccess.Data;
using HearthCup.Models;
using Microsoft.EntityFrameworkCore;

namespace HearthCup.DataAccess.DbInitializer;

public sealed class DbInitializer(
    ApplicationDbContext db,
    DbInitializerOptions options,
    IUserRepository users,
    IPasswordHasher hasher) : IDbInitializer
{
    private string CataloguePath => Path.Combine(options.ContentRootPath, "Data", "catalogue.json");

    private string BackupDirectory => Path.Combine(options.ContentRootPath, ".local", "catalogue-backups");

    public async Task InitializeAsync()
    {
        // Does the database exist yet? Only a brand-new one gets the sample catalogue, so a manager who
        // deliberately empties the shop does not find the cups restored on the next restart.
        var existed = await db.Database.CanConnectAsync();

        await db.Database.MigrateAsync();

        if (!existed) await SeedCatalogueAsync();

        await EnsureManagerAsync();
    }

    public Task ImportCatalogueAsync() => CatalogueImporter.ImportAsync(db, CataloguePath, BackupDirectory);

    private async Task SeedCatalogueAsync()
    {
        var products = JsonSerializer.Deserialize<List<Product>>(await File.ReadAllTextAsync(CataloguePath))
            ?? throw new InvalidOperationException("Catalogue is missing.");

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    private async Task EnsureManagerAsync()
    {
        if (string.IsNullOrWhiteSpace(options.AdminEmail) || string.IsNullOrWhiteSpace(options.AdminPassword))
        {
            throw new InvalidOperationException("Configure Admin:Email and Admin:Password before the first run.");
        }

        var email = options.AdminEmail.Trim().ToLowerInvariant();
        var manager = await users.EnsureManagerAsync(email, hasher.Hash(options.AdminPassword));

        // The browser checks and the demonstration both read this file; it never ships with the solution.
        if (!options.IsDevelopment) return;

        var directory = Path.Combine(options.ContentRootPath, ".local");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(
            Path.Combine(directory, "admin-login.txt"),
            $"Local coursework manager account\nEmail: {manager.Email}\nPassword: {options.AdminPassword}\nKeep this file private. Not included in source control.");
    }
}
