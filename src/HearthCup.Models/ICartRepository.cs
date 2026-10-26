namespace HearthCup.Models;

/// <summary>The signed-in buyer's basket, stored so it survives a restart and belongs to one account.</summary>
public interface ICartRepository
{
    Task<IReadOnlyList<CartEntry>> ReadAsync(int userId, CancellationToken ct = default);

    Task SaveAsync(int userId, IReadOnlyList<CartEntry> entries, CancellationToken ct = default);

    Task ClearAsync(int userId, CancellationToken ct = default);
}
