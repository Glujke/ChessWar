using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Интерфейс для fallback стратегий ИИ
/// </summary>
public interface IAiFallbackStrategy
{
    /// <summary>
    /// Пытается выполнить fallback действие
    /// </summary>
    /// <param name="session">Игровая сессия</param>
    /// <param name="turn">Текущий ход</param>
    /// <param name="active">Активный игрок</param>
    /// <param name="config">Конфигурация баланса</param>
    /// <returns>True если действие выполнено успешно</returns>
    bool TryExecute(GameSession session, Turn turn, Player active, BalanceConfig config);

    /// <summary>
    /// Приоритет стратегии (чем меньше, тем выше приоритет)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Название стратегии для логирования
    /// </summary>
    string Name { get; }
}
