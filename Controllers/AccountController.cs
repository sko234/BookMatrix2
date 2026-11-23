using System.Security.Claims;
using BookMatrix.Models;
using BookMatrix.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookMatrix.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accounts;
    private readonly UiText _text;

    public AccountController(IAccountService accounts, UiText text)
    {
        _accounts = accounts;
        _text = text;
    }

    [HttpGet("signup")]
    public IActionResult SignUp(string? returnUrl = null) => View(new SignUpInputModel { ReturnUrl = returnUrl });

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null) => View(new LoginInputModel { ReturnUrl = returnUrl });

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp(SignUpInputModel model)
    {
        if (ModelState.IsValid && !IsStrongPassword(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), _text["passwordError"]);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _accounts.RegisterAsync(model.FullName, model.Email, model.Password);
        if (user == null)
        {
            ViewData["AuthError"] = _text["emailExists"];
            return View(model);
        }

        await SignInAsync(user, rememberMe: false);
        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _accounts.AuthenticateAsync(model.Email, model.Password);
        if (user == null)
        {
            ViewData["AuthError"] = _text["invalidCredentials"];
            return View(model);
        }

        await SignInAsync(user, model.RememberMe);
        return RedirectToLocal(model.ReturnUrl);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInAsync(UserAccount user, bool rememberMe)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = rememberMe });
    }

    private IActionResult RedirectToLocal(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Home");

    private static bool IsStrongPassword(string password) =>
        password.Length >= 6 &&
        password.Any(char.IsUpper) &&
        password.Any(char.IsLower) &&
        password.Any(char.IsDigit) &&
        password.Any(character => !char.IsLetterOrDigit(character));
}
