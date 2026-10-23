using System.ComponentModel.DataAnnotations;
using HearthCup.Application;
namespace HearthCup.Web.Models;
public sealed record BrowseView(CatalogueQuery Query,CataloguePage Catalogue);
public sealed class LoginInput
{
 [Required,StringLength(80)] public string Username{get;set;}="";
 [Required,StringLength(200)] public string Password{get;set;}="";
 public string? ReturnUrl{get;set;}
}
