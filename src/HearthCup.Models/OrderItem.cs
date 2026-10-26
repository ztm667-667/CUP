namespace HearthCup.Models;

/// <summary>
/// A line of a placed order. Name and unit price are copied at checkout time, so renaming or
/// repricing a cup later never rewrites what the buyer actually paid.
/// </summary>
public sealed class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; } = "";

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
}
