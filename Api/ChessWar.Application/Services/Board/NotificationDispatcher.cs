using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.GameManagement;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Диспетчер уведомлений - fire-and-forget уведомления
/// </summary>
public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IGameNotificationService _notificationService;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        IGameNotificationService notificationService,
        ILogger<NotificationDispatcher> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void DispatchTurnEnded(Guid sessionId, string participantType, int turnNumber)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifyTurnEndedAsync(sessionId, participantType, turnNumber)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch turn ended notification for session {SessionId}", sessionId);
            }
        });
    }

    public void DispatchTurnStarted(Guid sessionId, string participantType, int turnNumber)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifyTurnStartedAsync(sessionId, participantType, turnNumber)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch turn started notification for session {SessionId}", sessionId);
            }
        });
    }

    public void DispatchAITurnInProgress(Guid sessionId)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifyAITurnInProgressAsync(sessionId)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch AI turn in progress notification for session {SessionId}", sessionId);
            }
        });
    }

    public void DispatchAITurnCompleted(Guid sessionId)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifyAITurnCompletedAsync(sessionId)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch AI turn completed notification for session {SessionId}", sessionId);
            }
        });
    }

    public void DispatchAiMove(Guid sessionId, object moveData)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifyAiMoveAsync(sessionId, moveData)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch AI move notification for session {SessionId}", sessionId);
            }
        });
    }

    public void DispatchGameEnded(Guid sessionId, string result, string message)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifyGameEndedAsync(sessionId, result, message)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch game ended notification for session {SessionId}", sessionId);
            }
        });
    }

    public void DispatchPieceEvolved(Guid sessionId, string pieceId, string newType, int x, int y)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifyPieceEvolvedAsync(sessionId, pieceId, newType, x, y)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch piece evolved notification for session {SessionId}", sessionId);
            }
        });
    }

    public void DispatchError(Guid sessionId, string error)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.NotifyErrorAsync(sessionId, error)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch error notification for session {SessionId}", sessionId);
            }
        });
    }
}

