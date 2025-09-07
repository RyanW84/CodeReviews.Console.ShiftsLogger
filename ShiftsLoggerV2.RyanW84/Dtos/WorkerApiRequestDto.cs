using System.ComponentModel.DataAnnotations;

namespace ShiftsLoggerV2.RyanW84.Dtos;

public class WorkerApiRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
