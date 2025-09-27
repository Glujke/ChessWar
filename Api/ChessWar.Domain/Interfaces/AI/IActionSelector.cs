using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Селектор действий для ИИ
/// </summary>
public interface IActionSelector
{
    /// <summary>
    /// Выбирает лучшие действия из доступных
    /// </summary>
    List<GameAction> SelectActions(GameSession session, Turn turn, Participant active, List<GameAction> availableActions);
}
