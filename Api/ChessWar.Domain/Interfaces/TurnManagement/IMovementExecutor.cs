using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Интерфейс для выполнения движений фигур
/// </summary>
public interface IMovementExecutor
{
    /// <summary>
    /// Выполняет движение фигуры на указанную позицию
    /// </summary>
    bool ExecuteMove(GameSession session, Turn turn, Piece piece, Position targetPosition);

    /// <summary>
    /// Получает список доступных ходов для фигуры
    /// </summary>
    List<Position> GetAvailableMoves(GameSession session, Turn turn, Piece piece);
}
