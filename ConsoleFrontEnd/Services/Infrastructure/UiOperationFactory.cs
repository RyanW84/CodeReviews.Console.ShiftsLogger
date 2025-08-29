using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Models.Dtos;
using Spectre.Console;

namespace ConsoleFrontEnd.Services.Infrastructure;

/// <summary>
/// Factory for creating common UI operation handlers following Factory Pattern and SOLID principles
/// </summary>
public static class UiOperationFactory
{
    /// <summary>
    /// Creates a standard CRUD operation handler
    /// </summary>
    public static async Task HandleCrudOperationAsync<T>(
        Func<Task<ApiResponseDto<T>>> operation,
        IConsoleDisplayService display,
        string operationName,
        string successMessage,
        Action<T>? onSuccess = null)
    {
        try
        {
            display.DisplayInfo($"Performing {operationName}...");
            
            var response = await operation();
            
            if (response.RequestFailed || response.Data == null)
            {
                display.DisplayError(response.Message ?? $"Failed to {operationName.ToLower()}.");
            }
            else
            {
                display.DisplaySuccess(successMessage);
                onSuccess?.Invoke(response.Data);
            }
        }
        catch (Exception ex)
        {
            display.DisplayError($"Error during {operationName.ToLower()}: {ex.Message}");
        }
        finally
        {
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Creates a standard list operation handler
    /// </summary>
    public static async Task HandleListOperationAsync<T>(
        Func<Task<ApiResponseDto<List<T>>>> operation,
        IConsoleDisplayService display,
        string operationName,
        string entityPluralName,
        Action<List<T>>? onSuccess = null)
    {
        try
        {
            display.DisplayInfo($"Loading {entityPluralName.ToLower()}...");
            
            var response = await operation();
            
            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                display.DisplayWarning(response.Message ?? $"No {entityPluralName.ToLower()} found.");
            }
            else
            {
                display.DisplaySuccess($"Found {response.Data.Count} {entityPluralName.ToLower()}.");
                onSuccess?.Invoke(response.Data);
            }
        }
        catch (Exception ex)
        {
            display.DisplayError($"Error during {operationName.ToLower()}: {ex.Message}");
        }
        finally
        {
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Creates a standard delete confirmation handler
    /// </summary>
    public static async Task HandleDeleteOperationAsync<T>(
        T entity,
        Func<Task<ApiResponseDto<bool>>> deleteOperation,
        IConsoleDisplayService display,
        string entityName,
        Func<T, string> getEntityDisplayName)
    {
        try
        {
            var entityDisplayName = getEntityDisplayName(entity);
            
            // Confirm deletion
            var confirmed = AnsiConsole.Confirm(
                $"Are you sure you want to delete {entityName.ToLower()} '{entityDisplayName}'?",
                false);
            
            if (!confirmed)
            {
                display.DisplayInfo("Delete operation cancelled.");
                return;
            }

            display.DisplayInfo($"Deleting {entityName.ToLower()}...");
            
            var response = await deleteOperation();
            
            if (response.RequestFailed || !response.Data)
            {
                display.DisplayError(response.Message ?? $"Failed to delete {entityName.ToLower()}.");
            }
            else
            {
                display.DisplaySuccess($"{entityName} '{entityDisplayName}' deleted successfully.");
            }
        }
        catch (Exception ex)
        {
            display.DisplayError($"Error during delete: {ex.Message}");
        }
        finally
        {
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
