using FluentAssertions;
using ShiftsLoggerV2.RyanW84.Models;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Models;

public class ShiftTests
{
    [Fact]
    public void Shift_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var shift = new Shift();

        // Assert
        shift.ShiftId.Should().Be(0);
        shift.WorkerId.Should().Be(0);
        shift.LocationId.Should().Be(0);
        shift.StartTime.Should().Be(default(DateTimeOffset));
        shift.EndTime.Should().Be(default(DateTimeOffset));
        shift.Location.Should().BeNull();
        shift.Worker.Should().BeNull();
    }

    [Fact]
    public void Shift_Id_Property_ShouldReturnShiftId()
    {
        // Arrange
        var shift = new Shift { ShiftId = 456 };

        // Act
        var id = shift.Id;

        // Assert
        id.Should().Be(456);
    }

    [Fact]
    public void Shift_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var startTime = new DateTimeOffset(2025, 8, 26, 9, 0, 0, TimeSpan.Zero);
        var endTime = new DateTimeOffset(2025, 8, 26, 17, 0, 0, TimeSpan.Zero);
        var worker = new Worker { WorkerId = 1, Name = "John Doe" };
        var location = new Location { LocationId = 1, Name = "Office A" };

        // Act
        var shift = new Shift
        {
            ShiftId = 100,
            WorkerId = 1,
            LocationId = 1,
            StartTime = startTime,
            EndTime = endTime,
            Worker = worker,
            Location = location
        };

        // Assert
        shift.ShiftId.Should().Be(100);
        shift.WorkerId.Should().Be(1);
        shift.LocationId.Should().Be(1);
        shift.StartTime.Should().Be(startTime);
        shift.EndTime.Should().Be(endTime);
        shift.Worker.Should().Be(worker);
        shift.Location.Should().Be(location);
        shift.Id.Should().Be(100);
    }

    [Fact]
    public void Shift_StartTimeAndEndTime_ShouldAcceptValidDateTimeOffsets()
    {
        // Arrange
        var shift = new Shift();
        var startTime = DateTimeOffset.Now;
        var endTime = startTime.AddHours(8);

        // Act
        shift.StartTime = startTime;
        shift.EndTime = endTime;

        // Assert
        shift.StartTime.Should().Be(startTime);
        shift.EndTime.Should().Be(endTime);
        shift.EndTime.Should().BeAfter(shift.StartTime);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(999, 888)]
    [InlineData(0, 0)]
    public void Shift_WorkerIdAndLocationId_ShouldSetCorrectly(int workerId, int locationId)
    {
        // Arrange & Act
        var shift = new Shift
        {
            WorkerId = workerId,
            LocationId = locationId
        };

        // Assert
        shift.WorkerId.Should().Be(workerId);
        shift.LocationId.Should().Be(locationId);
    }

    [Fact]
    public void Shift_NavigationProperties_ShouldAllowNullValues()
    {
        // Arrange & Act
        var shift = new Shift
        {
            Worker = null,
            Location = null
        };

        // Assert
        shift.Worker.Should().BeNull();
        shift.Location.Should().BeNull();
    }
}
