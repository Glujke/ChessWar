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
        var difficulty = AiDifficulty.Easy;

        var result = await _service.CreateBattleScenarioAsync(difficulty);

        Assert.NotNull(result);
        Assert.Equal(ScenarioType.Battle, result.Type);
    }

    [Fact]
    public async Task CreateBossScenarioAsync_ShouldCreateBossScenario()
    {
        var result = await _service.CreateBossScenarioAsync();

        Assert.NotNull(result);
        Assert.Equal(ScenarioType.Boss, result.Type);
    }

    [Fact]
    public async Task GetNextScenarioAsync_ShouldReturnNextScenario()
    {
        var currentScenario = ScenarioType.Battle;

        var result = await _service.GetNextScenarioAsync(currentScenario);

        Assert.NotNull(result);
        Assert.Equal(ScenarioType.Boss, result.Type);
    }
}
