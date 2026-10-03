using System.Net;
using System.Net.Http.Json;
using Auth.Api.Contracts.Auth;
using Xunit;

namespace Auth.Tests.Integration;

public sealed class AuthEndpointsTests : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(AuthApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Then_Login_Returns_Token()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        const string password = "Sup3rSecret!";

        var registerResponse = await _client.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest(email, password));

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var body = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.True(body.ExpiresAtUtc > DateTimeOffset.UtcNow);
        Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
    }

    [Fact]
    public async Task Register_With_Existing_Email_Returns_Conflict()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        const string password = "Sup3rSecret!";

        await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(email, password));
        var secondAttempt = await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(email, password));

        Assert.Equal(HttpStatusCode.Conflict, secondAttempt.StatusCode);
    }

    [Theory]
    [InlineData("not-an-email", "Sup3rSecret!")]
    [InlineData("", "Sup3rSecret!")]
    [InlineData("valid@example.com", "short")]
    [InlineData("valid@example.com", "")]
    [InlineData("valid@example.com", "alllowercase1!")] // missing uppercase
    [InlineData("valid@example.com", "ALLUPPERCASE1!")] // missing lowercase
    [InlineData("valid@example.com", "NoDigitsHere!")]  // missing digit
    [InlineData("valid@example.com", "NoSpecialChar1")] // missing special char
    public async Task Register_With_Invalid_Input_Returns_BadRequest(string email, string password)
    {
        var response = await _client.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest(email, password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("not-an-email", "Sup3rSecret!")]
    [InlineData("", "Sup3rSecret!")]
    [InlineData("valid@example.com", "")]
    public async Task Login_With_Invalid_Input_Returns_BadRequest(string email, string password)
    {
        var response = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_With_Empty_Token_Returns_BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/auth/refresh", new RefreshRequest(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_With_Empty_Token_Returns_BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/auth/logout", new LogoutRequest(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_With_Unknown_Email_Returns_Unauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest($"{Guid.NewGuid()}@example.com", "whatever-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_With_Wrong_Password_Returns_Unauthorized()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(email, "CorrectPassword1!"));

        var response = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest(email, "WrongPassword1!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_Without_Token_Returns_Unauthorized()
    {
        var response = await _client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_With_Valid_Token_Returns_User_Info()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        const string password = "Sup3rSecret!";

        await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(email, password));

        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest(email, password));

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            loginBody!.AccessToken);

        var meResponse = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var meBody = await meResponse.Content.ReadFromJsonAsync<MeResponse>();

        Assert.NotNull(meBody);
        Assert.Equal(email, meBody!.Email);
    }

    [Fact]
    public async Task Refresh_With_Valid_Token_Returns_New_Token_Pair()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        const string password = "Sup3rSecret!";

        await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(email, password));
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(email, password));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshResponse = await _client.PostAsJsonAsync(
            "/auth/refresh",
            new RefreshRequest(loginBody!.RefreshToken));

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(refreshBody);
        Assert.False(string.IsNullOrWhiteSpace(refreshBody!.AccessToken));
        Assert.NotEqual(loginBody.RefreshToken, refreshBody.RefreshToken);
    }

    [Fact]
    public async Task Refresh_With_Already_Used_Token_Returns_Unauthorized()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        const string password = "Sup3rSecret!";

        await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(email, password));
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(email, password));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        await _client.PostAsJsonAsync("/auth/refresh", new RefreshRequest(loginBody!.RefreshToken));
        var secondAttempt = await _client.PostAsJsonAsync(
            "/auth/refresh",
            new RefreshRequest(loginBody.RefreshToken));

        Assert.Equal(HttpStatusCode.Unauthorized, secondAttempt.StatusCode);
    }

    [Fact]
    public async Task Refresh_With_Unknown_Token_Returns_Unauthorized()
    {
        var response = await _client.PostAsJsonAsync("/auth/refresh", new RefreshRequest("not-a-real-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_Then_Refresh_Returns_Unauthorized()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        const string password = "Sup3rSecret!";

        await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(email, password));
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(email, password));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var logoutResponse = await _client.PostAsJsonAsync(
            "/auth/logout",
            new LogoutRequest(loginBody!.RefreshToken));

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshResponse = await _client.PostAsJsonAsync(
            "/auth/refresh",
            new RefreshRequest(loginBody.RefreshToken));

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_With_Unknown_Token_Still_Returns_NoContent()
    {
        var response = await _client.PostAsJsonAsync("/auth/logout", new LogoutRequest("not-a-real-token"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}

/// <summary>
/// Exercises the login rate limiter with a deliberately low permit limit so the
/// test suite doesn't need to send hundreds of requests to trip it.
/// </summary>
public sealed class AuthLoginRateLimitingTests : IClassFixture<RateLimitedAuthApiFactory>
{
    private readonly HttpClient _client;

    public AuthLoginRateLimitingTests(RateLimitedAuthApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Beyond_Permit_Limit_Returns_TooManyRequests()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        var request = new LoginRequest(email, "wrong-password");

        // RateLimitedAuthApiFactory caps login attempts at 2 per window.
        var first = await _client.PostAsJsonAsync("/auth/login", request);
        var second = await _client.PostAsJsonAsync("/auth/login", request);
        var third = await _client.PostAsJsonAsync("/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, first.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
    }
}
