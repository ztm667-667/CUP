using System.ComponentModel.DataAnnotations;

namespace HearthCup.Models;

/// <summary>Search, filter, sort and paging values bound from the query string.</summary>
public sealed class CatalogueQuery
{
    [StringLength(80)]
    public string? Q { get; set; }

    public string? Category { get; set; }

    public string? Material { get; set; }

    public bool InStock { get; set; }

    public string Sort { get; set; } = "featured";

    /// <summary>Set only by the manager screens, which must see delisted cups as well.</summary>
    public bool IncludeHidden { get; set; }

    public int Page { get; set; } = 1;
}
