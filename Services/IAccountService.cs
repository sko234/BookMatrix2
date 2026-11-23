using BookMatrix.Models;

namespace BookMatrix.Services;

public interface IAccountService
{
    Task<UserAccount?> RegisterAsync(string fullName, string email, string password);
    Task<UserAccount?> AuthenticateAsync(string email, string password);
}