using ChessWar.Application.Interfaces.Board;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Сервис кэширования с TTL для оптимизации производительности
/// </summary>
public class CachingService : ICachingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingService> _logger;
    private readonly TimeSpan _defaultTtl;

    public CachingService(
        IMemoryCache cache,
        ILogger<CachingService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultTtl = TimeSpan.FromMinutes(5);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                return value;
            }

            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key {Key}: {Message}", key, ex.Message);
            return default(T);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl,
                SlidingExpiration = TimeSpan.FromMinutes(1),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(key, value, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key {Key}: {Message}", key, ex.Message);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await GetAsync<T>(key, cancellationToken);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, ttl, cancellationToken);
            }

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or setting value in cache for key {Key}: {Message}", key, ex.Message);
            return default(T);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key {Key}: {Message}", key, ex.Message);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache is MemoryCache memoryCache)
            {
                var field = typeof(MemoryCache).GetField("_coherentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field?.GetValue(memoryCache) is object coherentState)
                {
                    var entriesCollection = coherentState.GetType().GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (entriesCollection?.GetValue(coherentState) is IDictionary entries)
                    {
                        var keysToRemove = new List<object>();
                        foreach (DictionaryEntry entry in entries)
                        {
                            if (entry.Key.ToString()?.Contains(pattern) == true)
                            {
                                keysToRemove.Add(entry.Key);
                            }
                        }

                        foreach (var key in keysToRemove)
                        {
                            _cache.Remove(key);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern {Pattern}: {Message}", pattern, ex.Message);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Clear();
                _logger.LogInformation("Cache cleared");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache: {Message}", ex.Message);
        }
    }

    public async Task<long> GetCacheSizeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache is MemoryCache memoryCache)
            {
                var field = typeof(MemoryCache).GetField("_coherentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field?.GetValue(memoryCache) is object coherentState)
                {
                    var entriesCollection = coherentState.GetType().GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (entriesCollection?.GetValue(coherentState) is IDictionary entries)
                    {
                        return entries.Count;
                    }
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache size: {Message}", ex.Message);
            return 0;
        }
    }
}
