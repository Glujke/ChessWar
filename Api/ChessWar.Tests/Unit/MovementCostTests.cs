using Microsoft.Extensions.Logging;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using ChessWar.Tests.Helpers;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для системы стоимости движения
/// </summary>
public class MovementCostTests
{
    private readonly Mock<IMovementRulesService> _movementRulesServiceMock;
    private readonly Mock<IAttackRulesService> _attackRulesServiceMock;
    private readonly Mock<IEvolutionService> _evolutionServiceMock;
    private readonly Mock<IBalanceConfigProvider> _configProviderMock;
    private readonly TurnService _turnService;

    public MovementCostTests()
    {
        _movementRulesServiceMock = new Mock<IMovementRulesService>();
        _attackRulesServiceMock = new Mock<IAttackRulesService>();
        _evolutionServiceMock = new Mock<IEvolutionService>();
        _configProviderMock = new Mock<IBalanceConfigProvider>();
        
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        _turnService = new TurnService(_movementRulesServiceMock.Object, _attackRulesServiceMock.Object, _evolutionServiceMock.Object, _configProviderMock.Object, new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
    }

    [Fact]
    public void ExecuteMove_ShouldCheckManaCost_ForPawn()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(5, 50);
        var turn = new Turn(1, player);
        var pawn = CreateTestPiece(PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        turn.SelectPiece(pawn);

        var config = CreateTestConfig();
        _configProviderMock.Setup(x => x.GetActive()).Returns(config);
        _movementRulesServiceMock.Setup(x => x.CanMoveTo(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        var result = _turnService.ExecuteMove(new GameSession(player, new Player("Enemy", new List<Piece>())), turn, pawn, new Position(1, 2));

        result.Should().BeTrue();
        turn.RemainingMP.Should().Be(4); // 5 - 1 (стоимость пешки)
        player.MP.Should().Be(4); // Игрок тоже должен потратить ману
    }

    [Fact]
    public void ExecuteMove_ShouldCheckManaCost_ForKnight()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(5, 50);
        var turn = new Turn(1, player);
        var knight = CreateTestPiece(PieceType.Knight, Team.Elves, new Position(1, 1), player);
        turn.SelectPiece(knight);

        var config = CreateTestConfig();
        _configProviderMock.Setup(x => x.GetActive()).Returns(config);
        _movementRulesServiceMock.Setup(x => x.CanMoveTo(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        var result = _turnService.ExecuteMove(new GameSession(player, new Player("Enemy", new List<Piece>())), turn, knight, new Position(2, 3));

        result.Should().BeTrue();
        turn.RemainingMP.Should().Be(3); // 5 - 2 (стоимость коня)
        player.MP.Should().Be(3);
    }

    [Fact]
    public void ExecuteMove_ShouldReturnFalse_WhenNotEnoughMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(1, 50); // Недостаточно для короля (стоимость 4)
        var turn = new Turn(1, player);
        var king = CreateTestPiece(PieceType.King, Team.Elves, new Position(1, 1), player);
        turn.SelectPiece(king);

        var config = CreateTestConfig();
        _configProviderMock.Setup(x => x.GetActive()).Returns(config);

        var result = _turnService.ExecuteMove(new GameSession(player, new Player("Enemy", new List<Piece>())), turn, king, new Position(1, 2));

        result.Should().BeFalse();
        turn.RemainingMP.Should().Be(1); // Не изменилось
        player.MP.Should().Be(1); // Не изменилось
    }

    [Fact]
    public void ExecuteMove_ShouldReturnFalse_WhenMovementRulesFail()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(10, 50);
        var turn = new Turn(1, player);
        var pawn = CreateTestPiece(PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        turn.SelectPiece(pawn);

        var config = CreateTestConfig();
        _configProviderMock.Setup(x => x.GetActive()).Returns(config);
        _movementRulesServiceMock.Setup(x => x.CanMoveTo(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(false); // Правила движения не позволяют

        var result = _turnService.ExecuteMove(new GameSession(player, new Player("Enemy", new List<Piece>())), turn, pawn, new Position(1, 2));

        result.Should().BeFalse();
        turn.RemainingMP.Should().Be(10); // Не изменилось
        player.MP.Should().Be(10); // Не изменилось
    }

    [Fact]
    public void ExecuteMove_ShouldUseDefaultCost_ForUnknownPieceType()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(5, 50);
        var turn = new Turn(1, player);
        var piece = CreateTestPiece(PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        turn.SelectPiece(piece);

        var config = CreateTestConfig();
        config.PlayerMana.MovementCosts.Clear(); // Убираем все стоимости
        _configProviderMock.Setup(x => x.GetActive()).Returns(config);
        _movementRulesServiceMock.Setup(x => x.CanMoveTo(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        var result = _turnService.ExecuteMove(new GameSession(player, new Player("Enemy", new List<Piece>())), turn, piece, new Position(1, 2));

        result.Should().BeTrue();
        turn.RemainingMP.Should().Be(4); // 5 - 1 (дефолтная стоимость)
        player.MP.Should().Be(4);
    }

    private static Piece CreateTestPiece(PieceType type, Team team, Position position, Player owner)
    {
        return new Piece
        {
            Id = 1,
            Type = type,
            Team = team,
            Position = position,
            Owner = owner,
            HP = 10,
            ATK = 5,
            Range = 1,
            Movement = 1,
            XP = 0,
            XPToEvolve = 20,
            IsFirstMove = true
        };
    }

    private static BalanceConfig CreateTestConfig()
    {
        return new BalanceConfig
        {
            Globals = new GlobalsSection { MpRegenPerTurn = 5, CooldownTickPhase = "EndTurn" },
            PlayerMana = new PlayerManaSection
            {
                InitialMana = 10,
                MaxMana = 50,
                ManaRegenPerTurn = 10,
                MandatoryAction = true,
                MovementCosts = new Dictionary<string, int>
                {
                    ["Pawn"] = 1,
                    ["Knight"] = 2,
                    ["Bishop"] = 3,
                    ["Rook"] = 3,
                    ["Queen"] = 4,
                    ["King"] = 4
                }
            },
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
        };
    }
}



