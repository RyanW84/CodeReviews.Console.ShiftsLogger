using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.MenuSystem.Base;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

/// <summary>
/// Refactored Worker UI implementation following SOLID principles with reduced code duplication
/// </summary>
public class WorkerUi : BaseEntityUi<Worker, WorkerFilterOptions>, IWorkerUi
{
    private readonly UiHelper _uiHelper;
    private readonly IWorkerService _workerService;
    private readonly PaginationHandler _paginationHandler;

    public WorkerUi(
        IConsoleDisplayService display,
        IConsoleInputService input,
        ILogger<WorkerUi> logger,
        IWorkerService workerService
    ) : base(display, logger)
    {
        _uiHelper = new UiHelper(display, logger);
        _workerService = workerService;
        _paginationHandler = new PaginationHandler(display, input);
    }

    protected override string EntityName => "Worker";
    protected override string EntityPluralName => "Workers";

    public override async Task<Worker> CreateUiAsync()
    {
        await Task.CompletedTask;
        _uiHelper.DisplayCreateHeader(EntityName);

        var name = _uiHelper.GetRequiredStringInput("Enter name");
        var email = _uiHelper.GetOptionalStringInput("Enter email");
        var phone = _uiHelper.GetOptionalStringInput("Enter phone number");

        // Validate email if provided
        if (!string.IsNullOrEmpty(email) && !_uiHelper.IsValidEmail(email))
        {
            _uiHelper.DisplayValidationError("Invalid email format.");
            return await CreateUiAsync(); // Retry
        }

        return new Worker
        {
            WorkerId = 0, // Will be assigned by service
            Name = name,
            Email = email,
            PhoneNumber = phone,
        };
    }

    public async Task<Worker> CreateWorkerUiAsync()
    {
        return await CreateUiAsync();
    }

    public Worker CreateWorkerUi()
    {
        return CreateUiAsync().GetAwaiter().GetResult();
    }

    public override async Task<Worker> UpdateUiAsync(Worker existingWorker)
    {
        await Task.CompletedTask;
        _uiHelper.DisplayUpdateHeader(EntityName, existingWorker.Name);

        var name =
            _uiHelper.GetOptionalStringInput("Enter name", existingWorker.Name)
            ?? existingWorker.Name;
        var email = _uiHelper.GetOptionalStringInput("Enter email", existingWorker.Email);
        var phone = _uiHelper.GetOptionalStringInput(
            "Enter phone number",
            existingWorker.PhoneNumber
        );

        // Validate email if provided
        if (!string.IsNullOrEmpty(email) && !_uiHelper.IsValidEmail(email))
        {
            _uiHelper.DisplayValidationError("Invalid email format.");
            return await UpdateUiAsync(existingWorker); // Retry
        }

        return new Worker
        {
            WorkerId = existingWorker.Id,
            Name = name,
            Email = email,
            PhoneNumber = phone,
        };
    }

    public async Task<Worker> UpdateWorkerUiAsync(Worker existingWorker)
    {
        return await UpdateUiAsync(existingWorker);
    }

    public Worker UpdateWorkerUi(Worker existingWorker)
    {
        return UpdateUiAsync(existingWorker).GetAwaiter().GetResult();
    }

    public override async Task<WorkerFilterOptions> FilterUiAsync()
    {
        await Task.CompletedTask;
        _uiHelper.DisplayFilterHeader(EntityPluralName);

        var name = _uiHelper.GetOptionalStringInput("Filter by name");
        var email = _uiHelper.GetOptionalStringInput("Filter by email");
        var phone = _uiHelper.GetOptionalStringInput("Filter by phone");

        return new WorkerFilterOptions
        {
            Name = name,
            Email = email,
            PhoneNumber = phone,
        };
    }

    public async Task<WorkerFilterOptions> FilterWorkersUiAsync()
    {
        return await FilterUiAsync();
    }

    public WorkerFilterOptions FilterWorkersUi()
    {
        return FilterUiAsync().GetAwaiter().GetResult();
    }

    public void DisplayWorkersTable(IEnumerable<Worker> workers, int startingRowNumber = 1)
    {
        _display.DisplayTable(workers, EntityPluralName, startingRowNumber);
    }

