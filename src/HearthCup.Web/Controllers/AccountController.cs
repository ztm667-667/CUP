using System.Security.Claims;
using HearthCup.Web.Models;
using HearthCup.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace HearthCup.Web.Controllers;
public sealed class AccountController(AdminCredentials credentials):Controller
{
 [HttpGet] public IActionResult Login(string? returnUrl)=>View(new LoginInput{ReturnUrl=Url.IsLocalUrl(returnUrl)?returnUrl:null});
 [HttpPost,EnableRateLimiting("login")] public async Task<IActionResult> Login(LoginInput input)
 {
  if(!ModelState.IsValid)return View(input);
  if(!credentials.Validate(input.Username,input.Password)){ModelState.AddModelError("","The username or password is incorrect.");return View(input);}
  var identity=new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,AdminCredentials.UserName),new Claim(ClaimTypes.Role,"Manager")},CookieAuthenticationDefaults.AuthenticationScheme);
  await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(identity));
  return Url.IsLocalUrl(input.ReturnUrl)?LocalRedirect(input.ReturnUrl!):RedirectToAction("Index","Manage");
 }
 [Authorize,HttpPost] public async Task<IActionResult> Logout(){await HttpContext.SignOutAsync();return RedirectToAction("Index","Home");}
}
