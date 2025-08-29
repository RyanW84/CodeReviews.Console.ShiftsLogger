using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services;
using ShiftsLoggerV2.RyanW84.Common;
using System.Net;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Services
{
    public class ShiftBusinessServiceTests
    {
        private readonly Mock<IShiftRepository> _mockShiftRepository;
        private readonly ShiftBusinessService _service;

        public ShiftBusinessServiceTests()
        {
            _mockShiftRepository = new Mock<IShiftRepository>();
            _service = new ShiftBusinessService(_mockShiftRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_WhenSuccessful_ShouldReturnShifts()
        {
            // Arrange
            var filterOptions = new ShiftFilterOptions();
            var shifts = new List<Shift>
            {
                new() { ShiftId = 1, WorkerId = 1, LocationId = 1, StartTime = DateTimeOffset.Now, EndTime = DateTimeOffset.Now.AddHours(8) },
                new() { ShiftId = 2, WorkerId = 2, LocationId = 2, StartTime = DateTimeOffset.Now.AddDays(1), EndTime = DateTimeOffset.Now.AddDays(1).AddHours(8) }
            };

            _mockShiftRepository.Setup(s => s.GetAllAsync(filterOptions))
                .ReturnsAsync(Result<List<Shift>>.Success(shifts, "Shifts retrieved successfully"));

            // Act
            var result = await _service.GetAllAsync(filterOptions);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data![0].ShiftId.Should().Be(1);
            result.Data[1].ShiftId.Should().Be(2);
        }

    [Fact]
    public async Task GetAllAsync_WhenExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var filterOptions = new ShiftFilterOptions();
        _mockShiftRepository.Setup(s => s.GetAllAsync(filterOptions))
            .ReturnsAsync(Result<List<Shift>>.Failure("Database error", HttpStatusCode.InternalServerError));

        // Act
        var result = await _service.GetAllAsync(filterOptions);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Contain("Database error");
    }        [Fact]
        public async Task GetByIdAsync_WhenShiftExists_ShouldReturnShift()
        {
            // Arrange
            const int shiftId = 1;
            var shift = new Shift
            {
                ShiftId = shiftId,
                WorkerId = 1,
                LocationId = 1,
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(8)
            };

            _mockShiftRepository.Setup(s => s.GetByIdAsync(shiftId))
                .ReturnsAsync(Result<Shift>.Success(shift, "Shift found"));

            // Act
            var result = await _service.GetByIdAsync(shiftId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.ShiftId.Should().Be(shiftId);
            result.Data.WorkerId.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_WhenShiftNotFound_ShouldReturnFailure()
        {
            // Arrange
            const int shiftId = 999;
            _mockShiftRepository.Setup(s => s.GetByIdAsync(shiftId))
                .ReturnsAsync(Result<Shift>.Failure("Shift not found", HttpStatusCode.NotFound));

            // Act
            var result = await _service.GetByIdAsync(shiftId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            result.Message.Should().Contain("not found");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_WhenValidData_ShouldReturnCreatedShift()
        {
            // Arrange
            var shiftDto = new ShiftApiRequestDto
            {
                WorkerId = 1,
                LocationId = 1,
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(8)
            };

            var createdShift = new Shift
            {
                ShiftId = 3,
                WorkerId = shiftDto.WorkerId,
                LocationId = shiftDto.LocationId,
                StartTime = shiftDto.StartTime,
                EndTime = shiftDto.EndTime
            };

            _mockShiftRepository.Setup(s => s.CreateAsync(shiftDto))
                .ReturnsAsync(Result<Shift>.Success(createdShift, "Shift created successfully"));

            // Act
            var result = await _service.CreateAsync(shiftDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.WorkerId.Should().Be(1);
            result.Data.ShiftId.Should().Be(3);
        }

        [Fact]
        public async Task CreateAsync_WhenInvalidWorkerId_ShouldReturnFailure()
        {
            // Arrange
            var shiftDto = new ShiftApiRequestDto
            {
                WorkerId = 0, // Invalid
                LocationId = 1,
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(8)
            };

            // Act
            var result = await _service.CreateAsync(shiftDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("WorkerId must be greater than zero");
        }

        [Fact]
        public async Task CreateAsync_WhenInvalidLocationId_ShouldReturnFailure()
        {
            // Arrange
            var shiftDto = new ShiftApiRequestDto
            {
                WorkerId = 1,
                LocationId = 0, // Invalid
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(8)
            };

            // Act
            var result = await _service.CreateAsync(shiftDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("LocationId must be greater than zero");
        }

        [Fact]
        public async Task UpdateAsync_WhenValidData_ShouldReturnUpdatedShift()
        {
            // Arrange
            const int shiftId = 1;
            var shiftDto = new ShiftApiRequestDto
            {
                WorkerId = 1,
                LocationId = 1,
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(8)
            };

            var existingShift = new Shift
            {
                ShiftId = shiftId,
                WorkerId = 1,
                LocationId = 1,
                StartTime = DateTimeOffset.Now.AddDays(-1),
                EndTime = DateTimeOffset.Now.AddDays(-1).AddHours(8)
            };

            var updatedShift = new Shift
            {
                ShiftId = shiftId,
                WorkerId = shiftDto.WorkerId,
                LocationId = shiftDto.LocationId,
                StartTime = shiftDto.StartTime,
                EndTime = shiftDto.EndTime
            };

            _mockShiftRepository.Setup(s => s.GetByIdAsync(shiftId))
                .ReturnsAsync(Result<Shift>.Success(existingShift, "Shift found"));
            _mockShiftRepository.Setup(s => s.UpdateAsync(shiftId, shiftDto))
                .ReturnsAsync(Result<Shift>.Success(updatedShift, "Shift updated successfully"));

            // Act
            var result = await _service.UpdateAsync(shiftId, shiftDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.ShiftId.Should().Be(shiftId);
            result.Data.WorkerId.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            const int shiftId = 1;
            var existingShift = new Shift
            {
                ShiftId = shiftId,
                WorkerId = 1,
                LocationId = 1,
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddHours(8)
            };

            _mockShiftRepository.Setup(s => s.GetByIdAsync(shiftId))
                .ReturnsAsync(Result<Shift>.Success(existingShift, "Shift found"));
            _mockShiftRepository.Setup(s => s.DeleteAsync(shiftId))
                .ReturnsAsync(Result.Success("Shift deleted successfully"));

            // Act
            var result = await _service.DeleteAsync(shiftId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("successfully");
        }
    }
}
