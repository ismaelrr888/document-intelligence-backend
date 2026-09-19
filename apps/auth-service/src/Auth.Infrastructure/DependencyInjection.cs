using Auth.Application.Abstractions.Security;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}