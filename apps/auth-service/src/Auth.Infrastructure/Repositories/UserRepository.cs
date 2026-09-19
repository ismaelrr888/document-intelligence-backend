using Microsoft.Extensions.Logging;
using Auth.Application.Interfaces;
using Auth.Domain.User;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class UserRepository: IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly AuthDbContext _dbContext;
    
    public UserRepository(ILogger<UserRepository> logger,
        AuthDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Searching user with email {Email}", email);

        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);
    }
}