using OrderManagement.Application;
using OrderManagement.Infrastructure;
using OrderManagement.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<UsageTrackingWorker>();

builder.Logging.AddSerilog(new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger());

var host = builder.Build();
host.Run();
