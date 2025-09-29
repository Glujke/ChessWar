using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Интерфейс для получения доступных действий в ходе
/// </summary>
public interface ITurnActionQuery
{
    /// <summary>
    /// Получает доступные ходы для фигуры
    /// </summary>
    List<Position> GetAvailableMoves(Turn turn, Piece piece);

    /// <summary>
    /// Получает доступные ходы для фигуры с использованием GameSession
    /// </summary>
    List<Position> GetAvailableMoves(GameSession gameSession, Turn turn, Piece piece);

    /// <summary>
    /// Получает доступные атаки для фигуры
    /// </summary>
    List<Position> GetAvailableAttacks(Turn turn, Piece piece);
}
