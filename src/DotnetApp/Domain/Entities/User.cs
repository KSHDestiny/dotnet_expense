namespace DotnetApp.Domain.Entities;

/// <summary>
/// Application user. Persisted via EF Core (see Infrastructure/Persistence).
/// </summary>
public class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
