using System.ComponentModel.DataAnnotations;

namespace ConsoleFrontEnd.Models;

public class Worker
{
    [Key] public int WorkerId { get; set; }

    public string Name { get; set; } = string.Empty;

    [Phone] public string? PhoneNumber { get; set; }

    [EmailAddress] public string? Email { get; set; }

    // For compatibility with UI layer
    public int Id => WorkerId;
    public string? Phone => PhoneNumber;
    
    // ...existing code...

    public virtual ICollection<Shift>? Shifts { get; set; }
    public virtual ICollection<Location>? Locations { get; set; }
    
    // Lightweight shift count populated by API to avoid loading full collections
    public int ShiftCount { get; set; }
}