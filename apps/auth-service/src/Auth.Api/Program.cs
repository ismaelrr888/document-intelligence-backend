using System.Text;
using System.Threading.RateLimiting;
using Auth.Api.Configuration;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Infrastructure;
using Auth.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    // Declare the Bearer scheme so Swagger UI shows an "Authorize" button
    // and lets you attach a JWT to protected endpoints like GET /auth/me.
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new();
        document.Components.SecuritySchemes["Bearer"] = new()
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations.Values))
        {
            operation.Security.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = Array.Empty<string>()
            });
        }

        return Task.CompletedTask;
    });
});
builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Bind the JwtBearer handler options lazily through IOptions<JwtOptions> (already
// registered by AddInfrastructure) instead of reading configuration eagerly here.
// This keeps a single source of truth for the Jwt settings and plays nicely with
// hosts that customize configuration after the builder is created (e.g. tests).
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptionsAccessor) =>
    {
        var jwtOptions = jwtOptionsAccessor.Value;

        // Keep the original JWT claim names (e.g. "sub", "email") instead of
        // letting the handler remap them to legacy Microsoft claim URIs.
        bearerOptions.MapInboundClaims = false;

        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services
    .AddOptions<RateLimitingOptions>()
    .Bind(builder.Configuration.GetSection(RateLimitingOptions.SectionName));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Partitioned by client IP so one abusive caller can't exhaust the quota
    // for every other user attempting to log in.
    options.AddPolicy(RateLimitingOptions.LoginPolicyName, httpContext =>
    {
        var rateLimitingOptions = httpContext.RequestServices
            .GetRequiredService<IOptions<RateLimitingOptions>>().Value;
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimitingOptions.LoginPermitLimit,
            Window = TimeSpan.FromSeconds(rateLimitingOptions.LoginWindowSeconds),
            QueueLimit = 0
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Auth API v1");
    });
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program;