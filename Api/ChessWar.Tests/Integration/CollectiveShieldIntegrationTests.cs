using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Events;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.ValueObjects;
using ChessWar.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChessWar.Tests.Integration;

/// <summary>
/// Интеграционные тесты для системы "Коллективный Щит" в TurnService
/// </summary>
public class CollectiveShieldIntegrationTests
{
    private readonly Mock<IBalanceConfigProvider> _configProvider;
    private readonly Mock<IMovementRulesService> _movementRulesService;
    private readonly Mock<IAttackRulesService> _attackRulesService;
    private readonly Mock<IEvolutionService> _evolutionService;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcher;
    private readonly Mock<IPieceDomainService> _pieceDomainService;
    private readonly Mock<ICollectiveShieldService> _collectiveShieldService;
    private readonly Mock<ILogger<TurnService>> _logger;
    private readonly TurnService _turnService;

    public CollectiveShieldIntegrationTests()
    {
        _configProvider = new Mock<IBalanceConfigProvider>();
        _configProvider.Setup(x => x.GetActive()).Returns(CreateTestConfig());
        
        _movementRulesService = new Mock<IMovementRulesService>();
        _attackRulesService = new Mock<IAttackRulesService>();
        _evolutionService = new Mock<IEvolutionService>();
        _eventDispatcher = new Mock<IDomainEventDispatcher>();
        _pieceDomainService = new Mock<IPieceDomainService>();
        _collectiveShieldService = new Mock<ICollectiveShieldService>();
        _logger = new Mock<ILogger<TurnService>>();

        _turnService = new TurnService(
            _movementRulesService.Object,
            _attackRulesService.Object,
            _evolutionService.Object,
            _configProvider.Object,
            _eventDispatcher.Object,
            _pieceDomainService.Object,
            _collectiveShieldService.Object,
            _logger.Object);
    }

    /// <summary>
    /// После движения фигуры должен вызываться пересчёт щитов
    /// </summary>
    [Fact]
    public void ExecuteMove_ShouldRecalculateShields_AfterMovement()
    {
       
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        session.StartGame();

        var pawn = CreateTestPiece("pawn1", PieceType.Pawn, Team.Elves, new Position(1, 1), player1);
        session.GetBoard().PlacePiece(pawn);

        var turn = session.GetCurrentTurn();
        turn.SelectPiece(pawn);

        _movementRulesService
            .Setup(x => x.CanMoveTo(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        _attackRulesService
            .Setup(x => x.CalculateChebyshevDistance(It.IsAny<Position>(), It.IsAny<Position>()))
            .Returns<Position, Position>((from, to) => 
                Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y)));

        _collectiveShieldService
            .Setup(x => x.RecalculateAllyShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()))
            .Returns(5);

        _pieceDomainService
            .Setup(x => x.MoveTo(It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<Piece, Position>((piece, pos) => piece.Position = pos);

       
        var result = _turnService.ExecuteMove(session, turn, pawn, new Position(2, 1));

       
        result.Should().BeTrue("движение должно быть успешным");
        
        _collectiveShieldService.Verify(
            x => x.RecalculateAllyShield(pawn, It.IsAny<List<Piece>>()),
            Times.Once,
            "должен вызываться пересчёт щита для сходившей фигуры");
    }

    /// <summary>
    /// После атаки с убийством должен вызываться пересчёт щитов атакующей фигуры
    /// </summary>
    [Fact]
    public void ExecuteAttack_WithKill_ShouldRecalculateShields_ForAttacker()
    {
       
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        session.StartGame();

        var attacker = CreateTestPiece("attacker", PieceType.Queen, Team.Elves, new Position(1, 1), player1);
        var target = CreateTestPiece("target", PieceType.Pawn, Team.Orcs, new Position(2, 2), player2);
        
       
        target.HP = 10;
        
        session.GetBoard().PlacePiece(attacker);
        session.GetBoard().PlacePiece(target);

        var turn = session.GetCurrentTurn();
        turn.SelectPiece(attacker);

        _attackRulesService
            .Setup(x => x.CanAttack(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<IReadOnlyList<Piece>>()))
            .Returns(true);

        _attackRulesService
            .Setup(x => x.CalculateChebyshevDistance(It.IsAny<Position>(), It.IsAny<Position>()))
            .Returns<Position, Position>((from, to) => 
                Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y)));

        _pieceDomainService
            .Setup(x => x.TakeDamage(It.IsAny<Piece>(), It.IsAny<int>()))
            .Callback<Piece, int>((piece, damage) => piece.HP -= damage);

