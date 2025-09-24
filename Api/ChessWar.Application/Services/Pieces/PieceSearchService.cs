using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Domain.Entities;

namespace ChessWar.Application.Services.Pieces;

/// <summary>
/// Сервис поиска фигур
/// </summary>
public class PieceSearchService : IPieceSearchService
{
    public Piece? FindPieceById(GameSession gameSession, string pieceId)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (string.IsNullOrWhiteSpace(pieceId))
            throw new ArgumentException("Piece ID cannot be empty", nameof(pieceId));

        var allPieces = gameSession.GetAllPieces();
        return allPieces.FirstOrDefault(p => p.Id.ToString() == pieceId);
    }
}
