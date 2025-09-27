using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Интерфейс для выполнения атак фигур
/// </summary>
public interface IAttackExecutor
{
    /// <summary>
    /// Выполняет атаку фигуры на указанную позицию
    /// </summary>
    bool ExecuteAttack(GameSession session, Turn turn, Piece attacker, Position targetPosition);

    /// <summary>
    /// Получает список доступных атак для фигуры
    /// </summary>
    List<Position> GetAvailableAttacks(Turn turn, Piece attacker);
}
