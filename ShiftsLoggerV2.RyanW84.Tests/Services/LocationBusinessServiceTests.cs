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
    public class LocationBusinessServiceTests
    {
        private readonly Mock<ILocationRepository> _mockLocationRepository;
        private readonly LocationBusinessService _service;

        public LocationBusinessServiceTests()
        {
            _mockLocationRepository = new Mock<ILocationRepository>();
            _service = new LocationBusinessService(_mockLocationRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_WhenSuccessful_ShouldReturnLocations()
        {
            // Arrange
            var filterOptions = new LocationFilterOptions();
            var locations = new List<Location>
            {
                new() { LocationId = 1, Name = "Main Office", Address = "123 Main St" },
                new() { LocationId = 2, Name = "Branch Office", Address = "456 Oak Ave" }
            };

            _mockLocationRepository.Setup(s => s.GetAllAsync(filterOptions))
                .ReturnsAsync(Result<List<Location>>.Success(locations, "Locations retrieved successfully"));

            // Act
            var result = await _service.GetAllAsync(filterOptions);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data![0].Name.Should().Be("Main Office");
            result.Data[1].Name.Should().Be("Branch Office");
        }

    [Fact]
    public async Task GetAllAsync_WhenExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var filterOptions = new LocationFilterOptions();
        _mockLocationRepository.Setup(s => s.GetAllAsync(filterOptions))
            .ReturnsAsync(Result<List<Location>>.Failure("Database error", HttpStatusCode.InternalServerError));

        // Act
        var result = await _service.GetAllAsync(filterOptions);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Contain("Database error");
    }        [Fact]
        public async Task GetByIdAsync_WhenLocationExists_ShouldReturnLocation()
        {
            // Arrange
            const int locationId = 1;
            var location = new Location
            {
                LocationId = locationId,
                Name = "Main Office",
                Address = "123 Main St",
                Town = "Anytown",
                County = "AnyCounty",
                PostCode = "12345",
                Country = "USA"
            };

            _mockLocationRepository.Setup(s => s.GetByIdAsync(locationId))
                .ReturnsAsync(Result<Location>.Success(location, "Location found"));

            // Act
            var result = await _service.GetByIdAsync(locationId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.LocationId.Should().Be(locationId);
            result.Data.Name.Should().Be("Main Office");
        }

        [Fact]
        public async Task GetByIdAsync_WhenLocationNotFound_ShouldReturnFailure()
        {
            // Arrange
            const int locationId = 999;
            _mockLocationRepository.Setup(s => s.GetByIdAsync(locationId))
                .ReturnsAsync(Result<Location>.Failure("Location not found", HttpStatusCode.NotFound));

            // Act
            var result = await _service.GetByIdAsync(locationId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            result.Message.Should().Contain("not found");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_WhenValidData_ShouldReturnCreatedLocation()
        {
            // Arrange
            var locationDto = new LocationApiRequestDto
            {
                Name = "New Office",
                Address = "789 Pine St",
                Town = "Newtown",
                County = "NewCounty",
                PostCode = "67890",
                Country = "USA"
            };

            var createdLocation = new Location
            {
                LocationId = 3,
                Name = locationDto.Name,
                Address = locationDto.Address,
                Town = locationDto.Town,
                County = locationDto.County,
                PostCode = locationDto.PostCode,
                Country = locationDto.Country
            };

            _mockLocationRepository.Setup(s => s.CreateAsync(locationDto))
                .ReturnsAsync(Result<Location>.Success(createdLocation, "Location created successfully"));

            // Act
            var result = await _service.CreateAsync(locationDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Name.Should().Be("New Office");
            result.Data.LocationId.Should().Be(3);
        }

        [Fact]
        public async Task UpdateAsync_WhenValidData_ShouldReturnUpdatedLocation()
        {
            // Arrange
            const int locationId = 1;
            var locationDto = new LocationApiRequestDto
            {
                Name = "Updated Office",
                Address = "123 Updated St",
                Town = "Updatedtown",
                County = "UpdatedCounty",
                PostCode = "12345",
                Country = "USA"
            };

            var existingLocation = new Location
            {
                LocationId = locationId,
                Name = "Old Office",
                Address = "Old Address"
            };

            var updatedLocation = new Location
            {
                LocationId = locationId,
                Name = locationDto.Name,
                Address = locationDto.Address,
                Town = locationDto.Town,
                County = locationDto.County,
                PostCode = locationDto.PostCode,
                Country = locationDto.Country
            };

            _mockLocationRepository.Setup(s => s.GetByIdAsync(locationId))
                .ReturnsAsync(Result<Location>.Success(existingLocation, "Location found"));
            _mockLocationRepository.Setup(s => s.UpdateAsync(locationId, locationDto))
                .ReturnsAsync(Result<Location>.Success(updatedLocation, "Location updated successfully"));

            // Act
            var result = await _service.UpdateAsync(locationId, locationDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Name.Should().Be("Updated Office");
            result.Data.LocationId.Should().Be(locationId);
        }

        [Fact]
        public async Task DeleteAsync_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            const int locationId = 1;
            var existingLocation = new Location
            {
                LocationId = locationId,
                Name = "Office to Delete"
            };

            _mockLocationRepository.Setup(s => s.GetByIdAsync(locationId))
                .ReturnsAsync(Result<Location>.Success(existingLocation, "Location found"));
            _mockLocationRepository.Setup(s => s.DeleteAsync(locationId))
                .ReturnsAsync(Result.Success("Location deleted successfully"));

            // Act
            var result = await _service.DeleteAsync(locationId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("successfully");
        }
    }
}
