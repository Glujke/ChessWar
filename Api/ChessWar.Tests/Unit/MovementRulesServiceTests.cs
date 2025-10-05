using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.GameLogic;

namespace ChessWar.Tests.Unit;

public class MovementRulesServiceTests
{
    private readonly IMovementRulesService _movementService;

    public MovementRulesServiceTests()
    {
        var loggerMock = new Mock<ILogger<MovementRulesService>>();
        _movementService = new MovementRulesService(loggerMock.Object);
    }

    #region Board Validation Tests

    [Fact]
    public void IsValidPosition_ShouldReturnTrue_ForValidPositions()
    {
        var validPositions = new[]
        {
            new Position(0, 0),
            new Position(7, 7),
            new Position(3, 4),
            new Position(0, 7),
            new Position(7, 0)
        };

        foreach (var position in validPositions)
        {
            _movementService.IsValidPosition(position).Should().BeTrue($"Position {position} should be valid");
        }
    }

    [Fact]
    public void IsValidPosition_ShouldReturnFalse_ForInvalidPositions()
    {
        var invalidPositions = new[]
        {
            new Position(-1, 0),
            new Position(0, -1),
            new Position(8, 0),
            new Position(0, 8),
            new Position(-1, -1),
            new Position(8, 8)
        };

        foreach (var position in invalidPositions)
        {
            _movementService.IsValidPosition(position).Should().BeFalse($"Position {position} should be invalid");
        }
    }

    #endregion

    #region Pawn Movement Tests

