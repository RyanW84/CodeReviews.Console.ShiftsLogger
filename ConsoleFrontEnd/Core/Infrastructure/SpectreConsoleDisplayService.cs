using System.Reflection;
using ConsoleFrontEnd.Core.Abstractions;
using Spectre.Console;

namespace ConsoleFrontEnd.Core.Infrastructure;

/// <summary>
/// Console display service implementation using Spectre.Console
/// Following Single Responsibility Principle - handles only display operations
/// </summary>
public class SpectreConsoleDisplayService : IConsoleDisplayService
{
    private static readonly object _consoleLock = new();

    // Dictionary to define model-specific property exclusions
    private static readonly Dictionary<string, HashSet<string>> ModelSpecificExclusions = new()
    {
        ["Shift"] = new HashSet<string> { "Id", "Start", "End" },
        ["Worker"] = new HashSet<string> { "Id", "Phone" },
        ["Location"] = new HashSet<string> { "Id" },
    };

    // Property ordering for better table display
    private static readonly Dictionary<string, string[]> ModelPropertyOrder = new()
    {
        ["Shift"] = ["ShiftId", "WorkerName", "LocationName", "StartTime", "EndTime"],
        ["Worker"] = ["WorkerId", "Name", "PhoneNumber", "Email", "ShiftCount"],
        ["Location"] =
        [
            "LocationId",
            "Name",
            "Address",
            "Town",
            "County",
            "PostCode",
            "Country",
            "ShiftCount",
        ],
    };

    public void Clear()
    {
        lock (_consoleLock)
        {
            AnsiConsole.Clear();
        }
    }

