using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Application.Services.Pieces;
using FluentAssertions;
using Moq;

namespace ChessWar.Tests.Unit;

public class PieceServiceTests
{
    private readonly Mock<IPieceRepository> _pieceRepositoryMock;
    private readonly IPieceService _pieceService;

    public PieceServiceTests()
    {
        _pieceRepositoryMock = new Mock<IPieceRepository>();
        var pieceFactory = TestHelpers.CreatePieceFactory();
        _pieceService = new PieceService(_pieceRepositoryMock.Object, pieceFactory);
    }

    [Fact]
    public async Task CreatePieceAsync_WithValidData_ShouldCreatePiece()
    {
        var type = PieceType.Pawn;
        var team = Team.Elves;
        var position = new Position(1, 1);
        var expectedPiece = new Piece(type, team, position);
        
        _pieceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _pieceService.CreatePieceAsync(type, team, position);

        result.Should().NotBeNull();
        result.Type.Should().Be(type);
        result.Team.Should().Be(team);
        result.Position.Should().Be(position);
        result.HP.Should().Be(10); // Default HP for Pawn
        result.ATK.Should().Be(2); // Default ATK for Pawn
        
        _pieceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePieceAsync_WithInvalidPosition_ShouldThrowArgumentException()
    {
        var type = PieceType.Pawn;
        var team = Team.Elves;
        var invalidPosition = new Position(-1, 1); // Invalid X coordinate

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pieceService.CreatePieceAsync(type, team, invalidPosition));
    }

    [Fact]
    public async Task GetPieceByIdAsync_WithExistingId_ShouldReturnPiece()
    {
        var pieceId = 1;
        var expectedPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPiece);

        var result = await _pieceService.GetPieceByIdAsync(pieceId);

        result.Should().NotBeNull();
        result.Should().Be(expectedPiece);
    }

    [Fact]
    public async Task GetPieceByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        var pieceId = 999;
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        var result = await _pieceService.GetPieceByIdAsync(pieceId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllPiecesAsync_ShouldReturnAllPieces()
    {
        var expectedPieces = new List<Piece>
        {
            new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)),
            new Piece(PieceType.Knight, Team.Orcs, new Position(2, 2))
        };
        
        _pieceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPieces);

        var result = await _pieceService.GetAllPiecesAsync();

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedPieces);
    }

    [Fact]
    public async Task GetPiecesByTeamAsync_WithValidTeam_ShouldReturnTeamPieces()
    {
        var team = Team.Elves;
        var expectedPieces = new List<Piece>
        {
            new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)),
            new Piece(PieceType.Knight, Team.Elves, new Position(2, 2))
        };
        
        _pieceRepositoryMock
            .Setup(x => x.GetByTeamAsync(team, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPieces);

        var result = await _pieceService.GetPiecesByTeamAsync(team);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Team.Should().Be(team));
    }

    [Fact]
    public async Task GetAlivePiecesAsync_ShouldReturnOnlyAlivePieces()
    {
        var pieces = new List<Piece>
        {
            new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)) { HP = 10 },
            new Piece(PieceType.Knight, Team.Orcs, new Position(2, 2)) { HP = 0 } // Dead
        };
        
        _pieceRepositoryMock
            .Setup(x => x.GetAlivePiecesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pieces.Where(p => p.IsAlive).ToList());

        var result = await _pieceService.GetAlivePiecesAsync();

        result.Should().HaveCount(1);
        result.Should().AllSatisfy(p => p.IsAlive.Should().BeTrue());
    }

    [Fact]
    public async Task UpdatePiecePositionAsync_WithValidData_ShouldUpdatePosition()
    {
        var pieceId = 1;
        var newPosition = new Position(3, 3);
        var existingPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);
        
        _pieceRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _pieceService.UpdatePiecePositionAsync(pieceId, newPosition);

        result.Position.Should().Be(newPosition);
        _pieceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePiecePositionAsync_WithInvalidPosition_ShouldThrowArgumentException()
    {
        var pieceId = 1;
        var invalidPosition = new Position(10, 10); // Outside board
        var existingPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pieceService.UpdatePiecePositionAsync(pieceId, invalidPosition));
    }

    [Fact]
    public async Task UpdatePiecePositionAsync_WithOccupiedPosition_ShouldThrowInvalidOperationException()
    {
        var pieceId = 1;
        var occupiedPosition = new Position(2, 2);
        var existingPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(occupiedPosition, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Piece(PieceType.Knight, Team.Orcs, occupiedPosition));

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _pieceService.UpdatePiecePositionAsync(pieceId, occupiedPosition));
    }

    [Fact]
    public async Task UpdatePieceStatsAsync_WithValidData_ShouldUpdateStats()
    {
        var pieceId = 1;
        var existingPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);
        
        _pieceRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _pieceService.UpdatePieceStatsAsync(pieceId, hp: 15, atk: 5, mp: 8, xp: 10);

        result.HP.Should().Be(15);
        result.ATK.Should().Be(5);
        result.XP.Should().Be(10);
        _pieceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePieceStatsAsync_WithNonExistingPiece_ShouldThrowInvalidOperationException()
    {
        var pieceId = 999;
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _pieceService.UpdatePieceStatsAsync(pieceId, hp: 15));
    }

    [Fact]
    public async Task DeletePieceAsync_WithExistingPiece_ShouldDeletePiece()
    {
        var pieceId = 1;
        var existingPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);
        
        _pieceRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<Piece>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _pieceService.DeletePieceAsync(pieceId);

        _pieceRepositoryMock.Verify(x => x.DeleteAsync(existingPiece, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePieceAsync_WithNonExistingPiece_ShouldThrowInvalidOperationException()
    {
        var pieceId = 999;
        
        _pieceRepositoryMock
            .Setup(x => x.GetByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _pieceService.DeletePieceAsync(pieceId));
    }

    [Fact]
    public async Task IsPositionFreeAsync_WithFreePosition_ShouldReturnTrue()
    {
        var position = new Position(1, 1);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        var result = await _pieceService.IsPositionFreeAsync(position);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPositionFreeAsync_WithOccupiedPosition_ShouldReturnFalse()
    {
        var position = new Position(1, 1);
        var piece = new Piece(PieceType.Pawn, Team.Elves, position);
        
        _pieceRepositoryMock
            .Setup(x => x.GetByPositionAsync(position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(piece);

        var result = await _pieceService.IsPositionFreeAsync(position);

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

        var result = _pieceService.IsPositionOnBoard(position);

        result.Should().Be(expected);
    }
}
