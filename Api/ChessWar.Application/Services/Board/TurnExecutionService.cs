using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Board; using ChessWar.Application.Interfaces.AI;
using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Сервис выполнения действий в ходе (Legacy - для обратной совместимости)
/// </summary>
public class TurnExecutionService : ITurnExecutionService
{
    private readonly IActionExecutionService _actionExecutionService;
    private readonly ITurnOrchestrator _turnOrchestrator;
    private readonly IAITurnService _aiTurnService;
    private readonly IActionQueryService _actionQueryService;

    public TurnExecutionService(
        IActionExecutionService actionExecutionService,
        ITurnOrchestrator turnOrchestrator,
        IAITurnService aiTurnService,
        IActionQueryService actionQueryService)
    {
        _actionExecutionService = actionExecutionService ?? throw new ArgumentNullException(nameof(actionExecutionService));
        _turnOrchestrator = turnOrchestrator ?? throw new ArgumentNullException(nameof(turnOrchestrator));
        _aiTurnService = aiTurnService ?? throw new ArgumentNullException(nameof(aiTurnService));
        _actionQueryService = actionQueryService ?? throw new ArgumentNullException(nameof(actionQueryService));
    }

    public async Task<bool> ExecuteActionAsync(GameSession gameSession, ExecuteActionDto dto, CancellationToken cancellationToken = default)
    {
        return await _actionExecutionService.ExecuteActionAsync(gameSession, dto, cancellationToken);
    }

    public async Task EndTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        
        try
        {
            await _turnOrchestrator.EndTurnAsync(gameSession, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
        
        Console.WriteLine($"[TurnExecutionService] EndTurnAsync completed for session {gameSession.Id}");
    }

    public async Task<bool> MakeAiTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        return await _aiTurnService.MakeAiTurnAsync(gameSession, cancellationToken);
    }

    public async Task<List<PositionDto>> GetAvailableActionsAsync(GameSession gameSession, string pieceId, string actionType, CancellationToken cancellationToken = default)
    {
        return await _actionQueryService.GetAvailableActionsAsync(gameSession, pieceId, actionType, cancellationToken);
    }

    public async Task<bool> ExecuteMoveAsync(GameSession gameSession, int pieceId, Position targetPosition, CancellationToken cancellationToken = default)
    {
        var turn = gameSession.GetCurrentTurn();
        var piece = gameSession.GetAllPieces().FirstOrDefault(p => p.Id == pieceId);
        
        if (piece == null)
            return false;
            
        if (piece.Owner?.Id != turn.ActiveParticipant.Id)
            return false;
            
        turn.SelectPiece(piece);
        return await _actionExecutionService.ExecuteMoveAsync(gameSession, turn, piece, targetPosition, cancellationToken);
    }

}
