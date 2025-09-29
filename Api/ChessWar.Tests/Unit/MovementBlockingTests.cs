using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.GameLogic;

namespace ChessWar.Tests.Unit;

public class MovementBlockingTests
{
    private readonly IMovementRulesService _movementService;

    public MovementBlockingTests()
    {
        var loggerMock = new Mock<ILogger<MovementRulesService>>();
        _movementService = new MovementRulesService(loggerMock.Object);
    }

    #region Pawn Blocking Tests

    [Fact]
    public void Pawn_ShouldNotMoveIfBlocked()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        var blocker = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 2));
        var boardPieces = new List<Piece> { pawn, blocker };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 2), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Pawn_ShouldNotMoveTwoSquaresIfBlocked()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        var blocker = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 2));
        var boardPieces = new List<Piece> { pawn, blocker };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 3), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Pawn_ShouldNotMoveTwoSquaresIfBlockedByAlly()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        var blocker = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        var boardPieces = new List<Piece> { pawn, blocker };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 3), boardPieces);

        canMove.Should().BeFalse();
    }

    #endregion

    #region Bishop Blocking Tests

    [Fact]
    public void Bishop_ShouldNotMoveIfBlocked()
    {
        var bishop = new Piece(PieceType.Bishop, Team.Elves, new Position(2, 2));
        var blocker = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 4));
        var boardPieces = new List<Piece> { bishop, blocker };

        var canMove = _movementService.CanMoveTo(bishop, new Position(5, 5), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Bishop_ShouldNotMoveIfBlockedByAlly()
    {
        var bishop = new Piece(PieceType.Bishop, Team.Elves, new Position(2, 2));
        var blocker = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 4));
        var boardPieces = new List<Piece> { bishop, blocker };

        var canMove = _movementService.CanMoveTo(bishop, new Position(5, 5), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Bishop_ShouldMoveToBlockedPositionIfEnemy()
    {
        var bishop = new Piece(PieceType.Bishop, Team.Elves, new Position(2, 2));
        var enemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 4));
        var boardPieces = new List<Piece> { bishop, enemy };

        var canMove = _movementService.CanMoveTo(bishop, new Position(4, 4), boardPieces);

        canMove.Should().BeTrue();
    }

    #endregion

    #region Rook Blocking Tests

    [Fact]
    public void Rook_ShouldNotMoveIfBlocked()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(3, 3));
        var blocker = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 3));
        var boardPieces = new List<Piece> { rook, blocker };

        var canMove = _movementService.CanMoveTo(rook, new Position(6, 3), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Rook_ShouldNotMoveIfBlockedByAlly()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(3, 3));
        var blocker = new Piece(PieceType.Pawn, Team.Elves, new Position(5, 3));
        var boardPieces = new List<Piece> { rook, blocker };

        var canMove = _movementService.CanMoveTo(rook, new Position(6, 3), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Rook_ShouldMoveToBlockedPositionIfEnemy()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(3, 3));
        var enemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 3));
        var boardPieces = new List<Piece> { rook, enemy };

        var canMove = _movementService.CanMoveTo(rook, new Position(5, 3), boardPieces);

        canMove.Should().BeTrue();
    }

    #endregion

    #region Queen Blocking Tests

    [Fact]
    public void Queen_ShouldNotMoveIfBlockedHorizontally()
    {
        var queen = new Piece(PieceType.Queen, Team.Elves, new Position(3, 3));
        var blocker = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 3));
        var boardPieces = new List<Piece> { queen, blocker };

        var canMove = _movementService.CanMoveTo(queen, new Position(6, 3), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Queen_ShouldNotMoveIfBlockedDiagonally()
    {
        var queen = new Piece(PieceType.Queen, Team.Elves, new Position(3, 3));
        var blocker = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 5));
        var boardPieces = new List<Piece> { queen, blocker };

        var canMove = _movementService.CanMoveTo(queen, new Position(6, 6), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Queen_ShouldMoveToBlockedPositionIfEnemy()
    {
        var queen = new Piece(PieceType.Queen, Team.Elves, new Position(3, 3));
        var enemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 3));
        var boardPieces = new List<Piece> { queen, enemy };

        var canMove = _movementService.CanMoveTo(queen, new Position(5, 3), boardPieces);

        canMove.Should().BeTrue();
    }

    #endregion

    #region Knight Blocking Tests

    [Fact]
    public void Knight_ShouldJumpOverPieces()
    {
        var knight = new Piece(PieceType.Knight, Team.Elves, new Position(3, 3));
        var blocker = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 3));
        var boardPieces = new List<Piece> { knight, blocker };

        var canMove = _movementService.CanMoveTo(knight, new Position(5, 4), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void Knight_ShouldNotMoveToAllyPosition()
    {
        var knight = new Piece(PieceType.Knight, Team.Elves, new Position(3, 3));
        var ally = new Piece(PieceType.Pawn, Team.Elves, new Position(5, 4));
        var boardPieces = new List<Piece> { knight, ally };

        var canMove = _movementService.CanMoveTo(knight, new Position(5, 4), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Knight_ShouldMoveToEnemyPosition()
    {
        var knight = new Piece(PieceType.Knight, Team.Elves, new Position(3, 3));
        var enemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 4));
        var boardPieces = new List<Piece> { knight, enemy };

        var canMove = _movementService.CanMoveTo(knight, new Position(5, 4), boardPieces);

        canMove.Should().BeTrue();
    }

    #endregion

    #region King Blocking Tests

    [Fact]
    public void King_ShouldNotMoveToAllyPosition()
    {
        var king = new Piece(PieceType.King, Team.Elves, new Position(3, 3));
        var ally = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 3));
        var boardPieces = new List<Piece> { king, ally };

        var canMove = _movementService.CanMoveTo(king, new Position(4, 3), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void King_ShouldMoveToEnemyPosition()
    {
        var king = new Piece(PieceType.King, Team.Elves, new Position(3, 3));
        var enemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 3));
        var boardPieces = new List<Piece> { king, enemy };

        var canMove = _movementService.CanMoveTo(king, new Position(4, 3), boardPieces);

        canMove.Should().BeTrue();
    }

    #endregion

    #region Complex Blocking Scenarios

    [Fact]
    public void MultipleBlockers_ShouldBlockCorrectly()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0));
        var blocker1 = new Piece(PieceType.Pawn, Team.Orcs, new Position(2, 0));
        var blocker2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 0));
        var boardPieces = new List<Piece> { rook, blocker1, blocker2 };

        var canMove = _movementService.CanMoveTo(rook, new Position(6, 0), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void PartialBlocking_ShouldAllowPartialMovement()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0));
        var blocker = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 0));
        var boardPieces = new List<Piece> { rook, blocker };

        var canMove = _movementService.CanMoveTo(rook, new Position(3, 0), boardPieces);

        canMove.Should().BeTrue();
    }

    #endregion
}
