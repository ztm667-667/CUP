namespace HearthCup.Models;

/// <summary>Read model handed to Razor views, so a page never touches a tracked entity.</summary>
public sealed record ProductCard(
    int Id,
    string Name,
    string Category,
    string Material,
    string Colour,
    int CapacityMl,
    decimal Price,
    int Stock,
    string Description,
    string Design,
    string Version)
{
    /// <summary>Only the approved local assets are ever turned into a URL.</summary>
    public string ImageUrl => Design.StartsWith("amazon-", StringComparison.Ordinal)
        ? $"/images/products/{Design[7..]}.jpg"
        : $"/images/{Design}.svg";

    public static ProductCard From(Product product) => new(
        product.Id,
        product.Name,
        product.Category,
        product.Material,
        product.Colour,
        product.CapacityMl,
        product.Price,
        product.Stock,
        product.Description,
        product.Design,
        VersionToken.Encode(product.Version));
}
