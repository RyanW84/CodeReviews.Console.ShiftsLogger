using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ShiftsLoggerV2.RyanW84.Common;

namespace ShiftsLoggerV2.RyanW84.Dtos;

public class ShiftApiRequestDto
{
    [Required] [Range(1, 255)] public int WorkerId { get; set; }

    [Required]
    [JsonConverter(typeof(DdMmYyyyHHmmDateTimeOffsetConverter))]
    public DateTimeOffset StartTime { get; set; }

    [Required]
    [JsonConverter(typeof(DdMmYyyyHHmmDateTimeOffsetConverter))]
    public DateTimeOffset EndTime { get; set; }

    [Required] [Range(1, 255)] public int LocationId { get; set; }
}