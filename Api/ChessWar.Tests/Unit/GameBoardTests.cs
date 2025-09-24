using FluentAssertions;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Tests.Unit;

public class GameBoardTests
{
    [Fact]
    public void IsValidPosition_ShouldReturnTrueForValidPositions()
    {
        var board = new GameBoard();
        var validPositions = new[]
        {
            new Position(0, 0),
            new Position(7, 7),
            new Position(3, 4)
        };

        foreach (var pos in validPositions)
        {
            board.IsValidPosition(pos).Should().BeTrue();
        }
    }

    [Fact]
    public void IsValidPosition_ShouldReturnFalseForInvalidPositions()
    {
        var board = new GameBoard();
        var invalidPositions = new[]
        {
            new Position(-1, 0),
            new Position(0, -1),
            new Position(8, 0),
            new Position(0, 8),
            new Position(10, 10)
        };

        foreach (var pos in invalidPositions)
        {
            board.IsValidPosition(pos).Should().BeFalse();
        }
    }

    [Fact]
    public void IsEmpty_ShouldReturnTrueForEmptyPositions()
    {
        var board = new GameBoard();
        var position = new Position(3, 4);

        board.IsEmpty(position).Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_ShouldReturnFalseForOccupiedPositions()
    {
        var board = new GameBoard();
        var position = new Position(3, 4);
        var piece = new Piece(PieceType.Pawn, Team.Elves, position);
        board.SetPieceAt(position, piece);

        board.IsEmpty(position).Should().BeFalse();
    }

    [Fact]
    public void SetPieceAt_ShouldPlacePieceOnBoard()
    {
        var board = new GameBoard();
        var position = new Position(3, 4);
        var piece = new Piece(PieceType.Pawn, Team.Elves, position);

        board.SetPieceAt(position, piece);

        board.GetPieceAt(position).Should().Be(piece);
    }

    [Fact]
    public void MovePiece_ShouldMovePieceToNewPosition()
    {
        var board = new GameBoard();
        var oldPosition = new Position(0, 0);
        var newPosition = new Position(3, 4);
        var piece = new Piece(PieceType.Pawn, Team.Elves, oldPosition);
        board.SetPieceAt(oldPosition, piece);

        board.MovePiece(piece, newPosition);

        board.GetPieceAt(oldPosition).Should().BeNull();
        board.GetPieceAt(newPosition).Should().Be(piece);
        piece.Position.Should().Be(newPosition);
    }

    [Fact]
    public void GetAllPieces_ShouldReturnAllPiecesOnBoard()
    {
        var board = new GameBoard();
        var piece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        var piece2 = new Piece(PieceType.Knight, Team.Orcs, new Position(1, 1));
        board.SetPieceAt(new Position(0, 0), piece1);
        board.SetPieceAt(new Position(1, 1), piece2);

        var pieces = board.GetAllPieces();

        pieces.Should().HaveCount(2);
        pieces.Should().Contain(piece1);
        pieces.Should().Contain(piece2);
    }

    [Fact]
    public void GetPiecesByTeam_ShouldReturnOnlyPiecesOfSpecificTeam()
    {
        var board = new GameBoard();
        var elfPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        var orcPiece = new Piece(PieceType.Knight, Team.Orcs, new Position(1, 1));
        board.SetPieceAt(new Position(0, 0), elfPiece);
        board.SetPieceAt(new Position(1, 1), orcPiece);

        var elfPieces = board.GetPiecesByTeam(Team.Elves);
        var orcPieces = board.GetPiecesByTeam(Team.Orcs);

        elfPieces.Should().HaveCount(1);
        elfPieces.Should().Contain(elfPiece);
        orcPieces.Should().HaveCount(1);
        orcPieces.Should().Contain(orcPiece);
    }

    [Fact]
    public void GetAlivePieces_ShouldReturnOnlyAlivePieces()
    {
        var board = new GameBoard();
        var alivePiece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        var deadPiece = TestHelpers.CreatePiece(PieceType.Knight, Team.Orcs, 1, 1);
        TestHelpers.TakeDamage(deadPiece, 20);
        board.SetPieceAt(new Position(0, 0), alivePiece);
        board.SetPieceAt(new Position(1, 1), deadPiece);

        var alivePieces = board.GetAlivePieces();

        alivePieces.Should().HaveCount(1);
        alivePieces.Should().Contain(alivePiece);
        alivePieces.Should().NotContain(deadPiece);
    }
}
