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
        // Arrange
        var pieces = new List<Piece>
        {
            new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)) { HP = 10 },
            new Piece(PieceType.Knight, Team.Orcs, new Position(2, 2)) { HP = 10 }
        };
        
        _pieceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pieces);

        // Act
        var result = await _boardService.GetBoardAsync();

        // Assert
        result.Should().NotBeNull();
        result.Pieces.Should().HaveCount(2);
        result.Pieces.Should().BeEquivalentTo(pieces);
    }

    [Fact]
    public async Task ResetBoardAsync_ShouldDeleteAllPieces()
    {
        // Arrange
        var pieces = new List<Piece>
        {
            new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)) { HP = 10 },
            new Piece(PieceType.Knight, Team.Orcs, new Position(2, 2)) { HP = 10 }
        };
        
        _pieceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pieces);

        // Act
        await _boardService.ResetBoardAsync();

        // Assert
        _pieceRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SetupInitialPositionAsync_ShouldCreateStandardChessSetup()
    {
        // Arrange
        _pieceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _pieceFactoryMock
            .Setup(x => x.CreatePiece(It.IsAny<PieceType>(), It.IsAny<Team>(), It.IsAny<Position>(), It.IsAny<Player>()))
            .Returns(new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0)) { HP = 10 });

        // Act
        await _boardService.SetupInitialPositionAsync();

        // Assert
        // Should create 18 pieces total: 8 pawns + 1 king for each team
        _pieceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Exactly(18));
    }

    [Fact]
    public async Task PlacePieceAsync_WithValidData_ShouldPlacePiece()
    {
        // Arrange
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

        // Act
        var result = await _boardService.PlacePieceAsync(type, team, position);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(type);
        result.Team.Should().Be(team);
        result.Position.Should().Be(position);
        _pieceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlacePieceAsync_WithInvalidPosition_ShouldThrowArgumentException()
    {
        // Arrange
        var type = PieceType.Pawn;
        var team = Team.Elves;
        var invalidPosition = new Position(-1, 1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _boardService.PlacePieceAsync(type, team, invalidPosition));
    }

    [Fact]
    public async Task PlacePieceAsync_WithOccupiedPosition_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var type = PieceType.Pawn;
        var team = Team.Elves;
        var position = new Position(1, 1);
        var existingPiece = new Piece(PieceType.Knight, Team.Orcs, position);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _boardService.PlacePieceAsync(type, team, position));
    }

    [Fact]
    public async Task MovePieceAsync_WithValidData_ShouldMovePiece()
    {
        // Arrange
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

        // Act
        var result = await _boardService.MovePieceAsync(pieceId, newPosition);

        // Assert
        result.Position.Should().Be(newPosition);
        _pieceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MovePieceAsync_WithInvalidPosition_ShouldThrowArgumentException()
    {
        // Arrange
        var pieceId = 1;
        var invalidPosition = new Position(10, 10);
        var existingPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)) { HP = 10 };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _boardService.MovePieceAsync(pieceId, invalidPosition));
    }

    [Fact]
    public async Task MovePieceAsync_WithOccupiedPosition_ShouldThrowInvalidOperationException()
    {
        // Arrange
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

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _boardService.MovePieceAsync(pieceId, occupiedPosition));
    }

    [Fact]
    public async Task MovePieceAsync_WithNonExistingPiece_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var pieceId = 999;
        var newPosition = new Position(3, 3);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _boardService.MovePieceAsync(pieceId, newPosition));
    }

    [Fact]
    public async Task GetPieceAtPositionAsync_WithExistingPiece_ShouldReturnPiece()
    {
        // Arrange
        var position = new Position(1, 1);
        var expectedPiece = new Piece(PieceType.Pawn, Team.Elves, position) { HP = 10 };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPiece);

        // Act
        var result = await _boardService.GetPieceAtPositionAsync(position);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedPiece);
    }

    [Fact]
    public async Task GetPieceAtPositionAsync_WithNoPiece_ShouldReturnNull()
    {
        // Arrange
        var position = new Position(1, 1);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        // Act
        var result = await _boardService.GetPieceAtPositionAsync(position);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsPositionFreeAsync_WithFreePosition_ShouldReturnTrue()
    {
        // Arrange
        var position = new Position(1, 1);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        // Act
        var result = await _boardService.IsPositionFreeAsync(position);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPositionFreeAsync_WithOccupiedPosition_ShouldReturnFalse()
    {
        // Arrange
        var position = new Position(1, 1);
        var piece = new Piece(PieceType.Pawn, Team.Elves, position) { HP = 10 };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(piece);

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
