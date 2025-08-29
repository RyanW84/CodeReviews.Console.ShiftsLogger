using FluentAssertions;
using ShiftsLoggerV2.RyanW84.Models;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Models;

public class LocationTests
{
    [Fact]
    public void Location_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var location = new Location();

        // Assert
        location.LocationId.Should().Be(0);
        location.Name.Should().Be(string.Empty);
        location.Address.Should().Be(string.Empty);
        location.Town.Should().Be(string.Empty);
        location.County.Should().Be(string.Empty);
        location.PostCode.Should().Be(string.Empty);
        location.Country.Should().Be(string.Empty);
        location.Shifts.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Location_Id_Property_ShouldReturnLocationId()
    {
        // Arrange
        var location = new Location { LocationId = 789 };

        // Act
        var id = location.Id;

        // Assert
        id.Should().Be(789);
    }

    [Theory]
    [InlineData("Office A", "123 Main St", "Springfield", "Illinois", "12345", "USA")]
    [InlineData("Warehouse B", "456 Industrial Blvd", "Chicago", "Cook", "60601", "USA")]
    [InlineData("", "", "", "", "", "")]
    public void Location_Properties_ShouldSetAndGetCorrectly(
        string name, string address, string town, string county, string postCode, string country)
    {
        // Arrange & Act
        var location = new Location
        {
            LocationId = 1,
            Name = name,
            Address = address,
            Town = town,
            County = county,
            PostCode = postCode,
            Country = country
        };

        // Assert
        location.LocationId.Should().Be(1);
        location.Name.Should().Be(name);
        location.Address.Should().Be(address);
        location.Town.Should().Be(town);
        location.County.Should().Be(county);
        location.PostCode.Should().Be(postCode);
        location.Country.Should().Be(country);
        location.Id.Should().Be(1);
    }

    [Fact]
    public void Location_Shifts_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var location = new Location();

        // Assert
        location.Shifts.Should().NotBeNull();
        location.Shifts.Should().BeOfType<List<Shift>>();
        location.Shifts.Count.Should().Be(0);
    }

    [Fact]
    public void Location_Shifts_ShouldAllowAddingShifts()
    {
        // Arrange
        var location = new Location { LocationId = 1 };
        var shift = new Shift
        {
            ShiftId = 1,
            LocationId = location.LocationId,
            WorkerId = 1,
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        // Act
        location.Shifts.Add(shift);

        // Assert
        location.Shifts.Should().HaveCount(1);
        location.Shifts.First().Should().Be(shift);
    }

    [Fact]
    public void Location_AllStringProperties_ShouldAcceptNullAndEmptyStrings()
    {
        // Arrange & Act
        var location1 = new Location
        {
            Name = null!,
            Address = null!,
            Town = null!,
            County = null!,
            PostCode = null!,
            Country = null!
        };

        var location2 = new Location
        {
            Name = "",
            Address = "",
            Town = "",
            County = "",
            PostCode = "",
            Country = ""
        };

        // Assert - The properties might be initialized to empty string by default
        // so we just verify they can be set
        location1.Name.Should().BeNull();
        location2.Name.Should().Be("");
    }
}
