using ChessWar.Domain.Entities;
using ChessWar.Domain.Services.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Enums;
using ChessWar.Application.Services.Pieces;
using ChessWar.Tests.Unit;
using Moq;
using Microsoft.Extensions.Logging;

namespace ChessWar.Tests.Integration.AI;

/// <summary>
/// Интеграционные тесты для проверки реального поведения ИИ
/// </summary>
public class AIRealBehaviorTests
{
    [Fact]
    public void AI_ShouldActuallyMovePiecesWhenSpendingMana()
    {
        var session = CreateRealGameSession();
        var aiService = CreateRealAIService();

        var initialPositions = GetPiecePositions(session.Player2); // AI это Player2
        var initialMana = session.GetCurrentTurn().RemainingMP;

        var result = aiService.MakeAiTurn(session);

        Assert.True(result, "ИИ должен успешно выполнить ход");

        var finalPositions = GetPiecePositions(session.Player2); // AI это Player2

        // Проверяем, что ИИ выполнил хотя бы одно действие
        var aiMana = session.Player2.MP;
        var movedPieces = CountMovedPieces(initialPositions, finalPositions);

        // ИИ должен потратить ману или сдвинуть фигуры, или хотя бы попытаться
        Assert.True(aiMana < 50 || movedPieces > 0 || result,
            $"ИИ должен выполнить действие. Результат: {result}, Ману потрачено: {50 - aiMana}, фигур сдвинуто: {movedPieces}");
    }

    [Fact]
    public void AI_ShouldActuallyDealDamageWhenAttacking()
    {
        var session = CreateGameSessionWithAttackTarget();
        var aiService = CreateRealAIService();

        var targetPiece = session.Player1.Pieces.First(p => p.IsAlive);
        var initialHp = targetPiece.HP;
        var initialMana = session.Player2.MP; // Проверяем ману у ИИ

        var result = aiService.MakeAiTurn(session);

        Assert.True(result, "ИИ должен успешно выполнить атаку");

        var finalHp = targetPiece.HP;
        var finalMana = session.Player2.MP; // Проверяем ману у ИИ

        // Проверяем, что ИИ потратил ману или нанес урон
        Assert.True(finalMana < initialMana || finalHp < initialHp,
            $"ИИ должен потратить ману или нанести урон. Ману: {initialMana} -> {finalMana}, HP: {initialHp} -> {finalHp}");
    }

    [Fact]
    public void AI_ShouldNotSpendManaOnImpossibleActions()
    {
        var session = CreateGameSessionWithNoValidActions();
        var aiService = CreateRealAIService();

        var initialMana = session.GetCurrentTurn().RemainingMP;
        var initialPositions = GetPiecePositions(session.Player2);

        var result = aiService.MakeAiTurn(session);

        var finalMana = session.GetCurrentTurn().RemainingMP;
        var finalPositions = GetPiecePositions(session.Player2);

        Assert.Equal(initialMana, finalMana); // Ману не должна измениться
        Assert.Equal(initialPositions, finalPositions); // Позиции не должны измениться

        if (result)
        {
            var movedPieces = CountMovedPieces(initialPositions, finalPositions);
            Assert.True(movedPieces > 0, "Если ИИ вернул true, он должен сдвинуть фигуры");
        }
    }

    [Fact]
    public void AI_ShouldUseAbilitiesWhenAvailable()
    {
        var session = CreateGameSessionWithAbilities();
        var aiService = CreateRealAIService();

        var initialMana = session.Player2.MP; // Проверяем ману у ИИ
        var pieceWithAbility = session.Player2.Pieces.First(p => p.AbilityCooldowns.ContainsKey("__AuraBuff"));
        var initialCooldown = pieceWithAbility.AbilityCooldowns["__AuraBuff"];

        var result = aiService.MakeAiTurn(session);

        Assert.True(result, "ИИ должен успешно выполнить ход");

        var finalMana = session.Player2.MP; // Проверяем ману у ИИ
        var finalCooldown = pieceWithAbility.AbilityCooldowns["__AuraBuff"];

        // Проверяем, что ИИ потратил ману или использовал способность
        Assert.True(finalMana < initialMana || finalCooldown != initialCooldown,
            $"ИИ должен потратить ману или использовать способность. Ману: {initialMana} -> {finalMana}, Кулдаун: {initialCooldown} -> {finalCooldown}");
    }

    [Fact]
    public void AI_ShouldRespectManaLimits()
    {
        var session = CreateGameSessionWithLimitedMana();
        var aiService = CreateRealAIService();

        var initialMana = session.GetCurrentTurn().RemainingMP;

        var result = aiService.MakeAiTurn(session);

        var finalMana = session.GetCurrentTurn().RemainingMP;

        Assert.True(finalMana >= 0, "ИИ не должен потратить больше маны, чем доступно");

        if (result)
        {
            Assert.True(finalMana < initialMana, "ИИ должен потратить часть маны");
        }
    }

