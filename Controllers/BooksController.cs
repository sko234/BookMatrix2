using BookMatrix.Models;
using BookMatrix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookMatrix.Controllers;

/// <summary>JSON endpoints used by the catalog (search, filter, paging, borrow and return).</summary>
[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _books;

    public BooksController(IBookService books)
    {
        _books = books;
    }

    [HttpGet]
    public async Task<ActionResult<BookPageDto>> List([FromQuery] string? search, [FromQuery] string? filter, [FromQuery] int page = 1)
    {
        return await _books.QueryAsync(search, filter, page);
    }

    [Authorize]
    [HttpPost("{id:int}/borrow")]
    public async Task<ActionResult<BookChangeResponse>> Borrow(int id)
    {
        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        BookChangeResult result;
        if (User.IsInRole("Admin"))
        {
            const string adminPrefix = "admin:";
            if (nameIdentifier == null || !nameIdentifier.StartsWith(adminPrefix, StringComparison.Ordinal) ||
                !int.TryParse(nameIdentifier[adminPrefix.Length..], out var adminId))
            {
                return Unauthorized();
            }

            result = await _books.BorrowAsAdminAsync(id, adminId);
        }
        else
        {
            if (!int.TryParse(nameIdentifier, out var borrowerId))
            {
                return Unauthorized();
            }

            result = await _books.BorrowAsync(id, borrowerId);
        }

        return result.Status switch
        {
            BookChangeStatus.Ok => new BookChangeResponse { Book = result.Book!, Free = result.Free },
            BookChangeStatus.BookNotFound => NotFound(),
            BookChangeStatus.UserNotFound => Unauthorized(),
            _ => Conflict(new { error = "The book is already borrowed." })
        };
    }

    [Authorize]
    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<BookChangeResponse>> Return(int id)
    {
        var result = await _books.ReturnAsync(id);
        if (result.Status != BookChangeStatus.Ok)
        {
            return NotFound();
        }

        return new BookChangeResponse { Book = result.Book!, Free = result.Free };
    }
}
