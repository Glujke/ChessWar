using FluentAssertions;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Tests.Unit;

public class PositionTests
{
    [Fact]
    public void DistanceTo_ShouldCalculateChebyshevDistance()
    {
        var pos1 = new Position(0, 0);
        var pos2 = new Position(3, 4);

        var distance = pos1.DistanceTo(pos2);

        distance.Should().Be(4);
    }

    [Fact]
    public void DistanceTo_ShouldCalculateDiagonalDistance()
    {
        var pos1 = new Position(0, 0);
        var pos2 = new Position(5, 5);

        var distance = pos1.DistanceTo(pos2);

        distance.Should().Be(5);
    }

    [Fact]
    public void IsAdjacent_ShouldReturnTrueForAdjacentPositions()
    {
        var pos1 = new Position(0, 0);
        var pos2 = new Position(1, 0);

        pos1.IsAdjacent(pos2).Should().BeTrue();
    }

    [Fact]
    public void IsAdjacent_ShouldReturnFalseForNonAdjacentPositions()
    {
        var pos1 = new Position(0, 0);
        var pos2 = new Position(2, 0);

        pos1.IsAdjacent(pos2).Should().BeFalse();
    }

    [Fact]
    public void IsOnSameDiagonal_ShouldReturnTrueForDiagonalPositions()
    {
        var pos1 = new Position(0, 0);
        var pos2 = new Position(3, 3);

        pos1.IsOnSameDiagonal(pos2).Should().BeTrue();
    }

    [Fact]
    public void IsOnSameDiagonal_ShouldReturnFalseForNonDiagonalPositions()
    {
        var pos1 = new Position(0, 0);
        var pos2 = new Position(1, 2);

        pos1.IsOnSameDiagonal(pos2).Should().BeFalse();
    }

    [Fact]
    public void IsOnSameRow_ShouldReturnTrueForSameRow()
    {
        var pos1 = new Position(0, 5);
        var pos2 = new Position(3, 5);

        pos1.IsOnSameRow(pos2).Should().BeTrue();
    }

    [Fact]
    public void IsOnSameColumn_ShouldReturnTrueForSameColumn()
    {
        var pos1 = new Position(3, 0);
        var pos2 = new Position(3, 7);

        pos1.IsOnSameColumn(pos2).Should().BeTrue();
    }
}
