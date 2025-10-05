using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Application.Services.Board;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для TurnOrchestrator
/// </summary>
public class TurnOrchestratorTests
{
    private readonly Mock<ITurnCompletionService> _turnCompletionServiceMock;
    private readonly Mock<IAITurnService> _aiServiceMock;
    private readonly Mock<IGameStateService> _gameStateServiceMock;
    private readonly Mock<IGameNotificationService> _notificationServiceMock;
    private readonly Mock<IGameSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IBalanceConfigProvider> _configProviderMock;
    private readonly Mock<ILogger<TurnOrchestrator>> _loggerMock;
    private readonly Mock<ITurnProcessingQueue> _turnQueueMock;
    private readonly TurnOrchestrator _turnOrchestrator;

    private void AddActionToCurrentTurn(GameSession gameSession)
    {
        var currentTurn = gameSession.GetCurrentTurn();
        currentTurn.AddAction(new TurnAction("Move", "1", new Position(0, 0)));
    }

    public TurnOrchestratorTests()
    {
        _turnCompletionServiceMock = new Mock<ITurnCompletionService>();
        _aiServiceMock = new Mock<IAITurnService>();
        _gameStateServiceMock = new Mock<IGameStateService>();
        _notificationServiceMock = new Mock<IGameNotificationService>();
        _sessionRepositoryMock = new Mock<IGameSessionRepository>();
        _configProviderMock = new Mock<IBalanceConfigProvider>();
        _loggerMock = new Mock<ILogger<TurnOrchestrator>>();

        _configProviderMock
            .Setup(x => x.GetActive())
            .Returns(new BalanceConfig
            {
                Globals = new GlobalsSection { MpRegenPerTurn = 10, CooldownTickPhase = "EndTurn" },
                PlayerMana = new PlayerManaSection { ManaRegenPerTurn = 10 },
                Pieces = new Dictionary<string, PieceStats>(),
                Abilities = new Dictionary<string, AbilitySpecModel>(),
                Evolution = new EvolutionSection
                {
                    XpThresholds = new Dictionary<string, int>(),
                    Rules = new Dictionary<string, List<string>>(),
                    ImmediateOnLastRank = new Dictionary<string, bool>()
                },
                Ai = new AiSection
                {
                    NearEvolutionXp = 19,
                    LastRankEdgeY = new Dictionary<string, int>(),
                    KingAura = new KingAuraConfig { Radius = 3, AtkBonus = 1 }
                },
                KillRewards = new KillRewardsSection
                {
                    Pawn = 10,
                    Knight = 20,
                    Bishop = 20,
                    Rook = 30,
                    Queen = 50,
                    King = 100
                },
                ShieldSystem = new ShieldSystemConfig
                {
                    King = new KingShieldConfig
                    {
                        BaseRegen = 10,
                        ProximityBonus1 = new Dictionary<string, int>(),
                        ProximityBonus2 = new Dictionary<string, int>()
                    },
                    Ally = new AllyShieldConfig
                    {
                        NeighborContribution = new Dictionary<string, int>()
                    }
                }
            });

        var turnProcessorMock = new Mock<ITurnProcessor>();
        var notificationDispatcherMock = new Mock<INotificationDispatcher>();
        var gameStateManagerMock = new Mock<IGameStateManager>();
        var aiSchedulerMock = new Mock<IAITurnScheduler>();
        var lockingServiceMock = new Mock<IOptimisticLockingService>();
        lockingServiceMock
            .Setup(x => x.ExecuteWithLockAsync(It.IsAny<Guid>(), It.IsAny<Func<Task<bool>>>(), It.IsAny<CancellationToken>()))
            .Returns<Guid, Func<Task<bool>>, CancellationToken>(async (id, op, ct) => await op());
        lockingServiceMock
            .Setup(x => x.ExecuteWithLockAsync<object?>(It.IsAny<Guid>(), It.IsAny<Func<Task<object?>>>(), It.IsAny<CancellationToken>()))
            .Returns<Guid, Func<Task<object?>>, CancellationToken>(async (id, op, ct) => await op());
        _turnQueueMock = new Mock<ITurnProcessingQueue>();

        aiSchedulerMock.Setup(x => x.ShouldScheduleAI(It.IsAny<GameSession>())).Returns(true);
        _turnQueueMock.Setup(x => x.EnqueueTurnAsync(It.IsAny<ChessWar.Application.Services.Board.TurnRequest>()))
            .ReturnsAsync(true);

        var turnServiceMock = new Mock<ITurnService>();
        turnServiceMock.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position>());
        turnServiceMock.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
            .Returns(new List<Position>());

        _turnOrchestrator = new TurnOrchestrator(
            turnProcessorMock.Object,
            notificationDispatcherMock.Object,
            gameStateManagerMock.Object,
            aiSchedulerMock.Object,
            lockingServiceMock.Object,
            _turnQueueMock.Object,
            turnServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldEnqueueProcessing_WhenAiShouldBeScheduled()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var aiPlayer = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        var gameSession = new GameSession(player1, aiPlayer, "AI");
        gameSession.StartGame();

        AddActionToCurrentTurn(gameSession);

        await _turnOrchestrator.EndTurnAsync(gameSession);

       
        _turnQueueMock.Verify(x => x.EnqueueTurnAsync(It.IsAny<ChessWar.Application.Services.Board.TurnRequest>()), Times.Once);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldEnqueueProcessing_ForAllModes()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        var gameSession = new GameSession(player1, player2, "LocalCoop");
        gameSession.StartGame();

        AddActionToCurrentTurn(gameSession);
        await _turnOrchestrator.EndTurnAsync(gameSession);

        _turnQueueMock.Verify(x => x.EnqueueTurnAsync(It.IsAny<ChessWar.Application.Services.Board.TurnRequest>()), Times.Once);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldEnqueueProcessing_OnlyOnce_ForPlayerPhase()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var aiPlayer = new Player("AI", new List<Piece>());
        var gameSession = new GameSession(player1, aiPlayer, "AI");
        gameSession.StartGame();

        AddActionToCurrentTurn(gameSession);
        await _turnOrchestrator.EndTurnAsync(gameSession);

        _turnQueueMock.Verify(x => x.EnqueueTurnAsync(It.IsAny<ChessWar.Application.Services.Board.TurnRequest>()), Times.Once);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldNotSendAiNotifications_SinceProcessingIsQueued()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var aiPlayer = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        var gameSession = new GameSession(player1, aiPlayer, "AI");
        gameSession.StartGame();

        AddActionToCurrentTurn(gameSession);
        await _turnOrchestrator.EndTurnAsync(gameSession);

        _notificationServiceMock.Verify(x => x.NotifyAiMoveAsync(
            It.IsAny<Guid>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldNotChangeTurnSynchronously_WhenQueued()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var aiPlayer = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        var gameSession = new GameSession(player1, aiPlayer, "AI");
        gameSession.StartGame();

        var activeBefore = gameSession.GetCurrentTurn().ActiveParticipant;

        AddActionToCurrentTurn(gameSession);
        await _turnOrchestrator.EndTurnAsync(gameSession);

       
        var activeAfter = gameSession.GetCurrentTurn().ActiveParticipant;
        activeAfter.Should().Be(activeBefore);
    }
}
