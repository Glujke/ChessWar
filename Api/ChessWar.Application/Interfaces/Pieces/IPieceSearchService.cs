using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.Pieces;

/// <summary>
/// Сервис поиска фигур
/// </summary>
public interface IPieceSearchService
{
    /// <summary>
    /// Находит фигуру по ID
    /// </summary>
    Piece? FindPieceById(GameSession gameSession, string pieceId);
}
