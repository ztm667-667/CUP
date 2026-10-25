namespace HearthCup.Models;

/// <summary>
/// The port the business tier depends on. Entity Framework lives behind it, so the domain rules can be
/// exercised with an in-memory fake and no SQL Server.
/// </summary>
public interface IProductRepository
{
    Task<CataloguePage> SearchAsync(CatalogueQuery query, CancellationToken ct = default);

    Task<Product?> FindAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> FindManyAsync(IEnumerable<int> ids, CancellationToken ct = default);

    Task<int> SaveAsync(int? id, ProductInput input, byte[]? expectedVersion, CancellationToken ct = default);

    Task DeleteAsync(int id, byte[] expectedVersion, CancellationToken ct = default);
}
