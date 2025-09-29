using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Координатор ходов - объединяет все аспекты управления ходами
/// </summary>
public interface ITurnService
{
    /// <summary>
    /// Начинает новый ход
    /// </summary>
    Turn StartTurn(GameSession gameSession, Participant activeParticipant);

    /// <summary>
    /// Завершает ход
    /// </summary>
    void EndTurn(Turn turn);

    /// <summary>
    /// Выполняет движение фигуры
    /// </summary>
    bool ExecuteMove(GameSession gameSession, Turn turn, Piece piece, Position targetPosition);

    /// <summary>
    /// Выполняет атаку
    /// </summary>
    bool ExecuteAttack(GameSession gameSession, Turn turn, Piece attacker, Position targetPosition);

    /// <summary>
    /// Получает доступные ходы для фигуры
    /// </summary>
    List<Position> GetAvailableMoves(GameSession gameSession, Turn turn, Piece piece);

    /// <summary>
    /// Получает доступные атаки для фигуры
    /// </summary>
    List<Position> GetAvailableAttacks(Turn turn, Piece piece);
}
