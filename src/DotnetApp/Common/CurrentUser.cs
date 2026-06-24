using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DotnetApp.Common;

/// <summary>
/// The authenticated user for the current request, read from the validated JWT
/// claims. Inject this instead of reaching into <c>ClaimsPrincipal</c> directly.
/// </summary>
public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid Id =>
        Guid.TryParse(Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
            ? id
            : throw new InvalidOperationException("No authenticated user on the current request.");

    public string Email =>
        Principal?.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? throw new InvalidOperationException("No authenticated user on the current request.");
}
