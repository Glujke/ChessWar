using ChessWar.Domain.Entities;

namespace ChessWar.Domain.Interfaces.DataAccess;

/// <summary>
/// Репозиторий для работы с версиями баланса
/// </summary>
public interface IBalanceVersionRepository
{
    /// <summary>
    /// Получает активную версию баланса
    /// </summary>
    Task<BalanceVersion?> GetActiveAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает версию по ID
    /// </summary>
    Task<BalanceVersion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает все версии
    /// </summary>
    Task<IReadOnlyList<BalanceVersion>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Добавляет версию
    /// </summary>
    Task AddAsync(BalanceVersion version, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновляет версию
    /// </summary>
    Task UpdateAsync(BalanceVersion version, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает версии с пагинацией и фильтрацией
    /// </summary>
    Task<(IReadOnlyList<BalanceVersion> Items, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? status = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверяет существование версии по номеру
    /// </summary>
    Task<bool> ExistsByVersionAsync(string version, CancellationToken cancellationToken = default);
}
