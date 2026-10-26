using System.Security.Claims;
using HearthCup.Models;
using HearthCup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HearthCup.Web.Controllers;

/// <summary>A buyer only ever sees their own orders.</summary>
[Authorize(Roles = Roles.Buyer)]
public sealed class OrdersController(IOrderRepository orders) : Controller
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await orders.ListForUserAsync(UserId, ct);
        return View(list.Select(OrderView.From).ToArray());
    }
}
