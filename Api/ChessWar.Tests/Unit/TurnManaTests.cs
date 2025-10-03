using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для системы маны в ходе
/// </summary>
public class TurnManaTests
{
    [Fact]
    public void Turn_ShouldInitializeWithPlayerMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(15, 50);

        var turn = new Turn(1, player);

        turn.RemainingMP.Should().Be(15);
    }

    [Fact]
    public void CanAfford_ShouldReturnTrue_WhenEnoughMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);
        var turn = new Turn(1, player);

        turn.CanAfford(10).Should().BeTrue();
        turn.CanAfford(20).Should().BeTrue();
        turn.CanAfford(21).Should().BeFalse();
    }

    [Fact]
    public void SpendMP_ShouldReduceRemainingMana_WhenEnoughMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);
        var turn = new Turn(1, player);

        var result = turn.SpendMP(10);

        result.Should().BeTrue();
        turn.RemainingMP.Should().Be(10);
    }

    [Fact]
    public void SpendMP_ShouldReturnFalse_WhenNotEnoughMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(5, 50);
        var turn = new Turn(1, player);

        var result = turn.SpendMP(10);

        result.Should().BeFalse();
        turn.RemainingMP.Should().Be(5); // Не изменилось
    }

    [Fact]
    public void UpdateRemainingMP_ShouldSyncWithPlayerMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);
        var turn = new Turn(1, player);

        player.Spend(5);

        turn.UpdateRemainingMP();

        turn.RemainingMP.Should().Be(15);
    }

    [Fact]
    public void MultipleSpendMP_ShouldWorkCorrectly()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(30, 50);
        var turn = new Turn(1, player);

        turn.SpendMP(10);
        turn.SpendMP(5);
        turn.SpendMP(15);

        turn.RemainingMP.Should().Be(0);
        turn.CanAfford(1).Should().BeFalse();
    }

    [Fact]
    public void SpendMP_ShouldNotAffectPlayerMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);
        var turn = new Turn(1, player);

        turn.SpendMP(10);

        turn.RemainingMP.Should().Be(10);
        player.MP.Should().Be(20); // Игрок не изменился
    }
}
