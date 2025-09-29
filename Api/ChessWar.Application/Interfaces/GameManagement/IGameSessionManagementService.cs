using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.GameManagement;

/// <summary>
/// Сервис управления игровыми сессиями
/// </summary>
public interface IGameSessionManagementService
{
    /// <summary>
    /// Создаёт новую игровую сессию
    /// </summary>
    Task<GameSession> CreateGameSessionAsync(CreateGameSessionDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Начинает игру
    /// </summary>
    Task StartGameAsync(GameSession gameSession, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Завершает игру
    /// </summary>
    Task CompleteGameAsync(GameSession gameSession, Domain.Enums.GameResult result, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает игровую сессию по ID
    /// </summary>
    Task<GameSession?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
