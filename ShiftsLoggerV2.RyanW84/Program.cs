using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Extensions;
using ShiftsLoggerV2.RyanW84.Filters;
using ShiftsLoggerV2.RyanW84.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Load user secrets if available - use the specific UserSecretsId from the project file
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Prevents circular dependency issues
builder
    .Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );

// Configure DbContext based on environment and OS platform
if (builder.Environment.EnvironmentName == "Testing")
{
    // Use In-Memory database for testing
    builder.Services.AddDbContext<ShiftsLoggerDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryTestDb")
    );
}
else
{
    // Use SQL Server for development and production
    var connectionString = GetConnectionString(builder.Configuration);
    builder.Services.AddDbContext<ShiftsLoggerDbContext>(options =>
        options.UseSqlServer(connectionString)
    );
}

// Register all application services
builder.Services.AddApplicationServices();

// Add global exception filter for centralized error handling
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
});

// Add memory caching for reference data
builder.Services.AddMemoryCache();

// Add health checks
builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<ShiftsLoggerDbContext>("database", tags: new[] { "database", "sql" })
    .AddCheck<CustomHealthCheck>("custom", tags: new[] { "custom" });
static string GetConnectionString(IConfiguration configuration)
{
    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    if (isWindows)
    {
        // Use LocalDB on Windows
        return configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=ShiftsLoggerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
    }
    else
    {
        // Use SQL Server on Linux - read from user secrets
        var connectionString = configuration["ConnectionStrings:LinuxSqlServer"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "LinuxSqlServer connection string must be configured in user secrets for non-Windows platforms. "
                    + "Run 'dotnet user-secrets set \"ConnectionStrings:LinuxSqlServer\" \"<your-connection-string>\"' to set it."
            );

        // Add connection timeout if not present
        if (
            !connectionString.Contains("Connection Timeout")
            && !connectionString.Contains("Timeout")
        )
        {
            connectionString += ";Connection Timeout=30;Command Timeout=30";
        }

        return connectionString;
    }
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Development Mode - Setting up database...");

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ShiftsLoggerDbContext>();

    try
    {
        // Always delete and recreate the database in development for a fresh start
        Console.WriteLine("Deleting existing database (if any)...");
        dbContext.Database.EnsureDeleted();

        Console.WriteLine("Creating new database and applying migrations...");
        dbContext.Database.Migrate();

        var logger =
            scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ShiftsLoggerDbContext>>();
        dbContext.SeedData(logger);

        Console.WriteLine("Database setup completed successfully - fresh database created");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database setup failed: {ex.Message}");
        Console.WriteLine("Please check your connection string and database permissions.");
    }
}

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Shifts Logger API")
        .WithTheme(ScalarTheme.BluePlanet)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithModels()
        .WithLayout(ScalarLayout.Classic);
});

// Map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks(
    "/health/database",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("database"),
    }
);
app.MapHealthChecks(
    "/health/custom",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("custom"),
    }
);

app.MapControllers();

app.Run();

// Make the implicit Program class accessible for integration tests
public partial class Program { }
