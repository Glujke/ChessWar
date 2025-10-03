using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.GameLogic;
using Microsoft.Extensions.Caching.Memory;

namespace ChessWar.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для управления режимами игры
/// </summary>
public class GameModeRepository : IGameModeRepository
{
    private readonly IMemoryCache _cache;
    private readonly string _cacheKeyPrefix = "gamemode_";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

    public GameModeRepository(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetModeByIdAsync<T>(Guid modeId, CancellationToken cancellationToken = default) where T : IGameModeBase
    {
        var cacheKey = GetCacheKey(modeId);

        if (_cache.TryGetValue(cacheKey, out IGameModeBase? mode) && mode is T typedMode)
        {
            return Task.FromResult<T?>(typedMode);
        }

        return Task.FromResult<T?>(default);
    }

    public Task SaveModeAsync<T>(T mode, CancellationToken cancellationToken = default) where T : IGameModeBase
    {
        if (mode == null)
            throw new ArgumentNullException(nameof(mode));

        var cacheKey = GetCacheKey(mode.Id);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(30),
            Priority = CacheItemPriority.High
        };

        _cache.Set(cacheKey, mode, cacheOptions);

        return Task.CompletedTask;
    }

    public Task DeleteModeAsync(Guid modeId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(modeId);
        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<T>> GetActiveModesAsync<T>(CancellationToken cancellationToken = default) where T : IGameModeBase
    {
        return Task.FromResult<IEnumerable<T>>(new List<T>());
    }

    public Task<bool> ModeExistsAsync(Guid modeId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(modeId);
        return Task.FromResult(_cache.TryGetValue(cacheKey, out _));
    }

    private string GetCacheKey(Guid modeId) => $"{_cacheKeyPrefix}{modeId}";
}
