using Auth.Application.Interfaces;
using Auth.Domain.User;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _dbContext;

    public RefreshTokenRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash);
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
        await _dbContext.SaveChangesAsync();
    }
}
