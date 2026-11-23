namespace BookMatrix.Models;

public class AdminDashboardViewModel
{
    public List<AdminUserRow> Users { get; set; } = new();
    public List<AdminBorrowedBookRow> BorrowedBooks { get; set; } = new();
}

public class AdminUserRow
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int BorrowedBookCount { get; set; }
}

public class AdminBorrowedBookRow
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
}