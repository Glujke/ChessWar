using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Services.Board;
using FluentAssertions;
using Moq;

namespace ChessWar.Tests.Unit;

public class BoardServiceTests
{
    private readonly Mock<IPieceRepository> _pieceRepositoryMock;
    private readonly Mock<IPieceFactory> _pieceFactoryMock;
    private readonly IBoardService _boardService;

    public BoardServiceTests()
    {
        _pieceRepositoryMock = new Mock<IPieceRepository>();
        _pieceFactoryMock = new Mock<IPieceFactory>();
        _boardService = new BoardService(_pieceRepositoryMock.Object, _pieceFactoryMock.Object);
    }

    [Fact]
    public async Task GetBoardAsync_ShouldReturnGameBoardWithPieces()
    {
        var pieces = new List<Piece>
        {
            new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)) { HP = 10 },
            new Piece(PieceType.Knight, Team.Orcs, new Position(2, 2)) { HP = 10 }
        };
        
        _pieceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pieces);

        var result = await _boardService.GetBoardAsync();

        result.Should().NotBeNull();
        result.Pieces.Should().HaveCount(2);
        result.Pieces.Should().BeEquivalentTo(pieces);
    }

    [Fact]
    public async Task ResetBoardAsync_ShouldDeleteAllPieces()
    {
        var pieces = new List<Piece>
        {
            new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)) { HP = 10 },
            new Piece(PieceType.Knight, Team.Orcs, new Position(2, 2)) { HP = 10 }
        };
        
        _pieceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pieces);

        await _boardService.ResetBoardAsync();

        _pieceRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SetupInitialPositionAsync_ShouldCreateStandardChessSetup()
    {
        _pieceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _pieceFactoryMock
            .Setup(x => x.CreatePiece(It.IsAny<PieceType>(), It.IsAny<Team>(), It.IsAny<Position>(), It.IsAny<Player>()))
            .Returns(new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0)) { HP = 10 });

        await _boardService.SetupInitialPositionAsync();

        _pieceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Exactly(18));
    }

    [Fact]
    public async Task PlacePieceAsync_WithValidData_ShouldPlacePiece()
    {
        var type = PieceType.Pawn;
        var team = Team.Elves;
        var position = new Position(1, 1);
        var expectedPiece = new Piece(type, team, position) { HP = 10 };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);
        
        _pieceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _pieceFactoryMock
            .Setup(x => x.CreatePiece(type, team, position, It.IsAny<Player>()))
            .Returns(expectedPiece);

        var result = await _boardService.PlacePieceAsync(type, team, position);

        result.Should().NotBeNull();
        result.Type.Should().Be(type);
        result.Team.Should().Be(team);
        result.Position.Should().Be(position);
        _pieceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlacePieceAsync_WithInvalidPosition_ShouldThrowArgumentException()
    {
        var type = PieceType.Pawn;
        var team = Team.Elves;
        var invalidPosition = new Position(-1, 1);

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _boardService.PlacePieceAsync(type, team, invalidPosition));
    }

    [Fact]
    public async Task PlacePieceAsync_WithOccupiedPosition_ShouldThrowInvalidOperationException()
    {
        var type = PieceType.Pawn;
        var team = Team.Elves;
        var position = new Position(1, 1);
        var existingPiece = new Piece(PieceType.Knight, Team.Orcs, position);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _boardService.PlacePieceAsync(type, team, position));
    }

    [Fact]
    public async Task MovePieceAsync_WithValidData_ShouldMovePiece()
    {
        var pieceId = 1;
        var newPosition = new Position(3, 3);
        var existingPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)) { HP = 10 };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(newPosition, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);
        
        _pieceRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _boardService.MovePieceAsync(pieceId, newPosition);

        result.Position.Should().Be(newPosition);
        _pieceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MovePieceAsync_WithInvalidPosition_ShouldThrowArgumentException()
    {
        var pieceId = 1;
        var invalidPosition = new Position(10, 10);
        var existingPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)) { HP = 10 };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _boardService.MovePieceAsync(pieceId, invalidPosition));
    }

    [Fact]
    public async Task MovePieceAsync_WithOccupiedPosition_ShouldThrowInvalidOperationException()
    {
        var pieceId = 1;
        var occupiedPosition = new Position(2, 2);
        var existingPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)) { HP = 10 };
        var pieceAtPosition = new Piece(PieceType.Knight, Team.Orcs, occupiedPosition) { HP = 10 };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(occupiedPosition, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pieceAtPosition);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _boardService.MovePieceAsync(pieceId, occupiedPosition));
    }

    [Fact]
    public async Task MovePieceAsync_WithNonExistingPiece_ShouldThrowInvalidOperationException()
    {
        var pieceId = 999;
        var newPosition = new Position(3, 3);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _boardService.MovePieceAsync(pieceId, newPosition));
    }

    [Fact]
    public async Task GetPieceAtPositionAsync_WithExistingPiece_ShouldReturnPiece()
    {
        var position = new Position(1, 1);
        var expectedPiece = new Piece(PieceType.Pawn, Team.Elves, position) { HP = 10 };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPiece);

        var result = await _boardService.GetPieceAtPositionAsync(position);

        result.Should().NotBeNull();
        result.Should().Be(expectedPiece);
    }

    [Fact]
    public async Task GetPieceAtPositionAsync_WithNoPiece_ShouldReturnNull()
    {
        var position = new Position(1, 1);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        var result = await _boardService.GetPieceAtPositionAsync(position);

        result.Should().BeNull();
    }

    [Fact]
    public async Task IsPositionFreeAsync_WithFreePosition_ShouldReturnTrue()
    {
        var position = new Position(1, 1);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        var result = await _boardService.IsPositionFreeAsync(position);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPositionFreeAsync_WithOccupiedPosition_ShouldReturnFalse()
    {
        var position = new Position(1, 1);
        var piece = new Piece(PieceType.Pawn, Team.Elves, position) { HP = 10 };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(piece);

        var result = await _boardService.IsPositionFreeAsync(position);

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
        var position = new Position(x, y);

        var result = _boardService.IsPositionOnBoard(position);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetBoardSize_ShouldReturn8()
    {
        var result = _boardService.GetBoardSize();

        result.Should().Be(8);
    }
}
