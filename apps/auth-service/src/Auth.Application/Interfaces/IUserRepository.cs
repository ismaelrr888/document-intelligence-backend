using Auth.Domain.User;

namespace Auth.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
}