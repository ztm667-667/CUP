using HearthCup.Models;

namespace HearthCup.Web.Models;

/// <summary>Everything the catalogue page needs: the query it applied and the page it got back.</summary>
public sealed record BrowseView(CatalogueQuery Query, CataloguePage Catalogue);
