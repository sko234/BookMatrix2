using System.Security.Claims;
using BookMatrix.Models;
using BookMatrix.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BookMatrix.Controllers;

public class AdminController : Controller
{
    private readonly IAdminAccountService _admins;
    private readonly IAdminDashboardService _dashboard;
    private readonly UiText _text;

    public AdminController(IAdminAccountService admins, IAdminDashboardService dashboard, UiText text)
    {
        _admins = admins;
        _dashboard = dashboard;
        _text = text;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Dashboard()
    {
        if (!User.IsInRole("Admin"))
        {
            return RedirectToAction(nameof(Login));
        }

        return View("Index", await _dashboard.GetDashboardAsync());
    }

    [HttpGet("/admin-login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction(nameof(Dashboard));
        }

        return View(new AdminLoginInputModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/admin-login")]
    public async Task<IActionResult> Login(AdminLoginInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var admin = await _admins.AuthenticateAsync(model.Username, model.Password);
        if (admin == null)
        {
            ModelState.AddModelError(string.Empty, _text["adminLoginError"]);
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, $"admin:{admin.Id}"),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = model.RememberMe });

        return Url.IsLocalUrl(model.ReturnUrl)
            ? Redirect(model.ReturnUrl!)
            : RedirectToAction(nameof(Dashboard));
    }
}