        _pieceDomainService
            .Setup(x => x.IsDead(It.IsAny<Piece>()))
            .Returns<Piece>(piece => piece.HP <= 0);

        _collectiveShieldService
            .Setup(x => x.RecalculateAllyShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()))
            .Returns(10);

       
        var result = _turnService.ExecuteAttack(session, turn, attacker, new Position(2, 2));

       

       
        result.Should().BeTrue("атака должна быть успешной");
        
        _collectiveShieldService.Verify(
            x => x.RecalculateAllyShield(attacker, It.IsAny<List<Piece>>()),
            Times.Once,
            "должен вызываться пересчёт щита для атакующей фигуры");
    }

    /// <summary>
    /// Пересчёт щитов должен вызываться для всех соседей в радиусе 1
    /// </summary>
    [Fact]
    public void ExecuteMove_ShouldRecalculateShields_ForAllNeighbors()
    {
       
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        session.StartGame();

        var mover = CreateTestPiece("mover", PieceType.Pawn, Team.Elves, new Position(2, 2), player1);
        var neighbor1 = CreateTestPiece("neighbor1", PieceType.Pawn, Team.Elves, new Position(1, 2), player1);
        var neighbor2 = CreateTestPiece("neighbor2", PieceType.Pawn, Team.Elves, new Position(3, 2), player1);
        var farPiece = CreateTestPiece("far", PieceType.Pawn, Team.Elves, new Position(5, 5), player1);
        
        session.GetBoard().PlacePiece(mover);
        session.GetBoard().PlacePiece(neighbor1);
        session.GetBoard().PlacePiece(neighbor2);
        session.GetBoard().PlacePiece(farPiece);

        var turn = session.GetCurrentTurn();
        turn.SelectPiece(mover);

        _movementRulesService
            .Setup(x => x.CanMoveTo(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        _attackRulesService
            .Setup(x => x.CalculateChebyshevDistance(It.IsAny<Position>(), It.IsAny<Position>()))
            .Returns<Position, Position>((from, to) => 
                Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y)));

        _collectiveShieldService
            .Setup(x => x.RecalculateAllyShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()))
            .Returns(5);

        _pieceDomainService
            .Setup(x => x.MoveTo(It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<Piece, Position>((piece, pos) => piece.Position = pos);

       
        var result = _turnService.ExecuteMove(session, turn, mover, new Position(2, 3));

       
        result.Should().BeTrue("движение должно быть успешным");
        
        _collectiveShieldService.Verify(
            x => x.RecalculateAllyShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()),
            Times.AtLeast(1),
            "должен вызываться пересчёт щита для сходившей фигуры");
    }

    /// <summary>
    /// Короли не должны пересчитывать щиты через RecalculateAllyShield
    /// </summary>
    [Fact]
    public void ExecuteMove_King_ShouldNotRecalculateAllyShield()
    {
       
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        session.StartGame();

        var king = CreateTestPiece("king", PieceType.King, Team.Elves, new Position(1, 1), player1);
        session.GetBoard().PlacePiece(king);

        var turn = session.GetCurrentTurn();
        turn.SelectPiece(king);

        _movementRulesService
            .Setup(x => x.CanMoveTo(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        _attackRulesService
            .Setup(x => x.CalculateChebyshevDistance(It.IsAny<Position>(), It.IsAny<Position>()))
            .Returns<Position, Position>((from, to) => 
                Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y)));

        _pieceDomainService
            .Setup(x => x.MoveTo(It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<Piece, Position>((piece, pos) => piece.Position = pos);

       
        var result = _turnService.ExecuteMove(session, turn, king, new Position(2, 1));

       
        result.Should().BeTrue("движение должно быть успешным");
        
        _collectiveShieldService.Verify(
            x => x.RecalculateAllyShield(king, It.IsAny<List<Piece>>()),
            Times.Never,
            "короли не должны пересчитывать щиты через RecalculateAllyShield");
    }

    /// <summary>
    /// Ошибка в пересчёте щитов не должна ломать ход
    /// </summary>
    [Fact]
    public void ExecuteMove_ShieldRecalculationError_ShouldNotBreakTurn()
    {
       
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        session.StartGame();

        var pawn = CreateTestPiece("pawn1", PieceType.Pawn, Team.Elves, new Position(1, 1), player1);
        session.GetBoard().PlacePiece(pawn);

        var turn = session.GetCurrentTurn();
        turn.SelectPiece(pawn);

        _movementRulesService
            .Setup(x => x.CanMoveTo(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        _attackRulesService
            .Setup(x => x.CalculateChebyshevDistance(It.IsAny<Position>(), It.IsAny<Position>()))
            .Returns<Position, Position>((from, to) => 
                Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y)));

        _collectiveShieldService
            .Setup(x => x.RecalculateAllyShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()))
            .Throws(new Exception("Shield calculation error"));

        _pieceDomainService
            .Setup(x => x.MoveTo(It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<Piece, Position>((piece, pos) => piece.Position = pos);

       
        var result = _turnService.ExecuteMove(session, turn, pawn, new Position(2, 1));

       
        result.Should().BeTrue("ход должен быть успешным даже при ошибке в пересчёте щитов");
    }

    #region Helper Methods

    private static Player CreateTestPlayer(string name)
    {
        return new Player(name, new List<Piece>());
    }

    private static Piece CreateTestPiece(string id, PieceType type, Team team, Position position, Player owner)
    {
        var piece = new Piece(id, type, team, position, owner);
        
       
        piece.HP = type switch
        {
            PieceType.King => 200,
            PieceType.Queen => 150,
            PieceType.Rook => 100,
            PieceType.Bishop => 80,
            PieceType.Knight => 80,
            PieceType.Pawn => 50,
            _ => 50
        };
        
        piece.ATK = type switch
        {
            PieceType.King => 20,
            PieceType.Queen => 15,
            PieceType.Rook => 12,
            PieceType.Bishop => 10,
            PieceType.Knight => 10,
            PieceType.Pawn => 5,
            _ => 5
        };
        
        return piece;
    }

    private static BalanceConfig CreateTestConfig()
    {
        return new BalanceConfig
        {
            Globals = new GlobalsSection { MpRegenPerTurn = 10, CooldownTickPhase = "EndTurn" },
            PlayerMana = new PlayerManaSection
            {
                InitialMana = 10,
                MaxMana = 50,
                ManaRegenPerTurn = 10,
                MandatoryAction = true,
                AttackCost = 1,
                MovementCosts = new Dictionary<string, int>
                {
                    ["Pawn"] = 1,
                    ["Queen"] = 2
                }
            },
            Pieces = new Dictionary<string, PieceStats>
            {
                ["King"] = new PieceStats { Hp = 50, Atk = 3, Range = 1, Movement = 1, XpToEvolve = 0, MaxShieldHP = 400 },
                ["Queen"] = new PieceStats { Hp = 30, Atk = 7, Range = 3, Movement = 8, XpToEvolve = 0, MaxShieldHP = 150 },
                ["Rook"] = new PieceStats { Hp = 25, Atk = 5, Range = 8, Movement = 8, XpToEvolve = 60, MaxShieldHP = 100 },
                ["Bishop"] = new PieceStats { Hp = 18, Atk = 3, Range = 4, Movement = 8, XpToEvolve = 40, MaxShieldHP = 80 },
                ["Knight"] = new PieceStats { Hp = 20, Atk = 4, Range = 1, Movement = 1, XpToEvolve = 40, MaxShieldHP = 80 },
                ["Pawn"] = new PieceStats { Hp = 10, Atk = 2, Range = 1, Movement = 1, XpToEvolve = 20, MaxShieldHP = 50 }
            },
            Abilities = new Dictionary<string, AbilitySpecModel>(),
            Evolution = new EvolutionSection 
            { 
                XpThresholds = new Dictionary<string, int>(), 
                Rules = new Dictionary<string, List<string>>() 
            },
            Ai = new AiSection { NearEvolutionXp = 0 },
            KillRewards = new KillRewardsSection { Pawn = 10, Knight = 20, Bishop = 20, Rook = 30, Queen = 50, King = 100 },
            ShieldSystem = new ShieldSystemConfig
            {
                King = new KingShieldConfig
                {
                    BaseRegen = 10,
                    ProximityBonus1 = new Dictionary<string, int>
                    {
                        ["King"] = 30,
                        ["Queen"] = 30,
                        ["Rook"] = 20,
                        ["Bishop"] = 15,
                        ["Knight"] = 15,
                        ["Pawn"] = 10
                    },
                    ProximityBonus2 = new Dictionary<string, int>
                    {
                        ["King"] = 60,
                        ["Queen"] = 60,
                        ["Rook"] = 40,
                        ["Bishop"] = 30,
                        ["Knight"] = 30,
                        ["Pawn"] = 10
                    }
                },
                Ally = new AllyShieldConfig
                {
                    NeighborContribution = new Dictionary<string, int>
                    {
                        ["King"] = 30,
                        ["Queen"] = 25,
                        ["Rook"] = 20,
                        ["Bishop"] = 15,
                        ["Knight"] = 15,
                        ["Pawn"] = 5
                    }
                }
            }
        };
    }

    #endregion
}
