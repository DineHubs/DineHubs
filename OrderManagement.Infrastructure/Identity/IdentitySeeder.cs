using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using OrderManagement.Domain.Identity;
using OrderManagement.Identity.Entities;

namespace OrderManagement.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (var roleName in SystemRoles.All)
            {
                try
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new ApplicationRole(roleName));
                        Log.Information("Created role {Role}", roleName);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error creating role {Role}", roleName);
                    throw;
                }
            }

            Log.Information("Identity seeding completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during identity seeding");
            throw;
        }
    }
}


