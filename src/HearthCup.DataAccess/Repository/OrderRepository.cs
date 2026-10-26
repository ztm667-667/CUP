using HearthCup.DataAccess.Data;
using HearthCup.Models;
using Microsoft.EntityFrameworkCore;

namespace HearthCup.DataAccess.Repository;

public sealed class OrderRepository(ApplicationDbContext db) : IOrderRepository
{
    public async Task<int> PlaceAsync(
        int userId,
        IReadOnlyList<OrderItem> items,
        decimal total,
        IReadOnlyDictionary<int, int> stockDeltas,
        CancellationToken ct = default)
    {
        // EnableRetryOnFailure installs a retrying execution strategy, and EF refuses a user-initiated
        // transaction outside it. The whole unit of work is therefore handed to the strategy.
        var strategy = db.Database.CreateExecutionStrategy();
        var orderId = 0;

        await strategy.ExecuteAsync(async () =>
        {
            // A retry must start from a clean slate: forget anything tracked by the previous attempt.
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            // Re-read inside the transaction: the stock check in the business tier may be moments old.
            var ids = stockDeltas.Keys.ToArray();
            var products = await db.Products.Where(x => ids.Contains(x.Id)).ToListAsync(ct);

            foreach (var (productId, quantity) in stockDeltas)
            {
                var product = products.FirstOrDefault(x => x.Id == productId)
                    ?? throw new ShopRuleException("A cup in your basket is no longer available.");
                if (product.Stock < quantity)
                {
                    throw new ShopRuleException($"Only {product.Stock} of {product.Name} left. Lower the quantity and try again.");
                }

                product.Stock -= quantity;
            }

            var order = new Order
            {
                UserId = userId,
                Total = total,
                PlacedAt = DateTime.UtcNow,
                Status = OrderStatuses.Pending,
                Items = items.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    Name = x.Name,
                    UnitPrice = x.UnitPrice,
                    Quantity = x.Quantity
                }).ToList()
            };

            db.Orders.Add(order);

            var cart = await db.CartItems.Where(x => x.UserId == userId).ToListAsync(ct);
            db.CartItems.RemoveRange(cart);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ShopRuleException("Someone else bought the last one while you were checking out. Review your basket.");
            }

            await transaction.CommitAsync(ct);
            orderId = order.Id;
        });

        return orderId;
    }

    public async Task<IReadOnlyList<Order>> ListForUserAsync(int userId, CancellationToken ct = default) =>
        await db.Orders.AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.PlacedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> ListAllAsync(CancellationToken ct = default) =>
        await db.Orders.AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.User)
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.PlacedAt)
            .ToListAsync(ct);

    public Task<Order?> FindAsync(int id, CancellationToken ct = default) =>
        db.Orders.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> ShipAsync(int id, CancellationToken ct = default)
    {
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (order is null || order.Status != OrderStatuses.Pending) return false;

        order.Status = OrderStatuses.Shipped;
        order.ShippedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
