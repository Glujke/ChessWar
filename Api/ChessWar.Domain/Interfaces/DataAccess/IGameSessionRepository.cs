using ChessWar.Domain.Entities;

namespace ChessWar.Domain.Interfaces.DataAccess;

/// <summary>
/// Репозиторий для управления игровыми сессиями
/// </summary>
public interface IGameSessionRepository
{
    /// <summary>
    /// Получает игровую сессию по ID
    /// </summary>
    Task<GameSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Сохраняет игровую сессию
    /// </summary>
    Task SaveAsync(GameSession session, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаляет игровую сессию
    /// </summary>
    Task DeleteAsync(Guid sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает все активные сессии
    /// </summary>
    Task<IEnumerable<GameSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверяет существование сессии
    /// </summary>
    Task<bool> ExistsAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
