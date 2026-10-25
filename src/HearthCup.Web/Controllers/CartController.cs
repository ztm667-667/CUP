using HearthCup.Models;
using HearthCup.Utility.Constants;
using HearthCup.Utility.Services;
using HearthCup.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HearthCup.Web.Controllers;

public sealed class CartController(ShopService shop, BasketStore basket) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cart = await shop.ReadCartAsync(basket.Read(), ct);
        basket.Save(cart.Lines.Select(x => new CartEntry(x.Product.Id, x.Quantity)));
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int id, int quantity = 1, string? returnUrl = null, CancellationToken ct = default)
    {
        try
        {
            basket.Save(await shop.AddAsync(basket.Read(), id, quantity, ct));
            TempData[SD.TempDataSuccess] = "A little comfort, added to your basket.";
        }
        catch (ShopRuleException e)
        {
            TempData[SD.TempDataError] = e.Message;
        }

        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(int id)
    {
        basket.Save(basket.Read().Where(x => x.ProductId != id));
        TempData[SD.TempDataSuccess] = "Cup removed from your basket.";
        return RedirectToAction(nameof(Index));
    }
}
