using ChessWar.Domain.Entities;
using FluentAssertions;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для системы маны игрока
/// </summary>
public class PlayerManaTests
{
    [Fact]
    public void Player_ShouldInitializeWithZeroMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());

        player.MP.Should().Be(0);
        player.MaxMP.Should().Be(0);
    }

    [Fact]
    public void SetMana_ShouldSetInitialAndMaxMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());

        player.SetMana(10, 50);

        player.MP.Should().Be(10);
        player.MaxMP.Should().Be(50);
    }

    [Fact]
    public void CanSpend_ShouldReturnTrue_WhenEnoughMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);

        player.CanSpend(10).Should().BeTrue();
        player.CanSpend(20).Should().BeTrue();
        player.CanSpend(21).Should().BeFalse();
    }

    [Fact]
    public void Spend_ShouldReduceMana_WhenEnoughMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);

        player.Spend(10);

        player.MP.Should().Be(10);
    }

    [Fact]
    public void Spend_ShouldThrowException_WhenNotEnoughMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(5, 50);

        var action = () => player.Spend(10);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Not enough mana to perform action.");
    }

    [Fact]
    public void Restore_ShouldIncreaseMana_UpToMax()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(10, 50);

        player.Restore(20);

        player.MP.Should().Be(30);
    }

    [Fact]
    public void Restore_ShouldNotExceedMaxMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(45, 50);

        player.Restore(20);

        player.MP.Should().Be(50);
    }

    [Fact]
    public void Restore_ShouldWorkWithZeroMana()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(0, 50);

        player.Restore(10);

        player.MP.Should().Be(10);
    }

    [Fact]
    public void Restore_ShouldWorkWithNegativeAmount()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);

        player.Restore(-5);

        player.MP.Should().Be(15);
    }

    [Fact]
    public void Restore_ShouldNotGoBelowZero()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(5, 50);

        player.Restore(-10);

        player.MP.Should().Be(0);
    }
}
