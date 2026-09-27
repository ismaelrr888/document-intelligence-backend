using System.IdentityModel.Tokens.Jwt;
using Auth.Domain.User;
using Auth.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Xunit;

namespace Auth.Tests.Unit;

public sealed class JwtTokenServiceTests
{
    private static JwtTokenService CreateSut(int expirationMinutes = 60) =>
        new(Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Secret = "unit-test-signing-secret-0123456789-abcdefghijk",
            ExpirationMinutes = expirationMinutes
        }));

    [Fact]
    public void GenerateToken_Includes_Expected_Claims_And_Metadata()
    {
        var sut = CreateSut(expirationMinutes: 30);
        var user = new User("someone@example.com", "hash");

        var (token, expiresAtUtc) = sut.GenerateToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Email, jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.True(expiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(29));
        Assert.True(expiresAtUtc <= DateTimeOffset.UtcNow.AddMinutes(30).AddSeconds(5));
    }
}
