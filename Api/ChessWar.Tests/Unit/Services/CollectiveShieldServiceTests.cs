using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
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
/// Unit тесты для CollectiveShieldService - система "Коллективный Щит"
/// </summary>
public class CollectiveShieldServiceTests
{
    private readonly Mock<IBalanceConfigProvider> _configProvider;
    private readonly Mock<IAttackRulesService> _attackRulesService;
    private readonly CollectiveShieldService _service;
    private readonly IPieceFactory _pieceFactory;

    public CollectiveShieldServiceTests()
    {
        _configProvider = new Mock<IBalanceConfigProvider>();
        _configProvider.Setup(x => x.GetActive()).Returns(CreateTestConfig());
        
        _attackRulesService = new Mock<IAttackRulesService>();
        _attackRulesService
            .Setup(x => x.CalculateChebyshevDistance(It.IsAny<Position>(), It.IsAny<Position>()))
            .Returns<Position, Position>((from, to) => 
                Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y)));
        
        var idGenerator = new Mock<IPieceIdGenerator>();
        var idSequence = 0;
        idGenerator.Setup(x => x.GetNextId()).Returns(() => ++idSequence);
        
        _pieceFactory = new PieceFactory(_configProvider.Object, idGenerator.Object);
        _service = new CollectiveShieldService(_configProvider.Object, _attackRulesService.Object);
    }

    /// <summary>
    /// Базовая регенерация щита короля без союзников
    /// </summary>
    [Fact]
    public void RegenerateKingShield_NoAllies_ShouldApplyBaseRegen()
    {
       
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4));
        king.ShieldHP = 0;
        var allies = new List<Piece>();

       
        var regenAmount = _service.RegenerateKingShield(king, allies);

       
        regenAmount.Should().Be(10, "базовая регенерация = 10");
        king.ShieldHP.Should().Be(10, "щит короля должен увеличиться на 10");
    }

    /// <summary>
    /// Регенерация щита короля с союзниками в радиусе ≤2
    /// </summary>
    [Fact]
    public void RegenerateKingShield_WithAlliesInRange_ShouldApplyProximityBonus()
    {
       
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4));
        king.ShieldHP = 0;
        
        var allies = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(4, 3)),
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(3, 4)), 
            _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(5, 5))  
        };

       
        var regenAmount = _service.RegenerateKingShield(king, allies);

       
       
       
       
       
       
        regenAmount.Should().Be(70, "базовая регенерация + бонус от близких союзников");
        king.ShieldHP.Should().Be(70);
    }

    /// <summary>
    /// Регенерация щита короля с союзниками в радиусе ≤2 (специальный бонус)
    /// </summary>
    [Fact]
    public void RegenerateKingShield_AlliesAtDistance2_ShouldApplySpecialBonus()
    {
       
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4));
        king.ShieldHP = 0;
        
        var allies = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(4, 2)),
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(2, 4))  
        };

       
        var regenAmount = _service.RegenerateKingShield(king, allies);

       
       
       
       
       
        regenAmount.Should().Be(110, "базовая регенерация + специальный бонус от союзников на расстоянии 2");
        king.ShieldHP.Should().Be(110);
    }

    /// <summary>
    /// Щит короля не может превышать максимум
    /// </summary>
    [Fact]
    public void RegenerateKingShield_CannotExceedMaxShieldHP()
    {
       
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4));
        king.ShieldHP = 350;
        
        var allies = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(4, 3)),
            _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(4, 5)),
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(3, 4)),
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(5, 4))
        };

       
        var regenAmount = _service.RegenerateKingShield(king, allies);

       
        king.ShieldHP.Should().Be(400, "щит не может превышать MaxShieldHP");
        regenAmount.Should().BeLessThanOrEqualTo(50, "регенерация была ограничена максимумом");
    }

    /// <summary>
    /// Союзники за пределами радиуса 2 не дают бонус
    /// </summary>
    [Fact]
    public void RegenerateKingShield_AlliesBeyondRadius_ShouldNotGiveBonus()
    {
       
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4));
        king.ShieldHP = 0;
        
        var allies = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(1, 1)),
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(7, 7))  
        };

       
        var regenAmount = _service.RegenerateKingShield(king, allies);

       
        regenAmount.Should().Be(10, "только базовая регенерация, союзники слишком далеко");
        king.ShieldHP.Should().Be(10);
    }

    /// <summary>
    /// Врагов в списке союзников игнорируются
    /// </summary>
    [Fact]
    public void RegenerateKingShield_EnemyPieces_ShouldBeIgnored()
    {
       
        var king = _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4));
        king.ShieldHP = 0;
        
        var allies = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(4, 3)), 
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Orcs, new Position(3, 4))    
        };

       
        var regenAmount = _service.RegenerateKingShield(king, allies);

       
       
       
       
       
        regenAmount.Should().Be(40, "вражеские фигуры не дают бонус");
        king.ShieldHP.Should().Be(40);
    }

    #region RecalculateAllyShield Tests

    /// <summary>
    /// Обычная фигура без соседей имеет Shield = 0
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_NoNeighbors_ShouldHaveZeroShield()
    {
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        pawn.ShieldHP = 0;
        var neighbors = new List<Piece>();

        var delta = _service.RecalculateAllyShield(pawn, neighbors);

        delta.Should().Be(0, "щит не изменился");
        pawn.ShieldHP.Should().Be(0, "одиночка беззащитен");
    }

    /// <summary>
    /// Одна пешка рядом → Shield = +5
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_OnePawnNeighbor_ShouldHaveShield5()
    {
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        pawn.ShieldHP = 0;
        
        var neighbors = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 4))
        };

        var delta = _service.RecalculateAllyShield(pawn, neighbors);

        delta.Should().Be(5, "щит вырос на 5");
        pawn.ShieldHP.Should().Be(5, "один сосед-пешка даёт +5");
    }

    /// <summary>
    /// Два соседа-пешки → Shield = +10
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_TwoPawnNeighbors_ShouldHaveShield10()
    {
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        pawn.ShieldHP = 0;
        
        var neighbors = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 4)),
            _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(5, 4))
        };

        var delta = _service.RecalculateAllyShield(pawn, neighbors);

        delta.Should().Be(10, "щит вырос на 10");
        pawn.ShieldHP.Should().Be(10, "два соседа-пешки дают +10");
    }

    /// <summary>
    /// Сосед ушёл: было Shield=10, ушёл один сосед → Shield=5
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_NeighborLeft_ShouldDecreaseShield()
    {
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        pawn.ShieldHP = 10;
        
        var neighbors = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 4))
        };

        var delta = _service.RecalculateAllyShield(pawn, neighbors);

        delta.Should().Be(-5, "щит уменьшился на 5");
        pawn.ShieldHP.Should().Be(5, "остался один сосед");
    }

    /// <summary>
    /// Пробитый щит + сосед ушёл: было Shield=6, ушёл один → Shield=1
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_BrokenShieldAndNeighborLeft_ShouldDecrease()
    {
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        pawn.ShieldHP = 6;
        
        var neighbors = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 4))
        };

        var delta = _service.RecalculateAllyShield(pawn, neighbors);

        delta.Should().Be(-1, "щит уменьшился с 6 до 5");
        pawn.ShieldHP.Should().Be(5, "сосед даёт +5, пробитый щит пересчитан");
    }

    /// <summary>
    /// Изоляция: было Shield=6, ушли оба соседа → Shield=0
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_Isolation_ShouldHaveZeroShield()
    {
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        pawn.ShieldHP = 6;
        
        var neighbors = new List<Piece>();

        var delta = _service.RecalculateAllyShield(pawn, neighbors);

        delta.Should().Be(-6, "щит полностью исчез");
        pawn.ShieldHP.Should().Be(0, "изоляция = нет щита");
    }

    /// <summary>
    /// Ферзь с телохранителями получает мощный щит
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_QueenWithBodyguards_ShouldGetStrongShield()
    {
        var queen = _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(4, 4));
        queen.ShieldHP = 0;
        
        var neighbors = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(3, 3)),
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(4, 3)),
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(4, 5)),
            _pieceFactory.CreatePiece(PieceType.Knight, Team.Elves, new Position(5, 4))
        };

        var delta = _service.RecalculateAllyShield(queen, neighbors);

        delta.Should().Be(85, "щит вырос на 85");
        queen.ShieldHP.Should().Be(85, "King +30, Rook +20, Rook +20, Knight +15 = 85");
    }

    /// <summary>
    /// Союзники за пределами радиуса 1 НЕ дают щит
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_AlliesBeyondRadius1_ShouldNotGiveShield()
    {
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        pawn.ShieldHP = 0;
        
        var neighbors = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(2, 2)),
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(4, 6))
        };

        var delta = _service.RecalculateAllyShield(pawn, neighbors);

        delta.Should().Be(0, "щит не изменился");
        pawn.ShieldHP.Should().Be(0, "союзники слишком далеко");
    }

    /// <summary>
    /// Вражеские фигуры не дают щит
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_EnemyNeighbors_ShouldBeIgnored()
    {
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        pawn.ShieldHP = 0;
        
        var neighbors = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(3, 4)),
            _pieceFactory.CreatePiece(PieceType.Queen, Team.Orcs, new Position(5, 4))
        };

        var delta = _service.RecalculateAllyShield(pawn, neighbors);

        delta.Should().Be(5, "щит вырос на 5");
        pawn.ShieldHP.Should().Be(5, "только союзная пешка даёт щит");
    }

    /// <summary>
    /// Щит не может превышать MaxShieldHP
    /// </summary>
    [Fact]
    public void RecalculateAllyShield_CannotExceedMaxShieldHP()
    {
        var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        pawn.ShieldHP = 0;
        
        var neighbors = new List<Piece>
        {
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(3, 4)),
            _pieceFactory.CreatePiece(PieceType.Rook, Team.Elves, new Position(5, 4)),
            _pieceFactory.CreatePiece(PieceType.Queen, Team.Elves, new Position(4, 3)),
            _pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 5))
        };

        var delta = _service.RecalculateAllyShield(pawn, neighbors);

        pawn.ShieldHP.Should().Be(50, "сумма вкладов = 95, но MaxShieldHP = 50");
        delta.Should().Be(50, "щит вырос до максимума");
    }

    #endregion

    /// <summary>
    /// Создаёт тестовый конфиг для щитов
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
}

