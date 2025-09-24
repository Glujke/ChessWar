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
        // Arrange
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var playerId = "player123";

        // Настраиваем мок репозитория
        mockRepository.Setup(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await tutorialService.StartTutorialAsync(playerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TutorialStage.Introduction, result.CurrentStage);
        Assert.Equal(0, result.Progress);
        Assert.False(result.IsCompleted);
        Assert.True(result.ShowHints);
        
        // Проверяем, что репозиторий был вызван
        mockRepository.Verify(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdvanceToNextStageAsync_ShouldUpdateStage_AndProgress()
    {
        // Arrange
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var sessionId = Guid.NewGuid();
        var session = new TutorialSession(new Player("player123", new List<Piece>()));
        
        // Настраиваем мок репозитория
        mockRepository.Setup(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        mockRepository.Setup(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await tutorialService.AdvanceToNextStageAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TutorialStage.Battle1, result.CurrentStage); // Должен перейти к следующему этапу
        Assert.Equal(25, result.Progress); // Прогресс должен обновиться
        
        // Проверяем, что репозиторий был вызван
        mockRepository.Verify(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        mockRepository.Verify(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTutorialProgressAsync_ShouldReturnCurrentProgress()
    {
        // Arrange
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var sessionId = Guid.NewGuid();
        var session = new TutorialSession(new Player("player123", new List<Piece>()));
        session.AdvanceToNextStage(); // Переходим к Battle1 (25% прогресс)
        
        // Настраиваем мок репозитория
        mockRepository.Setup(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await tutorialService.GetTutorialProgressAsync(sessionId);

        // Assert
        Assert.Equal(25, result); // Ожидаем 25% для Battle1
        
        // Проверяем, что репозиторий был вызван
        mockRepository.Verify(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentHintsAsync_ShouldReturnHintsForCurrentStage()
    {
        // Arrange
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var sessionId = Guid.NewGuid();
        var session = new TutorialSession(new Player("player123", new List<Piece>()));
        var expectedHints = new List<string> { "Добро пожаловать в обучение!", "Изучите основы игры в шахматы" };
        
        // Настраиваем моки
        mockRepository.Setup(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        mockHintService.Setup(x => x.GetHintsForStageAsync(TutorialStage.Introduction, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHints);

        // Act
        var result = await tutorialService.GetCurrentHintsAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedHints.Count, result.Count);
        Assert.Contains("Добро пожаловать в обучение!", result);
        Assert.Contains("Изучите основы игры в шахматы", result);
        
        // Проверяем, что сервисы были вызваны
        mockRepository.Verify(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        mockHintService.Verify(x => x.GetHintsForStageAsync(TutorialStage.Introduction, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteTutorialAsync_ShouldMarkTutorialAsCompleted()
    {
        // Arrange
        var mockRepository = new Mock<IGameModeRepository>();
        var mockHintService = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var tutorialService = new TutorialService(mockRepository.Object, mockHintService.Object, mockNotificationService.Object);
        var sessionId = Guid.NewGuid();
        var session = new TutorialSession(new Player("player123", new List<Piece>()));
        
        // Переводим сессию в состояние Boss (последний этап перед Completed)
        session.AdvanceToNextStage(); // Introduction -> Battle1
        session.AdvanceToNextStage(); // Battle1 -> Battle2  
        session.AdvanceToNextStage(); // Battle2 -> Boss
        
        // Настраиваем мок репозитория
        mockRepository.Setup(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        mockRepository.Setup(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await tutorialService.CompleteTutorialAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsCompleted); // Должно быть завершено
        Assert.Equal(TutorialStage.Completed, result.CurrentStage); // Должен быть Completed
        Assert.Equal(100, result.Progress); // Прогресс должен быть 100%
        
        // Проверяем, что репозиторий был вызван
        mockRepository.Verify(x => x.GetModeByIdAsync<ITutorialMode>(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        mockRepository.Verify(x => x.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