    [Fact]
    public void Pawn_ShouldMoveOneSquareForward()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 2), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void Pawn_ShouldMoveTwoSquaresOnFirstMove()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 3), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void Pawn_ShouldNotMoveTwoSquaresAfterFirstMove()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        pawn.IsFirstMove = false;
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 4), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Pawn_ShouldNotMoveBackwards()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 1), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Pawn_ShouldNotMoveDiagonallyWithoutEnemy()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(4, 3), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void Pawn_ShouldNotMoveDiagonallyWithoutEnemy_E2ECase()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 3));
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(1, 4), boardPieces);

        canMove.Should().BeFalse("Pawn should not move diagonally without enemy");
    }

    [Fact]
    public void Pawn_ShouldNotMoveDiagonally_WhenNoEnemy()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        var boardPieces = new List<Piece> { pawn };

        _movementService.CanMoveTo(pawn, new Position(2, 3), boardPieces).Should().BeFalse("Left diagonal without enemy");
        _movementService.CanMoveTo(pawn, new Position(4, 3), boardPieces).Should().BeFalse("Right diagonal without enemy");
    }

    [Fact]
    public void Pawn_ShouldNotMoveDiagonally_EvenWithEnemy()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        var enemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 3));
        var boardPieces = new List<Piece> { pawn, enemy };

        _movementService.CanMoveTo(pawn, new Position(4, 3), boardPieces).Should().BeFalse("Cannot move diagonally even with enemy");
        _movementService.CanMoveTo(pawn, new Position(2, 3), boardPieces).Should().BeFalse("Cannot move to empty diagonal");
    }

    [Fact]
    public void Pawn_ShouldNotOccupyEnemyPosition_ByMoving()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        var enemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 3));
        var boardPieces = new List<Piece> { pawn, enemy };

        var canMove = _movementService.CanMoveTo(pawn, new Position(4, 3), boardPieces);

        canMove.Should().BeFalse("Pawn cannot move to enemy position - must use ability to attack");
    }

    [Fact]
    public void Pawn_ShouldNotMoveDiagonally_ToAllyPosition()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        var ally = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 3));
        var boardPieces = new List<Piece> { pawn, ally };

        var canMove = _movementService.CanMoveTo(pawn, new Position(4, 3), boardPieces);

        canMove.Should().BeFalse("Pawn cannot attack ally");
    }

    [Fact]
    public void Pawn_E2E_ExactCase_ShouldNotMoveDiagonally()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 3));
        var boardPieces = new List<Piece> { pawn };

        var possibleMoves = _movementService.GetPossibleMoves(pawn, boardPieces);
        var canMoveDiagonally = _movementService.CanMoveTo(pawn, new Position(1, 4), boardPieces);

        possibleMoves.Should().NotContain(new Position(1, 4), "Diagonal move should not be in possible moves");
        canMoveDiagonally.Should().BeFalse("Should not be able to move diagonally without enemy");

        possibleMoves.Should().Contain(new Position(0, 4), "Should be able to move forward");
    }

    [Fact]
    public void Pawn_ShouldNotMoveToSameTeamPiece()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        var ally = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 3));
        var boardPieces = new List<Piece> { pawn, ally };

        var canMove = _movementService.CanMoveTo(pawn, new Position(4, 3), boardPieces);

        canMove.Should().BeFalse();
    }

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
    public void GetPossibleMoves_ShouldReturnCorrectMoves_ForPawn()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        var boardPieces = new List<Piece> { pawn };

        var possibleMoves = _movementService.GetPossibleMoves(pawn, boardPieces);

        possibleMoves.Should().HaveCount(2);
        possibleMoves.Should().Contain(new Position(3, 2));
        possibleMoves.Should().Contain(new Position(3, 3));
    }

    [Fact]
    public void GetPossibleMoves_ShouldReturnCorrectMoves_ForPawnAfterFirstMove()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2));
        pawn.IsFirstMove = false;
        var boardPieces = new List<Piece> { pawn };

        var possibleMoves = _movementService.GetPossibleMoves(pawn, boardPieces);

        possibleMoves.Should().HaveCount(1);
        possibleMoves.Should().Contain(new Position(3, 3));
    }

    #endregion

    #region Knight Movement Tests

    [Fact]
    public void Knight_ShouldMoveLShaped()
    {
        var knight = new Piece(PieceType.Knight, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { knight };

        var canMove = _movementService.CanMoveTo(knight, new Position(5, 4), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void Knight_ShouldNotMoveInvalidPattern()
    {
        var knight = new Piece(PieceType.Knight, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { knight };

        var canMove = _movementService.CanMoveTo(knight, new Position(5, 5), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void GetPossibleMoves_Knight_ShouldReturnAllLShapedMoves()
    {
        var knight = new Piece(PieceType.Knight, Team.Elves, new Position(4, 4));
        var boardPieces = new List<Piece> { knight };

        var moves = _movementService.GetPossibleMoves(knight, boardPieces);

        moves.Should().HaveCount(8);
        moves.Should().Contain(new Position(2, 3));
        moves.Should().Contain(new Position(2, 5));
        moves.Should().Contain(new Position(3, 2));
        moves.Should().Contain(new Position(3, 6));
        moves.Should().Contain(new Position(5, 2));
        moves.Should().Contain(new Position(5, 6));
        moves.Should().Contain(new Position(6, 3));
        moves.Should().Contain(new Position(6, 5));
    }

    #endregion

    #region Bishop Movement Tests

    [Fact]
    public void Bishop_ShouldMoveDiagonally()
    {
        var bishop = new Piece(PieceType.Bishop, Team.Elves, new Position(2, 2));
        var boardPieces = new List<Piece> { bishop };

        var canMove = _movementService.CanMoveTo(bishop, new Position(5, 5), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void Bishop_ShouldNotMoveHorizontally()
    {
        var bishop = new Piece(PieceType.Bishop, Team.Elves, new Position(2, 2));
        var boardPieces = new List<Piece> { bishop };

        var canMove = _movementService.CanMoveTo(bishop, new Position(5, 2), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void GetPossibleMoves_Bishop_ShouldReturnAllDiagonalMoves()
    {
        var bishop = new Piece(PieceType.Bishop, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { bishop };

        var moves = _movementService.GetPossibleMoves(bishop, boardPieces);

        moves.Should().HaveCount(13);
        moves.Should().Contain(new Position(0, 0));
        moves.Should().Contain(new Position(7, 7));
        moves.Should().Contain(new Position(0, 6));
        moves.Should().Contain(new Position(6, 0));
    }

    #endregion

    #region Rook Movement Tests

    [Fact]
    public void Rook_ShouldMoveHorizontally()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { rook };

        var canMove = _movementService.CanMoveTo(rook, new Position(6, 3), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void Rook_ShouldMoveVertically()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { rook };

        var canMove = _movementService.CanMoveTo(rook, new Position(3, 6), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void Rook_ShouldNotMoveDiagonally()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { rook };

        var canMove = _movementService.CanMoveTo(rook, new Position(6, 6), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void GetPossibleMoves_Rook_ShouldReturnAllHorizontalAndVerticalMoves()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { rook };

        var moves = _movementService.GetPossibleMoves(rook, boardPieces);

        moves.Should().HaveCount(14);
        moves.Should().Contain(new Position(0, 3));
        moves.Should().Contain(new Position(7, 3));
        moves.Should().Contain(new Position(3, 0));
        moves.Should().Contain(new Position(3, 7));
    }

    #endregion

    #region Queen Movement Tests

    [Fact]
    public void Queen_ShouldMoveLikeRook()
    {
        var queen = new Piece(PieceType.Queen, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { queen };

        var canMove = _movementService.CanMoveTo(queen, new Position(6, 3), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void Queen_ShouldMoveLikeBishop()
    {
        var queen = new Piece(PieceType.Queen, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { queen };

        var canMove = _movementService.CanMoveTo(queen, new Position(6, 6), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void GetPossibleMoves_Queen_ShouldReturnAllMoves()
    {
        var queen = new Piece(PieceType.Queen, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { queen };

        var moves = _movementService.GetPossibleMoves(queen, boardPieces);

        moves.Should().HaveCount(27);
    }

    #endregion

    #region King Movement Tests

    [Fact]
    public void King_ShouldMoveOneSquare()
    {
        var king = new Piece(PieceType.King, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { king };

        var canMove = _movementService.CanMoveTo(king, new Position(4, 3), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void King_ShouldNotMoveTwoSquares()
    {
        var king = new Piece(PieceType.King, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { king };

        var canMove = _movementService.CanMoveTo(king, new Position(5, 3), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void GetPossibleMoves_King_ShouldReturnOneSquareMoves()
    {
        var king = new Piece(PieceType.King, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { king };

        var moves = _movementService.GetPossibleMoves(king, boardPieces);

        moves.Should().HaveCount(8);
        moves.Should().Contain(new Position(2, 2));
        moves.Should().Contain(new Position(2, 3));
        moves.Should().Contain(new Position(2, 4));
        moves.Should().Contain(new Position(3, 2));
        moves.Should().Contain(new Position(3, 4));
        moves.Should().Contain(new Position(4, 2));
        moves.Should().Contain(new Position(4, 3));
        moves.Should().Contain(new Position(4, 4));
    }

    #endregion
}
