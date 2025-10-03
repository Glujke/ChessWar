using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Сервис выполнения действий в ходе
/// </summary>
public interface IActionExecutionService
{
    /// <summary>
    /// Выполняет действие в ходе
    /// </summary>
    Task<bool> ExecuteActionAsync(GameSession gameSession, ExecuteActionDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет движение фигуры
    /// </summary>
    Task<bool> ExecuteMoveAsync(GameSession gameSession, Turn turn, Piece piece, Position targetPosition, CancellationToken cancellationToken = default);
}
