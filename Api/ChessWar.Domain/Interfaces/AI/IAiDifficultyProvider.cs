using ChessWar.Domain.Entities;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Интерфейс для получения уровня сложности ИИ
/// </summary>
public interface IAiDifficultyProvider
{
    /// <summary>
    /// Получает уровень сложности для игрока
    /// </summary>
    /// <param name="player">Игрок</param>
    /// <returns>Уровень сложности</returns>
    string GetDifficultyLevel(Player player);
    
    /// <summary>
    /// Получает лимит маны для уровня сложности
    /// </summary>
    /// <param name="difficulty">Уровень сложности</param>
    /// <returns>Лимит маны за ход</returns>
    int GetManaLimit(string difficulty);
}
