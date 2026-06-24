namespace DotnetApp.Features.Auth;

/// <summary>Registration payload.</summary>
public sealed record RegisterRequest(string Name, string Email, string Password);

/// <summary>Login payload.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Auth result: the user plus a freshly-issued JWT bearer token.</summary>
public sealed record AuthResponse(Guid Id, string Name, string Email, string Token);
