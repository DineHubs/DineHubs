using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using OrderManagement.Api.Configuration;
using OrderManagement.Api.Middleware;
using OrderManagement.Application;
using OrderManagement.Infrastructure;
using OrderManagement.Infrastructure.Identity;
using OrderManagement.Infrastructure.Logging;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Tenancy;
using Serilog;
using Serilog.Sinks.PeriodicBatching;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    var loggerConfig = configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();

    // Add database sink for Errors and Warnings only
    var databaseSink = new BatchingDatabaseSink(services, batchSizeLimit: 50, period: TimeSpan.FromSeconds(5));
    var periodicBatchingSink = new PeriodicBatchingSink(
        databaseSink,
        new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = 50,
            Period = TimeSpan.FromSeconds(5),
            EagerlyEmitFirstEvent = true
        });
    loggerConfig.WriteTo.Sink(
        periodicBatchingSink,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Version"),
        new QueryStringApiVersionReader("version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Rate Limiting Configuration
var rateLimitSection = builder.Configuration.GetSection("Security:RateLimiting");
var authenticatedLimit = rateLimitSection.GetValue<int>("Authenticated:PermitLimit", 100);
var authenticatedWindow = rateLimitSection.GetValue<int>("Authenticated:WindowMinutes", 1);
var authenticatedQueue = rateLimitSection.GetValue<int>("Authenticated:QueueLimit", 10);
var anonymousLimit = rateLimitSection.GetValue<int>("Anonymous:PermitLimit", 20);
var anonymousWindow = rateLimitSection.GetValue<int>("Anonymous:WindowMinutes", 1);
var anonymousQueue = rateLimitSection.GetValue<int>("Anonymous:QueueLimit", 5);

builder.Services.AddRateLimiter(options =>
{
    // Authenticated users policy
    options.AddPolicy("Authenticated", context =>
    {
        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        if (isAuthenticated)
        {
            var partitionKey = context.User.Identity?.Name ?? context.Connection?.Id ?? Guid.NewGuid().ToString();
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: partitionKey,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authenticatedLimit,
                    Window = TimeSpan.FromMinutes(authenticatedWindow),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = authenticatedQueue
                });
        }
        return RateLimitPartition.GetNoLimiter(string.Empty);
    });

    // Anonymous users policy
    options.AddPolicy("Anonymous", context =>
    {
        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
        {
            var partitionKey = context.Connection?.Id ?? Guid.NewGuid().ToString();
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: partitionKey,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = anonymousLimit,
                    Window = TimeSpan.FromMinutes(anonymousWindow),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = anonymousQueue
                });
        }
        return RateLimitPartition.GetNoLimiter(string.Empty);
    });

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "Global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authenticatedLimit + anonymousLimit,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
    };
});

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();

// Swagger with API Versioning
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// Configure Swagger options for versioning
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// CORS Configuration
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var allowCredentials = builder.Configuration.GetValue<bool>("Cors:AllowCredentials", false);

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        if (builder.Environment.IsDevelopment() && corsOrigins.Length == 0)
        {
            // Development fallback: allow localhost origins
            policy.WithOrigins("http://localhost:4200", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else if (corsOrigins.Length > 0)
        {
            // Production/Configured: use specific origins
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            
            if (allowCredentials)
            {
                policy.AllowCredentials();
            }
        }
        else
        {
            // Production with no origins configured: deny all (secure default)
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithOrigins(); // Empty origins = deny all
        }
    });
});

var app = builder.Build();

await IdentitySeeder.SeedAsync(app.Services);
await DataSeeder.SeedAsync(app.Services);
await NavigationMenuSeeder.SeedAsync(app.Services);

// Security Headers Middleware (must be early in pipeline)
app.UseMiddleware<SecurityHeadersMiddleware>();

// Global Exception Handler (must be early in pipeline, before other middleware)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.OrderByDescending(d => d.ApiVersion))
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Order Management API {description.GroupName.ToUpperInvariant()}");
        }
        options.RoutePrefix = "swagger"; // Swagger UI at /swagger
    });
}

app.UseSerilogRequestLogging();
app.UseResponseCompression();
app.UseCors("default");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
