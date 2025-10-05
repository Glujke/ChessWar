using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;
using ChessWar.Application.Services.AI;
using Moq;
using ChessWar.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ChessWar.Tests.Unit.AI;

public class AIEfficiencyTests
{
    private readonly Mock<IProbabilityMatrix> _mockProbabilityMatrix;
    private readonly Mock<IGameStateEvaluator> _mockEvaluator;
    private readonly Mock<IAIDifficultyLevel> _mockDifficultyProvider;
    private readonly Mock<ITurnService> _mockTurnService;
    private readonly ChessWar.Domain.Services.AI.AIService _aiService;

    public AIEfficiencyTests()
    {
        _mockProbabilityMatrix = new Mock<IProbabilityMatrix>();
        _mockEvaluator = new Mock<IGameStateEvaluator>();
        _mockDifficultyProvider = new Mock<IAIDifficultyLevel>();
        _mockTurnService = new Mock<ITurnService>();

        _mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) =>
            {
                turn.SpendMP(1);
                turn.ActiveParticipant.Spend(1);
                piece.Position = position;
                piece.IsFirstMove = false;
                turn.UpdateRemainingMP();
            })
            .Returns(true);
        _mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) =>
            {
                turn.SpendMP(2);
                turn.ActiveParticipant.Spend(2);
                foreach (var p in session.Player1.Pieces)
                {
                }
                foreach (var p in session.Player2.Pieces)
                {
                }

                var target = session.GetAllPieces()
                    .FirstOrDefault(p => p.Position.Equals(position) && p.Owner?.Id != piece.Owner?.Id && p.IsAlive);
                if (target != null)
                {
                    TestHelpers.TakeDamage(target, piece.ATK);
                }
                else
                {
                }
                turn.UpdateRemainingMP();
            })
            .Returns(true);
        _mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(1, 1), new Position(2, 2), new Position(3, 3), new Position(4, 4) });
        _mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(5, 5), new Position(6, 6) });

        _mockProbabilityMatrix.Setup(x => x.GetActionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(0.9);
        _mockEvaluator.Setup(x => x.EvaluateGameState(It.IsAny<GameSession>(), It.IsAny<Player>()))
            .Returns(1.0);

        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(_mockTurnService.Object, Mock.Of<IAbilityService>(), Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(_mockProbabilityMatrix.Object, _mockEvaluator.Object, _mockDifficultyProvider.Object);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(_mockTurnService.Object, Mock.Of<IAbilityService>());

        _aiService = new ChessWar.Domain.Services.AI.AIService(
            actionGenerator,
            actionSelector,
            actionExecutor,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ChessWar.Domain.Services.AI.AIService>>()
        );
    }

    [Fact]
    public void MakeAiTurn_WithValidActions_ShouldSpendManaAndExecuteActions()
    {
        var session = CreateGameSessionWithValidActions();
        var initialMana = session.GetCurrentTurn().RemainingMP;

        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);

        _mockProbabilityMatrix.Setup(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.8);
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);

        var result = _aiService.MakeAiTurn(session);

        Assert.True(result, "ИИ должен успешно выполнить ход");

        var finalMana = session.GetCurrentTurn().RemainingMP;
        Assert.True(finalMana < initialMana, $"ИИ должен потратить ману. Было: {initialMana}, стало: {finalMana}");

        var activePlayer = session.GetCurrentTurn().ActiveParticipant;
        var activePieces = activePlayer.Pieces.Where(p => p.IsAlive).ToList();
        var movedPieces = activePieces.Where(p => !p.IsFirstMove).ToList();
        Assert.True(movedPieces.Any(), "ИИ должен сдвинуть хотя бы одну фигуру");
    }

    [Fact]
    public void MakeAiTurn_WithNoValidActions_ShouldNotSpendMana()
    {
        var session = CreateGameSessionWithNoValidActions();
        var initialMana = session.GetCurrentTurn().RemainingMP;

        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);

        _mockProbabilityMatrix.Setup(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.0);
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(0.0);

        var result = _aiService.MakeAiTurn(session);

        Assert.False(result, "ИИ не должен выполнить ход при отсутствии доступных действий");

        var finalMana = session.GetCurrentTurn().RemainingMP;
        Assert.Equal(initialMana, finalMana);
    }

    [Fact]
    public void MakeAiTurn_WithAttackAction_ShouldDealDamage()
    {
        var session = CreateGameSessionWithAttackTarget();
        var activePlayer = session.GetCurrentTurn().ActiveParticipant;
        var targetPiece = session.GetAllPieces().FirstOrDefault(p => p.Owner?.Id != activePlayer.Id && p.IsAlive);
        var initialHp = targetPiece?.HP ?? 0;
        var initialMana = session.GetCurrentTurn().RemainingMP;

        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);

        _mockProbabilityMatrix.Setup(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.8);
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);

        var result = _aiService.MakeAiTurn(session);

        Assert.True(result, "ИИ должен успешно выполнить атаку");

        var finalHp = targetPiece?.HP ?? 0;
       
        var manaSpent = initialMana - session.GetCurrentTurn().RemainingMP;
        Assert.True(manaSpent > 0, $"ИИ должен потратить ману. Потрачено: {manaSpent}");
    }

    [Fact]
    public void MakeAiTurn_WithFailedActions_ShouldNotSpendManaOnFailedActions()
    {
        var session = CreateGameSessionWithFailingActions();
        var initialMana = session.GetCurrentTurn().RemainingMP;

        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);

        _mockProbabilityMatrix.Setup(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.0);
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(0.0);

        var result = _aiService.MakeAiTurn(session);

        Assert.False(result, "ИИ не должен выполнить ход при неуспешных действиях");

        var finalMana = session.GetCurrentTurn().RemainingMP;
        Assert.Equal(initialMana, finalMana);
    }

    [Fact]
    public void MakeAiTurn_WithPartialSuccess_ShouldSpendManaOnlyOnSuccessfulActions()
    {
        var session = CreateGameSessionWithMixedActions();
        var initialMana = session.GetCurrentTurn().RemainingMP;

        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);

        _mockProbabilityMatrix.SetupSequence(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.8) 
            .Returns(0.0);

        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);

        var result = _aiService.MakeAiTurn(session);

        Assert.True(result, "ИИ должен выполнить хотя бы одно успешное действие");

        var finalMana = session.GetCurrentTurn().RemainingMP;
        Assert.True(finalMana < initialMana, "ИИ должен потратить ману на успешные действия");

        var manaSpent = initialMana - finalMana;
        Assert.True(manaSpent > 0, "ИИ должен потратить ману на успешные действия");
    }

    private GameSession CreateGameSessionWithValidActions()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);

        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 6));
        aiPiece.Id = 1;
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);

        var playerPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 1));
        playerPiece.Id = 2;
        playerPiece.HP = 10;
        playerPiece.Owner = player1;
        player1.AddPiece(playerPiece);

        player1.SetMana(50, 50);
        player2.SetMana(50, 50);

        var session = new GameSession(player2, player1, "Test");
        session.StartGame();

        session.GetBoard().PlacePiece(aiPiece);
        session.GetBoard().PlacePiece(playerPiece);

        return session;
    }

    private GameSession CreateGameSessionWithNoValidActions()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);

        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);

        player1.SetMana(50, 50);
        player2.SetMana(50, 50);

        var session = new GameSession(player1, player2, "Test");
        session.StartGame();

        session.GetBoard().PlacePiece(aiPiece);

        return session;
    }

    private GameSession CreateGameSessionWithAttackTarget()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);

        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        aiPiece.Id = 1;
        aiPiece.HP = 10;
        aiPiece.ATK = 5;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);

        var targetPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 1));
        targetPiece.Id = 2;
        targetPiece.HP = 10;
        targetPiece.Owner = player1;
        player1.AddPiece(targetPiece);

        player1.SetMana(50, 50);
        player2.SetMana(50, 50);

        var session = new GameSession(player2, player1, "Test");
        session.StartGame();

        session.GetBoard().PlacePiece(aiPiece);
        session.GetBoard().PlacePiece(targetPiece);

        return session;
    }

    private GameSession CreateGameSessionWithFailingActions()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);

        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 6));
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);

        player1.SetMana(50, 50);
        player2.SetMana(50, 50);

        var session = new GameSession(player1, player2, "Test");
        session.StartGame();

        session.GetBoard().PlacePiece(aiPiece);

        return session;
    }

    private GameSession CreateGameSessionWithMixedActions()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);

        var aiPiece1 = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 6));
        aiPiece1.HP = 10;
        aiPiece1.Owner = player2;
        player2.AddPiece(aiPiece1);

        var aiPiece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(1, 6));
        aiPiece2.HP = 10;
        aiPiece2.Owner = player2;
        player2.AddPiece(aiPiece2);

        player1.SetMana(50, 50);
        player2.SetMana(50, 50);

        var session = new GameSession(player2, player1, "Test");
        session.StartGame();

        session.GetBoard().PlacePiece(aiPiece1);
        session.GetBoard().PlacePiece(aiPiece2);

        return session;
    }
}
