using System.Text.Json;
using HearthCup.Models;
using HearthCup.Utility.Constants;

namespace HearthCup.Web.Services;

/// <summary>
/// The visitor's basket lives in session and holds product ids plus quantities only. No price is ever
/// stored on the client, so a tampered basket cannot change what a cup costs.
/// </summary>
public sealed class BasketStore(IHttpContextAccessor accessor)
{
    private ISession Session => accessor.HttpContext!.Session;

    public IReadOnlyList<CartEntry> Read() =>
        JsonSerializer.Deserialize<List<CartEntry>>(Session.GetString(SD.SessionBasket) ?? "[]") ?? [];

    public void Save(IEnumerable<CartEntry> entries) =>
        Session.SetString(SD.SessionBasket, JsonSerializer.Serialize(entries));
}
