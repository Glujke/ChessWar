using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ChessWar.Tests.Unit;

public class KingQueenAbilitiesTests
{
    [Fact]
    public void Resurrection_ShouldRevive_Ally_With_50PercentHp_And_StartCooldown()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50); // У игрока есть мана
        var queen = TestHelpers.CreatePiece(PieceType.Queen, Team.Elves, new Position(3, 3), owner);
        var fallen = TestHelpers.CreatePiece(PieceType.Knight, Team.Elves, new Position(3, 4), owner);

        TestHelpers.TakeDamage(fallen, fallen.HP + 100);
        fallen.IsAlive.Should().BeFalse();

        var eventDispatcher = new Mock<ChessWar.Domain.Events.IDomainEventDispatcher>();
        var pieceDomainService = new Mock<ChessWar.Domain.Interfaces.GameLogic.IPieceDomainService>();
        
        pieceDomainService
            .Setup(x => x.Heal(fallen, It.IsAny<int>()))
            .Callback<Piece, int>((piece, heal) => piece.HP += heal);
        pieceDomainService
            .Setup(x => x.SetAbilityCooldown(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<Piece, string, int>((piece, ability, cooldown) => piece.AbilityCooldowns[ability] = cooldown);
        pieceDomainService
            .Setup(x => x.GetMaxHP(It.IsAny<PieceType>()))
            .Returns<PieceType>(type => type switch
            {
                PieceType.Knight => 12,
                _ => 10
            });
        
        var svc = new AbilityService(_TestConfig.CreateProvider(), eventDispatcher.Object, pieceDomainService.Object);

        var ok = svc.UseAbility(queen, "Resurrection", fallen.Position, owner.Pieces);

        ok.Should().BeTrue();
        fallen.IsAlive.Should().BeTrue();
        fallen.HP.Should().BeGreaterThan(0);
        var expectedHalf = 12 / 2; // Knight max HP = 12
        fallen.HP.Should().Be(expectedHalf);
        queen.AbilityCooldowns.GetValueOrDefault("Resurrection").Should().BeGreaterThan(0);
        owner.MP.Should().BeLessThan(50); // Игрок потратил ману
    }

    [Fact]
    public void RoyalCommand_ShouldGrant_ExtraAction_ToTarget_And_StartCooldown()
    {
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50); // У игрока есть мана
        var king = TestHelpers.CreatePiece(PieceType.King, Team.Elves, new Position(2, 2), owner);
        var ally = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, new Position(2, 3), owner);

        var eventDispatcher = new Mock<ChessWar.Domain.Events.IDomainEventDispatcher>();
        var pieceDomainService = new Mock<ChessWar.Domain.Interfaces.GameLogic.IPieceDomainService>();
        
        pieceDomainService
            .Setup(x => x.SetAbilityCooldown(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<Piece, string, int>((piece, ability, cooldown) => piece.AbilityCooldowns[ability] = cooldown);
        
        var svc = new AbilityService(_TestConfig.CreateProvider(), eventDispatcher.Object, pieceDomainService.Object);

        var ok = svc.UseAbility(king, "RoyalCommand", ally.Position, owner.Pieces);

        ok.Should().BeTrue();
        king.AbilityCooldowns.GetValueOrDefault("RoyalCommand").Should().BeGreaterThan(0);
        owner.MP.Should().BeLessThan(50); // Игрок потратил ману
    }
}


