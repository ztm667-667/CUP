namespace HearthCup.Models;

/// <summary>What the session really stores: an id and a quantity, never a price.</summary>
public sealed record CartEntry(int ProductId, int Quantity);

/// <summary>A basket row after the current product and price have been read from the database.</summary>
public sealed record CartLine(ProductCard Product, int Quantity)
{
    public decimal Subtotal => Product.Price * Quantity;
}

public sealed record CartView(IReadOnlyList<CartLine> Lines, bool Changed)
{
    public decimal Total => Lines.Sum(line => line.Subtotal);

    public int Count => Lines.Sum(line => line.Quantity);
}
