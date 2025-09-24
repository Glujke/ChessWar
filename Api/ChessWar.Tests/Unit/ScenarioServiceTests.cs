using ChessWar.Application.Services.Tutorial;
using ChessWar.Domain.Enums;

namespace ChessWar.Tests.Unit;

public class ScenarioServiceTests
{
    private readonly ScenarioTutorialService _service;

    public ScenarioServiceTests()
    {
        _service = new ScenarioTutorialService();
    }

    [Fact]
    public async Task CreateBattleScenarioAsync_ShouldCreateBattleScenario()
    {
        // Arrange
        var difficulty = AiDifficulty.Easy;

        // Act
        var result = await _service.CreateBattleScenarioAsync(difficulty);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ScenarioType.Battle, result.Type);
    }

    [Fact]
    public async Task CreateBossScenarioAsync_ShouldCreateBossScenario()
    {
        // Act
        var result = await _service.CreateBossScenarioAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ScenarioType.Boss, result.Type);
    }

    [Fact]
    public async Task GetNextScenarioAsync_ShouldReturnNextScenario()
    {
        // Arrange
        var currentScenario = ScenarioType.Battle;

        // Act
        var result = await _service.GetNextScenarioAsync(currentScenario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ScenarioType.Boss, result.Type);
    }
}
