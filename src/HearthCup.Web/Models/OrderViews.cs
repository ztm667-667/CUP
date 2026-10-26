using HearthCup.Models;

namespace HearthCup.Web.Models;

public sealed record OrderLineView(string Name, decimal UnitPrice, int Quantity)
{
    public decimal Subtotal => UnitPrice * Quantity;
}

/// <summary>A read model for the order pages; the views never touch a tracked entity.</summary>
public sealed record OrderView(
    int Id,
    string Status,
    DateTime PlacedAt,
    DateTime? ShippedAt,
    decimal Total,
    string? BuyerEmail,
    IReadOnlyList<OrderLineView> Lines)
{
    public bool IsShipped => Status == OrderStatuses.Shipped;

    public static OrderView From(Order order) => new(
        order.Id,
        order.Status,
        order.PlacedAt,
        order.ShippedAt,
        order.Total,
        order.User?.Email,
        order.Items.Select(x => new OrderLineView(x.Name, x.UnitPrice, x.Quantity)).ToArray());
}
