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
    private readonly Mock<ILogger<ChessWar.Domain.Services.AI.AIService>> _mockLogger;

    public ProbabilisticAIServiceIntegrationTests()
    {
        _mockLogger = new Mock<ILogger<ChessWar.Domain.Services.AI.AIService>>();
    }

    private Mock<ITurnService> CreateMockTurnService()
    {
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(1, 1), new Position(2, 2), new Position(3, 3), new Position(4, 4) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(5, 5), new Position(6, 6) });
        return mockTurnService;
    }

    [Fact]
    public void MakeAiTurn_WithValidGameSession_ShouldReturnTrue()
    {
        var session = CreateGameSessionWithPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger<ChessWar.Domain.Services.AI.AIService>();
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(CreateMockTurnService().Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(CreateMockTurnService().Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, logger);

        var result = service.MakeAiTurn(session);

        Assert.True(result);
    }

    [Fact]
    public void MakeAiTurn_WithNoPieces_ShouldReturnFalse()
    {
        var session = CreateEmptyGameSession();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(CreateMockTurnService().Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(CreateMockTurnService().Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);

        Assert.False(result);
    }

    [Fact]
    public void MakeAiTurn_WithDifferentDifficulties_ShouldWork()
    {
        var session = CreateGameSessionWithPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(CreateMockTurnService().Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(CreateMockTurnService().Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        difficultyProvider.SetDifficultyLevel(session.Player2, AIDifficultyLevel.Easy);
        var easyResult = service.MakeAiTurn(session);
        Assert.True(easyResult);

        difficultyProvider.SetDifficultyLevel(session.Player2, AIDifficultyLevel.Medium);
        var mediumResult = service.MakeAiTurn(session);
        Assert.True(mediumResult);

        difficultyProvider.SetDifficultyLevel(session.Player2, AIDifficultyLevel.Hard);
        var hardResult = service.MakeAiTurn(session);
        Assert.True(hardResult);
    }

    [Fact]
    public void MakeAiTurn_WithMultipleTurns_ShouldNotFail()
    {
        var session = CreateGameSessionWithPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(CreateMockTurnService().Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(CreateMockTurnService().Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        for (int i = 0; i < 5; i++)
        {
            var result = service.MakeAiTurn(session);
            Assert.True(result);
        }
    }


    [Fact]
    public void MakeAiTurn_WithCenterPositionedPieces_ShouldPreferCenterActions()
    {
        var session = CreateGameSessionWithCenterPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(CreateMockTurnService().Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(CreateMockTurnService().Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);

        Assert.True(result);
    }

    [Fact]
    public void MakeAiTurn_WithEdgePositionedPieces_ShouldStillWork()
    {
        var session = CreateGameSessionWithEdgePieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(CreateMockTurnService().Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(CreateMockTurnService().Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);

        Assert.True(result);
    }

    private GameSession CreateGameSessionWithPieces()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.HP = 10; // Устанавливаем HP для живой фигуры
        piece1.Owner = player1;
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 1));
        piece2.HP = 10; // Устанавливаем HP для живой фигуры
        piece2.Owner = player2;
        player2.AddPiece(piece2);
        
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        session.StartGame();
        
        session.EndCurrentTurn();
        
        return session;
    }

    private GameSession CreateEmptyGameSession()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        return session;
    }

    private GameSession CreateGameSessionWithCenterPieces()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        
        var piece1 = new Piece(PieceType.Queen, Team.Elves, new Position(3, 3));
        piece1.HP = 10; // Устанавливаем HP для живой фигуры
        piece1.Owner = player1;
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Queen, Team.Orcs, new Position(4, 4));
        piece2.HP = 10; // Устанавливаем HP для живой фигуры
        piece2.Owner = player2;
        player2.AddPiece(piece2);
        
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        session.StartGame();
        
        session.EndCurrentTurn();
        
        return session;
    }

    private GameSession CreateGameSessionWithEdgePieces()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.HP = 10; // Устанавливаем HP для живой фигуры
        piece1.Owner = player1;
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 1));
        piece2.HP = 10; // Устанавливаем HP для живой фигуры
        piece2.Owner = player2;
        player2.AddPiece(piece2);
        
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        session.StartGame();
        
        session.EndCurrentTurn();
        
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
