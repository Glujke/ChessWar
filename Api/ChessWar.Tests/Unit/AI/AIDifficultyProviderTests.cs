using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Services.AI;

namespace ChessWar.Tests.Unit.AI;

/// <summary>
/// Тесты для провайдера уровней сложности ИИ
/// </summary>
public class AIDifficultyProviderTests
{
    private readonly AIDifficultyProvider _provider;

    public AIDifficultyProviderTests()
    {
        _provider = new AIDifficultyProvider();
    }

    [Fact]
    public void GetDifficultyLevel_WithNewPlayer_ShouldReturnMedium()
    {
        var player = new Player("Test Player", new List<Piece>());

        var result = _provider.GetDifficultyLevel(player);

        Assert.Equal(AIDifficultyLevel.Medium, result);
    }

    [Fact]
    public void GetDifficultyLevel_WithSetDifficulty_ShouldReturnSetDifficulty()
    {
        var player = new Player("Test Player", new List<Piece>());
        _provider.SetDifficultyLevel(player, AIDifficultyLevel.Hard);

        var result = _provider.GetDifficultyLevel(player);

        Assert.Equal(AIDifficultyLevel.Hard, result);
    }

    [Theory]
    [InlineData(AIDifficultyLevel.Easy, 2.0)]
    [InlineData(AIDifficultyLevel.Medium, 1.0)]
    [InlineData(AIDifficultyLevel.Hard, 0.5)]
    public void GetTemperature_WithDifferentLevels_ShouldReturnCorrectValues(
        AIDifficultyLevel level, double expectedTemperature)
    {
        var result = _provider.GetTemperature(level);

        Assert.Equal(expectedTemperature, result);
    }

    [Theory]
    [InlineData(AIDifficultyLevel.Easy, 1)]
    [InlineData(AIDifficultyLevel.Medium, 3)]
    [InlineData(AIDifficultyLevel.Hard, 5)]
    public void GetPlanningDepth_WithDifferentLevels_ShouldReturnCorrectValues(
        AIDifficultyLevel level, int expectedDepth)
    {
        var result = _provider.GetPlanningDepth(level);

        Assert.Equal(expectedDepth, result);
    }

    [Theory]
    [InlineData(AIDifficultyLevel.Easy, 0.7)]
    [InlineData(AIDifficultyLevel.Medium, 0.9)]
    [InlineData(AIDifficultyLevel.Hard, 0.95)]
    public void GetDiscountFactor_WithDifferentLevels_ShouldReturnCorrectValues(
        AIDifficultyLevel level, double expectedFactor)
    {
        var result = _provider.GetDiscountFactor(level);

        Assert.Equal(expectedFactor, result);
    }

    [Fact]
    public void SetDifficultyLevel_WithValidPlayer_ShouldUpdateDifficulty()
    {
        var player = new Player("Test Player", new List<Piece>());

        _provider.SetDifficultyLevel(player, AIDifficultyLevel.Easy);
        var result = _provider.GetDifficultyLevel(player);

        Assert.Equal(AIDifficultyLevel.Easy, result);
    }

    [Fact]
    public void SetDifficultyLevel_WithMultiplePlayers_ShouldMaintainSeparateDifficulties()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());

        _provider.SetDifficultyLevel(player1, AIDifficultyLevel.Easy);
        _provider.SetDifficultyLevel(player2, AIDifficultyLevel.Hard);
        
        var result1 = _provider.GetDifficultyLevel(player1);
        var result2 = _provider.GetDifficultyLevel(player2);

        Assert.Equal(AIDifficultyLevel.Easy, result1);
        Assert.Equal(AIDifficultyLevel.Hard, result2);
    }

    [Fact]
    public void GetAllDifficulties_WithNoSetDifficulties_ShouldReturnEmpty()
    {
        var result = _provider.GetAllDifficulties();

        Assert.Empty(result);
    }

    [Fact]
    public void GetAllDifficulties_WithSetDifficulties_ShouldReturnAllDifficulties()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        _provider.SetDifficultyLevel(player1, AIDifficultyLevel.Easy);
        _provider.SetDifficultyLevel(player2, AIDifficultyLevel.Hard);

        var result = _provider.GetAllDifficulties();

        Assert.Equal(2, result.Count);
        Assert.Equal(AIDifficultyLevel.Easy, result[player1.Id]);
        Assert.Equal(AIDifficultyLevel.Hard, result[player2.Id]);
    }

    [Fact]
    public void GetTemperature_WithInvalidLevel_ShouldReturnDefault()
    {
        var invalidLevel = (AIDifficultyLevel)999;

        var result = _provider.GetTemperature(invalidLevel);

        Assert.Equal(1.0, result); // Default value
    }

    [Fact]
    public void GetPlanningDepth_WithInvalidLevel_ShouldReturnDefault()
    {
        var invalidLevel = (AIDifficultyLevel)999;

        var result = _provider.GetPlanningDepth(invalidLevel);

        Assert.Equal(1, result); // Default value
    }

    [Fact]
    public void GetDiscountFactor_WithInvalidLevel_ShouldReturnDefault()
    {
        var invalidLevel = (AIDifficultyLevel)999;

        var result = _provider.GetDiscountFactor(invalidLevel);

        Assert.Equal(0.9, result); // Default value
    }
}
