using HearthCup.Models;
using HearthCup.Utility.Constants;

namespace HearthCup.Utility.Services;

/// <summary>
/// Turns a basket into an order. Every line is re-checked against the live product before anything is
/// written, and the price is copied into the order so history cannot be rewritten later.
/// </summary>
public sealed class OrderService(IOrderRepository orders, IProductRepository products)
{
    public async Task<int> PlaceAsync(int userId, IReadOnlyList<CartEntry> entries, CancellationToken ct = default)
    {
        if (entries.Count == 0) throw new ShopRuleException("Your basket is empty.");

        var found = await products.FindManyAsync(entries.Select(x => x.ProductId), ct);
        var items = new List<OrderItem>();
        var deltas = new Dictionary<int, int>();

        foreach (var entry in entries)
        {
            if (entry.Quantity < 1 || entry.Quantity > SD.MaxQuantity)
            {
                throw new ShopRuleException($"Choose a quantity between 1 and {SD.MaxQuantity}.");
            }

            var product = found.FirstOrDefault(x => x.Id == entry.ProductId)
                ?? throw new ShopRuleException("A cup in your basket is no longer available.");

            if (!product.IsListed)
            {
                throw new ShopRuleException($"{product.Name} is no longer for sale.");
            }

            if (product.Stock < entry.Quantity)
            {
                throw new ShopRuleException($"Only {product.Stock} of {product.Name} left. Lower the quantity.");
            }

            items.Add(new OrderItem
            {
                ProductId = product.Id,
                Name = product.Name,
                UnitPrice = product.Price,
                Quantity = entry.Quantity
            });

            deltas[product.Id] = entry.Quantity;
        }

        return await orders.PlaceAsync(userId, items, items.Sum(x => x.UnitPrice * x.Quantity), deltas, ct);
    }

    public Task<bool> ShipAsync(int orderId, CancellationToken ct = default) => orders.ShipAsync(orderId, ct);
}
