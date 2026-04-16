using ArixBack.Services;
using Xunit;

namespace ArixBack.Tests;

public class EloServiceTests
{
    private readonly EloService _sut = new();

    [Fact]
    public void Calculate_EqualElo_WinnerGains16_LoserLoses16()
    {
        // Arrange / Act
        var (newWinner, newLoser) = _sut.Calculate(1000, 1000);

        // Assert
        Assert.Equal(1016, newWinner);
        Assert.Equal(984, newLoser);
    }

    [Fact]
    public void Calculate_WinnerFavoured_WinnerGainsLessThan16()
    {
        // Arrange / Act
        var (newWinner, newLoser) = _sut.Calculate(1200, 1000);

        // Assert — expected win, so smaller gain
        Assert.True(newWinner > 1200);
        Assert.True(newWinner - 1200 < 16);
        Assert.True(newLoser < 1000);
    }

    [Fact]
    public void Calculate_UnderdogWins_WinnerGainsMoreThan16()
    {
        // Arrange / Act
        var (newWinner, newLoser) = _sut.Calculate(1000, 1200);

        // Assert — unexpected win, so larger gain
        Assert.True(newWinner - 1000 > 16);
        Assert.True(1200 - newLoser > 16);
    }

    [Fact]
    public void Calculate_EloIsNeverNegative_ForTypicalValues()
    {
        var (_, newLoser) = _sut.Calculate(2000, 800);
        Assert.True(newLoser >= 0);
    }
}
