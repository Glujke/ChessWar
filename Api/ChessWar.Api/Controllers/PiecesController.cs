using Microsoft.AspNetCore.Mvc;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Application.DTOs;
using AutoMapper;

namespace ChessWar.Api.Controllers;

/// <summary>
/// Контроллер для управления фигурами: получение, фильтрация по командам, создание и обновление
/// </summary>
public class PiecesController : BaseController
{
    private readonly IPieceService _pieceService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Создаёт контроллер управления фигурами
    /// </summary>
    public PiecesController(IPieceService pieceService, IMapper mapper, ILogger<PiecesController> logger) : base(logger)
    {
        _pieceService = pieceService;
        _mapper = mapper;
    }

    /// <summary>
    /// Возвращает все фигуры
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllPieces(CancellationToken ct)
    {
        try
        {
            LogInformation("Getting all pieces");
            var pieces = await _pieceService.GetAllPiecesAsync(ct);
            var piecesDto = _mapper.Map<List<PieceDto>>(pieces);

            LogInformation("Retrieved {Count} pieces", pieces.Count());
            return Ok(piecesDto);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error getting all pieces");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Возвращает фигуры указанной команды
    /// </summary>
    [HttpGet("team/{team}")]
    public async Task<IActionResult> GetPiecesByTeam(string team, CancellationToken ct)
    {
        try
        {
            LogInformation("Getting pieces for team {Team}", team);

            if (!Enum.TryParse<Team>(team, true, out var teamEnum))
            {
                LogWarning("Invalid team: {Team}", team);
                return BadRequest($"Invalid team: {team}");
            }

            var pieces = await _pieceService.GetPiecesByTeamAsync(teamEnum, ct);
            var piecesDto = _mapper.Map<List<PieceDto>>(pieces);

            LogInformation("Retrieved {Count} pieces for team {Team}", pieces.Count(), team);
            return Ok(piecesDto);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error getting pieces for team {Team}", team);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Возвращает все живые фигуры
    /// </summary>
    [HttpGet("alive")]
    public async Task<IActionResult> GetAlivePieces(CancellationToken ct)
    {
        try
        {
            LogInformation("Getting alive pieces");
            var pieces = await _pieceService.GetAlivePiecesAsync(ct);
            var piecesDto = _mapper.Map<List<PieceDto>>(pieces);

            LogInformation("Retrieved {Count} alive pieces", pieces.Count());
            return Ok(piecesDto);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error getting alive pieces");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Возвращает живые фигуры указанной команды
    /// </summary>
    [HttpGet("alive/team/{team}")]
    public async Task<IActionResult> GetAlivePiecesByTeam(string team, CancellationToken ct)
    {
        if (!Enum.TryParse<Team>(team, true, out var teamEnum))
            return BadRequest($"Invalid team: {team}");

        var pieces = await _pieceService.GetAlivePiecesByTeamAsync(teamEnum, ct);
        var piecesDto = _mapper.Map<List<PieceDto>>(pieces);

        return Ok(piecesDto);
    }

    /// <summary>
    /// Создаёт новую фигуру на указанной позиции
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePiece([FromBody] CreatePieceDto createDto, CancellationToken ct)
    {
        if (!Enum.TryParse<PieceType>(createDto.Type, true, out var pieceType))
            return BadRequest($"Invalid piece type: {createDto.Type}");

        if (!Enum.TryParse<Team>(createDto.Team, true, out var team))
            return BadRequest($"Invalid team: {createDto.Team}");

        var position = new Position(createDto.X, createDto.Y);

        try
        {
            var piece = await _pieceService.CreatePieceAsync(pieceType, team, position, ct);
            if (piece == null)
                return BadRequest("Failed to create piece");

            var pieceDto = _mapper.Map<PieceDto>(piece);
            return CreatedAtAction(nameof(GetPiece), new { id = piece.Id }, pieceDto);
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

    /// <summary>
    /// Возвращает фигуру по идентификатору
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPiece(int id, CancellationToken ct)
    {
        var piece = await _pieceService.GetPieceByIdAsync(id, ct);
        if (piece == null)
            return NotFound();

        var pieceDto = _mapper.Map<PieceDto>(piece);
        return Ok(pieceDto);
    }

    /// <summary>
    /// Обновляет характеристики фигуры
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePiece(int id, [FromBody] UpdatePieceDto updateDto, CancellationToken ct)
    {
        try
        {
            var piece = await _pieceService.UpdatePieceStatsAsync(
                id,
                updateDto.HP,
                updateDto.ATK,
                updateDto.MP,
                updateDto.XP,
                ct);

            var pieceDto = _mapper.Map<PieceDto>(piece);
            return Ok(pieceDto);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found") ? NotFound(ex.Message) : BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Обновляет позицию фигуры
    /// </summary>
    [HttpPut("{id}/position")]
    public async Task<IActionResult> UpdatePiecePosition(int id, [FromBody] UpdatePieceDto updateDto, CancellationToken ct)
    {
        if (!updateDto.X.HasValue || !updateDto.Y.HasValue)
            return BadRequest("X and Y coordinates are required");

        var position = new Position(updateDto.X.Value, updateDto.Y.Value);

        try
        {
            var piece = await _pieceService.UpdatePiecePositionAsync(id, position, ct);
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
            return ex.Message.Contains("not found") ? NotFound(ex.Message) : Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Удаляет фигуру
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePiece(int id, CancellationToken ct)
    {
        try
        {
            await _pieceService.DeletePieceAsync(id, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found") ? NotFound(ex.Message) : BadRequest(ex.Message);
        }
    }

}
