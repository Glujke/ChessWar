namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Интерфейс для сервиса кэширования
/// </summary>
public interface ICachingService
{
    /// <summary>
    /// Получает значение из кэша
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Устанавливает значение в кэш
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает значение из кэша или устанавливает его через фабрику
    /// </summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет значение из кэша
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет значения из кэша по паттерну
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Очищает весь кэш
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает размер кэша
    /// </summary>
    Task<long> GetCacheSizeAsync(CancellationToken cancellationToken = default);
}

