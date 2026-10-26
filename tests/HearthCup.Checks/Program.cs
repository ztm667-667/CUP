using HearthCup.Models;
using HearthCup.Utility.Services;

// Business checks: no SQL Server, no web server, no browser. Fake ports stand in for the database so
// every rule in the business tier can be exercised on its own.
var repo = new FakeRepository();
var shop = new ShopService(repo);
var passed = 0;

async Task Check(string name, Func<Task> run)
{
    await run();
    Console.WriteLine($"PASS {name}");
    passed++;
}

void Assert(bool condition)
{
    if (!condition) throw new Exception("Assertion failed");
}

async Task Reject(Func<Task> run)
{
    try
    {
        await run();
    }
    catch (ShopRuleException)
    {
        return;
    }

    throw new Exception("Expected business rejection");
}

ProductInput Input() => new()
{
    Name = "Test family cup",
    Description = "A comfortable ceramic cup for the family table.",
    CapacityMl = 350,
    Price = 14,
    Stock = 4
};

// --- catalogue and basket rules -------------------------------------------------------------
await Check("Add accepts available quantity", async () => Assert((await shop.AddAsync([], 1, 2)).Single().Quantity == 2));
await Check("Add merges duplicate lines", async () => Assert((await shop.AddAsync([new(1, 2)], 1, 1)).Single().Quantity == 3));
await Check("Reject cumulative quantity above stock", () => Reject(async () => await shop.AddAsync([new(1, 3)], 1, 2)));
await Check("Reject zero quantity", () => Reject(async () => await shop.AddAsync([], 1, 0)));
await Check("Reject negative quantity", () => Reject(async () => await shop.AddAsync([], 1, -1)));
await Check("Reject excessive quantity", () => Reject(async () => await shop.AddAsync([], 1, 100)));
await Check("Reject nonexistent product", () => Reject(async () => await shop.AddAsync([], 999, 1)));
await Check("Reject sold-out product", () => Reject(async () => await shop.AddAsync([], 2, 1)));

await Check("Basket uses current server price", async () =>
{
    var cart = await shop.ReadCartAsync([new(1, 2)]);
    Assert(cart.Total == 28 && cart.Count == 2);

    repo.Products[0].Price = 15;
    cart = await shop.ReadCartAsync([new(1, 2)]);
    Assert(cart.Total == 30);
});

await Check("Basket clamps stock and removes deleted products", async () =>
{
    var cart = await shop.ReadCartAsync([new(1, 9), new(999, 1), new(2, 1)]);
    Assert(cart.Changed && cart.Lines.Count == 1 && cart.Count == 4);
});

await Check("Reject negative price", () => { var input = Input(); input.Price = -1; return Reject(async () => await shop.SaveAsync(null, input)); });
await Check("Reject fractional penny", () => { var input = Input(); input.Price = 1.001m; return Reject(async () => await shop.SaveAsync(null, input)); });
await Check("Reject negative stock", () => { var input = Input(); input.Stock = -1; return Reject(async () => await shop.SaveAsync(null, input)); });
await Check("Reject unapproved image path", () => { var input = Input(); input.Design = "../../private"; return Reject(async () => await shop.SaveAsync(null, input)); });

await Check("Create normalizes name and saves valid product", async () =>
{
    var input = Input();
    input.Name = "  Family cup  ";
    await shop.SaveAsync(null, input);
    Assert(repo.Saved?.Name == "Family cup");
});

await Check("Edit requires a valid concurrency token", () => Reject(async () => await shop.SaveAsync(1, Input())));
await Check("Delete rejects malformed version", () => Reject(async () => await shop.DeleteAsync(1, "not-base64")));

// --- accounts ------------------------------------------------------------------------------
var hasher = new FakeHasher();
var users = new FakeUserRepository();
var accounts = new AccountService(users, hasher);

RegisterInput Registration(string email = "buyer@example.com", string password = "123456") =>
    new() { Email = email, Password = password, ConfirmPassword = password };

await Check("Register rejects a five digit password", () => Reject(async () => await accounts.RegisterAsync(Registration(password: "12345"))));
await Check("Register rejects letters in the password", () => Reject(async () => await accounts.RegisterAsync(Registration(password: "12a456"))));
await Check("Register rejects a malformed email", () => Reject(async () => await accounts.RegisterAsync(Registration(email: "not-an-email"))));
await Check("Register stores a buyer with a hashed password", async () =>
{
    var user = await accounts.RegisterAsync(Registration());
    Assert(user.Role == Roles.Buyer && user.PasswordHash == "hashed:123456");
});

await Check("Register rejects a duplicate email", () => Reject(async () => await accounts.RegisterAsync(Registration())));
await Check("Sign in rejects a wrong password", async () => Assert(await accounts.SignInAsync("buyer@example.com", "999999") is null));
await Check("Sign in returns the matching account", async () => Assert((await accounts.SignInAsync("BUYER@example.com", "123456"))?.Email == "buyer@example.com"));

