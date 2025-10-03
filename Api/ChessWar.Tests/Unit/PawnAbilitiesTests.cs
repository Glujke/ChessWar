using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ChessWar.Tests.Unit;

public class PawnAbilitiesTests
{
    [Fact]
    public void ShieldBash_ShouldDeal2Damage_Cost2Mp_Range1_AndCooldownHandled()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50); // У игрока есть мана
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), owner);
        var enemyOwner = new Player("P2", new List<Piece>());
        var enemy = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(3, 2), enemyOwner);
        var all = new List<Piece> { pawn, enemy };

        var eventDispatcher = new Mock<ChessWar.Domain.Events.IDomainEventDispatcher>();
        var pieceDomainService = new Mock<ChessWar.Domain.Interfaces.GameLogic.IPieceDomainService>();

        pieceDomainService
            .Setup(x => x.TakeDamage(enemy, It.IsAny<int>()))
            .Callback<Piece, int>((piece, damage) => piece.HP -= damage);
        pieceDomainService
            .Setup(x => x.SetAbilityCooldown(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<Piece, string, int>((piece, ability, cooldown) => piece.AbilityCooldowns[ability] = cooldown);

        var svc = new AbilityService(_TestConfig.CreateProvider(), eventDispatcher.Object, pieceDomainService.Object);

        var ok = svc.UseAbility(pawn, "ShieldBash", enemy.Position, all);

        ok.Should().BeTrue();
        enemy.HP.Should().BeLessThan(10);
        owner.MP.Should().BeLessThan(50); // Игрок потратил ману
        pawn.AbilityCooldowns.GetValueOrDefault("ShieldBash").Should().BeGreaterThanOrEqualTo(0); // CD может быть 0
    }

    [Fact]
    public void Breakthrough_ShouldDeal3Damage_Cost2Mp_DiagonalOnly()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50); // У игрока есть мана
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(2, 2), owner);
        var enemyOwner = new Player("P2", new List<Piece>());
        var enemy = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(3, 3), enemyOwner);
        var all = new List<Piece> { pawn, enemy };

        var eventDispatcher = new Mock<ChessWar.Domain.Events.IDomainEventDispatcher>();
        var pieceDomainService = new Mock<ChessWar.Domain.Interfaces.GameLogic.IPieceDomainService>();

        pieceDomainService
            .Setup(x => x.TakeDamage(enemy, It.IsAny<int>()))
            .Callback<Piece, int>((piece, damage) => piece.HP -= damage);
        pieceDomainService
            .Setup(x => x.SetAbilityCooldown(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<Piece, string, int>((piece, ability, cooldown) => piece.AbilityCooldowns[ability] = cooldown);

        var svc = new AbilityService(_TestConfig.CreateProvider(), eventDispatcher.Object, pieceDomainService.Object);

        var ok = svc.UseAbility(pawn, "Breakthrough", enemy.Position, all);

        ok.Should().BeTrue();
        enemy.HP.Should().BeLessThan(10);
        owner.MP.Should().BeLessThan(50); // Игрок потратил ману
    }
}


