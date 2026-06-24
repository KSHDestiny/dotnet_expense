using FluentValidation;

namespace DotnetApp.Features.Expenses;

public static class ExpensesServiceCollectionExtensions
{
    public static IServiceCollection AddExpenses(this IServiceCollection services)
    {
        services.AddScoped<IExpenseService, ExpenseService>();

        services.AddScoped<IValidator<CreateExpenseInput>, CreateExpenseInputValidator>();
        services.AddScoped<IValidator<UpdateExpenseInput>, UpdateExpenseInputValidator>();

        return services;
    }
}
