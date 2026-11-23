using BookMatrix.Data;
using BookMatrix.Models;
using Microsoft.EntityFrameworkCore;

namespace BookMatrix.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly BookMatrixContext _db;

    public AdminDashboardService(BookMatrixContext db)
    {
        _db = db;
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var users = await _db.UserAccounts
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .Select(user => new AdminUserRow
            {
                Email = user.Email,
                Username = user.FullName,
                BorrowedBookCount = _db.Books.Count(book => book.BorrowerId == user.Id)
            })
            .ToListAsync();

        var borrowedBooks = await _db.Books
            .AsNoTracking()
            .Where(book => book.BorrowerId != null || book.AdminBorrowerId != null)
            .OrderBy(book => book.Title)
            .Select(book => new AdminBorrowedBookRow
            {
                Title = book.Title,
                Author = book.Author,
                BorrowerName = book.BorrowerId != null
                    ? book.Borrower!.FullName
                    : book.AdminBorrower!.Username
            })
            .ToListAsync();

        return new AdminDashboardViewModel
        {
            Users = users,
            BorrowedBooks = borrowedBooks
        };
    }
}