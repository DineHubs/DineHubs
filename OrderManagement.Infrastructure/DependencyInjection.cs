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
using OrderManagement.Application.Dashboard;
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
using OrderManagement.Infrastructure.Dashboard;
using OrderManagement.Infrastructure.Security;
using OrderManagement.Infrastructure.Payments;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Reporting;
using OrderManagement.Infrastructure.Subscriptions;
using OrderManagement.Application.Branches;
using OrderManagement.Application.Users;
using OrderManagement.Application.MenuItems;
using OrderManagement.Infrastructure.Branches;
using OrderManagement.Infrastructure.Users;
using OrderManagement.Infrastructure.MenuItems;
using OrderManagement.Application.Receipts;
using OrderManagement.Application.KPIs;
using OrderManagement.Infrastructure.Receipts;
using OrderManagement.Infrastructure.KPIs;

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
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<OrderManagementDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        var jwtSection = configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "JWT key is not configured. Please set 'Jwt:Key' in appsettings.json or environment variables. " +
                "The key must be at least 32 characters long for HS256 algorithm.");
        }
        var key = Encoding.UTF8.GetBytes(jwtKey);

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
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        services.AddScoped<IWhatsAppNotificationService, WhatsAppNotificationService>();
        services.AddScoped<IBillingDispatcher, BillingDispatcher>();
        services.AddScoped<IPlanCatalog, PlanCatalog>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IUsageTracker, UsageTracker>();
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IKpiService, KpiService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IQrOrderingService, QrOrderingService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IKitchenService, KitchenService>();
        services.AddScoped<IKitchenPrintService, KitchenPrintService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<INavigationMenuService, NavigationMenuService>();
        services.AddScoped<IMenuManagementService, MenuManagementService>();
        services.AddSingleton<IInputSanitizer, InputSanitizer>();

        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Default") ?? string.Empty, name: "postgres")
            .AddRedis(configuration.GetValue<string>("Redis:Configuration") ?? "localhost:6379", name: "redis");

        services.AddFeatureManagement();

        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMenuItemService, MenuItemService>();

        return services;
    }
}
