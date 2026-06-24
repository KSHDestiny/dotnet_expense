using DotnetApp.Common;
using DotnetApp.Common.Errors;
using DotnetApp.Domain.Entities;
using DotnetApp.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DotnetApp.Features.Expenses;

public interface IExpenseService
{
    Task<List<ExpenseDto>> GetMineAsync(Guid? categoryId, DateTime? from, DateTime? to, CancellationToken ct);
    Task<Result<ExpenseDto>> CreateAsync(CreateExpenseInput input, CancellationToken ct);
    Task<Result<ExpenseDto>> UpdateAsync(UpdateExpenseInput input, CancellationToken ct);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class ExpenseService(
    AppDbContext db,
    ICurrentUser currentUser,
    IValidator<CreateExpenseInput> createValidator,
    IValidator<UpdateExpenseInput> updateValidator) : IExpenseService
{
    public async Task<List<ExpenseDto>> GetMineAsync(
        Guid? categoryId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var query = db.Expenses.Where(e => e.UserId == currentUser.Id);

        // Optional filters composed onto the IQueryable — translated to SQL.
        if (categoryId is { } cid) query = query.Where(e => e.CategoryId == cid);
        if (from is { } f) query = query.Where(e => e.SpentAt >= f);
        if (to is { } t) query = query.Where(e => e.SpentAt <= t);

        return await query
            .OrderByDescending(e => e.SpentAt)
            .Select(e => new ExpenseDto(e.Id, e.CategoryId, e.Amount, e.Note, e.SpentAt, e.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<Result<ExpenseDto>> CreateAsync(CreateExpenseInput input, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            return Result.Failure<ExpenseDto>(Error.Validation(validation.Errors[0].ErrorMessage));

        if (!await OwnsCategoryAsync(input.CategoryId, ct))
            return Result.Failure<ExpenseDto>(Error.NotFound("Category not found."));

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            CategoryId = input.CategoryId,
            Amount = input.Amount,
            Note = input.Note?.Trim(),
            SpentAt = input.SpentAt
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync(ct);

        return ToDto(expense);
    }

    public async Task<Result<ExpenseDto>> UpdateAsync(UpdateExpenseInput input, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
            return Result.Failure<ExpenseDto>(Error.Validation(validation.Errors[0].ErrorMessage));

        var expense = await db.Expenses
            .SingleOrDefaultAsync(e => e.Id == input.Id && e.UserId == currentUser.Id, ct);
        if (expense is null)
            return Result.Failure<ExpenseDto>(Error.NotFound("Expense not found."));

        if (!await OwnsCategoryAsync(input.CategoryId, ct))
            return Result.Failure<ExpenseDto>(Error.NotFound("Category not found."));

        expense.CategoryId = input.CategoryId;
        expense.Amount = input.Amount;
        expense.Note = input.Note?.Trim();
        expense.SpentAt = input.SpentAt;
        await db.SaveChangesAsync(ct);

        return ToDto(expense);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var expense = await db.Expenses
            .SingleOrDefaultAsync(e => e.Id == id && e.UserId == currentUser.Id, ct);
        if (expense is null)
            return Result.Failure(Error.NotFound("Expense not found."));

        db.Expenses.Remove(expense);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    // The category must exist AND belong to the current user — prevents attaching an
    // expense to someone else's category.
    private Task<bool> OwnsCategoryAsync(Guid categoryId, CancellationToken ct) =>
        db.Categories.AnyAsync(c => c.Id == categoryId && c.UserId == currentUser.Id, ct);

    private static ExpenseDto ToDto(Expense e) =>
        new(e.Id, e.CategoryId, e.Amount, e.Note, e.SpentAt, e.CreatedAt);
}
