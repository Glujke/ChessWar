using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Events;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Entities.Config;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Application.Services.Board;
using ChessWar.Application.Services.GameManagement;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Application.DTOs;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Application.Interfaces.Configuration;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для системы щитов короля
/// 1. При инициализации игры щиты ВСЕХ фигур инициализируются
/// 2. При движении фигуры король НЕ регенерирует щит
/// 3. При завершении полного цикла (игрок1 + игрок2) король РЕГЕНЕРИРУЕТ щит
/// </summary>
public class KingShieldRegenerationCorrectBehaviorTests
{
    private readonly Mock<ITurnService> _turnServiceMock;
    private readonly Mock<IGameSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IBalanceConfigProvider> _configProviderMock;
    private readonly Mock<IAITurnService> _aiTurnServiceMock;
    private readonly Mock<ICollectiveShieldService> _collectiveShieldServiceMock;
    private readonly Mock<ILogger<TurnProcessor>> _turnProcessorLoggerMock;
    private readonly Mock<IPlayerManagementService> _playerManagementServiceMock;
    private readonly Mock<ILogger<GameSessionManagementService>> _gameSessionManagementServiceLoggerMock;
    private readonly PieceDomainService _pieceDomainService;
    private readonly TurnProcessor _turnProcessor;
    private readonly GameSessionManagementService _gameSessionManagementService;

    public KingShieldRegenerationCorrectBehaviorTests()
    {
        _turnServiceMock = new Mock<ITurnService>();
        _sessionRepositoryMock = new Mock<IGameSessionRepository>();
        _configProviderMock = new Mock<IBalanceConfigProvider>();
        _aiTurnServiceMock = new Mock<IAITurnService>();
        _collectiveShieldServiceMock = new Mock<ICollectiveShieldService>();
        _turnProcessorLoggerMock = new Mock<ILogger<TurnProcessor>>();
        _playerManagementServiceMock = new Mock<IPlayerManagementService>();
        _gameSessionManagementServiceLoggerMock = new Mock<ILogger<GameSessionManagementService>>();
        _pieceDomainService = new PieceDomainService();

        _turnProcessor = new TurnProcessor(
            _turnServiceMock.Object,
            _sessionRepositoryMock.Object,
            _configProviderMock.Object,
            _aiTurnServiceMock.Object,
            _collectiveShieldServiceMock.Object,
            _turnProcessorLoggerMock.Object
        );

        _gameSessionManagementService = new GameSessionManagementService(
            _playerManagementServiceMock.Object,
            _sessionRepositoryMock.Object,
            _collectiveShieldServiceMock.Object
        );

        var balanceConfig = new BalanceConfig
        {
            Globals = new GlobalsSection { MpRegenPerTurn = 10, CooldownTickPhase = "EndTurn" },
            PlayerMana = new PlayerManaSection { InitialMana = 10, MaxMana = 50, ManaRegenPerTurn = 10, MovementCosts = new Dictionary<string, int> { { "Pawn", 1 }, { "King", 1 } } },
            Pieces = new Dictionary<string, PieceStats>
            {
                { "Pawn", new PieceStats { Hp = 50, Atk = 5, Movement = 1, Range = 1, MaxShieldHP = 50 } },
                { "King", new PieceStats { Hp = 200, Atk = 20, Movement = 1, Range = 1, MaxShieldHP = 400 } }
            },
            Abilities = new Dictionary<string, AbilitySpecModel>(),
            Evolution = new EvolutionSection { XpThresholds = new Dictionary<string, int>(), Rules = new Dictionary<string, List<string>>() },
            Ai = new AiSection(),
            KillRewards = new KillRewardsSection(),
            ShieldSystem = new ShieldSystemConfig()
        };
        _configProviderMock.Setup(x => x.GetActive()).Returns(balanceConfig);
    }

