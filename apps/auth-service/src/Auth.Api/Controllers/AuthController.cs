using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Api.Configuration;
using Auth.Api.Contracts.Auth;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Auth.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.Email,
            request.Password);

        if (!result)
        {
            return Conflict();
        }

        return Ok();
    }

    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [EnableRateLimiting(RateLimitingOptions.LoginPolicyName)]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.Succeeded)
        {
            // Generic message: never reveal whether the user exists or the
            // password was wrong, to avoid user enumeration.
            return Unauthorized(new ErrorResponse("Invalid credentials"));
        }

        return Ok(new LoginResponse(result.Token!, result.ExpiresAtUtc!.Value, result.RefreshToken!));
    }

    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);

        if (!result.Succeeded)
        {
            return Unauthorized(new ErrorResponse("Invalid or expired refresh token"));
        }

        return Ok(new LoginResponse(result.Token!, result.ExpiresAtUtc!.Value, result.RefreshToken!));
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        // Always respond 204 regardless of whether the token was valid/active,
        // to avoid leaking whether a given refresh token exists.
        await _authService.LogoutAsync(request.RefreshToken);

        return NoContent();
    }

    [Authorize]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);

        if (userId is null || email is null)
        {
            return Unauthorized();
        }

        return Ok(new MeResponse(Guid.Parse(userId), email));
    }
}