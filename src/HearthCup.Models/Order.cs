namespace HearthCup.Models;

/// <summary>A placed order. Totals are stored, not recomputed, so history survives later price edits.</summary>
public sealed class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = OrderStatuses.Pending;

    public DateTime PlacedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public decimal Total { get; set; }

    public List<OrderItem> Items { get; set; } = [];

    /// <summary>Navigation only; the buyer's email is read through it for the manager's order list.</summary>
    public User? User { get; set; }
}
