using ChessWar.Api.Controllers;
using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;

namespace ChessWar.Tests.Unit;

public class BoardControllerTests
{
    private readonly Mock<IBoardService> _boardServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<BoardController>> _loggerMock;
    private readonly BoardController _controller;
    private readonly CancellationToken _cancellationToken;

    public BoardControllerTests()
    {
        _boardServiceMock = new Mock<IBoardService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<BoardController>>();
        
        // Настройка универсального мока для маппера
        _mapperMock.Setup(x => x.Map<GameBoardDto>(It.IsAny<GameBoard>()))
            .Returns(new GameBoardDto());
        _mapperMock.Setup(x => x.Map<PieceDto>(It.IsAny<Domain.Entities.Piece>()))
            .Returns(new PieceDto());
        
        _controller = new BoardController(_boardServiceMock.Object, _mapperMock.Object, _loggerMock.Object);
        _cancellationToken = CancellationToken.None;
    }

    [Fact]
    public async Task GetGameBoard_ShouldReturnOkResult()
    {
        // Arrange
        var gameBoard = new GameBoard();
        gameBoard.SetPieceAt(new Position(1, 1), TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1));
        gameBoard.SetPieceAt(new Position(2, 2), TestHelpers.CreatePiece(PieceType.Knight, Team.Orcs, 2, 2));
        
        var gameBoardDto = new GameBoardDto();
        
        _boardServiceMock
            .Setup(x => x.GetBoardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameBoard);

        _mapperMock
            .Setup(x => x.Map<GameBoardDto>(gameBoard))
            .Returns(gameBoardDto);

        // Act
        var result = await _controller.GetGameBoard(_cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<GameBoardDto>();
    }

