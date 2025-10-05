using ChessWar.Application.Interfaces.Board;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Батчер уведомлений - группирует уведомления для эффективной отправки
/// </summary>
public class NotificationBatcher : INotificationBatcher
{
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly ILogger<NotificationBatcher> _logger;
    private readonly ConcurrentQueue<NotificationBatchItem> _notificationQueue;
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer _batchTimer;
    private readonly int _batchSize;
    private readonly TimeSpan _batchInterval;

    public NotificationBatcher(
        INotificationDispatcher notificationDispatcher,
        ILogger<NotificationBatcher> logger)
    {
        _notificationDispatcher = notificationDispatcher ?? throw new ArgumentNullException(nameof(notificationDispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationQueue = new ConcurrentQueue<NotificationBatchItem>();
        _semaphore = new SemaphoreSlim(1, 1);
        _batchSize = 50;
        _batchInterval = TimeSpan.FromMilliseconds(100);
        _batchTimer = new Timer(ProcessBatch, null, _batchInterval, _batchInterval);
    }

    public void AddNotification(NotificationBatchItem item)
    {
        if (item == null)
            return;

        _notificationQueue.Enqueue(item);
    }

    public void AddTurnEndedNotification(Guid sessionId, string participantType, int turnNumber)
    {
        var item = new NotificationBatchItem
        {
            Type = NotificationType.TurnEnded,
            SessionId = sessionId,
            ParticipantType = participantType,
            TurnNumber = turnNumber,
            Timestamp = DateTime.UtcNow
        };
        AddNotification(item);
    }

    public void AddTurnStartedNotification(Guid sessionId, string participantType, int turnNumber)
    {
        var item = new NotificationBatchItem
        {
            Type = NotificationType.TurnStarted,
            SessionId = sessionId,
            ParticipantType = participantType,
            TurnNumber = turnNumber,
            Timestamp = DateTime.UtcNow
        };
        AddNotification(item);
    }

    public void AddGameEndedNotification(Guid sessionId, string result, string message)
    {
        var item = new NotificationBatchItem
        {
            Type = NotificationType.GameEnded,
            SessionId = sessionId,
            Result = result,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
        AddNotification(item);
    }

    private async void ProcessBatch(object? state)
    {
        if (_notificationQueue.IsEmpty)
            return;

        await _semaphore.WaitAsync();
        try
        {
            var batch = new List<NotificationBatchItem>();
            var count = 0;

            while (_notificationQueue.TryDequeue(out var item) && count < _batchSize)
            {
                batch.Add(item);
                count++;
            }

            if (batch.Any())
            {
                await ProcessNotificationBatch(batch);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification batch: {Message}", ex.Message);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ProcessNotificationBatch(List<NotificationBatchItem> batch)
    {
        try
        {
            var groupedNotifications = batch.GroupBy(n => n.Type);

            foreach (var group in groupedNotifications)
            {
                switch (group.Key)
                {
                    case NotificationType.TurnEnded:
                        await ProcessTurnEndedBatch(group.ToList());
                        break;
                    case NotificationType.TurnStarted:
                        await ProcessTurnStartedBatch(group.ToList());
                        break;
                    case NotificationType.GameEnded:
                        await ProcessGameEndedBatch(group.ToList());
                        break;
                }
            }

            _ = batch.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification batch: {Message}", ex.Message);
        }
    }

    private async Task ProcessTurnEndedBatch(List<NotificationBatchItem> notifications)
    {
        foreach (var notification in notifications)
        {
            _notificationDispatcher.DispatchTurnEnded(
                notification.SessionId,
                notification.ParticipantType ?? "Unknown",
                notification.TurnNumber ?? 0);
        }
    }

    private async Task ProcessTurnStartedBatch(List<NotificationBatchItem> notifications)
    {
        foreach (var notification in notifications)
        {
            _notificationDispatcher.DispatchTurnStarted(
                notification.SessionId,
                notification.ParticipantType ?? "Unknown",
                notification.TurnNumber ?? 0);
        }
    }

    private async Task ProcessGameEndedBatch(List<NotificationBatchItem> notifications)
    {
        foreach (var notification in notifications)
        {
            _notificationDispatcher.DispatchGameEnded(
                notification.SessionId,
                notification.Result ?? "Unknown",
                notification.Message ?? "Game ended");
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification batcher started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping notification batcher");

        _batchTimer?.Dispose();
        _semaphore?.Dispose();

        _logger.LogInformation("Notification batcher stopped");
    }
}

/// <summary>
/// Элемент батча уведомлений
/// </summary>
public class NotificationBatchItem
{
    public NotificationType Type { get; set; }
    public Guid SessionId { get; set; }
    public string? ParticipantType { get; set; }
    public int? TurnNumber { get; set; }
    public string? Result { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Тип уведомления
/// </summary>
public enum NotificationType
{
    TurnEnded,
    TurnStarted,
    GameEnded
}
