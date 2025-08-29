using FluentAssertions;
using Moq;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services;
using System.Net;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Services;

public class WorkerServiceTests
{
    private readonly Mock<IWorkerRepository> _mockWorkerRepository;
    private readonly WorkerService _workerService;

    public WorkerServiceTests()
    {
        _mockWorkerRepository = new Mock<IWorkerRepository>();
        _workerService = new WorkerService(_mockWorkerRepository.Object);
    }

    [Fact]
    public async Task GetAllWorkers_WhenSuccessful_ShouldReturnSuccessResponse()
    {
        // Arrange
        var filterOptions = new WorkerFilterOptions();
        var workers = new List<Worker>
        {
            new() { WorkerId = 1, Name = "John Doe" },
            new() { WorkerId = 2, Name = "Jane Smith" }
        };

        var repositoryResult = Result<List<Worker>>.Success(workers, "Success");
        _mockWorkerRepository.Setup(r => r.GetAllAsync(filterOptions))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _workerService.GetAllWorkers(filterOptions);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().HaveCount(2);
        result.Data![0].Name.Should().Be("John Doe");
        result.Data[1].Name.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task GetAllWorkers_WhenRepositoryFails_ShouldReturnFailureResponse()
    {
        // Arrange
        var filterOptions = new WorkerFilterOptions();
        var repositoryResult = Result<List<Worker>>.Failure("Database error", HttpStatusCode.InternalServerError);
        
        _mockWorkerRepository.Setup(r => r.GetAllAsync(filterOptions))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _workerService.GetAllWorkers(filterOptions);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Database error");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkerById_WhenWorkerExists_ShouldReturnWorker()
    {
        // Arrange
        const int workerId = 1;
        var worker = new Worker { WorkerId = workerId, Name = "John Doe" };
        var repositoryResult = Result<Worker>.Success(worker, "Worker found");
        
        _mockWorkerRepository.Setup(r => r.GetByIdAsync(workerId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _workerService.GetWorkerById(workerId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Data!.WorkerId.Should().Be(workerId);
        result.Data.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetWorkerById_WhenWorkerNotFound_ShouldReturnFailureResponse()
    {
        // Arrange
        const int workerId = 999;
        var repositoryResult = Result<Worker>.Failure("Worker not found", HttpStatusCode.NotFound);
        
        _mockWorkerRepository.Setup(r => r.GetByIdAsync(workerId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _workerService.GetWorkerById(workerId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Worker not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkerById_WhenRepositoryReturnsNullData_ShouldReturnFailureResponse()
    {
        // Arrange
        const int workerId = 1;
        var repositoryResult = Result<Worker>.Success(null!, "No data");
        
        _mockWorkerRepository.Setup(r => r.GetByIdAsync(workerId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _workerService.GetWorkerById(workerId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new WorkerService(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("workerRepository");
    }

    [Fact]
    public async Task CreateWorker_WhenRepositorySucceeds_ShouldReturnSuccessResponse()
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

        _mockWorkerRepository.Setup(r => r.CreateAsync(It.IsAny<WorkerApiRequestDto>()))
            .ReturnsAsync(Result<Worker>.Success(createdWorker, "Worker created"));

        // Act
        var result = await _workerService.CreateWorker(workerDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.Created);
        result.Data!.Name.Should().Be("John Doe");
        result.Data.Email.Should().Be("john@example.com");
        
        // Verify that CreateAsync was called with correct worker data
        _mockWorkerRepository.Verify(r => r.CreateAsync(It.Is<WorkerApiRequestDto>(dto => 
            dto.Name == workerDto.Name &&
            dto.Email == workerDto.Email &&
            dto.PhoneNumber == workerDto.PhoneNumber)), Times.Once);
    }

    [Fact]
    public async Task CreateWorker_WhenRepositoryFails_ShouldReturnFailureResponse()
    {
        // Arrange
        var workerDto = new WorkerApiRequestDto
        {
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockWorkerRepository.Setup(r => r.CreateAsync(It.IsAny<WorkerApiRequestDto>()))
            .ReturnsAsync(Result<Worker>.Failure("Database error", HttpStatusCode.InternalServerError));

        // Act
        var result = await _workerService.CreateWorker(workerDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Database error");
        result.Data.Should().BeNull();
    }
}
