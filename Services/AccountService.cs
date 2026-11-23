using BookMatrix.Data;
using BookMatrix.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookMatrix.Services;

public class AccountService : IAccountService
{
    private readonly BookMatrixContext _db;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public AccountService(BookMatrixContext db, IPasswordHasher<UserAccount> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserAccount?> RegisterAsync(string fullName, string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (await _db.UserAccounts.AnyAsync(user => user.NormalizedEmail == normalizedEmail))
        {
            return null;
        }

        var user = new UserAccount
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            NormalizedEmail = normalizedEmail
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _db.UserAccounts.Add(user);

        try
        {
            await _db.SaveChangesAsync();
            return user;
        }
        catch (DbUpdateException)
        {
            _db.Entry(user).State = EntityState.Detached;
            if (await _db.UserAccounts.AnyAsync(existing => existing.NormalizedEmail == normalizedEmail))
            {
                return null;
            }

            throw;
        }
    }

    public async Task<UserAccount?> AuthenticateAsync(string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await _db.UserAccounts.FirstOrDefaultAsync(account => account.NormalizedEmail == normalizedEmail);
        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _db.SaveChangesAsync();
        }

        return user;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}