using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Services.AI;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChessWar.Tests.Unit.AI;

/// <summary>
/// Тесты для вероятностного ИИ
/// </summary>
public class ProbabilisticAITests
{
    private readonly Mock<IProbabilityMatrix> _mockProbabilityMatrix;
    private readonly Mock<IGameStateEvaluator> _mockEvaluator;
    private readonly Mock<IAIDifficultyLevel> _mockDifficultyProvider;
    private readonly Mock<ITurnService> _mockTurnService;
    private readonly Mock<ILogger<ChessWar.Domain.Services.AI.AIService>> _mockLogger;
    private readonly ChessWar.Domain.Services.AI.AIService _aiService;
    
    public ProbabilisticAITests()
    {
        _mockProbabilityMatrix = new Mock<IProbabilityMatrix>();
        _mockEvaluator = new Mock<IGameStateEvaluator>();
        _mockDifficultyProvider = new Mock<IAIDifficultyLevel>();
        _mockTurnService = new Mock<ITurnService>();
        _mockLogger = new Mock<ILogger<ChessWar.Domain.Services.AI.AIService>>();
        
        _mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Returns(true);
        _mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Returns(true);
        _mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(0, 5), new Position(1, 5) });
        _mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(1, 5) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(_mockTurnService.Object, Mock.Of<IAbilityService>(), Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(_mockProbabilityMatrix.Object, _mockEvaluator.Object, _mockDifficultyProvider.Object);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(_mockTurnService.Object, Mock.Of<IAbilityService>());
        
        _aiService = new ChessWar.Domain.Services.AI.AIService(
            actionGenerator,
            actionSelector,
            actionExecutor,
            _mockLogger.Object);
    }
    
    [Fact]
    public void MakeAiTurn_WithNoPieces_ShouldReturnFalse()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        var result = _aiService.MakeAiTurn(session);
        
        Assert.False(result);
    }
    
    [Fact]
    public void MakeAiTurn_WithPieces_ShouldReturnTrue()
    {
        var session = CreateGameSession();
        var activePlayer = session.GetCurrentTurn().ActiveParticipant; // Используем активного игрока из сессии
        
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(activePlayer))
            .Returns(AIDifficultyLevel.Medium);
        
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);
        
        _mockEvaluator.Setup(x => x.EvaluateGameState(It.IsAny<GameSession>(), It.IsAny<Player>()))
            .Returns(0.0);
        
        var result = _aiService.MakeAiTurn(session);
        
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(AIDifficultyLevel.Easy)]
    [InlineData(AIDifficultyLevel.Medium)]
    [InlineData(AIDifficultyLevel.Hard)]
    public void MakeAiTurn_WithDifferentDifficulties_ShouldWork(AIDifficultyLevel difficulty)
    {
        var session = CreateGameSession();
        var activePlayer = session.GetCurrentTurn().ActiveParticipant; // Используем активного игрока из сессии
        
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(activePlayer))
            .Returns(difficulty);
        
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);
        
        _mockEvaluator.Setup(x => x.EvaluateGameState(It.IsAny<GameSession>(), It.IsAny<Player>()))
            .Returns(0.0);
        
        var result = _aiService.MakeAiTurn(session);
        
        Assert.True(result);
    }
    
    private GameSession CreateGameSession()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        
        piece1.HP = 10; // HP пешки по умолчанию
        piece2.HP = 10;
        
        piece1.Owner = player1;
        piece2.Owner = player2;
        
        player1.AddPiece(piece1);
        player2.AddPiece(piece2);
        
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame(); // Запускаем игру
        
        session.EndCurrentTurn();
        
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }
}
