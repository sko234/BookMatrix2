using BookMatrix.Models;

namespace BookMatrix.Services;

public enum BookChangeStatus
{
    Ok,
    BookNotFound,
    UserNotFound,
    AlreadyBorrowed
}

public class BookChangeResult
{
    public BookChangeStatus Status { get; set; }
    public BookDto? Book { get; set; }
    public int Free { get; set; }
}

public interface IBookService
{
    Task<BookPageDto> QueryAsync(string? search, string? filter, int page);
    Task<BookChangeResult> BorrowAsync(int bookId, int borrowerId);
    Task<BookChangeResult> BorrowAsAdminAsync(int bookId, int adminId);
    Task<BookChangeResult> ReturnAsync(int bookId);
}
