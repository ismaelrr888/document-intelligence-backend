using Auth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Auth.Tests.Integration;

/// <summary>
/// Same setup as <see cref="AuthApiFactory"/> but with a deliberately low login
/// rate limit, so tests can trip it deterministically in a couple of requests.
/// </summary>
public sealed class RateLimitedAuthApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-only-signing-secret-not-for-production-use-0123456789",
                ["Jwt:Issuer"] = "Auth.Tests.Issuer",
                ["Jwt:Audience"] = "Auth.Tests.Audience",
                ["Jwt:ExpirationMinutes"] = "60",
                ["RateLimiting:LoginPermitLimit"] = "2",
                ["RateLimiting:LoginWindowSeconds"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AuthDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AuthDbContext>>();

            services.AddDbContext<AuthDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
