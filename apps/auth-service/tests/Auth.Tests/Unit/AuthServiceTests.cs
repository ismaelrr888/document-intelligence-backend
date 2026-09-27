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
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_passwordHasher.Object, _userRepository.Object, _jwtTokenService.Object);
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
    public async Task LoginAsync_With_Correct_Credentials_Returns_Token()
    {
        var user = new User("user@example.com", "stored-hash");
        var expectedExpiry = DateTimeOffset.UtcNow.AddHours(1);

        _userRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("correct-password", user.PasswordHash)).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateToken(user)).Returns(("signed-jwt", expectedExpiry));

        var result = await _sut.LoginAsync(user.Email, "correct-password");

        Assert.True(result.Succeeded);
        Assert.Equal("signed-jwt", result.Token);
        Assert.Equal(expectedExpiry, result.ExpiresAtUtc);
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
}
