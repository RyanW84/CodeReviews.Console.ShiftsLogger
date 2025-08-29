using FluentAssertions;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Integration;

public class SimpleIntegrationTest
{
    [Fact]
    public void SimpleTest_ShouldPass()
    {
        // This is just to test if integration tests are being discovered
        var result = true;
        result.Should().BeTrue();
    }
}
