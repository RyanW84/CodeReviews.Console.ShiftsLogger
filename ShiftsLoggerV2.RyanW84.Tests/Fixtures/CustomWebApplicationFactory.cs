using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShiftsLoggerV2.RyanW84.Data;

namespace ShiftsLoggerV2.RyanW84.Tests.Fixtures;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private static readonly string DatabaseName = $"InMemoryTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all existing DbContext registrations
            var descriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<ShiftsLoggerDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                d.ImplementationType == typeof(ShiftsLoggerDbContext) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContext)) ||
                d.ServiceType == typeof(ShiftsLoggerDbContext)
            ).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database with consistent name for the test class
            services.AddDbContext<ShiftsLoggerDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
                options.EnableSensitiveDataLogging(); // Helpful for debugging
            });

            // Ensure the database is created
            using var scope = services.BuildServiceProvider().CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ShiftsLoggerDbContext>();
            context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}
