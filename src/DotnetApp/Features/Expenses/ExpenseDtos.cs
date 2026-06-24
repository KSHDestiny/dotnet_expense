namespace DotnetApp.Features.Expenses;

/// <summary>GraphQL projection of an expense.</summary>
public sealed record ExpenseDto(
    Guid Id,
    Guid CategoryId,
    decimal Amount,
    string? Note,
    DateTime SpentAt,
    DateTime CreatedAt);

/// <summary>Input for <c>createExpense</c>.</summary>
public sealed record CreateExpenseInput(Guid CategoryId, decimal Amount, string? Note, DateTime SpentAt);

/// <summary>Input for <c>updateExpense</c>.</summary>
public sealed record UpdateExpenseInput(Guid Id, Guid CategoryId, decimal Amount, string? Note, DateTime SpentAt);
