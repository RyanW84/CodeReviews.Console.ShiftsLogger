using System.ComponentModel.DataAnnotations;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Models;

public class Location : IEntity
{
    [Key] public int LocationId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string PostCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Navigation property for related shifts
    public virtual ICollection<Shift> Shifts { get; set; } = [];

    // IEntity implementation
    public int Id => LocationId;
}