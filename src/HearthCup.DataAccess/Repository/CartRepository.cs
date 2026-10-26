using HearthCup.DataAccess.Data;
using HearthCup.Models;
using Microsoft.EntityFrameworkCore;

namespace HearthCup.DataAccess.Repository;

public sealed class CartRepository(ApplicationDbContext db) : ICartRepository
{
    public async Task<IReadOnlyList<CartEntry>> ReadAsync(int userId, CancellationToken ct = default) =>
        await db.CartItems.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Id)
            .Select(x => new CartEntry(x.ProductId, x.Quantity))
            .ToListAsync(ct);

    public async Task SaveAsync(int userId, IReadOnlyList<CartEntry> entries, CancellationToken ct = default)
    {
        var existing = await db.CartItems.Where(x => x.UserId == userId).ToListAsync(ct);
        db.CartItems.RemoveRange(existing);

        foreach (var entry in entries)
        {
            db.CartItems.Add(new CartItem { UserId = userId, ProductId = entry.ProductId, Quantity = entry.Quantity });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task ClearAsync(int userId, CancellationToken ct = default)
    {
        var existing = await db.CartItems.Where(x => x.UserId == userId).ToListAsync(ct);
        db.CartItems.RemoveRange(existing);
        await db.SaveChangesAsync(ct);
    }
}
