using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.AI;
using ChessWar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChessWar.Tests.Integration.AI;

/// <summary>
/// Интеграционные тесты для вероятностного ИИ
/// </summary>
public class ProbabilisticAIServiceIntegrationTests
{
    private readonly Mock<ILogger<Infrastructure.Services.AI.ProbabilisticAIService>> _mockLogger;

    public ProbabilisticAIServiceIntegrationTests()
    {
        _mockLogger = new Mock<ILogger<Infrastructure.Services.AI.ProbabilisticAIService>>();
    }

    private Mock<ITurnService> CreateMockTurnService()
    {
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Returns(true);
        return mockTurnService;
    }

    [Fact]
    public void MakeAiTurn_WithValidGameSession_ShouldReturnTrue()
    {
        // Arrange
        var session = CreateGameSessionWithPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем реальный логгер для отладки
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger<Infrastructure.Services.AI.ProbabilisticAIService>();
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, CreateMockTurnService().Object, CreateMockAbilityService().Object, logger);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MakeAiTurn_WithNoPieces_ShouldReturnFalse()
    {
        // Arrange
        var session = CreateEmptyGameSession();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, CreateMockTurnService().Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MakeAiTurn_WithDifferentDifficulties_ShouldWork()
    {
        // Arrange
        var session = CreateGameSessionWithPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, CreateMockTurnService().Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act & Assert
        // Легкий уровень
        difficultyProvider.SetDifficultyLevel(session.Player2, AIDifficultyLevel.Easy);
        var easyResult = service.MakeAiTurn(session);
        Assert.True(easyResult);

        // Средний уровень
        difficultyProvider.SetDifficultyLevel(session.Player2, AIDifficultyLevel.Medium);
        var mediumResult = service.MakeAiTurn(session);
        Assert.True(mediumResult);

        // Сложный уровень
        difficultyProvider.SetDifficultyLevel(session.Player2, AIDifficultyLevel.Hard);
        var hardResult = service.MakeAiTurn(session);
        Assert.True(hardResult);
    }

    [Fact]
    public void MakeAiTurn_WithMultipleTurns_ShouldNotFail()
    {
        // Arrange
        var session = CreateGameSessionWithPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, CreateMockTurnService().Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            var result = service.MakeAiTurn(session);
            Assert.True(result);
        }
    }


    [Fact]
    public void MakeAiTurn_WithCenterPositionedPieces_ShouldPreferCenterActions()
    {
        // Arrange
        var session = CreateGameSessionWithCenterPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, CreateMockTurnService().Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MakeAiTurn_WithEdgePositionedPieces_ShouldStillWork()
    {
        // Arrange
        var session = CreateGameSessionWithEdgePieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, CreateMockTurnService().Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.True(result);
    }

    private GameSession CreateGameSessionWithPieces()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        // Добавляем фигуры игрокам
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.HP = 10; // Устанавливаем HP для живой фигуры
        piece1.Owner = player1;
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.HP = 10; // Устанавливаем HP для живой фигуры
        piece2.Owner = player2;
        player2.AddPiece(piece2);
        
        // Устанавливаем ману для игроков
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }

    private GameSession CreateEmptyGameSession()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        // Устанавливаем ману для игроков
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        return session;
    }

    private GameSession CreateGameSessionWithCenterPieces()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        // Фигуры в центре доски
        var piece1 = new Piece(PieceType.Queen, Team.Elves, new Position(3, 3));
        piece1.HP = 10; // Устанавливаем HP для живой фигуры
        piece1.Owner = player1;
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Queen, Team.Orcs, new Position(4, 4));
        piece2.HP = 10; // Устанавливаем HP для живой фигуры
        piece2.Owner = player2;
        player2.AddPiece(piece2);
        
        // Устанавливаем ману для игроков
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }

    private GameSession CreateGameSessionWithEdgePieces()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        // Фигуры на краю доски
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.HP = 10; // Устанавливаем HP для живой фигуры
        piece1.Owner = player1;
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.HP = 10; // Устанавливаем HP для живой фигуры
        piece2.Owner = player2;
        player2.AddPiece(piece2);
        
        // Устанавливаем ману для игроков
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }

    private Mock<IAbilityService> CreateMockAbilityService()
    {
        var mockAbilityService = new Mock<IAbilityService>();
        mockAbilityService.Setup(x => x.UseAbility(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);
        return mockAbilityService;
    }
}
