using System.ComponentModel.DataAnnotations;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Models;

public class Shift : IEntity
{
    [Key] public int ShiftId { get; set; }

    public int WorkerId { get; set; }
    public int LocationId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    // Navigation property to the Location entity
    public virtual Location? Location { get; set; }

    // Navigation property to the Worker entity
    public virtual Worker? Worker { get; set; }

    // Computed property for shift duration
    public TimeSpan Duration => EndTime - StartTime;

    // IEntity implementation
    public int Id => ShiftId;
}