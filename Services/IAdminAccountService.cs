using BookMatrix.Models;

namespace BookMatrix.Services;

public interface IAdminAccountService
{
    Task<AdminAccount?> AuthenticateAsync(string username, string password);
}