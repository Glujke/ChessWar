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
        // Arrange & Act
        var player = new Player("TestPlayer", new List<Piece>());

        // Assert
        player.MP.Should().Be(0);
        player.MaxMP.Should().Be(0);
    }

    [Fact]
    public void SetMana_ShouldSetInitialAndMaxMana()
    {
        // Arrange
        var player = new Player("TestPlayer", new List<Piece>());

        // Act
        player.SetMana(10, 50);

        // Assert
        player.MP.Should().Be(10);
        player.MaxMP.Should().Be(50);
    }

    [Fact]
    public void CanSpend_ShouldReturnTrue_WhenEnoughMana()
    {
        // Arrange
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);

        // Act & Assert
        player.CanSpend(10).Should().BeTrue();
        player.CanSpend(20).Should().BeTrue();
        player.CanSpend(21).Should().BeFalse();
    }

    [Fact]
    public void Spend_ShouldReduceMana_WhenEnoughMana()
    {
        // Arrange
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);

        // Act
        player.Spend(10);

        // Assert
        player.MP.Should().Be(10);
    }

    [Fact]
    public void Spend_ShouldThrowException_WhenNotEnoughMana()
    {
        // Arrange
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(5, 50);

        // Act & Assert
        var action = () => player.Spend(10);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Not enough mana to perform action.");
    }

    [Fact]
    public void Restore_ShouldIncreaseMana_UpToMax()
    {
        // Arrange
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(10, 50);

        // Act
        player.Restore(20);

        // Assert
        player.MP.Should().Be(30);
    }

    [Fact]
    public void Restore_ShouldNotExceedMaxMana()
    {
        // Arrange
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(45, 50);

        // Act
        player.Restore(20);

        // Assert
        player.MP.Should().Be(50);
    }

    [Fact]
    public void Restore_ShouldWorkWithZeroMana()
    {
        // Arrange
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(0, 50);

        // Act
        player.Restore(10);

        // Assert
        player.MP.Should().Be(10);
    }

    [Fact]
    public void Restore_ShouldWorkWithNegativeAmount()
    {
        // Arrange
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(20, 50);

        // Act
        player.Restore(-5);

        // Assert
        player.MP.Should().Be(15);
    }

    [Fact]
    public void Restore_ShouldNotGoBelowZero()
    {
        // Arrange
        var player = new Player("TestPlayer", new List<Piece>());
        player.SetMana(5, 50);

        // Act
        player.Restore(-10);

        // Assert
        player.MP.Should().Be(0);
    }
}
