using FluentValidation;
using OrderManagement.Api.Contracts.Orders;

namespace OrderManagement.Api.Validators;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(items => items.Count <= 100).WithMessage("Order cannot contain more than 100 items.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderLineRequestValidator());

        RuleFor(x => x.TableNumber)
            .NotEmpty().When(x => !x.IsTakeAway)
            .WithMessage("Table number is required for dine-in orders.")
            .MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.TableNumber))
            .WithMessage("Table number must not exceed 50 characters.");
    }
}

public sealed class CreateOrderLineRequestValidator : AbstractValidator<CreateOrderLineRequest>
{
    public CreateOrderLineRequestValidator()
    {
        RuleFor(x => x.MenuItemId)
            .NotEmpty().WithMessage("Menu item ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(200).WithMessage("Item name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100.");
    }
}

