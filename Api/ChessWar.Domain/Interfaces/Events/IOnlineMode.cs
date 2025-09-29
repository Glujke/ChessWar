using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Interfaces.Events;

/// <summary>
/// Интерфейс для сетевой сессии
/// </summary>
public interface IOnlineMode : IGameModeBase
{
    /// <summary>
    /// ID игрока-создателя
    /// </summary>
    string HostPlayerId { get; }
    
    /// <summary>
    /// ID подключенного игрока
    /// </summary>
    string? ConnectedPlayerId { get; }
    
    /// <summary>
    /// Максимальное количество игроков
    /// </summary>
    int MaxPlayers { get; }
    
    /// <summary>
    /// Готовы ли все игроки к началу
    /// </summary>
    bool IsReadyToStart { get; }
}

