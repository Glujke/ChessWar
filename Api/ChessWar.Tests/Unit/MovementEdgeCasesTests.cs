using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.GameLogic;

namespace ChessWar.Tests.Unit;

public class MovementEdgeCasesTests
{
    private readonly IMovementRulesService _movementService;

    public MovementEdgeCasesTests()
    {
        var loggerMock = new Mock<ILogger<MovementRulesService>>();
        _movementService = new MovementRulesService(loggerMock.Object);
    }

    #region Corner Position Tests

    [Fact]
    public void KnightAtCorner_ShouldReturnOnlyValidMoves()
    {
        var knight = new Piece(PieceType.Knight, Team.Elves, new Position(0, 0));
        var boardPieces = new List<Piece> { knight };

        var moves = _movementService.GetPossibleMoves(knight, boardPieces);

        moves.Should().HaveCount(2);
        moves.Should().Contain(new Position(1, 2));
        moves.Should().Contain(new Position(2, 1));
    }

    [Fact]
    public void BishopAtCorner_ShouldReturnOnlyValidMoves()
    {
        var bishop = new Piece(PieceType.Bishop, Team.Elves, new Position(0, 0));
        var boardPieces = new List<Piece> { bishop };

        var moves = _movementService.GetPossibleMoves(bishop, boardPieces);

        moves.Should().HaveCount(7); // только одна диагональ
        moves.Should().Contain(new Position(1, 1));
        moves.Should().Contain(new Position(7, 7));
    }

    [Fact]
    public void RookAtCorner_ShouldReturnOnlyValidMoves()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0));
        var boardPieces = new List<Piece> { rook };

        var moves = _movementService.GetPossibleMoves(rook, boardPieces);

        moves.Should().HaveCount(14); // 7 горизонтально + 7 вертикально
        moves.Should().Contain(new Position(7, 0));
        moves.Should().Contain(new Position(0, 7));
    }

    [Fact]
    public void KingAtCorner_ShouldReturnOnlyValidMoves()
    {
        var king = new Piece(PieceType.King, Team.Elves, new Position(0, 0));
        var boardPieces = new List<Piece> { king };

        var moves = _movementService.GetPossibleMoves(king, boardPieces);

        moves.Should().HaveCount(3);
        moves.Should().Contain(new Position(0, 1));
        moves.Should().Contain(new Position(1, 0));
        moves.Should().Contain(new Position(1, 1));
    }

    #endregion

    #region Edge Position Tests

    [Fact]
    public void PieceAtEdge_ShouldNotIncludeOffBoardMoves()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(7, 7));
        var boardPieces = new List<Piece> { rook };

        var moves = _movementService.GetPossibleMoves(rook, boardPieces);

        moves.Should().NotContain(new Position(8, 7)); // за границей
        moves.Should().NotContain(new Position(7, 8)); // за границей
        moves.Should().Contain(new Position(0, 7)); // в пределах доски
        moves.Should().Contain(new Position(7, 0)); // в пределах доски
    }

    [Fact]
    public void CanMoveTo_MoveOffBoard_ShouldReturnFalse()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(7, 7));
        var boardPieces = new List<Piece> { pawn };

        var result = _movementService.CanMoveTo(pawn, new Position(8, 7), boardPieces);

        result.Should().BeFalse();
    }

    #endregion

    #region First Move Tests

    [Fact]
    public void PawnFirstMove_ShouldAllowTwoSquares()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        pawn.IsFirstMove = true;
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 3), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void PawnNotFirstMove_ShouldNotAllowTwoSquares()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        pawn.IsFirstMove = false;
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 3), boardPieces);

        canMove.Should().BeFalse();
    }

    #endregion

    #region Team Direction Tests

    [Fact]
    public void ElvesPawn_ShouldMoveUp()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 2), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void OrcsPawn_ShouldMoveDown()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 6));
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 5), boardPieces);

        canMove.Should().BeTrue();
    }

    [Fact]
    public void ElvesPawn_ShouldNotMoveDown()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 0), boardPieces);

        canMove.Should().BeFalse();
    }

    [Fact]
    public void OrcsPawn_ShouldNotMoveUp()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 6));
        var boardPieces = new List<Piece> { pawn };

        var canMove = _movementService.CanMoveTo(pawn, new Position(3, 7), boardPieces);

        canMove.Should().BeFalse();
    }

    #endregion

    #region Same Position Tests

    [Fact]
    public void CanMoveTo_SamePosition_ShouldReturnFalse()
    {
        var piece = new Piece(PieceType.King, Team.Elves, new Position(3, 3));
        var boardPieces = new List<Piece> { piece };

        var result = _movementService.CanMoveTo(piece, new Position(3, 3), boardPieces);

        result.Should().BeFalse();
    }

    #endregion
}
