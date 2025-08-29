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

public class WorkersControllerTests
{
    private readonly Mock<IWorkerBusinessService> _mockWorkerBusinessService;
    private readonly Mock<ILogger<WorkersController>> _mockLogger;
    private readonly WorkersController _controller;

    public WorkersControllerTests()
    {
        _mockWorkerBusinessService = new Mock<IWorkerBusinessService>();
        _mockLogger = new Mock<ILogger<WorkersController>>();
        _controller = new WorkersController(_mockWorkerBusinessService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllWorkers_WhenSuccessful_ShouldReturnOkWithWorkers()
    {
        // Arrange
        var filterOptions = new WorkerFilterOptions();
        var workers = new List<Worker>
        {
            new() { WorkerId = 1, Name = "John Doe" },
            new() { WorkerId = 2, Name = "Jane Smith" }
        };

        var result = Result<List<Worker>>.Success(workers, "Workers retrieved successfully");
        _mockWorkerBusinessService.Setup(v => v.GetAllAsync(filterOptions))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetAllWorkers(filterOptions);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<PaginatedApiResponseDto<List<Worker>>>();
        
        var apiResponse = okResult.Value as PaginatedApiResponseDto<List<Worker>>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data![0].Name.Should().Be("John Doe");
        apiResponse.Data[1].Name.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task GetAllWorkers_WhenFailed_ShouldReturnErrorResponse()
    {
        // Arrange
        var filterOptions = new WorkerFilterOptions();
        var result = Result<List<Worker>>.Failure("Database error", HttpStatusCode.InternalServerError);
        
        _mockWorkerBusinessService.Setup(v => v.GetAllAsync(filterOptions))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetAllWorkers(filterOptions);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        
        var apiResponse = objectResult.Value as PaginatedApiResponseDto<List<Worker>>;
        apiResponse.Should().NotBeNull();
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.InternalServerError);
        apiResponse.Message.Should().Be("Database error");
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkerById_WhenWorkerExists_ShouldReturnOkWithWorker()
    {
        // Arrange
        const int workerId = 1;
        var worker = new Worker { WorkerId = workerId, Name = "John Doe" };
        var result = Result<Worker>.Success(worker, "Worker found");
        
        _mockWorkerBusinessService.Setup(v => v.GetByIdAsync(workerId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetWorkerById(workerId);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;
        
        var apiResponse = okResult!.Value as ApiResponseDto<Worker>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data!.WorkerId.Should().Be(workerId);
        apiResponse.Data.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetWorkerById_WhenWorkerNotFound_ShouldReturnNotFound()
    {
        // Arrange
        const int workerId = 999;
        var result = Result<Worker>.Failure("Worker not found", HttpStatusCode.NotFound);
        
        _mockWorkerBusinessService.Setup(v => v.GetByIdAsync(workerId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetWorkerById(workerId);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(404);

        var apiResponse = objectResult.Value as ApiResponseDto<Worker>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateWorker_WhenSuccessful_ShouldReturnCreatedWithWorker()
    {
        // Arrange
        var workerDto = new WorkerApiRequestDto
        {
            Name = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "123-456-7890"
        };

        var createdWorker = new Worker 
        { 
            WorkerId = 1, 
            Name = workerDto.Name,
            Email = workerDto.Email,
            PhoneNumber = workerDto.PhoneNumber
        };

        var result = Result<Worker>.Success(createdWorker, "Worker created successfully");
        
        _mockWorkerBusinessService.Setup(v => v.CreateAsync(workerDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateWorker(workerDto);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<Worker>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Data!.Name.Should().Be("John Doe");
        apiResponse.Data.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task CreateWorker_WhenValidationFails_ShouldReturnBadRequest()
    {
        // Arrange
        var workerDto = new WorkerApiRequestDto { Name = "" }; // Invalid name
        var result = Result<Worker>.Failure("Worker name is required.", HttpStatusCode.BadRequest);
        
        _mockWorkerBusinessService.Setup(v => v.CreateAsync(workerDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateWorker(workerDto);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(400);
        
        var apiResponse = objectResult.Value as ApiResponseDto<Worker>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Message.Should().Be("Worker name is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetWorkerById_WithInvalidId_ShouldReturnBadRequest(int invalidId)
    {
        // Arrange
        _mockWorkerBusinessService.Setup(v => v.GetByIdAsync(invalidId))
            .ReturnsAsync(Result<Worker>.Failure("ID must be greater than 0", HttpStatusCode.BadRequest));

        // Act
        var response = await _controller.GetWorkerById(invalidId);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        
        var apiResponse = objectResult!.Value as ApiResponseDto<Worker>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Message.Should().Contain("ID must be greater than 0");
    }
}
