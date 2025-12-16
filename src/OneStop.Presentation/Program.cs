using Serilog;

// Configure Serilog early for startup logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting OneStop API");

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

var app = builder.Build();

// Configure pipeline
app.UseExceptionHandler();

app.UseSerilogRequestLogging();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
    .WithName("HealthCheck")
    .WithTags("Health")
    .ExcludeFromDescription();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
