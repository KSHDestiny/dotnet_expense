using System.IdentityModel.Tokens.Jwt;
using System.Text;
using DotnetApp.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DotnetApp.Features.Auth;

/// <summary>
/// Registers JWT issuing + validation: binds and validates <see cref="JwtOptions"/>,
/// registers the token service, and configures the JwtBearer authentication handler.
/// </summary>
public static class JwtAuthServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuth(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Keep claim names as issued (sub/email/jti) instead of remapping them to the
        // legacy long Microsoft URIs — so FindFirstValue("sub") works as expected.
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        // Bind + validate options. ValidateOnStart() => app refuses to boot on bad config.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Every incoming bearer token is checked against these parameters.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromSeconds(30) // tighten the default 5-min leeway
                };
            });

        services.AddAuthorization();

        // Current-user accessor: per-request, reads the validated JWT claims.
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
