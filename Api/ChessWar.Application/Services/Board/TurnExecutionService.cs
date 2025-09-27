using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Сервис выполнения ходов - объединяет все операции с ходами
/// </summary>
public class TurnExecutionService : ITurnExecutionService
{
    private readonly IActionExecutionService _actionExecutionService;
    private readonly ITurnOrchestrator _turnOrchestrator;
    private readonly IAITurnService _aiTurnService;
    private readonly IActionQueryService _actionQueryService;
    private readonly ILogger<TurnExecutionService> _logger;

    public TurnExecutionService(
        IActionExecutionService actionExecutionService,
        ITurnOrchestrator turnOrchestrator,
        IAITurnService aiTurnService,
        IActionQueryService actionQueryService,
        ILogger<TurnExecutionService> logger)
    {
        _actionExecutionService = actionExecutionService ?? throw new ArgumentNullException(nameof(actionExecutionService));
        _turnOrchestrator = turnOrchestrator ?? throw new ArgumentNullException(nameof(turnOrchestrator));
        _aiTurnService = aiTurnService ?? throw new ArgumentNullException(nameof(aiTurnService));
        _actionQueryService = actionQueryService ?? throw new ArgumentNullException(nameof(actionQueryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ExecuteActionAsync(GameSession gameSession, ExecuteActionDto dto)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        _logger.LogInformation("Executing action {ActionType} for piece {PieceId}", dto.Type, dto.PieceId);
        
        return await _actionExecutionService.ExecuteActionAsync(gameSession, dto);
    }

    public async Task EndTurnAsync(GameSession gameSession)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        _logger.LogInformation("Ending turn for session {SessionId}", gameSession.Id);
        
        await _turnOrchestrator.EndTurnAsync(gameSession);
    }

    public async Task<bool> MakeAiTurnAsync(GameSession gameSession)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        _logger.LogInformation("Making AI turn for session {SessionId}", gameSession.Id);
        
        return await _aiTurnService.MakeAiTurnAsync(gameSession);
    }

    public async Task<bool> ExecuteMoveAsync(GameSession gameSession, int pieceId, Position targetPosition)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));
        if (targetPosition == null)
            throw new ArgumentNullException(nameof(targetPosition));

        _logger.LogInformation("Executing move for piece {PieceId} to position ({X}, {Y})", 
            pieceId, targetPosition.X, targetPosition.Y);

        var currentTurn = gameSession.GetCurrentTurn();
        var piece = gameSession.GetAllPieces().FirstOrDefault(p => p.Id == pieceId);
        
        if (piece == null)
        {
            _logger.LogWarning("Piece {PieceId} not found", pieceId);
            return false;
        }

        return await _actionExecutionService.ExecuteMoveAsync(gameSession, currentTurn, piece, targetPosition);
    }

    public async Task<List<Position>> GetAvailableActionsAsync(GameSession gameSession, string pieceId, string actionType, string? abilityName = null)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));
        if (string.IsNullOrEmpty(pieceId))
            throw new ArgumentException("Piece ID cannot be null or empty", nameof(pieceId));
        if (string.IsNullOrEmpty(actionType))
            throw new ArgumentException("Action type cannot be null or empty", nameof(actionType));

        _logger.LogInformation("Getting available actions for piece {PieceId}, action type {ActionType}", 
            pieceId, actionType);

        var positionDtos = await _actionQueryService.GetAvailableActionsAsync(gameSession, pieceId, actionType, abilityName);
        return positionDtos.Select(p => new Position(p.X, p.Y)).ToList();
    }
}
