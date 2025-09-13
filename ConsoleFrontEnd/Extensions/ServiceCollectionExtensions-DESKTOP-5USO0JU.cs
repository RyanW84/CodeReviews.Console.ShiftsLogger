using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Core.Infrastructure;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.MenuSystem;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.MenuSystem.Menus;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

namespace ConsoleFrontEnd.Extensions;

/// <summary>
///     Extension methods for service registration following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Register all application services
    /// </summary>
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        // HTTP Client Factory and typed clients
        services.AddHttpClient();

        // Configure API services with common setup
        services.ConfigureApiService<IShiftService, ShiftService>();
        services.ConfigureApiService<IWorkerService, WorkerService>();
        services.ConfigureApiService<ILocationService, LocationService>();

        // Console services (Spectre.Console-based)
        services.AddSingleton<IConsoleDisplayService, SpectreConsoleDisplayService>();
        services.AddSingleton<IConsoleInputService, SpectreConsoleInputService>();

        // Core application services
        services.AddSingleton<IMenuFactory, MenuFactory>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IApplication, ConsoleApplication>();

        // API Services (registered as typed HttpClients above)
        // NOTE: Do not re-register IWorkerService/ILocationService here with AddScoped -
        // that would override the typed HttpClient registrations and produce HttpClient
        // instances without the configured BaseAddress. The typed AddHttpClient registrations
        // above already register the service implementations with configured HttpClient.

        // UI Services
        services.AddScoped<IShiftUi, ShiftUI>();
        services.AddScoped<IWorkerUi, WorkerUi>();
        services.AddScoped<ILocationUi, LocationUI>();

        // Helper Services
        services.AddScoped<ShiftInputHelper>();

        // Business Services (SOLID refactoring)
        services.AddScoped<ConsoleFrontEnd.Services.Common.IErrorHandlingService, ConsoleFrontEnd.Services.Infrastructure.ErrorHandlingService>();
        services.AddScoped<ConsoleFrontEnd.Services.Display.IShiftDisplayService, ConsoleFrontEnd.Services.Display.ShiftDisplayService>();
        services.AddScoped<ConsoleFrontEnd.Services.Controllers.IShiftControllerService, ConsoleFrontEnd.Services.Controllers.ShiftControllerService>();

        // Register concrete menu types for MenuFactory
        services.AddScoped<MainMenu>();
        services.AddScoped<ShiftMenu>();
        services.AddScoped<WorkerMenu>();
        services.AddScoped<LocationMenu>();

        // Register menus as IMenu for IEnumerable<IMenu> resolution
        services.AddScoped<IMenu, MainMenu>();
        services.AddScoped<IMenu, ShiftMenu>();
        services.AddScoped<IMenu, WorkerMenu>();
        services.AddScoped<IMenu, LocationMenu>();

        return services;
    }

    /// <summary>
    ///     Configure an API service with common HTTP client setup
    /// </summary>
    private static IServiceCollection ConfigureApiService<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddHttpClient<TInterface, TImplementation>(
            (sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var hostEnvironment = sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();

                // Use HTTP in development, HTTPS in production
                var protocol = "http"; // hostEnvironment.IsDevelopment() ? "http" : "https";
                var port = "5009"; // hostEnvironment.IsDevelopment() ? "5009" : "7009";
                var baseUrl = config.GetValue<string>("ApiBaseUrl") ?? $"{protocol}://localhost:{port}";
                client.BaseAddress = new Uri(baseUrl);
            }
        ).ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        });

        return services;
    }
}
