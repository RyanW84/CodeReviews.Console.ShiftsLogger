using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public static class InputValidator
{
    public static string GetValidatedString(
        string prompt,
        bool required = true,
        int? maxLength = null
    )
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>($"[green]{prompt}[/]");

            if (required && string.IsNullOrWhiteSpace(input))
            {
                AnsiConsole.MarkupLine("[red]This field is required.[/]");
                continue;
            }

            if (maxLength.HasValue && input.Length > maxLength.Value)
            {
                AnsiConsole.MarkupLine($"[red]Maximum length is {maxLength.Value} characters.[/]");
                continue;
            }

            return input;
        }
    }

    public static string GetValidatedEmail(string prompt)
    {
        while (true)
        {
            var email = AnsiConsole.Ask<string>($"[green]{prompt}[/]");

            if (string.IsNullOrWhiteSpace(email))
            {
                AnsiConsole.MarkupLine("[red]Email is required.[/]");
                continue;
            }

            if (!new EmailAddressAttribute().IsValid(email))
            {
                AnsiConsole.MarkupLine("[red]Please enter a valid email address.[/]");
                continue;
            }

            return email;
        }
    }

    public static string GetValidatedPhoneNumber(string prompt)
    {
        while (true)
        {
            var phone = AnsiConsole.Ask<string>($"[green]{prompt}[/]");

            if (string.IsNullOrWhiteSpace(phone))
            {
                AnsiConsole.MarkupLine("[red]Phone number is required.[/]");
                continue;
            }

            if (!new PhoneAttribute().IsValid(phone))
            {
                AnsiConsole.MarkupLine("[red]Please enter a valid phone number.[/]");
                continue;
            }

            return phone;
        }
    }

    public static int GetValidatedInteger(string prompt, int? min = null, int? max = null)
    {
        while (true)
            try
            {
                var value = AnsiConsole.Ask<int>($"[green]{prompt}[/]");

                if (min.HasValue && value < min.Value)
                {
                    AnsiConsole.MarkupLine($"[red]Value must be at least {min.Value}.[/]");
                    continue;
                }

                if (max.HasValue && value > max.Value)
                {
                    AnsiConsole.MarkupLine($"[red]Value must not exceed {max.Value}.[/]");
                    continue;
                }

                return value;
            }
            catch (FormatException)
            {
                AnsiConsole.MarkupLine("[red]Please enter a valid number.[/]");
            }
    }

    /// <summary>
    ///     Simple flexible DateTime parser - utility function only
    /// </summary>
    public static DateTime GetFlexibleDateTime(string prompt, DateTime? minDate = null, DateTime? maxDate = null)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>($"[green]{prompt}[/] [dim](dd/MM/yyyy HH:mm)[/]");

            if (string.IsNullOrWhiteSpace(input))
            {
                AnsiConsole.MarkupLine("[red]Date and time is required.[/]");
                continue;
            }

            DateTime parsedDateTime = default;
            var isValid = false;

            // Try multiple date formats with explicit culture
            string[] acceptedFormats =
            [
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy H:mm"
            ];

            foreach (var format in acceptedFormats)
            {
                if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out parsedDateTime))
                {
                    isValid = true;
                    break;
                }
            }

            if (!isValid)
            {
                AnsiConsole.MarkupLine("[red]Invalid date/time format. Please use dd/MM/yyyy HH:mm format.[/]");
                AnsiConsole.MarkupLine("[red]Invalid date/time format. Please use dd/MM/yyyy HH:mm format.[/]");
                continue;
            }

            // Validate date range
            if (minDate.HasValue && parsedDateTime < minDate.Value)
            {
                AnsiConsole.MarkupLine($"[red]Date must be after {minDate.Value:dd/MM/yyyy HH:mm}.[/]");
                AnsiConsole.MarkupLine($"[red]Date must be after {minDate.Value:dd/MM/yyyy HH:mm}.[/]");
                continue;
            }

            if (maxDate.HasValue && parsedDateTime > maxDate.Value)
            {
                AnsiConsole.MarkupLine($"[red]Date must be before {maxDate.Value:dd/MM/yyyy HH:mm}.[/]");
                AnsiConsole.MarkupLine($"[red]Date must be before {maxDate.Value:dd/MM/yyyy HH:mm}.[/]");
                continue;
            }

            return parsedDateTime;
        }
    }

    /// <summary>
    /// Prompts for an optional DateTime. Returns null when the user presses Enter without typing a value.
    /// </summary>
    public static DateTime? GetOptionalDateTime(string prompt, DateTime? minDate = null, DateTime? maxDate = null)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>($"[green]{prompt}[/] [dim](dd/MM/yyyy HH:mm, leave blank for none)[/]");

            if (string.IsNullOrWhiteSpace(input))
                return null;

            DateTime parsedDateTime = default;
            var isValid = false;

            string[] acceptedFormats =
            [
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy H:mm",
                "d/MM/yyyy HH:mm",
                "d/MM/yyyy H:mm"
            ];

            foreach (var format in acceptedFormats)
            {
                if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out parsedDateTime))
                {
                    isValid = true;
                    break;
                }
            }

            if (!isValid)
            {
                AnsiConsole.MarkupLine("[red]Invalid date/time format. Please use dd/MM/yyyy HH:mm format.[/]");
                continue;
            }

            if (minDate.HasValue && parsedDateTime < minDate.Value)
            {
                AnsiConsole.MarkupLine($"[red]Date must be after {minDate.Value:dd/MM/yyyy HH:mm}.[/]");
                continue;
            }

            if (maxDate.HasValue && parsedDateTime > maxDate.Value)
            {
                AnsiConsole.MarkupLine($"[red]Date must be before {maxDate.Value:dd/MM/yyyy HH:mm}.[/]");
                continue;
            }

            return parsedDateTime;
        }
    }
}