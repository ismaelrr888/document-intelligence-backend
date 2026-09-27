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
}