    public void DisplayHeader(string title, string color = "yellow")
    {
        lock (_consoleLock)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold]{title}[/]").RuleStyle(color).Centered());
            AnsiConsole.WriteLine();
        }
    }

    public void DisplayError(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[red]ERROR: {Markup.Escape(message)}[/]");
        }
    }

    public void DisplaySuccess(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[green]SUCCESS: {Markup.Escape(message)}[/]");
        }
    }

    public void DisplayInfo(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[cyan]{Markup.Escape(message)}[/]");
        }
    }

    public void DisplayWarning(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
        }
    }

    public void DisplayText(string text)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine(Markup.Escape(text));
        }
    }

    public void DisplayPrompt(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.Markup($"[bold cyan]{Markup.Escape(message)}[/] ");
        }
    }

    public string GetInput()
    {
        lock (_consoleLock)
        {
            return Console.ReadLine() ?? string.Empty;
        }
    }

    public void WaitForKeyPress()
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }

    public void DisplayMenu(string title, string[] options)
    {
        lock (_consoleLock)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[bold cyan]{title}[/]").RuleStyle("cyan").LeftJustified());
            AnsiConsole.WriteLine();

            for (int i = 0; i < options.Length; i++)
            {
                AnsiConsole.MarkupLine($"[green]{i + 1}.[/] {Markup.Escape(options[i])}");
            }
            AnsiConsole.WriteLine();
        }
    }

    public void DisplayTable<T>(IEnumerable<T> data, string? title = null, int startingRowNumber = 1)
    {
        lock (_consoleLock)
        {
            var table = new Table();
            if (!string.IsNullOrEmpty(title))
            {
                table.Title = new TableTitle($"[bold yellow]{title}[/]");
            }
            table.Border = TableBorder.Rounded;

            if (!data.Any())
            {
                AnsiConsole.MarkupLine(
                    $"[yellow]No data available{(string.IsNullOrEmpty(title) ? "" : $" for {title}")}[/]"
                );
                return;
            }

            var modelName = typeof(T).Name;

            // Handle different models with specialized displays
            switch (modelName)
            {
                case "Shift":
                    DisplayShiftsTable(data.Cast<dynamic>(), title, startingRowNumber);
                    return;
                case "Worker":
                    DisplayWorkersTable(data.Cast<dynamic>(), title, startingRowNumber);
                    return;
                case "Location":
                    DisplayLocationsTable(data.Cast<dynamic>(), title, startingRowNumber);
                    return;
            }

            // Default generic table display for other models
            var properties = GetFilteredProperties<T>(modelName);

            // Add columns
            foreach (var prop in properties)
            {
                var columnName = GetFriendlyColumnName(prop.Name);
                table.AddColumn(new TableColumn($"[bold]{columnName}[/]").Centered());
            }

            // Add rows
            foreach (var item in data)
            {
                var values = properties
                    .Select(prop => FormatPropertyValue(prop.GetValue(item)))
                    .ToArray();
                table.AddRow(values);
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }

    private void DisplayShiftsTable(IEnumerable<dynamic> shifts, string? title = "Shifts", int startingRowNumber = 1)
    {
        var table = new Table();
        if (!string.IsNullOrEmpty(title))
        {
            table.Title = new TableTitle($"[bold yellow]{title}[/]");
        }
        table.Border = TableBorder.Rounded;

        // Add specific columns for shifts with row count instead of shift ID
        table.AddColumn(new TableColumn("[bold]#[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Worker Name[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Location Name[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Start Time[/]").Centered());
        table.AddColumn(new TableColumn("[bold]End Time[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Duration[/]").Centered());

        // Add rows with row number instead of shift ID
        var rowCount = startingRowNumber;
        foreach (var shift in shifts)
        {
            var workerName = shift.Worker?.Name ?? "N/A";
            var locationName = shift.Location?.Name ?? "N/A";
            var startTime = FormatPropertyValue(shift.StartTime);
            var endTime = FormatPropertyValue(shift.EndTime);
            var duration = FormatDuration(shift.Duration);

            TableExtensions.AddRow(
                table,
                new string[]
                {
                    rowCount.ToString(),
                    Markup.Escape(workerName),
                    Markup.Escape(locationName),
                    startTime,
                    endTime,
                    duration.ToString(),
                }
            );
            rowCount++;
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private void DisplayWorkersTable(IEnumerable<dynamic> workers, string? title = "Workers", int startingRowNumber = 1)
    {
        var table = new Table();
        if (!string.IsNullOrEmpty(title))
        {
            table.Title = new TableTitle($"[bold yellow]{title}[/]");
        }
        table.Border = TableBorder.Rounded;

        // Add columns: row count, name, phone, email, total shifts
        table.AddColumn(new TableColumn("[bold]#[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Name[/]").LeftAligned());
        table.AddColumn(new TableColumn("[bold]Phone[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Email[/]").LeftAligned());
        table.AddColumn(new TableColumn("[bold]Total Shifts[/]").Centered());

        // Add rows with row number instead of worker ID
        var rowCount = startingRowNumber;
        foreach (var worker in workers)
        {
            // Use lightweight ShiftCount when available to avoid loading full collections
            var shiftCount = (worker.ShiftCount != 0) ? worker.ShiftCount : (worker.Shifts?.Count ?? 0);
            var phoneDisplay = string.IsNullOrWhiteSpace(worker.PhoneNumber)
                ? "N/A"
                : worker.PhoneNumber;
            var emailDisplay = string.IsNullOrWhiteSpace(worker.Email) ? "N/A" : worker.Email;
            var workerName = worker.Name ?? "N/A";

            TableExtensions.AddRow(
                table,
                new string[]
                {
                    rowCount.ToString(),
                    Markup.Escape(workerName),
                    Markup.Escape(phoneDisplay),
                    Markup.Escape(emailDisplay),
                    shiftCount.ToString(),
                }
            );
            rowCount++;
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private void DisplayLocationsTable(IEnumerable<dynamic> locations, string? title = "Locations", int startingRowNumber = 1)
    {
        var table = new Table();
        if (!string.IsNullOrEmpty(title))
        {
            table.Title = new TableTitle($"[bold yellow]{title}[/]");
        }
        table.Border = TableBorder.Rounded;

        // Add columns: row count, name, town, county, post code, country, total shifts
        table.AddColumn(new TableColumn("[bold]#[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Name[/]").LeftAligned());
        table.AddColumn(new TableColumn("[bold]Town[/]").LeftAligned());
        table.AddColumn(new TableColumn("[bold]County[/]").LeftAligned());
        table.AddColumn(new TableColumn("[bold]Post Code[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Country[/]").LeftAligned());
        table.AddColumn(new TableColumn("[bold]Total Shifts[/]").Centered());

        // Add rows with row number instead of location ID
        var rowCount = startingRowNumber;
        foreach (var location in locations)
        {
            var shiftCount = location.Shifts?.Count ?? 0;
            var locationName = location.Name ?? "N/A";
            var town = location.Town ?? "N/A";
            var county = location.County ?? "N/A";
            var postCode = location.PostCode ?? "N/A";
            var country = location.Country ?? "N/A";

            TableExtensions.AddRow(
                table,
                new string[]
                {
                    rowCount.ToString(),
                    Markup.Escape(locationName),
                    Markup.Escape(town),
                    Markup.Escape(county),
                    Markup.Escape(postCode),
                    Markup.Escape(country),
                    shiftCount.ToString(),
                }
            );
            rowCount++;
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private PropertyInfo[] GetFilteredProperties<T>(string modelName)
    {
        var allProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Filter properties based on type and model-specific exclusions
        var filteredProperties = allProperties
            .Where(prop =>
                IsDisplayablePropertyType(prop.PropertyType)
                && !IsNavigationProperty(prop)
                && !IsExcludedProperty(modelName, prop.Name)
            )
            .ToArray();

        // Apply custom ordering if defined for this model
        if (ModelPropertyOrder.TryGetValue(modelName, out var order))
        {
            return filteredProperties
                .OrderBy(prop =>
                {
                    var index = Array.IndexOf(order, prop.Name);
                    return index == -1 ? int.MaxValue : index;
                })
                .ToArray();
        }

        return filteredProperties.OrderBy(prop => prop.Name).ToArray();
    }

    private static bool IsDisplayablePropertyType(Type propertyType)
    {
        // Include primitive types, strings, DateTime, DateTimeOffset, decimal, Guid and their nullable variants
        return propertyType.IsPrimitive
            || propertyType == typeof(string)
            || propertyType == typeof(DateTime)
            || propertyType == typeof(DateTimeOffset)
            || propertyType == typeof(DateTime?)
            || propertyType == typeof(DateTimeOffset?)
            || propertyType == typeof(decimal)
            || propertyType == typeof(decimal?)
            || propertyType == typeof(Guid)
            || propertyType == typeof(Guid?)
            || Nullable.GetUnderlyingType(propertyType)?.IsPrimitive == true;
    }

    private static bool IsNavigationProperty(PropertyInfo prop)
    {
        // Check if it's a virtual property (typical for EF navigation properties)
        return prop.GetMethod?.IsVirtual == true
            && (
                prop.PropertyType.IsClass && prop.PropertyType != typeof(string)
                || prop.PropertyType.IsGenericType
                    && (
                        prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                        || prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                    )
            );
    }

    private static bool IsExcludedProperty(string modelName, string propertyName)
    {
        return ModelSpecificExclusions.TryGetValue(modelName, out var exclusions)
            && exclusions.Contains(propertyName);
    }

    private static string GetFriendlyColumnName(string propertyName)
    {
        // Convert camelCase/PascalCase to friendly names
        return propertyName switch
        {
            "WorkerId" => "Worker ID",
            "LocationId" => "Location ID",
            "ShiftId" => "Shift ID",
            "StartTime" => "Start Time",
            "EndTime" => "End Time",
            "Duration" => "Duration",
            "PhoneNumber" => "Phone",
            "PostCode" => "Post Code",
            _ => propertyName,
        };
    }

    private static string FormatPropertyValue(object? value)
    {
        return value switch
        {
            null => "N/A",
            DateTime dateTime => dateTime.ToString("dd/MM/yyyy HH:mm"),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("dd/MM/yyyy HH:mm"),
            string str when string.IsNullOrWhiteSpace(str) => "N/A",
            _ => Markup.Escape(value.ToString() ?? "N/A"),
        };
    }

    private static string FormatDuration(object? value)
    {
        if (value is TimeSpan timeSpan)
        {
            // Format duration as HH:mm or D days HH:mm if longer than 24 hours
            if (timeSpan.TotalDays >= 1)
            {
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours:00}:{timeSpan.Minutes:00}";
            }
            else
            {
                return $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}";
            }
        }
        return "N/A";
    }

    public void DisplaySeparator()
    {
        lock (_consoleLock)
        {
            AnsiConsole.Write(new Rule().RuleStyle("dim"));
        }
    }

    public void DisplaySystemInfo()
    {
        lock (_consoleLock)
        {
            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Title = new TableTitle("[bold yellow]System Information[/]");

            table.AddColumn(new TableColumn("[bold]Property[/]"));
            table.AddColumn(new TableColumn("[bold]Value[/]"));

            table.AddRow("Operating System", Environment.OSVersion.ToString());
            table.AddRow("Machine Name", Environment.MachineName);
            table.AddRow("User Name", Environment.UserName);
            table.AddRow(".NET Version", Environment.Version.ToString());
            table.AddRow("Working Directory", Environment.CurrentDirectory);

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }
}
