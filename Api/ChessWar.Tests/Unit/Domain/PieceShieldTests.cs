using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace ChessWar.Tests.Unit.Domain;

/// <summary>
/// Unit тесты для свойств ShieldHP и MaxShieldHP в Piece
/// </summary>
public class PieceShieldTests
{
    private readonly IPieceFactory _pieceFactory;
    private readonly Mock<IBalanceConfigProvider> _configProvider;

    public PieceShieldTests()
    {
        _configProvider = new Mock<IBalanceConfigProvider>();
        _configProvider.Setup(x => x.GetActive()).Returns(CreateTestConfig());
        
        var idGenerator = new Mock<IPieceIdGenerator>();
        var idSequence = 0;
        idGenerator.Setup(x => x.GetNextId()).Returns(() => ++idSequence);
        
        _pieceFactory = new PieceFactory(_configProvider.Object, idGenerator.Object);
    }

    /// <summary>
    /// Создаёт тестовый конфиг с MaxShieldHP для всех фигур
    /// </summary>
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
                MovementCosts = new Dictionary<string, int>()
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
            ShieldSystem = new ShieldSystemConfig
            {
                King = new KingShieldConfig { BaseRegen = 10, ProximityBonus1 = new Dictionary<string, int>(), ProximityBonus2 = new Dictionary<string, int>() },
                Ally = new AllyShieldConfig { NeighborContribution = new Dictionary<string, int>() }
            },
            KillRewards = new KillRewardsSection { Pawn = 10, Knight = 20, Bishop = 20, Rook = 30, Queen = 50, King = 100 }
        };
    }
    /// <summary>
    /// Король должен иметь ShieldHP и MaxShieldHP при создании
    /// </summary>
    [Fact]
    public void King_ShouldHave_ShieldProperties()
    {
        // Arrange & Act
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 0));
        
        // Assert
        king.Should().NotBeNull();
        king.ShieldHP.Should().Be(0, "щит изначально пустой");
        king.MaxShieldHP.Should().Be(400, "максимальный щит короля = 400");
    }

    /// <summary>
    /// Все фигуры имеют MaxShieldHP в зависимости от типа
    /// </summary>
    [Theory]
    [InlineData(PieceType.Queen, 150)]
    [InlineData(PieceType.Rook, 100)]
    [InlineData(PieceType.Bishop, 80)]
    [InlineData(PieceType.Knight, 80)]
    [InlineData(PieceType.Pawn, 50)]
    public void AllPieces_ShouldHave_MaxShieldHP(PieceType type, int expectedMaxShield)
    {
        // Arrange & Act
        var piece = _pieceFactory.CreatePiece(type, Team.Elves, new Position(0, 0));
        
        // Assert
        piece.ShieldHP.Should().Be(0, "щит изначально пустой");
        piece.MaxShieldHP.Should().Be(expectedMaxShield, $"{type} должна иметь MaxShieldHP = {expectedMaxShield}");
    }

    /// <summary>
    /// ShieldHP можно установить через сеттер
    /// </summary>
    [Fact]
    public void King_ShieldHP_CanBeSet()
    {
        // Arrange
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 0));
        
        // Act
        king.ShieldHP = 100;
        
        // Assert
        king.ShieldHP.Should().Be(100);
    }

    /// <summary>
    /// ShieldHP не может быть отрицательным
    /// </summary>
    [Fact]
    public void King_ShieldHP_CannotBeNegative()
    {
        // Arrange
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 0));
        
        // Act
        king.ShieldHP = -50;
        
        // Assert
        king.ShieldHP.Should().Be(0, "щит не может быть отрицательным");
    }

    /// <summary>
    /// ShieldHP не может превышать MaxShieldHP
    /// </summary>
    [Fact]
    public void King_ShieldHP_CannotExceedMax()
    {
        // Arrange
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 0));
        
        // Act
        king.ShieldHP = 500;
        
        // Assert
        king.ShieldHP.Should().Be(400, "щит не может превышать максимум");
    }

    /// <summary>
    /// MaxShieldHP одинаков для обеих команд
    /// </summary>
    [Theory]
    [InlineData(PieceType.King, 400)]
    [InlineData(PieceType.Queen, 150)]
    [InlineData(PieceType.Rook, 100)]
    public void MaxShieldHP_SameForBothTeams(PieceType type, int expectedMax)
    {
        // Arrange
        var elvesPiece = _pieceFactory.CreatePiece(type, Team.Elves, new Position(0, 0));
        var orcsPiece = _pieceFactory.CreatePiece(type, Team.Orcs, new Position(0, 7));
        
        // Assert
        elvesPiece.MaxShieldHP.Should().Be(expectedMax);
        orcsPiece.MaxShieldHP.Should().Be(expectedMax);
    }
}

