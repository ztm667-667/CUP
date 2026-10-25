using HearthCup.Models;
using HearthCup.Utility.Constants;
using HearthCup.Utility.Services;
using HearthCup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HearthCup.Web.Controllers;

/// <summary>Product management. Every action, including the GET forms, requires the manager role.</summary>
[Authorize(Roles = SD.RoleManager)]
public sealed class ManageController(ShopService shop, ILogger<ManageController> logger) : Controller
{
    public async Task<IActionResult> Index([FromQuery] CatalogueQuery query, CancellationToken ct) =>
        View(new BrowseView(query, await shop.BrowseAsync(query, ct)));

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
