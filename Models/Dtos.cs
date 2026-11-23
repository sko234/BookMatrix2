namespace BookMatrix.Models;

public class BookDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public int Year { get; set; }
    public bool IsBorrowed { get; set; }
    public DateTime? BorrowedOn { get; set; }
    public string? BorrowerName { get; set; }
}

public class BookPageDto
{
    public List<BookDto> Items { get; set; } = new();

    /// <summary>Number of books matching the current search and filter.</summary>
    public int Total { get; set; }

    /// <summary>Number of books in the whole library.</summary>
    public int LibraryTotal { get; set; }

    /// <summary>Number of available books in the whole library.</summary>
    public int Free { get; set; }

    public int Page { get; set; }
    public int Pages { get; set; }
}

public class BookChangeResponse
{
    public BookDto Book { get; set; } = new();
    public int Free { get; set; }
}
