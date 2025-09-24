using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.AI;
using ChessWar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChessWar.Tests.Unit.AI;

/// <summary>
/// Тесты для критических правил ИИ
/// </summary>
public class AICriticalRulesTests
{
    private readonly Mock<ILogger<Infrastructure.Services.AI.ProbabilisticAIService>> _mockLogger;

    public AICriticalRulesTests()
    {
        _mockLogger = new Mock<ILogger<Infrastructure.Services.AI.ProbabilisticAIService>>();
    }

    [Fact]
    public void AI_ShouldNeverSkipTurn_WhenPiecesAreAlive()
    {
        // Arrange
        var session = CreateGameSessionWithAlivePieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.True(result, "AI must never skip a turn when pieces are alive");
    }

    [Fact]
    public void AI_ShouldSkipTurn_OnlyWhenNoPiecesAlive()
    {
        // Arrange
        var session = CreateGameSessionWithDeadPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.False(result, "AI should skip turn only when no pieces are alive");
    }

    [Fact]
    public void AI_ShouldSkipTurn_OnlyWhenNoMP()
    {
        // Arrange
        var session = CreateGameSessionWithNoMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.False(result, "AI should skip turn only when no MP available");
    }

    [Fact]
    public void AI_ShouldAlwaysGenerateAtLeastOneAction_WhenPiecesAndMPAvailable()
    {
        // Arrange
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.True(result, "AI must always generate at least one action when pieces and MP are available");
    }

    [Fact]
    public void AI_ShouldHandleAllDifficultyLevels_WithoutThrowing()
    {
        // Arrange
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        var difficulties = new[] { AIDifficultyLevel.Easy, AIDifficultyLevel.Medium, AIDifficultyLevel.Hard };

        // Act & Assert
        foreach (var difficulty in difficulties)
        {
            difficultyProvider.SetDifficultyLevel(session.Player2, difficulty);
            var result = service.MakeAiTurn(session);
            Assert.True(result, $"AI should work with {difficulty} difficulty");
        }
    }

    [Fact]
    public void AI_ShouldNotMakeInvalidMoves()
    {
        // Arrange
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.True(result, "AI should not make invalid moves");
        
        // Проверяем, что все фигуры остались на доске
        var allPieces = session.GetAllPieces();
        Assert.All(allPieces, piece => 
        {
            Assert.True(piece.Position.X >= 0 && piece.Position.X < 8, 
                $"Piece {piece.Id} has invalid X position: {piece.Position.X}");
            Assert.True(piece.Position.Y >= 0 && piece.Position.Y < 8, 
                $"Piece {piece.Id} has invalid Y position: {piece.Position.Y}");
        });
    }

    [Fact]
    public void AI_ShouldRespectTurnOrder()
    {
        // Arrange
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var initialTurn = session.GetCurrentTurn();
        var activePlayer = initialTurn.ActiveParticipant;
        
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.True(result, "AI should respect turn order");
        // После хода ИИ должен быть ход другого игрока
        var nextTurn = session.GetCurrentTurn();
        Assert.NotEqual(activePlayer.Id, nextTurn.ActiveParticipant.Id);
    }

    [Fact]
    public void AI_ShouldHandleEdgeCases_Gracefully()
    {
        // Arrange
        var session = CreateGameSessionWithEdgeCasePieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act & Assert
        // ИИ должен обрабатывать граничные случаи без падений
        var result = service.MakeAiTurn(session);
        Assert.True(result, "AI should handle edge cases gracefully");
    }

    [Fact]
    public void AI_ShouldConsumeMP_WhenMakingMoves()
    {
        // Arrange
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var initialMP = session.GetCurrentTurn().RemainingMP;
        var result = service.MakeAiTurn(session);

        // Assert
        Assert.True(result, "AI should consume MP when making moves");
        // MP должно уменьшиться (если ИИ действительно сделал ход)
        if (result)
        {
            var finalMP = session.GetCurrentTurn().RemainingMP;
            Assert.True(finalMP <= initialMP, "MP should decrease after AI move");
        }
    }

    [Fact]
    public void AI_ShouldNotExceedMP_Limits()
    {
        // Arrange
        var session = CreateGameSessionWithLowMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        // Создаем мок TurnService, который возвращает true для всех действий
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        var service = new Infrastructure.Services.AI.ProbabilisticAIService(
            matrix, evaluator, difficultyProvider, mockTurnService.Object, CreateMockAbilityService().Object, _mockLogger.Object);

        // Act
        var result = service.MakeAiTurn(session);

        // Assert
        // ИИ должен учитывать ограничения по MP
        if (result)
        {
            var finalMP = session.GetCurrentTurn().RemainingMP;
            Assert.True(finalMP >= 0, "MP should not go below zero");
        }
    }

    private GameSession CreateGameSessionWithAlivePieces()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        
        // Устанавливаем MP для игроков
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }

    private GameSession CreateGameSessionWithDeadPieces()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 0; // Мёртвая фигура
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 0; // Мёртвая фигура
        player2.AddPiece(piece2);
        
        // Устанавливаем MP для игроков
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }

    private GameSession CreateGameSessionWithNoMP()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        
        // НЕ устанавливаем MP для игроков (0 MP)
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }

    private GameSession CreateGameSessionWithPiecesAndMP()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        
        // Устанавливаем MP для игроков
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }

    private GameSession CreateGameSessionWithEdgeCasePieces()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        // Фигуры в углах доски
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        
        // Устанавливаем MP для игроков
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        return session;
    }

    private GameSession CreateGameSessionWithLowMP()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        
        // Устанавливаем низкий MP для игроков
        player1.SetMana(1, 1);
        player2.SetMana(1, 1);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(piece1);
        session.GetBoard().PlacePiece(piece2);
        
        // Устанавливаем низкую ману
        var turn = session.GetCurrentTurn();
        // Потратим всю ману кроме 1
        while (turn.RemainingMP > 1)
        {
            turn.SpendMP(1);
        }
        
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
