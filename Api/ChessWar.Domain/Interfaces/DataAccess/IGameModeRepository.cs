using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Interfaces.DataAccess;

/// <summary>
/// Репозиторий для управления режимами игры
/// </summary>
public interface IGameModeRepository
{
    /// <summary>
    /// Сохраняет режим игры
    /// </summary>
    Task SaveModeAsync<T>(T mode, CancellationToken cancellationToken = default) where T : IGameModeBase;
    
    /// <summary>
    /// Получает режим игры по ID
    /// </summary>
    Task<T?> GetModeByIdAsync<T>(Guid modeId, CancellationToken cancellationToken = default) where T : IGameModeBase;
    
    /// <summary>
    /// Удаляет режим игры
    /// </summary>
    Task DeleteModeAsync(Guid modeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает все активные режимы
    /// </summary>
    Task<IEnumerable<T>> GetActiveModesAsync<T>(CancellationToken cancellationToken = default) where T : IGameModeBase;
    
    /// <summary>
    /// Проверяет существование режима
    /// </summary>
    Task<bool> ModeExistsAsync(Guid modeId, CancellationToken cancellationToken = default);
}
