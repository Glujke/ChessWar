using Microsoft.AspNetCore.Mvc;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Api.Services;
using ChessWar.Domain.ValueObjects;
using ChessWar.Application.DTOs;
using AutoMapper;

namespace ChessWar.Api.Controllers;

/// <summary>
/// Контроллер для управления атаками в игре Chess War
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class AttackController : BaseController
{
    private readonly IAttackApplicationService _attackService;
    private readonly IMapper _mapper;
    private readonly IErrorHandlingService _errorHandlingService;

    public AttackController(IAttackApplicationService attackService, IMapper mapper, IErrorHandlingService errorHandlingService, ILogger<AttackController> logger) : base(logger)
    {
        _attackService = attackService;
        _mapper = mapper;
        _errorHandlingService = errorHandlingService;
    }

    /// <summary>
    /// Проверяет, может ли фигура атаковать указанную позицию
    /// </summary>
    /// <param name="request">Запрос на проверку атаки</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Результат проверки атаки</returns>
    [HttpPost("check")]
    [ProducesResponseType(typeof(AttackResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CheckAttack(
        [FromBody] AttackRequestDto request,
        CancellationToken ct = default)
    {
        try
        {
            LogInformation("Checking attack for piece {AttackerId} to position ({X}, {Y})",
                request.AttackerId, request.TargetX, request.TargetY);

            var targetPosition = _mapper.Map<Position>(request);
            var result = await _attackService.CheckAttackAsync(request.AttackerId, targetPosition, ct);

            if (result.Attacker == null)
            {
                LogWarning("Piece with ID {AttackerId} not found", request.AttackerId);
                return _errorHandlingService.CreateNotFoundError("Piece", request.AttackerId.ToString());
            }

            var response = _mapper.Map<AttackResponseDto>(result);
            LogInformation("Attack check completed for piece {AttackerId}: {CanAttack}",
                request.AttackerId, result.CanAttack);
            return Ok(response);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error checking attack for piece {AttackerId}", request.AttackerId);
            return _errorHandlingService.CreateValidationError("Failed to check attack", ex.Message);
        }
    }

    /// <summary>
    /// Получает все позиции, которые может атаковать указанная фигура
    /// </summary>
    /// <param name="attackerId">ID атакующей фигуры</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список атакуемых позиций</returns>
    [HttpGet("{attackerId}/positions")]
    [ProducesResponseType(typeof(AttackablePositionsResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAttackablePositions(
        [FromRoute] int attackerId,
        CancellationToken ct = default)
    {
        try
        {
            LogInformation("Getting attackable positions for piece {AttackerId}", attackerId);

            var attackablePositions = await _attackService.GetAttackablePositionsAsync(attackerId, ct);

            if (!attackablePositions.Any())
            {
                LogWarning("Piece with ID {AttackerId} not found", attackerId);
                return _errorHandlingService.CreateNotFoundError("Piece", attackerId.ToString());
            }

            var response = new AttackablePositionsResponseDto
            {
                AttackerId = attackerId,
                AttackablePositions = _mapper.Map<List<PositionDto>>(attackablePositions),
                TotalCount = attackablePositions.Count()
            };

            LogInformation("Found {Count} attackable positions for piece {AttackerId}",
                attackablePositions.Count(), attackerId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error getting attackable positions for piece {AttackerId}", attackerId);
            return _errorHandlingService.CreateValidationError("Failed to get attackable positions", ex.Message);
        }
    }

    /// <summary>
    /// Проверяет, является ли цель врагом для атакующей фигуры
    /// </summary>
    /// <param name="attackerId">ID атакующей фигуры</param>
    /// <param name="targetId">ID целевой фигуры</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>True, если цель является врагом</returns>
    [HttpGet("{attackerId}/enemy/{targetId}")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> IsEnemy(
        [FromRoute] int attackerId,
        [FromRoute] int targetId,
        CancellationToken ct = default)
    {
        try
        {
            LogInformation("Checking if piece {TargetId} is enemy of piece {AttackerId}", targetId, attackerId);

            var isEnemy = await _attackService.IsEnemyAsync(attackerId, targetId, ct);

            LogInformation("Piece {TargetId} is enemy of piece {AttackerId}: {IsEnemy}",
                targetId, attackerId, isEnemy);
            return Ok(isEnemy);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error checking enemy status between pieces {AttackerId} and {TargetId}",
                attackerId, targetId);
            return _errorHandlingService.CreateValidationError("Failed to check enemy status", ex.Message);
        }
    }

    /// <summary>
    /// Вычисляет расстояние Чебышёва между двумя позициями
    /// </summary>
    /// <param name="fromX">X координата начальной позиции</param>
    /// <param name="fromY">Y координата начальной позиции</param>
    /// <param name="toX">X координата конечной позиции</param>
    /// <param name="toY">Y координата конечной позиции</param>
    /// <returns>Расстояние Чебышёва</returns>
    [HttpGet("distance")]
    [ProducesResponseType(typeof(int), 200)]
    public IActionResult CalculateDistance(
        [FromQuery] int fromX,
        [FromQuery] int fromY,
        [FromQuery] int toX,
        [FromQuery] int toY)
    {
        try
        {
            LogInformation("Calculating Chebyshev distance from ({FromX}, {FromY}) to ({ToX}, {ToY})",
                fromX, fromY, toX, toY);

            var from = new Position(fromX, fromY);
            var to = new Position(toX, toY);
            var distance = _attackService.CalculateChebyshevDistance(from, to);

            LogInformation("Chebyshev distance calculated: {Distance}", distance);
            return Ok(distance);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error calculating distance from ({FromX}, {FromY}) to ({ToX}, {ToY})",
                fromX, fromY, toX, toY);
            return _errorHandlingService.CreateValidationError("Failed to calculate distance", ex.Message);
        }
    }
}
