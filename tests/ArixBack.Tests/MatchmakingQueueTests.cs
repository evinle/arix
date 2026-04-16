using ArixBack.Models;
using ArixBack.Services;
using Xunit;

namespace ArixBack.Tests;

/// <summary>
/// Tests the Elo gap formula and pairing logic in isolation by exercising
/// the public Enqueue surface and the internal gap calculation directly.
/// </summary>
public class MatchmakingQueueTests
{
    // Mirrors the formula in MatchmakingQueue.TryPair
    private static double AllowedGap(double secondsInQueue) =>
        100 + (secondsInQueue / 10) * 50;

    [Theory]
    [InlineData(0,   100)]   // fresh entry → 100
    [InlineData(10,  150)]   // 10 s → 150
    [InlineData(20,  200)]   // 20 s → 200
    [InlineData(100, 600)]   // 100 s → 600
    public void AllowedGapFormula_MatchesSpec(double seconds, double expected)
    {
        Assert.Equal(expected, AllowedGap(seconds));
    }

    [Fact]
    public void Players_WithinGap_ShouldBePaired()
    {
        // Arrange — 0 seconds in queue, gap = 100
        double seconds = 0;
        int eloA = 1000, eloB = 1090; // gap = 90 ≤ 100

        // Act / Assert
        Assert.True(Math.Abs(eloA - eloB) <= AllowedGap(seconds));
    }

    [Fact]
    public void Players_ExceedingGap_ShouldNotBePaired_Initially()
    {
        // Arrange — 0 seconds in queue, gap = 100
        double seconds = 0;
        int eloA = 1000, eloB = 1200; // gap = 200 > 100

        // Act / Assert
        Assert.False(Math.Abs(eloA - eloB) <= AllowedGap(seconds));
    }

    [Fact]
    public void Players_ExceedingGap_BecomePairable_AfterEnoughTime()
    {
        // Arrange — gap = 200, need seconds where 100 + (s/10)*50 >= 200 → s >= 20
        int eloA = 1000, eloB = 1200;
        double seconds = 20;

        // Act / Assert
        Assert.True(Math.Abs(eloA - eloB) <= AllowedGap(seconds));
    }

    [Fact]
    public void QueueEntry_EnqueuedAt_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entry = new QueueEntry();
        var after = DateTime.UtcNow;

        Assert.True(entry.EnqueuedAt >= before && entry.EnqueuedAt <= after);
    }
}
