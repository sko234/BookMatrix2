using System.ComponentModel.DataAnnotations;

namespace BookMatrix.Models;

public class Book
{
    /// <summary>Numeric key. The public ID shown in the catalog is "BK-" + Id padded to 5 digits.</summary>
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Author { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Publisher { get; set; } = string.Empty;

    public int Year { get; set; }

    /// <summary>Lower-cased "title author" text used by the catalog search.</summary>
    [Required, MaxLength(450)]
    public string SearchText { get; set; } = string.Empty;

    /// <summary>The account that currently has the book (null = available).</summary>
    public int? BorrowerId { get; set; }
    public UserAccount? Borrower { get; set; }
    public int? AdminBorrowerId { get; set; }
    public AdminAccount? AdminBorrower { get; set; }

    /// <summary>When the book was borrowed (UTC), null when available.</summary>
    public DateTime? BorrowedOn { get; set; }
}
