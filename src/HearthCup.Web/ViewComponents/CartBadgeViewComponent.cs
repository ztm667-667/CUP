using System.Security.Claims;
using HearthCup.Models;
using Microsoft.AspNetCore.Mvc;

namespace HearthCup.Web.ViewComponents;

/// <summary>Renders the number of cups in the signed-in buyer's basket for the header link.</summary>
public sealed class CartBadgeViewComponent(ICartRepository cart) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var claim = ViewContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var userId)) return Content("0");

        var count = (await cart.ReadAsync(userId)).Sum(x => x.Quantity);
        return Content(count.ToString());
    }
}
