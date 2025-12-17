using FluentValidation;
using OrderManagement.Api.Contracts.MenuManagement;

namespace OrderManagement.Api.Validators;

public sealed class ReorderMenuRequestValidator : AbstractValidator<ReorderMenuRequest>
{
    public ReorderMenuRequestValidator()
    {
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to zero.")
            .LessThan(10000).WithMessage("Display order must be less than 10,000.");
    }
}

