using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

/// <summary>
/// Refactored Worker UI implementation following SOLID principles with reduced code duplication
/// </summary>
public class WorkerUi : IWorkerUi
{
    private readonly UiHelper _uiHelper;
    private readonly IWorkerService _workerService;
    private readonly IConsoleDisplayService _display;
    private const string EntityName = "Worker";
    private const string EntityPluralName = "Workers";

    public WorkerUi(IConsoleDisplayService display, ILogger<WorkerUi> logger, IWorkerService workerService)
    {
        _uiHelper = new UiHelper(display, logger);
        _workerService = workerService;
        _display = display;
    }

    public Worker CreateWorkerUi()
    {
        _uiHelper.DisplayCreateHeader(EntityName);
        
        var name = _uiHelper.GetRequiredStringInput("Enter name");
        var email = _uiHelper.GetOptionalStringInput("Enter email");
        var phone = _uiHelper.GetOptionalStringInput("Enter phone number");

        // Validate email if provided
        if (!string.IsNullOrEmpty(email) && !_uiHelper.IsValidEmail(email))
        {
            _uiHelper.DisplayValidationError("Invalid email format.");
            return CreateWorkerUi(); // Retry
        }

        return new Worker
        {
            WorkerId = 0, // Will be assigned by service
            Name = name,
            Email = email,
            PhoneNumber = phone
        };
    }

    public Worker UpdateWorkerUi(Worker existingWorker)
    {
        _uiHelper.DisplayUpdateHeader(EntityName, existingWorker.Name);
        
        var name = _uiHelper.GetOptionalStringInput("Enter name", existingWorker.Name) ?? existingWorker.Name;
        var email = _uiHelper.GetOptionalStringInput("Enter email", existingWorker.Email);
        var phone = _uiHelper.GetOptionalStringInput("Enter phone number", existingWorker.PhoneNumber);

        // Validate email if provided
        if (!string.IsNullOrEmpty(email) && !_uiHelper.IsValidEmail(email))
        {
            _uiHelper.DisplayValidationError("Invalid email format.");
            return UpdateWorkerUi(existingWorker); // Retry
        }

        return new Worker
        {
            WorkerId = existingWorker.Id,
            Name = name,
            Email = email,
            PhoneNumber = phone
        };
    }

    public WorkerFilterOptions FilterWorkersUi()
    {
        _uiHelper.DisplayFilterHeader(EntityPluralName);
        
        var name = _uiHelper.GetOptionalStringInput("Filter by name");
        var email = _uiHelper.GetOptionalStringInput("Filter by email");
        var phone = _uiHelper.GetOptionalStringInput("Filter by phone");

        return new WorkerFilterOptions
        {
            Name = name,
            Email = email,
            PhoneNumber = phone
        };
    }

    public void DisplayWorkersTable(IEnumerable<Worker> workers, int startingRowNumber = 1)
    {
        _display.DisplayTable(workers, EntityPluralName, startingRowNumber);
    }

    public async Task DisplayWorkersWithPaginationAsync(int initialPageNumber = 1, int pageSize = 10)
    {
        var currentPage = initialPageNumber;

        while (true)
        {
            _display.DisplayHeader($"Workers (Page {currentPage})", "blue");

            var response = await _workerService.GetAllWorkersAsync(currentPage, pageSize).ConfigureAwait(false);

            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (currentPage == 1)
                {
                    _display.DisplayError("No workers found.");
                    return;
                }
                else
                {
                    _display.DisplayError($"No workers found on page {currentPage}. Returning to page 1.");
                    currentPage = 1;
                    continue;
                }
            }

            // Calculate starting index for continuous numbering across pages
            int startIndex = (currentPage - 1) * pageSize;

            DisplayWorkersTable(response.Data, startIndex + 1);

            // Display pagination info
            _display.DisplayInfo($"Page {response.PageNumber} of {response.TotalPages} | Total: {response.TotalCount} workers");
            _display.DisplayInfo($"Showing {response.Data.Count()} of {response.TotalCount} workers");

            // Create pagination options
            var options = new List<string>();

            if (response.HasPreviousPage)
                options.Add("Previous Page");

            if (response.HasNextPage)
                options.Add("Next Page");

            options.Add("Go to Page");
            options.Add("Change Page Size");
            options.Add("Back to Menu");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an action:")
                    .AddChoices(options)
            );

            switch (choice)
            {
                case "Previous Page":
                    currentPage--;
                    break;

                case "Next Page":
                    currentPage++;
                    break;

                case "Go to Page":
                    var pageInput = AnsiConsole.Ask<int>($"Enter page number (1-{response.TotalPages}):");
                    if (pageInput >= 1 && pageInput <= response.TotalPages)
                        currentPage = pageInput;
                    else
                        _display.DisplayError($"Invalid page number. Please enter a number between 1 and {response.TotalPages}.");
                    break;

                case "Change Page Size":
                    var sizeInput = AnsiConsole.Ask<int>("Enter new page size (1-100):");
                    if (sizeInput >= 1 && sizeInput <= 100)
                    {
                        pageSize = sizeInput;
                        currentPage = 1; // Reset to first page
                    }
                    else
                        _display.DisplayError("Invalid page size. Please enter a number between 1 and 100.");
                    break;

                case "Back to Menu":
                    return;
            }
        }
    }

    public async Task<int> GetWorkerByIdUi()
    {
        _display.DisplayHeader("Select Worker", "blue");

        var currentPage = 1;
        const int pageSize = 10; // Display 10 items at a time

        while (true)
        {
            var response = await _workerService.GetAllWorkersAsync(currentPage, pageSize).ConfigureAwait(false);
            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (currentPage == 1)
                {
                    AnsiConsole.MarkupLine("[red]No workers available or failed to fetch workers.[/]");
                    return -1;
                }
                else
                {
                    currentPage = 1;
                    continue;
                }
            }

            // Calculate starting index for continuous numbering across pages
            int startIndex = (currentPage - 1) * pageSize;

            var choices = response.Data
                .Select((w, index) => $"{startIndex + index + 1}. {w.Name}")
                .ToList();

            // Add navigation options if there are more pages
            if (response.HasNextPage)
                choices.Add("Next Page...");
            if (response.HasPreviousPage)
                choices.Add("Previous Page...");

            choices.Add("Enter ID Manually");
            choices.Add("Cancel/Return to Menu");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Select Worker (Page {response.PageNumber} of {response.TotalPages}):")
                    .AddChoices(choices)
            );

            if (selected == "Next Page...")
            {
                currentPage++;
                continue;
            }
            else if (selected == "Previous Page...")
            {
                currentPage--;
                continue;
            }
            else if (selected == "Enter ID Manually")
            {
                return AnsiConsole.Ask<int>("[green]Enter worker ID:[/]");
            }
            else if (selected == "Cancel/Return to Menu")
            {
                return -1; // Signal cancellation
            }
            else
            {
                // Extract the count from the selected choice and get the corresponding worker
                var count = UiHelper.ExtractCountFromChoice(selected);
                if (count > 0 && count <= response.Data.Count)
                {
                    return response.Data[count - 1].WorkerId;
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid selection.[/]");
                    continue;
                }
            }
        }
    }

    public async Task<int> SelectWorker()
    {
        return await GetWorkerByIdUi();
    }
}
