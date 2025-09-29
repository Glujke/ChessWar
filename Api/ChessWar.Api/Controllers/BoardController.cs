using Microsoft.AspNetCore.Mvc;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Application.DTOs;
using AutoMapper;

namespace ChessWar.Api.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class BoardController : BaseController
{
    private readonly IBoardService _boardService;
    private readonly IMapper _mapper;

    public BoardController(IBoardService boardService, IMapper mapper, ILogger<BoardController> logger) : base(logger)
    {
        _boardService = boardService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetGameBoard(CancellationToken ct)
    {
        try
        {
            LogInformation("Getting game board");
            var gameBoard = await _boardService.GetBoardAsync(ct);
            var boardDto = _mapper.Map<GameBoardDto>(gameBoard);
            LogInformation("Game board retrieved successfully");
            return Ok(boardDto);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error getting game board");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetBoard(CancellationToken ct)
    {
        await _boardService.ResetBoardAsync(ct);
        return Ok("Board reset successfully");
    }

    [HttpPost("setup")]
    public async Task<IActionResult> SetupInitialPosition(CancellationToken ct)
    {
        await _boardService.SetupInitialPositionAsync(ct);
        return Ok("Initial position setup completed");
    }

    [HttpPost("place")]
    public async Task<IActionResult> PlacePiece([FromBody] PlacePieceDto placeDto, CancellationToken ct)
    {
        if (!Enum.TryParse<PieceType>(placeDto.Type, true, out var pieceType))
            return BadRequest($"Invalid piece type: {placeDto.Type}");

        if (!Enum.TryParse<Team>(placeDto.Team, true, out var team))
            return BadRequest($"Invalid team: {placeDto.Team}");

        var position = new Position(placeDto.X, placeDto.Y);
        
        try
        {
            var piece = await _boardService.PlacePieceAsync(pieceType, team, position, ct);
            if (piece == null)
                return BadRequest("Failed to place piece");
                
            var pieceDto = _mapper.Map<PieceDto>(piece);
            return CreatedAtAction(nameof(GetPieceAtPosition), new { x = position.X, y = position.Y }, pieceDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id}/move")]
    public async Task<IActionResult> MovePiece(int id, [FromBody] UpdatePieceDto moveDto, CancellationToken ct)
    {
        if (!moveDto.X.HasValue || !moveDto.Y.HasValue)
            return BadRequest("X and Y coordinates are required");

        var position = new Position(moveDto.X.Value, moveDto.Y.Value);
        
        try
        {
            var piece = await _boardService.MovePieceAsync(id, position, ct);
            if (piece == null)
                return NotFound();
                
            var pieceDto = _mapper.Map<PieceDto>(piece);
            return Ok(pieceDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found") ? NotFound() : Conflict(ex.Message);
        }
    }

    [HttpGet("position/{x}/{y}")]
    public async Task<IActionResult> GetPieceAtPosition(int x, int y, CancellationToken ct)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return BadRequest("Position is outside board boundaries");

        var position = new Position(x, y);
        var piece = await _boardService.GetPieceAtPositionAsync(position, ct);
        if (piece == null)
            return NotFound();

        var pieceDto = _mapper.Map<PieceDto>(piece);
        return Ok(pieceDto);
    }

    [HttpGet("free/{x}/{y}")]
    public async Task<IActionResult> IsPositionFree(int x, int y, CancellationToken ct)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return BadRequest("Position is outside board boundaries");

        var position = new Position(x, y);
        var isFree = await _boardService.IsPositionFreeAsync(position, ct);
        return Ok(isFree);
    }

}
