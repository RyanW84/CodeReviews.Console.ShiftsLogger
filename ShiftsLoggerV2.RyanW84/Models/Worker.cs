using System.ComponentModel.DataAnnotations;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Models;

public class Worker : IEntity
{
    [Key] public int WorkerId { get; set; }

    public string Name { get; set; } = string.Empty;

    [Phone] public string? PhoneNumber { get; set; }

    [EmailAddress] public string? Email { get; set; }

    // Navigation property for related shifts
    public virtual ICollection<Shift> Shifts { get; set; } = [];

	// For compatibility with UI layer
	public int Id => WorkerId;
	public string? Phone => PhoneNumber;

    // Computed count of shifts for lightweight projections (populated by repository)
    public int ShiftCount { get; set; }
}