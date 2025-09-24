using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Интерфейс для управления ходами
/// </summary>
public interface ITurnManager
{
    /// <summary>
    /// Начинает новый ход
    /// </summary>
    Turn StartTurn(GameSession gameSession, Player activeParticipant);

    /// <summary>
    /// Завершает ход
    /// </summary>
    void EndTurn(Turn turn);
}
