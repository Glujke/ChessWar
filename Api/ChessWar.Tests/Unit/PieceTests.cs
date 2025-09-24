using FluentAssertions;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Tests.Unit;

public class PieceTests
{
    private readonly IPieceDomainService _pieceDomainService = new PieceDomainService();

    [Fact]
    public void Pawn_ShouldHaveCorrectStats()
    {
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);

        pawn.HP.Should().Be(10);
        pawn.ATK.Should().Be(2);
        pawn.Range.Should().Be(1);
        pawn.Movement.Should().Be(1);
        pawn.XP.Should().Be(0);
        pawn.XPToEvolve.Should().Be(20);
        pawn.CanEvolve.Should().BeFalse();
        pawn.IsAlive.Should().BeTrue();
    }

    [Fact]
    public void Knight_ShouldHaveCorrectStats()
    {
        var knight = TestHelpers.CreatePiece(PieceType.Knight, Team.Elves, 0, 0);

        knight.HP.Should().Be(20);
        knight.ATK.Should().Be(4);
        knight.Range.Should().Be(1);
        knight.Movement.Should().Be(1);
        knight.XP.Should().Be(0);
        knight.XPToEvolve.Should().Be(40);
        knight.CanEvolve.Should().BeFalse();
        knight.IsAlive.Should().BeTrue();
    }

    [Fact]
    public void Bishop_ShouldHaveCorrectStats()
    {
        var bishop = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, 0, 0);

        bishop.HP.Should().Be(18);
        bishop.ATK.Should().Be(3);
        bishop.Range.Should().Be(4);
        bishop.Movement.Should().Be(8);
        bishop.XP.Should().Be(0);
        bishop.XPToEvolve.Should().Be(40);
        bishop.CanEvolve.Should().BeFalse();
        bishop.IsAlive.Should().BeTrue();
    }

    [Fact]
    public void Rook_ShouldHaveCorrectStats()
    {
        var rook = TestHelpers.CreatePiece(PieceType.Rook, Team.Elves, 0, 0);

        rook.HP.Should().Be(25);
        rook.ATK.Should().Be(5);
        rook.Range.Should().Be(8);
        rook.Movement.Should().Be(8);
        rook.XP.Should().Be(0);
        rook.XPToEvolve.Should().Be(60);
        rook.CanEvolve.Should().BeFalse();
        rook.IsAlive.Should().BeTrue();
    }

    [Fact]
    public void Queen_ShouldHaveCorrectStats()
    {
        var queen = TestHelpers.CreatePiece(PieceType.Queen, Team.Elves, 0, 0);

        queen.HP.Should().Be(30);
        queen.ATK.Should().Be(7);
        queen.Range.Should().Be(3);
        queen.Movement.Should().Be(8);
        queen.XP.Should().Be(0);
        queen.XPToEvolve.Should().Be(0);
        queen.CanEvolve.Should().BeTrue();
        queen.IsAlive.Should().BeTrue();
    }

    [Fact]
    public void King_ShouldHaveCorrectStats()
    {
        var king = TestHelpers.CreatePiece(PieceType.King, Team.Elves, 0, 0);

        king.HP.Should().Be(50);
        king.ATK.Should().Be(3);
        king.Range.Should().Be(1);
        king.Movement.Should().Be(1);
        king.XP.Should().Be(0);
        king.XPToEvolve.Should().Be(0);
        king.CanEvolve.Should().BeTrue();
        king.IsAlive.Should().BeTrue();
    }

    [Fact]
    public void Pawn_ShouldEvolveWhenXPReached()
    {
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        _pieceDomainService.AddXP(pawn, 20);

        pawn.CanEvolve.Should().BeTrue();
    }

    [Fact]
    public void Pawn_ShouldDieWhenHPZero()
    {
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        _pieceDomainService.TakeDamage(pawn, 10);

        _pieceDomainService.IsDead(pawn).Should().BeTrue();
        pawn.HP.Should().Be(0);
    }

    [Fact]
    public void Piece_ShouldHealCorrectly()
    {
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        _pieceDomainService.TakeDamage(pawn, 5);

        _pieceDomainService.Heal(pawn, 3);

        pawn.HP.Should().Be(8);
    }

    [Fact]
    public void Piece_ShouldNotExceedMaxHP()
    {
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        pawn.HP = 9;

        _pieceDomainService.Heal(pawn, 5);

        pawn.HP.Should().Be(10);
    }

    [Fact]
    public void Piece_ShouldAddXP()
    {
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);

        _pieceDomainService.AddXP(pawn, 15);

        pawn.XP.Should().Be(15);
        pawn.CanEvolve.Should().BeFalse();
    }

    [Fact]
    public void Piece_ShouldTickCooldowns()
    {
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        _pieceDomainService.SetAbilityCooldown(pawn, "test_ability", 3);

        _pieceDomainService.TickCooldowns(pawn);

        pawn.AbilityCooldowns["test_ability"].Should().Be(2);
    }

    [Fact]
    public void Piece_ShouldRemoveZeroCooldowns()
    {
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        _pieceDomainService.SetAbilityCooldown(pawn, "test_ability", 1);

        _pieceDomainService.TickCooldowns(pawn);

        pawn.AbilityCooldowns["test_ability"].Should().Be(0);
        _pieceDomainService.CanUseAbility(pawn, "test_ability").Should().BeTrue();
    }
}
