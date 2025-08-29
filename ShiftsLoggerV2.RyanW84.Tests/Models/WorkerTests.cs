using FluentAssertions;
using ShiftsLoggerV2.RyanW84.Models;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Models;

public class WorkerTests
{
    [Fact]
    public void Worker_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var worker = new Worker();

        // Assert
        worker.WorkerId.Should().Be(0);
        worker.Name.Should().Be(string.Empty);
        worker.PhoneNumber.Should().BeNull();
        worker.Email.Should().BeNull();
        worker.Shifts.Should().NotBeNull().And.BeEmpty();
        worker.ShiftCount.Should().Be(0);
    }

    [Fact]
    public void Worker_Id_Property_ShouldReturnWorkerId()
    {
        // Arrange
        var worker = new Worker { WorkerId = 123 };

        // Act
        var id = worker.Id;

        // Assert
        id.Should().Be(123);
    }

    [Fact]
    public void Worker_Phone_Property_ShouldReturnPhoneNumber()
    {
        // Arrange
        const string phoneNumber = "123-456-7890";
        var worker = new Worker { PhoneNumber = phoneNumber };

        // Act
        var phone = worker.Phone;

        // Assert
        phone.Should().Be(phoneNumber);
    }

    [Theory]
    [InlineData("John Doe", "123-456-7890", "john@example.com")]
    [InlineData("Jane Smith", null, null)]
    [InlineData("", "", "")]
    public void Worker_Properties_ShouldSetAndGetCorrectly(string name, string? phoneNumber, string? email)
    {
        // Arrange & Act
        var worker = new Worker
        {
            WorkerId = 1,
            Name = name,
            PhoneNumber = phoneNumber,
            Email = email
        };

        // Assert
        worker.WorkerId.Should().Be(1);
        worker.Name.Should().Be(name);
        worker.PhoneNumber.Should().Be(phoneNumber);
        worker.Email.Should().Be(email);
        worker.Id.Should().Be(1);
        worker.Phone.Should().Be(phoneNumber);
    }

    [Fact]
    public void Worker_Shifts_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var worker = new Worker();

        // Assert
        worker.Shifts.Should().NotBeNull();
        worker.Shifts.Should().BeOfType<List<Shift>>();
        worker.Shifts.Count.Should().Be(0);
    }

    [Fact]
    public void Worker_Shifts_ShouldAllowAddingShifts()
    {
        // Arrange
        var worker = new Worker();
        var shift = new Shift
        {
            ShiftId = 1,
            WorkerId = worker.WorkerId,
            LocationId = 1,
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        // Act
        worker.Shifts.Add(shift);

        // Assert
        worker.Shifts.Should().HaveCount(1);
        worker.Shifts.First().Should().Be(shift);
    }
}
