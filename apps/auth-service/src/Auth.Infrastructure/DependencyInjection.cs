using Auth.Application.Abstractions.Security;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
        )
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        var connectionString = configuration.GetConnectionString("AuthDatabase");
        
        services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            }
        );   

        return services;
    }
}