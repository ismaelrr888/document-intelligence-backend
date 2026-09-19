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
}