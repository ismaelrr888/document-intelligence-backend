using Auth.Application.Abstractions.Security;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Domain.User;
using Moq;
using Xunit;

namespace Auth.Tests.Unit;

public sealed class AuthServiceTests
{
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _passwordHasher.Object,
            _userRepository.Object,
            _jwtTokenService.Object,
            _refreshTokenService.Object,
            _refreshTokenRepository.Object);
    }

    [Fact]
    public async Task LoginAsync_With_Unknown_User_Fails_Without_Checking_Password()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("missing@example.com"))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync("missing@example.com", "irrelevant");

        Assert.False(result.Succeeded);
        Assert.Null(result.Token);
        _passwordHasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_With_Wrong_Password_Fails()
    {
        var user = new User("user@example.com", "stored-hash");
        _userRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("wrong-password", user.PasswordHash)).Returns(false);

        var result = await _sut.LoginAsync(user.Email, "wrong-password");

        Assert.False(result.Succeeded);
        Assert.Null(result.Token);
        _jwtTokenService.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_With_Correct_Credentials_Returns_Token_And_RefreshToken()
    {
        var user = new User("user@example.com", "stored-hash");
        var expectedExpiry = DateTimeOffset.UtcNow.AddHours(1);
        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(7);

        _userRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("correct-password", user.PasswordHash)).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateToken(user)).Returns(("signed-jwt", expectedExpiry));
        _refreshTokenService.Setup(r => r.GenerateToken())
            .Returns(("raw-refresh-token", "hashed-refresh-token", refreshExpiry));

        var result = await _sut.LoginAsync(user.Email, "correct-password");

        Assert.True(result.Succeeded);
        Assert.Equal("signed-jwt", result.Token);
        Assert.Equal(expectedExpiry, result.ExpiresAtUtc);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.Is<RefreshToken>(rt =>
            rt.UserId == user.Id && rt.TokenHash == "hashed-refresh-token")), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_With_Existing_Email_Fails()
    {
        var existingUser = new User("user@example.com", "hash");
        _userRepository.Setup(r => r.GetByEmailAsync(existingUser.Email)).ReturnsAsync(existingUser);

        var result = await _sut.RegisterAsync(existingUser.Email, "password");

        Assert.False(result);
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_With_New_Email_Hashes_Password_And_Persists_User()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("new@example.com")).ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash("plain-password")).Returns("hashed-password");

        var result = await _sut.RegisterAsync("new@example.com", "plain-password");

        Assert.True(result);
        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "new@example.com" && u.PasswordHash == "hashed-password")), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_With_Unknown_Token_Fails()
    {
        _refreshTokenService.Setup(r => r.Hash("unknown-token")).Returns("unknown-hash");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("unknown-hash"))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _sut.RefreshAsync("unknown-token");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task RefreshAsync_With_Revoked_Token_Fails()
    {
        var user = new User("user@example.com", "hash");
        var refreshToken = new RefreshToken(user.Id, "hashed-token", DateTimeOffset.UtcNow.AddDays(7));
        refreshToken.Revoke();

        _refreshTokenService.Setup(r => r.Hash("raw-token")).Returns("hashed-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hashed-token")).ReturnsAsync(refreshToken);

        var result = await _sut.RefreshAsync("raw-token");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task RefreshAsync_With_Expired_Token_Fails()
    {
        var user = new User("user@example.com", "hash");
        // Created already past its expiration, no need to wait for real time to pass.
        var expiredToken = new RefreshToken(user.Id, "hashed-token", DateTimeOffset.UtcNow.AddDays(-1));

        _refreshTokenService.Setup(r => r.Hash("raw-token")).Returns("hashed-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hashed-token")).ReturnsAsync(expiredToken);

        var result = await _sut.RefreshAsync("raw-token");

        Assert.False(result.Succeeded);
        _refreshTokenRepository.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_With_Valid_Token_But_Deleted_User_Fails()
    {
        var userId = Guid.NewGuid();
        var refreshToken = new RefreshToken(userId, "hashed-token", DateTimeOffset.UtcNow.AddDays(7));

        _refreshTokenService.Setup(r => r.Hash("raw-token")).Returns("hashed-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hashed-token")).ReturnsAsync(refreshToken);
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var result = await _sut.RefreshAsync("raw-token");

        Assert.False(result.Succeeded);
        _jwtTokenService.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_With_Valid_Token_Rotates_It_And_Returns_New_Pair()
    {
        var user = new User("user@example.com", "hash");
        var refreshToken = new RefreshToken(user.Id, "hashed-token", DateTimeOffset.UtcNow.AddDays(7));

        _refreshTokenService.Setup(r => r.Hash("raw-token")).Returns("hashed-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hashed-token")).ReturnsAsync(refreshToken);
        _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _jwtTokenService.Setup(j => j.GenerateToken(user))
            .Returns(("new-jwt", DateTimeOffset.UtcNow.AddHours(1)));
        _refreshTokenService.Setup(r => r.GenerateToken())
            .Returns(("new-raw-token", "new-hashed-token", DateTimeOffset.UtcNow.AddDays(7)));

        var result = await _sut.RefreshAsync("raw-token");

        Assert.True(result.Succeeded);
        Assert.Equal("new-jwt", result.Token);
        Assert.Equal("new-raw-token", result.RefreshToken);
        Assert.False(refreshToken.IsActive);
        _refreshTokenRepository.Verify(r => r.UpdateAsync(refreshToken), Times.Once);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.Is<RefreshToken>(rt =>
            rt.TokenHash == "new-hashed-token")), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_With_Unknown_Token_Returns_False()
    {
        _refreshTokenService.Setup(r => r.Hash("unknown-token")).Returns("unknown-hash");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("unknown-hash"))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _sut.LogoutAsync("unknown-token");

        Assert.False(result);
    }

    [Fact]
    public async Task LogoutAsync_With_Active_Token_Revokes_It()
    {
        var user = new User("user@example.com", "hash");
        var refreshToken = new RefreshToken(user.Id, "hashed-token", DateTimeOffset.UtcNow.AddDays(7));

        _refreshTokenService.Setup(r => r.Hash("raw-token")).Returns("hashed-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hashed-token")).ReturnsAsync(refreshToken);

        var result = await _sut.LogoutAsync("raw-token");

        Assert.True(result);
        Assert.False(refreshToken.IsActive);
        _refreshTokenRepository.Verify(r => r.UpdateAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_With_Already_Revoked_Token_Is_Idempotent_And_Returns_False()
    {
        var user = new User("user@example.com", "hash");
        var refreshToken = new RefreshToken(user.Id, "hashed-token", DateTimeOffset.UtcNow.AddDays(7));
        refreshToken.Revoke();

        _refreshTokenService.Setup(r => r.Hash("raw-token")).Returns("hashed-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hashed-token")).ReturnsAsync(refreshToken);

        var result = await _sut.LogoutAsync("raw-token");

        Assert.False(result);
        _refreshTokenRepository.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
    }
}
