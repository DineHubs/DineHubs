using FluentValidation;
using OrderManagement.Api.Contracts.MenuManagement;

namespace OrderManagement.Api.Validators;

public sealed class UpdateMenuPermissionsRequestValidator : AbstractValidator<UpdateMenuPermissionsRequest>
{
    public UpdateMenuPermissionsRequestValidator()
    {
        RuleFor(x => x.AllowedRoles)
            .NotNull().WithMessage("Allowed roles cannot be null.")
            .Must(roles => roles.Count > 0).WithMessage("At least one role must be specified.");

        RuleForEach(x => x.AllowedRoles)
            .NotEmpty().WithMessage("Role name cannot be empty.")
            .MaximumLength(50).WithMessage("Role name must not exceed 50 characters.");
    }
}

