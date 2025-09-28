using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Application.Services.Board;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Domain.Interfaces.GameLogic;
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
                }
            });

        _turnOrchestrator = new TurnOrchestrator(
            _turnCompletionServiceMock.Object,
            _aiServiceMock.Object,
            _gameStateServiceMock.Object,
            _notificationServiceMock.Object,
            _sessionRepositoryMock.Object,
            _configProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldSwitchTurnBackToPlayer_AfterAiTurn()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var aiPlayer = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        var gameSession = new GameSession(player1, aiPlayer, "AI");
        gameSession.StartGame();

        _turnCompletionServiceMock
            .Setup(x => x.EndTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .Callback<GameSession, CancellationToken>((session, ct) =>
            {
                // После завершения хода игрока активный игрок становится Player2 (ИИ)
                var aiTurn = new Turn(2, aiPlayer);
                session.SetCurrentTurn(aiTurn);
            });

        _aiServiceMock
            .Setup(x => x.MakeAiTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _gameStateServiceMock
            .Setup(x => x.CheckVictory(It.IsAny<GameSession>()))
            .Returns((GameResult?)null);

        AddActionToCurrentTurn(gameSession);
        await _turnOrchestrator.EndTurnAsync(gameSession);

        // После завершения хода игрока активный игрок должен стать AI
        var finalActivePlayer = gameSession.GetCurrentTurn().ActiveParticipant;
        finalActivePlayer.Should().Be(aiPlayer, "ход должен переключиться на ИИ после завершения хода игрока");
        
        // ИИ не должен автоматически вызываться в EndTurnAsync
        _aiServiceMock.Verify(x => x.MakeAiTurnAsync(gameSession, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldNotExecuteAiTurn_WhenNotAiMode()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        var gameSession = new GameSession(player1, player2, "LocalCoop");
        gameSession.StartGame();

        _turnCompletionServiceMock
            .Setup(x => x.EndTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _gameStateServiceMock
            .Setup(x => x.CheckVictory(It.IsAny<GameSession>()))
            .Returns((GameResult?)null);

        AddActionToCurrentTurn(gameSession);
        await _turnOrchestrator.EndTurnAsync(gameSession);

        _aiServiceMock.Verify(x => x.MakeAiTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationServiceMock.Verify(x => x.NotifyAiMoveAsync(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldNotExecuteAiTurn_WhenPlayer1Turn()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var aiPlayer = new Player("AI", new List<Piece>());
        var gameSession = new GameSession(player1, aiPlayer, "AI");
        gameSession.StartGame();

        _turnCompletionServiceMock
            .Setup(x => x.EndTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _gameStateServiceMock
            .Setup(x => x.CheckVictory(It.IsAny<GameSession>()))
            .Returns((GameResult?)null);

        AddActionToCurrentTurn(gameSession);
        await _turnOrchestrator.EndTurnAsync(gameSession);

        _aiServiceMock.Verify(x => x.MakeAiTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldSendNotificationWithAiMoveDetails()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var aiPlayer = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        var gameSession = new GameSession(player1, aiPlayer, "AI");
        gameSession.StartGame();

        _turnCompletionServiceMock
            .Setup(x => x.EndTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .Callback<GameSession, CancellationToken>((session, ct) =>
            {
                // Сначала переключаем ход на ИИ
                var aiTurn = new Turn(2, aiPlayer);
                session.SetCurrentTurn(aiTurn);
            });

        _aiServiceMock
            .Setup(x => x.MakeAiTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _gameStateServiceMock
            .Setup(x => x.CheckVictory(It.IsAny<GameSession>()))
            .Returns((GameResult?)null);

        AddActionToCurrentTurn(gameSession);
        await _turnOrchestrator.EndTurnAsync(gameSession);

        // ИИ не должен автоматически вызываться в EndTurnAsync
        _aiServiceMock.Verify(x => x.MakeAiTurnAsync(gameSession, It.IsAny<CancellationToken>()), Times.Never);
        _notificationServiceMock.Verify(x => x.NotifyAiMoveAsync(
            gameSession.Id,
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EndTurnAsync_ShouldRestorePlayerMana_AfterAiTurn()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var aiPlayer = new ChessWar.Domain.Entities.AI("AI", Team.Orcs);
        
        var config = _configProviderMock.Object.GetActive();
        player1.SetMana(config.PlayerMana.InitialMana, config.PlayerMana.MaxMana);
        aiPlayer.SetMana(config.PlayerMana.InitialMana, config.PlayerMana.MaxMana);
        
        var gameSession = new GameSession(player1, aiPlayer, "AI");
        gameSession.StartGame();

        var initialPlayerMana = player1.MP;

        _turnCompletionServiceMock
            .Setup(x => x.EndTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .Callback<GameSession, CancellationToken>((session, ct) =>
            {
                // Сначала переключаем ход на ИИ
                var aiTurn = new Turn(2, aiPlayer);
                session.SetCurrentTurn(aiTurn);
            });

        _aiServiceMock
            .Setup(x => x.MakeAiTurnAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _gameStateServiceMock
            .Setup(x => x.CheckVictory(It.IsAny<GameSession>()))
            .Returns((GameResult?)null);

        AddActionToCurrentTurn(gameSession);
        await _turnOrchestrator.EndTurnAsync(gameSession);

        // После завершения хода игрока активный игрок должен стать AI
        var finalActivePlayer = gameSession.GetCurrentTurn().ActiveParticipant;
        finalActivePlayer.Should().Be(aiPlayer, "ход должен переключиться на ИИ после завершения хода игрока");
        
        // ИИ не должен автоматически вызываться в EndTurnAsync
        _aiServiceMock.Verify(x => x.MakeAiTurnAsync(gameSession, It.IsAny<CancellationToken>()), Times.Never);
    }
}
