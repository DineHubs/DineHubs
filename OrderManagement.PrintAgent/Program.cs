using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderManagement.PrintAgent.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Print Agent...");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext());

    builder.Services.Configure<PrintAgentOptions>(
        builder.Configuration.GetSection("PrintAgent"));

    builder.Services.AddHttpClient();
    builder.Services.AddSingleton<EscPosGenerator>();
    builder.Services.AddSingleton<PrinterManager>();
    builder.Services.AddHostedService<WebSocketServerService>();

    // Enable Windows Service support
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "DineHubs Print Agent";
    });

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Print Agent terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

