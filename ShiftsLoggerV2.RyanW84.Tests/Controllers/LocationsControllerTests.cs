using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShiftsLoggerV2.RyanW84.Controllers;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;
using ShiftsLoggerV2.RyanW84.Common;
using System.Net;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Controllers;

public class LocationsControllerTests
{
    private readonly Mock<ILocationBusinessService> _mockLocationBusinessService;
    private readonly Mock<ILogger<LocationsController>> _mockLogger;
    private readonly LocationsController _controller;

    public LocationsControllerTests()
    {
        _mockLocationBusinessService = new Mock<ILocationBusinessService>();
        _mockLogger = new Mock<ILogger<LocationsController>>();
        _controller = new LocationsController(_mockLocationBusinessService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllLocations_WhenSuccessful_ShouldReturnOkWithLocations()
    {
        // Arrange
        var filterOptions = new LocationFilterOptions();
        var locations = new List<Location>
        {
            new() { LocationId = 1, Name = "Main Office", Address = "123 Main St" },
            new() { LocationId = 2, Name = "Branch Office", Address = "456 Oak Ave" }
        };

        var result = Result<List<Location>>.Success(locations, "Locations retrieved successfully");
        _mockLocationBusinessService.Setup(v => v.GetAllAsync(filterOptions))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetAllLocations(filterOptions);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<PaginatedApiResponseDto<List<Location>>>();

        var apiResponse = okResult.Value as PaginatedApiResponseDto<List<Location>>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data![0].Name.Should().Be("Main Office");
        apiResponse.Data[1].Name.Should().Be("Branch Office");
    }

    [Fact]
    public async Task GetAllLocations_WhenFailed_ShouldReturnErrorResponse()
    {
        // Arrange
        var filterOptions = new LocationFilterOptions();
        var result = Result<List<Location>>.Failure("Database error", HttpStatusCode.InternalServerError);

        _mockLocationBusinessService.Setup(v => v.GetAllAsync(filterOptions))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetAllLocations(filterOptions);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);

        var apiResponse = objectResult.Value as PaginatedApiResponseDto<List<Location>>;
        apiResponse.Should().NotBeNull();
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.InternalServerError);
        apiResponse.Message.Should().Be("Database error");
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetLocationById_WhenLocationExists_ShouldReturnOkWithLocation()
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
        var result = Result<Location>.Success(location, "Location found");

        _mockLocationBusinessService.Setup(v => v.GetByIdAsync(locationId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetLocationById(locationId);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ApiResponseDto<Location>>();

        var apiResponse = okResult.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.LocationId.Should().Be(locationId);
        apiResponse.Data.Name.Should().Be("Main Office");
    }

    [Fact]
    public async Task GetLocationById_WhenLocationNotFound_ShouldReturnNotFound()
    {
        // Arrange
        const int locationId = 999;
        var result = Result<Location>.Failure("Location not found", HttpStatusCode.NotFound);

        _mockLocationBusinessService.Setup(v => v.GetByIdAsync(locationId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetLocationById(locationId);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(404);

        var apiResponse = objectResult.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Message.Should().Be("Location not found");
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateLocation_WhenValidData_ShouldReturnCreated()
    {
        // Arrange
        var locationDto = new LocationApiRequestDto
        {
            Name = "New Office",
            Address = "789 Pine St",
            Town = "Newtown",
            County = "NewCounty",
            State = "NY",
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

        var result = Result<Location>.Success(createdLocation, "Location created successfully");
        _mockLocationBusinessService.Setup(v => v.CreateAsync(locationDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateLocation(locationDto);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ApiResponseDto<Location>>();

        var apiResponse = okResult.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be("New Office");
    }

    [Fact]
    public async Task CreateLocation_WhenInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var locationDto = new LocationApiRequestDto(); // Empty object will cause model validation errors
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var response = await _controller.CreateLocation(locationDto);

        // Assert
        response.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeOfType<ApiResponseDto<object>>();

        var apiResponse = badRequestResult.Value as ApiResponseDto<object>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLocation_WhenValidData_ShouldReturnOk()
    {
        // Arrange
        const int locationId = 1;
        var locationDto = new LocationApiRequestDto
        {
            Name = "Updated Office",
            Address = "123 Updated St",
            Town = "Updatedtown",
            County = "UpdatedCounty",
            State = "CA",
            PostCode = "12345",
            Country = "USA"
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

        var result = Result<Location>.Success(updatedLocation, "Location updated successfully");
        _mockLocationBusinessService.Setup(v => v.UpdateAsync(locationId, locationDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.UpdateLocation(locationId, locationDto);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ApiResponseDto<Location>>();

        var apiResponse = okResult.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be("Updated Office");
    }

    [Fact]
    public async Task DeleteLocation_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        const int locationId = 1;
        var result = Result.Success("Location deleted successfully");

        _mockLocationBusinessService.Setup(v => v.DeleteAsync(locationId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.DeleteLocation(locationId);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ApiResponseDto<string>>();

        var apiResponse = okResult.Value as ApiResponseDto<string>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Message.Should().Be("Location deleted successfully");
    }

    [Fact]
    public async Task DeleteLocation_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        const int locationId = 999;
        var result = Result.Failure("Location not found", HttpStatusCode.NotFound);

        _mockLocationBusinessService.Setup(v => v.DeleteAsync(locationId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.DeleteLocation(locationId);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(404);

        var apiResponse = objectResult.Value as ApiResponseDto<string>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Message.Should().Be("Location not found");
    }
}
