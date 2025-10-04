using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace ChessWar.Tests.Unit.Services;

/// <summary>
/// Тесты для PieceDomainService.TakeDamage с системой щитов
/// </summary>
public class PieceDomainServiceShieldTests
{
    private readonly IPieceDomainService _service;
    private readonly IPieceFactory _pieceFactory;

    public PieceDomainServiceShieldTests()
    {
        var configProvider = new Mock<IBalanceConfigProvider>();
        configProvider.Setup(x => x.GetActive()).Returns(TestHelper.CreateTestConfig());
        
        var idGenerator = new Mock<IPieceIdGenerator>();
        var idSequence = 0;
        idGenerator.Setup(x => x.GetNextId()).Returns(() => ++idSequence);
        
        _pieceFactory = new PieceFactory(configProvider.Object, idGenerator.Object);
        _service = new PieceDomainService();
    }

    /// <summary>
    /// Урон сначала наносится по щиту
    /// </summary>
    [Fact]
    public void TakeDamage_WithShield_ShouldDamageShieldFirst()
    {
        // Arrange
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4));
        king.HP = 50;
        king.ShieldHP = 100;

        // Act
        _service.TakeDamage(king, 30);

        // Assert
        king.ShieldHP.Should().Be(70, "урон сначала по щиту");
        king.HP.Should().Be(50, "реальное HP не тронуто");
    }

    /// <summary>
    /// Если урон больше щита, остаток наносится по HP
    /// </summary>
    [Fact]
    public void TakeDamage_DamageExceedsShield_ShouldDamageHPAfterShield()
    {
        // Arrange
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 3));
        pawn.HP = 10;
        pawn.ShieldHP = 20;

        // Act
        _service.TakeDamage(pawn, 30); // 20 по щиту, 10 по HP

        // Assert
        pawn.ShieldHP.Should().Be(0, "щит полностью разрушен");
        pawn.HP.Should().Be(0, "остаток урона нанесён по HP");
    }

    /// <summary>
    /// Без щита урон наносится напрямую по HP
    /// </summary>
    [Fact]
    public void TakeDamage_NoShield_ShouldDamageHPDirectly()
    {
        // Arrange
        var rook = _pieceFactory.CreatePiece(PieceType.Rook, Team.Orcs, new Position(0, 0));
        rook.HP = 25;
        rook.ShieldHP = 0;

        // Act
        _service.TakeDamage(rook, 10);

        // Assert
        rook.ShieldHP.Should().Be(0, "щита не было");
        rook.HP.Should().Be(15, "урон нанесён напрямую по HP");
    }

    /// <summary>
    /// Щит полностью поглощает урон
    /// </summary>
    [Fact]
    public void TakeDamage_ShieldAbsorbsAllDamage_HPUntouched()
    {
        // Arrange
        var queen = _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(4, 0));
        queen.HP = 30;
        queen.ShieldHP = 150;

        // Act
        _service.TakeDamage(queen, 50);

        // Assert
        queen.ShieldHP.Should().Be(100, "щит поглотил весь урон");
        queen.HP.Should().Be(30, "HP не тронуто");
    }

    /// <summary>
    /// Огромный урон пробивает щит и убивает фигуру
    /// </summary>
    [Fact]
    public void TakeDamage_MassiveDamage_ShouldKillPiece()
    {
        // Arrange
        var knight = _pieceFactory.CreatePiece(PieceType.Knight, Team.Elves, new Position(1, 0));
        knight.HP = 20;
        knight.ShieldHP = 30;

        // Act
        _service.TakeDamage(knight, 100); // 30 по щиту, 70 по HP (но HP всего 20)

        // Assert
        knight.ShieldHP.Should().Be(0, "щит разрушен");
        knight.HP.Should().Be(0, "фигура убита");
        _service.IsDead(knight).Should().BeTrue("фигура мертва");
    }

    /// <summary>
    /// Урон = 0 не изменяет состояние
    /// </summary>
    [Fact]
    public void TakeDamage_ZeroDamage_ShouldNotChangeState()
    {
        // Arrange
        var bishop = _pieceFactory.CreatePiece(PieceType.Bishop, Team.Orcs, new Position(2, 7));
        bishop.HP = 18;
        bishop.ShieldHP = 40;

        // Act
        _service.TakeDamage(bishop, 0);

        // Assert
        bishop.ShieldHP.Should().Be(40, "щит не изменён");
        bishop.HP.Should().Be(18, "HP не изменено");
    }

    /// <summary>
    /// Щит не может стать отрицательным
    /// </summary>
    [Fact]
    public void TakeDamage_ShieldCannotBeNegative()
    {
        // Arrange
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 1));
        pawn.HP = 10;
        pawn.ShieldHP = 5;

        // Act
        _service.TakeDamage(pawn, 20); // 5 по щиту, 15 по HP

        // Assert
        pawn.ShieldHP.Should().Be(0, "щит не может быть отрицательным");
        pawn.HP.Should().Be(0, "остаток урона убил фигуру");
    }
}

