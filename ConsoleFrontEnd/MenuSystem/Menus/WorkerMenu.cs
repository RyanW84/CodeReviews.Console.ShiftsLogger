using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
/// Worker menu implementation following Single Responsibility Principle
/// Handles worker-specific operations
/// </summary>
public class WorkerMenu : BaseMenu
{
    private readonly IWorkerService _workerService;
    private readonly IWorkerUi _workerUi;
    private readonly Dictionary<string, Func<Task<bool>>> MenuActions;

    public WorkerMenu(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<WorkerMenu> logger,
        IWorkerService workerService,
        IWorkerUi workerUi
    )
        : base(displayService, inputService, navigationService, logger)
    {
        _workerService = workerService ?? throw new ArgumentNullException(nameof(workerService));
        _workerUi = workerUi ?? throw new ArgumentNullException(nameof(workerUi));

        MenuActions = new Dictionary<string, Func<Task<bool>>>
        {
            ["View All Workers"] = async () => { await ViewAllWorkersAsync(); return false; },
            ["View Worker by ID"] = async () => { await ViewWorkerByIdAsync(); return false; },
            ["Create New Worker"] = async () => { await CreateWorkerAsync(); return false; },
            ["Update Worker"] = async () => { await UpdateWorkerAsync(); return false; },
            ["Delete Worker"] = async () => { await DeleteWorkerAsync(); return false; },
            ["Filter Workers"] = async () => { await FilterWorkersAsync(); return false; },
            ["View Workers by Email Domain"] = async () => { await ViewWorkersByEmailDomainAsync(); return false; },
            ["View Workers by Phone Area Code"] = async () => { await ViewWorkersByPhoneAreaCodeAsync(); return false; },
            ["Back to Main Menu"] = () => Task.FromResult(true)
        };
    }

    public override string Title => "Worker Management";
    public override string Context => "Worker Management";

    protected override async Task ShowMenuAsync()
    {
        bool shouldExit = false;

        while (!shouldExit)
        {
            var choice = await InputService.GetMenuChoiceAsync(
                "Select a worker operation:",
                "View All Workers",
                "View Worker by ID",
                "Create New Worker",
                "Update Worker",
                "Delete Worker",
                "Filter Workers",
                "View Workers by Email Domain",
                "View Workers by Phone Area Code",
                "Back to Main Menu"
            );

            shouldExit = await HandleWorkerChoice(choice);
        }
    }

    private async Task<bool> HandleWorkerChoice(string choice)
    {
        Logger.LogDebug("Worker menu choice selected: {Choice}", choice);

        if (await HandleCommonActions(choice))
            return true;

        if (MenuActions.TryGetValue(choice, out var action))
        {
            return await action();
        }
        else
        {
            DisplayService.DisplayError("Invalid choice");
            await InputService.WaitForKeyPressAsync();
            return false;
        }
    }

    private async Task ViewAllWorkersAsync()
    {
        DisplayService.DisplayHeader("All Workers", "blue");
        await _workerUi.DisplayWorkersWithPaginationAsync();
        await InputService.WaitForKeyPressAsync();
    }

