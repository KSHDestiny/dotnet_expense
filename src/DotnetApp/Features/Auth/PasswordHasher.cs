using DotnetApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DotnetApp.Features.Auth;

/// <summary>
/// <see cref="IPasswordHasher"/> backed by ASP.NET Core Identity's PBKDF2 hasher
/// (salted, with an iteration count). The <see cref="User"/> argument is unused by
/// the algorithm — Identity's API just requires the user type for its interface.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _inner = new PasswordHasher<User>();

    private static readonly User Placeholder = new();

    public string Hash(string password) =>
        _inner.HashPassword(Placeholder, password);

    public bool Verify(string password, string hash) =>
        _inner.VerifyHashedPassword(Placeholder, hash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
