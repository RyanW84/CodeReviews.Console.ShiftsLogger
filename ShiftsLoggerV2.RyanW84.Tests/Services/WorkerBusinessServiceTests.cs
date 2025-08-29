using FluentAssertions;
using Moq;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;
using System.Net;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Services;

public class WorkerBusinessServiceTests
{
    private readonly Mock<IWorkerRepository> _mockWorkerRepository;
    private readonly WorkerBusinessService _workerBusinessService;

    public WorkerBusinessServiceTests()
    {
        _mockWorkerRepository = new Mock<IWorkerRepository>();
        _workerBusinessService = new WorkerBusinessService(_mockWorkerRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidWorker_ShouldCreateWorker()
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
            Name = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "123-456-7890"
        };

        _mockWorkerRepository.Setup(r => r.CreateAsync(It.IsAny<WorkerApiRequestDto>()))
            .ReturnsAsync(Result<Worker>.Success(createdWorker, "Worker created successfully"));

        // Act
        var result = await _workerBusinessService.CreateAsync(workerDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("John Doe");
        result.Data.Email.Should().Be("john@example.com");
        result.Data.PhoneNumber.Should().Be("123-456-7890");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithInvalidName_ShouldReturnValidationError(string? invalidName)
    {
        // Arrange
        var workerDto = new WorkerApiRequestDto
        {
            Name = invalidName!,
            Email = "john@example.com"
        };

        // Act
        var result = await _workerBusinessService.CreateAsync(workerDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("name");
    }

    [Fact]
    public async Task CreateAsync_WithTooLongName_ShouldReturnValidationError()
    {
        // Arrange
        var longName = new string('A', 101); // 101 characters, exceeds max of 100
        var workerDto = new WorkerApiRequestDto
        {
            Name = longName,
            Email = "john@example.com"
        };

        // Act
        var result = await _workerBusinessService.CreateAsync(workerDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("100 characters");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    public async Task CreateAsync_WithInvalidEmail_ShouldReturnValidationError(string invalidEmail)
    {
        // Arrange
        var workerDto = new WorkerApiRequestDto
        {
            Name = "John Doe",
            Email = invalidEmail
        };

        // Act
        var result = await _workerBusinessService.CreateAsync(workerDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("Email");
    }

    [Fact]
    public async Task CreateAsync_WithTooLongEmail_ShouldReturnValidationError()
    {
        // Arrange - Email that exceeds 254 character limit
        var longEmail = new string('a', 250) + "@test.com"; // Total: 259 characters, exceeds 254 limit
        var workerDto = new WorkerApiRequestDto
        {
            Name = "John Doe",
            Email = longEmail
        };

        // Act
        var result = await _workerBusinessService.CreateAsync(workerDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("254 characters");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123-45")]
    [InlineData("abc-def-ghij")]
    public async Task CreateAsync_WithInvalidPhoneNumber_ShouldReturnValidationError(string invalidPhone)
    {
        // Arrange
        var workerDto = new WorkerApiRequestDto
        {
            Name = "John Doe",
            PhoneNumber = invalidPhone
        };

        // Act
        var result = await _workerBusinessService.CreateAsync(workerDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("Phone number");
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("123-456-7890")]
    [InlineData("(123) 456-7890")]
    [InlineData("+1-123-456-7890")]
    public async Task CreateAsync_WithValidPhoneNumber_ShouldPass(string validPhone)
    {
        // Arrange
        var workerDto = new WorkerApiRequestDto
        {
            Name = "John Doe",
            PhoneNumber = validPhone
        };

        var createdWorker = new Worker
        {
            WorkerId = 1,
            Name = "John Doe",
            PhoneNumber = validPhone
        };

        _mockWorkerRepository.Setup(r => r.CreateAsync(It.IsAny<WorkerApiRequestDto>()))
            .ReturnsAsync(Result<Worker>.Success(createdWorker, "Worker created successfully"));

        // Act
        var result = await _workerBusinessService.CreateAsync(workerDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.PhoneNumber.Should().Be(validPhone);
    }

    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("test.email@example.org")]
    [InlineData("user+tag@domain.co.uk")]
    public async Task CreateAsync_WithValidEmail_ShouldPass(string validEmail)
    {
        // Arrange
        var workerDto = new WorkerApiRequestDto
        {
            Name = "John Doe",
            Email = validEmail
        };

        var createdWorker = new Worker
        {
            WorkerId = 1,
            Name = "John Doe",
            Email = validEmail
        };

        _mockWorkerRepository.Setup(r => r.CreateAsync(It.IsAny<WorkerApiRequestDto>()))
            .ReturnsAsync(Result<Worker>.Success(createdWorker, "Worker created successfully"));

        // Act
        var result = await _workerBusinessService.CreateAsync(workerDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Email.Should().Be(validEmail);
    }

    [Fact]
    public async Task CreateAsync_WithNullOptionalFields_ShouldPass()
    {
        // Arrange
        var workerDto = new WorkerApiRequestDto
        {
            Name = "John Doe",
            Email = "john@example.com", // At least one contact method required
            PhoneNumber = null
        };

        var createdWorker = new Worker
        {
            WorkerId = 1,
            Name = "John Doe",
            Email = "john@example.com",
            PhoneNumber = null
        };

        _mockWorkerRepository.Setup(r => r.CreateAsync(It.IsAny<WorkerApiRequestDto>()))
            .ReturnsAsync(Result<Worker>.Success(createdWorker, "Worker created successfully"));

        // Act
        var result = await _workerBusinessService.CreateAsync(workerDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("John Doe");
        result.Data.Email.Should().Be("john@example.com");
        result.Data.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepository()
    {
        // Arrange
        var filterOptions = new WorkerFilterOptions();
        var workers = new List<Worker>
        {
            new() { WorkerId = 1, Name = "John Doe" }
        };

        _mockWorkerRepository.Setup(r => r.GetAllAsync(filterOptions))
            .ReturnsAsync(Result<List<Worker>>.Success(workers, "Success"));

        // Act
        var result = await _workerBusinessService.GetAllAsync(filterOptions);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        _mockWorkerRepository.Verify(r => r.GetAllAsync(filterOptions), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepository()
    {
        // Arrange
        const int workerId = 1;
        var worker = new Worker { WorkerId = workerId, Name = "John Doe" };
        
        _mockWorkerRepository.Setup(r => r.GetByIdAsync(workerId))
            .ReturnsAsync(Result<Worker>.Success(worker, "Found"));

        // Act
        var result = await _workerBusinessService.GetByIdAsync(workerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.WorkerId.Should().Be(workerId);
        _mockWorkerRepository.Verify(r => r.GetByIdAsync(workerId), Times.Once);
    }
}
