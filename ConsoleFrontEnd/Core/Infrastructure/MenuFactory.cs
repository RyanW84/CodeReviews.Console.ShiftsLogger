using ConsoleFrontEnd.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFrontEnd.Core.Infrastructure;

/// <summary>
/// Menu factory implementation following Dependency Inversion Principle
/// Creates menu instances with proper dependency injection
/// </summary>
public class MenuFactory : IMenuFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MenuFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IMenu CreateMainMenu()
    {
        return _serviceProvider.GetServices<IMenu>()
            .FirstOrDefault(m => m.GetType().Name == "MainMenu")
            ?? throw new InvalidOperationException("MainMenu not registered");
    }

    public IMenu CreateShiftMenu()
    {
        return _serviceProvider.GetServices<IMenu>()
            .FirstOrDefault(m => m.GetType().Name == "ShiftMenu")
            ?? throw new InvalidOperationException("ShiftMenu not registered");
    }

    public IMenu CreateLocationMenu()
    {
        return _serviceProvider.GetServices<IMenu>()
            .FirstOrDefault(m => m.GetType().Name == "LocationMenu")
            ?? throw new InvalidOperationException("LocationMenu not registered");
    }

    public IMenu CreateWorkerMenu()
    {
        return _serviceProvider.GetServices<IMenu>()
            .FirstOrDefault(m => m.GetType().Name == "WorkerMenu")
            ?? throw new InvalidOperationException("WorkerMenu not registered");
    }
}
