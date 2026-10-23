using HearthCup.Application;
using HearthCup.Web.Models;
using Microsoft.AspNetCore.Mvc;
namespace HearthCup.Web.Controllers;
public sealed class HomeController(ShopService shop):Controller
{
 public IActionResult Index([FromQuery]CatalogueQuery query)
 {
  // Preserve old bookmarked catalogue query URLs while keeping the landing page separate.
  if(Request.Query.Count>0)return RedirectToAction(nameof(Shop),query);
  return View();
 }
 [HttpGet("/Shop")]
 public async Task<IActionResult> Shop([FromQuery]CatalogueQuery query,CancellationToken ct)=>View(new BrowseView(query,await shop.BrowseAsync(query,ct)));
 public async Task<IActionResult> Details(int id,CancellationToken ct)=>(await shop.FindAsync(id,ct)) is {} p?View(p):NotFound();
 [ResponseCache(Duration=0,Location=ResponseCacheLocation.None,NoStore=true)]
 public IActionResult Error(){Response.StatusCode=503;return View();}
}
