namespace ChessWar.Domain.Exceptions;

/// <summary>
/// Исключение при попытке переключить фигуры в ходе, когда это запрещено.
/// </summary>
public class PieceSwitchForbiddenException : GameException
{
    public Guid PieceId { get; }
    public string Reason { get; }

    public PieceSwitchForbiddenException(Guid pieceId, string reason)
        : base($"Cannot switch to piece {pieceId}. Reason: {reason}")
    {
        if (pieceId == Guid.Empty)
            throw new ArgumentException("Piece ID cannot be empty", nameof(pieceId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be null or empty", nameof(reason));

        PieceId = pieceId;
        Reason = reason;
    }
}
