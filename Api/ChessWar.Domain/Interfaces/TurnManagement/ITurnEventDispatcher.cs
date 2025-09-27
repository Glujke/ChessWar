using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Интерфейс для отправки событий ходов
/// </summary>
public interface ITurnEventDispatcher
{
    /// <summary>
    /// Отправляет событие начала хода
    /// </summary>
    Task DispatchTurnStartedEventAsync(GameSession session, Turn turn);

    /// <summary>
    /// Отправляет событие окончания хода
    /// </summary>
    Task DispatchTurnEndedEventAsync(GameSession session, Turn turn);

    /// <summary>
    /// Отправляет событие движения фигуры
    /// </summary>
    Task DispatchPieceMovedEventAsync(GameSession session, Piece piece, Position fromPosition, Position toPosition);

    /// <summary>
    /// Отправляет событие атаки фигуры
    /// </summary>
    Task DispatchPieceAttackedEventAsync(GameSession session, Piece attacker, Piece target, int damage);
}