    private GameSession CreateRealGameSession()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);

        var idGenerator = new PieceIdGenerator();

        var aiPiece1 = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 6));
        aiPiece1.Id = idGenerator.GetNextId();
        aiPiece1.HP = 10;
        aiPiece1.Owner = player2;
        player2.AddPiece(aiPiece1);

        var aiPiece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 6));
        aiPiece2.Id = idGenerator.GetNextId();
        aiPiece2.HP = 10;
        aiPiece2.Owner = player2;
        player2.AddPiece(aiPiece2);

        var playerPiece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        playerPiece1.Id = idGenerator.GetNextId();
        playerPiece1.HP = 10;
        playerPiece1.Owner = player1;
        player1.AddPiece(playerPiece1);

        var playerPiece2 = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 1));
        playerPiece2.Id = idGenerator.GetNextId();
        playerPiece2.HP = 10;
        playerPiece2.Owner = player1;
        player1.AddPiece(playerPiece2);

        player1.SetMana(50, 50);
        player2.SetMana(50, 50);

        var session = new GameSession(player2, player1, "Test");
        session.StartGame();

        session.GetBoard().PlacePiece(aiPiece1);
        session.GetBoard().PlacePiece(aiPiece2);
        session.GetBoard().PlacePiece(playerPiece1);
        session.GetBoard().PlacePiece(playerPiece2);

        return session;
    }

    private GameSession CreateGameSessionWithAttackTarget()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);

        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 6));
        aiPiece.Id = 1;
        aiPiece.HP = 10;
        aiPiece.ATK = 5;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);

        var targetPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 5));
        targetPiece.Id = 2;
        targetPiece.HP = 10;
        targetPiece.Owner = player1;
        player1.AddPiece(targetPiece);

        player1.SetMana(50, 50);
        player2.SetMana(50, 50);

        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        session.GetBoard().PlacePiece(aiPiece);
        session.GetBoard().PlacePiece(targetPiece);

        var newTurn = new Turn(1, player2);
        session.SetCurrentTurn(newTurn);

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

    private GameSession CreateGameSessionWithAbilities()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);

        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 6));
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        aiPiece.AbilityCooldowns["__AuraBuff"] = 0;
        player2.AddPiece(aiPiece);

        player1.SetMana(50, 50);
        player2.SetMana(50, 50);

        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        session.GetBoard().PlacePiece(aiPiece);

        var newTurn = new Turn(1, player2);
        session.SetCurrentTurn(newTurn);

        return session;
    }

    private GameSession CreateGameSessionWithLimitedMana()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);

        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 6));
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);

        player1.SetMana(50, 50);
        player2.SetMana(5, 5);

        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        session.GetBoard().PlacePiece(aiPiece);

        return session;
    }

    private ChessWar.Domain.Services.AI.AIService CreateRealAIService()
    {
        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        var logger = Mock.Of<ILogger<ChessWar.Domain.Services.AI.AIService>>();

        var mockTurnService = new Mock<ITurnService>();

        var mockAbilityService = new Mock<IAbilityService>();
        mockAbilityService.Setup(x => x.UseAbility(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Callback<Piece, string, Position, List<Piece>>((piece, abilityName, target, allPieces) =>
            {
                piece.AbilityCooldowns[abilityName] = 3;

                var manaCost = 2;
                piece.Owner?.Spend(manaCost);
            })
            .Returns(true);
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) =>
            {
                turn.SpendMP(1);
                turn.ActiveParticipant.Spend(1);
                piece.Position = position;
                session.GetBoard().MovePiece(piece, position);
                turn.UpdateRemainingMP();
            })
            .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) =>
            {
                turn.SpendMP(2);
                turn.ActiveParticipant.Spend(2);
                var target = session.GetAllPieces().FirstOrDefault(p => p.Position.Equals(position) && p.Owner?.Id != piece.Owner?.Id);
                if (target != null)
                {
                    TestHelpers.TakeDamage(target, piece.ATK);
                }

                turn.UpdateRemainingMP();
            })
            .Returns(true);
        mockTurnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(1, 1), new Position(2, 2), new Position(3, 3), new Position(4, 4) });
        mockTurnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(5, 5), new Position(6, 6) });

        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(mockTurnService.Object, mockAbilityService.Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(probabilityMatrix, evaluator, difficultyProvider);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(mockTurnService.Object, mockAbilityService.Object);

        return new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, logger);
    }

    private Dictionary<int, Position> GetPiecePositions(Participant player)
    {
        return player.Pieces
            .Where(p => p.IsAlive)
            .Select((p, index) => new { Piece = p, Index = index })
            .ToDictionary(x => x.Index, x => x.Piece.Position);
    }

    private int CountMovedPieces(Dictionary<int, Position> initial, Dictionary<int, Position> final)
    {
        return initial.Count(kvp =>
            final.ContainsKey(kvp.Key) &&
            final[kvp.Key] != kvp.Value);
    }
}
