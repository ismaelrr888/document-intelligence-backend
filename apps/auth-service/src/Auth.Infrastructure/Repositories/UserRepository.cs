using Microsoft.Extensions.Logging;
using Auth.Application.Interfaces;
using Auth.Domain.User;

namespace Auth.Infrastructure.Repositories;

public class UserRepository: IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    
    public UserRepository(ILogger<UserRepository> logger)
    {
        _logger = logger;
    }
    
    public Task<User?> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Searching user with email {Email}", email);
        
        throw new NotImplementedException();
    }
}