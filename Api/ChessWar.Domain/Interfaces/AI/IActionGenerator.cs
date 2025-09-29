using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Генератор действий для ИИ
/// </summary>
public interface IActionGenerator
{
    /// <summary>
    /// Генерирует доступные действия для ИИ
    /// </summary>
    List<GameAction> GenerateActions(GameSession session, Turn turn, Participant active);
}
