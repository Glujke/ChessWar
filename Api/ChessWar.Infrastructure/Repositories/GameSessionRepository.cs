using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.DataAccess;
using Microsoft.Extensions.Caching.Memory;

namespace ChessWar.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для управления игровыми сессиями с кэшированием
/// </summary>
public class GameSessionRepository : IGameSessionRepository
{
    private readonly IMemoryCache _cache;
    private readonly string _cacheKeyPrefix = "gamesession_";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(2); 

    public GameSessionRepository(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public Task<GameSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(sessionId);
        
        if (_cache.TryGetValue(cacheKey, out GameSession? session))
        {
            return Task.FromResult(session);
        }

        return Task.FromResult<GameSession?>(null);
    }

    public Task SaveAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        var cacheKey = GetCacheKey(session.Id);
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(30), 
            Priority = CacheItemPriority.High
        };

        _cache.Set(cacheKey, session, cacheOptions);
        
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(sessionId);
        _cache.Remove(cacheKey);
        
        return Task.CompletedTask;
    }

    public Task<IEnumerable<GameSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Enumerable.Empty<GameSession>());
    }

    public Task<bool> ExistsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(sessionId);
        return Task.FromResult(_cache.TryGetValue(cacheKey, out _));
    }


    private string GetCacheKey(Guid sessionId) => $"{_cacheKeyPrefix}{sessionId}";
}
