using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Abstractions.Billing;
using OrderManagement.Application.Abstractions.Notifications;
using OrderManagement.Application.Auth;
using OrderManagement.Application.Inventory;
using OrderManagement.Application.Kitchen;
using OrderManagement.Application.MenuManagement;
using OrderManagement.Application.Navigation;
using OrderManagement.Application.Ordering;
using OrderManagement.Application.Payments;
using OrderManagement.Application.Reporting;
using OrderManagement.Infrastructure.Tenancy;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Tenants;
using OrderManagement.Domain.Identity;
using OrderManagement.Identity.Entities;
using OrderManagement.Infrastructure.Inventory;
using OrderManagement.Infrastructure.Billing;
using OrderManagement.Infrastructure.Common;
using OrderManagement.Infrastructure.Identity;
using OrderManagement.Infrastructure.Messaging;
using OrderManagement.Infrastructure.Options;
using OrderManagement.Infrastructure.MenuManagement;
using OrderManagement.Infrastructure.Navigation;
using OrderManagement.Infrastructure.Ordering;
using OrderManagement.Infrastructure.Kitchen;
using OrderManagement.Infrastructure.Security;
using OrderManagement.Infrastructure.Payments;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Reporting;
using OrderManagement.Infrastructure.Subscriptions;
using OrderManagement.Application.Branches;
using OrderManagement.Application.Users;
using OrderManagement.Infrastructure.Branches;
using OrderManagement.Infrastructure.Users;

namespace OrderManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection("Messaging:Email"));
        services.Configure<WhatsAppOptions>(configuration.GetSection("Messaging:WhatsApp"));
        services.Configure<SubscriptionOptions>(configuration.GetSection("Subscription"));
        services.Configure<PaymentOptions>(configuration.GetSection("Payments"));
        services.Configure<MultiTenancyOptions>(configuration.GetSection("MultiTenancy"));
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddDbContext<OrderManagementDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default") ??
                                   "Host=localhost;Port=5432;Database=order_management;Username=om_admin;Password=om_password";
            options.UseNpgsql(connectionString);
        });

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<OrderManagementDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        var jwtSection = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? "CHANGE_ME_SUPER_SECRET");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization(options =>
        {
            foreach (var role in SystemRoles.All)
            {
                options.AddPolicy(role, policy => policy.RequireRole(role));
            }
        });

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        services.AddScoped<IWhatsAppNotificationService, WhatsAppNotificationService>();
        services.AddScoped<IBillingDispatcher, BillingDispatcher>();
        services.AddScoped<IPlanCatalog, PlanCatalog>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IUsageTracker, UsageTracker>();
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IQrOrderingService, QrOrderingService>();
        services.AddScoped<IKitchenService, KitchenService>();
        services.AddScoped<INavigationMenuService, NavigationMenuService>();
        services.AddScoped<IMenuManagementService, MenuManagementService>();
        services.AddSingleton<IInputSanitizer, InputSanitizer>();

        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Default") ?? string.Empty, name: "postgres")
            .AddRedis(configuration.GetValue<string>("Redis:Configuration") ?? "localhost:6379", name: "redis");

        services.AddFeatureManagement();

        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
