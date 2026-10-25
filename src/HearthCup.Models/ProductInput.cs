using System.ComponentModel.DataAnnotations;

namespace HearthCup.Models;

/// <summary>What the manager submits on the create/edit form. Validation lives here so both tiers share one rule set.</summary>
public sealed class ProductInput
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [Required]
    public string Category { get; set; } = "Mugs";

    [Required]
    public string Material { get; set; } = "Ceramic";

    [Required]
    public string Colour { get; set; } = "Oat";

    [Range(50, 2000)]
    public int CapacityMl { get; set; } = 350;

    [Range(typeof(decimal), "0.50", "999.99")]
    public decimal Price { get; set; } = 12;

    [Range(0, 9999)]
    public int Stock { get; set; } = 10;

    [Required, StringLength(1200, MinimumLength = 10)]
    public string Description { get; set; } = "";

    [Required]
    public string Design { get; set; } = "oat-mug";

    /// <summary>Rowversion sent back by the edit form; null when creating a new cup.</summary>
    public string? Version { get; set; }
}
