using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ChessWar.Tests.Integration;

public class BoardServiceIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly IBoardService _boardService;

    public BoardServiceIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
        _boardService = _scope.ServiceProvider.GetRequiredService<IBoardService>();
    }

    [Fact]
    public async Task GetBoard_WithEmptyBoard_ShouldReturnEmptyBoard()
    {
        // Act
        var result = await _boardService.GetBoardAsync();

        // Assert
        result.Should().NotBeNull();
        result.Pieces.Should().BeEmpty();
        GameBoard.Size.Should().Be(8);
    }

    [Fact]
    public async Task GetBoard_WithPieces_ShouldReturnBoardWithPieces()
    {
        // Arrange
        await _boardService.PlacePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));
        await _boardService.PlacePieceAsync(PieceType.Knight, Team.Orcs, new Position(2, 2));

        // Act
        var result = await _boardService.GetBoardAsync();

        // Assert
        result.Should().NotBeNull();
        result.Pieces.Should().HaveCount(2);
        GameBoard.Size.Should().Be(8);
    }

    [Fact]
    public async Task ResetBoard_ShouldRemoveAllPieces()
    {
        // Arrange
        await _boardService.PlacePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));
        await _boardService.PlacePieceAsync(PieceType.Knight, Team.Orcs, new Position(2, 2));

        // Act
        await _boardService.ResetBoardAsync();

        // Assert
        var result = await _boardService.GetBoardAsync();
        result.Pieces.Should().BeEmpty();
    }

    [Fact]
    public async Task SetupInitialPosition_ShouldCreateStandardSetup()
    {
        // Act
        await _boardService.SetupInitialPositionAsync();

        // Assert
        var result = await _boardService.GetBoardAsync();
        result.Pieces.Should().HaveCount(18); // 8 pawns + 1 king for each team
        
        // Check that we have pieces for both teams
        var elvesPieces = result.Pieces.Where(p => p.Team == Team.Elves).ToList();
        var orcsPieces = result.Pieces.Where(p => p.Team == Team.Orcs).ToList();
        
        elvesPieces.Should().HaveCount(9); // 8 pawns + 1 king
        orcsPieces.Should().HaveCount(9); // 8 pawns + 1 king
        
        // Check that we have pawns and kings
        var elvesPawns = elvesPieces.Where(p => p.Type == PieceType.Pawn).ToList();
        var elvesKings = elvesPieces.Where(p => p.Type == PieceType.King).ToList();
        var orcsPawns = orcsPieces.Where(p => p.Type == PieceType.Pawn).ToList();
        var orcsKings = orcsPieces.Where(p => p.Type == PieceType.King).ToList();
        
        elvesPawns.Should().HaveCount(8);
        elvesKings.Should().HaveCount(1);
        orcsPawns.Should().HaveCount(8);
        orcsKings.Should().HaveCount(1);
    }

    [Fact]
    public async Task PlacePiece_WithValidData_ShouldPlacePiece()
    {
        // Arrange
        var type = PieceType.Pawn;
        var team = Team.Elves;
        var position = new Position(1, 1);

        // Act
        var result = await _boardService.PlacePieceAsync(type, team, position);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(type);
        result.Team.Should().Be(team);
        result.Position.Should().Be(position);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PlacePiece_WithOccupiedPosition_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var position = new Position(1, 1);
        await _boardService.PlacePieceAsync(PieceType.Pawn, Team.Elves, position);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _boardService.PlacePieceAsync(PieceType.Knight, Team.Orcs, position));
    }

    [Fact]
    public async Task MovePiece_WithValidData_ShouldMovePiece()
    {
        // Arrange
        var piece = await _boardService.PlacePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));
        var newPosition = new Position(3, 3);

        // Act
        var result = await _boardService.MovePieceAsync(piece.Id, newPosition);

        // Assert
        result.Position.Should().Be(newPosition);
        
        // Verify the piece is no longer at the old position
        var pieceAtOldPosition = await _boardService.GetPieceAtPositionAsync(new Position(1, 1));
        pieceAtOldPosition.Should().BeNull();
        
        // Verify the piece is at the new position
        var pieceAtNewPosition = await _boardService.GetPieceAtPositionAsync(newPosition);
        pieceAtNewPosition.Should().NotBeNull();
        pieceAtNewPosition!.Id.Should().Be(piece.Id);
    }

    [Fact]
    public async Task MovePiece_WithOccupiedPosition_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var piece1 = await _boardService.PlacePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));
        var piece2 = await _boardService.PlacePieceAsync(PieceType.Knight, Team.Orcs, new Position(2, 2));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _boardService.MovePieceAsync(piece1.Id, piece2.Position));
    }

    [Fact]
    public async Task GetPieceAtPosition_WithExistingPiece_ShouldReturnPiece()
    {
        // Arrange
        var position = new Position(1, 1);
        var expectedPiece = await _boardService.PlacePieceAsync(PieceType.Pawn, Team.Elves, position);

        // Act
        var result = await _boardService.GetPieceAtPositionAsync(position);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedPiece.Id);
        result.Type.Should().Be(expectedPiece.Type);
        result.Team.Should().Be(expectedPiece.Team);
        result.Position.Should().Be(expectedPiece.Position);
    }

    [Fact]
    public async Task GetPieceAtPosition_WithNoPiece_ShouldReturnNull()
    {
        // Arrange
        var position = new Position(1, 1);

        // Act
        var result = await _boardService.GetPieceAtPositionAsync(position);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsPositionFree_WithFreePosition_ShouldReturnTrue()
    {
        // Arrange
        var position = new Position(1, 1);

        // Act
        var result = await _boardService.IsPositionFreeAsync(position);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPositionFree_WithOccupiedPosition_ShouldReturnFalse()
    {
        // Arrange
        var position = new Position(1, 1);
        await _boardService.PlacePieceAsync(PieceType.Pawn, Team.Elves, position);

        // Act
        var result = await _boardService.IsPositionFreeAsync(position);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(7, 7, true)]
    [InlineData(4, 4, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(8, 0, false)]
    [InlineData(0, 8, false)]
    [InlineData(10, 10, false)]
    public void IsPositionOnBoard_WithVariousPositions_ShouldReturnCorrectResult(int x, int y, bool expected)
    {
        // Arrange
        var position = new Position(x, y);

        // Act
        var result = _boardService.IsPositionOnBoard(position);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetBoardSize_ShouldReturn8()
    {
        // Act
        var result = _boardService.GetBoardSize();

        // Assert
        result.Should().Be(8);
    }

}
