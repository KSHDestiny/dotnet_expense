using DotnetApp.GraphQL;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace DotnetApp.Features.Expenses;

[ExtendObjectType<Query>]
public sealed class ExpenseQueries
{
    [Authorize]
    public Task<List<ExpenseDto>> GetMyExpensesAsync(
        IExpenseService expenses,
        CancellationToken ct,
        Guid? categoryId = null,
        DateTime? from = null,
        DateTime? to = null) =>
        expenses.GetMineAsync(categoryId, from, to, ct);
}

[ExtendObjectType<Mutation>]
public sealed class ExpenseMutations
{
    [Authorize]
    public async Task<ExpenseDto> CreateExpenseAsync(
        CreateExpenseInput input, IExpenseService expenses, CancellationToken ct) =>
        (await expenses.CreateAsync(input, ct)).ValueOrThrow();

    [Authorize]
    public async Task<ExpenseDto> UpdateExpenseAsync(
        UpdateExpenseInput input, IExpenseService expenses, CancellationToken ct) =>
        (await expenses.UpdateAsync(input, ct)).ValueOrThrow();

    [Authorize]
    public async Task<bool> DeleteExpenseAsync(
        Guid id, IExpenseService expenses, CancellationToken ct)
    {
        (await expenses.DeleteAsync(id, ct)).EnsureSuccess();
        return true;
    }
}
