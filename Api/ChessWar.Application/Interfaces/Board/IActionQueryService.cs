using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Сервис получения доступных действий
/// </summary>
public interface IActionQueryService
{
    /// <summary>
    /// Получает доступные действия для фигуры
    /// </summary>
    Task<List<PositionDto>> GetAvailableActionsAsync(GameSession gameSession, string pieceId, string actionType, CancellationToken cancellationToken = default);
}
