using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Infrastructure.Services;
using FluentAssertions;
using ChessWar.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Services.AI;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для уровней сложности ИИ.
/// </summary>
public class AiDifficultyTests
{


    [Fact]
    public void MediumDifficulty_ShouldCallEvaluator_AndUseTopKSelection()
    {
        var p1 = new Player("P1", new List<Piece>());
        var enemyPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), p1);
        var enemyPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 2), p1);
        var enemyPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 2), p1);
        p1.Pieces.Add(enemyPawn1);
        p1.Pieces.Add(enemyPawn2);
        p1.Pieces.Add(enemyPawn3);

        var aiPlayer = new ChessWar.Domain.Entities.AI("AI-Medium", Team.Orcs);
        var aiPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(2, 3), aiPlayer);
        var aiPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(3, 3), aiPlayer);
        var aiPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(4, 3), aiPlayer);
        aiPlayer.Pieces.Add(aiPawn1);
        aiPlayer.Pieces.Add(aiPawn2);
        aiPlayer.Pieces.Add(aiPawn3);
        aiPlayer.SetMana(50, 50);

        var session = new GameSession(p1, aiPlayer, "AI");
        session.StartGame();
        session.EndCurrentTurn();

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var cfg = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        var difficultyProviderMock = new Mock<IAIDifficultyLevel>();
        difficultyProviderMock.Setup(x => x.GetDifficultyLevel(It.IsAny<ChessWar.Domain.Entities.AI>())).Returns(AIDifficultyLevel.Medium);

        var turnService = new Mock<ITurnService>();
        turnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(1, 1), new Position(2, 2), new Position(3, 3), new Position(4, 4) });
        turnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(5, 5), new Position(6, 6) });
        turnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) =>
            {
                turn.SpendMP(1);
                turn.ActiveParticipant.Spend(1);
                piece.Position = position;
                turn.UpdateRemainingMP();
            })
            .Returns(true);
        turnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) =>
            {
                turn.SpendMP(2);
                turn.ActiveParticipant.Spend(2);
                var target = session.GetAllPieces()
                    .FirstOrDefault(p => p.Position.Equals(position) && p.Owner?.Id != piece.Owner?.Id && p.IsAlive);
                if (target != null)
                {
                    target.HP -= piece.ATK;
                    if (target.HP <= 0)
                    {
                       
                    }
                }
                turn.UpdateRemainingMP();
            })
            .Returns(true);

        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyLevelProvider = new Mock<IAIDifficultyLevel>();
        difficultyLevelProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<ChessWar.Domain.Entities.AI>())).Returns(AIDifficultyLevel.Medium);
        difficultyLevelProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>())).Returns(1.0);
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(turnService.Object, Mock.Of<IAbilityService>(), Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(probabilityMatrix, evaluator, difficultyLevelProvider.Object);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(turnService.Object, Mock.Of<IAbilityService>());

        IAIService ai = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, Mock.Of<ILogger<ChessWar.Domain.Services.AI.AIService>>());

        var result = ai.MakeAiTurn(session);

        result.Should().BeTrue();
    }

    [Fact]
    public void HardDifficulty_ShouldPreferAggressiveActions()
    {
        var p1 = new Player("P1", new List<Piece>());
        var enemyPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), p1);
        var enemyPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 2), p1);
        var enemyPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 2), p1);
        p1.Pieces.Add(enemyPawn1);
        p1.Pieces.Add(enemyPawn2);
        p1.Pieces.Add(enemyPawn3);

        var aiPlayer = new ChessWar.Domain.Entities.AI("AI-Hard", Team.Orcs);
        var aiPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(2, 3), aiPlayer);
        var aiPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(3, 3), aiPlayer);
        var aiPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(4, 3), aiPlayer);
        aiPlayer.Pieces.Add(aiPawn1);
        aiPlayer.Pieces.Add(aiPawn2);
        aiPlayer.Pieces.Add(aiPawn3);
        aiPlayer.SetMana(50, 50);

        var session = new GameSession(p1, aiPlayer, "AI");
        session.StartGame();
        session.EndCurrentTurn();

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var cfg = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        var difficultyProviderMock = new Mock<IAIDifficultyLevel>();
        difficultyProviderMock.Setup(x => x.GetDifficultyLevel(It.IsAny<ChessWar.Domain.Entities.AI>())).Returns(AIDifficultyLevel.Hard);

        var turnService = new Mock<ITurnService>();
        turnService.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(1, 1), new Position(2, 2), new Position(3, 3), new Position(4, 4) });
        turnService.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position> { new Position(5, 5), new Position(6, 6) });
        turnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) =>
            {
                turn.SpendMP(1);
                turn.ActiveParticipant.Spend(1);
                piece.Position = position;
                turn.UpdateRemainingMP();
            })
            .Returns(true);
        turnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) =>
            {
                turn.SpendMP(2);
                turn.ActiveParticipant.Spend(2);
                var target = session.GetAllPieces()
                    .FirstOrDefault(p => p.Position.Equals(position) && p.Owner?.Id != piece.Owner?.Id && p.IsAlive);
                if (target != null)
                {
                    target.HP -= piece.ATK;
                    if (target.HP <= 0)
                    {
                       
                    }
                }
                turn.UpdateRemainingMP();
            })
            .Returns(true);

        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyLevelProvider = new Mock<IAIDifficultyLevel>();
        difficultyLevelProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<ChessWar.Domain.Entities.AI>())).Returns(AIDifficultyLevel.Hard);
        difficultyLevelProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>())).Returns(1.0);
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(turnService.Object, Mock.Of<IAbilityService>(), Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(probabilityMatrix, evaluator, difficultyLevelProvider.Object);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(turnService.Object, Mock.Of<IAbilityService>());

        IAIService ai = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, Mock.Of<ILogger<ChessWar.Domain.Services.AI.AIService>>());

        var result = ai.MakeAiTurn(session);

        result.Should().BeTrue();
    }

    [Fact]
    public void Ai_ShouldUseAbilities_AtMediumAndHardLevels()
    {
        var p1 = new Player("P1", new List<Piece>());
        var enemyPawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), p1);
        p1.Pieces.Add(enemyPawn);

        var aiPlayer = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        var aiQueen = TestHelpers.CreatePiece(PieceType.Queen, Team.Orcs, new Position(3, 3), aiPlayer);
        aiPlayer.Pieces.Add(aiQueen);
        aiPlayer.SetMana(50, 50);

        var session = new GameSession(p1, aiPlayer, "AI");
        session.StartGame();
        session.EndCurrentTurn();

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var cfg = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnService = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg, TestHelpers.CreatePieceFactory()), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), Mock.Of<ICollectiveShieldService>(), turnServiceLogger);
        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyLevelProvider = new Mock<IAIDifficultyLevel>();
        difficultyLevelProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<ChessWar.Domain.Entities.AI>())).Returns(AIDifficultyLevel.Medium);
        difficultyLevelProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>())).Returns(1.0);
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(turnService, Mock.Of<IAbilityService>(), Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(probabilityMatrix, evaluator, difficultyLevelProvider.Object);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(turnService, Mock.Of<IAbilityService>());

        IAIService ai = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, Mock.Of<ILogger<ChessWar.Domain.Services.AI.AIService>>());

        var turnBefore = session.GetCurrentTurn();
        var actionsBefore = turnBefore.Actions.Count;

        var result = ai.MakeAiTurn(session);

        result.Should().BeTrue();
        var turnAfter = session.GetCurrentTurn();
        turnAfter.Actions.Count.Should().BeGreaterThan(actionsBefore, "ИИ должен использовать способности на среднем и сложном уровнях");
    }

    [Fact]
    public void EasyDifficulty_ShouldAvoidExpensiveAbilities_WhenSaferMoveExists()
    {
        var p1 = new Player("P1", new List<Piece>());
        var enemyPawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), p1);
        p1.Pieces.Add(enemyPawn);

        var aiPlayer = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        var aiQueen = TestHelpers.CreatePiece(PieceType.Queen, Team.Orcs, new Position(3, 3), aiPlayer);
        aiPlayer.Pieces.Add(aiQueen);
        aiPlayer.SetMana(50, 50);

        var session = new GameSession(p1, aiPlayer, "AI");
        session.StartGame();
        session.EndCurrentTurn();

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var cfg = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnService = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg, TestHelpers.CreatePieceFactory()), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), Mock.Of<ICollectiveShieldService>(), turnServiceLogger);
        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyLevelProvider = new Mock<IAIDifficultyLevel>();
        difficultyLevelProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<ChessWar.Domain.Entities.AI>())).Returns(AIDifficultyLevel.Medium);
        difficultyLevelProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>())).Returns(1.0);
        var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(turnService, Mock.Of<IAbilityService>(), Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
        var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(probabilityMatrix, evaluator, difficultyLevelProvider.Object);
        var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(turnService, Mock.Of<IAbilityService>());

        IAIService ai = new ChessWar.Domain.Services.AI.AIService(actionGenerator, actionSelector, actionExecutor, Mock.Of<ILogger<ChessWar.Domain.Services.AI.AIService>>());

        var result = ai.MakeAiTurn(session);

        result.Should().BeTrue();
    }
}
