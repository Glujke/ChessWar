using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Интерфейс для работы с матрицами вероятностей в марковских цепях
/// </summary>
public interface IProbabilityMatrix
{
    /// <summary>
    /// Получить вероятность перехода из состояния from в to при действии action
    /// </summary>
    double GetTransitionProbability(GameSession from, GameAction action, GameSession to);
    
    /// <summary>
    /// Получить ожидаемую награду за действие action в состоянии session
    /// </summary>
    double GetReward(GameSession session, GameAction action);
    
    /// <summary>
    /// Обновить политику - вероятность выбора действия action в состоянии session
    /// </summary>
    void UpdatePolicy(GameSession session, GameAction action, double probability);
    
    /// <summary>
    /// Получить вероятность выбора действия action в состоянии session
    /// </summary>
    double GetActionProbability(GameSession session, GameAction action);
}
