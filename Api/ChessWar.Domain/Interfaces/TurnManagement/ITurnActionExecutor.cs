using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Интерфейс для выполнения действий в ходе
/// </summary>
public interface ITurnActionExecutor
{

    /// <summary>
    /// Выполняет движение фигуры с контекстом GameSession
    /// </summary>
    bool ExecuteMove(GameSession gameSession, Turn turn, Piece piece, Position targetPosition);

    /// <summary>
    /// Выполняет атаку с контекстом GameSession
    /// </summary>
    bool ExecuteAttack(GameSession gameSession, Turn turn, Piece attacker, Position targetPosition);

    /// <summary>
    /// Выполняет движение фигуры асинхронно
    /// </summary>
    Task<bool> ExecuteMoveAsync(GameSession gameSession, Turn turn, Piece piece, Position targetPosition, CancellationToken cancellationToken = default);
}
