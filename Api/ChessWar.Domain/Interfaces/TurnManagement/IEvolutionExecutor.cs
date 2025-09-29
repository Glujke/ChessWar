using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Интерфейс для выполнения эволюции фигур
/// </summary>
public interface IEvolutionExecutor
{
    /// <summary>
    /// Выполняет эволюцию фигуры
    /// </summary>
    bool ExecuteEvolution(GameSession session, Turn turn, Piece piece);

    /// <summary>
    /// Проверяет возможность эволюции фигуры
    /// </summary>
    bool CanEvolve(GameSession session, Turn turn, Piece piece);
}
