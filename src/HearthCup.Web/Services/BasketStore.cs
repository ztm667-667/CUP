using System.Text.Json;
using HearthCup.Application;
namespace HearthCup.Web.Services;
public sealed class BasketStore(IHttpContextAccessor accessor)
{
 private ISession Session=>accessor.HttpContext!.Session;
 public IReadOnlyList<CartEntry> Read()=>JsonSerializer.Deserialize<List<CartEntry>>(Session.GetString("basket")??"[]")??[];
 public void Save(IEnumerable<CartEntry> entries)=>Session.SetString("basket",JsonSerializer.Serialize(entries));
}
