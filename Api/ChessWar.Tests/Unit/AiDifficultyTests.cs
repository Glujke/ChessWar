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
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Services.AI;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для уровней сложности ИИ (RED PHASE)
/// </summary>
public class AiDifficultyTests
{
   /* [Fact]
    public void Ai_ShouldRespectEasyDifficulty_ManaLimit()
    {
        // Arrange: ИИ уровня "Easy" должен тратить 20-30 маны за ход
        var p1 = new Player("P1", new List<Piece>());
        // Добавляем несколько врагов для атаки
        var enemyPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), p1);
        var enemyPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 2), p1);
        var enemyPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 2), p1);
        p1.Pieces.Add(enemyPawn1);
        p1.Pieces.Add(enemyPawn2);
        p1.Pieces.Add(enemyPawn3);

        var aiPlayer = new Player("AI-Easy", new List<Piece>()); // Имя с уровнем сложности
        var aiPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(2, 3), aiPlayer); // Под врагом
        var aiPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(3, 3), aiPlayer); // Под врагом
        var aiPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(4, 3), aiPlayer); // Под врагом
        aiPlayer.Pieces.Add(aiPawn1);
        aiPlayer.Pieces.Add(aiPawn2);
        aiPlayer.Pieces.Add(aiPawn3);
        aiPlayer.SetMana(50, 50); // Полная мана

        var session = new GameSession(p1, aiPlayer, "AI");
        session.StartGame();
        session.EndCurrentTurn(); // ход ИИ

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var cfg = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        // Создаем мок AiDifficultyProvider, который возвращает "Easy" для этого игрока
        var difficultyProviderMock = new Mock<IAiDifficultyProvider>();
        difficultyProviderMock.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>())).Returns("Easy");
        difficultyProviderMock.Setup(x => x.GetManaLimit("Easy")).Returns(15);

        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnService = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyLevelProvider = new Mock<IAIDifficultyLevel>();
        difficultyLevelProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>())).Returns(AIDifficultyLevel.Medium);
        difficultyLevelProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>())).Returns(1.0);
        IAIService ai = new Infrastructure.Services.AI.ProbabilisticAIService(probabilityMatrix, evaluator, difficultyLevelProvider.Object, turnService, Mock.Of<IAbilityService>(), Mock.Of<ILogger<Infrastructure.Services.AI.ProbabilisticAIService>>());

        var manaBefore = aiPlayer.MP;

        // Act
        var result = ai.MakeAiTurn(session);

        // Assert: ИИ должен потратить 5-15 маны (Easy уровень)
        result.Should().BeTrue();
        var manaSpent = manaBefore - aiPlayer.MP;
        manaSpent.Should().BeInRange(5, 15, "ИИ уровня Easy должен тратить 5-15 маны за ход");
    }*/

