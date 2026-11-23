using System.ComponentModel.DataAnnotations;

namespace BookMatrix.Models;

public class AdminAccount
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string NormalizedUsername { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}