using System.Diagnostics;
using BookMatrix.Models;
using BookMatrix.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookMatrix.Controllers;

public class HomeController : Controller
{
    private readonly IBookService _books;

    public HomeController(IBookService books)
    {
        _books = books;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            Initial = await _books.QueryAsync(null, "all", 1),
            PerPage = BookService.PageSize,
            CanBorrow = User.Identity?.IsAuthenticated == true
        };
        return View(model);
    }

    [IgnoreAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
