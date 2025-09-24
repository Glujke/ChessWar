using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Базовый интерфейс для AI стратегий
/// </summary>
public interface IAIStrategy
{
    /// <summary>
    /// Приоритет стратегии (меньше = выше приоритет)
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Название стратегии
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Может ли стратегия выполнить действие в текущем состоянии
    /// </summary>
    bool CanExecute(GameSession session, Turn turn, Player active);
    
    /// <summary>
    /// Выполнить действие согласно стратегии
    /// </summary>
    bool Execute(GameSession session, Turn turn, Player active);
    
    /// <summary>
    /// Рассчитать вероятность успеха действия
    /// </summary>
    double CalculateActionProbability(GameSession session, Turn turn, Player active, GameAction action);
}
