using BookMatrix.Models;
using Microsoft.EntityFrameworkCore;

namespace BookMatrix.Data;

public class BookMatrixContext : DbContext
{
    public BookMatrixContext(DbContextOptions<BookMatrixContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<AdminAccount> AdminAccounts => Set<AdminAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>()
            .ToTable("Users")
            .HasIndex(user => user.NormalizedEmail)
            .IsUnique();
        modelBuilder.Entity<AdminAccount>()
            .HasIndex(admin => admin.NormalizedUsername)
            .IsUnique();
    }
}
