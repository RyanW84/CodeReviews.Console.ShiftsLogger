using FluentAssertions;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Tests.Fixtures;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace ShiftsLoggerV2.RyanW84.Tests.Debug;

public class DebugTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public DebugTest(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task DebugWorkerValidation()
    {
        // Arrange
        var invalidWorker = new WorkerApiRequestDto
        {
            Name = "", // Invalid empty name
            Email = "john@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workers", invalidWorker);
        var contentString = await response.Content.ReadAsStringAsync();
        
        _output.WriteLine($"Status Code: {response.StatusCode}");
        _output.WriteLine($"Response Content: {contentString}");

        var content = await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>();
        
        _output.WriteLine($"RequestFailed: {content?.RequestFailed}");
        _output.WriteLine($"Message: {content?.Message}");
        _output.WriteLine($"Data: {content?.Data?.WorkerId}");
        
        // Just to see what's happening
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
