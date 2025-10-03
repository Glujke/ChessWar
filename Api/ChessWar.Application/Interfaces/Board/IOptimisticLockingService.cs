namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Интерфейс для сервиса оптимистичной блокировки
/// </summary>
public interface IOptimisticLockingService
{
    /// <summary>
    /// Пытается получить блокировку для сессии
    /// </summary>
    Task<bool> TryAcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Освобождает блокировку для сессии
    /// </summary>
    void ReleaseLock(Guid sessionId);

    /// <summary>
    /// Выполняет операцию с блокировкой
    /// </summary>
    Task<T?> ExecuteWithLockAsync<T>(Guid sessionId, Func<Task<T?>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет операцию с блокировкой
    /// </summary>
    Task<bool> ExecuteWithLockAsync(Guid sessionId, Func<Task<bool>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Очищает истекшие блокировки
    /// </summary>
    void CleanupExpiredLocks();
}