/// <summary>
/// Хелпер для создания тестового конфига
/// </summary>
internal static class TestHelper
{
    public static ChessWar.Domain.Entities.Config.BalanceConfig CreateTestConfig()
    {
        return new ChessWar.Domain.Entities.Config.BalanceConfig
        {
            Globals = new ChessWar.Domain.Entities.Config.GlobalsSection { MpRegenPerTurn = 10, CooldownTickPhase = "EndTurn" },
            PlayerMana = new ChessWar.Domain.Entities.Config.PlayerManaSection
            {
                InitialMana = 10,
                MaxMana = 50,
                ManaRegenPerTurn = 10,
                MandatoryAction = true,
                AttackCost = 1,
                MovementCosts = new Dictionary<string, int>()
            },
            Pieces = new Dictionary<string, ChessWar.Domain.Entities.Config.PieceStats>
            {
                ["King"] = new ChessWar.Domain.Entities.Config.PieceStats { Hp = 50, Atk = 3, Range = 1, Movement = 1, XpToEvolve = 0, MaxShieldHP = 400 },
                ["Queen"] = new ChessWar.Domain.Entities.Config.PieceStats { Hp = 30, Atk = 7, Range = 3, Movement = 8, XpToEvolve = 0, MaxShieldHP = 150 },
                ["Rook"] = new ChessWar.Domain.Entities.Config.PieceStats { Hp = 25, Atk = 5, Range = 8, Movement = 8, XpToEvolve = 60, MaxShieldHP = 100 },
                ["Bishop"] = new ChessWar.Domain.Entities.Config.PieceStats { Hp = 18, Atk = 3, Range = 4, Movement = 8, XpToEvolve = 40, MaxShieldHP = 80 },
                ["Knight"] = new ChessWar.Domain.Entities.Config.PieceStats { Hp = 20, Atk = 4, Range = 1, Movement = 1, XpToEvolve = 40, MaxShieldHP = 80 },
                ["Pawn"] = new ChessWar.Domain.Entities.Config.PieceStats { Hp = 10, Atk = 2, Range = 1, Movement = 1, XpToEvolve = 20, MaxShieldHP = 50 }
            },
            Abilities = new Dictionary<string, ChessWar.Domain.Entities.Config.AbilitySpecModel>(),
            Evolution = new ChessWar.Domain.Entities.Config.EvolutionSection 
            { 
                XpThresholds = new Dictionary<string, int>(), 
                Rules = new Dictionary<string, List<string>>() 
            },
            Ai = new ChessWar.Domain.Entities.Config.AiSection { NearEvolutionXp = 0 },
            ShieldSystem = new ChessWar.Domain.Entities.Config.ShieldSystemConfig
            {
                King = new ChessWar.Domain.Entities.Config.KingShieldConfig { BaseRegen = 10, ProximityBonus1 = new Dictionary<string, int>(), ProximityBonus2 = new Dictionary<string, int>() },
                Ally = new ChessWar.Domain.Entities.Config.AllyShieldConfig { NeighborContribution = new Dictionary<string, int>() }
            },
            KillRewards = new ChessWar.Domain.Entities.Config.KillRewardsSection { Pawn = 10, Knight = 20, Bishop = 20, Rook = 30, Queen = 50, King = 100 }
        };
    }
}

