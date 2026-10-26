using System.ComponentModel.DataAnnotations;
using HearthCup.Models;
using HearthCup.Utility.Constants;

namespace HearthCup.Utility.Services;

/// <summary>
/// The business tier. Every rule that must hold whatever the browser sends lives here, the repository
/// port keeps it free of Entity Framework, and the checks project exercises it without SQL Server.
/// </summary>
public sealed class ShopService(IProductRepository repository)
{
    public Task<CataloguePage> BrowseAsync(CatalogueQuery query, CancellationToken ct = default)
    {
        query.Q = query.Q?.Trim();
        if (query.Q?.Length > SD.MaxSearchLength) query.Q = query.Q[..SD.MaxSearchLength];

        query.Page = Math.Clamp(query.Page, 1, 100000);

        // Anything not on the approved list is dropped rather than trusted.
        if (!CupOptions.Categories.Contains(query.Category)) query.Category = null;
        if (!CupOptions.Materials.Contains(query.Material)) query.Material = null;
        if (!new[] { "featured", "price-low", "price-high", "name" }.Contains(query.Sort)) query.Sort = "featured";

        return repository.SearchAsync(query, ct);
    }

    public async Task<ProductCard?> FindAsync(int id, CancellationToken ct = default) =>
        await repository.FindAsync(id, ct) is { } product ? ProductCard.From(product) : null;

    public Task<int> SaveAsync(int? id, ProductInput input, CancellationToken ct = default)
    {
        input.Name = input.Name.Trim();
        input.Description = input.Description.Trim();

        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), errors, true))
        {
            throw new ShopRuleException(errors[0].ErrorMessage!);
        }

        if (!CupOptions.Categories.Contains(input.Category) ||
            !CupOptions.Materials.Contains(input.Material) ||
            !CupOptions.Colours.Contains(input.Colour) ||
            !CupOptions.Designs.Contains(input.Design))
        {
            throw new ShopRuleException("Choose one of the available product options.");
        }

        if (decimal.Round(input.Price, 2) != input.Price)
        {
            throw new ShopRuleException("Use no more than two decimal places for the price.");
        }

        return repository.SaveAsync(id, input, id.HasValue ? ParseVersion(input.Version) : null, ct);
    }

    public Task DeleteAsync(int id, string version, CancellationToken ct = default) =>
        repository.DeleteAsync(id, ParseVersion(version), ct);

    public Task SetListedAsync(int id, bool listed, CancellationToken ct = default) =>
        repository.SetListedAsync(id, listed, ct);

    public async Task<IReadOnlyList<CartEntry>> AddAsync(
        IReadOnlyList<CartEntry> entries,
        int productId,
        int quantity,
        CancellationToken ct = default)
    {
        if (quantity < 1 || quantity > SD.MaxQuantity)
        {
            throw new ShopRuleException($"Choose a quantity between 1 and {SD.MaxQuantity}.");
        }

        var product = await repository.FindAsync(productId, ct)
            ?? throw new ShopRuleException("This cup is no longer available.");

        var current = entries.FirstOrDefault(x => x.ProductId == productId)?.Quantity ?? 0;

        if (current + quantity > Math.Min(product.Stock, SD.MaxQuantity))
        {
            throw new ShopRuleException($"Only {Math.Min(product.Stock, SD.MaxQuantity)} can be added for this cup.");
        }

        if (current == 0 && entries.Count >= SD.MaxBasketLines)
        {
            throw new ShopRuleException($"Your basket can hold up to {SD.MaxBasketLines} different cups.");
        }

        return entries.Where(x => x.ProductId != productId).Append(new CartEntry(productId, current + quantity)).ToArray();
    }

    public async Task<CartView> ReadCartAsync(IReadOnlyList<CartEntry> entries, CancellationToken ct = default)
    {
        var products = await repository.FindManyAsync(entries.Select(x => x.ProductId), ct);
        var lines = new List<CartLine>();
        var changed = false;

        foreach (var entry in entries)
        {
            var product = products.FirstOrDefault(x => x.Id == entry.ProductId);

            // Deleted, delisted or sold-out cups never survive a visit to the basket.
            if (product is null || product.Stock == 0 || !product.IsListed)
            {
                changed = true;
                continue;
            }

            var quantity = Math.Clamp(entry.Quantity, 1, Math.Min(SD.MaxQuantity, product.Stock));
            changed |= quantity != entry.Quantity;
            lines.Add(new CartLine(ProductCard.From(product), quantity));
        }

        return new CartView(lines, changed);
    }

    private static byte[] ParseVersion(string? version) =>
        VersionToken.TryParse(version, out var bytes)
            ? bytes
            : throw new ShopRuleException("This product has changed. Reload the page and try again.");
}
