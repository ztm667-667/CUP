using HearthCup.Models;
using HearthCup.Utility.Constants;
using HearthCup.Utility.Services;
using HearthCup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HearthCup.Web.Controllers;

/// <summary>Product and order management. Every action, including the GET forms, requires the manager role.</summary>
[Authorize(Roles = Roles.Manager)]
public sealed class ManageController(
    ShopService shop,
    OrderService orders,
    IOrderRepository orderRepository,
    ILogger<ManageController> logger) : Controller
{
    public async Task<IActionResult> Index([FromQuery] CatalogueQuery query, CancellationToken ct)
    {
        query.IncludeHidden = true;
        var page = new BrowseView(query, await shop.BrowseAsync(query, ct));
        var all = await orderRepository.ListAllAsync(ct);

        return View(new ManageView(page, all.Count, all.Count(x => x.Status == OrderStatuses.Pending)));
    }

    [HttpGet]
    public IActionResult Create() => View("Edit", new ProductInput());

    [HttpPost]
    public async Task<IActionResult> Create(ProductInput input, CancellationToken ct) => await Save(null, input, ct);

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var product = await shop.FindAsync(id, ct);
        if (product is null) return NotFound();

        ViewData["Id"] = id;
        return View(new ProductInput
        {
            Name = product.Name,
            Category = product.Category,
            Material = product.Material,
            Colour = product.Colour,
            CapacityMl = product.CapacityMl,
            Price = product.Price,
            Stock = product.Stock,
            Description = product.Description,
            Design = product.Design,
            Version = product.Version
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, ProductInput input, CancellationToken ct) => await Save(id, input, ct);

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await shop.FindAsync(id, ct) is { } product ? View(product) : NotFound();

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, string version, CancellationToken ct)
    {
        try
        {
            await shop.DeleteAsync(id, version, ct);
            logger.LogInformation("Manager deleted product {ProductId}", id);
            TempData[SD.TempDataSuccess] = "Product deleted.";
        }
        catch (ShopRuleException e)
        {
            TempData[SD.TempDataError] = e.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Lists or delists a cup. Delisted cups stay in the database and in past orders.</summary>
    [HttpPost]
    public async Task<IActionResult> ToggleListed(int id, CancellationToken ct)
    {
        var product = await shop.FindAsync(id, ct);
        if (product is null) return NotFound();

        await shop.SetListedAsync(id, !product.IsListed, ct);
        logger.LogInformation("Manager set product {ProductId} listed={Listed}", id, !product.IsListed);
        TempData[SD.TempDataSuccess] = product.IsListed ? "Cup delisted: hidden from the shop." : "Cup listed: back in the shop.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Orders(CancellationToken ct)
    {
        var all = await orderRepository.ListAllAsync(ct);
        return View(all.Select(OrderView.From).ToArray());
    }

    [HttpPost]
    public async Task<IActionResult> Ship(int id, CancellationToken ct)
    {
        var shipped = await orders.ShipAsync(id, ct);
        TempData[shipped ? SD.TempDataSuccess : SD.TempDataError] =
            shipped ? $"Order #{id} marked as shipped." : $"Order #{id} cannot be shipped again.";

        return RedirectToAction(nameof(Orders));
    }

    private async Task<IActionResult> Save(int? id, ProductInput input, CancellationToken ct)
    {
        ViewData["Id"] = id;

        if (ModelState.IsValid)
        {
            try
            {
                var saved = await shop.SaveAsync(id, input, ct);
                logger.LogInformation("Manager {Operation} product {ProductId}", id.HasValue ? "updated" : "created", saved);
                TempData[SD.TempDataSuccess] = "Product saved.";
                return RedirectToAction(nameof(Index));
            }
            catch (ShopRuleException e)
            {
                // Business rejections are shown on the form, not turned into a 500.
                ModelState.AddModelError("", e.Message);
            }
        }

        return View("Edit", input);
    }
}
