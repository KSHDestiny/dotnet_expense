using DotnetApp.Common;
using DotnetApp.Common.Errors;
using DotnetApp.Domain.Entities;
using DotnetApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotnetApp.Features.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
}

public sealed class AuthService(AppDbContext db, IPasswordHasher passwordHasher) : IAuthService
{
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Inline minimal validation (FluentValidation comes in Step 7).
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<AuthResponse>(Error.Validation("Name is required."));
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<AuthResponse>(Error.Validation("Email is required."));
        if (request.Password.Length < 8)
            return Result.Failure<AuthResponse>(Error.Validation("Password must be at least 8 characters."));

        // Expected failure → Result, not an exception. (The DB unique index is the
        // real guard; this gives a friendly message before hitting it.)
        var emailTaken = await db.Users.AnyAsync(u => u.Email == email, ct);
        if (emailTaken)
            return Result.Failure<AuthResponse>(Error.Conflict("Email is already registered."));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password)
            // CreatedAt is set by the DB default (now()).
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return new AuthResponse(user.Id, user.Name, user.Email);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);

        // Same generic error whether the email is unknown or the password is wrong —
        // don't leak which accounts exist.
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>(Error.Unauthorized("Invalid email or password."));

        return new AuthResponse(user.Id, user.Name, user.Email);
    }
}
