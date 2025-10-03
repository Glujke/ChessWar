using ChessWar.Api.Controllers;
using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;

namespace ChessWar.Tests.Unit;

public class PiecesControllerTests
{
    private readonly Mock<IPieceService> _pieceServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<PiecesController>> _loggerMock;
    private readonly PiecesController _controller;
    private readonly CancellationToken _cancellationToken;

    public PiecesControllerTests()
    {
        _pieceServiceMock = new Mock<IPieceService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<PiecesController>>();

        _mapperMock.Setup(x => x.Map<List<PieceDto>>(It.IsAny<IEnumerable<Domain.Entities.Piece>>()))
            .Returns(new List<PieceDto>());
        _mapperMock.Setup(x => x.Map<PieceDto>(It.IsAny<Domain.Entities.Piece>()))
            .Returns(new PieceDto());

        _controller = new PiecesController(_pieceServiceMock.Object, _mapperMock.Object, _loggerMock.Object);
        _cancellationToken = CancellationToken.None;
    }

    [Fact]
    public async Task CreatePiece_WithValidData_ShouldReturnCreatedResult()
    {
        var createDto = new CreatePieceDto
        {
            Type = "Pawn",
            Team = "Elves",
            X = 1,
            Y = 1
        };

        var expectedPiece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1);
        var expectedPieceDto = new PieceDto();

        _pieceServiceMock
            .Setup(x => x.CreatePieceAsync(It.IsAny<PieceType>(), It.IsAny<Team>(), It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPiece);

        _mapperMock
            .Setup(x => x.Map<PieceDto>(expectedPiece))
            .Returns(expectedPieceDto);

        var result = await _controller.CreatePiece(createDto, _cancellationToken);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeOfType<PieceDto>();
    }

    [Fact]
    public async Task CreatePiece_WithInvalidTeam_ShouldReturnBadRequest()
    {
        var createDto = new CreatePieceDto
        {
            Type = "Pawn",
            Team = "InvalidTeam",
            X = 1,
            Y = 1
        };

        var result = await _controller.CreatePiece(createDto, _cancellationToken);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreatePiece_WithInvalidType_ShouldReturnBadRequest()
    {
        var createDto = new CreatePieceDto
        {
            Type = "InvalidType",
            Team = "Elves",
            X = 1,
            Y = 1
        };

        var result = await _controller.CreatePiece(createDto, _cancellationToken);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreatePiece_WithInvalidPosition_ShouldReturnBadRequest()
    {
        var createDto = new CreatePieceDto
        {
            Type = "Pawn",
            Team = "Elves",
            X = -1,
            Y = 1
        };

        var result = await _controller.CreatePiece(createDto, _cancellationToken);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetPiece_WithExistingId_ShouldReturnOkResult()
    {
        var pieceId = 1;
        var expectedPiece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1);

        _pieceServiceMock
            .Setup(x => x.GetPieceByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPiece);

        var result = await _controller.GetPiece(pieceId, _cancellationToken);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<PieceDto>();
    }

    [Fact]
    public async Task GetPiece_WithNonExistingId_ShouldReturnNotFound()
    {
        var pieceId = 999;

        _pieceServiceMock
            .Setup(x => x.GetPieceByIdAsync(pieceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Piece?)null);

        var result = await _controller.GetPiece(pieceId, _cancellationToken);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAllPieces_ShouldReturnOkResult()
    {
        var expectedPieces = new List<Piece>
        {
            TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1),
            TestHelpers.CreatePiece(PieceType.Knight, Team.Orcs, 2, 2)
        };

        _pieceServiceMock
            .Setup(x => x.GetAllPiecesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPieces);

        var result = await _controller.GetAllPieces(_cancellationToken);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<List<PieceDto>>();
    }

    [Fact]
    public async Task GetPiecesByTeam_WithValidTeam_ShouldReturnOkResult()
    {
        var team = "Elves";
        var expectedPieces = new List<Piece>
        {
            TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1),
            TestHelpers.CreatePiece(PieceType.Knight, Team.Elves, 2, 2)
        };

        _pieceServiceMock
            .Setup(x => x.GetPiecesByTeamAsync(Team.Elves, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPieces);

        var result = await _controller.GetPiecesByTeam(team, _cancellationToken);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<List<PieceDto>>();
    }

    [Fact]
    public async Task GetPiecesByTeam_WithInvalidTeam_ShouldReturnBadRequest()
    {
        var team = "InvalidTeam";

        var result = await _controller.GetPiecesByTeam(team, _cancellationToken);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAlivePieces_ShouldReturnOkResult()
    {
        var expectedPieces = new List<Piece>
        {
            TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1),
            TestHelpers.CreatePiece(PieceType.Knight, Team.Orcs, 2, 2)
        };
        expectedPieces[0].HP = 10;
        expectedPieces[1].HP = 20;

        _pieceServiceMock
            .Setup(x => x.GetAlivePiecesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPieces);

        var result = await _controller.GetAlivePieces(_cancellationToken);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<List<PieceDto>>();
    }

    [Fact]
    public async Task UpdatePiece_WithValidData_ShouldReturnOkResult()
    {
        var pieceId = 1;
        var updateDto = new UpdatePieceDto
        {
            HP = 15,
            ATK = 5,
            MP = 8,
            XP = 10
        };

        var existingPiece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1);

        _pieceServiceMock
            .Setup(x => x.UpdatePieceStatsAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);

        var result = await _controller.UpdatePiece(pieceId, updateDto, _cancellationToken);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<PieceDto>();
    }

    [Fact]
    public async Task UpdatePiece_WithNonExistingPiece_ShouldReturnNotFound()
    {
        var pieceId = 999;
        var updateDto = new UpdatePieceDto { HP = 15 };

        _pieceServiceMock
            .Setup(x => x.UpdatePieceStatsAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Piece not found"));

        var result = await _controller.UpdatePiece(pieceId, updateDto, _cancellationToken);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePiecePosition_WithValidData_ShouldReturnOkResult()
    {
        var pieceId = 1;
        var updateDto = new UpdatePieceDto { X = 3, Y = 3 };
        var existingPiece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 1, 1);

        _pieceServiceMock
            .Setup(x => x.UpdatePiecePositionAsync(It.IsAny<int>(), It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPiece);

        var result = await _controller.UpdatePiecePosition(pieceId, updateDto, _cancellationToken);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<PieceDto>();
    }

    [Fact]
    public async Task UpdatePiecePosition_WithInvalidPosition_ShouldReturnBadRequest()
    {
        var pieceId = 1;
        var updateDto = new UpdatePieceDto { X = -1, Y = 1 };

        _pieceServiceMock
            .Setup(x => x.UpdatePiecePositionAsync(It.IsAny<int>(), It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid position"));

        var result = await _controller.UpdatePiecePosition(pieceId, updateDto, _cancellationToken);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeletePiece_WithExistingPiece_ShouldReturnNoContent()
    {
        var pieceId = 1;

        _pieceServiceMock
            .Setup(x => x.DeletePieceAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeletePiece(pieceId, _cancellationToken);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeletePiece_WithNonExistingPiece_ShouldReturnNotFound()
    {
        var pieceId = 999;

        _pieceServiceMock
            .Setup(x => x.DeletePieceAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Piece not found"));

        var result = await _controller.DeletePiece(pieceId, _cancellationToken);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
