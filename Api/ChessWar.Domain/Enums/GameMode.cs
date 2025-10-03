namespace ChessWar.Domain.Enums;

/// <summary>
/// Режимы игры
/// </summary>
public enum GameMode
{
    /// <summary>
    /// Обучение (демо)
    /// </summary>
    Tutorial,

    /// <summary>
    /// Сетевая игра
    /// </summary>
    Online,

    /// <summary>
    /// Локальная игра
    /// </summary>
    Local,

    /// <summary>
    /// Игра с ИИ
    /// </summary>
    Ai
}
