using Microsoft.OpenApi;
using OneStop.Application;
using OneStop.Infrastructure;
using OneStop.Presentation.Api.Endpoints;
using OneStop.Presentation.Exceptions;
using Scalar.AspNetCore;
using Serilog;

// Configure Serilog early for startup logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting OneStop API");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from configuration
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    // Add services
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure();

    // Add exception handling
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Add OpenAPI (modern .NET 9+ approach)
    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Title = "OneStop Basket Calculator API";
            document.Info.Version = "v1";
            document.Info.Description = "API for calculating basket totals with discounts and loyalty points.";
            
            return Task.CompletedTask;
        });
    });

    var app = builder.Build();

    // Configure pipeline
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        // Map OpenAPI document endpoint
        app.MapOpenApi();
        
        // Map Scalar API reference UI at root
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("OneStop API")
                .WithTheme(ScalarTheme.BluePlanet)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseSerilogRequestLogging();

    // Map endpoints
    app.MapBasketEndpoints();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
        .WithName("HealthCheck")
        .WithTags("Health")
        .ExcludeFromDescription();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }
