using ChessWar.Application.Services.Tutorial;
using ChessWar.Domain.Enums;

namespace ChessWar.Tests.Unit;

public class TutorialHintServiceTests
{
    [Fact]
    public async Task GetHintsForStageAsync_ShouldReturnHintsForIntroduction()
    {
        var hintService = new TutorialHintService();
        var stage = TutorialStage.Introduction;

        var result = await hintService.GetHintsForStageAsync(stage);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Добро пожаловать в обучение!", result);
        Assert.Contains("Изучите основы игры в шахматы", result);
    }

    [Fact]
    public async Task GetHintsForStageAsync_ShouldReturnHintsForBattle1()
    {
        var hintService = new TutorialHintService();
        var stage = TutorialStage.Battle1;

        var result = await hintService.GetHintsForStageAsync(stage);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Ваш первый бой с ИИ!", result);
        Assert.Contains("Попробуйте атаковать вражеские фигуры", result);
    }

    [Fact]
    public async Task GetContextualHintsAsync_ShouldReturnContextualHints()
    {
        var hintService = new TutorialHintService();
        var sessionId = Guid.NewGuid();
        var gameState = "player_turn";

        var result = await hintService.GetContextualHintsAsync(sessionId, gameState);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Следуйте подсказкам на экране", result);
        Assert.Contains("Изучайте интерфейс игры", result);
    }
}
