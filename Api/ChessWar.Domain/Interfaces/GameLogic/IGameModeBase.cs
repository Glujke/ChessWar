using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Interfaces.GameLogic;

/// <summary>
/// Базовый интерфейс для всех типов игровых сессий
/// </summary>
public interface IGameModeBase
{
    /// <summary>
    /// Уникальный идентификатор сессии
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// Режим игры
    /// </summary>
    GameMode Mode { get; }
    
    /// <summary>
    /// Статус игры
    /// </summary>
    GameStatus Status { get; }
    
    /// <summary>
    /// Время создания сессии
    /// </summary>
    DateTime CreatedAt { get; }
    
    /// <summary>
    /// Время последнего обновления
    /// </summary>
    DateTime UpdatedAt { get; }
}

