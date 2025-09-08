using System.ComponentModel.DataAnnotations;

namespace ShiftsLoggerV2.RyanW84.Dtos;

public class LocationApiRequestDto
{
    [Required(ErrorMessage = "Location name is required")]
    [MinLength(1, ErrorMessage = "Location name cannot be empty")]
    [MaxLength(255, ErrorMessage = "Location name cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location address is required")]
    [MinLength(1, ErrorMessage = "Location address cannot be empty")]
    [MaxLength(500, ErrorMessage = "Location address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location town is required")]
    [MinLength(1, ErrorMessage = "Location town cannot be empty")]
    [MaxLength(100, ErrorMessage = "Location town cannot exceed 100 characters")]
    public string Town { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location county is required")]
    [MinLength(1, ErrorMessage = "Location county cannot be empty")]
    [MaxLength(100, ErrorMessage = "Location county cannot exceed 100 characters")]
    public string County { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Location state cannot exceed 100 characters")]
    public string? State { get; set; }

    [Required(ErrorMessage = "Location postcode is required")]
    [MinLength(1, ErrorMessage = "Location postcode cannot be empty")]
    [MaxLength(20, ErrorMessage = "Location postcode cannot exceed 20 characters")]
    [RegularExpression(@"^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$",
        ErrorMessage = "Postcode must be in a valid UK format (e.g., SW1A 1AA)")]
    public string Postcode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location country is required")]
    [MinLength(1, ErrorMessage = "Location country cannot be empty")]
    [MaxLength(100, ErrorMessage = "Location country cannot exceed 100 characters")]
    public string Country { get; set; } = string.Empty;
}
