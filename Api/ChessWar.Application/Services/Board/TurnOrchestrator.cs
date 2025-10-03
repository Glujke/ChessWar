using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.TurnManagement;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Координатор ходов - управляет завершением ходов и связанными операциями
/// </summary>
public class TurnOrchestrator : ITurnOrchestrator
{
    private readonly ITurnProcessor _turnProcessor;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IGameStateManager _gameStateManager;
    private readonly IAITurnScheduler _aiScheduler;
    private readonly IOptimisticLockingService _lockingService;
    private readonly ITurnProcessingQueue _turnQueue;
    private readonly ITurnService _turnService;
    private readonly ILogger<TurnOrchestrator> _logger;

    public TurnOrchestrator(
        ITurnProcessor turnProcessor,
        INotificationDispatcher notificationDispatcher,
        IGameStateManager gameStateManager,
        IAITurnScheduler aiScheduler,
        IOptimisticLockingService lockingService,
        ITurnProcessingQueue turnQueue,
        ITurnService turnService,
        ILogger<TurnOrchestrator> logger)
    {
        _turnProcessor = turnProcessor ?? throw new ArgumentNullException(nameof(turnProcessor));
        _notificationDispatcher = notificationDispatcher ?? throw new ArgumentNullException(nameof(notificationDispatcher));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _aiScheduler = aiScheduler ?? throw new ArgumentNullException(nameof(aiScheduler));
        _lockingService = lockingService ?? throw new ArgumentNullException(nameof(lockingService));
        _turnQueue = turnQueue ?? throw new ArgumentNullException(nameof(turnQueue));
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Завершает текущий ход: выполняет проверки, ставит задачу обработки в очередь и отправляет уведомления.
    /// </summary>
    public async Task EndTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        try
        {
            _logger.LogInformation("Starting turn end process for session {SessionId}", gameSession.Id);

            var success = await _lockingService.ExecuteWithLockAsync(gameSession.Id, async () =>
            {
                var isGameCompleted = await _gameStateManager.IsGameCompletedAsync(gameSession, cancellationToken);
                if (isGameCompleted)
                {
                    _logger.LogWarning("Game already completed for session {SessionId}", gameSession.Id);
                    return false;
                }

                var currentTurn = gameSession.GetCurrentTurn();
                var activePlayer = currentTurn.ActiveParticipant;

                if (!(activePlayer is ChessWar.Domain.Entities.AI))
                {
                    if (currentTurn.Actions == null || currentTurn.Actions.Count == 0)
                    {
                        var activePlayerPieces = activePlayer.Pieces ?? new List<ChessWar.Domain.Entities.Piece>();
                        var hasAnyAction = activePlayerPieces.Any(p => p.IsAlive &&
                            (_turnService.GetAvailableMoves(gameSession, currentTurn, p).Any() || _turnService.GetAvailableAttacks(currentTurn, p).Any()));

                        var noMana = activePlayer.MP <= 0 || currentTurn.RemainingMP <= 0;

                        if (hasAnyAction && !noMana)
                        {
                            throw new InvalidOperationException("Для завершения хода требуется выполнение хотя бы одного действия.");
                        }
                    }
                }

                var turnRequest = new TurnRequest(
                    gameSession.Id,
                    gameSession,
                    TurnRequestType.PlayerTurn,
                    priority: 1);

                var enqueued = await _turnQueue.EnqueueTurnAsync(turnRequest);
                if (!enqueued)
                {
                    _logger.LogError("Failed to enqueue turn request for session {SessionId}", gameSession.Id);
                    return false;
                }

                return true;
            }, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Turn end process failed for session {SessionId}", gameSession.Id);
                _notificationDispatcher.DispatchError(gameSession.Id, "Не удалось завершить ход");
            }
            else
            {
                _logger.LogInformation("Turn end process completed for session {SessionId}", gameSession.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EndTurnAsync for session {SessionId}: {Message}", gameSession.Id, ex.Message);
            _notificationDispatcher.DispatchError(gameSession.Id, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Планирует выполнение хода ИИ для указанной сессии и ставит задачу в очередь.
    /// Возвращает true при успешной постановке.
    /// </summary>
    public async Task<bool> ExecuteAITurnAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        try
        {
            _logger.LogInformation("Starting AI turn execution for session {SessionId}", gameSession.Id);

            var success = await _lockingService.ExecuteWithLockAsync(gameSession.Id, async () =>
            {
                var isGameCompleted = await _gameStateManager.IsGameCompletedAsync(gameSession, cancellationToken);
                if (isGameCompleted)
                {
                    _logger.LogWarning("Game already completed for session {SessionId}", gameSession.Id);
                    return false;
                }

                if (!_aiScheduler.ShouldScheduleAI(gameSession))
                {
                    return true;
                }

                var turnRequest = new TurnRequest(
                    gameSession.Id,
                    gameSession,
                    TurnRequestType.AITurn,
                    priority: 2);

                var enqueued = await _turnQueue.EnqueueTurnAsync(turnRequest);
                if (!enqueued)
                {
                    _logger.LogError("Failed to enqueue AI turn request for session {SessionId}", gameSession.Id);
                    return false;
                }

                return true;
            }, cancellationToken);

            if (success)
            {
                _logger.LogInformation("AI turn execution completed for session {SessionId}", gameSession.Id);
            }
            else
            {
                _logger.LogWarning("AI turn execution failed for session {SessionId}", gameSession.Id);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExecuteAITurnAsync for session {SessionId}: {Message}", gameSession.Id, ex.Message);
            _notificationDispatcher.DispatchError(gameSession.Id, ex.Message);
            return false;
        }
    }
}