using System.ComponentModel.DataAnnotations;

namespace ConsoleFrontEnd.Models;

public class Location
{
    [Key] public int LocationId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string PostCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // For compatibility with UI layer
    public int Id => LocationId;

    public virtual ICollection<Shift>? Shifts { get; set; } // Navigation property to the Shifts entity
    public virtual ICollection<Worker>? Workers { get; set; } // Navigation property to the Workers entity
}