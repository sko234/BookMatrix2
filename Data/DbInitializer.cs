using BookMatrix.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookMatrix.Data;

public static class DbInitializer
{
    /// <summary>Creates the SQLite database on first run and fills it with the demo catalog.</summary>
    public static void Initialize(
        BookMatrixContext db,
        IConfiguration configuration,
        IPasswordHasher<AdminAccount> passwordHasher)
    {
        db.Database.EnsureCreated();
        UpgradeLegacyDatabase(db);

        if (!db.Books.Any())
        {
            db.Books.AddRange(CatalogSeed.BuildBooks(DateTime.UtcNow));
            db.SaveChanges();
        }

        SeedAdmins(db, configuration, passwordHasher);
    }

    private static void SeedAdmins(
        BookMatrixContext db,
        IConfiguration configuration,
        IPasswordHasher<AdminAccount> passwordHasher)
    {
        for (var index = 1; index <= 2; index++)
        {
            var username = configuration[$"AdminAccounts:Admin{index}:Username"]?.Trim();
            var password = configuration[$"AdminAccounts:Admin{index}:Password"];
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
            {
                continue;
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException($"Admin{index} username and password must both be configured.");
            }

            var normalizedUsername = username.ToUpperInvariant();
            var admin = db.AdminAccounts.FirstOrDefault(account => account.NormalizedUsername == normalizedUsername);
            if (admin == null)
            {
                admin = new AdminAccount { Username = username, NormalizedUsername = normalizedUsername };
                admin.PasswordHash = passwordHasher.HashPassword(admin, password);
                db.AdminAccounts.Add(admin);
                continue;
            }

            var verification = passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, password);
            if (verification is PasswordVerificationResult.Failed or PasswordVerificationResult.SuccessRehashNeeded)
            {
                admin.Username = username;
                admin.PasswordHash = passwordHasher.HashPassword(admin, password);
            }
        }

        db.SaveChanges();
    }

    private static void UpgradeLegacyDatabase(BookMatrixContext db)
    {
        using var transaction = db.Database.BeginTransaction();
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "Users" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
                "FullName" TEXT NOT NULL,
                "Email" TEXT NOT NULL,
                "NormalizedEmail" TEXT NOT NULL,
                "PasswordHash" TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_NormalizedEmail"
                ON "Users" ("NormalizedEmail");
            CREATE TABLE IF NOT EXISTS "AdminAccounts" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_AdminAccounts" PRIMARY KEY AUTOINCREMENT,
                "Username" TEXT NOT NULL,
                "NormalizedUsername" TEXT NOT NULL,
                "PasswordHash" TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_AdminAccounts_NormalizedUsername"
                ON "AdminAccounts" ("NormalizedUsername");
            """);

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var connection = db.Database.GetDbConnection();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
            command.CommandText = "PRAGMA table_info('Books');";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                columns.Add(reader.GetString(1));
            }
        }

        if (!columns.Contains("BorrowerId"))
        {
            db.Database.ExecuteSqlRaw("ALTER TABLE \"Books\" ADD COLUMN \"BorrowerId\" INTEGER NULL REFERENCES \"Users\" (\"Id\");");
        }

        if (!columns.Contains("AdminBorrowerId"))
        {
            db.Database.ExecuteSqlRaw("ALTER TABLE \"Books\" ADD COLUMN \"AdminBorrowerId\" INTEGER NULL REFERENCES \"AdminAccounts\" (\"Id\");");
        }

        if (columns.Contains("ReaderId"))
        {
            db.Database.ExecuteSqlRaw("UPDATE \"Books\" SET \"ReaderId\" = NULL, \"BorrowedOn\" = NULL;");
            db.Database.ExecuteSqlRaw("DELETE FROM \"Readers\";");
        }

        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS \"IX_Books_BorrowerId\" ON \"Books\" (\"BorrowerId\");");
        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS \"IX_Books_AdminBorrowerId\" ON \"Books\" (\"AdminBorrowerId\");");
        transaction.Commit();
    }
}
