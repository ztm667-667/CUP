using System.Security.Claims;
using HearthCup.Models;
using HearthCup.Utility.Constants;
using HearthCup.Utility.Services;
using HearthCup.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HearthCup.Web.Controllers;

public sealed class AccountController(AccountService accounts) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl) =>
        View(new LoginInput { ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null });

    [HttpPost, EnableRateLimiting(SD.LoginRateLimitPolicy)]
    public async Task<IActionResult> Login(LoginInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(input);

        var user = await accounts.SignInAsync(input.Email, input.Password, ct);
        if (user is null)
        {
            // One generic message: it never reveals whether the email exists.
            ModelState.AddModelError("", "The email address or password is incorrect.");
            return View(input);
        }

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            },
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (Url.IsLocalUrl(input.ReturnUrl)) return LocalRedirect(input.ReturnUrl!);

        return user.Role == Roles.Manager
            ? RedirectToAction("Index", "Manage")
            : RedirectToAction("Shop", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterInput());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterInput input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(input);

        try
        {
            var user = await accounts.RegisterAsync(input, ct);

            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                },
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            TempData[SD.TempDataSuccess] = "Welcome. Your account is ready.";
            return RedirectToAction("Shop", "Home");
        }
        catch (ShopRuleException e)
        {
            ModelState.AddModelError("", e.Message);
            return View(input);
        }
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
