namespace ChessWar.Domain.Enums;

/// <summary>
/// Статус игровой сессии
/// </summary>
public enum GameStatus
{
    /// <summary>
    /// Ожидание начала игры
    /// </summary>
    Waiting,
    
    /// <summary>
    /// Игра активна
    /// </summary>
    Active,
    
    /// <summary>
    /// Игра завершена
    /// </summary>
    Finished
}
