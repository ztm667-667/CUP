namespace HearthCup.Models;

public interface IOrderRepository
{
    /// <summary>
    /// Stores the order, applies the stock deltas and clears the buyer's basket in one transaction.
    /// The lines and total are passed in, so the implementation can build a fresh order on a retry.
    /// Throws <see cref="ShopRuleException"/> when a cup ran out between validation and commit.
    /// </summary>
    Task<int> PlaceAsync(
        int userId,
        IReadOnlyList<OrderItem> items,
        decimal total,
        IReadOnlyDictionary<int, int> stockDeltas,
        CancellationToken ct = default);

    Task<IReadOnlyList<Order>> ListForUserAsync(int userId, CancellationToken ct = default);

    Task<IReadOnlyList<Order>> ListAllAsync(CancellationToken ct = default);

    Task<Order?> FindAsync(int id, CancellationToken ct = default);

    /// <summary>Moves a pending order to shipped. Returns false when the order is not pending any more.</summary>
    Task<bool> ShipAsync(int id, CancellationToken ct = default);
}
