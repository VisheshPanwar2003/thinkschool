using QuotesApi.Services;

namespace QuotesApi.Tests;

public class ClockTests
{
    [Fact]
    public void Fake_clock_returns_the_time_we_set()
    {
        var expected = new DateTimeOffset(
            2026, 8, 11, 10, 30, 0,
            TimeSpan.Zero);

        var clock = new FakeClock
        {
            UtcNow = expected
        };

        Assert.Equal(expected, clock.UtcNow);
    }
}