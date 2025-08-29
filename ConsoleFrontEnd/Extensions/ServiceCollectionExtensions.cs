using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Core.Infrastructure;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.MenuSystem;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.MenuSystem.Menus;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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
        // Typed client for ShiftService - sets BaseAddress from configuration via named setting
        services.AddHttpClient<IShiftService, ShiftService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7009";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Typed clients for other API services
        services.AddHttpClient<IWorkerService, WorkerService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7009";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<ILocationService, LocationService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7009";
            client.BaseAddress = new Uri(baseUrl);
        });

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
}