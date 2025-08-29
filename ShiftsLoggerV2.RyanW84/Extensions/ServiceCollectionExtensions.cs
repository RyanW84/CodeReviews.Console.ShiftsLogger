// Removed ConsoleFrontEnd using directives - API should not depend on console UI types
using ShiftsLoggerV2.RyanW84.Repositories;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Extensions;

/// <summary>
///     Extension methods for service registration following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Register all repositories (Data Access Layer)
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IWorkerRepository, WorkerRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();

        return services;
    }

    /// <summary>
    ///     Register all business services (Business Logic Layer)
    /// </summary>
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Register business services (consolidated from validation classes)
        services.AddScoped<IShiftBusinessService, ShiftBusinessService>();
        services.AddScoped<IWorkerBusinessService, WorkerBusinessService>();
        services.AddScoped<ILocationBusinessService, LocationBusinessService>();

        return services;
    }

    /// <summary>
    ///     Register all application services following SOLID principles
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services
            .AddRepositories()
            .AddBusinessServices();


        return services;
    }
}