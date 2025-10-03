using ChessWar.Application.Services.Board;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Интерфейс для батчера уведомлений
/// </summary>
public interface INotificationBatcher
{
    /// <summary>
    /// Добавляет уведомление в батч
    /// </summary>
    void AddNotification(NotificationBatchItem item);

    /// <summary>
    /// Добавляет уведомление о завершении хода
    /// </summary>
    void AddTurnEndedNotification(Guid sessionId, string participantType, int turnNumber);

    /// <summary>
    /// Добавляет уведомление о начале хода
    /// </summary>
    void AddTurnStartedNotification(Guid sessionId, string participantType, int turnNumber);

    /// <summary>
    /// Добавляет уведомление о завершении игры
    /// </summary>
    void AddGameEndedNotification(Guid sessionId, string result, string message);
}
