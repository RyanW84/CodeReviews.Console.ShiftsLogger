using System.ComponentModel.DataAnnotations;

namespace ConsoleFrontEnd.Models.Dtos;

public class LocationApiRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Town { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string County { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PostCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
}