// --- orders --------------------------------------------------------------------------------
var orderProducts = new FakeRepository();
var orderSink = new FakeOrderRepository();
var orderService = new OrderService(orderSink, orderProducts);

await Check("Order rejects an empty basket", () => Reject(async () => await orderService.PlaceAsync(1, [])));
await Check("Order rejects a delisted cup", () => Reject(async () => await orderService.PlaceAsync(1, [new(3, 1)])));
await Check("Order rejects more than the stock", () => Reject(async () => await orderService.PlaceAsync(1, [new(1, 9)])));

await Check("Order snapshots the price and lowers the stock", async () =>
{
    var id = await orderService.PlaceAsync(1, [new(1, 2), new(4, 1)]);
    Assert(id == 99);
    Assert(orderSink.Total == 38);
    Assert(orderSink.Items?.Count == 2);
    Assert(orderSink.Items![0].UnitPrice == 14);
    Assert(orderSink.Items[0].Name == "Sunday Mug");
    Assert(orderSink.Deltas is { Count: 2 } && orderSink.Deltas[1] == 2 && orderSink.Deltas[4] == 1);
});

await Check("Ship moves a pending order once", async () =>
{
    var shipped = await orderService.ShipAsync(7);
    Assert(shipped);
});

Console.WriteLine($"{passed} business checks passed.");

sealed class FakeHasher : IPasswordHasher
{
    public string Hash(string password) => "hashed:" + password;

    public bool Verify(string passwordHash, string password) => passwordHash == "hashed:" + password;
}

sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> users = [];
    private int nextId = 1;

    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default) =>
        Task.FromResult(users.FirstOrDefault(x => x.Email == email));

    public Task<User?> FindByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(users.FirstOrDefault(x => x.Id == id));

    public Task<bool> ExistsAsync(string email, CancellationToken ct = default) =>
        Task.FromResult(users.Any(x => x.Email == email));

    public Task<int> AddAsync(User user, CancellationToken ct = default)
    {
        user.Id = nextId++;
        users.Add(user);
        return Task.FromResult(user.Id);
    }

    public Task<User> EnsureManagerAsync(string email, string passwordHash, CancellationToken ct = default) =>
        throw new NotSupportedException("Seeding is exercised by the database, not by these checks.");
}

sealed class FakeOrderRepository : IOrderRepository
{
    public IReadOnlyList<OrderItem>? Items { get; private set; }

    public decimal Total { get; private set; }

    public IReadOnlyDictionary<int, int>? Deltas { get; private set; }

    public Task<int> PlaceAsync(
        int userId,
        IReadOnlyList<OrderItem> items,
        decimal total,
        IReadOnlyDictionary<int, int> stockDeltas,
        CancellationToken ct = default)
    {
        Items = items;
        Total = total;
        Deltas = stockDeltas;
        return Task.FromResult(99);
    }

    public Task<IReadOnlyList<Order>> ListForUserAsync(int userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Order>>([]);

    public Task<IReadOnlyList<Order>> ListAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Order>>([]);

    public Task<Order?> FindAsync(int id, CancellationToken ct = default) => Task.FromResult<Order?>(null);

    public Task<bool> ShipAsync(int id, CancellationToken ct = default) => Task.FromResult(true);
}

sealed class FakeRepository : IProductRepository
{
    public List<Product> Products { get; } =
    [
        new() { Id = 1, Name = "Sunday Mug", Price = 14, Stock = 4, IsListed = true },
        new() { Id = 2, Name = "Sold-out Mug", Price = 20, Stock = 0, IsListed = true },
        new() { Id = 3, Name = "Retired Cup", Price = 10, Stock = 5, IsListed = false },
        new() { Id = 4, Name = "Friday Cup", Price = 10, Stock = 5, IsListed = true }
    ];

    public ProductInput? Saved { get; private set; }

    public Task<Product?> FindAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Products.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Product>> FindManyAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Product>>(Products.Where(p => ids.Contains(p.Id)).ToArray());

    public Task<CataloguePage> SearchAsync(CatalogueQuery q, CancellationToken ct = default) =>
        Task.FromResult(new CataloguePage([], 0, 1, 8));

    public Task<int> SaveAsync(int? id, ProductInput input, byte[]? version, CancellationToken ct = default)
    {
        Saved = input;
        return Task.FromResult(id ?? 3);
    }

    public Task DeleteAsync(int id, byte[] version, CancellationToken ct = default) => Task.CompletedTask;

    public Task SetListedAsync(int id, bool listed, CancellationToken ct = default)
    {
        var product = Products.FirstOrDefault(x => x.Id == id);
        if (product is not null) product.IsListed = listed;
        return Task.CompletedTask;
    }
}
