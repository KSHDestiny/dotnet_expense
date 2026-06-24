namespace DotnetApp.Features.Auth;

/// <summary>Registration payload.</summary>
public sealed record RegisterRequest(string Name, string Email, string Password);

/// <summary>Login payload.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Auth result returned to the client. In Step 4 a <c>Token</c> property is added
/// here; the rest of the slice stays unchanged.
/// </summary>
public sealed record AuthResponse(Guid Id, string Name, string Email);
