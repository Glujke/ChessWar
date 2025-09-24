namespace ChessWar.Application.Interfaces.Configuration;

/// <summary>
/// Сервис кэширования
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Получает значение из кэша
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Сохраняет значение в кэш
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаляет значение из кэша
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверяет существование ключа в кэше
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
