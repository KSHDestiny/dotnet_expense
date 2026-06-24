using DotnetApp.Common;
using DotnetApp.Infrastructure.Persistence;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DotnetApp.GraphQL;

/// <summary>GraphQL projection of a user (never exposes the entity / password hash).</summary>
public sealed record UserDto(Guid Id, string Name, string Email);

/// <summary>
/// Root GraphQL query type. Each public method becomes a queryable field.
/// Resolvers receive services via parameter injection, like Minimal API handlers.
/// </summary>
public sealed class Query
{
    /// <summary>The currently authenticated user.</summary>
    [Authorize]
    public async Task<UserDto?> GetMeAsync(
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        return await db.Users
            .Where(u => u.Id == currentUser.Id)
            .Select(u => new UserDto(u.Id, u.Name, u.Email))
            .SingleOrDefaultAsync(ct);
    }
}
