using Auth.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public class AuthDbContext: DbContext
{
    public AuthDbContext(DbContextOptions options)
        : base(options)
    {
        
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.HasIndex(refreshToken => refreshToken.TokenHash).IsUnique();
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}