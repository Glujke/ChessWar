using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Исполнитель действий для ИИ
/// </summary>
public interface IActionExecutor
{
    /// <summary>
    /// Выполняет выбранные действия
    /// </summary>
    bool ExecuteActions(GameSession session, Turn turn, List<GameAction> actions);
}
