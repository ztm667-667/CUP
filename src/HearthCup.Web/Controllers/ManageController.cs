using HearthCup.Application;
using HearthCup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HearthCup.Web.Controllers;
[Authorize(Roles="Manager")]
public sealed class ManageController(ShopService shop,ILogger<ManageController> logger):Controller
{
 public async Task<IActionResult> Index([FromQuery]CatalogueQuery query,CancellationToken ct)=>View(new BrowseView(query,await shop.BrowseAsync(query,ct)));
 [HttpGet] public IActionResult Create()=>View("Edit",new ProductInput());
 [HttpPost] public async Task<IActionResult> Create(ProductInput input,CancellationToken ct)=>await Save(null,input,ct);
 [HttpGet] public async Task<IActionResult> Edit(int id,CancellationToken ct)
 {
  var p=await shop.FindAsync(id,ct);if(p is null)return NotFound();
  ViewData["Id"]=id;return View(new ProductInput{Name=p.Name,Category=p.Category,Material=p.Material,Colour=p.Colour,CapacityMl=p.CapacityMl,Price=p.Price,Stock=p.Stock,Description=p.Description,Design=p.Design,Version=p.Version});
 }
 [HttpPost] public async Task<IActionResult> Edit(int id,ProductInput input,CancellationToken ct)=>await Save(id,input,ct);
 private async Task<IActionResult> Save(int? id,ProductInput input,CancellationToken ct)
 {
  ViewData["Id"]=id;
  if(ModelState.IsValid){try{var saved=await shop.SaveAsync(id,input,ct);logger.LogInformation("Manager {Operation} product {ProductId}",id.HasValue?"updated":"created",saved);TempData["Success"]="Product saved.";return RedirectToAction(nameof(Index));}catch(ShopRuleException e){ModelState.AddModelError("",e.Message);}}
  return View("Edit",input);
 }
 [HttpGet] public async Task<IActionResult> Delete(int id,CancellationToken ct)=>(await shop.FindAsync(id,ct)) is {} p?View(p):NotFound();
 [HttpPost,ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id,string version,CancellationToken ct)
 {
  try{await shop.DeleteAsync(id,version,ct);logger.LogInformation("Manager deleted product {ProductId}",id);TempData["Success"]="Product deleted.";}catch(ShopRuleException e){TempData["Error"]=e.Message;}
  return RedirectToAction(nameof(Index));
 }
}
