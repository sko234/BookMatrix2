using BookMatrix.Data;
using BookMatrix.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookMatrix.Services;

public class AdminAccountService : IAdminAccountService
{
    private readonly BookMatrixContext _db;
    private readonly IPasswordHasher<AdminAccount> _passwordHasher;

    public AdminAccountService(BookMatrixContext db, IPasswordHasher<AdminAccount> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<AdminAccount?> AuthenticateAsync(string username, string password)
    {
        var normalizedUsername = username.Trim().ToUpperInvariant();
        var admin = await _db.AdminAccounts.FirstOrDefaultAsync(account => account.NormalizedUsername == normalizedUsername);
        if (admin == null)
        {
            return null;
        }

        var verification = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            admin.PasswordHash = _passwordHasher.HashPassword(admin, password);
            await _db.SaveChangesAsync();
        }

        return admin;
    }
}