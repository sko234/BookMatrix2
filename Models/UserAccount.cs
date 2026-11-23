using System.ComponentModel.DataAnnotations;

namespace BookMatrix.Models;

public class UserAccount
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(254)]
    public string NormalizedEmail { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}