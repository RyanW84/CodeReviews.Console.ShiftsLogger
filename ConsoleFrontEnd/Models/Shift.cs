using System.ComponentModel.DataAnnotations;

namespace ConsoleFrontEnd.Models;

public class Shift
{
    [Key] public int ShiftId { get; set; }

    public int WorkerId { get; set; }
    public int LocationId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    // For compatibility with UI layer
    public int Id => ShiftId;

    // Preserve offset: expose DateTimeOffset aliases that map directly to the underlying fields
    public DateTimeOffset Start
    {
        get => StartTime;
        set => StartTime = value;
    }

    public DateTimeOffset End
    {
        get => EndTime;
        set => EndTime = value;
    }

    // Computed property for shift duration
    public TimeSpan Duration => EndTime - StartTime;

    // Navigation property to the Location entity
    public virtual Location? Location { get; set; }

    // Navigation property to the Worker entity
    public virtual Worker? Worker { get; set; }
}