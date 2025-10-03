using ChessWar.Application.Interfaces.Board;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Фоновый сервис для обработки ходов с очередями
/// </summary>
public class TurnProcessingBackgroundService : BackgroundService, ITurnProcessingQueue
{
    private readonly Channel<TurnRequest> _turnChannel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TurnProcessingBackgroundService> _logger;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentDictionary<Guid, bool> _processingSessions;

    public TurnProcessingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<TurnProcessingBackgroundService> logger,
        TurnQueueHub hub)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _turnChannel = hub?.TurnChannel ?? throw new ArgumentNullException(nameof(hub));
        _semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);
        _processingSessions = new ConcurrentDictionary<Guid, bool>();
    }

    public async Task<bool> EnqueueTurnAsync(TurnRequest request)
    {
        if (request == null)
            return false;

        try
        {
            await _turnChannel.Writer.WriteAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue turn request for session {SessionId}", request.SessionId);
            return false;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Turn processing background service started");

        await foreach (var request in _turnChannel.Reader.ReadAllAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            _ = Task.Run(async () => await ProcessTurnRequestAsync(request, stoppingToken), stoppingToken);
        }

        _logger.LogInformation("Turn processing background service stopped");
    }

    private async Task ProcessTurnRequestAsync(TurnRequest request, CancellationToken cancellationToken)
    {
        if (_processingSessions.TryGetValue(request.SessionId, out var isProcessing) && isProcessing)
        {
            _logger.LogWarning("Session {SessionId} is already being processed, skipping", request.SessionId);
            return;
        }

        _processingSessions.TryAdd(request.SessionId, true);

        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            using var scope = _serviceProvider.CreateScope();
            var turnProcessor = scope.ServiceProvider.GetRequiredService<ITurnProcessor>();
            var notificationDispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
            var gameStateManager = scope.ServiceProvider.GetRequiredService<IGameStateManager>();

            _logger.LogInformation("Processing turn request for session {SessionId}, type: {Type}",
                request.SessionId, request.Type);

            var activePlayerBefore = request.GameSession.GetCurrentTurn().ActiveParticipant;

            notificationDispatcher.DispatchTurnEnded(request.SessionId,
                activePlayerBefore.IsAI ? "AI" : "Player",
                request.GameSession.GetCurrentTurn().Number);

            await turnProcessor.ProcessTurnPhaseAsync(request.GameSession, cancellationToken);

            var activePlayerAfter = request.GameSession.GetCurrentTurn().ActiveParticipant;

            notificationDispatcher.DispatchTurnStarted(request.SessionId,
                activePlayerAfter.IsAI ? "AI" : "Player",
                request.GameSession.GetCurrentTurn().Number);

            var gameResult = await gameStateManager.CheckAndHandleGameCompletionAsync(request.GameSession, cancellationToken);

            if (gameResult.HasValue)
            {
                notificationDispatcher.DispatchGameEnded(request.SessionId,
                    gameResult.Value.ToString(),
                    $"Игра завершена: {gameResult.Value}");
            }

            _logger.LogInformation("Turn request processed successfully for session {SessionId}", request.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing turn request for session {SessionId}: {Message}",
                request.SessionId, ex.Message);

            using var scope = _serviceProvider.CreateScope();
            var notificationDispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
            notificationDispatcher.DispatchError(request.SessionId, ex.Message);
        }
        finally
        {
            _semaphore.Release();
            _processingSessions.TryRemove(request.SessionId, out _);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping turn processing background service");

        _turnChannel.Writer.TryComplete();
        try
        {
            await _turnChannel.Reader.Completion.ConfigureAwait(false);
        }
        catch { }

        await base.StopAsync(cancellationToken);

        _semaphore.Dispose();
        _logger.LogInformation("Turn processing background service stopped");
    }
}
