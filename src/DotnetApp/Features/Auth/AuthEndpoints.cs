using DotnetApp.Common;

namespace DotnetApp.Features.Auth;

/// <summary>
/// Auth feature wiring: endpoint mappings and DI registration. Keeps the slice
/// self-contained — <c>Program.cs</c> just calls <c>AddAuth</c> + <c>MapAuthEndpoints</c>.
/// </summary>
public static class AuthEndpoints
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>(); // stateless → singleton
        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request, IAuthService auth, CancellationToken ct) =>
        {
            var result = await auth.RegisterAsync(request, ct);
            return result.ToHttpResult(user => Results.Created($"/users/{user.Id}", user));
        });

        group.MapPost("/login", async (
            LoginRequest request, IAuthService auth, CancellationToken ct) =>
        {
            var result = await auth.LoginAsync(request, ct);
            return result.ToHttpResult(Results.Ok);
        });

        return app;
    }
}
