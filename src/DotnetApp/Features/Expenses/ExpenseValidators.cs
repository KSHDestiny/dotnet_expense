using FluentValidation;

namespace DotnetApp.Features.Expenses;

public sealed class CreateExpenseInputValidator : AbstractValidator<CreateExpenseInput>
{
    public CreateExpenseInputValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m);
        RuleFor(x => x.Note).MaximumLength(280);
        // Allow a little clock skew, but no clearly-future expenses.
        RuleFor(x => x.SpentAt).LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5))
            .WithMessage("SpentAt cannot be in the future.");
    }
}

public sealed class UpdateExpenseInputValidator : AbstractValidator<UpdateExpenseInput>
{
    public UpdateExpenseInputValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m);
        RuleFor(x => x.Note).MaximumLength(280);
        RuleFor(x => x.SpentAt).LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5))
            .WithMessage("SpentAt cannot be in the future.");
    }
}
