namespace DotnetApp.Domain.Entities;

/// <summary>An expense category (e.g. Food, Transport), owned by a user.</summary>
public class Category
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
