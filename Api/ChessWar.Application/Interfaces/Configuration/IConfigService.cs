using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.Configuration;

/// <summary>
/// Сервис для управления конфигурацией игры
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Получает активную конфигурацию
    /// </summary>
    Task<BalanceVersion?> GetActiveConfigAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает версии конфигурации с пагинацией
    /// </summary>
    Task<(IReadOnlyList<BalanceVersion> Items, int TotalCount)> GetConfigVersionsAsync(
        int page,
        int pageSize,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создает новую версию конфигурации
    /// </summary>
    Task<BalanceVersion> CreateConfigVersionAsync(string version, string comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет версию конфигурации
    /// </summary>
    Task<BalanceVersion> UpdateConfigVersionAsync(Guid id, string version, string comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Публикует версию конфигурации (делает её активной)
    /// </summary>
    Task<BalanceVersion> PublishConfigVersionAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет JSON-пейлоад для версии (только Draft)
    /// </summary>
    Task SavePayloadAsync(Guid versionId, string json, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает JSON-пейлоад для версии
    /// </summary>
    Task<string?> GetPayloadAsync(Guid versionId, CancellationToken cancellationToken = default);
}
