using BookMatrix.Data;
using BookMatrix.Models;
using Microsoft.EntityFrameworkCore;

namespace BookMatrix.Services;

public class BookService : IBookService
{
    public const int PageSize = 12;

    private readonly BookMatrixContext _db;

    public BookService(BookMatrixContext db)
    {
        _db = db;
    }

    public async Task<BookPageDto> QueryAsync(string? search, string? filter, int page)
    {
        IQueryable<Book> query = _db.Books.AsNoTracking();

        if (filter == "free")
        {
            query = query.Where(b => b.BorrowerId == null && b.AdminBorrowerId == null);
        }
        else if (filter == "out")
        {
            query = query.Where(b => b.BorrowerId != null || b.AdminBorrowerId != null);
        }

        // Same rule as the prototype: case-insensitive "contains" on "title author".
        var term = (search ?? string.Empty).Trim().ToLowerInvariant();
        if (term.Length > 0)
        {
            query = query.Where(b => b.SearchText.Contains(term));
        }

        var total = await query.CountAsync();
        var pages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Min(Math.Max(page, 1), pages);

        var books = await query
            .Include(b => b.Borrower)
            .Include(b => b.AdminBorrower)
            .OrderBy(b => b.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return new BookPageDto
        {
            Items = books.Select(ToDto).ToList(),
            Total = total,
            LibraryTotal = await _db.Books.CountAsync(),
            Free = await CountFreeAsync(),
            Page = page,
            Pages = pages
        };
    }

    public Task<BookChangeResult> BorrowAsync(int bookId, int borrowerId) =>
        BorrowAsync(bookId, borrowerId, null);

    public Task<BookChangeResult> BorrowAsAdminAsync(int bookId, int adminId) =>
        BorrowAsync(bookId, null, adminId);

    private async Task<BookChangeResult> BorrowAsync(int bookId, int? borrowerId, int? adminId)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        if (book == null)
        {
            return new BookChangeResult { Status = BookChangeStatus.BookNotFound };
        }

        UserAccount? borrower = null;
        AdminAccount? adminBorrower = null;
        if (borrowerId.HasValue)
        {
            borrower = await _db.UserAccounts.FirstOrDefaultAsync(user => user.Id == borrowerId.Value);
            if (borrower == null)
            {
                return new BookChangeResult { Status = BookChangeStatus.UserNotFound };
            }
        }
        else if (adminId.HasValue)
        {
            adminBorrower = await _db.AdminAccounts.FirstOrDefaultAsync(admin => admin.Id == adminId.Value);
            if (adminBorrower == null)
            {
                return new BookChangeResult { Status = BookChangeStatus.UserNotFound };
            }
        }
        else
        {
            return new BookChangeResult { Status = BookChangeStatus.UserNotFound };
        }

        if (book.BorrowerId != null || book.AdminBorrowerId != null)
        {
            return new BookChangeResult { Status = BookChangeStatus.AlreadyBorrowed };
        }

        book.BorrowerId = borrower?.Id;
        book.Borrower = borrower;
        book.AdminBorrowerId = adminBorrower?.Id;
        book.AdminBorrower = adminBorrower;
        book.BorrowedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new BookChangeResult
        {
            Status = BookChangeStatus.Ok,
            Book = ToDto(book),
            Free = await CountFreeAsync()
        };
    }

    public async Task<BookChangeResult> ReturnAsync(int bookId)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        if (book == null)
        {
            return new BookChangeResult { Status = BookChangeStatus.BookNotFound };
        }

        book.BorrowerId = null;
        book.Borrower = null;
        book.AdminBorrowerId = null;
        book.AdminBorrower = null;
        book.BorrowedOn = null;
        await _db.SaveChangesAsync();

        return new BookChangeResult
        {
            Status = BookChangeStatus.Ok,
            Book = ToDto(book),
            Free = await CountFreeAsync()
        };
    }

    private Task<int> CountFreeAsync() => _db.Books.CountAsync(b => b.BorrowerId == null && b.AdminBorrowerId == null);

    private static BookDto ToDto(Book b)
    {
        var borrowed = b.BorrowerId != null || b.AdminBorrowerId != null;
        return new BookDto
        {
            Id = b.Id,
            Code = "BK-" + b.Id.ToString("D5"),
            Title = b.Title,
            Author = b.Author,
            Publisher = b.Publisher,
            Year = b.Year,
            IsBorrowed = borrowed,
            // SQLite hands DateTime back as "Unspecified"; it was stored as UTC, so mark it as such.
            BorrowedOn = borrowed && b.BorrowedOn.HasValue ? DateTime.SpecifyKind(b.BorrowedOn.Value, DateTimeKind.Utc) : null,
            BorrowerName = borrowed ? b.Borrower?.FullName ?? b.AdminBorrower?.Username : null
        };
    }
}