    public async Task<(bool Selected, int WorkerId)> DisplayWorkersWithPaginationAsync(
        int initialPageNumber = 1,
        int pageSize = 10
    )
    {
        var currentPage = initialPageNumber;

        while (true)
        {
            _display.DisplayHeader($"Workers (Page {currentPage})", "blue");

            var response = await _workerService
                .GetAllWorkersAsync(currentPage, pageSize)
                .ConfigureAwait(false);

            if (response.RequestFailed || response.Data == null || !response.Data.Any())
            {
                if (currentPage == 1)
                {
                    _display.DisplayError("No workers found.");
                    return (false, -1);
                }
                else
                {
                    _display.DisplayError(
                        $"No workers found on page {currentPage}. Returning to page 1."
                    );
                    currentPage = 1;
                    continue;
                }
            }

            // Calculate starting index for continuous numbering across pages
            int startIndex = (currentPage - 1) * pageSize;

            DisplayWorkersTable(response.Data, startIndex + 1);

            // Display pagination info
            _display.DisplayInfo(
                $"Page {response.PageNumber} of {response.TotalPages} | Total: {response.TotalCount} workers"
            );
            _display.DisplayInfo(
                $"Showing {response.Data.Count()} of {response.TotalCount} workers"
            );

            // Create pagination options
            var options = new List<string>();

            if (response.HasPreviousPage)
                options.Add("Previous Page");

            if (response.HasNextPage)
                options.Add("Next Page");

            options.Add("Go to Page");
            options.Add("Change Page Size");
            options.Add("Select Worker");
            options.Add("Back to Menu");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Choose an action:").AddChoices(options)
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
                    var pageInput = AnsiConsole.Ask<int>(
                        $"Enter page number (1-{response.TotalPages}):"
                    );
                    if (pageInput >= 1 && pageInput <= response.TotalPages)
                        currentPage = pageInput;
                    else
                        _display.DisplayError(
                            $"Invalid page number. Please enter a number between 1 and {response.TotalPages}."
                        );
                    break;

                case "Change Page Size":
                    var sizeInput = AnsiConsole.Ask<int>("Enter new page size (1-100):");
                    if (sizeInput >= 1 && sizeInput <= 100)
                    {
                        pageSize = sizeInput;
                        currentPage = 1; // Reset to first page
                    }
                    else
                        _display.DisplayError(
                            "Invalid page size. Please enter a number between 1 and 100."
                        );
                    break;

                case "Select Worker":
                    // Return the current page data for selection
                    var selectedWorkerId = await GetWorkerByIdUi();
                    return (true, selectedWorkerId);

                case "Back to Menu":
                    return (false, -1);
            }
        }
    }

    public async Task<int> GetWorkerByIdUi()
    {
        var result = await _paginationHandler.HandlePaginationAsync(
            async (page, size) => await _workerService.GetAllWorkersAsync(page, size),
            (response, page, size) => DisplayPage(response, page, size),
            BuildChoices,
            async (selected, response, page, size) =>
            {
                await Task.CompletedTask;
                if (selected == "Next Page...") return null; // Continue pagination
                else if (selected == "Previous Page...") return null;
                else if (selected == "Enter ID Manually") return AnsiConsole.Ask<int>("[green]Enter worker ID:[/]");
                else if (selected == "Cancel/Return to Menu") return -1;
                else
                {
                    var choices = BuildChoices(response, page, size);
                    var index = choices.IndexOf(selected);
                    if (index >= 0 && response.Data != null && index < response.Data.Count)
                    {
                        return response.Data[index].WorkerId;
                    }
                }
                return null;
            },
            1,
            10
        );
        return result as int? ?? -1;
    }

    private void DisplayPage(ApiResponseDto<List<Worker>> response, int currentPage, int pageSize)
    {
        int startIndex = (currentPage - 1) * pageSize;
        if (response.Data != null) DisplayWorkersTable(response.Data, startIndex + 1);
        _display.DisplayInfo($"Page {response.PageNumber} of {response.TotalPages} | Total: {response.TotalCount} workers");
    }

    private List<string> BuildChoices(ApiResponseDto<List<Worker>> response, int currentPage, int pageSize)
    {
        int startIndex = (currentPage - 1) * pageSize;
        var choices = response.Data?.Select((w, index) =>
            $"{startIndex + index + 1}. {w.Name}"
        ).ToList() ?? new List<string>();

        if (response.HasNextPage) choices.Add("Next Page...");
        if (response.HasPreviousPage) choices.Add("Previous Page...");
        choices.Add("Enter ID Manually");
        choices.Add("Cancel/Return to Menu");
        return choices;
    }

    public async Task<int> SelectWorker()
    {
        return await GetWorkerByIdUi();
    }
}