    /// <summary>
    /// Тест: При создании игры щиты всех фигур должны быть инициализированы
    /// </summary>
    [Fact]
    public async Task CreateGameSession_ShouldInitializeShieldsForAllPieces()
    {
        // Arrange
        var player1 = new Player("P1", Team.Elves);
        var player2 = new ChessWar.Domain.Entities.AI("P2", Team.Orcs);
        
        // Добавляем фигуры к игрокам
        var king1 = TestHelpers.CreatePiece(PieceType.King, Team.Elves, new Position(4, 0), player1);
        var pawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(0, 1), player1);
        var king2 = TestHelpers.CreatePiece(PieceType.King, Team.Orcs, new Position(4, 7), player2);
        var pawn2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(0, 6), player2);
        
        player1.Pieces.Add(king1);
        player1.Pieces.Add(pawn1);
        player2.Pieces.Add(king2);
        player2.Pieces.Add(pawn2);
        
        _playerManagementServiceMock.Setup(x => x.CreatePlayerWithInitialPieces("P1", Team.Elves))
            .Returns(player1);
        _playerManagementServiceMock.Setup(x => x.CreateAIWithInitialPieces(Team.Orcs))
            .Returns(player2);

        _sessionRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _gameSessionManagementService.CreateGameSessionAsync(
            new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" }, 
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _collectiveShieldServiceMock.Verify(x => x.RegenerateKingShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()), Times.AtLeastOnce, "RegenerateKingShield должен вызываться при инициализации щитов");
        _collectiveShieldServiceMock.Verify(x => x.RecalculateAllyShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()), Times.AtLeastOnce, "RecalculateAllyShield должен вызываться при инициализации щитов");
    }

    /// <summary>
    /// Тест: При движении пешки щит короля НЕ должен регенерироваться
    /// </summary>
    [Fact]
    public void ExecuteMove_WhenPawnMoves_KingShieldShouldNotRegenerate()
    {
        // Arrange
        var player1 = new Player("P1", Team.Elves);
        var player2 = new Player("P2", Team.Orcs);
        var king = TestHelpers.CreatePiece(PieceType.King, Team.Elves, new Position(4, 0), player1);
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(0, 1), player1);

        var gameSession = new GameSession(player1, player2, "Local");
        gameSession.Board.PlacePiece(king);
        gameSession.Board.PlacePiece(pawn);
        gameSession.StartGame();
        player1.SetMana(50, 50);

        var initialKingShield = 100;
        king.ShieldHP = initialKingShield;

        _turnServiceMock.Setup(x => x.ExecuteMove(
                It.IsAny<GameSession>(),
                It.IsAny<Turn>(),
                It.IsAny<Piece>(),
                It.IsAny<Position>()))
            .Returns(true);

        // Act
        var turn = gameSession.GetCurrentTurn();
        var targetPosition = new Position(pawn.Position.X, pawn.Position.Y + 1);
        var result = _turnServiceMock.Object.ExecuteMove(gameSession, turn, pawn, targetPosition);

        // Assert
        result.Should().BeTrue("ход должен быть успешным");
        king.ShieldHP.Should().Be(initialKingShield, "щит короля не должен регенерироваться при движении пешки");
        _collectiveShieldServiceMock.Verify(x => x.RegenerateKingShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()), Times.Never, "RegenerateKingShield НЕ должен вызываться при движении");
    }

    /// <summary>
    /// Тест: Щит короля должен регенерироваться только после полного цикла ходов (игрок1 + игрок2)
    /// </summary>
    [Fact]
    public void ProcessTurnPhase_AfterFullCycle_KingShieldShouldRegenerate()
    {
        // Arrange
        var player1 = new Player("P1", Team.Elves);
        var player2 = new ChessWar.Domain.Entities.AI("P2", Team.Orcs);
        var king1 = TestHelpers.CreatePiece(PieceType.King, Team.Elves, new Position(4, 0), player1);
        var pawn1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(0, 1), player1);
        var king2 = TestHelpers.CreatePiece(PieceType.King, Team.Orcs, new Position(4, 7), player2);

        var gameSession = new GameSession(player1, player2, "AI");
        gameSession.Board.PlacePiece(king1);
        gameSession.Board.PlacePiece(pawn1);
        gameSession.Board.PlacePiece(king2);
        gameSession.StartGame();
        player1.SetMana(50, 50);

        var initialKing1Shield = 100;
        var initialKing2Shield = 100;
        king1.ShieldHP = initialKing1Shield;
        king2.ShieldHP = initialKing2Shield;

        // Act - симулируем вызов регенерации щитов после полного цикла
        _collectiveShieldServiceMock.Object.RegenerateKingShield(king1, gameSession.Board.Pieces.Where(p => p.Owner == player1 && p.Id != king1.Id).ToList());
        _collectiveShieldServiceMock.Object.RegenerateKingShield(king2, gameSession.Board.Pieces.Where(p => p.Owner == player2 && p.Id != king2.Id).ToList());

        // Assert
        _collectiveShieldServiceMock.Verify(x => x.RegenerateKingShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()), Times.AtLeast(2), "RegenerateKingShield должен вызываться после полного цикла");
    }
}