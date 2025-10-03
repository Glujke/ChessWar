using ChessWar.Application.Interfaces.Board;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Сервис оптимистичной блокировки для предотвращения race conditions
/// </summary>
public class OptimisticLockingService : IOptimisticLockingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<OptimisticLockingService> _logger;
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _sessionLocks;
    private readonly TimeSpan _lockTimeout;

    public OptimisticLockingService(
        IMemoryCache cache,
        ILogger<OptimisticLockingService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sessionLocks = new ConcurrentDictionary<Guid, SemaphoreSlim>();
        _lockTimeout = TimeSpan.FromSeconds(30);
    }

    public async Task<bool> TryAcquireLockAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var semaphore = _sessionLocks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));

            var acquired = await semaphore.WaitAsync(_lockTimeout, cancellationToken);

            if (acquired)
            {
                _logger.LogDebug("Lock acquired for session {SessionId}", sessionId);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to acquire lock for session {SessionId} within timeout", sessionId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock for session {SessionId}: {Message}", sessionId, ex.Message);
            return false;
        }
    }

    public void ReleaseLock(Guid sessionId)
    {
        try
        {
            if (_sessionLocks.TryGetValue(sessionId, out var semaphore))
            {
                semaphore.Release();
                _logger.LogDebug("Lock released for session {SessionId}", sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock for session {SessionId}: {Message}", sessionId, ex.Message);
        }
    }

    public async Task<T?> ExecuteWithLockAsync<T>(Guid sessionId, Func<Task<T?>> operation, CancellationToken cancellationToken = default)
    {
        if (!await TryAcquireLockAsync(sessionId, cancellationToken))
        {
            _logger.LogWarning("Could not acquire lock for session {SessionId}, operation skipped", sessionId);
            return default(T);
        }

        try
        {
            return await operation();
        }
        finally
        {
            ReleaseLock(sessionId);
        }
    }

    public async Task<bool> ExecuteWithLockAsync(Guid sessionId, Func<Task<bool>> operation, CancellationToken cancellationToken = default)
    {
        if (!await TryAcquireLockAsync(sessionId, cancellationToken))
        {
            _logger.LogWarning("Could not acquire lock for session {SessionId}, operation skipped", sessionId);
            return false;
        }

        try
        {
            return await operation();
        }
        finally
        {
            ReleaseLock(sessionId);
        }
    }

    public void CleanupExpiredLocks()
    {
        var expiredSessions = new List<Guid>();

        foreach (var kvp in _sessionLocks)
        {
            if (kvp.Value.CurrentCount == 1)
            {
                expiredSessions.Add(kvp.Key);
            }
        }

        foreach (var sessionId in expiredSessions)
        {
            if (_sessionLocks.TryRemove(sessionId, out var semaphore))
            {
                semaphore.Dispose();
                _logger.LogDebug("Cleaned up expired lock for session {SessionId}", sessionId);
            }
        }
    }

    public void Dispose()
    {
        foreach (var semaphore in _sessionLocks.Values)
        {
            semaphore?.Dispose();
        }
        _sessionLocks.Clear();
    }
}

