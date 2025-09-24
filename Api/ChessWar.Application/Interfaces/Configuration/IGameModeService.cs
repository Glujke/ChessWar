using ChessWar.Application.DTOs;

namespace ChessWar.Application.Interfaces.Configuration;

/// <summary>
/// Сервис для управления режимами игры
/// </summary>
public interface IGameModeService
{
    /// <summary>
    /// Запускает обучение
    /// </summary>
    Task<TutorialSessionDto> StartTutorialAsync(CreateTutorialSessionDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Запускает игру с ИИ
    /// </summary>
    Task<AiSessionDto> StartAiGameAsync(CreateAiSessionDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Запускает локальную игру
    /// </summary>
    Task<GameSessionDto> StartLocalGameAsync(CreateLocalSessionDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Запускает онлайн игру
    /// </summary>
    Task<OnlineSessionDto> StartOnlineGameAsync(CreateOnlineSessionDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает доступные режимы игры
    /// </summary>
    Task<object> GetAvailableModesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает статистику игрока
    /// </summary>
    Task<PlayerStatsDto> GetPlayerStatsAsync(string playerId, CancellationToken cancellationToken = default);
}
