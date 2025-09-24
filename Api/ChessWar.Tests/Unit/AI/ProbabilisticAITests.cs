using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;
using ChessWar.Infrastructure.Services.AI;
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
    private readonly Mock<ILogger<ChessWar.Infrastructure.Services.AI.ProbabilisticAIService>> _mockLogger;
    private readonly ChessWar.Infrastructure.Services.AI.ProbabilisticAIService _aiService;
    
    public ProbabilisticAITests()
    {
        _mockProbabilityMatrix = new Mock<IProbabilityMatrix>();
        _mockEvaluator = new Mock<IGameStateEvaluator>();
        _mockDifficultyProvider = new Mock<IAIDifficultyLevel>();
        _mockTurnService = new Mock<ITurnService>();
        _mockLogger = new Mock<ILogger<ProbabilisticAIService>>();
        
        // Настраиваем мок для TurnService
        _mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Returns(true);
        _mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Returns(true);
        
        _aiService = new ChessWar.Infrastructure.Services.AI.ProbabilisticAIService(
            _mockProbabilityMatrix.Object,
            _mockEvaluator.Object,
            _mockDifficultyProvider.Object,
            _mockTurnService.Object,
            Mock.Of<IAbilityService>(),
            _mockLogger.Object);
    }
    
    [Fact]
    public void MakeAiTurn_WithNoPieces_ShouldReturnFalse()
    {
        // Arrange
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Act
        var result = _aiService.MakeAiTurn(session);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void MakeAiTurn_WithPieces_ShouldReturnTrue()
    {
        // Arrange
        var session = CreateGameSession();
        var activePlayer = session.GetCurrentTurn().ActiveParticipant; // Используем активного игрока из сессии
        
        // Настраиваем моки для активного игрока
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(activePlayer))
            .Returns(AIDifficultyLevel.Medium);
        
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);
        
        _mockEvaluator.Setup(x => x.EvaluateGameState(It.IsAny<GameSession>(), It.IsAny<Player>()))
            .Returns(0.0);
        
        // Act
        var result = _aiService.MakeAiTurn(session);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(AIDifficultyLevel.Easy)]
    [InlineData(AIDifficultyLevel.Medium)]
    [InlineData(AIDifficultyLevel.Hard)]
    public void MakeAiTurn_WithDifferentDifficulties_ShouldWork(AIDifficultyLevel difficulty)
    {
        // Arrange
        var session = CreateGameSession();
        var activePlayer = session.GetCurrentTurn().ActiveParticipant; // Используем активного игрока из сессии
        
        // Настраиваем моки для активного игрока
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(activePlayer))
            .Returns(difficulty);
        
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);
        
        _mockEvaluator.Setup(x => x.EvaluateGameState(It.IsAny<GameSession>(), It.IsAny<Player>()))
            .Returns(0.0);
        
        // Act
        var result = _aiService.MakeAiTurn(session);
        
        // Assert
        Assert.True(result);
    }
    
    private GameSession CreateGameSession()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        // Добавляем фигуры игрокам
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        
        // Устанавливаем HP для фигур (иначе они не будут живыми)
        piece1.HP = 10; // HP пешки по умолчанию
        piece2.HP = 10;
        
        piece1.Owner = player1;
        piece2.Owner = player2;
        
        player1.AddPiece(piece1);
        player2.AddPiece(piece2);
        
        // Устанавливаем MP для игроков
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame(); // Запускаем игру
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }
}
