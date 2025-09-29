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
    private readonly Mock<ILogger<ChessWar.Domain.Services.AI.AIService>> _mockLogger;

    public AICriticalRulesTests()
    {
        _mockLogger = new Mock<ILogger<ChessWar.Domain.Services.AI.AIService>>();
    }

    [Fact]
    public void AI_ShouldNeverSkipTurn_WhenPiecesAreAlive()
    {
        var session = CreateGameSessionWithAlivePieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);

        Assert.True(result, "AI must never skip a turn when pieces are alive");
    }

    [Fact]
    public void AI_ShouldSkipTurn_OnlyWhenNoPiecesAlive()
    {
        var session = CreateGameSessionWithDeadPieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);

        Assert.False(result, "AI should skip turn only when no pieces are alive");
    }

    [Fact]
    public void AI_ShouldSkipTurn_OnlyWhenNoMP()
    {
        var session = CreateGameSessionWithNoMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position>());
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position>());
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);

        Assert.False(result, "AI should skip turn only when no MP available");
    }

    [Fact]
    public void AI_ShouldAlwaysGenerateAtLeastOneAction_WhenPiecesAndMPAvailable()
    {
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);

        Assert.True(result, "AI must always generate at least one action when pieces and MP are available");
    }

    [Fact]
    public void AI_ShouldHandleAllDifficultyLevels_WithoutThrowing()
    {
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var difficulties = new[] { AIDifficultyLevel.Easy, AIDifficultyLevel.Medium, AIDifficultyLevel.Hard };

        foreach (var difficulty in difficulties)
        {
            var testSession = CreateGameSessionWithAlivePieces();
            difficultyProvider.SetDifficultyLevel(testSession.Player2, difficulty);
            var result = service.MakeAiTurn(testSession);
            Assert.True(result, $"AI should work with {difficulty} difficulty");
        }
    }

    [Fact]
    public void AI_ShouldNotMakeInvalidMoves()
    {
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);

        Assert.True(result, "AI should not make invalid moves");
        
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
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var initialTurn = session.GetCurrentTurn();
        var activePlayer = initialTurn.ActiveParticipant;
        
        var result = service.MakeAiTurn(session);

        Assert.True(result, "AI should respect turn order");
        // ИИ не должен переключать ход - это делает TurnOrchestrator
        // Проверяем, что ИИ выполнил ход успешно
        var nextTurn = session.GetCurrentTurn();
        Assert.Equal(activePlayer.Id, nextTurn.ActiveParticipant.Id);
    }

    [Fact]
    public void AI_ShouldHandleEdgeCases_Gracefully()
    {
        var session = CreateGameSessionWithEdgeCasePieces();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);
        Assert.True(result, "AI should handle edge cases gracefully");
    }

    [Fact]
    public void AI_ShouldConsumeMP_WhenMakingMoves()
    {
        var session = CreateGameSessionWithPiecesAndMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var initialMP = session.GetCurrentTurn().RemainingMP;
        var result = service.MakeAiTurn(session);

        Assert.True(result, "AI should consume MP when making moves");
        if (result)
        {
            var finalMP = session.GetCurrentTurn().RemainingMP;
            Assert.True(finalMP <= initialMP, "MP should decrease after AI move");
        }
    }

    [Fact]
    public void AI_ShouldNotExceedMP_Limits()
    {
        var session = CreateGameSessionWithLowMP();
        var evaluator = new GameStateEvaluator();
        var matrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        
        var mockTurnService = new Mock<ITurnService>();
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(1, 1), new Position(2, 2) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                      .Returns(new List<Position> { new Position(3, 3) });
        
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, CreateMockAbilityService().Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(matrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, CreateMockAbilityService().Object);
        
        var service = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, _mockLogger.Object);

        var result = service.MakeAiTurn(session);

        if (result)
        {
            var finalMP = session.GetCurrentTurn().RemainingMP;
            Assert.True(finalMP >= 0, "MP should not go below zero");
        }
    }

    private GameSession CreateGameSessionWithAlivePieces()
    {
        var player1 = new ChessWar.Domain.Entities.AI("AI", Team.Elves);
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        
        var session = new GameSession(player1, player2);
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        session.GetBoard().PlacePiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        session.GetBoard().PlacePiece(piece2);
        
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        session.StartGame();
        
        return session;
    }

    private GameSession CreateGameSessionWithDeadPieces()
    {
        var player1 = new ChessWar.Domain.Entities.AI("AI", Team.Elves);
        var player2 = new Player("Player 2", new List<Piece>());
        
        var session = new GameSession(player1, player2);
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 0; // Мёртвая фигура
        player1.AddPiece(piece1);
        session.GetBoard().PlacePiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 0; // Мёртвая фигура
        player2.AddPiece(piece2);
        session.GetBoard().PlacePiece(piece2);
        
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        session.StartGame();
        
        return session;
    }

    private GameSession CreateGameSessionWithNoMP()
    {
        var player1 = new ChessWar.Domain.Entities.AI("AI", Team.Elves);
        var player2 = new Player("Player 2", new List<Piece>());
        
        var session = new GameSession(player1, player2);
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        session.GetBoard().PlacePiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        session.GetBoard().PlacePiece(piece2);
        
        
        session.StartGame();
        
        return session;
    }

    private GameSession CreateGameSessionWithPiecesAndMP()
    {
        var player1 = new ChessWar.Domain.Entities.AI("AI", Team.Elves);
        var player2 = new Player("Player 2", new List<Piece>());
        
        var session = new GameSession(player1, player2);
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        session.GetBoard().PlacePiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        session.GetBoard().PlacePiece(piece2);
        
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        session.StartGame();
        
        return session;
    }

    private GameSession CreateGameSessionWithEdgeCasePieces()
    {
        var player1 = new ChessWar.Domain.Entities.AI("AI", Team.Elves);
        var player2 = new Player("Player 2", new List<Piece>());
        
        var session = new GameSession(player1, player2);
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        session.GetBoard().PlacePiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        session.GetBoard().PlacePiece(piece2);
        
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        
        session.StartGame();
        
        return session;
    }

    private GameSession CreateGameSessionWithLowMP()
    {
        var player1 = new ChessWar.Domain.Entities.AI("AI", Team.Elves);
        var player2 = new Player("Player 2", new List<Piece>());
        
        var session = new GameSession(player1, player2);
        
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        piece1.Owner = player1;
        piece1.HP = 10; // Живая фигура
        player1.AddPiece(piece1);
        session.GetBoard().PlacePiece(piece1);
        
        var piece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        piece2.Owner = player2;
        piece2.HP = 10; // Живая фигура
        player2.AddPiece(piece2);
        session.GetBoard().PlacePiece(piece2);
        
        player1.SetMana(1, 1);
        player2.SetMana(1, 1);
        
        session.StartGame();
        
        var turn = session.GetCurrentTurn();
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
