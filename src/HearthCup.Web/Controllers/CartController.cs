using System.Security.Claims;
using HearthCup.Models;
using HearthCup.Utility.Constants;
using HearthCup.Utility.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HearthCup.Web.Controllers;

/// <summary>Only a signed-in buyer has a basket; the rows live in the database, one set per account.</summary>
[Authorize(Roles = Roles.Buyer)]
public sealed class CartController(ShopService shop, OrderService orders, ICartRepository cart) : Controller
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var view = await shop.ReadCartAsync(await cart.ReadAsync(UserId, ct), ct);
        await cart.SaveAsync(UserId, view.Lines.Select(x => new CartEntry(x.Product.Id, x.Quantity)).ToArray(), ct);
        return View(view);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int id, int quantity = 1, string? returnUrl = null, CancellationToken ct = default)
    {
        try
        {
            var updated = await shop.AddAsync(await cart.ReadAsync(UserId, ct), id, quantity, ct);
            await cart.SaveAsync(UserId, updated, ct);
            TempData[SD.TempDataSuccess] = "A little comfort, added to your basket.";
        }
        catch (ShopRuleException e)
        {
            TempData[SD.TempDataError] = e.Message;
        }

        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SetQuantity(int id, int quantity, CancellationToken ct)
    {
        try
        {
            var without = (await cart.ReadAsync(UserId, ct)).Where(x => x.ProductId != id).ToArray();

            if (quantity <= 0)
            {
                await cart.SaveAsync(UserId, without, ct);
                TempData[SD.TempDataSuccess] = "Cup removed from your basket.";
            }
            else
            {
                await cart.SaveAsync(UserId, await shop.AddAsync(without, id, quantity, ct), ct);
            }
        }
        catch (ShopRuleException e)
        {
            TempData[SD.TempDataError] = e.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        await cart.SaveAsync(UserId, (await cart.ReadAsync(UserId, ct)).Where(x => x.ProductId != id).ToArray(), ct);
        TempData[SD.TempDataSuccess] = "Cup removed from your basket.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        try
        {
            var orderId = await orders.PlaceAsync(UserId, await cart.ReadAsync(UserId, ct), ct);
            TempData[SD.TempDataSuccess] = $"Order #{orderId} placed. We will mark it shipped when it leaves the shop.";
            return RedirectToAction("Index", "Orders");
        }
        catch (ShopRuleException e)
        {
            TempData[SD.TempDataError] = e.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
