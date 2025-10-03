using Moq;
using ChessWar.Application.Interfaces.Tutorial;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Services.Tutorial;
using ChessWar.Domain.Interfaces.Tutorial;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Entities;

namespace ChessWar.Tests.Unit;

public class TutorialServiceTests
{
    [Fact]
    public async Task StartTutorialAsync_ShouldCreateTutorialSession_WithIntroductionStage()
    {
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var playerId = "player123";

        mockRepository.Setup(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await tutorialService.StartTutorialAsync(playerId);

        Assert.NotNull(result);
        Assert.Equal(TutorialStage.Introduction, result.CurrentStage);
        Assert.Equal(0, result.Progress);
        Assert.False(result.IsCompleted);
        Assert.True(result.ShowHints);

        mockRepository.Verify(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdvanceToNextStageAsync_ShouldUpdateStage_AndProgress()
    {
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var sessionId = Guid.NewGuid();
        var session = new TutorialSession(new Player("player123", new List<Piece>()));

        mockRepository.Setup(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        mockRepository.Setup(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await tutorialService.AdvanceToNextStageAsync(sessionId);

        Assert.NotNull(result);
        Assert.Equal(TutorialStage.Battle1, result.CurrentStage); // Должен перейти к следующему этапу
        Assert.Equal(25, result.Progress); // Прогресс должен обновиться

        mockRepository.Verify(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        mockRepository.Verify(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTutorialProgressAsync_ShouldReturnCurrentProgress()
    {
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var sessionId = Guid.NewGuid();
        var session = new TutorialSession(new Player("player123", new List<Piece>()));
        session.AdvanceToNextStage(); // Переходим к Battle1 (25% прогресс)

        mockRepository.Setup(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await tutorialService.GetTutorialProgressAsync(sessionId);

        Assert.Equal(25, result); // Ожидаем 25% для Battle1

        mockRepository.Verify(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentHintsAsync_ShouldReturnHintsForCurrentStage()
    {
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var sessionId = Guid.NewGuid();
        var session = new TutorialSession(new Player("player123", new List<Piece>()));
        var expectedHints = new List<string> { "Добро пожаловать в обучение!", "Изучите основы игры в шахматы" };

        mockRepository.Setup(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        mockHintService.Setup(x => x.GetHintsForStageAsync(TutorialStage.Introduction, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHints);

        var result = await tutorialService.GetCurrentHintsAsync(sessionId);

        Assert.NotNull(result);
        Assert.Equal(expectedHints.Count, result.Count);
        Assert.Contains("Добро пожаловать в обучение!", result);
        Assert.Contains("Изучите основы игры в шахматы", result);

        mockRepository.Verify(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        mockHintService.Verify(x => x.GetHintsForStageAsync(TutorialStage.Introduction, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteTutorialAsync_ShouldMarkTutorialAsCompleted()
    {
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var sessionId = Guid.NewGuid();
        var session = new TutorialSession(new Player("player123", new List<Piece>()));

        session.AdvanceToNextStage(); // Introduction -> Battle1
        session.AdvanceToNextStage(); // Battle1 -> Battle2  
        session.AdvanceToNextStage(); // Battle2 -> Boss

        mockRepository.Setup(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        mockRepository.Setup(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await tutorialService.CompleteTutorialAsync(sessionId);

        Assert.NotNull(result);
        Assert.True(result.IsCompleted); // Должно быть завершено
        Assert.Equal(TutorialStage.Completed, result.CurrentStage); // Должен быть Completed
        Assert.Equal(100, result.Progress); // Прогресс должен быть 100%

        mockRepository.Verify(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        mockRepository.Verify(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
