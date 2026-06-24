namespace DotnetApp.Domain.Entities;

/// <summary>A single recorded expense, owned by a user and filed under a category.</summary>
public class Expense
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid CategoryId { get; set; }

    /// <summary>Amount spent. decimal (not double) — money must not lose precision.</summary>
    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public DateTime SpentAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
