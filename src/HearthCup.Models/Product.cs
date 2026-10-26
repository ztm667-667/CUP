namespace HearthCup.Models;

/// <summary>A cup that the shop sells. Entity Framework Core maps this type to the Products table.</summary>
public sealed class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Category { get; set; } = "Mugs";

    public string Material { get; set; } = "Ceramic";

    public string Colour { get; set; } = "Oat";

    public int CapacityMl { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Description { get; set; } = "";

    /// <summary>Approved local image asset name; see <see cref="CupOptions.Designs"/>.</summary>
    public string Design { get; set; } = "oat-mug";

    /// <summary>False when the manager has delisted the cup: hidden from the shop, kept in the database.</summary>
    public bool IsListed { get; set; } = true;

    /// <summary>SQL Server rowversion used to detect concurrent edits.</summary>
    public byte[] Version { get; set; } = [];
}
