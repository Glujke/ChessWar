using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Events;
using ChessWar.Domain.Events.Handlers;
using ChessWar.Domain.Services.GameLogic;
using FluentAssertions;
using Moq;
using ChessWar.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace ChessWar.Tests.Unit;

public class AbilityServiceTests
{
    private readonly IAbilityService _abilityService;

    public AbilityServiceTests()
    {
        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var provider = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        var pieceDomainService = new Mock<IPieceDomainService>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<PieceKilledEvent>>)))
            .Returns(new List<IDomainEventHandler<PieceKilledEvent>>
            {
                new ExperienceAwardHandler(pieceDomainService.Object, provider),
                new BoardCleanupHandler(),
                new PositionSwapHandler()
            });

        var eventDispatcher = new DomainEventDispatcher(serviceProviderMock.Object);

        _abilityService = new AbilityService(provider, eventDispatcher, pieceDomainService.Object);
    }

    [Fact]
    public void UseAbility_ShouldFail_WhenNotEnoughMp()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(0, 50);
        var piece = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, 2, 2);
        piece.Owner = owner;
        owner.Pieces.Add(piece);

        var canUse = _abilityService.CanUseAbility(piece, "LightArrow", new Position(4, 4), owner.Pieces);

        canUse.Should().BeFalse();
    }

    [Fact]
    public void UseAbility_ShouldSetCooldown_AndSpendMp_OnSuccess()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50);
        var piece = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, 2, 2);
        piece.Owner = owner;
        owner.Pieces.Add(piece);
        var target = new Position(4, 4);

        var enemyOwner = new Player("P2", new List<Piece>());
        var enemy = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, target.X, target.Y);
        enemy.Owner = enemyOwner;
        var allPieces = owner.Pieces.Concat(new[] { enemy }).ToList();

        var testAbilityService = CreateAbilityServiceWithConfiguredMocks(new List<Piece> { enemy });

        var result = testAbilityService.UseAbility(piece, "LightArrow", target, allPieces);

        result.Should().BeTrue();
        owner.MP.Should().BeLessThan(50);
        piece.AbilityCooldowns.GetValueOrDefault("LightArrow").Should().BeGreaterThan(0);
        enemy.HP.Should().BeLessThan(10);
    }

    [Fact]
    public void Heal_ShouldIncreaseHp_WithinRange()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50);
        var bishop = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, 2, 2);
        var ally = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 3, 3);
        bishop.Owner = owner; ally.Owner = owner;
        owner.Pieces.Add(bishop);
        owner.Pieces.Add(ally);
        TestHelpers.TakeDamage(ally, 5);

        var testAbilityService = CreateAbilityServiceWithConfiguredMocks(new List<Piece> { ally });

        var ok = testAbilityService.UseAbility(bishop, "Heal", ally.Position, owner.Pieces);

        ok.Should().BeTrue();
        ally.HP.Should().BeGreaterThan(5);
    }

    [Fact]
    public void UseAbility_ShouldFail_WhenOnCooldown()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50);
        var piece = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, 2, 2);
        piece.Owner = owner;
        owner.Pieces.Add(piece);
        TestHelpers.SetAbilityCooldown(piece, "LightArrow", 1);

        var canUse = _abilityService.CanUseAbility(piece, "LightArrow", new Position(3, 3), owner.Pieces);

        canUse.Should().BeFalse();
    }

    [Fact]
    public void UseAbility_Aoe_ShouldIgnoreLineOfSight()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50);
        var queen = TestHelpers.CreatePiece(PieceType.Queen, Team.Elves, 3, 3);
        queen.Owner = owner;
        owner.Pieces.Add(queen);

        var blockers = new List<Piece>
        {
            queen,
            TestHelpers.CreatePiece(PieceType.Rook, Team.Elves, 4, 3),
            TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, 5, 3)  
        };

        var result = _abilityService.UseAbility(queen, "MagicExplosion", new Position(5, 3), blockers);

        result.Should().BeTrue();
    }

    #region Position Swap on Ability Kill Tests

    [Fact]
    public void UseAbility_ShieldBash_WhenKillingEnemy_ShouldMoveAttackerToKilledPosition()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50);
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 2, 2);
        pawn.Owner = owner;
        owner.Pieces.Add(pawn);

        var enemyOwner = new Player("P2", new List<Piece>());
        var enemy = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, 3, 2);
        enemy.Owner = enemyOwner;
        enemy.HP = 1;

        var allPieces = new List<Piece> { pawn, enemy };

        var testAbilityService = CreateAbilityServiceWithConfiguredMocks(new List<Piece> { enemy });

        var result = testAbilityService.UseAbility(pawn, "ShieldBash", enemy.Position, allPieces);

        result.Should().BeTrue();
        pawn.Position.Should().Be(enemy.Position, "Attacker should move to killed enemy position");
        enemy.IsAlive.Should().BeFalse("Target should be dead");
    }

    [Fact]
    public void UseAbility_LightArrow_WhenKillingEnemy_ShouldMoveAttackerToKilledPosition()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50);
        var bishop = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, 2, 2);
        bishop.Owner = owner;
        owner.Pieces.Add(bishop);

        var enemyOwner = new Player("P2", new List<Piece>());
        var enemy = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, 4, 4);
        enemy.Owner = enemyOwner;
        enemy.HP = 1;

        var allPieces = new List<Piece> { bishop, enemy };

        var testAbilityService = CreateAbilityServiceWithConfiguredMocks(new List<Piece> { enemy });

        var result = testAbilityService.UseAbility(bishop, "LightArrow", enemy.Position, allPieces);

        result.Should().BeTrue();
        bishop.Position.Should().Be(enemy.Position, "Attacker should move to killed enemy position");
        enemy.IsAlive.Should().BeFalse("Target should be dead");
    }

    [Fact]
    public void UseAbility_MagicExplosion_WhenKillingEnemies_ShouldNotMoveAttacker()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50);
        var queen = TestHelpers.CreatePiece(PieceType.Queen, Team.Elves, 3, 3);
        queen.Owner = owner;
        owner.Pieces.Add(queen);
        var originalPosition = queen.Position;

        var enemyOwner = new Player("P2", new List<Piece>());
        var enemy1 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, 4, 4);
        var enemy2 = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, 5, 5);
        enemy1.Owner = enemyOwner;
        enemy2.Owner = enemyOwner;
        enemy1.HP = 1;
        enemy2.HP = 1;

        var allPieces = new List<Piece> { queen, enemy1, enemy2 };

        var testAbilityService = CreateAbilityServiceWithConfiguredMocks(new List<Piece> { enemy1, enemy2 });

        var result = testAbilityService.UseAbility(queen, "MagicExplosion", new Position(4, 4), allPieces);

        result.Should().BeTrue();
        queen.Position.Should().Be(originalPosition, "AOE abilities should NOT move attacker");
        enemy1.IsAlive.Should().BeFalse("Enemy1 should be dead");
        enemy2.IsAlive.Should().BeFalse("Enemy2 should be dead");
    }

    [Fact]
    public void UseAbility_WhenNotKillingEnemy_ShouldNotMoveAttacker()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50);
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 2, 2);
        pawn.Owner = owner;
        owner.Pieces.Add(pawn);
        var originalPosition = pawn.Position;

        var enemyOwner = new Player("P2", new List<Piece>());
        var enemy = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, 3, 2);
        enemy.Owner = enemyOwner;
        enemy.HP = 100;

        var allPieces = new List<Piece> { pawn, enemy };

        var testAbilityService = CreateAbilityServiceWithConfiguredMocks(new List<Piece> { enemy });

        var result = testAbilityService.UseAbility(pawn, "ShieldBash", enemy.Position, allPieces);

        result.Should().BeTrue();
        pawn.Position.Should().Be(originalPosition, "Attacker should stay in original position when not killing");
        enemy.IsAlive.Should().BeTrue("Target should be alive");
    }

    #endregion

    private AbilityService CreateAbilityServiceWithMockedPieceDomainService()
    {
        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        var provider = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        var pieceDomainService = new Mock<IPieceDomainService>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<PieceKilledEvent>>)))
            .Returns(new List<IDomainEventHandler<PieceKilledEvent>>
            {
                new ExperienceAwardHandler(pieceDomainService.Object, provider),
                new BoardCleanupHandler(),
                new PositionSwapHandler()
            });

        var eventDispatcher = new DomainEventDispatcher(serviceProviderMock.Object);

        return new AbilityService(provider, eventDispatcher, pieceDomainService.Object);
    }

    private AbilityService CreateAbilityServiceWithConfiguredMocks(List<Piece> piecesToKill)
    {
        var abilityService = CreateAbilityServiceWithMockedPieceDomainService();
        var pieceDomainServiceMock = new Mock<IPieceDomainService>();

        foreach (var piece in piecesToKill)
        {
            pieceDomainServiceMock
                .Setup(x => x.TakeDamage(piece, It.IsAny<int>()))
                .Callback<Piece, int>((p, damage) => p.HP -= damage);
            pieceDomainServiceMock
                .Setup(x => x.IsDead(piece))
                .Returns(() => piece.HP <= 0);
        }

        pieceDomainServiceMock
            .Setup(x => x.SetAbilityCooldown(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<Piece, string, int>((piece, ability, cooldown) => piece.AbilityCooldowns[ability] = cooldown);
        pieceDomainServiceMock
            .Setup(x => x.Heal(It.IsAny<Piece>(), It.IsAny<int>()))
            .Callback<Piece, int>((piece, heal) => piece.HP += heal);
        pieceDomainServiceMock
            .Setup(x => x.GetMaxHP(It.IsAny<PieceType>()))
            .Returns<PieceType>(type => type switch
            {
                PieceType.Pawn => 10,
                PieceType.Bishop => 8,
                PieceType.Knight => 12,
                PieceType.Rook => 15,
                PieceType.Queen => 20,
                PieceType.King => 25,
                _ => 10
            });

        var configProvider = abilityService.GetType().GetField("_configProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(abilityService) as IBalanceConfigProvider;
        var eventDispatcher = abilityService.GetType().GetField("_eventDispatcher", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(abilityService) as IDomainEventDispatcher;

        return new AbilityService(configProvider, eventDispatcher, pieceDomainServiceMock.Object);
    }
}