    private async Task ViewWorkerByIdAsync()
    {
        try
        {
            DisplayService.DisplayHeader("View Worker by ID", "blue");

            var workerId = await _workerUi.GetWorkerByIdUi();
            if (workerId <= 0)
            {
                DisplayService.DisplayInfo("Operation cancelled.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            var response = await _workerService.GetWorkerByIdAsync(workerId);

            if (response.RequestFailed)
            {
                DisplayService.DisplayError($"Failed to retrieve worker: {response.Message}");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            if (response.Data == null)
            {
                DisplayService.DisplayError("Worker not found.");
                await InputService.WaitForKeyPressAsync();
                return;
            }

            await DisplayWorkerDetails(response.Data);
        }
        catch (Exception ex)
        {
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
            await InputService.WaitForKeyPressAsync();
        }
    }

    private async Task CreateWorkerAsync()
    {
        DisplayService.DisplayHeader("Create New Worker", "green");

        try
        {
            var worker = await _workerUi.CreateUiAsync();
            var response = await _workerService.CreateWorkerAsync(worker);
            if (response.RequestFailed || response.Data == null)
            {
                DisplayService.DisplayError(response.Message ?? "Failed to create worker.");
            }
            else
            {
                DisplayService.DisplaySuccess("Worker created successfully!");
                DisplayService.DisplayTable([response.Data], "Created Worker");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating worker");
            DisplayService.DisplayError($"Failed to create worker: {ex.Message}");
        }

        await InputService.WaitForKeyPressAsync();
    }

    private async Task UpdateWorkerAsync()
    {
        DisplayService.DisplayHeader("Update Worker", "yellow");

        var workerId = await _workerUi.GetWorkerByIdUi();
        if (workerId <= 0)
        {
            DisplayService.DisplayError("No worker selected.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        // Get the current worker details
        var workerResponse = await _workerService.GetWorkerByIdAsync(workerId);
        if (workerResponse.RequestFailed || workerResponse.Data == null)
        {
            DisplayService.DisplayError(
                workerResponse.Message ?? "Failed to retrieve worker details."
            );
            await InputService.WaitForKeyPressAsync();
            return;
        }

        var worker = workerResponse.Data;
        var updatedWorker = await _workerUi.UpdateUiAsync(worker);
        var response = await _workerService.UpdateWorkerAsync(workerId, updatedWorker);
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "Failed to update worker.");
        }
        else
        {
            DisplayService.DisplaySuccess("Worker updated successfully.");
            DisplayService.DisplayTable([response.Data], "Updated Worker");
        }
        await InputService.WaitForKeyPressAsync();
    }

    private async Task DeleteWorkerAsync()
    {
        DisplayService.DisplayHeader("Delete Worker", "red");

        var workerId = await _workerUi.GetWorkerByIdUi();
        if (workerId <= 0)
        {
            DisplayService.DisplayError("No worker selected.");
            await InputService.WaitForKeyPressAsync();
            return;
        }

        if (await InputService.GetConfirmationAsync($"Are you sure you want to delete worker {workerId}?"))
        {
            var response = await _workerService.DeleteWorkerAsync(workerId);
            if (response.RequestFailed)
            {
                DisplayService.DisplayError(response.Message ?? "Failed to delete worker.");
            }
            else
            {
                DisplayService.DisplaySuccess(
                    response.Message ?? $"Worker {workerId} deleted successfully."
                );
            }
        }
        else
        {
            DisplayService.DisplayInfo("Delete cancelled.");
        }
        await InputService.WaitForKeyPressAsync();
    }

    private async Task FilterWorkersAsync()
    {
        DisplayService.DisplayHeader("Filter Workers", "blue");

        // Get all workers for name/email selection
        var allWorkersResponse = await _workerService.GetAllWorkersAsync();
        string? name = null;
        string? email = null;
        if (allWorkersResponse.Data != null && allWorkersResponse.Data.Any())
        {
            var names = allWorkersResponse
                .Data.Select(w => w.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();
            var emails = allWorkersResponse
                .Data.Select(w => w.Email)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .OrderBy(e => e)
                .ToList();
            if (names.Any())
            {
                string[] nameChoices = ["Any", .. names];
                var selectedName = await InputService.GetMenuChoiceAsync("Filter by Name:", nameChoices);
                if (selectedName != "Any")
                    name = selectedName;
            }
            if (emails.Any())
            {
                string[] emailChoices = ["Any", .. emails.Select(s => s!)];
                var selectedEmail = await InputService.GetMenuChoiceAsync("Filter by Email:", emailChoices);
                if (selectedEmail != "Any")
                    email = selectedEmail;
            }
        }

        var filter = new WorkerFilterOptions
        {
            Name = name,
            Email = email,
            PhoneNumber = await InputService.GetTextInputAsync(
                "Filter by phone (leave blank for any):",
                false
            ),
            // If you add date/time fields in the future, use dd/MM/yyyy HH:mm format for prompts and parsing
        };
        var response = await _workerService.GetWorkersByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? "No workers found matching filter.");
        }
        else
        {
            DisplayService.DisplayTable(response.Data, "Filtered Workers");
            DisplayService.DisplaySuccess($"Total filtered workers: {response.TotalCount}");
        }
        await InputService.WaitForKeyPressAsync();
    }

    private async Task ViewWorkersByEmailDomainAsync()
    {
        DisplayService.DisplayHeader("Workers by Email Domain", "blue");
        var domain = await InputService.GetTextInputAsync("Enter email domain (e.g. gmail.com):");
        var response = await _workerService.GetAllWorkersAsync();
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "No workers found.");
        }
        else
        {
            var filtered = response
                .Data.Where(w =>
                    !string.IsNullOrEmpty(w.Email)
                    && w.Email.EndsWith(domain, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();
            if (!filtered.Any())
            {
                DisplayService.DisplayError($"No workers found with email domain '{domain}'.");
            }
            else
            {
                DisplayService.DisplayTable(filtered, $"Workers with domain '{domain}'");
                DisplayService.DisplaySuccess($"Total: {filtered.Count}");
            }
        }
        await InputService.WaitForKeyPressAsync();
    }

    private async Task ViewWorkersByPhoneAreaCodeAsync()
    {
        DisplayService.DisplayHeader("Workers by Phone Area Code", "blue");
        var areaCode = await InputService.GetTextInputAsync("Enter phone area code:");
        var response = await _workerService.GetAllWorkersAsync();
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "No workers found.");
        }
        else
        {
            var filtered = response
                .Data.Where(w =>
                    !string.IsNullOrEmpty(w.PhoneNumber) && w.PhoneNumber.StartsWith(areaCode)
                )
                .ToList();
            if (!filtered.Any())
            {
                DisplayService.DisplayError($"No workers found with area code '{areaCode}'.");
            }
            else
            {
                DisplayService.DisplayTable(filtered, $"Workers with area code '{areaCode}'");
                DisplayService.DisplaySuccess($"Total: {filtered.Count}");
            }
        }
        await InputService.WaitForKeyPressAsync();
    }

    private async Task DisplayWorkerDetails(Worker worker)
    {
        DisplayService.DisplayHeader("Worker Details", "green");
        DisplayService.DisplayTable([worker], "Worker Details");
        await InputService.WaitForKeyPressAsync();
    }
}
