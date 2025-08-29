using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleFrontEnd;

/// <summary>
///     SOLID Refactored Program Entry Point
///     Demonstrates proper SOLID principles implementation
/// </summary>
public class Program //testing git commit
{
    public static async Task Main(string[] args)
    {
        try
        {
            // Create and configure the host using Microsoft.Extensions.Hosting
            var host = CreateHostBuilder(args).Build();

            // Get the application service and run
            var app = host.Services.GetRequiredService<IApplication>();
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register all application services following SOLID principles
                services.RegisterApplicationServices();
            });
    }
}