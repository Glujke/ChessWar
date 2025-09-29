using ChessWar.Application.DTOs;

namespace ChessWar.Application.Interfaces.Configuration;

/// <summary>
/// Сервис для работы со статистикой
/// </summary>
public interface IStatsService
{
    /// <summary>
    /// Получает статистику игрока
    /// </summary>
    Task<PlayerStatsDto> GetPlayerStatsAsync(string playerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновляет статистику после завершения игры
    /// </summary>
    Task UpdatePlayerStatsAsync(string playerId, GameResultDto result, CancellationToken cancellationToken = default);
}