    [Fact]
    public async Task ResetBoard_ShouldReturnOkResult()
    {
        // Arrange
        _boardServiceMock
            .Setup(x => x.ResetBoardAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ResetBoard(_cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _boardServiceMock.Verify(x => x.ResetBoardAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetupInitialPosition_ShouldReturnOkResult()
    {
        // Arrange
        _boardServiceMock
            .Setup(x => x.SetupInitialPositionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SetupInitialPosition(_cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _boardServiceMock.Verify(x => x.SetupInitialPositionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlacePiece_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var placeDto = new PlacePieceDto
        {
            Type = "Pawn",
            Team = "Elves",
            X = 1,
            Y = 1
        };
        
        var expectedPiece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1);
        _boardServiceMock
            .Setup(x => x.PlacePieceAsync(It.IsAny<PieceType>(), It.IsAny<Team>(), It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPiece);

        // Act
        var result = await _controller.PlacePiece(placeDto, _cancellationToken);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeOfType<PieceDto>();
    }

    [Fact]
    public async Task PlacePiece_WithInvalidTeam_ShouldReturnBadRequest()
    {
        // Arrange
        var placeDto = new PlacePieceDto
        {
            Type = "Pawn",
            Team = "InvalidTeam",
            X = 1,
            Y = 1
        };

        // Act
        var result = await _controller.PlacePiece(placeDto, _cancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PlacePiece_WithInvalidType_ShouldReturnBadRequest()
    {
        // Arrange
        var placeDto = new PlacePieceDto
        {
            Type = "InvalidType",
            Team = "Elves",
            X = 1,
            Y = 1
        };

        // Act
        var result = await _controller.PlacePiece(placeDto, _cancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PlacePiece_WithInvalidPosition_ShouldReturnBadRequest()
    {
        // Arrange
        var placeDto = new PlacePieceDto
        {
            Type = "Pawn",
            Team = "Elves",
            X = -1,
            Y = 1
        };

        // Act
        var result = await _controller.PlacePiece(placeDto, _cancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PlacePiece_WithOccupiedPosition_ShouldReturnConflict()
    {
        // Arrange
        var placeDto = new PlacePieceDto
        {
            Type = "Pawn",
            Team = "Elves",
            X = 1,
            Y = 1
        };
        
        var existingPiece = TestHelpers.CreatePiece(PieceType.Knight, Team.Orcs, 1, 1);
        _boardServiceMock
            .Setup(x => x.PlacePieceAsync(It.IsAny<PieceType>(), It.IsAny<Team>(), It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Position is occupied"));

        // Act
        var result = await _controller.PlacePiece(placeDto, _cancellationToken);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task MovePiece_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var pieceId = 1;
        var moveDto = new UpdatePieceDto { X = 3, Y = 3 };
        var existingPiece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1);
        
        _boardServiceMock
            .Setup(x => x.MovePieceAsync(pieceId, It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);

        // Act
        var result = await _controller.MovePiece(pieceId, moveDto, _cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<PieceDto>();
    }

    [Fact]
    public async Task MovePiece_WithNonExistingPiece_ShouldReturnNotFound()
    {
        // Arrange
        var pieceId = 999;
        var moveDto = new UpdatePieceDto { X = 3, Y = 3 };
        
        _boardServiceMock
            .Setup(x => x.MovePieceAsync(pieceId, It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Piece not found"));

        // Act
        var result = await _controller.MovePiece(pieceId, moveDto, _cancellationToken);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task MovePiece_WithInvalidPosition_ShouldReturnBadRequest()
    {
        // Arrange
        var pieceId = 1;
        var moveDto = new UpdatePieceDto { X = -1, Y = 1 };
        
        _boardServiceMock
            .Setup(x => x.MovePieceAsync(It.IsAny<int>(), It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Position is outside the board boundaries"));

        // Act
        var result = await _controller.MovePiece(pieceId, moveDto, _cancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MovePiece_WithOccupiedPosition_ShouldReturnConflict()
    {
        // Arrange
        var pieceId = 1;
        var moveDto = new UpdatePieceDto { X = 2, Y = 2 };
        var existingPiece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1);
        var pieceAtPosition = TestHelpers.CreatePiece(PieceType.Knight, Team.Orcs, 2, 2);
        
        _boardServiceMock
            .Setup(x => x.MovePieceAsync(pieceId, It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Position is occupied"));

        // Act
        var result = await _controller.MovePiece(pieceId, moveDto, _cancellationToken);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task GetPieceAtPosition_WithExistingPiece_ShouldReturnOkResult()
    {
        // Arrange
        var x = 1;
        var y = 1;
        var expectedPiece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1);
        
        _boardServiceMock
            .Setup(x => x.GetPieceAtPositionAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPiece);

        // Act
        var result = await _controller.GetPieceAtPosition(x, y, _cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<PieceDto>();
    }

    [Fact]
    public async Task GetPieceAtPosition_WithNoPiece_ShouldReturnNotFound()
    {
        // Arrange
        var x = 1;
        var y = 1;
        
        _boardServiceMock
            .Setup(x => x.GetPieceAtPositionAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        // Act
        var result = await _controller.GetPieceAtPosition(x, y, _cancellationToken);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPieceAtPosition_WithInvalidPosition_ShouldReturnBadRequest()
    {
        // Arrange
        var x = -1;
        var y = 1;

        // Act
        var result = await _controller.GetPieceAtPosition(x, y, _cancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task IsPositionFree_WithFreePosition_ShouldReturnOkResult()
    {
        // Arrange
        var x = 1;
        var y = 1;
        
        _boardServiceMock
            .Setup(x => x.IsPositionFreeAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.IsPositionFree(x, y, _cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task IsPositionFree_WithOccupiedPosition_ShouldReturnOkResult()
    {
        // Arrange
        var x = 1;
        var y = 1;
        var piece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1);
        
        _boardServiceMock
            .Setup(x => x.IsPositionFreeAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.IsPositionFree(x, y, _cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(false);
    }

    [Fact]
    public async Task IsPositionFree_WithInvalidPosition_ShouldReturnBadRequest()
    {
        // Arrange
        var x = -1;
        var y = 1;

        // Act
        var result = await _controller.IsPositionFree(x, y, _cancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
