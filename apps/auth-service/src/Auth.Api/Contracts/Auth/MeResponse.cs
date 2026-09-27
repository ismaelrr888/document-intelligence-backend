namespace Auth.Api.Contracts.Auth;

public sealed record MeResponse(
    Guid Id,
    string Email
);
