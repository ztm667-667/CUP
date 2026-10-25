using HearthCup.DataAccess.Data;
using HearthCup.Models;
using HearthCup.Utility.Constants;
using Microsoft.EntityFrameworkCore;

namespace HearthCup.DataAccess.Repository;

/// <summary>Entity Framework Core implementation of the catalogue port declared by the models tier.</summary>
public sealed class ProductRepository(ApplicationDbContext db) : IProductRepository
{
    public async Task<CataloguePage> SearchAsync(CatalogueQuery query, CancellationToken ct = default)
    {
        var source = db.Products.AsNoTracking().AsQueryable();

        // EF Core turns these comparisons into parameterised SQL; no user text is concatenated.
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            source = source.Where(x =>
                x.Name.Contains(query.Q) ||
                x.Description.Contains(query.Q) ||
                x.Colour.Contains(query.Q) ||
                x.Material.Contains(query.Q));
        }

        if (query.Category is not null) source = source.Where(x => x.Category == query.Category);
        if (query.Material is not null) source = source.Where(x => x.Material == query.Material);
        if (query.InStock) source = source.Where(x => x.Stock > 0);

        var total = await source.CountAsync(ct);
        const int size = SD.PageSize;
        var page = Math.Clamp(query.Page, 1, Math.Max(1, (total + size - 1) / size));

        // Every sort ends with Id so that paging is deterministic.
        source = query.Sort switch
        {
            "price-low" => source.OrderBy(x => x.Price).ThenBy(x => x.Id),
            "price-high" => source.OrderByDescending(x => x.Price).ThenBy(x => x.Id),
            "name" => source.OrderBy(x => x.Name).ThenBy(x => x.Id),
            _ => source.OrderBy(x => x.Id)
        };

        var items = await source.Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return new CataloguePage(items.Select(ProductCard.From).ToArray(), total, page, size);
    }

    public Task<Product?> FindAsync(int id, CancellationToken ct = default) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Product>> FindManyAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
        await db.Products.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync(ct);

    public async Task<int> SaveAsync(int? id, ProductInput input, byte[]? expectedVersion, CancellationToken ct = default)
    {
        Product product;

        if (id.HasValue)
        {
            product = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new ShopRuleException("This cup has been deleted.");
            db.Entry(product).Property(x => x.Version).OriginalValue = expectedVersion!;
        }
        else
        {
            product = new Product();
            db.Products.Add(product);
        }

        product.Name = input.Name;
        product.Category = input.Category;
        product.Material = input.Material;
        product.Colour = input.Colour;
        product.CapacityMl = input.CapacityMl;
        product.Price = input.Price;
        product.Stock = input.Stock;
        product.Description = input.Description;
        product.Design = input.Design;

        await SaveChangesAsync(ct);
        return product.Id;
    }

    public async Task DeleteAsync(int id, byte[] expectedVersion, CancellationToken ct = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ShopRuleException("This cup has already been deleted.");

        db.Entry(product).Property(x => x.Version).OriginalValue = expectedVersion;
        db.Products.Remove(product);
        await SaveChangesAsync(ct);
    }

    private async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ShopRuleException("Someone changed this cup while you were editing. Reload before trying again.");
        }
    }
}
