namespace HearthCup.Models;

/// <summary>One row per cup in a signed-in buyer's basket. Persisted, so the basket survives a restart.</summary>
public sealed class CartItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }
}
