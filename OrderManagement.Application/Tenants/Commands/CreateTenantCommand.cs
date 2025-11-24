using FluentValidation;
using MediatR;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Subscriptions.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Tenants.Commands;

public sealed record CreateTenantCommand(
    string Name,
    string Code,
    string AdminEmail,
    SubscriptionPlanCode PlanCode,
    bool AutoRenew) : IRequest<Tenant>;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
    }
}

public sealed class CreateTenantCommandHandler(
    ITenantProvisioningService provisioningService,
    ISubscriptionService subscriptionService)
    : IRequestHandler<CreateTenantCommand, Tenant>
{
    public async Task<Tenant> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await provisioningService.CreateTenantAsync(request.Name, request.Code, request.AdminEmail, cancellationToken);
        await subscriptionService.CreateAsync(tenant.Id, request.PlanCode, request.AutoRenew, cancellationToken);
        return tenant;
    }
}

