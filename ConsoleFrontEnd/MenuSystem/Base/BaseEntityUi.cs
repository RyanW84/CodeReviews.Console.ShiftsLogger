using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Models.Dtos;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem.Base;

/// <summary>
/// Base UI service implementing common CRUD operations following SOLID principles
/// T - Entity type, TFilter - Filter options type
/// </summary>
public abstract class BaseEntityUi<T, TFilter>(IConsoleDisplayService display, ILogger logger) : IEntityUi<T, TFilter>
    where T : class, new()
    where TFilter : class, new()
{
    private readonly IConsoleDisplayService _display = display ?? throw new ArgumentNullException(nameof(display));
    protected readonly ILogger Logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // Abstract methods that must be implemented by derived classes
    public abstract T CreateEntityUi();
    public abstract T UpdateEntityUi(T existingEntity);
    public abstract TFilter FilterEntityUi();
    protected abstract string EntityName { get; }
    protected abstract string EntityPluralName { get; }

    // Common UI operations with default implementations
    public virtual void DisplayEntitiesTable(IEnumerable<T> entities)
    {
        _display.DisplayTable(entities, EntityPluralName);
    }

    public virtual int GetEntityByIdUi()
    {
        return AnsiConsole.Ask<int>($"[green]Enter {EntityName.ToLower()} ID:[/]");
    }

    public virtual int SelectEntityUi()
    {
        return AnsiConsole.Ask<int>($"[green]Select {EntityName.ToLower()} ID:[/]");
    }

    // Helper methods for common UI patterns
    protected virtual void DisplayCreateHeader()
    {
        _display.DisplayHeader($"Create New {EntityName}");
    }

    protected virtual void DisplayUpdateHeader(string identifier)
    {
        _display.DisplayHeader($"Update {EntityName}: {identifier}");
    }

    protected virtual void DisplayFilterHeader()
    {
        _display.DisplayHeader($"Filter {EntityPluralName}");
    }

    protected virtual string GetRequiredStringInput(string prompt)
    {
        return AnsiConsole.Ask<string>($"[green]{prompt}:[/]");
    }

    protected virtual string GetOptionalStringInput(string prompt, string defaultValue = "")
    {
        return AnsiConsole.Ask<string>($"[yellow]{prompt} (press Enter to skip):[/]", defaultValue);
    }

    protected virtual TResult? GetOptionalInput<TResult>(string prompt, TResult? defaultValue = default)
    {
        try
        {
            return AnsiConsole.Ask<TResult?>($"[yellow]{prompt} (press Enter to skip):[/]", defaultValue);
        }
        catch
        {
            return defaultValue;
        }
    }

    protected virtual DateTime? GetOptionalDateTimeInput(string prompt)
    {
        try
        {
            return AnsiConsole.Ask<DateTime?>($"[yellow]{prompt} (yyyy-MM-dd HH:mm, press Enter to skip):[/]", null);
        }
        catch
        {
            return null;
        }
    }

    // Validation helper methods
    protected virtual bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }

    protected virtual void DisplayValidationError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }
}
