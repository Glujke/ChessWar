using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Интерфейс для планировщика AI ходов
/// </summary>
public interface IAITurnScheduler
{
    /// <summary>
    /// Планирует AI ход
    /// </summary>
    Task<bool> ScheduleAITurnAsync(GameSession gameSession, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет, нужно ли планировать AI ход
    /// </summary>
    bool ShouldScheduleAI(GameSession gameSession);

    /// <summary>
    /// Выполняет AI ход
    /// </summary>
    Task<bool> ExecuteAITurnAsync(GameSession gameSession, CancellationToken cancellationToken = default);
}