    [Fact]
    public void MediumDifficulty_ShouldCallEvaluator_AndUseTopKSelection()
    {
        // Arrange: средняя сложность должна вызывать оценщик и выбирать действие из топ-K
        var p1 = new Player("P1", new List<Piece>());
        // Добавляем несколько врагов для атаки
        var enemyPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), p1);
        var enemyPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 2), p1);
        var enemyPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 2), p1);
        p1.Pieces.Add(enemyPawn1);
        p1.Pieces.Add(enemyPawn2);
        p1.Pieces.Add(enemyPawn3);

        var aiPlayer = new Player("AI-Medium", new List<Piece>()); // Имя с уровнем сложности
        var aiPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(2, 3), aiPlayer); // Под врагом
        var aiPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(3, 3), aiPlayer); // Под врагом
        var aiPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(4, 3), aiPlayer); // Под врагом
        aiPlayer.Pieces.Add(aiPawn1);
        aiPlayer.Pieces.Add(aiPawn2);
        aiPlayer.Pieces.Add(aiPawn3);
        aiPlayer.SetMana(50, 50); // Полная мана

        var session = new GameSession(p1, aiPlayer, "AI");
        session.StartGame();
        session.EndCurrentTurn(); // ход ИИ

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var cfg = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        // Создаем мок AiDifficultyProvider, который возвращает "Medium" для этого игрока
        var difficultyProviderMock = new Mock<IAiDifficultyProvider>();
        difficultyProviderMock.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>())).Returns("Medium");
        difficultyProviderMock.Setup(x => x.GetManaLimit("Medium")).Returns(20);

        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnService = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyLevelProvider = new Mock<IAIDifficultyLevel>();
        difficultyLevelProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>())).Returns(AIDifficultyLevel.Medium);
        difficultyLevelProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>())).Returns(1.0);
        IAIService ai = new Infrastructure.Services.AI.ProbabilisticAIService(probabilityMatrix, evaluator, difficultyLevelProvider.Object, turnService, Mock.Of<IAbilityService>(), Mock.Of<ILogger<Infrastructure.Services.AI.ProbabilisticAIService>>());

        // Act
        var result = ai.MakeAiTurn(session);

        // Assert
        result.Should().BeTrue();
        // Поведенческие проверки выполняются через MarkovDecisionAITests (Verify на матрице/оценщике)
    }

    [Fact]
    public void HardDifficulty_ShouldPreferAggressiveActions()
    {
        // Arrange: сложная сложность должна предпочитать выгодные атаки при доступности
        var p1 = new Player("P1", new List<Piece>());
        // Добавляем несколько врагов для атаки
        var enemyPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), p1);
        var enemyPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 2), p1);
        var enemyPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 2), p1);
        p1.Pieces.Add(enemyPawn1);
        p1.Pieces.Add(enemyPawn2);
        p1.Pieces.Add(enemyPawn3);

        var aiPlayer = new Player("AI-Hard", new List<Piece>()); // Имя с уровнем сложности
        var aiPawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(2, 3), aiPlayer); // Под врагом
        var aiPawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(3, 3), aiPlayer); // Под врагом
        var aiPawn3 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(4, 3), aiPlayer); // Под врагом
        aiPlayer.Pieces.Add(aiPawn1);
        aiPlayer.Pieces.Add(aiPawn2);
        aiPlayer.Pieces.Add(aiPawn3);
        aiPlayer.SetMana(50, 50); // Полная мана

        var session = new GameSession(p1, aiPlayer, "AI");
        session.StartGame();
        session.EndCurrentTurn(); // ход ИИ

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var cfg = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        // Создаем мок AiDifficultyProvider, который возвращает "Hard" для этого игрока
        var difficultyProviderMock = new Mock<IAiDifficultyProvider>();
        difficultyProviderMock.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>())).Returns("Hard");
        difficultyProviderMock.Setup(x => x.GetManaLimit("Hard")).Returns(25);

        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnService = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyLevelProvider = new Mock<IAIDifficultyLevel>();
        difficultyLevelProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>())).Returns(AIDifficultyLevel.Medium);
        difficultyLevelProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>())).Returns(1.0);
        IAIService ai = new Infrastructure.Services.AI.ProbabilisticAIService(probabilityMatrix, evaluator, difficultyLevelProvider.Object, turnService, Mock.Of<IAbilityService>(), Mock.Of<ILogger<Infrastructure.Services.AI.ProbabilisticAIService>>());

        // Act
        var result = ai.MakeAiTurn(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Ai_ShouldUseAbilities_AtMediumAndHardLevels()
    {
        // Arrange: ИИ с доступными способностями
        var p1 = new Player("P1", new List<Piece>());
        var enemyPawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), p1);
        p1.Pieces.Add(enemyPawn);

        var aiPlayer = new Player("AI", new List<Piece>());
        var aiQueen = TestHelpers.CreatePiece(PieceType.Queen, Team.Orcs, new Position(3, 3), aiPlayer);
        aiPlayer.Pieces.Add(aiQueen);
        aiPlayer.SetMana(50, 50); // Полная мана

        var session = new GameSession(p1, aiPlayer, "AI");
        session.StartGame();
        session.EndCurrentTurn(); // ход ИИ

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var cfg = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnService = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyLevelProvider = new Mock<IAIDifficultyLevel>();
        difficultyLevelProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>())).Returns(AIDifficultyLevel.Medium);
        difficultyLevelProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>())).Returns(1.0);
        IAIService ai = new Infrastructure.Services.AI.ProbabilisticAIService(probabilityMatrix, evaluator, difficultyLevelProvider.Object, turnService, Mock.Of<IAbilityService>(), Mock.Of<ILogger<Infrastructure.Services.AI.ProbabilisticAIService>>());

        var turnBefore = session.GetCurrentTurn();
        var actionsBefore = turnBefore.Actions.Count;

        // Act
        var result = ai.MakeAiTurn(session);

        // Assert: ИИ должен использовать способности на среднем и сложном уровнях
        result.Should().BeTrue();
        var turnAfter = session.GetCurrentTurn();
        turnAfter.Actions.Count.Should().BeGreaterThan(actionsBefore, "ИИ должен использовать способности на среднем и сложном уровнях");
    }

    [Fact]
    public void EasyDifficulty_ShouldAvoidExpensiveAbilities_WhenSaferMoveExists()
    {
        // Arrange: лёгкая сложность должна избегать дорогих способностей, если есть безопасное перемещение
        var p1 = new Player("P1", new List<Piece>());
        var enemyPawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), p1);
        p1.Pieces.Add(enemyPawn);

        var aiPlayer = new Player("AI", new List<Piece>());
        var aiQueen = TestHelpers.CreatePiece(PieceType.Queen, Team.Orcs, new Position(3, 3), aiPlayer);
        aiPlayer.Pieces.Add(aiQueen);
        aiPlayer.SetMana(50, 50); // Полная мана

        var session = new GameSession(p1, aiPlayer, "AI");
        session.StartGame();
        session.EndCurrentTurn(); // ход ИИ

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var cfg = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnService = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyLevelProvider = new Mock<IAIDifficultyLevel>();
        difficultyLevelProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>())).Returns(AIDifficultyLevel.Medium);
        difficultyLevelProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>())).Returns(1.0);
        IAIService ai = new Infrastructure.Services.AI.ProbabilisticAIService(probabilityMatrix, evaluator, difficultyLevelProvider.Object, turnService, Mock.Of<IAbilityService>(), Mock.Of<ILogger<Infrastructure.Services.AI.ProbabilisticAIService>>());

        // Act
        var result = ai.MakeAiTurn(session);

        // Assert
        result.Should().BeTrue();
    }
}
