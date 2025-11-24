using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OrderManagement.Infrastructure.Identity;
using OrderManagement.Infrastructure.Tenancy;

namespace OrderManagement.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderManagementDbContext>
{
    public OrderManagementDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<OrderManagementDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Default") ??
                                 "Host=localhost;Port=15432;Database=dinehub;Username=Postgress;Password=Shakthi@18");

        return new OrderManagementDbContext(optionsBuilder.Options, new Tenancy.TenantContext(), new Identity.BackgroundUserContext(), new Common.DateTimeProvider());
    }
}

