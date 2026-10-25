namespace HearthCup.Models;

/// <summary>One page of products plus the totals the pager needs.</summary>
public sealed record CataloguePage(IReadOnlyList<ProductCard> Items, int Total, int Page, int PageSize)
{
    public int Pages => Math.Max(1, (int)Math.Ceiling((double)Total / PageSize));
}
